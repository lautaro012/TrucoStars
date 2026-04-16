using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioEnforcer : MonoBehaviour
{
    [Header("Resolución Objetivo")]
    public float targetAspectX = 16f;
    public float targetAspectY = 9f;

    private Camera cam;
    private float lastScreenWidth = 0;
    private float lastScreenHeight = 0;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateAspectRatio();
    }

    void Update()
    {
        // Optimizacion: Solo recalculamos si el jugador redimensionó la ventana
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateAspectRatio();
        }
    }

    private void UpdateAspectRatio()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        // 1. Calculamos las proporciones
        float targetAspect = targetAspectX / targetAspectY;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        
        // 2. Comparamos para saber qué sobra
        float scaleHeight = windowAspect / targetAspect;

        // 3. Aplicamos el recorte
        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else 
        {    
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}