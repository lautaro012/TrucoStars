using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class OnPlayerCalledArgs : EventArgs
{
    public int seatIndex;
    public string callText;
}

public class OnWaitingConfirmationArgs : EventArgs
{
    public bool isStageEnded;
}
public class OnSentEnvidoArgs : EventArgs
{
    public int value;
}
public class OnPointsGainedArgs : EventArgs
{
    public int points;
}
public class OnRoundFinishedArgs : EventArgs {
    public bool shuffleDeck;   
    public int team1PointsGained; 
    public int team2PointsGained; 
    public int totalPointsInPlay; // La suma de todo lo que se apostó
}
public class OnEnvidoWinnerArgs : EventArgs
{
    public int winningTeam;
    public int pointsWon;
    public int winningScore; 
}
public class OnTrucoAcceptedArgs : EventArgs
{
    public TrucoStage currentStage; // Para saber si es Truco, Retruco, etc.
    public int pointsAtStake; // Puntos en juego (2, 3 o 4)
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
public class OnTeamWinnerArgs : EventArgs {
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
    [Header("Valores asociados a la partida")]
    [SerializeField] private int totalPlayers;

    [Header("Helpers")]
    [SerializeField] private TestLobbyUIMainScene testLobby;
    [SerializeField] private SeatLayoutManager seatLayoutManager;
    [SerializeField] private TablePlayAreaManager tablePlayAreaManager;
    [SerializeField] private ScoringLogic scoringLogic = new ScoringLogic();
    
    [Header("Deck y Mesa")]
    [SerializeField] private DeckSO deckSO;
    [SerializeField] private Table table;

    [Header("VALORES PARA EL INICIO DEL JUEGO")]
    [SerializeField] private int center = 1;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private int heightY = -90;

    //? REGLAS DEL JUEGO, MOMENTANEAS PARA DEBUG
    private int pointsToWin = 15;
    private bool conFlor = false;
    private bool conPicaPica = false;


    //? Data del jugador, incluye index de Asiento, ClientID, equipo y cartas actuales en mano 
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

    //? VARIABLES PARA DEFINIR POSICIONES
    private Dictionary<int, PlayerData> Seats;
    private Dictionary<int, SeatController> Seats_index;
    private Dictionary<ulong, int> clientsId_Seats;
    private Dictionary<int, PlaySlotView> playSlots_Seats;
    private Dictionary<int, int> envidoValue_Seats;
    private int[] LastSeats;


    //? --- EVENTOS --- //
    public event EventHandler<OnEnvidoWinnerArgs> OnEnvidoWinnerDecided;
    public event EventHandler<OnTrucoAcceptedArgs> OnTrucoAccepted;
    public event EventHandler AreAllPlayersConnected;
    public event EventHandler OnRoundStarted;
    public event EventHandler<OnRoundFinishedArgs> OnRoundFinished;
    public event EventHandler<OnTeamEnvidoCall> OnSomeoneCalledEnvido;
    public event EventHandler<OnTeamTrucoCall> OnSomeoneCalledTruco;
    public event EventHandler<OnWaitingConfirmationArgs> OnWaitingTrucoConfirmation;
    public event EventHandler<OnWaitingConfirmationArgs> OnWaitingEnvidoConfirmation;
    public event EventHandler<OnPointsGainedArgs> OnTeam1PointsChanged;
    public event EventHandler<OnPointsGainedArgs> OnTeam2PointsChanged;
    public event EventHandler<OnTeamWinnerArgs> OnRoundWined;
    public event EventHandler<OnPlayerCalledArgs> OnPlayerMadeCall;
    public event EventHandler<OnTeamWinnerArgs> OnGameFinished;


    //? --- NETWORK VARIABLES --- */

    private NetworkVariable<int> Team1Points = new(0);
    private NetworkVariable<int> Team2Points = new(0);
    public NetworkVariable<bool> roundFinished = new(false);
    private NetworkVariable<RoundState> currentPhase = new(RoundState.None);

