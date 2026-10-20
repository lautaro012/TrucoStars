using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DynamicBoardUI : MonoBehaviour
{
    [Header("Los 3 Botones sin presionar")]
    [SerializeField] private Button TopButton;
    [SerializeField] private Button MediumButton;
    [SerializeField] private Button BotButton;
    [Header("Los 3 Botones Activos")]
    [SerializeField] private GameObject TopButtonActive;
    [SerializeField] private GameObject MediumButtonActive;
    [SerializeField] private GameObject BotButtonActive;
    [Header("Los 3 Contenidos")]
    public GameObject contenidoValores;
    public GameObject contenidoSenas;
    public GameObject contenidoGestos;

    
    void Start()
    {
        TopButton.onClick.AddListener(() => 
        { 
            DisableButtons();
            TopButtonActive.SetActive(true);
            ActivarValores();
        });
        MediumButton.onClick.AddListener(() => 
        {  
            DisableButtons();
            MediumButtonActive.SetActive(true);
            ActivarGestos();
        });
        BotButton.onClick.AddListener(() => 
        { 
            DisableButtons();
            BotButtonActive.SetActive(true);
            ActivarSenas();
        });
        DisableButtons();
        ActivarValores();
        TopButtonActive.SetActive(true);
    }

    private void DisableButtons(){
        TopButtonActive.SetActive(false);
        MediumButtonActive.SetActive(false);
        BotButtonActive.SetActive(false);
    }
    public void ActivarValores()
    {
        contenidoValores.SetActive(true);
        contenidoSenas.SetActive(false);
        contenidoGestos.SetActive(false);
    }
    public void ActivarSenas()
    {
        contenidoValores.SetActive(false);
        contenidoSenas.SetActive(true);
        contenidoGestos.SetActive(false);
    }
    public void ActivarGestos()
    {
        contenidoValores.SetActive(false);
        contenidoSenas.SetActive(false);
        contenidoGestos.SetActive(true);
    }
}
