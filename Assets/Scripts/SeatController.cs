using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum Seña {
    AnchoEspada, AnchoBasto, SieteEspada, SieteOro, Tres, Dos, Nada
}

public class SeatController : MonoBehaviour
{
    [Header("Datos de Red")]
    private bool isLocal;
    private int seatIndex;
    private ulong clientId;
    private int team;

    [Header("UI 3D")]
    [SerializeField] private TextGlobeUI textGlobeUI;

    [Header("Animation Rigging")]
    [SerializeField] private Transform headTarget;
    [Header("Animación Mano Cartas")]
    [SerializeField] private TwoBoneIKConstraint manoCartasIK;
    private float pesoObjetivoIK = 0f;
    [Header("Referencias de Mallas - Señas")]
    [SerializeField] private SkinnedMeshRenderer meshCuerpo;
    [SerializeField] private SkinnedMeshRenderer meshCejas;

    [Header("Configuración de Señas")]
    [SerializeField] private float velocidadSena = 500f;

    [Header("Cartas")]
    [SerializeField] private Transform[] handHolderPoints;
    [SerializeField] private GameObject cardPrefab;
    private Card[] HandCards;
    private int[] cardsIndexes;

    [Header("Cámara y Visión")]
    [SerializeField] private Transform cameraMount; 
    public float mouseSensitivity = 250f;
    public float minPitch = -60f;
    public float maxPitch = 30f;
    public float maxYaw = 60f;
    private float xRotation = 0f; 
    private float yRotation = 0f;

    [Header("Sincronización de Red")]
    private float syncTimer = 0f;
    private float syncRate = 0.1f; 
    public static Transform LocalPlayerCamera { get; private set; } 
    private Dictionary<Seña, float> targetWeights = new();

    private void Awake()
    {
        HandCards = new Card[3];
        cardsIndexes = new int[3];
    }

    void Start()
    {
        GameManager.Instance.OnPlayerMadeCall += GameManager_OnPlayerMadeCall;
    }

    private void Update()
    {
        if (isLocal)
        {
            HandleSenaInputs();
            HandleCameraMovement();
        }
        if (headTarget != null && cameraMount != null) 
        {
            // Calculamos un punto a 3 metros en la dirección "Forward" del mount
            Vector3 posicionObjetivo = cameraMount.position + (cameraMount.forward * 2f);
            
            // Aplicamos la posición al objeto físico que el Rig está mirando
            headTarget.position = posicionObjetivo;
        }
    }
 
    private void LateUpdate()
    {
        ActualizarAnimacionSenas();
    }

    private void HandleSenaInputs()
    {
        if (Input.GetKeyDown(KeyCode.E)) GameManager.Instance.ToggleSenaServerRpc(Seña.AnchoEspada, true);
        if (Input.GetKeyUp(KeyCode.E)) GameManager.Instance.ToggleSenaServerRpc(Seña.AnchoEspada, false);
            
        if (Input.GetKeyDown(KeyCode.B)) GameManager.Instance.ToggleSenaServerRpc(Seña.Tres, true);
        if (Input.GetKeyUp(KeyCode.B)) GameManager.Instance.ToggleSenaServerRpc(Seña.Tres, false);

        if (Input.GetMouseButtonDown(0)) HandleCardClick();
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

                cameraMount.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
                
                syncTimer += Time.deltaTime;
                if (syncTimer >= syncRate)
                {
                    syncTimer = 0f;
                    GameManager.Instance.SyncHeadRotationServerRpc(seatIndex, cameraMount.localRotation);
                }

                // NUEVO: Calcular qué tanto estamos mirando hacia abajo
                // InverseLerp devuelve 0 cuando xRotation es 0, y 1 cuando xRotation es igual a maxPitch.
                // Los valores negativos (mirar hacia arriba) se clavan en 0.
                pesoObjetivoIK = Mathf.InverseLerp(0f, maxPitch, xRotation);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                // Opcional: si sueltas el click, la mano vuelve a bajar
                // pesoObjetivoIK = 0f; 
            }