    //? VARIABLES DE JUEGO */
    private bool GameStarted = false;
    private bool isFirstTurn = true;
    private int playersReady = 0;

    //? variables para las manos
    private int firstToPlay = 0; // El índice del jugador que reparte/empieza
    private Hand currentHand;
    private int handCount = 0;
    //? VARIABLES DE ENVIDO
    private int TeamThatCalledEnvido = -1;
    private bool waitingEnvidoConfirmation = false;

    //? VARIABLES DE TRUCO
    private int TeamThatCalledTruco = -1;
    private bool waitingTrucoConfirmation = false;

    public static GameManager Instance { get; private set; }


    //? --- UNITY METHODS --- */
    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
        clientsId_Seats = new Dictionary<ulong, int>();
        Seats = new Dictionary<int, PlayerData>();
        Seats_index = new Dictionary<int, SeatController>();
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

        //! ESTAS REGLAS DEBERAN CONSEGUIRSE DESDE EL LOBBY SCENE
        if (IsServer) 
        {
            scoringLogic.ApplyLobbySettings(
                pointsToWin,
                conFlor,
                conPicaPica
            );
        }
        
    }


    //? EVENTOS LOCALES
    private void SeatLayoutManager_OnSeatCreated(object sender, SeatCreatedEventArgs e)
    {
        Seats_index[e.SeatIndex] = e.Seat;
    }
    private void TablePlayAreaManager_OnSlotLaidOut(object sender, OnSlotsLaidOutArgs e)
    {
        playSlots_Seats = e.PlayAreaBySeatIndex;
    }



    //? Empieza el juego, Asigna los asientos, Baraja las cartas y Create lo asienots y los jugadores
    private void StartGame()
    {
        if (!IsServer) return;
        currentPhase.Value = RoundState.RoundStarted;
        roundFinished.Value = false;
        AssignSeats();
        DrawCards();
        CreateSeatsAndPlayAreaClientRpc(totalPlayers, Vector3.zero, radius, heightY, 0f);
    }

    //? ClientRPC que manda a crear a todos los jugadores sus asientos y donde van a jugar las cartas en la mesa 
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

    //? CREA LOS PLAYER DATA AL INICIAR LA PARTIDA
    private void AssignSeats()
    {
        clientsId_Seats.Clear();
        Seats.Clear();
        for (int i = 0; i < totalPlayers; i++)
        {
            //?CREO LOS PLAYERDATA, Y CREO LOS ASIENTOS ALREDEDOR DE LA MESA
            int seatIndex = i % totalPlayers; //? Asigno asientos de 0 a 3
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
    
    /// <summary>
    /// CALCULA UN ARREGLO DE CARTAS UNICO Y ALEATORIO Y ASIGNA 3 CARTAS A CADA JUGADOR. CALCULANDO SU ENVIDOVALUE EN EL PROCESO
    /// </summary>
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
            int EnvidoValue = scoringLogic.CalculateEnvidoValue(firstCard, secondCard,thirdCard);
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


    /// <summary>
    /// SERVERRPC QUE SE LLAMA CUANDO SE TERMINA DE CREAR EL LAYOUT DEL JUEGO. CUANDO CADA JUGADOR TERMINA SU LAYOUT SE CREAN LAS MANOS
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LayoutReadyServerRpc()
    {
        playersReady++;
        if (playersReady == totalPlayers)
        {
            CreatePlayerHandsServerRpc();
        }
    }

    //? ServerRPC que crea las manos y avisa a cada cliente que cartas les toco
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CreatePlayerHandsServerRpc()
    {
        SetPlayersDataClientRPC(Seats.Values.ToArray());
        StartNewHand();
        for (int i = 0; i < totalPlayers; i++)
        {
            int[] cards = Seats[i].cardsInHands;            
            CreateCardsClientRpc(envidoValue_Seats[i], cards, Seats[i].seatIndex, GetRpcTargetParams(new[] { Seats[i].clientId }));
        }
        DealAnimationClientRpc(LastSeats[0]);
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
            Seats_index[p.seatIndex].SetPlayerData(p.seatIndex, p.clientId, p.team, p.playerId, isMine);
        }
    }

    /// <summary>
    /// ROTA LA MESA PARA QUE CADA JUGADOR VEA SUS CARTAS
    /// </summary>
    /// <param name="clientId"></param>
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
        Seats_index[seatindex].SetCardsIds(cards);
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
        PlaySlotView LastplaySlot = playSlots_Seats[lastSeats[0]];
        PlaySlotView previousLastPlaySlot = playSlots_Seats[lastSeats[1]];
        LastplaySlot.LastTurn(true);
        previousLastPlaySlot.LastTurn(false);
    }


