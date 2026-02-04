
using NUnit.Framework;
using UnityEngine;

public class HandView : MonoBehaviour
{
    private bool isLocal;
    private int seatIndex;
    private ulong clientId;
    private int team;
    private int[] cardsIndexes;

    //* PRIVADO POR CLIENTE LOCAL
    private Card[] HandCards;

    private Unity.Collections.FixedString128Bytes playerId;
    [SerializeField] private Transform[] handHolderPoints;
    [SerializeField] private GameObject cardPrefab;

    private void Awake()
    {
        HandCards = new Card[3];
        cardsIndexes = new int[3];
    }
    private void Update()
    {
        // Solo ejecutamos el raycast cuando el botón izquierdo del ratón es presionado y si la hand es local
        if (!isLocal) return;
        if (Input.GetMouseButtonDown(0)) // 0 es el clic izquierdo
        {
            HandleCardClick();
        }
    }
    public void SetPlayerData(int seatIndex, ulong clientId, int team, Unity.Collections.FixedString128Bytes playerId, bool isMine)
    {
        this.seatIndex = seatIndex;
        this.clientId = clientId;
        this.team = team;
        this.playerId = playerId;
        isLocal = isMine;
        if (!isMine)
        {
            foreach (var c in HandCards)
            {
                if (c == null) continue;
                if (c.TryGetComponent<Collider>(out var col)) col.enabled = false;
            }
        }
        CreateHandView();
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
                cardTransform.localPosition = Vector3.zero;
                Card newCard = cardObject.GetComponent<Card>();
                newCard.SetCardParentIndex(i);
                HandCards[i] = newCard;
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
        if (!isLocal)
        {
            Debug.LogWarning("[Error] NO SE PUEDE JUGAR");
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<Card>(out var clickedCard))
            {
                int clickedCardIndex = clickedCard.GetCardParentIndex();
                if (GameManager.Instance.IsSeatIndexTurn(seatIndex))
                {
                    GameManager.Instance.ClickOnCardServerRpc(clickedCardIndex);
                }
                else
                {
                    Debug.LogWarning("NO ES TU TURNO");
                }
            }
            else
            {
                //Debug.LogWarning("No se encontro carta clickeada");
            }
        }
    }

    public void HideCard(int slotIndex)
    {
        handHolderPoints[slotIndex].gameObject.SetActive(false);
    }
    public void RestarCards()
    {
        for (int i = 0; i < handHolderPoints.Length; i++)
        {
            handHolderPoints[i].gameObject.SetActive(true);
        }
    }

}
