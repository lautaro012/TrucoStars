using UnityEngine;
using UnityEngine.UI;

public class TestLobbyUI : MonoBehaviour
{
    [SerializeField] private Button CreateGameButton;
    [SerializeField] private Button JoinGameButton;

    private void Awake()
    {
        CreateGameButton.onClick.AddListener(() => {
           GameManager.Instance.StartHost(); 
           Loader.LoadNetworkManager(Loader.Scene.MainScene);
           Hide();
        });

        JoinGameButton.onClick.AddListener(() => {
            GameManager.Instance.StartClient();
            Loader.LoadNetworkManager(Loader.Scene.MainScene);
            Hide();
        });

    }
    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}
