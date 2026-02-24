using System;
using Unity.Netcode;
using UnityEngine;

public class ScoringLogic : NetworkBehaviour
{
    public static ScoringLogic Instance { get; private set; }

    [SerializeField] private int pointsToWin = 15;

    //* NETWORK VARIABLES DE PUNTOS
    public NetworkVariable<int> Team1Points = new(0);
    public NetworkVariable<int> Team2Points = new(0);

    //* APUESTAS EN JUEGO (Mano actual)
    private int pointsInPlay = 1; // Puntos del Truco
    private int envidoPointsInPlay = 0; // Puntos del Envido

    //* EVENTOS
    public event EventHandler<OnHandsWonArgs> OnTeam1PointsChanged;
    public event EventHandler<OnHandsWonArgs> OnTeam2PointsChanged;
    public event EventHandler<OnGameFinishedArgs> OnGameFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Team1Points.Value = 0;
            Team2Points.Value = 0;
        }

        // Suscribimos a los cambios para avisarle a la UI
        Team1Points.OnValueChanged += (prev, current) => OnTeam1PointsChanged?.Invoke(this, new OnHandsWonArgs { points = current });
        Team2Points.OnValueChanged += (prev, current) => OnTeam2PointsChanged?.Invoke(this, new OnHandsWonArgs { points = current });
    }

    //* --- MÉTODOS DE TRUCO ---
    public void IncreaseTrucoPoints()
    {
        if (!IsServer) return;
        pointsInPlay++;
    }

    public int GetPointsInPlay() => pointsInPlay;

    //* --- MÉTODOS DE ENVIDO ---
    public void AddEnvidoPointsByStage(EnvidoStage stage)
    {
        if (!IsServer) return;
        switch (stage)
        {
            case EnvidoStage.Envido: envidoPointsInPlay++; break;
            case EnvidoStage.RealEnvido: envidoPointsInPlay += 2; break;
            case EnvidoStage.FaltaEnvido: break; // Se calcula al resolver
        }
    }

    public void AddEnvidoPointsToWinner(int winnerTeam, EnvidoStage stage)
    {
        if (!IsServer) return;

        if (stage == EnvidoStage.FaltaEnvido)
        {
            // Lógica de Falta Envido según las malas/buenas
            int pointsToAdd = winnerTeam == 1 ? (15 - (Team2Points.Value % 15)) : (15 - (Team1Points.Value % 15));
            AddPoints(winnerTeam, pointsToAdd);
            Debug.Log("-     FALTA ENVIDO       -");
        }
        else
        {
            AddPoints(winnerTeam, envidoPointsInPlay);
        }
        Debug.Log($"SE AGREGAN puntos por {stage} AL EQUIPO {winnerTeam}");
    }

    //* --- RESOLUCIÓN DE MANO Y PARTIDA ---
    public void AddHandPointsToWinner(int winnerTeam)
    {
        if (!IsServer) return;
        
        if (winnerTeam != -1) // -1 es empate/pardas
        {
            AddPoints(winnerTeam, pointsInPlay);
        }
        else
        {
            Debug.Log("--- EMPATE TOTAL ---");
        }
    }

    private void AddPoints(int team, int amount)
    {
        if (team == 1) Team1Points.Value += amount;
        else if (team == 2) Team2Points.Value += amount;

        CheckForWin();
    }

    private void CheckForWin()
    {
        if (Team1Points.Value >= pointsToWin) FinishGameClientRpc(1);
        else if (Team2Points.Value >= pointsToWin) FinishGameClientRpc(2);
    }

    [Rpc(SendTo.Everyone)]
    private void FinishGameClientRpc(int win)
    {
        OnGameFinished?.Invoke(this, new OnGameFinishedArgs { winnerTeam = win });
    }

    //* --- REINICIO ---
    public void ResetPointsForNextHand()
    {
        if (!IsServer) return;
        pointsInPlay = 1;
        envidoPointsInPlay = 0;
    }
}