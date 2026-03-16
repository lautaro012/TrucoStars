using System.Collections;
using TMPro;
using UnityEngine;

public class TextGlobeUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject bubbleContainer; 
    [SerializeField] private TextMeshProUGUI speechText; 

    [Header("Configuración")]
    [SerializeField] private float displayTime = 3f; 

    private Coroutine hideCoroutine;

    private void Start()
    {
        bubbleContainer.SetActive(false);
    }

    private void LateUpdate()
    {
        if (SeatController.LocalPlayerCamera == null) return;
        if (bubbleContainer.activeSelf)
        {
            Transform camTransform = SeatController.LocalPlayerCamera;
            
            transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                             camTransform.rotation * Vector3.up);
        }
    }

    public void ShowMessage(string message)
    {
        speechText.text = message;
        bubbleContainer.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(displayTime);
        bubbleContainer.SetActive(false);
    }
}
