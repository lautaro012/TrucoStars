using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


//! DEPRECATED
public class EnvidoConfirmationUI : MonoBehaviour
{
    [SerializeField] private Button RaiseEnvidoButton;
    [SerializeField] private Button RaiseRealEnvidoButton;
    [SerializeField] private Button RaiseFaltaEnvidoButton;
    [SerializeField] private Button YesButton;
    [SerializeField] private Button NoButton;
    [SerializeField] private TextMeshProUGUI upgradeText;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private TextMeshProUGUI generalText;

    private void Awake()
    {
        NoButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EnvidoConfirmation(false);
        });
        YesButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EnvidoConfirmation(true);
        });
        RaiseEnvidoButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RaiseEnvido(EnvidoStage.Envido);
        });
        RaiseRealEnvidoButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RaiseEnvido(EnvidoStage.RealEnvido);
        });
        RaiseFaltaEnvidoButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RaiseEnvido(EnvidoStage.FaltaEnvido);
        });
    }
    private void Start()
    {
       // GameClientManager.Instance.EnvidoEvent += GCM_EnvidoEvent;
        GameManager.Instance.OnWaitingEnvidoConfirmation += GM_OnWaitingEnvidoConfirmation;
        Hide();
    }

    private void GM_OnWaitingEnvidoConfirmation(object sender, OnWaitingConfirmationArgs e)
    {
        if (e.isStageEnded)
        {
            Hide();
        }
        else
        {
            
        }
    }

    private void GCM_EnvidoEvent(object sender, EnvidoEventArgs e)
    {
        generalText.text = e.mainText;
        upgradeText.text = e.upgradeText;
        RaiseFaltaEnvidoButton.gameObject.SetActive(!e.hideUpgrade);
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}

