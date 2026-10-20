using UnityEngine;

public class TurnIndicatorUI : MonoBehaviour
{
    [Header("Configuración de Flote")]
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.1f;

    private Vector3 startPos;

    private void Start()
    {
        // Guardamos la posición original (la "Y" base)
        startPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        // 1. ANIMACIÓN DE FLOTE (Rebote vertical)
        float newY = startPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatAmplitude);
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // 2. BILLBOARD (Mirar siempre a la cámara)
        if (SeatController.LocalPlayerCamera != null)
        {
            Transform camTransform = SeatController.LocalPlayerCamera;
            
            // Usamos la misma matemática de tu globo de texto
            transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                             camTransform.rotation * Vector3.up);
        }
    }
}