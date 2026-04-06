using System;
using TMPro;
using Unity.Services.Lobbies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CurrentPointsTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject[] Team1Points;
    [SerializeField] private GameObject[] Team2Points;
    [SerializeField] private GameObject Team1LocalIndicator;
    [SerializeField] private GameObject Team2LocalIndicator;

    [SerializeField] private GameObject Team1TurnIndicator;
    [SerializeField] private GameObject Team2TurnIndicator;
    [SerializeField] private TextMeshProUGUI Team1Name;
    [SerializeField] private TextMeshProUGUI Team2Name;
    [SerializeField] private TextMeshProUGUI RoundNumber;
    [SerializeField] private Image[] P1_Light_Tracker = new Image[2];
    [SerializeField] private Image[] P2_Light_Tracker = new Image[2];
    private int roundsWonByTeam1 = 0;
    private int roundsWonByTeam2 = 0;
    private int LocalPlayerTeam;

    int currentRound = 1;

    private void Start()
    {
        Team1LocalIndicator.SetActive(false);
        Team2LocalIndicator.SetActive(false);
        Team1TurnIndicator.SetActive(false);
        Team2TurnIndicator.SetActive(false);
        GameManager.Instance.OnTeam1PointsChanged += OnTeam1Points_OnValueChanged;
        GameManager.Instance.OnTeam2PointsChanged += OnTeam2PointsChanged_OnValueChanged;
        GameManager.Instance.OnRoundWined += OnRoundWined_OnValuechanged;
        GameClientManager.Instance.SetNewRound += GCM_SetNewRound;
        GameClientManager.Instance.SetCurrentTurn += GCM_SetCurrentTurn;
        GameClientManager.Instance.PlayersDataReady += (_, __) => SetupTeamUI();
        if (GameClientManager.Instance.GetLocalTeam() != -1) SetupTeamUI();
        RestartLightPoints();
        ResetScores();
    }

    private void OnRoundWined_OnValuechanged(object sender, OnTeamWinnerArgs e)
    {
        if(e.winnerTeam == 1)
        {
            P1_Light_Tracker[roundsWonByTeam1].color = Color.green;
            roundsWonByTeam1++;    
        }else
        {
            P2_Light_Tracker[roundsWonByTeam2].color =  Color.green;;    
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
        UpdateScoreDisplay(Team2Points, e.points);
    }

    private void OnTeam1Points_OnValueChanged(object sender, OnPointsGainedArgs e)
    {
        UpdateScoreDisplay(Team1Points, e.points);
    }

    private void SetupTeamUI()
    {
        LocalPlayerTeam = GameClientManager.Instance.GetLocalTeam();
        int totalplayers = GameManager.Instance.GetTotalPlayers();
        if (LocalPlayerTeam == 1)
        {
            if(totalplayers == 2)
            {
                Team1Name.text = "Vos"; 
                Team2Name.text = "El"; 
            } else
            {
                Team1Name.text = "Nos";
                Team2Name.text = "Ellos";
            }
        }
        else
        {
            if(totalplayers == 2)
            {
                Team1Name.text = "El";
                Team2Name.text = "Yo";
            } else
            {
                Team1Name.text = "Ellos";
                Team2Name.text = "Nos";
            }
        }
    }
    private void GCM_SetCurrentTurn(object sender, IsMyTurnArgs e)
    {
        if (e.TeamTurn == 1)
        {
            Team1TurnIndicator.SetActive(true);
            Team2TurnIndicator.SetActive(false);
            if (e.IsMyTurn)
            {
                if(LocalPlayerTeam == e.TeamTurn)
                {
                    Team1LocalIndicator.SetActive(true);
                }
            } else
            {
                Team2LocalIndicator.SetActive(false);
                Team1LocalIndicator.SetActive(false);
            }
        }
        else
        {
            Team1TurnIndicator.SetActive(false);
            Team2TurnIndicator.SetActive(true);
            if(e.IsMyTurn)
            {
                if(LocalPlayerTeam == e.TeamTurn)
                {
                    Team2LocalIndicator.SetActive(true);
                }
            }else
            {
                Team2LocalIndicator.SetActive(false);
                Team1LocalIndicator.SetActive(false);
            }
        }
    }

    private void RestartLightPoints()
    {
        for (int i = 0; i < P1_Light_Tracker.Length; i++) {
            P1_Light_Tracker[i].color = Color.red;
            P2_Light_Tracker[i].color = Color.red;
        }       
        roundsWonByTeam1 = 0;
        roundsWonByTeam2 = 0;
    }
    private void UpdateScoreDisplay(GameObject[] sticksArray, int currentTotalPoints)
    {
        for (int i = 0; i < sticksArray.Length; i++)
        {
            // Si es mayor o igual, da FALSE (se apaga).
            sticksArray[i].SetActive(i < currentTotalPoints);
        }
    }
    
    private void ResetScores()
    {
        UpdateScoreDisplay(Team1Points, 0);
        UpdateScoreDisplay(Team2Points, 0);
    }
}