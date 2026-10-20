using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Configuración Visual")]
    [Tooltip("El texto que se va a mover")]
    [SerializeField] private RectTransform textTransform;
    
    [Tooltip("Posicion donde se mueve el texto")]
    [SerializeField] private RectTransform pressedPositionTarget;
    [Tooltip("Colores del texto del boton")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private TextMeshProUGUI text;


    private Vector2 originalPosition;
    private bool isPressed = false;

    private void Start()
    {
        if (textTransform != null)
        {
            originalPosition = textTransform.anchoredPosition;
            text.color = normalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (textTransform != null && pressedPositionTarget != null && !isPressed)
        {
            isPressed = true;
            textTransform.anchoredPosition = pressedPositionTarget.anchoredPosition;
            text.color = pressedColor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (textTransform != null && isPressed)
        {
            isPressed = false;
            textTransform.anchoredPosition = originalPosition;
            text.color = normalColor;
        }
    }
}
