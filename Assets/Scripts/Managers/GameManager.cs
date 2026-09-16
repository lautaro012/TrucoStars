using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
public class OnSentEnvidoArgs : EventArgs
{
    public int value;
}
public class OnPointsGainedArgs : EventArgs
{
    public int points;
}
public class OnTeamWinnerArgs : EventArgs {
    public int winnerTeam;
}


public class GameManager : NetworkBehaviour
{
    #region 1 Eventos y Estructuras Playerdata, PlayerSnapshots
    //? --- EVENTOS --- //
    public event EventHandler<OnPointsGainedArgs> OnTeam1PointsChanged;
    public event EventHandler<OnPointsGainedArgs> OnTeam2PointsChanged;
    public event EventHandler<OnTeamWinnerArgs> OnRoundWined;
    public event EventHandler<OnTeamWinnerArgs> OnGameFinished;
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
    #endregion

    [Header("Valores asociados a la partida")]
    [SerializeField] private int totalPlayers;

    [Header("Helpers")]
    [SerializeField] private SeatLayoutManager seatLayoutManager;
    [SerializeField] private TablePlayAreaManager tablePlayAreaManager;

    [Header("Deck y Mesa")]
    [SerializeField] private DeckSO deckSO;
    [SerializeField] private Table table;

    [Header("VALORES PARA EL INICIO DEL JUEGO")]
    [SerializeField] private int center = 1;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private int heightY = -90;    

