using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    [Header("Ajustes de Visión")]
    public float mouseSensitivity = 250f;

    [Header("Límites del Cuello (Grados)")]
    public float minPitch = -60f; // Cuánto puede mirar hacia abajo (a la mesa)
    public float maxPitch = 30f;  // Cuánto puede mirar hacia arriba
    public float maxYaw = 60f;    // Cuánto puede girar a izquierda/derecha

    private float xRotation = 0f; 
    private float yRotation = 0f; 
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        // Opcional: Forzar a que empiece mirando un poco hacia las cartas
        xRotation = -20f; 
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void Update()
    {
        
        if (Input.GetMouseButton(1)) 
        {
            
            Cursor.lockState = CursorLockMode.Locked;

            // 1. Obtenemos el movimiento del mouse
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // 2. Calculamos la rotación vertical y la limitamos (Clamp)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

            // 3. Calculamos la rotación horizontal y la limitamos
            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, -maxYaw, maxYaw);

            // 4. Aplicamos la rotación al Head_Pivot
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetupTableCamera(int totalPlayers)
    {
        if (totalPlayers == 6) // 3v3 (Separados por 60°)
        {
            cam.fieldOfView = 35f; // Lente de retrato (cerrado)
            maxYaw = 65f;          // Te dejo girar lo suficiente para ver al de al lado
        }
        else if (totalPlayers == 4) // 2v2 (Separados por 90°)
        {
            cam.fieldOfView = 45f; // Lente medio
            maxYaw = 95f;          // Te dejo girar más para llegar al de 90°
        }
        else // 1v1 (2 Jugadores, frente a frente)
        {
            cam.fieldOfView = 55f; // Lente abierto
            maxYaw = 30f;          // No hace falta girar tanto el cuello
        }
    }
}