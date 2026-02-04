using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private GameObject cardFront;
    private CardSO cardSO;
    private int cardParentIndex;
    private MeshRenderer frontRenderer;

    void Awake() {
        if (cardFront != null)
            frontRenderer = cardFront.GetComponent<MeshRenderer>();
    }

    public void SetCardParentIndex(int index) {
         cardParentIndex = index;
    }

    //* RECIBE LA CARTA Y LE APLICA SU TEXTURA
    private void SetCardData(CardSO cardData ) {
        cardSO = cardData;
        if (cardData.cardSprite == null) return;
        Material newMat = new Material(frontRenderer.material);
        newMat.mainTexture = cardData.cardSprite.texture;

        Rect textureRect = cardData.cardSprite.textureRect;
        newMat.mainTextureScale = new Vector2(
            textureRect.width / cardData.cardSprite.texture.width,
            textureRect.height / cardData.cardSprite.texture.height
        );
        newMat.mainTextureOffset = new Vector2(
            textureRect.x / cardData.cardSprite.texture.width,
            textureRect.y / cardData.cardSprite.texture.height
        );
        frontRenderer.material = newMat; 
    }
    //* SETEA EL CARDSO SEGUN EL INDEX
    public void SetCardSObyIndex(int cardIndex)
    {
        CardSO cardData = DeckManager.Instance.GetCardByIndex(cardIndex);
        SetCardData(cardData);
    }
    public CardSO GetCardSO()
    {
        if (cardSO == null) Debug.LogWarning("CARDSO from card NULL");
        return cardSO;
    }
    public int GetCardParentIndex()
    {
        return cardParentIndex;
    }
    public void DestroySelf() {
        Destroy(gameObject);
    }
    public void UpdateCardPosition(Transform newPosition){
        transform.position = newPosition.position;  
        transform.rotation = newPosition.rotation; 
    }
    public void SetCardMaterial(Material material) {
        frontRenderer.material = material;
    }
}
