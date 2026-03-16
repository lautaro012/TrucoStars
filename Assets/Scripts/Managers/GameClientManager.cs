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

    [Header("UI Panels Dinámicos")]
    [SerializeField] private DynamicResponsePanelUI dynamicPanel;
    [SerializeField] private GameObject normalActionsPanel;
    [SerializeField] private BackLogUI backlog;

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
        GameManager.Instance.OnEnvidoWinnerDecided += GameManager_OnEnvidoWinnerDecided;
        GameManager.Instance.OnTrucoAccepted += GameManager_OnTrucoAccepted;
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
    //public event EventHandler<EnvidoEventArgs> EnvidoEvent;
    public event EventHandler EnvidoStageEnded;
    //? Eventos de Truco
    //public event EventHandler<TrucoEventArgs> TrucoEvent;
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
        ShowEndRoundText?.Invoke(this, new OnRoundFinishedArgs { shuffleDeck = e.shuffleDeck });
        
        // El resumen al Log:
        backlog.AddLogMessage($"<b>--- RONDA FINALIZADA ---</b>");
        if (e.team1PointsGained > 0) backlog.AddLogMessage($"<color=#55FF55>Equipo 1</color> suma {e.team1PointsGained} puntos.");
        if (e.team2PointsGained > 0) backlog.AddLogMessage($"<color=#FF5555>Equipo 2</color> suma {e.team2PointsGained} puntos.");
    }

    private void GM_OnWaitingEnvidoConfirmation(object sender, OnWaitingConfirmationArgs e)
    {
        if (e.isStageEnded)
        {
            if (normalActionsPanel != null) normalActionsPanel.SetActive(true);
            
            // 2. Avisamos a los botones hijos
            EnvidoStageEnded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            HideEnvidoButtons?.Invoke(this, EventArgs.Empty);
        }
    }
    private void GM_OnWaitingTrucoConfirmation(object sender, OnWaitingConfirmationArgs e)
    {
        if (e.isStageEnded)
        {
            if (normalActionsPanel != null) normalActionsPanel.SetActive(true);
            EnvidoStageEnded?.Invoke(this, EventArgs.Empty); //? termina etapa de envido
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
        backlog.AddLogMessage("Comienzo de partida.");
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
        // 1. Apagamos tus botones de jugar normales
        if (normalActionsPanel != null) normalActionsPanel.SetActive(false);

        // 2. Preparamos los textos según la etapa
        string title = e.trucostage == TrucoStage.Truco ? "¡TE CANTARON TRUCO!" : 
                       e.trucostage == TrucoStage.Retruco ? "¡QUIERO RE-TRUCO!" : "¡QUIERO VALE 4!";
                       
        string raiseText = e.trucostage == TrucoStage.Truco ? "Quiero Re-Truco" : "Quiero Vale 4";
        bool canRaise = e.trucostage != TrucoStage.Vale4; // Si ya es Vale 4, no se puede subir más

        // 3. Armamos la lista de botones base (Quiero y No Quiero)
        var options = new List<ButtonOption>
        {
            new() { buttonText = "Quiero", buttonAction = () => GameManager.Instance.TrucoConfirmation(true), buttonColor = Color.green },
            new() { buttonText = "No Quiero / Mazo", buttonAction = () => GameManager.Instance.Surrender(), buttonColor = Color.red }
        };

        // 4. Si se puede subir la apuesta, lo insertamos PRIMERO en la lista
        if (canRaise)
        {
            options.Insert(0, new ButtonOption { 
                buttonText = raiseText, 
                buttonColor = Color.blue,
                buttonAction = () => { GameManager.Instance.TrucoConfirmation(true); GameManager.Instance.Truco(); } 
            });
        }
        dynamicPanel.ShowOptions(title, options.ToArray());

        string teamColor = e.team == 1 ? "#55FF55" : "#FF5555";
        backlog.AddLogMessage($"<color={teamColor}>Equipo {e.team}</color> cantó <b>{e.trucostage}</b>. Esperando respuesta...");
    }

    private void GM_OnSomeoneCalledEnvido(object sender, OnTeamEnvidoCall e)
    {
        if (normalActionsPanel != null) normalActionsPanel.SetActive(false);

        string title = "EL EQUIPO " + e.team + " CANTÓ " + e.envidoStage;
        
        var options = new List<ButtonOption>();

        // Agregamos las opciones de subir apuesta según corresponda
        if (e.envidoStage == EnvidoStage.Envido)
        {
            options.Add(new ButtonOption { buttonText = "Envido", buttonColor = Color.blue, buttonAction = () => GameManager.Instance.RaiseEnvido(EnvidoStage.Envido) });
            options.Add(new ButtonOption { buttonText = "Real Envido", buttonColor = Color.aliceBlue, buttonAction = () => GameManager.Instance.RaiseEnvido(EnvidoStage.RealEnvido) });
        }
        else if (e.envidoStage == EnvidoStage.EnvidoEnvido)
        {
            options.Add(new ButtonOption { buttonText = "Real Envido", buttonColor = Color.aquamarine ,buttonAction = () => GameManager.Instance.RaiseEnvido(EnvidoStage.RealEnvido) });
        }

        // El Falta Envido siempre está disponible a menos que ya lo hayan cantado
        if (e.envidoStage != EnvidoStage.FaltaEnvido)
        {
            options.Add(new ButtonOption { buttonText = "Falta Envido", buttonColor = Color.coral ,buttonAction = () => GameManager.Instance.RaiseEnvido(EnvidoStage.FaltaEnvido) });
        }

        // Agregamos los obligatorios al final
        options.Add(new ButtonOption { buttonText = "Quiero", buttonColor = Color.green, buttonAction = () => GameManager.Instance.EnvidoConfirmation(true) });
        options.Add(new ButtonOption { buttonText = "No Quiero", buttonColor = Color.red, buttonAction = () => GameManager.Instance.EnvidoConfirmation(false) });

        dynamicPanel.ShowOptions(title, options.ToArray());

        string teamColor = e.team == 1 ? "#55FF55" : "#FF5555";
        backlog.AddLogMessage($"<color={teamColor}>Equipo {e.team}</color> cantó <b>{e.envidoStage}</b>. Esperando respuesta...");
    }
    private void GameManager_OnEnvidoWinnerDecided(object sender, OnEnvidoWinnerArgs e)
    {
        string teamColor = e.winningTeam == 1 ? "#55FF55" : "#FF5555";
        backlog.AddLogMessage($"<color={teamColor}>Equipo {e.winningTeam}</color> ganó el Envido con {e.winningScore} tantos. (+{e.pointsWon} pts)");
    }

    private void GameManager_OnTrucoAccepted(object sender, OnTrucoAcceptedArgs e)
    {
        backlog.AddLogMessage($"El <b>{e.currentStage}</b> fue <color=#55FF55>ACEPTADO</color>. Se juega por {e.pointsAtStake} puntos.");
    }


}
