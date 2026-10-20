using System;
using System.Collections;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private MeshRenderer meshRenderer; // Arrastrá el MeshRenderer de tu nueva carta acá
    
    private float Yoffset = 0.01f;
    private CardSO cardSO;
    private int cardParentIndex;
    
    // Guardamos la instancia del material del frente para no crear basura en memoria
    private Material frontMaterialInstance; 
    private bool isCurrentlyHovered = false;
    public bool isInteractable = false; 

    private void Awake() 
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetCardParentIndex(int index) 
    {
         cardParentIndex = index;
    }

    // --- MANEJO DE MATERIAL Y TEXTURA (SPRITE SHEET) ---

    public void SetCardSObyIndex(int cardIndex)
    {
        CardSO cardData = DeckManager.Instance.GetCardByIndex(cardIndex);
        SetCardData(cardData);
    }

    private void SetCardData(CardSO cardData) 
    {
        cardSO = cardData;
        if (cardData.cardSprite == null) return;

        // 1. Obtenemos el arreglo de materiales de la carta
        Material[] mats = meshRenderer.materials;
        
        // 2. Extraemos el material del frente (Índice 0)
        frontMaterialInstance = mats[0]; 

        // ¡NUEVO! Apagamos la luz por defecto apenas la carta se configura
        frontMaterialInstance.DisableKeyword("_EMISSION");
        // 3. Le aplicamos la textura completa del SpriteSheet
        frontMaterialInstance.mainTexture = cardData.cardSprite.texture;

        // 4. Calculamos el recorte exacto de la carta (Tu código original perfecto)
        Rect textureRect = cardData.cardSprite.textureRect;
        frontMaterialInstance.mainTextureScale = new Vector2(
            textureRect.width / cardData.cardSprite.texture.width,
            textureRect.height / cardData.cardSprite.texture.height
        );
        frontMaterialInstance.mainTextureOffset = new Vector2(
            textureRect.x / cardData.cardSprite.texture.width,
            textureRect.y / cardData.cardSprite.texture.height
        );

        // 5. Volvemos a guardar el material modificado en el arreglo
        mats[0] = frontMaterialInstance;
        meshRenderer.materials = mats; 
    }
    
    // --- EFECTO DE HOVER (EMISIÓN Y ELEVACIÓN) ---

    public void SetHoverState(bool isHovering)
    {
        if (!isInteractable || frontMaterialInstance == null) return;

        if (isHovering && !isCurrentlyHovered)
        {
            isCurrentlyHovered = true;
            
            // ¡Prendemos la llave de luz! (Usa la config que dejaste en Unity)
            frontMaterialInstance.EnableKeyword("_EMISSION");
        }
        else if (!isHovering && isCurrentlyHovered)
        {
            isCurrentlyHovered = false;
            
            // Apagamos la llave de luz
            frontMaterialInstance.DisableKeyword("_EMISSION");
        }
    }

    // --- MÉTODOS GETTERS Y ANIMACIÓN (Sin cambios lógicos) ---

    public CardSO GetCardSO()
    {
        if (cardSO == null) Debug.LogWarning("CARDSO from card NULL");
        return cardSO;
    }
    
    public int GetCardParentIndex() => cardParentIndex;
    
    public void DestroySelf() => Destroy(gameObject);

    public void SmoothMoveCardTo(Transform destiny, float time)
    {
        isInteractable = false;
        SetHoverState(false); // Forzamos apagar el hover antes de mover
        StartCoroutine(MoveRoutine(destiny, time));
    }

    private IEnumerator MoveRoutine(Transform target, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        Vector3 endPos = target.position;
        float randomZ = UnityEngine.Random.Range(-5f, 5f);
        Quaternion endRot = Quaternion.Euler(90f, -90f, randomZ);

        float timeElapsed = 0f;
        float archHeight = 0.2f; 

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration; 

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * archHeight;
            
            transform.SetPositionAndRotation(currentPos, Quaternion.Lerp(startRot, endRot, t));
            yield return null; 
        }

        transform.SetParent(target);
        transform.SetPositionAndRotation(endPos, endRot);
    }
}