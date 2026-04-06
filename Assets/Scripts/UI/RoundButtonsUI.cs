using System;
using NUnit.Framework.Constraints;
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
        if (eventArg.IsMyTurn)
        {
            SetTrucoButtonText();
            int myTeam = GameClientManager.Instance.GetLocalTeam();
            TrucoStage stage = GameManager.Instance.GetCurrentTrucoStage();


            if (stage == TrucoStage.Vale4 || GameManager.Instance.GetTeamThatCalledTruco() == myTeam)
            {
                Truco.gameObject.SetActive(false);
            }
            else
            {
                Truco.gameObject.SetActive(true);
            }
        }
        else Truco.gameObject.SetActive(false);
    }

    private void GCM_OnRoundStarted(object sender, EventArgs e)
    {
        Show();
    }
    private void Show()
    {
        SetTrucoButtonText();
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetTrucoButtonText()
    {
        string buttonText = GameManager.Instance.GetCurrentTrucoStage() switch
        {
            TrucoStage.None => "Truco",
            TrucoStage.Truco => "RE TRUCO",
            TrucoStage.Retruco => "QUIERO VALE 4",
            TrucoStage.Vale4 => "----",
            _ => "Deafault Truco",
        };
        CantarTrucoText.text = buttonText;
    }
}
