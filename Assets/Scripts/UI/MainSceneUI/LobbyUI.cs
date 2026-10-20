using Unity.Netcode;
using Unity.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public struct PlayerLobbyData : INetworkSerializable, IEquatable<PlayerLobbyData>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName; 
    public int TeamIndex; 
    public int SeatIndex; 

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref TeamIndex);
        serializer.SerializeValue(ref SeatIndex);
    }

    public bool Equals(PlayerLobbyData other) => ClientId == other.ClientId && TeamIndex == other.TeamIndex && SeatIndex == other.SeatIndex;
}

public class LobbyUI : NetworkBehaviour
{
    [Header("UI de Asientos")]
    [SerializeField] private Transform team1Column;
    [SerializeField] private Transform team2Column;
    [SerializeField] private LobbySeatUI seatPrefab;

    [Header("Navegación")]
    [SerializeField] private Button exitButton;

    [Header("Configuraciones del Host")]
    [SerializeField] private OptionSelectorUI pointsSelector;
    [SerializeField] private OptionSelectorUI timeSelector;
    [SerializeField] private Toggle florToggle;
    [SerializeField] private Toggle interactionsToggle;
    [SerializeField] private Toggle pyhToggle;
    [SerializeField] private Toggle pyhSumatoriaToggle;
    [SerializeField] private Button startGameButton;

    private List<LobbySeatUI> spawnedSeats = new List<LobbySeatUI>();    
    public NetworkList<PlayerLobbyData> LobbyPlayers;
    public NetworkVariable<int> NetTotalPlayers = new NetworkVariable<int>(2);

    private void Awake()
    {
        LobbyPlayers = new NetworkList<PlayerLobbyData>();
        ConfigurarPanelHost();
    }

    private void ConfigurarPanelHost()
    {
        pointsSelector.Setup("A Jugar", new string[] { "15 Puntos", "30 Puntos", "Infinito" }, 0);
        pointsSelector.OnValueChanged += (index) => { if(IsServer) HostSettings.Instance.totalPoints = (index == 0) ? 15 : ((index == 1) ? 30 : 999); };
        
        timeSelector.Setup("Tiempo", new string[] { "15 Segundos", "30 Segundos", "Sin Límite" }, 1);
        timeSelector.OnValueChanged += (index) => { if(IsServer) HostSettings.Instance.timeToPlay = (index == 0) ? 15f : ((index == 1) ? 30f : -1f); };
        
        florToggle.onValueChanged.AddListener((isOn) => { if(IsServer) HostSettings.Instance.flor = isOn; });
        interactionsToggle.onValueChanged.AddListener((isOn) => { if(IsServer) HostSettings.Instance.interactions = isOn; });
        pyhSumatoriaToggle.onValueChanged.AddListener((isOn) => { if(IsServer) HostSettings.Instance.PyH_Sumatoria = isOn; });

        pyhToggle.onValueChanged.AddListener((isOn) => 
        { 
            if(IsServer) { HostSettings.Instance.puntaYHacha = isOn; UpdateDynamicUI(); } 
        });

        exitButton.onClick.AddListener(OnExitClicked);
        startGameButton.onClick.AddListener(() => { if (IsServer) StartGame(); });
    }


    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        int currentPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        int expectedPlayers = HostSettings.Instance.totalPlayers;

