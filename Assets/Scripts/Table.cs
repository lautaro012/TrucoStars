using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class Table : MonoBehaviour
{

    [SerializeField] private Transform[] seats;

    private Dictionary<ulong, Card[]> CardsOnTable;
    private void Awake()
    {   
        CardsOnTable = new Dictionary<ulong, Card[]>();
    }
    public void SetCard(Card card, int cardIndex)
    {
        Debug.LogError("No deberíamos estar llamando a SetCard en la mesa");
    }

    public Card GetCard(int index)
    {
        Debug.LogError("No deberíamos estar llamando a GetCard en la mesa");
        return null;
    }

    public void ClearCard(int cardIndex)
    {
        Debug.LogError("No deberíamos estar llamando a ClearCard en la mesa");
    }

    public bool HasCard(int cardIndex)
    {
        Debug.LogError("No deberíamos estar llamando a HasCard en la mesa");
        return false;
    }
}
