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
    [SerializeField] private GameObject[] P1_Light_Tracker = new GameObject[2];
    [SerializeField] private GameObject[] P2_Light_Tracker = new GameObject[2];
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
    private void OnDestroy()
    {
        GameManager.Instance.OnTeam1PointsChanged -= OnTeam1Points_OnValueChanged;
        GameManager.Instance.OnTeam2PointsChanged -= OnTeam2PointsChanged_OnValueChanged;
        GameManager.Instance.OnRoundWined -= OnRoundWined_OnValuechanged;
        GameClientManager.Instance.SetNewRound -= GCM_SetNewRound;
        GameClientManager.Instance.SetCurrentTurn -= GCM_SetCurrentTurn;
        GameClientManager.Instance.PlayersDataReady -= (_, __) => SetupTeamUI();
    }

    private void OnRoundWined_OnValuechanged(object sender, OnTeamWinnerArgs e)
    {
        if(e.winnerTeam == 1)
        {
            P1_Light_Tracker[roundsWonByTeam1].SetActive(true);
            roundsWonByTeam1++;    
        }else if(e.winnerTeam == 2)
        {
            P2_Light_Tracker[roundsWonByTeam2].SetActive(true);
            roundsWonByTeam2++;
        } else
        {
            P2_Light_Tracker[roundsWonByTeam2].SetActive(true);
            roundsWonByTeam2++;
            P1_Light_Tracker[roundsWonByTeam1].SetActive(true);
            roundsWonByTeam1++;             
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
                Team1Name.text = "Jugador 1"; 
                Team2Name.text = "Jugador 2"; 
            } else
            {
                Team1Name.text = "Nosotros";
                Team2Name.text = "Ellos";
            }
        }
        else
        {
            if(totalplayers == 2)
            {
                Team1Name.text = "Jugador 2";
                Team2Name.text = "Jugador 1";
            } else
            {
                Team1Name.text = "Ellos";
                Team2Name.text = "Nosotros";
            }
        }
    }
    private void GCM_SetCurrentTurn(object sender, IsMyTurnArgs e)
    {
        if (e.TeamTurn == 1)
        {
            Team1TurnIndicator.SetActive(true);
            Team2TurnIndicator.SetActive(false);

            Team1Name.fontStyle |= FontStyles.Underline;
            Team2Name.fontStyle &= ~FontStyles.Underline;

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
            
            
            Team2Name.fontStyle |= FontStyles.Underline;
            Team1Name.fontStyle &= ~FontStyles.Underline;
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
            P1_Light_Tracker[i].SetActive(false);
            P2_Light_Tracker[i].SetActive(false);
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