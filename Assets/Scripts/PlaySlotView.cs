using UnityEngine;

public class PlaySlotView : MonoBehaviour
{
    [SerializeField] Transform[] PlaySlotAnchors;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform Deck;
    private int seatIndex;
    private int[] cardsIndexes;
    private int nextFreeSlot = 0;
    private void Awake()
    {
        cardsIndexes = new int[3];
        Deck.gameObject.SetActive(false);
    }
    public void SpawnOrUpdateCard(int cardId)
    {
        cardsIndexes[nextFreeSlot] = cardId;
        GameObject cardObject = Instantiate(cardPrefab, PlaySlotAnchors[nextFreeSlot]);
        Transform cardTransform = cardObject.transform;
        cardTransform.localPosition = Vector3.zero;
        Card newCard = cardObject.GetComponent<Card>();
        newCard.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        newCard.SetCardParentIndex(nextFreeSlot);
        newCard.SetCardSObyIndex(cardId);
        nextFreeSlot++;
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
        for (int i = 0; i < cardsIndexes.Length; i++)
        {
            cardsIndexes[i] = -1;
            for (int j = PlaySlotAnchors[i].childCount - 1; j >= 0; j--)
            {
                Destroy(PlaySlotAnchors[i].GetChild(j).gameObject);
            }
        }
    }

    public void LastTurn(bool ImLastTurn)
    {
        if (ImLastTurn) Deck.gameObject.SetActive(true);
        else Deck.gameObject.SetActive(false);
    }
}
