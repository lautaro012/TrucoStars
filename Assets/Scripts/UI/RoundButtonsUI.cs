using System;
using NUnit.Framework.Constraints;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundButtonsUI : MonoBehaviour
{
    [SerializeField] private Button Truco;
    [SerializeField] private Button EnvidoMainButton;
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
        EnvidoMainButton.onClick.AddListener(() =>
        {
            GameClientManager.Instance.OpenEnvidoSelectionPanel();
        });
        }
    private void Start()
    { 
        GameClientManager.Instance.ShowMainRoundButtons += GCM_OnRoundStarted;
        GameClientManager.Instance.SetCurrentTurn += GCM_SetCurrentTurn;
        GameClientManager.Instance.TrucoStageEnded += GCM_TrucoStageEnded;
        GameClientManager.Instance.HideTrucoButtons += GCM_HideTrucoButtons;
    }
    private void OnDestroy()
    {
        GameClientManager.Instance.ShowMainRoundButtons -= GCM_OnRoundStarted;
        GameClientManager.Instance.SetCurrentTurn -= GCM_SetCurrentTurn;
        GameClientManager.Instance.TrucoStageEnded -= GCM_TrucoStageEnded;
        GameClientManager.Instance.HideTrucoButtons -= GCM_HideTrucoButtons;    
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
            // 1. Mostrar u ocultar Envido (La validación viene del GameClientManager)
            EnvidoMainButton.gameObject.SetActive(GameClientManager.Instance.CanLocalCallEnvido);

            // 2. Textos y lógica del Truco
            SetTrucoButtonText();
            int myTeam = GameClientManager.Instance.GetLocalTeam();
            TrucoStage stage = GameManager.Instance.GetCurrentTrucoStage();
            int lastTeamTruco = GameManager.Instance.GetTeamThatCalledTruco();

            // 3. Mostrar u ocultar Truco
            if (stage == TrucoStage.Vale4 || (lastTeamTruco != -1 && lastTeamTruco == myTeam))
            {
                Truco.gameObject.SetActive(false);
            }
            else
            {
                Truco.gameObject.SetActive(true); // ¡AHORA SÍ!
            }
        }
        else 
        {
            // Si NO es mi turno, me aseguro de apagar todo
            Truco.gameObject.SetActive(false);
            EnvidoMainButton.gameObject.SetActive(false);
        }
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
