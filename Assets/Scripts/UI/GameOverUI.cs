using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalMessage;

    private void Start()
    {
        GameManager.Instance.OnGameFinished += GameManager_OnGameFinished;
        Hide();
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnGameFinished -= GameManager_OnGameFinished;
    }

    private void GameManager_OnGameFinished(object sender, OnTeamWinnerArgs e)
    {
        finalMessage.text = "Ha ganado el equipo " + e.winnerTeam + " !";
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}
