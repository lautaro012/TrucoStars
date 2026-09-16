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

    public void SetupTeamColor(int myTeam)
    {
        if (myTeam == 1) Team1Title.color = Color.green;
        else if (myTeam == 2) Team2Title.color = Color.green;
    }

    public void UpdateTeam1Points(int points) => Team1Points.text = points.ToString();
    
    public void UpdateTeam2Points(int points) => Team2Points.text = points.ToString();


    public void AddRoundWinLight(int winnerTeam)
    {
        if(winnerTeam == 1 && roundsWonByTeam1 < P1_Light_Tracker.Length)
        {
            P1_Light_Tracker[roundsWonByTeam1].SetActive(true);
            roundsWonByTeam1++;    
        }
        else if(winnerTeam == 2 && roundsWonByTeam2 < P2_Light_Tracker.Length)
        {
            P2_Light_Tracker[roundsWonByTeam2].SetActive(true);    
            roundsWonByTeam2++;
        }
    }

    public void SetRoundText(int currentRound)
    {
        RoundNumber.text = currentRound.ToString();
    }

    public void SetTurnIndicator(bool isMyTurn)
    {
        TurnIndicator.text = isMyTurn ? "YOUR TURN" : "NOT YOUR TURN";
    }

    public void RestartLightPoints()
    {
        for (int i = 0; i < P1_Light_Tracker.Length; i++) {
            P1_Light_Tracker[i].SetActive(false);
            P2_Light_Tracker[i].SetActive(false);
        }       
        roundsWonByTeam1 = 0;
        roundsWonByTeam2 = 0;
    }
}