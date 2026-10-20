using System.Collections.Generic;
using UnityEngine;

public class PuntaYHachaRules : GameRulesBase
{
    private bool calculatePointsDifference; // true = suma total, false = diferencia
    private int team1AccumulatedPoints = 0;
    private int team2AccumulatedPoints = 0;
    private int currentDuelIndex = 0;
    private int[,] duelPairs;
    private Dictionary<int, int[]> cardsInHands = new Dictionary<int, int[]>();
    private Dictionary<int, int> envidoValue_Seats = new Dictionary<int, int>();
    private ScoringLogic scoringLogic;
    private int handCount = 0;
    private int firstToPlay = 0;
    private Hand currentHand;
    public PuntaYHachaRules(GameManager gm, int pointsToWin, bool playWithFlor, bool playWithPicaPica, bool sumPointsMode) 
        : base(gm, pointsToWin, playWithFlor, playWithPicaPica) 
    { 
        this.calculatePointsDifference = sumPointsMode;
    }
    public override void StartHand()
    {
        int totalPlayers = gameManager.GetTotalPlayers();
        int totalDuels = totalPlayers / 2;
        
        firstToPlay = (firstToPlay + 1) % totalPlayers;
        handCount++;
        
        team1AccumulatedPoints = 0;
        team2AccumulatedPoints = 0;
        currentDuelIndex = 0;

        ResetHandData();

        duelPairs = new int[totalDuels, 2];
        for (int i = 0; i < totalDuels; i++)
        {
            duelPairs[i, 0] = (firstToPlay + i) % totalPlayers;       
            duelPairs[i, 1] = (firstToPlay + i + (totalPlayers / 2)) % totalPlayers;   
        }

        DrawCards();


        int[] globalLastSeats = new int[2];
        globalLastSeats[0] = (firstToPlay + totalPlayers - 1) % totalPlayers;
        globalLastSeats[1] = (firstToPlay + totalPlayers - 2) % totalPlayers;

        gameManager.Bridge_StartHandVisuals(globalLastSeats, cardsInHands, envidoValue_Seats);
        StartMiniDuel(currentDuelIndex);
    }
    private void DrawCards()
    {
        int totalPlayers = gameManager.GetTotalPlayers();
        int[] cardsToPlay = GetPlayingCardsIds(totalPlayers);
        
        if (cardsToPlay == null || cardsToPlay.Length < totalPlayers * 3)
        {
            Debug.LogError("Error: No se pudieron obtener las cartas únicas.");
            return;
        }

        for (int i = 0; i < totalPlayers; i++)
        {
            cardsInHands[i] = new int[3];
        }

        for (int i = 0; i < cardsToPlay.Length; i++)
        {
            int seatIndex = i % totalPlayers; 
            cardsInHands[seatIndex][i / totalPlayers] = cardsToPlay[i]; 
        }

        foreach (var kvp in cardsInHands)
        {
            CardSO firstCard = DeckManager.Instance.GetCardByIndex(kvp.Value[0]);
            CardSO secondCard = DeckManager.Instance.GetCardByIndex(kvp.Value[1]);
            CardSO thirdCard = DeckManager.Instance.GetCardByIndex(kvp.Value[2]);
            
            int EnvidoValue = scoringLogic.CalculateEnvidoValue(firstCard, secondCard, thirdCard);
            envidoValue_Seats[kvp.Key] = EnvidoValue;
        }
    }
    private int[] GetPlayingCardsIds(int totalPlayers)
    {
        HashSet<int> cardIDs = new HashSet<int>();
        System.Random rng = new System.Random();

        while (cardIDs.Count < totalPlayers * 3)
        {
            cardIDs.Add(rng.Next(0, 40));
        }

        return System.Linq.Enumerable.ToArray(cardIDs);
    }
    //* cada uno de los 3 juegos 1v1
    private void StartMiniDuel(int duelIndex)
    {
        scoringLogic = new ScoringLogic();
        
        int manoSeat = duelPairs[duelIndex, 0];

        currentHand = new Hand(handCount, manoSeat, gameManager.GetTotalPlayers(), 2);
        // Avisamos a la red quién empieza este 1v1
        var ctx = gameManager.currentContext.Value;
        ctx.dealerSeatIdx = manoSeat; 
        ctx.trucoCallerTeam = -1;
        ctx.envidoCallerTeam = -1;
        ctx.trucoStage = TrucoStage.None;
        ctx.envidoStage = EnvidoStage.None;
        gameManager.currentContext.Value = ctx;

        Debug.Log($"[PICA PICA] Arranca el Duelo {duelIndex}: Asiento {duelPairs[duelIndex, 0]} vs Asiento {duelPairs[duelIndex, 1]}");
        AdvanceTurn(manoSeat);
    }
    protected override void AdvanceTurn(int winnerSeat = -1)
    {
        if (winnerSeat >= 0)
        {
            gameManager.currentTurnSeatIndex.Value = winnerSeat;
        }
        else
        {
            int jugadorA = duelPairs[currentDuelIndex, 0];
            int jugadorB = duelPairs[currentDuelIndex, 1];
            
            if (gameManager.currentTurnSeatIndex.Value == jugadorA)
                gameManager.currentTurnSeatIndex.Value = jugadorB;
            else
                gameManager.currentTurnSeatIndex.Value = jugadorA;
        }
    }

