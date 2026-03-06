using UnityEngine;

public class PlaySlotView : MonoBehaviour
{
    [SerializeField] Transform[] PlaySlotAnchors;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform Deck;
    [SerializeField] Transform ShuffledCardAnchor;

    private int seatIndex;
    private Card[] cardsInTable;
    private int nextFreeSlot = 0;
    private void Awake()
    {
        cardsInTable = new Card[3];
        Deck.gameObject.SetActive(false);
    }
    private void Start()
    {
        SpawnCardsInTable();
    }

    //? Inicia las cartas de la mesa y las esconde
    private void SpawnCardsInTable()
    {
        for (int i = 0; i < cardsInTable.Length; i++)
        {
            GameObject cardObject = Instantiate(cardPrefab, PlaySlotAnchors[i]);
            cardObject.transform.localPosition = Vector3.zero;
            Card newCard = cardObject.GetComponent<Card>();
            newCard.gameObject.SetActive(false);
            cardsInTable[i] = newCard;
        }
    }
    public void PlayThisCard(int cardId, Transform origin)
    {
        /*
        cardsIndexes[nextFreeSlot] = cardId;
        GameObject cardObject = Instantiate(cardPrefab, PlaySlotAnchors[nextFreeSlot]);
        Transform cardTransform = cardObject.transform;
        cardTransform.localPosition = Vector3.zero;
        Card newCard = cardObject.GetComponent<Card>();
        */
        Transform destiny = PlaySlotAnchors[nextFreeSlot];

        Card playedCard = cardsInTable[nextFreeSlot];
        RevealPlayedCard(playedCard,cardId);
        
        //* La movemos al origen y la hacemos mover
        playedCard.transform.SetPositionAndRotation(origin.transform.position, origin.transform.rotation);
        playedCard.SmoothMoveCardTo(destiny, 0.4f);
        
        nextFreeSlot++;
    }

    /// <summary>
    /// Setea los datos de la carta y la revela
    /// </summary>
    private void RevealPlayedCard(Card playedCard, int cardID)
    {
        playedCard.SetCardParentIndex(nextFreeSlot);
        playedCard.SetCardSObyIndex(cardID);
        playedCard.gameObject.SetActive(true);
    }
    public void SetSeatIndex(int seat)
    {
        seatIndex = seat;
    }
    public int GetSeatIndex(int seat)
    {
        return seatIndex;
    }
    public void RestartPlaySlot()
    {
        nextFreeSlot = 0;
        for (int i = 0; i < cardsInTable.Length; i++)
        {
            cardsInTable[i].gameObject.SetActive(false);
        }
    }

    public void LastTurn(bool ImLastTurn)
    {
        if (ImLastTurn) Deck.gameObject.SetActive(true);
        else Deck.gameObject.SetActive(false);
    }
    public Transform GetShuffleCardAnchor() => ShuffledCardAnchor;
    public Transform GetDeckPosition() => Deck;
}
