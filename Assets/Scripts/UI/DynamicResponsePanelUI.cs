using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicResponsePanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    
    [Header("Button Pool")]
    [SerializeField] private Button[] actionButtons; 
    [SerializeField] private TextMeshProUGUI[] buttonTexts; 

    private int currentButtonIndex = 0;

    public void Clear()
    {
        currentButtonIndex = 0;
        for (int i = 0; i < actionButtons.Length; i++)
        {
            actionButtons[i].gameObject.SetActive(false);
            actionButtons[i].onClick.RemoveAllListeners();
        }
    }

    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void AddButton(string text, Color color, Action onClickAction)
    {
        if (currentButtonIndex >= actionButtons.Length)
        {
            Debug.LogError("No hay más botones en el pool del DynamicResponsePanel.");
            return;
        }

        Button btn = actionButtons[currentButtonIndex];
        TextMeshProUGUI txt = buttonTexts[currentButtonIndex];

        // Configuramos la vista
        txt.text = text;
       // btn.GetComponent<Image>().color = color;
        
        // Configuramos la acción (el Lambda)
        btn.onClick.AddListener(() => onClickAction?.Invoke());
        
        // Opcional: Que al tocar cualquier botón se oculte el panel automáticamente
        btn.onClick.AddListener(Hide); 

        btn.gameObject.SetActive(true);
        currentButtonIndex++;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}