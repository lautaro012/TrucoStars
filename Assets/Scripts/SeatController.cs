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

    [Header("Roles de Ronda (UI)")]
    [SerializeField] private TextGlobeUI textGlobeUI;
    [Header("Visuales de Turno")]
    [SerializeField] private GameObject turnIndicatorRoot;
    [SerializeField] private GameObject iconoMano;
    [SerializeField] private GameObject iconoPie;
    [Header("Animation Rigging")]
    [SerializeField] private Transform headTarget; 
    [SerializeField] private Animator animator;

    [Header("Animación Mano Cartas")]
    [SerializeField] private TwoBoneIKConstraint manoCartasIK;
    private float pesoObjetivoIK = 0f;
    [Header("Referencias de Mallas - Señas")]
    [SerializeField] private SkinnedMeshRenderer meshCuerpo;

    [Header("Configuración de Señas")]
    [SerializeField] private float velocidadSena = 250f;

    [Header("Cartas")]
    [SerializeField] private Transform[] handHolderPoints;
    [SerializeField] private GameObject cardPrefab;
    private Card[] HandCards;
    private int[] cardsIndexes;

    [Header("Seleccion de Cartas")]
    private Card cartaSeleccionadaActual; 
    [SerializeField] private LayerMask CartasEnMano; 


    [Header("Cámara y Visión")]
    [SerializeField] private Transform cameraMount; 
    [SerializeField] private Transform HeadTransform; 

    public float mouseSensitivity = 250f;
    public float minPitch = -45f;
    public float maxPitch = 30f;
    public float maxYaw = 45f;
    private float xRotation = 0f; 
    private float yRotation = 0f;

    private const string SenaAnchoEspada = "ancho_espada";
    private const string SenaAnchoBasto = "ancho_basto";
    private const string SenaSieteEspada = "7_izq";
    private const string SenaSieteOro = "7_der";
    private const string SenaTres = "Morder_3";
    private const string SenaDos = "Beso_2";
    private const string SenaNada = "nada";

    [Header("Sincronización de Red")]
    private float syncTimer = 0f;
    private float syncRate = 0.1f; 
    public static Transform LocalPlayerCamera { get; private set; } 
    private Dictionary<Seña, float> targetWeights = new();
    private Quaternion targetHeadRotation;

    private void Awake()
    {
        HandCards = new Card[3];
        cardsIndexes = new int[3];
    }

    void Start()
    {
        GameManager.Instance.OnPlayerMadeCall += GameManager_OnPlayerMadeCall;
        TurnManager.Instance.OnChangedTurn += TurnManager_OnChangedTurn;
        
        if (turnIndicatorRoot != null) turnIndicatorRoot.SetActive(false);
    }

    private void OnDestroy()
    {

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnChangedTurn -= TurnManager_OnChangedTurn;
        }
        if (GameManager.Instance != null) GameManager.Instance.OnPlayerMadeCall -= GameManager_OnPlayerMadeCall;
    }

