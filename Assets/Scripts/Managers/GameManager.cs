using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class OnWaitingConfirmationArgs : EventArgs
{
    public bool isStageEnded;
}
public class OnSentEnvidoArgs : EventArgs
{
    public int value;
}
public class OnHandsWonArgs : EventArgs
{
    public int points;
}
public class OnRoundFinishedArgs : EventArgs {
    public bool shuffleDeck;   
}
public class OnTeamTrucoCall : EventArgs
{
    public int team;
    public TrucoStage trucostage;
}
public class OnTeamEnvidoCall : EventArgs {
    public int team;
    public EnvidoStage envidoStage;
}
public class OnGameFinishedArgs : EventArgs {
    public int winnerTeam;
}
public class CardClickedEventArgs : EventArgs {
    public int cardIndex;
}

public struct PlayedCard{
    public ulong playerID;
    public CardSO CardSO;
}
public enum RoundState {
    None,
    RoundStarted,
    RoundFinished
}
public enum TrucoStage
{
    None,
    Truco,
    Retruco,
    Vale4
}
public enum EnvidoStage
{
    None,
    Envido,
    EnvidoEnvido,
    RealEnvido,
    FaltaEnvido
}
public class GameManager : NetworkBehaviour
{
    [SerializeField] private DeckSO deckSO;
    [SerializeField] private Table table;
    [SerializeField] private int totalPlayers;
    [SerializeField] private int pointsToWin = 15;
    [SerializeField] private TestLobbyUIMainScene testLobby;
    [SerializeField] private SeatLayoutManager seatLayoutManager;
    [SerializeField] private TablePlayAreaManager tablePlayAreaManager;

