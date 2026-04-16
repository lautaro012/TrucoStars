using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EnvidoButtonsUI : MonoBehaviour
{
    [SerializeField] Button EnvidoButton;
    [SerializeField] Button RealButton;
    [SerializeField] Button FaltaButton;
    [SerializeField] Button MainButton;
    [SerializeField] Button CancelButton;
    [SerializeField] TextMeshProUGUI points;
    [SerializeField] Transform SubButtonsParent;
    private bool areButtonsHide = true;
    private bool stageEnded = false;
    private bool lastCanCallEnvido = false;
    private void Awake()
    {
        MainButton.onClick.AddListener(() =>
        {
            SetSubButtons();
        });
        CancelButton.onClick.AddListener(() =>
        {
            SetHideButtons();
        });
        EnvidoButton.onClick.AddListener(() =>
        {
            Hide();
            GameManager.Instance.Envido(EnvidoStage.Envido);
        });
        RealButton.onClick.AddListener(() =>
        {
            Hide();
            GameManager.Instance.Envido(EnvidoStage.RealEnvido);
        });
        FaltaButton.onClick.AddListener(() =>
        {
            Hide();
            GameManager.Instance.Envido(EnvidoStage.FaltaEnvido);
        });
    }

    private void SetSubButtons()
    {
        if (areButtonsHide)
        {
            SetActiveButtons();
        }
        else
        {
            SetHideButtons();
        }
    }

    private void Start()
    {
        Hide();
        SetHideButtons();
        GameClientManager.Instance.ShowMainRoundButtons += GCM_OnRoundStarted;
        GameClientManager.Instance.SetEnvidoButton += GCM_SetEnvidoButton;
        GameClientManager.Instance.HideEnvidoButtons += GCM_HideEnvidoButtons;
        GameClientManager.Instance.EnvidoStageEnded += GCM_EnvidoStageEnded;
        GameManager.Instance.SentEnvidoValue += GameManager_OnSentEnvidoValue;
    }
    private void OnDestroy()
    {
        GameClientManager.Instance.ShowMainRoundButtons -= GCM_OnRoundStarted;
        GameClientManager.Instance.SetEnvidoButton -= GCM_SetEnvidoButton;
        GameClientManager.Instance.HideEnvidoButtons -= GCM_HideEnvidoButtons;
        GameClientManager.Instance.EnvidoStageEnded -= GCM_EnvidoStageEnded;
        GameManager.Instance.SentEnvidoValue -= GameManager_OnSentEnvidoValue;        
    }

    private void GCM_EnvidoStageEnded(object sender, EventArgs e)
    {
        Hide();
        stageEnded = true;
        UpdateButtonVisibility(); 
    }
    private void GCM_OnRoundStarted(object sender, EventArgs e)
    {
        Debug.Log("ronda iniciada");
        stageEnded = false;
        UpdateButtonVisibility(); 
    }

    private void GCM_SetEnvidoButton(object sender, CanICallEnvidoArgs e)
    {
        lastCanCallEnvido = e.canICallEnvido; 
        UpdateButtonVisibility(); 
    }


    private void UpdateButtonVisibility()
    {
        if (lastCanCallEnvido && !stageEnded)
        {
            Show();
            SetHideButtons();
        }
        else
        {
            Hide();
            SetHideButtons();
        }
    }
    private void GameManager_OnSentEnvidoValue(object sender, OnSentEnvidoArgs e)
    {
        points.text = e.value.ToString();
    }

    private void GCM_HideEnvidoButtons(object sender, EventArgs e)
    {
        Hide();
    }

    private void SetActiveButtons()
    {
        areButtonsHide = false;
        SubButtonsParent.gameObject.SetActive(true);
    }
    private void SetHideButtons()
    {
        areButtonsHide = true;
        SubButtonsParent.gameObject.SetActive(false);
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