    /// <summary>
    /// NUEVA: APAGA LAS CARTAS DE CADA JUGADOR, EMPIEZA LA NUEVA MANO, DISPARA LA ANIMACION DE REPARTIR Y MUESTRA LAS CARTAS
    /// </summary>
    /// <param name="rpc"></param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestStartNextHandServerRpc(RpcParams rpc = default)
    {
        if (!roundFinished.Value)
        {
            Debug.Log("NO TERMINO LA RONDA TODAVIA " + roundFinished.Value);
            return;
        }

        // 1. Apagamos todas las manos y limpiamos la mesa de todos los jugadores
        for (int i = 0; i < totalPlayers; i++)
        {
             ClearTableAndHideHandsClientRpc(Seats[i].seatIndex);
        }

        StartNewHand();
        roundFinished.Value = false;
        
        // 2. Calculamos las cartas matemáticamente
        DrawCards();

        // 3. Avisamos a los clientes qué cartas les tocaron en memoria
        for (int i = 0; i < totalPlayers; i++)
        {
            int[] cards = Seats[i].cardsInHands;
            CreateCardsClientRpc(envidoValue_Seats[i], cards, Seats[i].seatIndex, GetRpcTargetParams(new[] { Seats[i].clientId }));
        }

        // 4. ¡Disparamos la ilusión visual! (LastSeats[0] es el dealer)
        DealAnimationClientRpc(LastSeats[0]);
    }

    /// <summary>
    /// Limpia las cartas de la mesa y APAGA las manos
    /// </summary>
    /// <param name="seat"></param>
    [Rpc(SendTo.Everyone)]
    private void ClearTableAndHideHandsClientRpc(int seat)
    {
        // LLAMAMOS AL NUEVO METODO PARA APAGAR LAS CARTAS
        Seats_index[seat].ClearHand(); 
        playSlots_Seats[seat].RestartPlaySlot();
    }

    /// <summary>
    /// Ejecuta la animación y LUEGO enciende las manos
    /// </summary>
    /// <param name="dealerSeat"></param>
    [Rpc(SendTo.Everyone)]
    private void DealAnimationClientRpc(int dealerSeat)
    {
        PlaySlotView dealerSlot = playSlots_Seats[dealerSeat];
        
        // RECOLECTAMOS LOS ANCHORS DE LA MESA
        Transform[] anchors = new Transform[totalPlayers];
        for(int i=0; i < totalPlayers; i++)
        {
            anchors[i] = playSlots_Seats[i].GetShuffleCardAnchor(); 
        }

        tablePlayAreaManager.StartCoroutine(tablePlayAreaManager.DealCardsAnimation(dealerSlot, anchors, () => 
        {
            // AL TERMINAR, PRENDEMOS LAS CARTAS
            foreach (var seat in Seats_index.Values)
            {
                seat.ShowCardsInHand(); 
            }
        }));
    }
    //? --- FUNCIONES DE CAMBIOS DE VALORES --- */



    //* ----- FUNCIONES QUE SE LLAMAN CUANDO NETWORK VARIABLES CAMBIAN -----
    private void RoundFinished_OnValueChanged(bool previousValue, bool newValue)
    {
        if (!IsServer) return;

        if (newValue)
        {
            int dealerSeat = firstToPlay;
            Debug.Log("first to play: " + firstToPlay);
        }
        else
        {
            RoundStartedCallClientRpc();
        }
    }


