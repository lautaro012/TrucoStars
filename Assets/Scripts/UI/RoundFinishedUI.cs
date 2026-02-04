using System;
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
    private void Start()
    {
        Hide();
        GameClientManager.Instance.ShowEndRoundText += GCM_ShowEndRoundText;
        GameClientManager.Instance.ShowMainRoundButtons += GCM_OnRoundStarted;
    }

    private void GCM_OnRoundStarted(object sender, EventArgs e)
    {
        Hide();
    }

    private void GCM_ShowEndRoundText(object sender, OnRoundFinishedArgs e)
    {
        Show();
        if (e.shuffleDeck) { SuffleDeck.gameObject.SetActive(true); }
        else { SuffleDeck.gameObject.SetActive(false); }
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
}