    #region VARIABLES 
    public struct GameContext : INetworkSerializable, IEquatable<GameContext>
    {
        public int trucoCallerTeam;
        public TrucoStage trucoStage;
        public int envidoCallerTeam;
        public EnvidoStage envidoStage;
        public int CurrentRound;
        public int dealerSeatIdx;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref trucoCallerTeam);
            s.SerializeValue(ref trucoStage);
            s.SerializeValue(ref envidoCallerTeam);
            s.SerializeValue(ref envidoStage);
            s.SerializeValue(ref CurrentRound);
            s.SerializeValue(ref dealerSeatIdx);
        }

        public bool Equals(GameContext other)
        {
            return trucoCallerTeam == other.trucoCallerTeam &&
                trucoStage == other.trucoStage &&
                envidoCallerTeam == other.envidoCallerTeam &&
                envidoStage == other.envidoStage &&
                CurrentRound == other.CurrentRound &&
                dealerSeatIdx == other.dealerSeatIdx;
        }
    }

    //? VARIABLES PARA DEFINIR POSICIONES
    private Dictionary<int, PlayerData> Seats;
    private Dictionary<int, SeatController> Seats_index;
    private Dictionary<ulong, int> clientsId_Seats;
    private Dictionary<int, PlaySlotView> playSlots_Seats;
    private int[] LastSeats;
    //? --- NETWORK VARIABLES --- */
    public NetworkVariable<int> currentTurnSeatIndex = new NetworkVariable<int>(-1);
    public NetworkVariable<GameState> currentGameState = new NetworkVariable<GameState>(GameState.None);
    private NetworkVariable<int> Team1Points = new(0);
    private NetworkVariable<int> Team2Points = new(0);
    private int playersReady = 0;
    //? Reglas y contexto de juego
    private GameRulesBase currentRules;
    public NetworkVariable<GameContext> currentContext = new(new GameContext 
    { 
        trucoCallerTeam = -1, 
        envidoCallerTeam = -1,
        trucoStage = TrucoStage.None,
        envidoStage = EnvidoStage.None
    });

    public static GameManager Instance { get; private set; }
    #endregion

    //? REGLAS DEL JUEGO, MOMENTANEAS PARA DEBUG
    private int pointsToWin = 4;
    private bool conFlor = false;
    private bool conPicaPica = false;
    
    #region UNITY FUNCTIONS
    private void Awake()
    {
        Instance = Instance != null ? Instance : this;
        clientsId_Seats = new Dictionary<ulong, int>();
        Seats = new Dictionary<int, PlayerData>();
        Seats_index = new Dictionary<int, SeatController>();
        playSlots_Seats = new Dictionary<int, PlaySlotView>();
        //envidoValue_Seats = new();
        seatLayoutManager.OnSeatCreated += SeatLayoutManager_OnSeatCreated;
        tablePlayAreaManager.OnSlotsLaidOut += TablePlayAreaManager_OnSlotLaidOut;
        LastSeats = new int[2];

    }
    private void Update()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.ConnectedClients.Count == totalPlayers && currentGameState.Value == GameState.None)
        {
            StartGame();
        }
    }
    public override void OnNetworkSpawn()
    {
        Team1Points.OnValueChanged += Team1Points_OnValueChanged;
        Team2Points.OnValueChanged += Team2Points_OnValueChanged;
        currentGameState.OnValueChanged += GameState_OnValueChanged;
        currentRules = new PuntaYHachaRules(this, pointsToWin,true);
    }
    #endregion
    
    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        if (IsServer)
        {
            switch (newValue)
            {
                case GameState.SetupHand:
                    if (previousValue == GameState.None)
                    {
                    }
                    break;
                case GameState.PlayingTurn:
                    if (previousValue == GameState.SetupHand)
                    {
                    }
                    break;

                case GameState.RoundFinished:
                    break;
                case GameState.HandFinished:
                    break;
            }
        }
    }

    //? Empieza el juego, Asigna los asientos, Baraja las cartas y Create lo asienots y los jugadores
    private void StartGame()
    {
        if (!IsServer) return;
        SetCurrentGameState(GameState.SetupHand);
        AssignSeats();
        CreateSeatsAndPlayAreaClientRpc(totalPlayers, Vector3.zero, radius, heightY, 0f);
    }

    #region PUENTES DE CONTEXTOS
    public void Bridge_StartHandVisuals(int[] ruleLastSeats, Dictionary<int, int[]> ruleCardsInHands, Dictionary<int, int> ruleEnvidoValues)
    {
        this.LastSeats = ruleLastSeats;
        ShowDeckClientRpc(ruleLastSeats);

        for (int i = 0; i < totalPlayers; i++)
        {
            int[] cards = ruleCardsInHands[i];
            int envidoPts = ruleEnvidoValues[i];
            ulong clientId = Seats[i].clientId;
            CreateCardsClientRpc(envidoPts, cards, i, GetRpcTargetParams(clientId));
        }
    }
    public void Bridge_MoveCardToTable(int cardIndex, int clientSeat, int cardId)
    {
        MoveCardToTableClientRpc(cardIndex, clientSeat, cardId);
    }

    public void Bridge_AnnounceRoundWinner(int winnerTeam)
    {
        AnnounceRoundWinnerClientRpc(winnerTeam);
    }

    public void Bridge_CalculatePoints(int winnerTeam, int points)
    {
        if (winnerTeam == 1) Team1Points.Value += points;
        else if (winnerTeam == 2) Team2Points.Value += points;
    }
    public void Bridge_FinishGame(int winnerTeam)
    {
        SetCurrentGameState(GameState.HandFinished);
        FinishGameClientRpc(winnerTeam);
    }
    #endregion
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
        currentRules = new PuntaYHachaRules(this, pointsToWin,true);
        currentRules.StartHand();
        
        DealAnimationClientRpc(LastSeats[0]);
        SetCurrentGameState(GameState.PlayingTurn);
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


    [Rpc(SendTo.Everyone)]
    private void ShowDeckClientRpc(int[] lastSeats)
    {
        LastSeats = lastSeats;
        PlaySlotView LastplaySlot = playSlots_Seats[lastSeats[0]];
        PlaySlotView previousLastPlaySlot = playSlots_Seats[lastSeats[1]];
        LastplaySlot.LastTurn(true);
        previousLastPlaySlot.LastTurn(false);
    }


    /// <summary>
    /// NUEVA: APAGA LAS CARTAS DE CADA JUGADOR, EMPIEZA LA NUEVA MANO, DISPARA LA ANIMACION DE REPARTIR Y MUESTRA LAS CARTAS
    /// </summary>
    /// <param name="rpc"></param>
    /// [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestStartNextHandServerRpc(RpcParams rpc = default)
    {
        if (currentGameState.Value != GameState.RoundFinished && currentGameState.Value != GameState.HandFinished)
        {
            Debug.LogWarning("NO TERMINO LA RONDA TODAVIA " + currentGameState.Value);
            return;
        }
        
        SetCurrentGameState(GameState.SetupHand);

        for (int i = 0; i < totalPlayers; i++)
        {
             ClearTableAndHideHandsClientRpc(Seats[i].seatIndex);
        }

        currentRules.StartHand();
        DealAnimationClientRpc(LastSeats[0]);
        SetCurrentGameState(GameState.PlayingTurn);
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

    private void Team2Points_OnValueChanged(int previousValue, int newValue)
    {
        OnTeam2PointsChanged?.Invoke(this, new OnPointsGainedArgs { points = newValue });
    }
    private void Team1Points_OnValueChanged(int previousValue, int newValue)
    {
        OnTeam1PointsChanged?.Invoke(this, new OnPointsGainedArgs { points = newValue });
    }

    /// <summary>
    /// Le pide permiso al servidor para jugar una carta. Toma el index de la carta y el clientID del jugador por Params
    /// </summary>
    /// <param name="cardParentIndex"></param>
    /// <param name="rpc"></param>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ClickOnCardServerRpc(int cardParentIndex, RpcParams rpc = default) 
    {
        switch (currentGameState.Value)
            {
                case GameState.WaitingEnvido:
                case GameState.WaitingTruco:
                    Debug.LogWarning("NO SE PUEDE CLICKEAR CARTAS: Hay un canto pendiente.");
                    return;
                    
                case GameState.None:
                case GameState.SetupHand:
                case GameState.RoundFinished:
                case GameState.HandFinished:
                    Debug.LogWarning("NO SE PUEDE CLICKEAR CARTAS: No estamos en fase de juego.");
                    return;
                    
                case GameState.PlayingTurn:
                    // Todo bien, dejamos que el código siga hacia abajo
                    break; 
            }
        ResolveClickedCard(cardParentIndex, rpc);
    }

    //? FUNCION QUE RESUELVE SI LA CARTA PUEDE SER JUGADA POR EL JUGADOR. Y HACE LO CHEQUEOS DE RONDAS
    private void ResolveClickedCard(int cardParentIndex, RpcParams rpc = default)
    {

        if (currentGameState.Value == GameState.WaitingEnvido || currentGameState.Value == GameState.WaitingTruco )
        {
            Debug.LogWarning("No puedes jugar mientras hay confirmación pendiente.");
            return;
        }

        ulong sender = rpc.Receive.SenderClientId;
        int clientSeat = GetSeatIndexFromClientId(sender);

        if (clientSeat < 0) return; //? ClientSeat error
        if (currentTurnSeatIndex.Value != clientSeat)
        {
            Debug.LogWarning("No es tu turno. Turno: " + currentTurnSeatIndex.Value + " y asiento actual: " + clientSeat);
            return;
        }
        if (cardParentIndex < 0 || cardParentIndex >= Seats[clientSeat].cardsInHands.Length) return; //? ParentCardId Error
        
        int cardId = currentRules.GetCardId(clientSeat, cardParentIndex); 
        if (cardId < 0)
        {
            Debug.LogWarning("Carta inválida o ya jugada en ese slot.");
            return;
        }
        currentRules.ProcessCardPlayed(clientSeat, cardId, cardParentIndex);
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
        if (currentGameState.Value != GameState.WaitingTruco)
        {
            Debug.Log("[ERROR] No puedes confirmar el Truco en este momento.");
            return;
        }
        ulong sender = rpc.Receive.SenderClientId;
        int senderId = GetSeatIndexFromClientId(sender);

        currentRules.ProcessTrucoConfirmation(senderId,accepted);
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
        ulong sender = rpc.Receive.SenderClientId;
        int senderSeat = GetSeatIndexFromClientId(sender);
        int callerTeam = Seats[senderSeat].team;
        if (currentContext.Value.trucoCallerTeam == callerTeam)
        {
            Debug.Log("[ERROR] No puedes cantar Truco dos veces seguidas.");
            return;
        }
        
        currentRules.ProcessTrucoCall(senderSeat,callerTeam);
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
        if (currentGameState.Value != GameState.PlayingTurn && currentGameState.Value != GameState.WaitingEnvido)
        {
            Debug.LogWarning("NO SE PUEDE CANTAR Envido, estado actual: " + currentGameState.Value);
            return;
        }

        ulong sender = rpc.Receive.SenderClientId;
        int senderSeat = GetSeatIndexFromClientId(sender);
        int callerTeam = Seats[senderSeat].team;

        currentRules.ProcessEnvidoCall(call,senderSeat,callerTeam);
    }

    /// <summary>
    /// CONFIRMA SI EL EQUIPO AL QUE SE LE CANTO ENVIDO ACEPTA O NO ACEPTA EL ENVIDO
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
        int responderSeat = GetSeatIndexFromClientId(sender);

        currentRules.ProcessEnvidoConfirmation(responderSeat, accepted);
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
        ulong sender = rpc.Receive.SenderClientId;
        int surrenderSeat = GetSeatIndexFromClientId(sender);
        
        currentRules.ProcessSurrender(surrenderSeat);
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









    //* HANDLERS  
    private void SeatLayoutManager_OnSeatCreated(object sender, SeatCreatedEventArgs e)
    {
        Seats_index[e.SeatIndex] = e.Seat;
    }
    private void TablePlayAreaManager_OnSlotLaidOut(object sender, OnSlotsLaidOutArgs e)
    {
        playSlots_Seats = e.PlayAreaBySeatIndex;
    }
    private RpcParams GetRpcTargetParams(params ulong[] targetIds)
    {
        RpcParams rpcParams = default;
        if (targetIds.Length == 1)
        {
            rpcParams.Send.Target = RpcTarget.Single(targetIds[0], RpcTargetUse.Temp);
        }
        else
        {
            rpcParams.Send.Target = RpcTarget.Group(targetIds, RpcTargetUse.Temp);
        }
        return rpcParams;
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
        return currentGameState.Value == GameState.RoundFinished;
    }
    public int GetTeam1TotalPoints()
    {
        return Team1Points.Value;
    }
    public int GetTeam2TotalPoints()
    {
        return Team2Points.Value;
    }
    public void SetCurrentGameState(GameState gs) { currentGameState.Value = gs; }
    public PlayerData GetPlayerData(int seatIndex) => Seats[seatIndex];
    public int[] GetLastSeats() => LastSeats;
}