    [Rpc(SendTo.Everyone)]
    private void RoundStartedCallClientRpc()
    {
        OnRoundStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Team2Points_OnValueChanged(int previousValue, int newValue)
    {
        OnTeam2PointsChanged?.Invoke(this, new OnPointsGainedArgs { points = newValue });
    }
    private void Team1Points_OnValueChanged(int previousValue, int newValue)
    {
        OnTeam1PointsChanged?.Invoke(this, new OnPointsGainedArgs { points = newValue });
    }
    private void RoundState_OnValueChanged(RoundState previousValue, RoundState newValue)
    {
        if (previousValue == RoundState.None)
        {
            AreAllPlayersConnected?.Invoke(this, EventArgs.Empty);
        }
    }





    /// <summary>
    /// Le pide permiso al servidor para jugar una carta. Toma el index de la carta y el clientID del jugador por Params
    /// </summary>
    /// <param name="cardParentIndex"></param>
    /// <param name="rpc"></param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ClickOnCardServerRpc(int cardParentIndex, RpcParams rpc = default) 
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
        ResolveClickedCard(cardParentIndex, rpc);
        CardInPlayClientRpc();
    }


    [Rpc(SendTo.Everyone)]
    private void CardInPlayClientRpc()
    {
        //! FUNCION PARA ANUNCIAR CARTA EN JUEGO
    }

    //? FUNCION QUE RESUELVE SI LA CARTA PUEDE SER JUGADA POR EL JUGADOR. Y HACE LO CHEQUEOS DE RONDAS
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



        //?  --- GUARDADO DE LA CARTA Y CAMBIOS DE UI ---

        Round round = currentHand.CurrentRound;
        bool cardPlayedCorrectly = round.TryPlay(clientSeat, cardId); //? Se intenta jugar la carta en la ronda
        if (!cardPlayedCorrectly)
        {
            Debug.LogError("------ERROR JUGANDO CARTA EN RONDA " + currentHand.GetCurrentRoundIndex() + " --------");
            return;
        }
        MoveCardToTableClientRpc(cardParentIndex, clientSeat, cardId); //? MODIFICAR UI
        Seats[clientSeat].cardsInHands[cardParentIndex] = -1; //? SE QUITA LA CARTA JUGADA

        //? --- CHEQUEO: LA RONDA NO TERMINO? ? PASO TURNO : RESUELVO LA RONDA
        if (!round.IsComplete())
        {
            TurnManager.Instance.AdvanceTurn(-1);
            return;
        }

        //?----- JUGARON TODOS LOS ASIENTOS -------    
        //Debug.Log("-------------------------------------------------------------------");
        //Debug.Log("--- SE JUGARON TODAS LAS CARTAS DE LA RONDA. CALCULANDO GANADOR DE LA RONDA " + currentHand.GetCurrentRoundIndex() + " ---");

        //?----- JUGARON TODOS LOS ASIENTOS -------    

        // Le pasamos las cartas, y dos funciones (Lambdas) que obtienen el valor y el equipo.
        int winnerSeat = scoringLogic.ResolveRound(
            round.SnapshotCards(),
            (cardId) => getCardSOfromCardIndex(cardId).cardValue, 
            (seatIndex) => Seats[seatIndex].team                      
        );

        int winnerTeam = (winnerSeat == -1) ? -1 : Seats[winnerSeat].team;

        currentHand.RegisterRoundWinner(winnerSeat, winnerTeam); //? SE GUARDAN Y ANUNCIAN GANADORES DE LA RONDA ACTUAL Y CIERRA LA MANO
        AnnounceRoundWinnerClientRpc(winnerTeam);

        //? CHEQUEO: LA MANO NO CERRO ? ANUNCIO NUEVA RONDA : ANUNCIO GANADOR
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

    [Rpc(SendTo.Everyone)]
    private void AnnounceRoundWinnerClientRpc(int winnerTeam)
    {
        OnRoundWined?.Invoke(this,new OnTeamWinnerArgs{ winnerTeam = winnerTeam });
    }

