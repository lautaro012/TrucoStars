using UnityEngine;
using Unity.Netcode;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Referencias a Paneles de UI")]
    [SerializeField] private RoundButtonsUI roundButtonsUI;
    [SerializeField] private EnvidoButtonsUI envidoCallUI;
    [SerializeField] private RoundFinishedUI roundFinishedUI;
    [SerializeField] private CurrentPointsTrackerUI pointsTrackerUI;
    [SerializeField] private DynamicResponsePanelUI responsePanel;
    [SerializeField] private WaitingForOpponentUI waitingForOpponent;
    [SerializeField] private LogChat logChat;

    private void Start()
    {        
        //* Suscripciones a Network Variables
        GameManager.Instance.currentGameState.OnValueChanged += GameManager_OnCurrentGameStateChanged; 
        GameManager.Instance.currentContext.OnValueChanged += GameManager_OnContextChanged;

        //* eventos locales 
        GameManager.Instance.currentTurnSeatIndex.OnValueChanged += GameManager_OnTurnChanged;        
        GameManager.Instance.OnTeam1PointsChanged += (sender, e) => pointsTrackerUI.UpdateTeam1Points(e.points);
        GameManager.Instance.OnTeam2PointsChanged += (sender, e) => pointsTrackerUI.UpdateTeam2Points(e.points);
        GameManager.Instance.SentEnvidoValue += (sender, e) => envidoCallUI.SetPointsText(e.value);
        GameManager.Instance.OnRoundWined += (sender, e) => {
            pointsTrackerUI.AddRoundWinLight(e.winnerTeam);
            logChat.GenerateNewLog("EL EQUIPO " + e.winnerTeam + "GANO LA RONDA");
        };

        GameClientManager.Instance.PlayersDataReady += (sender, e) => {
            pointsTrackerUI.SetupTeamColor(GameClientManager.Instance.GetLocalTeam());
        };
        
        HideAllPanels();
    }

    private void GameManager_OnTurnChanged(int previousSeat, int newSeat)
    {
        EvaluateTurnUI();       
    }

    private void GameManager_OnCurrentGameStateChanged(GameState previousState, GameState newState)
    {
        HideAllPanels(); 
        
        switch (newState)
        {
            case GameState.PlayingTurn:
                EvaluateTurnUI();
                roundButtonsUI.Show();
                break;
            case GameState.SetupHand:
                waitingForOpponent.Hide();
                break;

            case GameState.WaitingTruco:
                CheckTrucoContext();
                break;

            case GameState.WaitingEnvido:
                CheckEnvidoContext();
                break;

            case GameState.RoundFinished:
            case GameState.HandFinished:
                int dealer = GameManager.Instance.currentContext.Value.dealerSeatIdx;
                bool isMyTurnToShuffle = (dealer == GameClientManager.Instance.GetLocalSeat());
                roundFinishedUI.Setup(isMyTurnToShuffle);
                break;
        }
    }

    private void GameManager_OnContextChanged(GameManager.GameContext previousValue, GameManager.GameContext newValue)
    {
        if (GameManager.Instance.currentGameState.Value == GameState.WaitingTruco)
        {
            CheckTrucoContext();
        }
        else if (GameManager.Instance.currentGameState.Value == GameState.WaitingEnvido)
        {
            CheckEnvidoContext();
        }else if (GameManager.Instance.currentGameState.Value == GameState.PlayingTurn)
        {
            EvaluateTurnUI();
        }
    }

    private void CheckTrucoContext()
    {
        int trucoTeam = GameManager.Instance.currentContext.Value.trucoCallerTeam;
        TrucoStage tStage = GameManager.Instance.currentContext.Value.trucoStage;
        if (trucoTeam == -1) return;
        HideAllPanels();
        if (trucoTeam != GameClientManager.Instance.GetLocalTeam())
            BuildTrucoResponse(trucoTeam, tStage);
        else
            waitingForOpponent.Show(); 
    }

    private void CheckEnvidoContext()
    {
        int envidoTeam = GameManager.Instance.currentContext.Value.envidoCallerTeam;
        EnvidoStage eStage = GameManager.Instance.currentContext.Value.envidoStage;
        if (envidoTeam == -1) return;
        HideAllPanels();
        if (envidoTeam != GameClientManager.Instance.GetLocalTeam())
            BuildEnvidoResponse(envidoTeam, eStage);
        else
            waitingForOpponent.Show();
    }

    private void BuildTrucoResponse(int team, TrucoStage stage)
    {
        responsePanel.Clear();
        string callName = stage == TrucoStage.Truco ? "TRUCO" : "RETRUCO";
        responsePanel.SetTitle($"EL EQUIPO {team} CANTÓ {callName}");

        responsePanel.AddButton("QUIERO", Color.green, () => GameManager.Instance.TrucoConfirmation(true));
        responsePanel.AddButton("NO QUIERO", Color.red, () => GameManager.Instance.TrucoConfirmation(false));

        if (stage == TrucoStage.Truco)
        {
            responsePanel.AddButton("RETRUCO", Color.yellow, () => 
            {
                GameManager.Instance.TrucoConfirmation(true);
                GameManager.Instance.Truco();
            });
        }
        else if (stage == TrucoStage.Retruco)
        {
            responsePanel.AddButton("QUIERO VALE 4", Color.yellow, () => 
            {
                GameManager.Instance.TrucoConfirmation(true);
                GameManager.Instance.Truco();
            });
        }

        responsePanel.Show();

        logChat.GenerateNewLog("EL EQUIPO " + team + " CANTO " + callName);
    }

    private void BuildEnvidoResponse(int team, EnvidoStage stage)
    {
        responsePanel.Clear();
        responsePanel.SetTitle($"EL EQUIPO {team} CANTÓ {stage}");

        responsePanel.AddButton("QUIERO", Color.green, () => GameManager.Instance.EnvidoConfirmation(true));
        responsePanel.AddButton("NO QUIERO", Color.red, () => GameManager.Instance.EnvidoConfirmation(false));

        if (stage == EnvidoStage.Envido)
        {
            // ✅ AHORA LLAMAN A Envido()
            responsePanel.AddButton("ENVIDO ENVIDO", Color.yellow, () => GameManager.Instance.Envido(EnvidoStage.EnvidoEnvido));
            responsePanel.AddButton("REAL ENVIDO", Color.yellow, () => GameManager.Instance.Envido(EnvidoStage.RealEnvido));
            responsePanel.AddButton("FALTA ENVIDO", Color.yellow, () => GameManager.Instance.Envido(EnvidoStage.FaltaEnvido));
        }
        else if (stage == EnvidoStage.RealEnvido)
        {
            responsePanel.AddButton("FALTA ENVIDO", Color.yellow, () => GameManager.Instance.Envido(EnvidoStage.FaltaEnvido));
        }

        responsePanel.Show();

        logChat.GenerateNewLog($"EL EQUIPO {team} CANTÓ {stage}");
    }

    private void EvaluateTurnUI()
    {
        int mySeat = GameClientManager.Instance.GetLocalSeat();
        int currentTurn = GameManager.Instance.currentTurnSeatIndex.Value;
        bool myTurnNow = currentTurn == mySeat;
        
        pointsTrackerUI.SetTurnIndicator(myTurnNow);

        if (GameManager.Instance.currentGameState.Value == GameState.PlayingTurn)
        {
            var context = GameManager.Instance.currentContext.Value;
            roundButtonsUI.Setup(myTurnNow, GameClientManager.Instance.GetLocalTeam(), context);
            
            // ✅ Leemos la ronda REAL desde el contexto, no desde el cálculo local
            int currentRound = context.CurrentRound; 
            int[] lastSeats = GameManager.Instance.GetLastSeats();
            bool imLastTurn = (mySeat == lastSeats[0]) || (mySeat == lastSeats[1]);

            // ✅ FIX BUGS 1, 2 y 4: Triple validación
            bool canCallEnvido = myTurnNow && 
                                 currentRound == 0 && 
                                 imLastTurn && 
                                 context.trucoStage == TrucoStage.None && // No se puede si ya hay truco
                                 context.envidoStage == EnvidoStage.None; // No se puede si ya hubo envido
            
            if (canCallEnvido) envidoCallUI.Show();
            else envidoCallUI.Hide();    
        }
    }

    private void HideAllPanels()
    {
        roundButtonsUI.Hide();
        envidoCallUI.Hide();
        roundFinishedUI.Hide();
        responsePanel.Hide();  
        waitingForOpponent.Hide(); 
    }
}