private void TurnManager_OnChangedTurn(object sender, OnChangeTurn_TurnChangedArgs e)
    {
       
        bool isMyTurn = TurnManager.Instance.GetCurrentTurnIndex() == this.seatIndex; 
        
        if (turnIndicatorRoot != null)
        {
            turnIndicatorRoot.SetActive(isMyTurn);
        }
    }
    private void Update()
    {
        if (isLocal)
        {
            HandleSenaInputs();
            HandleCameraMovement();
            HandleCardHoverAndClick();
            HeadTransform.localScale = Vector3.zero;
        }
        else 
        {
            if (cameraMount != null)
            {
                cameraMount.localRotation = Quaternion.Slerp(cameraMount.localRotation, targetHeadRotation, Time.deltaTime * 12f);
            }
        }
        
        if (headTarget != null && cameraMount != null) 
        {
            Vector3 posicionObjetivo = cameraMount.position + (cameraMount.forward * 2f);
            headTarget.position = posicionObjetivo;
        }

        /*if (manoCartasIK != null)
        {
            manoCartasIK.weight = Mathf.Lerp(manoCartasIK.weight, pesoObjetivoIK, Time.deltaTime * 5f);
        }*/
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
            
            // Calculamos el IK antes del timer de red, para enviar el dato fresco
            pesoObjetivoIK = Mathf.InverseLerp(0f, maxPitch, xRotation);

            syncTimer += Time.deltaTime;
            if (syncTimer >= syncRate)
            {
                syncTimer = 0f;
                // Enviamos ambos datos por la red
                GameManager.Instance.SyncHeadRotationServerRpc(seatIndex, cameraMount.localRotation, pesoObjetivoIK);
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public void SetTurnIndicator(bool isActive)
    {
        if (turnIndicatorRoot != null)
        {
            turnIndicatorRoot.SetActive(isActive);
        }
    }
    private void LateUpdate()
    {
        ActualizarAnimacionSenas();
    }

    private void HandleSenaInputs()
    {
        if (Input.GetKeyDown(KeyCode.Q)) GameManager.Instance.ToggleSenaServerRpc(Seña.AnchoEspada, true);
        if (Input.GetKeyUp(KeyCode.Q)) GameManager.Instance.ToggleSenaServerRpc(Seña.AnchoEspada, false);
        
        if (Input.GetKeyDown(KeyCode.W)) GameManager.Instance.ToggleSenaServerRpc(Seña.AnchoBasto, true);
        if (Input.GetKeyUp(KeyCode.W)) GameManager.Instance.ToggleSenaServerRpc(Seña.AnchoBasto, false);

        if (Input.GetKeyDown(KeyCode.E)) GameManager.Instance.ToggleSenaServerRpc(Seña.Tres, true);
        if (Input.GetKeyUp(KeyCode.E)) GameManager.Instance.ToggleSenaServerRpc(Seña.Tres, false);    
        
        if (Input.GetKeyDown(KeyCode.R)) GameManager.Instance.ToggleSenaServerRpc(Seña.Dos, true);
        if (Input.GetKeyUp(KeyCode.R)) GameManager.Instance.ToggleSenaServerRpc(Seña.Dos, false);

        if (Input.GetKeyDown(KeyCode.T)) GameManager.Instance.ToggleSenaServerRpc(Seña.SieteEspada, true);
        if (Input.GetKeyUp(KeyCode.T)) GameManager.Instance.ToggleSenaServerRpc(Seña.SieteEspada, false);

        if (Input.GetKeyDown(KeyCode.Y)) GameManager.Instance.ToggleSenaServerRpc(Seña.SieteOro, true);
        if (Input.GetKeyUp(KeyCode.Y)) GameManager.Instance.ToggleSenaServerRpc(Seña.SieteOro, false);

        if (Input.GetKeyDown(KeyCode.U)) GameManager.Instance.ToggleSenaServerRpc(Seña.Nada, true);
        if (Input.GetKeyUp(KeyCode.U)) GameManager.Instance.ToggleSenaServerRpc(Seña.Nada, false);
        if (Input.GetKeyDown(KeyCode.A)) animator.SetTrigger("TEST");

        if (Input.GetMouseButtonDown(0)) HandleCardHoverAndClick();
    }

    public void ReceiveHeadRotation(Quaternion newRotation, float newIKWeight)
    {
        if (!isLocal && cameraMount != null)
        {
            targetHeadRotation = newRotation;
            pesoObjetivoIK = newIKWeight; 
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
                    SuavizarBlendShape(meshCuerpo, SenaAnchoEspada, target);
                    break;
                case Seña.AnchoBasto:
                    SuavizarBlendShape(meshCuerpo, SenaAnchoBasto , target);
                    break;
                case Seña.SieteEspada:
                    SuavizarBlendShape(meshCuerpo, SenaSieteEspada, target);
                    break;
                case Seña.SieteOro:
                    SuavizarBlendShape(meshCuerpo, SenaSieteOro, target);
                    break;
                case Seña.Tres:
                    SuavizarBlendShape(meshCuerpo, SenaTres, target);
                    break;
                case Seña.Dos:
                    SuavizarBlendShape(meshCuerpo, SenaDos, target);
                    break;
                default:
                    SuavizarBlendShape(meshCuerpo, SenaNada, target);
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

    /*private void HandleCardClick()
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
    }*/

    public void ShowCardsInHand() { foreach(var c in HandCards) if(c != null) c.gameObject.SetActive(true); }
    public void ClearHand() 
    { 
        for (int i = 0; i < HandCards.Length; i++)
        {
            if (HandCards[i] != null) 
            {
                // 1. Apagamos la carta visualmente
                HandCards[i].gameObject.SetActive(false);
                
                // 2. La devolvemos a su anclaje original en la mano
                HandCards[i].transform.SetParent(handHolderPoints[i]);
                
                // 3. Reseteamos su posición y rotación (0,0,0 local)
                HandCards[i].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                
                // 4. Clave: Volvemos a hacerla interactuable para la nueva mano 
                // (porque la corrutina de tirar la carta lo había puesto en false)
                if (isLocal) 
                {
                    HandCards[i].isInteractable = true;
                }
            }
        }
    }
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
    private void HandleCardHoverAndClick()
    {
        // 1. Tiramos el rayo desde la cámara al mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, CartasEnMano))
        {
            // ¿Es una carta y es interactuable?
            if (hit.collider.TryGetComponent<Card>(out var hitCard) && hitCard.isInteractable)
            {
                // Si el mouse acaba de entrar a una carta NUEVA
                if (cartaSeleccionadaActual != hitCard)
                {
                    // Apagamos la carta vieja (si veníamos de mirar otra)
                    if (cartaSeleccionadaActual != null) 
                        cartaSeleccionadaActual.SetHoverState(false);
                    
                    // Guardamos y prendemos la nueva
                    cartaSeleccionadaActual = hitCard;
                    cartaSeleccionadaActual.SetHoverState(true);
                }

                // LÓGICA DEL CLICK (movida acá adentro para aprovechar que ya sabemos qué carta es)
                if (Input.GetMouseButtonDown(0))
                {
                    if (TurnManager.Instance.IsSeatIndexTurn(seatIndex))
                    {
                        GameManager.Instance.ClickOnCardServerRpc(cartaSeleccionadaActual.GetCardParentIndex());
                        
                        // Apagamos el hover para que no quede flotando al irse a la mesa
                        cartaSeleccionadaActual.SetHoverState(false);
                        cartaSeleccionadaActual = null;
                    }
                }
                
                // Salimos de la función para no ejecutar el paso 3
                return; 
            }
        }

        // 3. Si el raycast no pegó en NADA, o pegó en la mesa/pared
        if (cartaSeleccionadaActual != null)
        {
            // Apagamos la carta que teníamos iluminada
            cartaSeleccionadaActual.SetHoverState(false);
            cartaSeleccionadaActual = null;
        }
    }

    /// <summary>
    /// ACTIVA LOS ICONOS DE MANO Y PIE
    /// </summary>
    /// <param name="isMano"></param>
    /// <param name="isPie"></param>
    public void SetRoundRoles(bool isMano, bool isPie)
    {
        if (iconoMano != null) iconoMano.SetActive(isMano);
        if (iconoPie != null) iconoPie.SetActive(isPie);
    }

}