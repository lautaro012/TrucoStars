using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackLogUI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject textArea; 
    [SerializeField] private Transform contentContainer; 
    [SerializeField] private int maxMessages = 20; 
    [SerializeField] private Button closeButton; 
    [SerializeField] private GameObject closeImage;
 
    bool isActive = false;

    void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            isActive = !isActive;
            gameObject.SetActive(isActive);
            closeImage.SetActive(isActive);
        });
    }
    void Start()
    {
        gameObject.SetActive(false);
        closeImage.SetActive(false);
    }
    // La Queue. "Enqueue" mete al final, "Dequeue" saca el primero.
    private Queue<GameObject> messageQueue = new();

    public void AddLogMessage(String message)
    {
        GameObject newTextArea = Instantiate(textArea, contentContainer);
        newTextArea.GetComponent<TextMeshProUGUI>().text = message;
        messageQueue.Enqueue(newTextArea); 

        if(messageQueue.Count > maxMessages)
        {
            GameObject lastMessage = messageQueue.Dequeue();
            Destroy(lastMessage);
        }
    }
}
