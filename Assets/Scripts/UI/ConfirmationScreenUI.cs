using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationScreenUI : MonoBehaviour
{
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI confirmationText;


    private void Awake(){
        upgradeButton.onClick.AddListener(() =>
        {
            RaiseBet();
            Hide();
        });
        confirmButton.onClick.AddListener(() => {
            CallbackSelector(true);
            Hide();
        });
        cancelButton.onClick.AddListener(() => {
            GameManager.Instance.Surrender();
            Hide();
        });
    }
    private void Start() {
        GameClientManager.Instance.TrucoEvent += GCM_TrucoEvent;
        GameManager.Instance.OnRoundFinished += GameManager_OnRoundFinished;
        GameManager.Instance.OnWaitingTrucoConfirmation += GameManager_OnWaitingTrucoConfirmation;
        Hide();
    }

    private void GameManager_OnWaitingTrucoConfirmation(object sender, OnWaitingConfirmationArgs e)
    {
        if (e.isStageEnded)
        {
            Hide();
        }
    }


    private void GameManager_OnRoundFinished(object sender, EventArgs e)
    {
        Hide();
    }

    private void GCM_TrucoEvent(object sender, TrucoEventArgs e)
    {
        confirmationText.text = e.mainText;
        upgradeText.text = e.upgradeText;
        upgradeButton.gameObject.SetActive(e.hideUpgrade);
        Show();
    }
 
    private void CallbackSelector(bool response) {
        GameManager.Instance.TrucoConfirmation(response);
    }
    private void RaiseBet()
    {
        GameManager.Instance.TrucoConfirmation(true);
        GameManager.Instance.Truco();
    }
    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}