     public override void ProcessCardPlayed(int clientSeat, int cardId, int cardUIIndex)
    {
        // 1. Validaciones básicas de estado (es tu turno, no hay cantos pendientes, etc.)
        if (gameManager.currentTurnSeatIndex.Value != clientSeat) return;
        if (gameManager.currentGameState.Value == GameState.WaitingEnvido || gameManager.currentGameState.Value == GameState.WaitingTruco) return;

        Round round = currentHand.CurrentRound;
        
        // 2. Jugamos la carta en la mano lógica
        bool cardPlayedCorrectly = round.TryPlay(clientSeat, cardId);
        if (!cardPlayedCorrectly) return;

        // 3. ¡PUENTE VISUAL! Movemos la carta en la pantalla de todos
        gameManager.Bridge_MoveCardToTable(cardUIIndex, clientSeat, cardId);
        
        // 4. Quitamos la carta de la mano (usamos tu diccionario de la Regla)
        cardsInHands[clientSeat][cardUIIndex] = -1; 

        // 5. ¿Terminó la ronda (tiraron los 2, 4 o 6)?
        if (!round.IsComplete())
        {
            AdvanceTurn(-1);
            return;
        }

        // Si completaron, evaluamos al ganador
        EvaluateRound();
    }

    protected override void EvaluateRound()
    {
        Round round = currentHand.CurrentRound;
        int winnerSeat = scoringLogic.ResolveRound(
            round.SnapshotCards(),
            (cardId) => DeckManager.Instance.GetCardByIndex(cardId).cardValue, 
            (seatIndex) => gameManager.GetPlayerData(seatIndex).team                      
        );

        int winnerTeam = (winnerSeat == -1) ? -1 : gameManager.GetPlayerData(winnerSeat).team;
        currentHand.RegisterRoundWinner(winnerSeat, winnerTeam); 
        
        gameManager.Bridge_AnnounceRoundWinner(winnerTeam);

        if (!currentHand.IsHandClosed())
        {
            int nextLeader = (winnerSeat >= 0) ? winnerSeat : round.GetLeaderSeat();
            StartNextRound(nextLeader);
        }
        else
        {
            int handWinnerTeam = currentHand.CalculateWinner();
        
            EndMiniDuel(handWinnerTeam, scoringLogic.GetPointsInPlay());
            var ctx = gameManager.currentContext.Value; 
            ctx.trucoCallerTeam = -1; 
            ctx.envidoCallerTeam = -1;
            ctx.trucoStage = TrucoStage.None;
            ctx.envidoStage = EnvidoStage.None;
            gameManager.currentContext.Value = ctx;
        }
    }
    private void StartNextRound(int nextLeader)
    {
        int nextIndex = currentHand.StartNextRound(nextLeader); 
        
        var ctx = gameManager.currentContext.Value;
        ctx.CurrentRound = nextIndex; 
        gameManager.currentContext.Value = ctx;
        
        AdvanceTurn(nextLeader);
    }

