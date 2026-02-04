using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class TestLobbyUIMainScene : MonoBehaviour
{
    [SerializeField] private Button CreateGameButton;
    [SerializeField] private Button JoinGameButton;

    void Awake()
    {
        CreateGameButton.onClick.AddListener(() =>
        {
            GameManager.Instance.StartHost();
            CreateGameButton.interactable = false;
            JoinGameButton.interactable = false;
            Hide();
        });

        JoinGameButton.onClick.AddListener(() =>
        {
            GameManager.Instance.StartClient();
            CreateGameButton.interactable = false;
            JoinGameButton.interactable = false;
            Hide();
        });

        Show();
    }
    public void Hide() => gameObject.SetActive(false);
    void Show() => gameObject.SetActive(true);
}
