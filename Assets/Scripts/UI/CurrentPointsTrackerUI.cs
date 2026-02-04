using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CurrentPointsTrackerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Team1Points;
    [SerializeField] private TextMeshProUGUI Team2Points;
    [SerializeField] private TextMeshProUGUI Team1Title;
    [SerializeField] private TextMeshProUGUI Team2Title;

    [SerializeField] private TextMeshProUGUI RoundNumber;
    [SerializeField] private TextMeshProUGUI TurnIndicator;
    int currentRound = 1;

    private void Start()
    {
        GameManager.Instance.OnTeam1PointsChanged += OnTeam1Points_OnValueChanged;
        GameManager.Instance.OnTeam2PointsChanged += OnTeam2PointsChanged_OnValueChanged;
        GameClientManager.Instance.SetNewRound += GCM_SetNewRound;
        GameClientManager.Instance.SetCurrentTurn += GCM_SetCurrentTurn;
        GameClientManager.Instance.PlayersDataReady += (_, __) => SetupTeamUI();
        if (GameClientManager.Instance.GetLocalTeam() != -1) SetupTeamUI();
    }

    private void GCM_SetNewRound(object sender, EventArgs e)
    {
        currentRound++;
        RoundNumber.text = currentRound.ToString();
    }

    private void OnTeam2PointsChanged_OnValueChanged(object sender, OnHandsWonArgs e)
    {
        Team2Points.text = e.points.ToString();
    }

    private void OnTeam1Points_OnValueChanged(object sender, OnHandsWonArgs e)
    {
        Team1Points.text = e.points.ToString();
    }

    private void SetupTeamUI()
    {
        int myTeam = GameClientManager.Instance.GetLocalTeam();
        if (myTeam == 1) Team1Title.color = Color.green;
        else Team2Title.color = Color.green;
    }
    private void GCM_SetCurrentTurn(object sender, IsMyTurnArgs e)
    {
        if (e.IsMyTurn)
        {
            TurnIndicator.text = "YOUR TURN";
        }
        else
        {
            TurnIndicator.text = "NOT YOUR TURN";
        }
    }

}
