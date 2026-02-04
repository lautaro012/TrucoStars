using System.Collections.Generic;
using UnityEngine;
//[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/Deck", order = 1)]
public class DeckSO : ScriptableObject
{
    public List<CardSO> Cards = new List<CardSO>();
}
