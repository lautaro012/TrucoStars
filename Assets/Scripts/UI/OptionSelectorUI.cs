using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class OptionSelectorUI : MonoBehaviour
{
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI titleText; 

    private string[] options;
    private int currentIndex = 0;
    public event Action<int> OnValueChanged;

    public void Setup(string title, string[] optionsLabels, int startingIndex = 0)
    {
        if (titleText != null)
        {
            titleText.text = title; 
        }

        this.options = optionsLabels;
        this.currentIndex = startingIndex;
        UpdateDisplay();

        prevButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();

        prevButton.onClick.AddListener(SelectPrevious);
        nextButton.onClick.AddListener(SelectNext);
    }

    private void SelectNext()
    {
        currentIndex = (currentIndex + 1) % options.Length;
        UpdateDisplay();
        OnValueChanged?.Invoke(currentIndex);
    }

    private void SelectPrevious()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = options.Length - 1;
        UpdateDisplay();
        OnValueChanged?.Invoke(currentIndex);
    }

    private void UpdateDisplay()
    {
        if (options != null && options.Length > 0)
        {
            valueText.text = options[currentIndex];
        }
    }
    public void SetInteractable(bool isInteractable)
    {
        prevButton.interactable = isInteractable;
        nextButton.interactable = isInteractable;
    }

    public void SetIndexWithoutNotify(int newIndex)
    {
        if (options == null || options.Length == 0) return;
        currentIndex = Mathf.Clamp(newIndex, 0, options.Length - 1);
        valueText.text = options[currentIndex];
    }
    
}