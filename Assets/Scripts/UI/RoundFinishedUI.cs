using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundFinishedUI : MonoBehaviour
{
    [SerializeField] private Button SuffleDeck;
    [SerializeField] private TextMeshProUGUI FinishRoundText;

    private void Awake()
    {
        SuffleDeck.onClick.AddListener(() =>
        {
            GameManager.Instance.RequestStartNextHandServerRpc();
        });
    }

    public void Setup(bool shuffleDeck)
    {
        SuffleDeck.gameObject.SetActive(shuffleDeck);
        Show();
    }

    public void Hide() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);
}