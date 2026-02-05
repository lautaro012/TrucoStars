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
    private NetworkVariable<int> currentTurnSeatIndex = new(-1);
    public NetworkVariable<int> NextRoundLeaderSeatIndex = new(-1);
    
    public event EventHandler<OnChangeTurn_TurnChangedArgs> OnChangedTurn;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentTurnSeatIndex.OnValueChanged += CurrenTurn_OnValueChanged;
        }
    }

    private void CurrenTurn_OnValueChanged(int previousValue, int newValue)
    {
        if (!IsServer || newValue == -1) return;


        var playerData = GameManager.Instance.GetPlayerData(newValue); 
        int currentTeamTurn = playerData.team;
        ulong currentClientId = playerData.clientId;
  
        int[] lastSeats = GameManager.Instance.GetLastSeats();
        
        bool isLast = (newValue == lastSeats[0] || newValue == lastSeats[1]);

        UpdateCurrentTurnRpc(currentTeamTurn, currentClientId, GameManager.Instance.GetCurrentRound(), isLast);
    }

    [Rpc(SendTo.Everyone)] // Sintaxis Unity 6.3
    private void UpdateCurrentTurnRpc(int currentTurnTeam, ulong currentClientId, int round, bool ImLastTurn)
    {
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
            currentTurnSeatIndex.Value = winnerSeat;
        }
        else
        {
            currentTurnSeatIndex.Value = (currentTurnSeatIndex.Value + 1) % GameManager.Instance.GetTotalPlayers();
        }
    }

    public bool IsSeatIndexTurn(int seatindex) => currentTurnSeatIndex.Value == seatindex;
    public int GetCurrentTurnIndex() => currentTurnSeatIndex.Value;
}