using System;
using Unity.Netcode;

public class OnChangeTurn_TurnChangedArgs
{
    public int team; 
    public ulong clientId;
    public int round;
    public bool ImLastTurn;
}

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set;}
    private int currentTurnSeatIndex = -1;
    public int NextRoundLeaderSeatIndex = -1;
    
    public event EventHandler<OnChangeTurn_TurnChangedArgs> OnChangedTurn;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
     // Sintaxis Unity 6.3
    [Rpc(SendTo.Everyone)]
    private void UpdateCurrentTurnRpc(int currentTurnTeam, ulong currentClientId, int round, bool ImLastTurn, int currentTurnIndex)
    {
        currentTurnSeatIndex = currentTurnIndex;
        OnChangedTurn?.Invoke(this, new OnChangeTurn_TurnChangedArgs
        {
            team = currentTurnTeam,
            clientId = currentClientId,
            round = round,
            ImLastTurn = ImLastTurn
        });
    }

    public void AdvanceTurn(int winnerSeat)
    {
        if (!IsServer) return;

        if (winnerSeat >= 0)
        {
            currentTurnSeatIndex = winnerSeat;
        }
        else
        {
            currentTurnSeatIndex = (currentTurnSeatIndex + 1) % GameManager.Instance.GetTotalPlayers();
        }

        var playerData = GameManager.Instance.GetPlayerData(currentTurnSeatIndex); 
        int currentTeamTurn = playerData.team;
        ulong currentClientId = playerData.clientId;
  
        int[] lastSeats = GameManager.Instance.GetLastSeats();
        
        bool isLast = (currentTurnSeatIndex == lastSeats[0]) || (currentTurnSeatIndex == lastSeats[1]);

        UpdateCurrentTurnRpc(currentTeamTurn, currentClientId, GameManager.Instance.GetCurrentRound(), isLast, currentTurnSeatIndex);
    }

    public bool IsSeatIndexTurn(int seatindex) => currentTurnSeatIndex == seatindex;
    public int GetCurrentTurnIndex() => currentTurnSeatIndex;
}