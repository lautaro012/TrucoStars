using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameClientManager : MonoBehaviour
{
    public static GameClientManager Instance { get; private set; }

    private readonly Dictionary<ulong, int> _clientToSeat = new();
    private readonly Dictionary<int, int> _seatToTeam = new();

    public int LocalSeat { get; private set; } = -1;
    public int LocalTeam { get; private set; } = -1;

    public event EventHandler PlayersDataReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ApplyPlayersSnapshot(GameManager.PlayerSnapshot[] snapshots)
    {
        _clientToSeat.Clear();
        _seatToTeam.Clear();

        foreach (var s in snapshots)
        {
            _clientToSeat[s.clientId] = s.seatIndex;
            _seatToTeam[s.seatIndex] = s.team;
        }

        var myId = NetworkManager.Singleton.LocalClientId;
        if (_clientToSeat.TryGetValue(myId, out var mySeat))
        {
            LocalSeat = mySeat;
            LocalTeam = _seatToTeam[mySeat];
        }

        PlayersDataReady?.Invoke(this, EventArgs.Empty);
    }

    public int GetSeatOf(ulong clientId) => _clientToSeat.TryGetValue(clientId, out var s) ? s : -1;
    public int GetTeamOfSeat(int seat) => _seatToTeam.TryGetValue(seat, out var t) ? t : -1;
    public int GetLocalTeam() => LocalTeam;
    public int GetLocalSeat() => LocalSeat;
}