            // Aplicar el peso al IK de forma suave (Lerp) para que no sea un movimiento robótico
            if (manoCartasIK != null)
            {
                manoCartasIK.weight = Mathf.Lerp(manoCartasIK.weight, pesoObjetivoIK, Time.deltaTime * 5f);
            }
        }
    public void ReceiveHeadRotation(Quaternion newRotation)
    {
        if (!isLocal && cameraMount != null)
        {
            cameraMount.localRotation = newRotation;
        }
    }

    public void SetSenaState(Seña sena, bool active)
    {
        targetWeights[sena] = active ? 100f : 0f;
    }

    private void ActualizarAnimacionSenas()
    {
        foreach (var sena in targetWeights.Keys)
        {
            float target = targetWeights[sena];
            switch (sena)
            {
                case Seña.AnchoEspada:
                    SuavizarBlendShape(meshCejas, "ancho_espada", target);
                    SuavizarBlendShape(meshCuerpo, "ancho_espada", target); 
                    break;
                case Seña.AnchoBasto:
                    SuavizarBlendShape(meshCejas, "ancho_basto", target);
                    break;
                case Seña.Tres:
                    SuavizarBlendShape(meshCuerpo, "3", target);
                    break;
                case Seña.Dos:
                    SuavizarBlendShape(meshCuerpo, "2", target);
                    break;
            }
        }
    }

    private void SuavizarBlendShape(SkinnedMeshRenderer mesh, string name, float target)
    {
        if (mesh == null) return;
        int index = mesh.sharedMesh.GetBlendShapeIndex(name);
        if (index == -1) return;

        float current = mesh.GetBlendShapeWeight(index);
        if (Mathf.Approximately(current, target)) return;

        float next = Mathf.MoveTowards(current, target, velocidadSena * Time.deltaTime);
        mesh.SetBlendShapeWeight(index, next);
    }

    // --- MÉTODOS DE SOPORTE (Red, UI, Cartas) ---
    public void SetPlayerData(int seatIndex, ulong clientId, int team, Unity.Collections.FixedString128Bytes playerId, bool isMine)
    {
        this.seatIndex = seatIndex;
        this.clientId = clientId;
        this.team = team;
        isLocal = isMine;

        if (isMine) SetupLocalCamera();
        CreateHandView();
    }

    private void SetupLocalCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && cameraMount != null)
        { 
            mainCam.transform.SetParent(cameraMount);
            mainCam.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(xRotation, yRotation, 0f));
            LocalPlayerCamera = mainCam.transform; 
        }
    }

    private void GameManager_OnPlayerMadeCall(object sender, OnPlayerCalledArgs e)
    {
        if (e.seatIndex == this.seatIndex && textGlobeUI != null) textGlobeUI.ShowMessage(e.callText);
    }

    public void SetCardsIds(int[] cards) { cardsIndexes = cards; UpdateHandView(); }

    public void CreateHandView()
    {
        for (int i = 0; i < handHolderPoints.Length; i++)
        {
            if (handHolderPoints[i] != null)
            {
                GameObject cardObject = Instantiate(cardPrefab, handHolderPoints[i]);
                cardObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                Card newCard = cardObject.GetComponent<Card>();
                newCard.SetCardParentIndex(i);
                if (isLocal) newCard.isInteractable = true; 
                HandCards[i] = newCard;
                newCard.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateHandView()
    {
        for (int i = 0; i < HandCards.Length; i++)
        {
            if (handHolderPoints[i] != null) HandCards[i].SetCardSObyIndex(cardsIndexes[i]);
        }
    }

    private void HandleCardClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<Card>(out var clickedCard))
            {
                if (TurnManager.Instance.IsSeatIndexTurn(seatIndex))
                    GameManager.Instance.ClickOnCardServerRpc(clickedCard.GetCardParentIndex());
            }
        }
    }

    public void ShowCardsInHand() { foreach(var c in HandCards) if(c != null) c.gameObject.SetActive(true); }
    public void ClearHand() { foreach(var c in HandCards) if(c != null) c.gameObject.SetActive(false); }
    private void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.OnPlayerMadeCall -= GameManager_OnPlayerMadeCall; }
    public Transform HideCardAndGetOrigin(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= HandCards.Length) return null;
        
        Card card = HandCards[slotIndex];
        if (card != null)
        {
            // Guardamos su posición actual para que la animación de tirar la carta sepa de dónde salir
            Transform originTransform = card.transform; 
            
            // Ocultamos la carta de nuestra mano (la "ilusión")
            card.gameObject.SetActive(false); 
            
            return originTransform;
        }
        return null;
    }


}