    //* ESTRUCTURAS
    public struct PlayerData : INetworkSerializable
    {
        public int seatIndex;
        public ulong clientId;
        public int team;
        public int[] cardsInHands;
        public Unity.Collections.FixedString128Bytes playerId;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref seatIndex);
            s.SerializeValue(ref clientId);
            s.SerializeValue(ref team);
            s.SerializeValue(ref playerId);
            s.SerializeValue(ref cardsInHands);
        }
    }
    public struct PlayerSnapshot : INetworkSerializable
    {
        public int seatIndex;
        public ulong clientId;
        public int team;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref seatIndex);
            s.SerializeValue(ref clientId);
            s.SerializeValue(ref team);
        }
    }

    //* VARIABLES PARA DEFINIR POSICIONES
    private Dictionary<int, PlayerData> Seats;
    private Dictionary<int, HandView> handViews_Seats;
    private Dictionary<ulong, int> clientsId_Seats;
    private Dictionary<int, PlaySlotView> playSlots_Seats;
    private Dictionary<int, int> envidoValue_Seats;
    private int[] LastSeats;


    //* --- EVENTOS --- //
    public event EventHandler AreAllPlayersConnected;
    public event EventHandler OnRoundStarted;
    public event EventHandler<OnRoundFinishedArgs> OnRoundFinished;
    public event EventHandler<OnTeamEnvidoCall> OnSomeoneCalledEnvido;
    public event EventHandler<OnTeamTrucoCall> OnSomeoneCalledTruco;
    public event EventHandler<OnWaitingConfirmationArgs> OnWaitingTrucoConfirmation;
    public event EventHandler<OnWaitingConfirmationArgs> OnWaitingEnvidoConfirmation;
    public event EventHandler<OnHandsWonArgs> OnTeam1PointsChanged;
    public event EventHandler<OnHandsWonArgs> OnTeam2PointsChanged;
    public event EventHandler<OnGameFinishedArgs> OnGameFinished;


    //* --- NETWORK VARIABLES --- */

    private NetworkVariable<int> Team1Points = new(0);
    private NetworkVariable<int> Team2Points = new(0);
    public NetworkVariable<bool> roundFinished = new(false);
    private NetworkVariable<RoundState> currentPhase = new(RoundState.None);

    //* VARIABLES DE JUEGO */
    private bool GameStarted = false;
    private bool isFirstTurn = true;
    private int playersReady = 0;

    //? variables para las manos
    private int HandsWonByTeam1;
    private int HandsWonByTeam2;
    private int firstToPlay = 0; // El índice del jugador que reparte/empieza
    private Hand currentHand;
    private int handCount = 0;
    //? VARIABLES DE ENVIDO
    private bool waitingEnvidoConfirmation = false;
    private int TeamThatCalledEnvido = -1;
    private EnvidoStage envidoStage = EnvidoStage.None;
    private int EnvidoPointsInPlay = 0;
    //? VARIABLES DE TRUCO
    private int TeamThatCalledTruco = -1;
    private bool waitingTrucoConfirmation = false;
    private int pointsInPlay = 1;
    private TrucoStage trucoStage = TrucoStage.None;

    public static GameManager Instance { get; private set; }


    //* --- UNITY METHODS --- */
    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
        clientsId_Seats = new Dictionary<ulong, int>();
        Seats = new Dictionary<int, PlayerData>();
        handViews_Seats = new Dictionary<int, HandView>();
        playSlots_Seats = new Dictionary<int, PlaySlotView>();
        envidoValue_Seats = new();
        seatLayoutManager.OnSeatCreated += SeatLayoutManager_OnSeatCreated;
        tablePlayAreaManager.OnSlotsLaidOut += TablePlayAreaManager_OnSlotLaidOut;
        LastSeats = new int[2];
    }
    private void Update()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.ConnectedClients.Count == totalPlayers && currentPhase.Value == RoundState.None)
        {
            GameStarted = true;
            StartGame();
        }
    }
    public override void OnNetworkSpawn()
    {
        currentPhase.OnValueChanged += RoundState_OnValueChanged;
        Team1Points.OnValueChanged += Team1Points_OnValueChanged;
        Team2Points.OnValueChanged += Team2Points_OnValueChanged;
        roundFinished.OnValueChanged += RoundFinished_OnValueChanged;

    }


    //* EVENTOS LOCALES
    private void SeatLayoutManager_OnSeatCreated(object sender, SeatCreatedEventArgs e)
    {
        handViews_Seats[e.SeatIndex] = e.HandView;
    }
    private void TablePlayAreaManager_OnSlotLaidOut(object sender, OnSlotsLaidOutArgs e)
    {
        playSlots_Seats = e.PlayAreaBySeatIndex;
    }


    //* START GAME FUNCTIONS
    private void StartGame()
    {
        if (!IsServer) return;
        currentPhase.Value = RoundState.RoundStarted;
        roundFinished.Value = false;
        AssignSeats();
        DrawCards();
        CreateSeatsAndPlayAreaClientRpc(
            totalPlayers,
            Vector3.zero,
            1,
            1.5f,
            -90
        );
    }
    [Rpc(SendTo.Everyone)]
    public void CreateSeatsAndPlayAreaClientRpc(int totalPlayers, Vector3 center, float radius, float heightY, float angleOffsetDeg = 0f)
    {
        seatLayoutManager.CreateSeats(
            totalPlayers,
            center,
            radius,
            heightY,
            angleOffsetDeg
        );
        tablePlayAreaManager.CreatePlayArea(
            totalPlayers,
            center,
            radius / 2,
            angleOffsetDeg
        );
    }
    private void AssignSeats()
    {
        clientsId_Seats.Clear();
        Seats.Clear();
        for (int i = 0; i < totalPlayers; i++)
        {
            //*CREO LOS PLAYERDATA, Y CREO LOS ASIENTOS ALREDEDOR DE LA MESA
            int seatIndex = i % totalPlayers; //* Asigno asientos de 0 a 3
            ulong clientId = NetworkManager.Singleton.ConnectedClientsList[i].ClientId;
            PlayerData playerData = new()
            {
                playerId = $"Player_{clientId}",
                team = (i % 2) + 1, //? EQUIPO 1 Y 2 
                seatIndex = seatIndex,
                clientId = clientId,
                cardsInHands = new int[3]
            };
            Seats[seatIndex] = playerData;
            clientsId_Seats[clientId] = seatIndex;
        }
    }
    private void DrawCards()
    {
        // Obtengo los IDs de las cartas a jugar
        int[] cardsToPlay = GetPlayingCardsIds();
        if (cardsToPlay == null || cardsToPlay.Length < totalPlayers * 3)
        {
            Debug.LogError("Error: No se pudieron obtener las cartas únicas.");
            return;
        }
        for (int i = 0; i < cardsToPlay.Length; i++)
        {
            int seatIndex = i % totalPlayers; // Asignar asiento de 0 a 3
            Seats[seatIndex].cardsInHands[i / totalPlayers] = cardsToPlay[i]; // Asignar la carta al array de cartas en mano
        }
        foreach (var kvp in Seats)
        {
            CardSO firstCard = getCardSOfromCardIndex(kvp.Value.cardsInHands[0]);
            CardSO secondCard = getCardSOfromCardIndex(kvp.Value.cardsInHands[1]);
            CardSO thirdCard = getCardSOfromCardIndex(kvp.Value.cardsInHands[2]);
            int EnvidoValue;
            if (firstCard.EnvidoValue > secondCard.EnvidoValue && firstCard.EnvidoValue > thirdCard.EnvidoValue) EnvidoValue = firstCard.EnvidoValue;
            else if (secondCard.EnvidoValue > thirdCard.EnvidoValue) EnvidoValue = secondCard.EnvidoValue;
            else EnvidoValue = thirdCard.EnvidoValue;
            if (
                firstCard.CardSuit == secondCard.CardSuit ||
                firstCard.CardSuit == thirdCard.CardSuit ||
                secondCard.CardSuit == thirdCard.CardSuit
            )
            {
                if (firstCard.CardSuit == secondCard.CardSuit)
                {
                    int newValue = firstCard.EnvidoValue + secondCard.EnvidoValue + 20;
                    if (EnvidoValue < newValue)
                    {
                        EnvidoValue = newValue;
                    }
                }
                if (firstCard.CardSuit == thirdCard.CardSuit)
                {
                    int newValue = firstCard.EnvidoValue + thirdCard.EnvidoValue + 20;
                    if (EnvidoValue < newValue)
                    {
                        EnvidoValue = newValue;
                    }
                }
                if (secondCard.CardSuit == thirdCard.CardSuit)
                {
                    int newValue = thirdCard.EnvidoValue + secondCard.EnvidoValue + 20;
                    if (EnvidoValue < newValue)
                    {
                        EnvidoValue = newValue;
                    }
                }
            }
            envidoValue_Seats[kvp.Key] = EnvidoValue;
        }
    }
    private int[] GetPlayingCardsIds()
    {
        HashSet<int> cardIDs = new();
        System.Random rng = new();

        while (cardIDs.Count < totalPlayers * 3)
        {
            cardIDs.Add(rng.Next(0, 40));
        }

        return cardIDs.ToArray();
    }


    //* FUNCIONES DE CREACION DE JUGADORES
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LayoutReadyServerRpc()
    {
        playersReady++;
        if (playersReady == totalPlayers)
        {
            CreatePlayerHandsServerRpc();
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CreatePlayerHandsServerRpc()
    {
        StartNewHand(); //* INICIA EL JUGADOR EN SEAT 0
        SetPlayersDataClientRPC(Seats.Values.ToArray());
        for (int i = 0; i < totalPlayers; i++)
        {
            int[] cards = Seats[i].cardsInHands;            
            CreateCardsClientRpc(envidoValue_Seats[i], cards, Seats[i].seatIndex, GetRpcTargetParams(new[] { Seats[i].clientId }));
        }
    }
    [Rpc(SendTo.Everyone)]
    private void SetPlayersDataClientRPC(PlayerData[] players)
    {
        var snapshots = new PlayerSnapshot[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            snapshots[i] = new PlayerSnapshot {
                seatIndex = players[i].seatIndex,
                clientId = players[i].clientId,
                team = players[i].team
            };
        }

        GameClientManager.Instance.ApplyPlayersSnapshot(snapshots);

        ulong myId = NetworkManager.Singleton.LocalClientId;
        int mySeat = players.First(p => p.clientId == myId).seatIndex;
        RotateTable(mySeat);
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            bool isMine = p.clientId == myId;
            handViews_Seats[p.seatIndex].SetPlayerData(p.seatIndex, p.clientId, p.team, p.playerId, isMine);
        }
    }

    private void RotateTable(int clientId)
    {
        int totalRotation = (360 / totalPlayers) * clientId;
        //Debug.Log("jugadores: " + totalPlayers + " desde cliente " + clientId);
        table.transform.rotation = Quaternion.Euler(0, totalRotation, 0);
    }

    public event EventHandler<OnSentEnvidoArgs> SentEnvidoValue;
    [Rpc(SendTo.SpecifiedInParams)]
    private void CreateCardsClientRpc(int envidoPoints, int[] cards, int seatindex, RpcParams rpcParams = default)
    {
        handViews_Seats[seatindex].SetCardsIds(cards);
        SentEnvidoValue?.Invoke(this, new OnSentEnvidoArgs
        {
            value = envidoPoints
        });
    }
    /// <summary> 
    ///CREA LA MANO, AUMENTA EL CONTADOR DE MANOS Y SETEA EL PROXIMO EN JUGAR
    /// </summary>
    private void StartNewHand() 
    {
        currentHand = new Hand(handCount, firstToPlay, totalPlayers);
        
        firstToPlay = (firstToPlay + 1) % totalPlayers; 
        handCount++; 

        int lastSeat = (firstToPlay + totalPlayers - 1) % totalPlayers;
        int lastSeat2 = (firstToPlay + totalPlayers - 2) % totalPlayers;
        
        LastSeats[0] = lastSeat;
        LastSeats[1] = lastSeat2;

        ShowDeckClientRpc(LastSeats);

        int initialTurn = currentHand.GetStartingSeatThisHand(); 
        TurnManager.Instance.AdvanceTurn(initialTurn); 
    }
    [Rpc(SendTo.Everyone)]
    private void ShowDeckClientRpc(int[] lastSeats)
    {
        //Debug.Log("le toca el mazo al cliente " + lastSeats[0] + " y devuelve " + lastSeats[1]);
        PlaySlotView LastplaySlot = playSlots_Seats[lastSeats[0]];
        PlaySlotView previousLastPlaySlot = playSlots_Seats[lastSeats[1]];
        LastplaySlot.LastTurn(true);
        previousLastPlaySlot.LastTurn(false);
    }


    //* --- REINICIO DE LA RONDA --- */  
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestStartNextHandServerRpc(RpcParams rpc = default)
    {
        if (!roundFinished.Value)
        {
            Debug.Log("NO TERMINO LA RONDA TODAVIA " + roundFinished.Value);
            return;
        }
        StartNewHand();
        roundFinished.Value = false;
        //TurnManager.Instance.AdvanceTurn(-1);

        //? ARRANCA LA NUEVA MANO
        DrawCards();
        for (int i = 0; i < totalPlayers; i++)
        {
            int[] cards = Seats[i].cardsInHands;
            RestartCardsClientRpc(Seats[i].seatIndex);
            CreateCardsClientRpc(envidoValue_Seats[i], cards, Seats[i].seatIndex, GetRpcTargetParams(new[] { Seats[i].clientId }));
        }
    }
    [Rpc(SendTo.Everyone)]
    private void RestartCardsClientRpc(int seat)
    {
        handViews_Seats[seat].RestarCards();
        playSlots_Seats[seat].RestartPlaySlot();
    }
    //* --- FUNCIONES DE CAMBIOS DE VALORES --- */




    private void RoundFinished_OnValueChanged(bool previousValue, bool newValue)
    {
        if (!IsServer) return;

        if (newValue)
        {
            int dealerSeat = firstToPlay;
            Debug.Log("first to play: " + firstToPlay);
            CallRoundFinishedEventClientRpc(dealerSeat);
        }
        else
        {
            RoundStartedCallClientRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void CallRoundFinishedEventClientRpc(int dealerSeat)
    {
        int mySeat = GameClientManager.Instance.GetLocalSeat();
        bool shuffleDeck = (mySeat == dealerSeat);
        OnRoundFinished?.Invoke(this, new OnRoundFinishedArgs { shuffleDeck = shuffleDeck });
    }
    [Rpc(SendTo.Everyone)]
    private void RoundStartedCallClientRpc()
    {
        OnRoundStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Team2Points_OnValueChanged(int previousValue, int newValue)
    {
        OnTeam2PointsChanged?.Invoke(this, new OnHandsWonArgs { points = newValue });
    }
    private void Team1Points_OnValueChanged(int previousValue, int newValue)
    {
        OnTeam1PointsChanged?.Invoke(this, new OnHandsWonArgs { points = newValue });
    }
    private void RoundState_OnValueChanged(RoundState previousValue, RoundState newValue)
    {
        if (previousValue == RoundState.None)
        {
            AreAllPlayersConnected?.Invoke(this, EventArgs.Empty);
        }
    }





    //* --- JUGAR CARTAS --- */
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ClickOnCardServerRpc(int cardParentIndex, RpcParams rpc = default) //* PERMISO 
    {
        if (waitingEnvidoConfirmation || waitingTrucoConfirmation)
        {
            Debug.LogWarning("NO PUEDES CLICKEAR SI ESTAS ESPERANDO CONFIRMACION");
            return;
        }
        if (roundFinished.Value)
        {
            Debug.Log("RONDA TERMINADA");
            return;
        }
        CardInPlayClientRpc();
        ResolveClickedCard(cardParentIndex, rpc);
    }
    [Rpc(SendTo.Everyone)]
    private void CardInPlayClientRpc()
    {
        //! FUNCION PARA ANUNCIAR CARTA EN JUEGO
    }
    private void ResolveClickedCard(int cardParentIndex, RpcParams rpc = default)
    {
        ulong sender = rpc.Receive.SenderClientId;
        int clientSeat = GetSeatIndexFromClientId(sender);

        //! --- VALIDACIONES ---
        if (clientSeat < 0) return; //? ClientSeat error
        if (!TurnManager.Instance.IsSeatIndexTurn(clientSeat))
        {
            Debug.LogWarning("No es tu turno");
            return;
        }
        if (cardParentIndex < 0 || cardParentIndex >= Seats[clientSeat].cardsInHands.Length) return; //? ParentCardId Error

        int cardId = Seats[clientSeat].cardsInHands[cardParentIndex];
        if (cardId < 0)
        {
            Debug.LogWarning("Carta inválida o ya jugada en ese slot.");
            return;
        }

        if (waitingEnvidoConfirmation || waitingTrucoConfirmation)
        {
            Debug.LogWarning("No puedes jugar mientras hay confirmación pendiente.");
            return;
        }



        //*  --- GUARDADO DE LA CARTA Y CAMBIOS DE UI ---

        Round round = currentHand.CurrentRound;
        bool cardPlayedCorrectly = round.TryPlay(clientSeat, cardId); //? Se intenta jugar la carta en la ronda
        if (!cardPlayedCorrectly)
        {
            Debug.LogError("------ERROR JUGANDO CARTA EN RONDA " + currentHand.GetCurrentRoundIndex() + " --------");
            return;
        }
        MoveCardToTableClientRpc(cardParentIndex, clientSeat, cardId); //? MODIFICAR UI
        Seats[clientSeat].cardsInHands[cardParentIndex] = -1; //? SE QUITA LA CARTA JUGADA

        //* --- CHEQUEO: LA RONDA NO TERMINO? ? PASO TURNO : RESUELVO LA RONDA
        if (!round.IsComplete())
        {
            TurnManager.Instance.AdvanceTurn(-1);
            return;
        }

        //*----- JUGARON TODOS LOS ASIENTOS -------    
        //Debug.Log("-------------------------------------------------------------------");
        //Debug.Log("--- SE JUGARON TODAS LAS CARTAS DE LA RONDA. CALCULANDO GANADOR DE LA RONDA " + currentHand.GetCurrentRoundIndex() + " ---");

        int winnerSeat = ResolveRound(); //? CALCULO ASIENTO GANADOR. -1 INDICA PARDAS
        int winnerTeam;

        if (winnerSeat == -1) winnerTeam = -1;
        else winnerTeam = Seats[winnerSeat].team;

        currentHand.RegisterRoundWinner(winnerSeat, winnerTeam); //? SE GUARDA GANADORES DE LA RONDA ACTUAL Y CIERRA LA MANO

        //* CHEQUEO: LA MANO NO CERRO ? ANUNCIO NUEVA RONDA : ANUNCIO GANADOR
        if (!currentHand.IsHandClosed())
        {
            int nextLeader = (winnerSeat >= 0) ? winnerSeat : round.GetLeaderSeat();
            AnnounceNextRound(nextLeader);
            return;
        }

        //? FLAG DE QUE TERMINO LA RONDA
        roundFinished.Value = true;
        int HandWinner = currentHand.CalculateWinner();
        CalculatePoints(HandWinner);
        RestartDefaultValues();
    }


    //* FUNCIONES UI */
    [Rpc(SendTo.Everyone)]

    private void MoveCardToTableClientRpc(int cardIndex, int clientSeat, int cardId)
    {
        PlaySlotView playSlot = playSlots_Seats[clientSeat];
        HandView hv = handViews_Seats[clientSeat];
        playSlot.SpawnOrUpdateCard(cardId);
        hv.HideCard(cardIndex);
    }
    private int ResolveRound()
    {
        Round round = currentHand.CurrentRound;
        int[] cards = round.SnapshotCards();

        int highestValue = -2;
        List<int> SeatsWhitBestCards = new();
        bool totalCardsPlayed = cards.Length == totalPlayers;
        for (int i = 0; i < cards.Length; i++) //* SE CALCULA ASIENTO/S GANADORES
        {
            int cardId = cards[i];
            if (cardId < 0) continue;
            CardSO card = getCardSOfromCardIndex(cardId);
            int value = card.cardValue;
            if (value > highestValue)
            {
                highestValue = value;
                SeatsWhitBestCards.Clear();
                SeatsWhitBestCards.Add(i);
            }
            else if (value == highestValue) SeatsWhitBestCards.Add(i);
        }

        if (SeatsWhitBestCards.Count == 0)
        {
            Debug.LogError("ResolveRound: NO HUBO CARTAS JUGADAS ?");
            return -1;
        }
        if (SeatsWhitBestCards.Count == 1)
        {
            TurnManager.Instance.AdvanceTurn(SeatsWhitBestCards[0]);
            return SeatsWhitBestCards[0];   //* SI HUBO UN SOLO ASIENTO GANADOR, SE GUARDA Y EMPIEZA LA SIGUIENTE RONDA EL  
        }

        //* --- HAY MAS DE UN GANADOR ---*/
        int team0 = Seats[SeatsWhitBestCards[0]].team;
        bool sameTeam = SeatsWhitBestCards.TrueForAll(s => Seats[s].team == team0);
        if (sameTeam)
        {
            TurnManager.Instance.AdvanceTurn(SeatsWhitBestCards[0]); //* TURNO DEL GANADOR
            return SeatsWhitBestCards[0]; //* QUEDA COMO GANADOR EL QUE JUGO LA PRIMER CARTA ALTA
        }
        TurnManager.Instance.AdvanceTurn(0); //* EMPIEZA EL PRIMER JUGADOR
        return -1; //* EMPATE ENTRE EQUIPOS
    }

    private void AnnounceNextRound(int nextLeader)
    {
        TurnManager.Instance.NextRoundLeaderSeatIndex.Value = nextLeader;
        StartNextRound(nextLeader);
    }
    private void StartNextRound(int nextLeader)
    {
        int nextIndex = currentHand.NextCurrentRoundIndex();  //* AVANZA DE RONDA Y CIERRA LA MANO SI ESTAMOS EN LA ULTIMA
        currentHand.Rounds.Add(new Round(
            roundIndex: nextIndex,
            leaderSeat: nextLeader,
            totalPlayers: totalPlayers
        ));
        // Señal visual (opcional)
    }
    [Rpc(SendTo.Everyone)]
    private void NextRoundStartedClientRpc()
    {
        //TODO AGREGAR PUNTOS ROJOS EN EL MARCADOR DE RONDAS 
    }
    //* --- CONFIRMACION DE TRUCO --- */
    public void TrucoConfirmation(bool Isaccepted)
    {
        TrucoConfirmationServerRpc(Isaccepted);
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TrucoConfirmationServerRpc(bool accepted, RpcParams rpc = default)
    {
        if (waitingTrucoConfirmation == false)
        {
            Debug.Log("[ERROR] No puedes confirmar el Truco en este momento.");
            return;
        }
        if (accepted)
        {
            // Aceptaron el Truco
            pointsInPlay++;
            Debug.Log($"¡Truco aceptado! Ahora se juega por {pointsInPlay} puntos.");
        }
        else
        {
            // Rechazaron el Truco
            ulong surrenderSenderId = rpc.Receive.SenderClientId;
            DeclareTeamWinnerBySurrender(surrenderSenderId);
        }
        // Resetear valores
        waitingTrucoConfirmation = false;
        //? AVISAR A LOS CLIENTES QUE SE ACEPTO O RECHAZO EL TRUCO
        TrucoConfirmationClientRpc();
    }
    [Rpc(SendTo.Everyone)]
    private void TrucoConfirmationClientRpc()
    {
        OnWaitingTrucoConfirmation?.Invoke(this, new OnWaitingConfirmationArgs
        {
            isStageEnded = true
        });
    }
    //* --- CANTAR TRUCO --- 
    public void Truco()
    {
        TrucoServerRpc();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TrucoServerRpc(RpcParams rpc = default)
    {
        if (roundFinished.Value || waitingTrucoConfirmation || trucoStage == TrucoStage.Vale4)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR TRUCO, round finished: " + roundFinished.Value + ", waitingconfirmation: " + waitingTrucoConfirmation + ", trucoStage: " + trucoStage);
            return;
        }
        ulong sender = rpc.Receive.SenderClientId;
        int senderSeat = GetSeatIndexFromClientId(sender);
        int callerTeam = Seats[senderSeat].team;

        if (TeamThatCalledTruco == callerTeam)
        {
            Debug.Log("[ERROR] No puedes cantar Truco dos veces seguidas.");
            return;
        }
        TeamThatCalledTruco = callerTeam;
        waitingTrucoConfirmation = true;
        NextTrucoStage();
        List<ulong> targetClients = new List<ulong>();
        foreach (var kvp in Seats)
        {
            if (kvp.Value.team != callerTeam)
            {
                targetClients.Add(kvp.Value.clientId);
            }
        }
        SendTrucoToOpponentClientRpc(callerTeam, trucoStage, GetRpcTargetParams(targetClients.ToArray()));
        StartTrucoConfirmationClientRpc();
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void SendTrucoToOpponentClientRpc(int callerTeam, TrucoStage trucoStage, RpcParams rpcParams = default)
    {
        OnSomeoneCalledTruco?.Invoke(this, new OnTeamTrucoCall { team = callerTeam, trucostage = trucoStage });
    }
    [Rpc(SendTo.Everyone)]
    private void StartTrucoConfirmationClientRpc()
    {
        OnWaitingTrucoConfirmation?.Invoke(this, new OnWaitingConfirmationArgs
        {
            isStageEnded = false
        });
    }

    //* ENVIDO

    public void Envido(EnvidoStage call)
    {
        EnvidoServerRpc(call);
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnvidoServerRpc(EnvidoStage call, RpcParams rpc = default)
    {
        if (roundFinished.Value || waitingEnvidoConfirmation || envidoStage == EnvidoStage.FaltaEnvido)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR Envido, round finished: " + roundFinished.Value + ", waitingconfirmation: " + waitingEnvidoConfirmation + ", envidoStage: " + envidoStage);
            return;
        }
        if (envidoStage == EnvidoStage.RealEnvido || envidoStage == EnvidoStage.FaltaEnvido && call == EnvidoStage.Envido)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR ENVIDO SI LA APUESTA YA ESTA SUBIDA. stage: " + envidoStage + " y call: " + call);
            return;
        }

        //? Tomo el receptor del call
        ulong sender = rpc.Receive.SenderClientId;
        int senderSeat = GetSeatIndexFromClientId(sender);
        bool isLast = senderSeat == LastSeats[0] || senderSeat == LastSeats[1];
        if (!isLast)
        {
            Debug.LogWarning("SOLOS LOS ASIENTOS: " + LastSeats[0] + " y " + LastSeats[1] + " pueden cantar envido. vos sos " + senderSeat);
            return;
        }
        EnvidoPointsInPlay++;
        int callerTeam = Seats[senderSeat].team;
        waitingEnvidoConfirmation = true;
        TeamThatCalledEnvido = callerTeam;
        NextEnvidoStage(call);
        List<ulong> targetClients = new();
        foreach (var kvp in Seats)
        {
            if (kvp.Value.team != callerTeam)
            {
                targetClients.Add(kvp.Value.clientId);
            }
        }
        SendEnvidoToOpponentClientRpc(callerTeam, envidoStage, GetRpcTargetParams(targetClients.ToArray()));
        StartEnvidoStageClientRpc();
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void SendEnvidoToOpponentClientRpc(int callerTeam, EnvidoStage envidoStage, RpcParams RpcParams = default)
    {
        OnSomeoneCalledEnvido?.Invoke(this, new OnTeamEnvidoCall { team = callerTeam, envidoStage = envidoStage });
    }
    [Rpc(SendTo.Everyone)]
    private void StartEnvidoStageClientRpc()
    {
        OnWaitingEnvidoConfirmation?.Invoke(this,new OnWaitingConfirmationArgs
        {
            isStageEnded= false,
        });
    }
    //* Se sube la apuesta
    public void RaiseEnvido(EnvidoStage call) { RaiseEnvidoServerRpc(call); }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RaiseEnvidoServerRpc(EnvidoStage call)
    {
        waitingEnvidoConfirmation = false;
        EnvidoServerRpc(call);
    }
    //* CONFIRMACION DE ENVIDO */
    public void EnvidoConfirmation(bool Isaccepted)
    {
        EnvidoConfirmationServerRpc(Isaccepted);
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnvidoConfirmationServerRpc(bool accepted, RpcParams rpc = default)
    {
        ulong sender = rpc.Receive.SenderClientId;
        int senderSeat = GetSeatIndexFromClientId(sender);
        int senderTeam = Seats[senderSeat].team;
        if (accepted)
        {
            int winnerSeat = -1;
            int maxEnvidoValue = -1;
            AddEnvidoPointsByStage();
            foreach (var pd in envidoValue_Seats)
            {
                if (pd.Value > maxEnvidoValue)
                {
                    maxEnvidoValue = pd.Value;
                    winnerSeat = pd.Key;
                }
            }
            AddEnvidoPointsToWinner(Seats[winnerSeat].team);
        }
        else
        {
            if (senderTeam == 1) AddEnvidoPointsToWinner(2); //? Gana el equipo contrario.
            else AddEnvidoPointsToWinner(1);
        }
        waitingEnvidoConfirmation = false;
        EnvidoConfirmationClientRpc(accepted);
    }
    private void AddEnvidoPointsByStage()
    {
        switch (envidoStage)
        {
            case EnvidoStage.Envido:
                EnvidoPointsInPlay++;
                break;
            case EnvidoStage.RealEnvido:
                EnvidoPointsInPlay += 2;
                break;
            case EnvidoStage.FaltaEnvido:
                break;
            default:
                break;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void EnvidoConfirmationClientRpc(bool accepted)
    {
        //Debug.Log("ENVIDO ACEPTADO " + accepted);
        OnWaitingEnvidoConfirmation?.Invoke(this, new OnWaitingConfirmationArgs
        {
            isStageEnded = true
        });
    }

    //* CALCULAR PUNTOS DE ENVIDO */
    private void AddEnvidoPointsToWinner(int winnerTeam)
    {
        if (envidoStage == EnvidoStage.FaltaEnvido)
        {
            if (winnerTeam == 1) Team1Points.Value += (15-(Team2Points.Value % 15));
            else Team2Points.Value += (15-(Team1Points.Value % 15));
            Debug.Log("-     FALTA ENVIDO       -");
            Debug.Log("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
        }
        else
        {
            if (winnerTeam == 1) Team1Points.Value += EnvidoPointsInPlay;
            else Team2Points.Value += EnvidoPointsInPlay;
        }
        Debug.Log("SE AGREGAN " + EnvidoPointsInPlay + " por " + envidoStage + " AL EQUIPO " + winnerTeam);
    }


    //* --- RESOLUCION DE MANO --- */
    private void CalculatePoints(int winnerTeam)
    {
        if (!currentHand.IsHandClosed())
        {
            Debug.LogError("NO SE PUEDE CALCULAR PUNTOS SI LA MANO NO ESTA CERRADA");
            return;
        }
        if (winnerTeam == -1)
        {
            //* TERMINO LA MANO Y EMPATARON LAS 3 RONDAS, NO SE SUMA PUNTOS.
            Debug.Log("--- EMPATE TOTAL ---");
        }
        else
        {
            if (winnerTeam == 1) Team1Points.Value += pointsInPlay;
            else Team2Points.Value += pointsInPlay;
        }
        if (GetTeam1TotalPoints() >= pointsToWin)
        {
            FinishGameClientRpc(1);
            return;
        }
        if (GetTeam2TotalPoints() >= pointsToWin)
        {
            FinishGameClientRpc(2);
            return;
        }
    }
    [Rpc(SendTo.Everyone)]
    private void FinishGameClientRpc(int win)
    {
        OnGameFinished?.Invoke(this, new OnGameFinishedArgs
        {
            winnerTeam = win
        });
    }


    //* --- BOTON DE IRSE AL MAZO PRESIONADO --- *//
    public void Surrender()
    {
        SurrenderServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SurrenderServerRpc(RpcParams rpc = default)
    {
        Debug.Log($"El equipo rival Se rindio. El equipo gana {pointsInPlay} puntos.");
        ulong surrenderSenderId = rpc.Receive.SenderClientId;
        DeclareTeamWinnerBySurrender(surrenderSenderId);
    }
    private void DeclareTeamWinnerBySurrender(ulong surrenderSenderId)
    {
        roundFinished.Value = true;
        currentHand.CloseHand();
        int SurrenderSeat = GetSeatIndexFromClientId(surrenderSenderId);
        if (Seats[SurrenderSeat].team == 1) CalculatePoints(2);    //? Se rindio equipo 1
        else CalculatePoints(1);                                //? Se rindio equipo 2
        RestartDefaultValues();
    }










    //* --- FUNCIONES DE UTILIDAD --- */
    private RpcParams GetRpcTargetParams(params ulong[] targetIds)
{
    RpcParams rpcParams = default;

    if (targetIds.Length == 1)
    {
        // CASO 1: Para mandarle las cartas a UN solo jugador
        // Se usa RpcTarget.Single
        rpcParams.Send.Target = RpcTarget.Single(targetIds[0], RpcTargetUse.Temp);
    }
    else
    {
        // CASO 2: Para mandarle el "Truco" al equipo contrario (VARIOS IDs)
        // Se usa RpcTarget.Group
        rpcParams.Send.Target = RpcTarget.Group(targetIds, RpcTargetUse.Temp);
    }

    return rpcParams;
}
    private void RestartDefaultValues()
    {
        waitingTrucoConfirmation = false;
        waitingEnvidoConfirmation = false;
        trucoStage = TrucoStage.None;
        envidoStage = EnvidoStage.None;
        TeamThatCalledEnvido = -1;
        TeamThatCalledTruco = -1;
        pointsInPlay = 1;
        EnvidoPointsInPlay = 0;
    }

    public int GetLocalPlayerTeam()
    {
        int seatIndex = GetSeatIndexFromClientId(NetworkManager.Singleton.LocalClientId);
        return Seats[seatIndex].team;
    }
    public int GetTotalPlayers()
    {
        return totalPlayers;
    }

    private int GetSeatIndexFromClientId(ulong clientId)
    {
        return clientsId_Seats[clientId];
    }
    public CardSO getCardSOfromCardIndex(int index)
    {
        return deckSO.Cards[index];
    }
    public int getCardIndexFromCardSO(CardSO card)
    {
        return deckSO.Cards.IndexOf(card);
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    public bool isRoundFinished()
    {
        return roundFinished.Value;
    }
    public int GetPointsInPlay()
    {
        return pointsInPlay;
    }
    public bool IsFirstTurn()
    {
        return isFirstTurn;
    }
    public int GetTeam1TotalPoints()
    {
        return Team1Points.Value;
    }
    public int GetTeam2TotalPoints()
    {
        return Team2Points.Value;
    }
    private void NextTrucoStage()
    {
        if (trucoStage == TrucoStage.None) trucoStage = TrucoStage.Truco;
        else if (trucoStage == TrucoStage.Truco) trucoStage = TrucoStage.Retruco;
        else trucoStage = TrucoStage.Vale4;
    }
    private void NextEnvidoStage(EnvidoStage nextStage)
    {
        envidoStage = nextStage;
    }

    //* Metodos para el TurnManager
    public PlayerData GetPlayerData(int seatIndex) => Seats[seatIndex];
    public int[] GetLastSeats() => LastSeats;
    public int GetCurrentRound() => currentHand != null ? currentHand.GetCurrentRoundIndex() : 0;
}
