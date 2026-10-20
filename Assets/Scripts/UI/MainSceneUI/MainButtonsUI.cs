using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class MainButtonsUI : MonoBehaviour
{
    [SerializeField] Button CreateRoomButton;
    [SerializeField] Button JoinRoomButton;
    [SerializeField] Button SettingsButton;
    [SerializeField] Button ExitButton;
    
    [SerializeField] PreHostUI preHostUI;
    [SerializeField] LobbyUI lobbyUI;
    
    void Awake()
    {
        CreateRoomButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("Sala ya creada en esta instancia.");
                return;
            }
            preHostUI.Show();
            Hide();
        });
        JoinRoomButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsListening) return;
            
            lobbyUI.Show();
            
            if (NetworkManager.Singleton.StartClient())
            {
                Hide();
            }
            else
            {
                Debug.LogWarning("Error crítico al intentar iniciar el cliente local.");
                lobbyUI.Hide();
            }
        });

        // Escuchar si falla la conexión del cliente (ej. no hay host)
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            // Si el que se desconectó somos nosotros (el cliente local)
            if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 0) 
            {
                Debug.Log("No hay sala creada o se perdió la conexión.");
                lobbyUI.Hide();
                Show(); // Volvemos al menú principal
            }
        };
    }

    void Start() => Show();
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}