using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundButtonsUI : MonoBehaviour
{
    [SerializeField] private Button Truco;
    [SerializeField] private Button Surrender;

    [SerializeField] private TextMeshProUGUI CantarTrucoText;


    private void Awake()
    {
        Truco.onClick.AddListener(() =>
        {
            GameManager.Instance.Truco();
        });
        Surrender.onClick.AddListener(() =>
        {
            GameManager.Instance.Surrender();
        });
    }
    private void Start()
    { 
        GameClientManager.Instance.ShowMainRoundButtons += GCM_OnRoundStarted;
        GameClientManager.Instance.SetCurrentTurn += GCM_SetCurrentTurn;
        GameClientManager.Instance.TrucoStageEnded += GCM_TrucoStageEnded;
        GameClientManager.Instance.HideTrucoButtons += GCM_HideTrucoButtons;
    }

    private void GCM_HideTrucoButtons(object sender, EventArgs e)
    {
        Hide();
    }
    private void GCM_TrucoStageEnded(object sender, EventArgs e)
    {
        Show();
        Truco.gameObject.SetActive(false);
    }

    private void GCM_SetCurrentTurn(object sender, IsMyTurnArgs eventArg)
    {
        if (eventArg.IsMyTurn) Truco.gameObject.SetActive(true);
        else Truco.gameObject.SetActive(false);
    }

    private void GCM_OnRoundStarted(object sender, EventArgs e)
    {
        Show();
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
