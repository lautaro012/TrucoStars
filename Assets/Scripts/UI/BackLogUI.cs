using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BackLogUI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject textArea; 
    [SerializeField] private Transform contentContainer; 
    [SerializeField] private int maxMessages = 20; 

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
