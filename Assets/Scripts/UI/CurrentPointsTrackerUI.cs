using System;
using TMPro;
using UnityEngine;

public class CurrentPointsTrackerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Team1Points;
    [SerializeField] private TextMeshProUGUI Team2Points;
    [SerializeField] private TextMeshProUGUI Team1Title;
    [SerializeField] private TextMeshProUGUI Team2Title;

    [SerializeField] private TextMeshProUGUI RoundNumber;
    [SerializeField] private TextMeshProUGUI TurnIndicator;
    [SerializeField] private GameObject[] P1_Light_Tracker = new GameObject[2];
    [SerializeField] private GameObject[] P2_Light_Tracker = new GameObject[2];
    private int roundsWonByTeam1 = 0;
    private int roundsWonByTeam2 = 0;


    int currentRound = 1;

    private void Start()
    {
        GameManager.Instance.OnTeam1PointsChanged += OnTeam1Points_OnValueChanged;
        GameManager.Instance.OnTeam2PointsChanged += OnTeam2PointsChanged_OnValueChanged;
        GameManager.Instance.OnRoundWined += OnRoundWined_OnValuechanged;
        GameClientManager.Instance.SetNewRound += GCM_SetNewRound;
        GameClientManager.Instance.SetCurrentTurn += GCM_SetCurrentTurn;
        GameClientManager.Instance.PlayersDataReady += (_, __) => SetupTeamUI();
        if (GameClientManager.Instance.GetLocalTeam() != -1) SetupTeamUI();
        RestartLightPoints();
    }

    private void OnRoundWined_OnValuechanged(object sender, OnTeamWinnerArgs e)
    {
        if(e.winnerTeam == 1)
        {
            P1_Light_Tracker[roundsWonByTeam1].SetActive(true);
            roundsWonByTeam1++;    
        }else
        {
            P2_Light_Tracker[roundsWonByTeam2].SetActive(true);    
            roundsWonByTeam2++;
        }
    }

    private void GCM_SetNewRound(object sender, EventArgs e)
    {
        currentRound++;
        RoundNumber.text = currentRound.ToString();
        RestartLightPoints();
    }

    private void OnTeam2PointsChanged_OnValueChanged(object sender, OnPointsGainedArgs e)
    {
        Team2Points.text = e.points.ToString();
    }

    private void OnTeam1Points_OnValueChanged(object sender, OnPointsGainedArgs e)
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

    private void RestartLightPoints()
    {
        for (int i = 0; i < P1_Light_Tracker.Length; i++) {
            P1_Light_Tracker[i].SetActive(false);
            P2_Light_Tracker[i].SetActive(false);
        }       
        roundsWonByTeam1 = 0;
        roundsWonByTeam2 = 0;
    }
}