        if (currentPlayers == expectedPlayers)
        {
            Debug.Log("Sala llena. Iniciando partida...");
            
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single   );
        }
        else
        {
            Debug.LogWarning($"Faltan jugadores para empezar. Hay {currentPlayers} de {expectedPlayers}.");
        }
    }

    void Start() => Hide();
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void OnExitClicked()
    {
        if (IsServer)
        {
            Debug.Log("El Host cerró el lobby.");
            LobbyPlayers.Clear(); 
        }
        
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetTotalPlayers.Value = HostSettings.Instance.totalPlayers;
            
            AddPlayerToLobby(NetworkManager.Singleton.LocalClientId, "Host");
            
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
        else
        {
            BloquearUICliente();
        }

        UpdateDynamicUI();

        if (spawnedSeats.Count == 0)
        {
            GenerarAsientosVisuales(NetTotalPlayers.Value);
        }

        LobbyPlayers.OnListChanged += (changeEvent) => RefreshSeatsUI();
        RefreshSeatsUI();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        foreach (var seat in spawnedSeats) if (seat != null) Destroy(seat.gameObject);
        spawnedSeats.Clear();
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        foreach (var p in LobbyPlayers) if (p.ClientId == clientId) return;

        AddPlayerToLobby(clientId, $"Jugador {clientId}");
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < LobbyPlayers.Count; i++)
        {
            if (LobbyPlayers[i].ClientId == clientId)
            {
                LobbyPlayers.RemoveAt(i);
                break;
            }
        }
    }

    private void AddPlayerToLobby(ulong clientId, string playerName)
    {
        int asientosPorEquipo = NetTotalPlayers.Value / 2;
        
        for (int t = 1; t <= 2; t++)
        {
            for (int s = 0; s < asientosPorEquipo; s++)
            {
                bool ocupado = false;
                foreach (var p in LobbyPlayers) if (p.TeamIndex == t && p.SeatIndex == s) ocupado = true;
                
                if (!ocupado)
                {
                    LobbyPlayers.Add(new PlayerLobbyData { ClientId = clientId, PlayerName = playerName, TeamIndex = t, SeatIndex = s });
                    return; 
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestChangeSeatServerRpc(int targetTeam, int targetSeat, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        foreach (var player in LobbyPlayers) 
        {
            if (player.TeamIndex == targetTeam && player.SeatIndex == targetSeat) return; 
        }

        for (int i = 0; i < LobbyPlayers.Count; i++)
        {
            if (LobbyPlayers[i].ClientId == senderId)
            {
                var playerData = LobbyPlayers[i];
                playerData.TeamIndex = targetTeam;
                playerData.SeatIndex = targetSeat;
                LobbyPlayers[i] = playerData; 
                break;
            }
        }
    }

    private void RefreshSeatsUI()
    {
        foreach (var seat in spawnedSeats) seat.SetEmpty();

        foreach (var player in LobbyPlayers)
        {
            var targetSeat = spawnedSeats.Find(s => s.TeamIndex == player.TeamIndex && s.SeatIndex == player.SeatIndex);
            if (targetSeat != null)
            {
                bool isMe = player.ClientId == NetworkManager.Singleton.LocalClientId;
                targetSeat.SetOccupied(player.PlayerName.ToString(), isMe);
            }
        }
    }

    private void BloquearUICliente()
    {
        pointsSelector.SetInteractable(false);
        timeSelector.SetInteractable(false);
        florToggle.interactable = false;
        interactionsToggle.interactable = false;
        pyhToggle.interactable = false;
        pyhSumatoriaToggle.interactable = false;
        startGameButton.gameObject.SetActive(false);
    }

    private void UpdateDynamicUI()
    {
        if (IsServer) 
        {
            bool is6Players = HostSettings.Instance.totalPlayers == 6;
            pyhToggle.gameObject.SetActive(is6Players);
            pyhSumatoriaToggle.gameObject.SetActive(is6Players && HostSettings.Instance.puntaYHacha);

            if (!is6Players) 
            {
                HostSettings.Instance.puntaYHacha = false;
                pyhToggle.isOn = false;
            }
        }
    }

    private void GenerarAsientosVisuales(int cantidadJugadores)
    {
        int asientosPorEquipo = cantidadJugadores / 2;
        for (int i = 0; i < asientosPorEquipo; i++) CreateSeatPrefab(team1Column, 1, i);
        for (int i = 0; i < asientosPorEquipo; i++) CreateSeatPrefab(team2Column, 2, i);
    }

    private void CreateSeatPrefab(Transform parentColumn, int teamIndex, int seatIndex)
    {
        GameObject seatGO = Instantiate(seatPrefab.gameObject, parentColumn);
        LobbySeatUI seatUI = seatGO.GetComponent<LobbySeatUI>();
        seatUI.Setup(teamIndex, seatIndex);
        
        // Siempre le pedimos permiso al servidor
        seatUI.OnSeatClicked += (team, seat) => RequestChangeSeatServerRpc(team, seat);
        
        spawnedSeats.Add(seatUI);
    }
}