using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PreHostUI : MonoBehaviour
{
    [Header("Navegación")]
    [SerializeField] MainButtonsUI mainScene;
    [SerializeField] LobbyUI lobbyUI;

    [Header("Botones")]
    [SerializeField] Button continueButton;
    [SerializeField] Button cancelButton;

    [Header("Configuraciones Previas")]
    [SerializeField] OptionSelectorUI playersSelector;
    [SerializeField] OptionSelectorUI privacySelector;

    void Awake()
    {
        playersSelector.Setup("Jugadores", new string[] { "2 Jugadores", "4 Jugadores", "6 Jugadores" }, 0);
        playersSelector.OnValueChanged += (index) => 
        {
            if (index == 0) HostSettings.Instance.totalPlayers = 2;
            else if (index == 1) HostSettings.Instance.totalPlayers = 4;
            else if (index == 2) HostSettings.Instance.totalPlayers = 6;
        };

        privacySelector.Setup("Privacidad", new string[] { "Sala Pública", "Sala Privada" }, 0);
        privacySelector.OnValueChanged += (index) =>
        {
            HostSettings.Instance.isPrivate = (index == 1);
        };

        continueButton.onClick.AddListener(() =>
        {
            try
            {
                lobbyUI.Show();
                NetworkManager.Singleton.StartHost();
                Hide();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("La sala ya está creada o el puerto está ocupado: " + e.Message);
                lobbyUI.Hide();
                mainScene.Show();
            }
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