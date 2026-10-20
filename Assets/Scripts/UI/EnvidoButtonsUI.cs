using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnvidoButtonsUI : MonoBehaviour
{
    [Header("Botones Directos de Envido")]
    [SerializeField] private Button EnvidoButton;
    [SerializeField] private Button RealButton;
    [SerializeField] private Button FaltaButton;
    
    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI pointsText;

    private void Awake()
    {
        EnvidoButton.onClick.AddListener(() => GameManager.Instance.Envido(EnvidoStage.Envido));
        RealButton.onClick.AddListener(() => GameManager.Instance.Envido(EnvidoStage.RealEnvido));
        FaltaButton.onClick.AddListener(() => GameManager.Instance.Envido(EnvidoStage.FaltaEnvido));
    }

    public void SetPointsText(int points)
    {
        pointsText.text = "Envido [" + points.ToString() + "]";
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}