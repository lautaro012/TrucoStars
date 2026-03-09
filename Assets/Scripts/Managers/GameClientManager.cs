using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class TrucoEventArgs : EventArgs
{
    public string mainText;
    public string upgradeText;
    public bool hideUpgrade;
}
public class EnvidoEventArgs : EventArgs
{
    public string mainText;
    public string upgradeText;
    public bool hideUpgrade;
}
public class IsMyTurnArgs : EventArgs
{
    public bool IsMyTurn;
    public int value;
    public int TeamTurn;
}
public class CanICallEnvidoArgs : EventArgs
{
    public bool canICallEnvido;
}
public class GameClientManager : MonoBehaviour
{
    public static GameClientManager Instance { get; private set; }
    private readonly Dictionary<ulong, int> _clientToSeat = new();
    private readonly Dictionary<int, int> _seatToTeam = new();

    public int LocalSeat { get; private set; } = -1;
    public int LocalTeam { get; private set; } = -1;

    public bool IsWaitingTruco { get; private set; } = false;

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

        // Avisá a la UI que los datos ya están listos
        PlayersDataReady?.Invoke(this, EventArgs.Empty);
    }

    // Helpers para UI/otros managers
    public int GetSeatOf(ulong clientId) => _clientToSeat.TryGetValue(clientId, out var s) ? s : -1;
    public int GetTeamOfSeat(int seat) => _seatToTeam.TryGetValue(seat, out var t) ? t : -1;
    public int GetLocalTeam() => LocalTeam;
    public int GetLocalSeat() => LocalSeat;
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
    private void Start()
    {
        //GameManager.Instance.OnGameFinished += GM_OnGameFinished;
        GameManager.Instance.OnRoundFinished += GM_OnRoundFinished;
        GameManager.Instance.AreAllPlayersConnected += GM_AreAllPlayersConnected;
        GameManager.Instance.OnRoundStarted += GM_OnRoundStarted;
        TurnManager.Instance.OnChangedTurn += GM_OnChangedTurn;
        GameManager.Instance.OnSomeoneCalledTruco += GM_OnSomeoneCalledTruco;
        GameManager.Instance.OnSomeoneCalledEnvido += GM_OnSomeoneCalledEnvido;
        GameManager.Instance.OnWaitingEnvidoConfirmation += GM_OnWaitingEnvidoConfirmation;
        GameManager.Instance.OnWaitingTrucoConfirmation += GM_OnWaitingTrucoConfirmation;
    }





    //* EVENTOS PARA LA UI */
    public event EventHandler HideLoadingScreen;
    public event EventHandler SetNewRound;
    public event EventHandler<IsMyTurnArgs> SetCurrentTurn;
    public event EventHandler ShowMainRoundButtons;
    public event EventHandler ArePlayingOnlyTwo;
    //? Eventos de Envido
    public event EventHandler<CanICallEnvidoArgs> SetEnvidoButton;
    public event EventHandler HideEnvidoButtons;
    public event EventHandler<EnvidoEventArgs> EnvidoEvent;
    public event EventHandler EnvidoStageEnded;
    //? Eventos de Truco
    public event EventHandler<TrucoEventArgs> TrucoEvent;
    public event EventHandler TrucoStageEnded;
    public event EventHandler HideTrucoButtons;
    public event EventHandler<OnRoundFinishedArgs> ShowEndRoundText;
    /*
        METODOS 
    */
    private void GM_OnRoundFinished(object sender, OnRoundFinishedArgs e)
    {
        HideEnvidoButtons?.Invoke(this, EventArgs.Empty);
        HideTrucoButtons?.Invoke(this, EventArgs.Empty);
        ShowEndRoundText?.Invoke(this, new OnRoundFinishedArgs { shuffleDeck = e.shuffleDeck});
    }
    private void GM_OnWaitingEnvidoConfirmation(object sender, OnWaitingConfirmationArgs e)
    {
        HideEnvidoButtons?.Invoke(this, EventArgs.Empty);
        EnvidoStageEnded?.Invoke(this, EventArgs.Empty);
    }
    private void GM_OnWaitingTrucoConfirmation(object sender, OnWaitingConfirmationArgs e)
    {
        EnvidoStageEnded?.Invoke(this, EventArgs.Empty); //? termina etapa de envido
        if (e.isStageEnded)
        {
            TrucoStageEnded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            HideTrucoButtons?.Invoke(this, EventArgs.Empty);
        }
    }
    private void GM_AreAllPlayersConnected(object sender, EventArgs e)
    {
        HideLoadingScreen?.Invoke(this, EventArgs.Empty);
    }
    private void GM_OnRoundStarted(object sender, EventArgs e)
    {
        ShowMainRoundButtons?.Invoke(this, EventArgs.Empty);    //? MOSTRAR BOTONES PRINCIPALES DE JUEGO
        SetNewRound?.Invoke(this, EventArgs.Empty);             //? SETEAR EL TURNO ACTUAL
        ArePlayingOnlyTwo?.Invoke(this, EventArgs.Empty);       //? CHEQUEAR SI SON DOS JUGADORES
    }
    private void GM_OnChangedTurn(object sender, OnChangeTurn_TurnChangedArgs e)
    {
        //* Setear de quien es el turno
        bool isLocalPlayerturn = NetworkManager.Singleton.LocalClientId == e.clientId;
        bool canCallEnvido = isLocalPlayerturn && e.round == 0 && e.ImLastTurn;
        SetCurrentTurn?.Invoke(this, new IsMyTurnArgs { IsMyTurn = isLocalPlayerturn, TeamTurn = e.team });
        SetEnvidoButton?.Invoke(this, new CanICallEnvidoArgs { canICallEnvido = canCallEnvido });
    }

    private void GM_OnSomeoneCalledTruco(object sender, OnTeamTrucoCall e)
    {
        string MT = "";
        string UT = "";
        bool HUPG = true;
        if (e.trucostage == TrucoStage.Truco)
        {
            MT = "TRUCO";
            UT = "RETRUCO";
        }
        else if (e.trucostage == TrucoStage.Retruco)
        {
            MT = "RETRUCO";
            UT = "QUIERO VALE 4";
        }
        else
        {
            MT = "QUIERO VALE 4";
            UT = "null button";
            HUPG = true;
        }
        TrucoEvent?.Invoke(this, new TrucoEventArgs
        {
            mainText = MT,
            upgradeText = UT,
            hideUpgrade = HUPG
        });
    }

    private void GM_OnSomeoneCalledEnvido(object sender, OnTeamEnvidoCall e)
    {
        string GT = "EL EQUIPO " + e.team + " CANTO " + e.envidoStage;
        string UT = "";
        bool hideUpgradeButton = false;
        if (e.envidoStage == EnvidoStage.Envido)
        {
            UT = "REAL ENVIDO";
        }
        else UT = "FALTA ENVIDO";
        if (e.envidoStage == EnvidoStage.FaltaEnvido) hideUpgradeButton = true;
        EnvidoEvent?.Invoke(this, new EnvidoEventArgs
        {
            mainText = GT,
            upgradeText = UT,
            hideUpgrade = hideUpgradeButton
        });
    }

}
