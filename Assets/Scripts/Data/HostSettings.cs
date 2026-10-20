using UnityEngine;
public class HostSettings : MonoBehaviour
{
    public static HostSettings Instance { get; private set; }

    [Header("Configuración de Partida")]
    public Truco truco = Truco.ARGENTINO;
    public int totalPlayers = 2;
    public int totalPoints = 15;
    public float timeToPlay = 30f;
    public bool flor = false;
    public bool puntaYHacha = false;
    public bool PyH_Sumatoria = false;
    public bool interactions = true;
    public bool isPrivate = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}