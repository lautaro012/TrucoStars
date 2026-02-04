using Unity.Netcode;
using UnityEngine;

public interface ICardParent {

    public Transform GetCardHolderPoint(int cardIndex);
    public void SetCard(Card card, int cardIndex);
    public Card GetCard(int cardIndex);
    public void ClearCard(int cardIndex);
    public bool HasCard(int cardIndex);

}
