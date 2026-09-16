using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundButtonsUI : MonoBehaviour
{
    [SerializeField] private Button Truco;
    [SerializeField] private Button Surrender;
    [SerializeField] private TextMeshProUGUI CantarTrucoText;

    private void Awake()
    {
        Truco.onClick.AddListener(() => GameManager.Instance.Truco());
        Surrender.onClick.AddListener(() => GameManager.Instance.Surrender());
    }
    public void Setup(bool isMyTurn, int localTeam, GameManager.GameContext context)
    {
        bool canCallTruco = false;
        string trucoText = "TRUCO";

        if (context.trucoCallerTeam != localTeam)
        {
            if (context.trucoStage == TrucoStage.None)
            {
                canCallTruco = true;
                trucoText = "TRUCO";
            }
            else if (context.trucoStage == TrucoStage.Truco)
            {
                canCallTruco = true;
                trucoText = "RETRUCO";
            }
            else if (context.trucoStage == TrucoStage.Retruco)
            {
                canCallTruco = true;
                trucoText = "VALE 4";
            }
        }

        Truco.gameObject.SetActive(canCallTruco);
        CantarTrucoText.text = trucoText;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}