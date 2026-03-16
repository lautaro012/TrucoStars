using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private GameObject cardFront;
    [SerializeField] private GameObject hover;

    private float Yoffset = 0.01f;
    private CardSO cardSO;
    private int cardParentIndex;
    private MeshRenderer frontRenderer;

    void Awake() {
        if (cardFront != null)
            frontRenderer = cardFront.GetComponent<MeshRenderer>();
    }

    public bool isInteractable = false; 

    void OnMouseEnter()
    {
        // Si no es interactable (ej: es del rival), ignoramos el mouse
        if (!isInteractable) return; 

        hover.SetActive(true);
        // Usamos localPosition directo, nada de Translate
        transform.localPosition = new Vector3(
            transform.localPosition.x, 
            transform.localPosition.y + Yoffset, 
            transform.localPosition.z
        );
    }

    void OnMouseExit()
    {
        if (!isInteractable) return;

        hover.SetActive(false);
        // Revertimos la posición matemáticamente
        transform.localPosition = new Vector3(
            transform.localPosition.x, 
            transform.localPosition.y - Yoffset, 
            transform.localPosition.z
        );
    }

    public void SetCardParentIndex(int index) {
         cardParentIndex = index;
    }

    //* SETEA EL CARDSO SEGUN EL INDEX
    public void SetCardSObyIndex(int cardIndex)
    {
        CardSO cardData = DeckManager.Instance.GetCardByIndex(cardIndex);
        SetCardData(cardData);
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

    /// <summary>
    /// Mueve una carta hacia un destino en un tiempo float determinado
    /// </summary>
    /// <param name="destiny"></param>
    /// <param name="Time"></param>
    public void SmoothMoveCardTo(Transform destiny, float time){
        isInteractable = false;
        hover.SetActive(false);
        StartCoroutine(MoveRoutine(destiny,time));
    }

    private IEnumerator MoveRoutine(Transform target, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        // 2. Definimos la posición final (el anclaje) y su rotación plana (acostada)
        Vector3 endPos = target.position;
        // Le damos un pequeño desvío en Z para que no parezca perfecta de robot
        float randomZ = UnityEngine.Random.Range(-5f, 5f);
        Quaternion endRot = Quaternion.Euler(90f, -90f, randomZ);

        float timeElapsed = 0f;
        float archHeight = 0.2f; // Ajustá esto para que vuele más o menos alto

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            // 't' va de 0 a 1
            float t = timeElapsed / duration; 

            // 3. El movimiento suave entre A y B
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            
            // 4. La magia de la curva usando Mathf.Sin
            currentPos.y += Mathf.Sin(t * Mathf.PI) * archHeight;
            
            // 5. La rotación suave y la posicion
            transform.SetPositionAndRotation(currentPos, Quaternion.Lerp(startRot, endRot, t));
            yield return null; // Esperamos al siguiente frame
        }

        // 6. Al terminar, la emparentamos a la mesa y aseguramos que quede perfecta
        transform.SetParent(target);
        transform.SetPositionAndRotation(endPos, endRot);
    }
    public void SetCardMaterial(Material material) {
        frontRenderer.material = material;
    }
}
