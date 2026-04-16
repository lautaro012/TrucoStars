using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ButtonOption
{
    public string buttonText;
    public Action buttonAction;
    public Color buttonColor;
}
public class DynamicResponsePanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button[] poolButtons;
    private void Start()
    {
        Hide();
    }

    public void ShowOptions(string title, ButtonOption[] options)
    {
        titleText.text = title;

        for (int i = 0; i < poolButtons.Length; i++)
        {
            if (i < options.Length)
            {
                poolButtons[i].gameObject.SetActive(true);
                poolButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i].buttonText;
                poolButtons[i].GetComponentInChildren<Image>().color = options[i].buttonColor;
                
                //acciones viejas
                poolButtons[i].onClick.RemoveAllListeners();
                
                //! Hay que copiar el índice a una variable local para que el Action funcione bien
                int index = i; 
                poolButtons[i].onClick.AddListener(() => 
                { 
                    options[index].buttonAction?.Invoke(); 
                    Hide(); // Ocultamos el panel al hacer clic
                });
            }
            else
            {
                // No hay más opciones, apagamos este botón
                poolButtons[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    
}
