
using Unity.Netcode;
using UnityEngine;

public class OpponentHand : MonoBehaviour, ICardParent
{
    public static OpponentHand Instance { get; private set; }
    
    [SerializeField] private Transform[] cardsHolderPoints;
    [SerializeField] private DeckSO deckSO;
    [SerializeField] private GameObject cardPrefab;
    private Card[] OpponentCard;

    private void Awake()
    {
        OpponentCard = new Card[3];
        Instance = this;
    }

    public void AddNewCards(int[] CardIndexs) {
        for (int i = 0; i < CardIndexs.Length; i++) {
            Transform OpponentHolderPoint = cardsHolderPoints[i];

            GameObject OpponentCard = Instantiate(cardPrefab, OpponentHolderPoint);
            Transform OpponentCardTransform = OpponentCard.transform;
            OpponentCardTransform.localPosition = Vector3.zero;
            Card newOpponentCard = OpponentCard.GetComponent<Card>();

            SetCard(newOpponentCard, i);
        }
    }
    public void SetExistingCard(Card card) {
        int i = FindEmptySlot();
       // card.SetCardParent(this, i, true);
        SetCard(card, i);
    }
    private int FindEmptySlot(){
        for(int i=0; i < OpponentCard.Length; i++) {
            if(OpponentCard[i] == null) {
                return i;
            }
        }
        Debug.LogError("No hay espacio disponible en la mano del oponente");
        return -1; 
    }
    public Transform GetCardHolderPoint(int cardIndex)
    {
        return cardsHolderPoints[cardIndex];
    }

    public void SetCard(Card card, int cardIndex)
    {
        OpponentCard[cardIndex] = card;
    }

    public Card GetCard(int cardIndex)
    {
        return OpponentCard[cardIndex];
    }

    public void ClearCard(int cardIndex)
    {
        OpponentCard[cardIndex] = null;
    }

    public bool HasCard(int cardIndex)
    {
        return OpponentCard[cardIndex] != null;
    }

    public Transform GetOpponentHolderPoint(int cardIndex)
    {
        return cardsHolderPoints[cardIndex];
    }
}
