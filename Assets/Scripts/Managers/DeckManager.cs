using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private DeckSO deckSO;
    [SerializeField] private Sprite[] CardSprites;
    private string[] suits = { "swords", "clubs", "golds", "goblets" };
    private int[] cardNumbers = { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 };
    Dictionary<string, Sprite> spriteDictionary;
    private int CardId = 0;
    public static DeckManager Instance{get; private set;}
    private void Awake()
    {
        Instance = this;
        spriteDictionary = new Dictionary<string, Sprite>();
        GenerateDeck();
    }
    private void GenerateDeck()
    {
        CreateSpriteDiccionary(CardSprites);
        //* Limpio el deck
        deckSO.Cards.Clear();
        foreach (var suit in suits)
        {
            foreach (var cardNumber in cardNumbers)
            {
                string cardKey = cardNumber + "_" + suit; // Ejemplo: "1_Espada"
                if (spriteDictionary.TryGetValue(cardKey, out Sprite foundSprite))
                {
                    CardSO newCardSO = ScriptableObject.CreateInstance<CardSO>();
                    newCardSO.cardSprite = foundSprite;
                    newCardSO.cardName = $"{cardNumber} de {suit}";
                    newCardSO.CardSuit = suit;
                    newCardSO.cardValue = GenerateCardValue(cardNumber, suit);
                    newCardSO.CardId = CardId;
                    newCardSO.EnvidoValue = GenerateEnvidoValue(cardNumber);
                    CardId++;
                    deckSO.Cards.Add(newCardSO);
                }
                else
                {
                    Debug.LogError($"No se encontró la carta: {cardKey}");
                }
            }
        }
        Debug.Log("Deck generado");
    }

    private int GenerateEnvidoValue(int cardNumber)
    {
        if (cardNumber == 10 || cardNumber == 11 || cardNumber == 12)
        {
            return 0;
        }
        else
        {
            return cardNumber;
        }
    }

    private int GenerateCardValue(int cardNumber, string cardSuit)
    {
        return cardNumber switch
        {
            1 when cardSuit == "swords" => 14,
            1 when cardSuit == "clubs" => 13,
            7 when cardSuit == "swords" => 12,
            7 when cardSuit == "golds" => 11,
            3 => 10,
            2 => 9,
            1 => 8,
            12 => 7,
            11 => 6,
            10 => 5,
            7 => 4,
            6 => 3,
            5 => 2,
            4 => 1,
            _ => 0,
        };
    }

    private void CreateSpriteDiccionary(Sprite[] sprites)
    {
        foreach (Sprite sprite in sprites)
        {
            spriteDictionary[sprite.name] = sprite; // Guarda con el nombre como clave
        }
    }
    
    public CardSO GetCardByIndex(int index)
    {
        if (index >= 0 && index < deckSO.Cards.Count)
        {
            return deckSO.Cards[index];
        }
        Debug.LogError($"Índice de carta inválido: {index}");
        return null;
    }   
}
