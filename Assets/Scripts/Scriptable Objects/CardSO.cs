using UnityEngine;
using UnityEngine.UI;
//[CreateAssetMenu(fileName = "Card", menuName = "Card", order = 0)]
public class CardSO : ScriptableObject
{
    public string cardName;
    public int cardValue;
    public Sprite cardSprite;
    public string CardSuit;
    public int CardId;
    public int EnvidoValue;
}
