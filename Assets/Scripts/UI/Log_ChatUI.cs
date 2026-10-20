using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Log_ChatUI : MonoBehaviour
{
    [Header("Paneles de Log")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject logPanel;

    [Header("Botones de Pestañas")]
    [SerializeField] Button activateLogPanel;
    [SerializeField] Button activateChatPanel;
    [Header("Color de Pestañas")]
    [SerializeField] private Color colorActivo = Color.white;
    [SerializeField] private Color colorInactivo = Color.gray;
    void Start()
    {
        activateChatPanel.onClick.AddListener(() =>
        {
            
        });
        activateLogPanel.onClick.AddListener(() =>
        {
            
        });
    }
public void ShowChat()
    {
        // Activar/Desactivar paneles
        chatPanel.SetActive(true);
        logPanel.SetActive(false);

        // Feedback visual en los botones
        SetButtonState(activateChatPanel, true);
        SetButtonState(activateLogPanel, false);
    }

    public void ShowLogs()
    {
        // Activar/Desactivar paneles
        chatPanel.SetActive(false);
        logPanel.SetActive(true);

        // Feedback visual en los botones
        SetButtonState(activateChatPanel, false);
        SetButtonState(activateLogPanel, true);
    }

    private void SetButtonState(Button boton, bool estaActivo)
    {
        // Cambia el color del texto del botón para saber cuál está seleccionado
        TMP_Text textoBoton = boton.GetComponentInChildren<TMP_Text>();
        if (textoBoton != null)
        {
            textoBoton.color = estaActivo ? colorActivo : colorInactivo;
        }
    }
}