    //? Le avisa a los clientes que muevan la carta "cardIndex" del "clientSeat" y les avisa que carta era
    [Rpc(SendTo.Everyone)]
    private void MoveCardToTableClientRpc(int cardIndex, int clientSeat, int cardId)
    {
        PlaySlotView playSlot = playSlots_Seats[clientSeat];
        SeatController Seat = Seats_index[clientSeat];
        Transform origin = Seat.HideCardAndGetOrigin(cardIndex);
        playSlot.PlayThisCard(cardId, origin);
    }


    private void AnnounceNextRound(int nextLeader)
    {
        TurnManager.Instance.NextRoundLeaderSeatIndex = nextLeader;

        StartNextRound(nextLeader);
        TurnManager.Instance.AdvanceTurn(nextLeader);
    }
    private void StartNextRound(int nextLeader)
    {
        int nextIndex = currentHand.NextCurrentRoundIndex();  //? AVANZA DE RONDA Y CIERRA LA MANO SI ESTAMOS EN LA ULTIMA
        currentHand.Rounds.Add(new Round(
            roundIndex: nextIndex,
            leaderSeat: nextLeader,
            totalPlayers: totalPlayers
        ));
        // NextRoundStartedClientRpc()
    }
    [Rpc(SendTo.Everyone)]
    private void NextRoundStartedClientRpc()
    {
        //TODO AGREGAR PUNTOS ROJOS EN EL MARCADOR DE RONDAS 
    }

    /// <summary>
    /// Muestra un globo de texto en un jugador especifico
    /// </summary>
    /// <param name="seatIndex"></param>
    /// <param name="callText"></param>
    [Rpc(SendTo.Everyone)]
    private void AnnounceCallToAllClientRpc(int seatIndex, string callText)
    {
        // Esto lo va a escuchar el SeatController de CADA jugador
        OnPlayerMadeCall?.Invoke(this, new OnPlayerCalledArgs { seatIndex = seatIndex, callText = callText });
    }

