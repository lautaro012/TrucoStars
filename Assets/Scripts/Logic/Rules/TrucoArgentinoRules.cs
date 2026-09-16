using System.Collections.Generic;
using UnityEngine;

public class TrucoArgentinoRules : GameRulesBase
{
    private Dictionary<int, int[]> cardsInHands = new Dictionary<int, int[]>();
    private Dictionary<int, int> envidoValue_Seats = new Dictionary<int, int>();
    private int[] lastSeats = new int[2];
    private ScoringLogic scorer;
    private int handCount = 0;
    private int firstToPlay = 0;
    private Hand currentHand;
    public TrucoArgentinoRules(GameManager gameManager, int pointsToWin) : base(gameManager, pointsToWin) { }

    /// <summary> 
    ///CREA LA MANO, AUMENTA EL CONTADOR DE MANOS Y SETEA EL PROXIMO EN JUGAR
    /// </summary>
    public override void StartHand()
    {
        int totalPlayers = gameManager.GetTotalPlayers();

        firstToPlay = (firstToPlay + 1) % totalPlayers;
        handCount++;
        ResetHandData();
        
        currentHand = new Hand(handCount, firstToPlay, totalPlayers);
        lastSeats[0] = (firstToPlay + totalPlayers - 1) % totalPlayers;
        lastSeats[1] = (firstToPlay + totalPlayers - 2) % totalPlayers;

        DrawCards(); 
        gameManager.Bridge_StartHandVisuals(lastSeats, cardsInHands, envidoValue_Seats);
        
        AdvanceTurn(currentHand.GetStartingSeatThisHand());
    }

    // --- TU LÓGICA ORIGINAL INTACTA ---
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
            
            int EnvidoValue = scorer.CalculateEnvidoValue(firstCard, secondCard, thirdCard);
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
    // -----------------------------------

