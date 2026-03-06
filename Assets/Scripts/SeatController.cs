using NUnit.Framework;
using UnityEngine;

public class SeatController : MonoBehaviour
{
    [Header("Datos de Red")]
    private bool isLocal;
    private int seatIndex;
    private ulong clientId;
    private int team;
    private Unity.Collections.FixedString128Bytes playerId;

    [Header("Cartas")]
    [SerializeField] private Transform[] handHolderPoints;
    [SerializeField] private GameObject cardPrefab;
    private Card[] HandCards;
    private int[] cardsIndexes;
    [Header("Cámara y Visión")]
    [SerializeField] private Transform cameraMount; 
    [SerializeField] private GameObject myModelHead; // (Opcional) la cabeza para apagarlo si te tapa la visión
    
    public float mouseSensitivity = 250f;
    public float minPitch = -60f;
    public float maxPitch = 30f;
    public float maxYaw = 60f;
    private float xRotation = -20f; // Empieza mirando un poco hacia abajo
    private float yRotation = 0f;
    [Header("Sincronización de Red")]
    private Quaternion targetHeadRotation = Quaternion.identity;
    private float syncTimer = 0f;
    private float syncRate = 0.1f; // Manda datos 10 veces por segundo

    private void Awake()
    {
        HandCards = new Card[3];
        cardsIndexes = new int[3];
    }

    private void Update()
    {
        if (isLocal)
        {
            // 1. Lógica de Cartas (Clic Izquierdo)
            if (Input.GetMouseButtonDown(0)) 
            {
                HandleCardClick();
            }

            // 2. Lógica de Cámara (Mantener Clic Derecho)
            HandleCameraMovement();
        }
        else
        {
        if (myModelHead != null)
            {
                myModelHead.transform.localRotation = Quaternion.Lerp(
                    myModelHead.transform.localRotation, 
                    targetHeadRotation, 
                    Time.deltaTime * 15f
                );
            }
        }
    }

    public void SetPlayerData(int seatIndex, ulong clientId, int team, Unity.Collections.FixedString128Bytes playerId, bool isMine)
    {
        this.seatIndex = seatIndex;
        this.clientId = clientId;
        this.team = team;
        this.playerId = playerId;
        isLocal = isMine;

        if (isMine)
        {
            SetupLocalCamera();
        }
        else
        {
            // Apagamos colliders de las cartas enemigas
            foreach (var c in HandCards)
            {
                if (c == null) continue;
                if (c.TryGetComponent<Collider>(out var col)) col.enabled = false;
            }
        }
        CreateHandView();
    }

    private void SetupLocalCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && cameraMount != null)
        { 
            // Robamos la cámara y la pegamos en la cabeza
            mainCam.transform.SetParent(cameraMount);
            mainCam.transform.localPosition = Vector3.zero;
            // Aplicamos la rotación inicial para que mire a la mesa
            mainCam.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            // Ocultamos nuestra propia cabeza para no ver el interior del modelo
            if (myModelHead != null) myModelHead.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Falta asignar la MainCamera o el CameraMount en el SeatController.");
        }
    }

    private void HandleCameraMovement()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, -maxYaw, maxYaw);

            // Calculamos la rotación una sola vez
            Quaternion lookRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            // Se la aplicamos al pecho (Cámara)
            if (cameraMount != null) cameraMount.localRotation = lookRotation;
            
            // Se la aplicamos a TU cabeza localmente
            if (myModelHead != null) myModelHead.transform.localRotation = lookRotation;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // --- Enviar la rotación por la red sin saturar ---
        syncTimer += Time.deltaTime;
        if (syncTimer >= syncRate && cameraMount != null)
        {
            syncTimer = 0f;
            GameManager.Instance.SyncHeadRotationServerRpc(seatIndex, cameraMount.localRotation);
        }
    }
    public void SetCardsIds(int[] cards)
    {
        cardsIndexes = cards;
        UpdateHandView();
    }

    public void CreateHandView()
    {
        for (int i = 0; i < handHolderPoints.Length; i++)
        {
            if (handHolderPoints[i] != null)
            {
                GameObject cardObject = Instantiate(cardPrefab, handHolderPoints[i]);
                Transform cardTransform = cardObject.transform;
                cardTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                Card newCard = cardObject.GetComponent<Card>();
                newCard.SetCardParentIndex(i);
                HandCards[i] = newCard;
                newCard.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateHandView()
    {
        for (int i = 0; i < HandCards.Length; i++)
        {
            if (handHolderPoints[i] != null)
            {
                HandCards[i].SetCardSObyIndex(cardsIndexes[i]);
            }
        }
    }

    private void HandleCardClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<Card>(out var clickedCard))
            {
                int clickedCardIndex = clickedCard.GetCardParentIndex();
                if (TurnManager.Instance.IsSeatIndexTurn(seatIndex))
                {
                    GameManager.Instance.ClickOnCardServerRpc(clickedCardIndex);
                }
                else
                {
                    Debug.LogWarning("NO ES TU TURNO");
                }
            }
        }
    }

    public Transform HideCardAndGetOrigin(int slotIndex){
        Card card = HandCards[slotIndex];
        if (card != null)
        {
            // Guardamos su posición actual antes de apagarla
            Transform originTransform = card.transform; 
            
            // Ocultamos la carta de nuestra mano (la ilusión)
            card.gameObject.SetActive(false); 
            
            return originTransform;
        }
        return null;
    }

    public void ShowCardsInHand()
    {
        for (int i = 0; i < HandCards.Length; i++)
        {
            HandCards[i].gameObject.SetActive(true);
        }
    }
    public void ReceiveHeadRotation(Quaternion newRotation)
    {
        if (!isLocal)
        {
            targetHeadRotation = newRotation;
        }
    }
    public void ClearHand()
    {
        for (int i = 0; i < HandCards.Length; i++)
        {
            if (HandCards[i] != null)
            {
                HandCards[i].gameObject.SetActive(false);
            }
        }
    }
}