    /// <summary>
    /// SE LLAMA CUANDO El EQUIPO CONTRARIO ACEPTA O RECHAZA EL TRUCO.
    /// </summary>
    /// <param name="Isaccepted"></param>
    public void TrucoConfirmation(bool Isaccepted)
    {
        TrucoConfirmationServerRpc(Isaccepted);
    }
   [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TrucoConfirmationServerRpc(bool accepted, RpcParams rpc = default)
    {
        if (waitingTrucoConfirmation == false) return;
        
        if (accepted)
        {
            scoringLogic.TrucoAccepted();
            AnnounceTrucoAcceptedClientRpc(scoringLogic.trucoStage, scoringLogic.GetPointsInPlay());
        }
        else
        {
            ulong surrenderSenderId = rpc.Receive.SenderClientId;
            DeclareTeamWinnerBySurrender(surrenderSenderId);
        }
        
        int senderSeat = GetSeatIndexFromClientId(rpc.Receive.SenderClientId);
        AnnounceCallToAllClientRpc(senderSeat, "¡QUIERO!");
        
        waitingTrucoConfirmation = false;
        TrucoConfirmationClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void AnnounceTrucoAcceptedClientRpc(TrucoStage stage, int points)
    {
        OnTrucoAccepted?.Invoke(this, new OnTrucoAcceptedArgs { currentStage = stage, pointsAtStake = points });
    }

    //? CLIENTRP QUE LE AVISA A LOS JUGADORES QUE LA CONFIRMACION FINALIZO PARA SEGUIR JUGANDO
    [Rpc(SendTo.Everyone)]
    private void TrucoConfirmationClientRpc()
    {
        OnWaitingTrucoConfirmation?.Invoke(this, new OnWaitingConfirmationArgs
        {
            isStageEnded = true
        });
    }

    /// <summary>
    /// Le hace saber al servidor que jugador canto truco e inicia la fase de confirmacion
    /// </summary>
    public void Truco()
    {
        TrucoServerRpc();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TrucoServerRpc(RpcParams rpc = default)
    {
        if (roundFinished.Value || waitingTrucoConfirmation || scoringLogic.trucoStage == TrucoStage.Vale4)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR TRUCO, round finished: " + roundFinished.Value + ", waitingconfirmation: " + waitingTrucoConfirmation + ", trucoStage: " + scoringLogic.trucoStage);
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
        scoringLogic.NextTrucoStage();
        List<ulong> targetClients = new List<ulong>();
        foreach (var kvp in Seats)
        {
            if (kvp.Value.team != callerTeam)
            {
                targetClients.Add(kvp.Value.clientId);
            }
        }
        
        string textCall = scoringLogic.trucoStage == TrucoStage.Truco ? "¡TRUCO!" :
                  scoringLogic.trucoStage == TrucoStage.Retruco ? "¡QUIERO RE-TRUCO!" : "¡QUIERO VALE 4!";
        AnnounceCallToAllClientRpc(senderSeat, textCall);
        
        SendTrucoToOpponentClientRpc(callerTeam, scoringLogic.trucoStage, GetRpcTargetParams(targetClients.ToArray()));
        StartTrucoConfirmationClientRpc();
    }

    //? LE AVISA A LOS OPONENTES QUE LES CANTARON TRUCO
    [Rpc(SendTo.SpecifiedInParams)]
    private void SendTrucoToOpponentClientRpc(int callerTeam, TrucoStage trucoStage, RpcParams rpcParams = default)
    {
        OnSomeoneCalledTruco?.Invoke(this, new OnTeamTrucoCall { team = callerTeam, trucostage = trucoStage });
    }

    //? INICIA LA FASE DE CONFIRMACION DE TRUCO EN GENERAL
    [Rpc(SendTo.Everyone)]
    private void StartTrucoConfirmationClientRpc()
    {
        OnWaitingTrucoConfirmation?.Invoke(this, new OnWaitingConfirmationArgs
        {
            isStageEnded = false
        });
    }

    /// <summary>
    /// Le hace saber al rival que se canto envido. Espera como argumento la fase de envido que se canto
    /// </summary>
    /// <param name="call"></param>
    public void Envido(EnvidoStage call)
    {
        EnvidoServerRpc(call);
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnvidoServerRpc(EnvidoStage call, RpcParams rpc = default)
    {
        if (roundFinished.Value || waitingEnvidoConfirmation || scoringLogic.envidoStage == EnvidoStage.FaltaEnvido)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR Envido, round finished: " + roundFinished.Value + ", waitingconfirmation: " + waitingEnvidoConfirmation + ", envidoStage: " + scoringLogic.envidoStage);
            return;
        }
        if (scoringLogic.envidoStage == EnvidoStage.RealEnvido || scoringLogic.envidoStage == EnvidoStage.FaltaEnvido && call == EnvidoStage.Envido)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR ENVIDO SI LA APUESTA YA ESTA SUBIDA. stage: " + scoringLogic.envidoStage + " y call: " + call);
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
        scoringLogic.EnvidoCalled();
        int callerTeam = Seats[senderSeat].team;
        waitingEnvidoConfirmation = true;
        TeamThatCalledEnvido = callerTeam;
        scoringLogic.NextEnvidoStage(call);
        List<ulong> targetClients = new();
        foreach (var kvp in Seats)
        {
            if (kvp.Value.team != callerTeam)
            {
                targetClients.Add(kvp.Value.clientId);
            }
        }
        
        string textCall = call == EnvidoStage.EnvidoEnvido ? "¡ENVIDO ENVIDO!" :
                  call == EnvidoStage.RealEnvido ? "¡REAL ENVIDO!" :
                  call == EnvidoStage.FaltaEnvido ? "¡FALTA ENVIDO!" : "¡ENVIDO!";
        AnnounceCallToAllClientRpc(senderSeat, textCall);

        SendEnvidoToOpponentClientRpc(callerTeam, scoringLogic.envidoStage, GetRpcTargetParams(targetClients.ToArray()));
        StartEnvidoStageClientRpc();
    }

    //? les avisa a los oponentes que el rival canto envido
    [Rpc(SendTo.SpecifiedInParams)]
    private void SendEnvidoToOpponentClientRpc(int callerTeam, EnvidoStage envidoStage, RpcParams RpcParams = default)
    {
        OnSomeoneCalledEnvido?.Invoke(this, new OnTeamEnvidoCall { team = callerTeam, envidoStage = envidoStage });
    }


    //? EMPIEZA LA FASE DE CONFIRMACION DE ENVIDO
    [Rpc(SendTo.Everyone)]
    private void StartEnvidoStageClientRpc()
    {
        OnWaitingEnvidoConfirmation?.Invoke(this,new OnWaitingConfirmationArgs
        {
            isStageEnded= false,
        });
    }
    /// <summary>
    /// SE LLAMA CUANDO EL EQUIPO SUBE LA APUESTA DEL ENVIDO. RECIBE EL ENVIDOSTAGE LLAMADO, NO CONFUNDIR CON EnvidoConfirmation()
    /// </summary>
    /// <param name="call"></param>
    public void RaiseEnvido(EnvidoStage call) { RaiseEnvidoServerRpc(call); }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RaiseEnvidoServerRpc(EnvidoStage call)
    {
        waitingEnvidoConfirmation = false;
        EnvidoServerRpc(call);
    }
    
    /// <summary>
    /// CONFIRMA SI EL EQUIPO AL QUE SE LE CANTO ENVIDO ACEPTA O NO ACEPTA EL ENVIDO, NO CONFUNDIR CON RaiseEnvido()
    /// </summary>
    /// <param name="Isaccepted"></param>
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
        
        int winnerTeam;
        int pointsWon;
        int winningScore = 0;

        if (accepted)
        {
            int winnerSeat = -1;
            int maxEnvidoValue = -1;
            scoringLogic.AddEnvidoPointsByStage();
            
            foreach (var pd in envidoValue_Seats)
            {
                if (pd.Value > maxEnvidoValue)
                {
                    maxEnvidoValue = pd.Value;
                    winnerSeat = pd.Key;
                }
            }
            winnerTeam = Seats[winnerSeat].team;
            winningScore = maxEnvidoValue; 
            pointsWon = AddEnvidoPointsToWinner(winnerTeam);
        }
        else
        {
            winnerTeam = (senderTeam == 1) ? 2 : 1;
            pointsWon = AddEnvidoPointsToWinner(winnerTeam);
        }

        AnnounceCallToAllClientRpc(senderSeat, "¡QUIERO!");
        
        waitingEnvidoConfirmation = false;
        EnvidoConfirmationClientRpc();
        AnnounceEnvidoWinnerClientRpc(winnerTeam, pointsWon, winningScore);
    }


    [Rpc(SendTo.Everyone)]
    private void EnvidoConfirmationClientRpc()
    {
        //Debug.Log("ENVIDO ACEPTADO " + accepted);
        OnWaitingEnvidoConfirmation?.Invoke(this, new OnWaitingConfirmationArgs
        {
            isStageEnded = true
        });
    }

    //? AGREGA LOS PUNTOS DE ENVIDO AL GANADOR
    private int AddEnvidoPointsToWinner(int winnerTeam)
    {
        int pointsToAdd = 0;
        if (scoringLogic.envidoStage == EnvidoStage.FaltaEnvido)
        {
            pointsToAdd = (winnerTeam == 1) ? 15 - (Team2Points.Value % 15) : 15 - (Team1Points.Value % 15);
        }
        else
        {
            pointsToAdd = scoringLogic.GetEnvidoPointsInPlay();
        }

        if (winnerTeam == 1) Team1Points.Value += pointsToAdd;
        else Team2Points.Value += pointsToAdd;

        return pointsToAdd;
    }
    [Rpc(SendTo.Everyone)]
    private void AnnounceEnvidoWinnerClientRpc(int winnerTeam, int pointsWon, int winningScore)
    {
        OnEnvidoWinnerDecided?.Invoke(this, new OnEnvidoWinnerArgs {
            winningTeam = winnerTeam,
            pointsWon = pointsWon,
            winningScore = winningScore
        });
    }


    //? --- RESOLUCION DE MANO --- */
    private void CalculatePoints(int winnerTeam)
    {
        if (!currentHand.IsHandClosed()) return;

        int pointsGainedT1 = 0;
        int pointsGainedT2 = 0;
        int pointsInPlay = scoringLogic.GetPointsInPlay();

        if (winnerTeam != -1)
        {
            if (winnerTeam == 1) {
                pointsGainedT1 = pointsInPlay;
                Team1Points.Value += pointsInPlay;
            } else {
                pointsGainedT2 = pointsInPlay;
                Team2Points.Value += pointsInPlay;
            }
        }

        CallRoundFinishedEventClientRpc(firstToPlay, pointsGainedT1, pointsGainedT2, pointsInPlay);

        if (GetTeam1TotalPoints() >= pointsToWin) FinishGameClientRpc(1);
        else if (GetTeam2TotalPoints() >= pointsToWin) FinishGameClientRpc(2);
    }
    

    [Rpc(SendTo.Everyone)]
    private void CallRoundFinishedEventClientRpc(int dealerSeat, int t1Gained, int t2Gained, int totalInPlay)
    {
        int mySeat = GameClientManager.Instance.GetLocalSeat();
        bool shuffleDeck = mySeat == dealerSeat;
        OnRoundFinished?.Invoke(this, new OnRoundFinishedArgs { 
            shuffleDeck = shuffleDeck,
            team1PointsGained = t1Gained,
            team2PointsGained = t2Gained,
            totalPointsInPlay = totalInPlay
        });
    }

    [Rpc(SendTo.Everyone)]
    private void FinishGameClientRpc(int win)
    {
        OnGameFinished?.Invoke(this, new OnTeamWinnerArgs
        {
            winnerTeam = win
        });
    }


    /// <summary>
    /// LE AVISA AL SERVIDOR, SEGUN EL JUGADOR QUE LO LLAMA. QUE EQUIPO SE RINDIO
    /// </summary>
    public void Surrender()
    {
        SurrenderServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SurrenderServerRpc(RpcParams rpc = default)
    {
        Debug.Log($"El equipo rival Se rindio. El equipo gana {scoringLogic.GetPointsInPlay()} puntos.");
        ulong surrenderSenderId = rpc.Receive.SenderClientId;
        
        int surrenderSeat = GetSeatIndexFromClientId(rpc.Receive.SenderClientId);
        AnnounceCallToAllClientRpc(surrenderSeat, "Me voy al mazo...");
        
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



    //? FUNCION QUE DEVUELVE LOS PARAMS CON LOS ID A LOS CUALES MANDARLE EL CLIENTRPC 
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

    //? REINICIO DE VALORES DE RONDAS
    private void RestartDefaultValues()
    {
        waitingTrucoConfirmation = false;
        waitingEnvidoConfirmation = false;
        TeamThatCalledEnvido = -1;
        TeamThatCalledTruco = -1;
        scoringLogic.RestartScoringValues();
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

    //? RECIBE LA ROTACION DE LA CABEZA DEL JUGADOR
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SyncHeadRotationServerRpc(int seatIndex, Quaternion headRotation, RpcParams rpc = default)
    {
        SyncHeadRotationClientRpc(seatIndex, headRotation);
    }

    [Rpc(SendTo.Everyone)]
    private void SyncHeadRotationClientRpc(int seatIndex, Quaternion headRotation)
    {
        if (Seats_index.TryGetValue(seatIndex, out var seat))
        {
            if (seat.TryGetComponent<SeatController>(out var controller))
            {
                controller.ReceiveHeadRotation(headRotation);
            }
        }
    }

    //? Metodos para el TurnManager
    public PlayerData GetPlayerData(int seatIndex) => Seats[seatIndex];
    public int[] GetLastSeats() => LastSeats;
    public int GetCurrentRound() => currentHand != null ? currentHand.GetCurrentRoundIndex() : 0;
}
