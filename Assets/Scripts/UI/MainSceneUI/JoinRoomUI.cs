using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class JoinRoomUI : MonoBehaviour
{
    [Header("Navegación")]
    [SerializeField] private MainButtonsUI mainScene;
    [SerializeField] private LobbyUI lobbyUI;

    [Header("UI")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button cancelButton;

    void Awake()
    {
        // Ponemos Localhost por defecto para que testees rápido sin tipear
        ipInputField.text = "127.0.0.1";

        connectButton.onClick.AddListener(() =>
        {
            // 1. Agarramos el componente de transporte de Netcode
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            // 2. Le seteamos la IP que escribiste y el puerto por defecto (7777)
            transport.SetConnectionData(ipInputField.text, 7777);
            
            NetworkManager.Singleton.StartClient();
            
            // 4. Pasamos al Lobby
            lobbyUI.Show();
            Hide();
        });

        cancelButton.onClick.AddListener(() =>
        {
            mainScene.Show();
            Hide();
        });
    }

    void Start() => Hide();
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}