    private void EndMiniDuel(int winnerTeamOfThisDuel, int pointsWon)
    {
        if (winnerTeamOfThisDuel == 1) team1AccumulatedPoints += pointsWon;
        else if (winnerTeamOfThisDuel == 2) team2AccumulatedPoints += pointsWon;

        Debug.Log($"[PICA PICA] Fin Duelo {currentDuelIndex}. Acumulado T1: {team1AccumulatedPoints}, T2: {team2AccumulatedPoints}");

        currentDuelIndex++;
        int totalDuels = duelPairs.GetLength(0);
        if (currentDuelIndex < totalDuels)
        {
            StartMiniDuel(currentDuelIndex);
        }
        else
        {
            ResolveFullPicaPicaRound();
        }
    }

    private void ResolveFullPicaPicaRound()
    {
        gameManager.SetCurrentGameState(GameState.RoundFinished);
        currentHand.CloseHand();

        if (calculatePointsDifference)
        {
            //* Suma directa
            if (team1AccumulatedPoints > 0) gameManager.Bridge_CalculatePoints(1, team1AccumulatedPoints);
            if (team2AccumulatedPoints > 0) gameManager.Bridge_CalculatePoints(2, team2AccumulatedPoints);
        }
        else
        {
            //* Diferencia (El Punta y Hacha lento)
            if (team1AccumulatedPoints > team2AccumulatedPoints)
            {
                gameManager.Bridge_CalculatePoints(1, team1AccumulatedPoints - team2AccumulatedPoints);
            }
            else if (team2AccumulatedPoints > team1AccumulatedPoints)
            {
                gameManager.Bridge_CalculatePoints(2, team2AccumulatedPoints - team1AccumulatedPoints);
            }
            // Si empatan, no suma nadie
        }
        CheckForGameWin();
    }
    public override void ProcessSurrender(int surrenderSeat)
    {        
        int surrenderTeam = gameManager.GetPlayerData(surrenderSeat).team;
        int winnerTeam = (surrenderTeam == 1) ? 2 : 1;
        Debug.Log($"El equipo rival Se rindio. El equipo gana {scoringLogic.GetPointsInPlay()} puntos.");

        currentHand.CloseHand();
        
        EndMiniDuel(winnerTeam, scoringLogic.GetPointsInPlay());
    }
    public override void ResetHandData()
    {
        cardsInHands.Clear();
        envidoValue_Seats.Clear();
        scoringLogic = new ScoringLogic();
        
        var ctx = gameManager.currentContext.Value;
        ctx.CurrentRound = 0;
        ctx.trucoCallerTeam = -1; 
        ctx.envidoCallerTeam = -1;
        ctx.trucoStage = TrucoStage.None;
        ctx.envidoStage = EnvidoStage.None;
        gameManager.currentContext.Value = ctx;
    }
    public override void ProcessTrucoCall(int callerSeat, int callerTeam)
    {
        scoringLogic.NextTrucoStage();

        var newContext = gameManager.currentContext.Value;
        newContext.trucoCallerTeam = callerTeam;
        newContext.trucoStage = scoringLogic.trucoStage;
        gameManager.currentContext.Value = newContext;
        
        gameManager.SetCurrentGameState(GameState.WaitingTruco);
    }

    public override void ProcessTrucoConfirmation(int responderSeat, bool accepted)
    {
        if (gameManager.currentGameState.Value != GameState.WaitingTruco) return;

        if (accepted)
        {
            scoringLogic.TrucoAccepted();
            Debug.Log($"[PICA PICA] Truco aceptado en el duelo {currentDuelIndex}. Se juega por {scoringLogic.GetPointsInPlay()} puntos.");
            gameManager.SetCurrentGameState(GameState.PlayingTurn);
        }
        else
        {
            ProcessSurrender(responderSeat);
        }
    }