    protected override void AdvanceTurn(int winnerSeat = -1)
    {
        if (winnerSeat >= 0)
        {
            gameManager.currentTurnSeatIndex.Value = winnerSeat;
        }
        else
        {
            gameManager.currentTurnSeatIndex.Value = (gameManager.currentTurnSeatIndex.Value + 1) % gameManager.GetTotalPlayers();
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

        // Le pasamos las cartas jugadas y funciones anónimas (lambdas) para buscar el valor y el equipo
        int winnerSeat = scorer.ResolveRound(
            round.SnapshotCards(),
            (cardId) => DeckManager.Instance.GetCardByIndex(cardId).cardValue, 
            (seatIndex) => gameManager.GetPlayerData(seatIndex).team                      
        );

        int winnerTeam = (winnerSeat == -1) ? -1 : gameManager.GetPlayerData(winnerSeat).team;

        // Registramos al ganador de la ronda (Primera, Segunda o Tercera)
        currentHand.RegisterRoundWinner(winnerSeat, winnerTeam); 
        
        // PUENTE VISUAL: Prender luz de victoria en la UI
        gameManager.Bridge_AnnounceRoundWinner(winnerTeam);

        // CHEQUEO: ¿Cerró la mano entera?
        if (!currentHand.IsHandClosed())
        {
            int nextLeader = (winnerSeat >= 0) ? winnerSeat : round.GetLeaderSeat();
            StartNextRound(nextLeader);
        }
        else
        {
            gameManager.SetCurrentGameState(GameState.RoundFinished);
            int handWinnerTeam = currentHand.CalculateWinner();
            
            gameManager.Bridge_CalculatePoints(handWinnerTeam, scorer.GetPointsInPlay());
            CheckForGameWin();

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
    // MÉTODOS VACÍOS (Los llenamos en las próximas fases)
    public override void ProcessEnvidoCall(EnvidoStage call, int callerSeat, int callerTeam)
    {
        if (gameManager.currentGameState.Value != GameState.PlayingTurn && gameManager.currentGameState.Value != GameState.WaitingEnvido)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR Envido, estado actual: " + gameManager.currentGameState.Value);
            return;
        }

        if (scorer.envidoStage == EnvidoStage.RealEnvido && call == EnvidoStage.Envido) return;
        if (scorer.envidoStage == EnvidoStage.FaltaEnvido) return;

        if (gameManager.currentGameState.Value == GameState.PlayingTurn)
        {
            bool isLast = callerSeat == lastSeats[0] || callerSeat == lastSeats[1];
            if (!isLast)
            {
                Debug.LogWarning("SOLOS LOS ASIENTOS PIE pueden cantar envido.");
                return;
            }
        }

        else if (gameManager.currentGameState.Value == GameState.WaitingEnvido)
        {
            if (gameManager.currentContext.Value.envidoCallerTeam == callerTeam)
            {
                Debug.LogWarning("No puedes subir tu propia apuesta.");
                return;
            }
        }

   
        scorer.EnvidoCalled();
        scorer.NextEnvidoStage(call);

        var newEnvidoContext = gameManager.currentContext.Value;
        newEnvidoContext.envidoCallerTeam = callerTeam;
        newEnvidoContext.envidoStage = scorer.envidoStage;
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
            scorer.AddEnvidoPointsByStage();
            int winnerSeat = -1;
            int maxEnvidoValue = -1;            
            foreach (var kvp in envidoValue_Seats)
            {
                if (kvp.Value > maxEnvidoValue)
                {
                    maxEnvidoValue = kvp.Value;
                    winnerSeat = kvp.Key;
                }
            }
            winnerTeam = gameManager.GetPlayerData(winnerSeat).team;
        }
        else
        {
            winnerTeam = (responderTeam == 1) ? 2 : 1;
        }

        if (scorer.envidoStage == EnvidoStage.FaltaEnvido)
        {
            int enemyPoints = (winnerTeam == 1) ? gameManager.GetTeam2TotalPoints() : gameManager.GetTeam1TotalPoints();
            pointsWon = 15 - (enemyPoints % 15);
            Debug.Log("-     FALTA ENVIDO       -");
        }
        else
        {
            pointsWon = scorer.GetEnvidoPointsInPlay();
        } 

        Debug.Log("SE AGREGAN " + pointsWon + " por " + scorer.envidoStage + " AL EQUIPO " + winnerTeam);

        gameManager.Bridge_CalculatePoints(winnerTeam, pointsWon);
        CheckForGameWin();
        var ctx = gameManager.currentContext.Value;
        ctx.envidoCallerTeam = -1;
        gameManager.currentContext.Value = ctx;

        if (gameManager.currentGameState.Value != GameState.RoundFinished)
        {
            gameManager.SetCurrentGameState(GameState.PlayingTurn);
        }
    }


    public override void ProcessTrucoCall(int callerSeat, int callerTeam)
    {     
        scorer.NextTrucoStage();

        var newContext = gameManager.currentContext.Value;
        newContext.trucoCallerTeam = callerTeam;
        newContext.trucoStage = scorer.trucoStage;
        gameManager.currentContext.Value = newContext;
        
        gameManager.SetCurrentGameState(GameState.WaitingTruco);
    }

    public override void ProcessTrucoConfirmation(int responderSeat, bool accepted)
    {
        if (gameManager.currentGameState.Value != GameState.WaitingTruco)
        {
            Debug.Log("[ERROR] No puedes confirmar el Truco en este momento.");
            return;
        }

        if (accepted)
        {
            scorer.TrucoAccepted();
            Debug.Log($"¡Truco aceptado! Ahora se juega por {scorer.GetPointsInPlay()} puntos.");
            gameManager.SetCurrentGameState(GameState.PlayingTurn);
        }
        else
        {
            ProcessSurrender(responderSeat);
        }
    }

    public override void ProcessSurrender(int surrenderSeat)
    {        
        int surrenderTeam = gameManager.GetPlayerData(surrenderSeat).team;
        int winnerTeam = (surrenderTeam == 1) ? 2 : 1;
        Debug.Log($"El equipo rival Se rindio. El equipo gana {scorer.GetPointsInPlay()} puntos.");

        gameManager.SetCurrentGameState(GameState.RoundFinished);
        currentHand.CloseHand();
        
        gameManager.Bridge_CalculatePoints(winnerTeam, scorer.GetPointsInPlay());
        CheckForGameWin();
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

    public override void ResetHandData()
    {
        cardsInHands.Clear();
        envidoValue_Seats.Clear();
        scorer = new ScoringLogic();
        var ctx = gameManager.currentContext.Value;
        ctx.CurrentRound = 0;
        ctx.dealerSeatIdx = firstToPlay; 
        ctx.trucoCallerTeam = -1; 
        ctx.envidoCallerTeam = -1;
        ctx.trucoStage = TrucoStage.None;
        ctx.envidoStage = EnvidoStage.None;
        gameManager.currentContext.Value = ctx;
    }


}