    public override void ProcessEnvidoCall(EnvidoStage call, int callerSeat, int callerTeam)
    {
        if (gameManager.currentGameState.Value != GameState.PlayingTurn && gameManager.currentGameState.Value != GameState.WaitingEnvido) return;

        if (scoringLogic.envidoStage == EnvidoStage.RealEnvido && call == EnvidoStage.Envido) return;
        if (scoringLogic.envidoStage == EnvidoStage.FaltaEnvido) return;

        if (gameManager.currentGameState.Value == GameState.WaitingEnvido)
        {
            if (gameManager.currentContext.Value.envidoCallerTeam == callerTeam) return;
        }

        scoringLogic.EnvidoCalled();
        scoringLogic.NextEnvidoStage(call);

        var newEnvidoContext = gameManager.currentContext.Value;
        newEnvidoContext.envidoCallerTeam = callerTeam;
        newEnvidoContext.envidoStage = scoringLogic.envidoStage;
        gameManager.currentContext.Value = newEnvidoContext;

        gameManager.SetCurrentGameState(GameState.WaitingEnvido);
    }

    public override void ProcessEnvidoConfirmation(int responderSeat, bool accepted)
    {
        if (gameManager.currentGameState.Value != GameState.WaitingEnvido) return;

        int responderTeam = gameManager.GetPlayerData(responderSeat).team;
        int winnerTeam = -1;
        int pointsWon = 0;

        if (accepted)
        {
            scoringLogic.AddEnvidoPointsByStage();
            int manoSeat = duelPairs[currentDuelIndex, 0];
            int pieSeat = duelPairs[currentDuelIndex, 1];

            int envidoMano = envidoValue_Seats[manoSeat];
            int envidoPie = envidoValue_Seats[pieSeat];
            int winnerSeat = (envidoPie > envidoMano) ? pieSeat : manoSeat;
            winnerTeam = gameManager.GetPlayerData(winnerSeat).team;
            Debug.Log($"[PICA PICA] Envido querido: Mano ({envidoMano}) vs Pie ({envidoPie}). Gana Equipo {winnerTeam}");
        }
        else
        {
            winnerTeam = (responderTeam == 1) ? 2 : 1;
        }

        if (scoringLogic.envidoStage == EnvidoStage.FaltaEnvido)
        {
            int enemyPoints = (winnerTeam == 1) ? gameManager.GetTeam2TotalPoints() : gameManager.GetTeam1TotalPoints();
            pointsWon = 15 - (enemyPoints % 15); 
        }
        else
        {
            pointsWon = scoringLogic.GetEnvidoPointsInPlay();
        } 
        if (winnerTeam == 1) team1AccumulatedPoints += pointsWon;
        else if (winnerTeam == 2) team2AccumulatedPoints += pointsWon;

        Debug.Log($"[PICA PICA] Envido resuelto. Suma {pointsWon} puntos el Equipo {winnerTeam} al acumulado.");

        var ctx = gameManager.currentContext.Value;
        ctx.envidoCallerTeam = -1;
        gameManager.currentContext.Value = ctx;

        if (gameManager.currentGameState.Value != GameState.RoundFinished)
        {
            gameManager.SetCurrentGameState(GameState.PlayingTurn);
        }
    }

    protected override void CheckForGameWin()
    {
        if (gameManager.GetTeam1TotalPoints() >= pointsToWin)
        {
            gameManager.Bridge_FinishGame(1);
        }
        else if (gameManager.GetTeam2TotalPoints() >= pointsToWin)
        {
            gameManager.Bridge_FinishGame(2);
        }
    }

    public override int GetCardId(int seat, int crid) => cardsInHands[seat][crid];

}
