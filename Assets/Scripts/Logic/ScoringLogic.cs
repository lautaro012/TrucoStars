using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoringLogic 
{
    [Header("Reglas de la Casa")]
    //? Valores default para proteccion
    public int pointsToWin = 15;
    public bool playWithFlor = false;
    public bool playWithPicaPica = false;
    public int TrucoPointsInPlay { get; private set; } = 1;

    [Header("Estado de la ronda")]
    public EnvidoStage envidoStage { get; private set; } = EnvidoStage.None;
    private int envidoPointsInPlay = 0;
    public TrucoStage trucoStage { get; private set; } = TrucoStage.None;
    private int pointsInPlay = 1;


    /// <summary>
    /// METODO LLAMADO AL PRINCIPIO DEL JUEGO, SETEA LAS REGLAS DE LA CASA.
    /// </summary>
    /// <param name="lobbyPoints"></param>
    /// <param name="lobbyFlor"></param>
    /// <param name="lobbyPica"></param>
    public void ApplyLobbySettings(int lobbyPoints, bool lobbyFlor, bool lobbyPica)
    {
        pointsToWin = lobbyPoints;
        playWithFlor = lobbyFlor;
        playWithPicaPica = lobbyPica;
        
        Debug.Log($"[ScoringLogic] Reglas aplicadas: {pointsToWin} pts | Flor: {playWithFlor}");
    }

    /// <summary>
    /// Calcula el valor de envido de una mano y lo devuelve
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="c3"></param>
    /// <returns></returns>
    public int CalculateEnvidoValue(CardSO c1, CardSO c2, CardSO c3)
    {
        int EnvidoValue;
        if (c1.EnvidoValue > c2.EnvidoValue && c1.EnvidoValue > c3.EnvidoValue) EnvidoValue = c1.EnvidoValue;
        else if (c2.EnvidoValue > c3.EnvidoValue) EnvidoValue = c2.EnvidoValue;
        else EnvidoValue = c3.EnvidoValue;
        if (
            c1.CardSuit == c2.CardSuit ||
            c1.CardSuit == c3.CardSuit ||
            c2.CardSuit == c3.CardSuit
        )
        {
            if (c1.CardSuit == c2.CardSuit)
            {
                int newValue = c1.EnvidoValue + c2.EnvidoValue + 20;
                if (EnvidoValue < newValue)
                {
                    EnvidoValue = newValue;
                }
            }
            if (c1.CardSuit == c3.CardSuit)
            {
                int newValue = c1.EnvidoValue + c3.EnvidoValue + 20;
                if (EnvidoValue < newValue)
                {
                    EnvidoValue = newValue;
                }
            }
            if (c2.CardSuit == c3.CardSuit)
            {
                int newValue = c3.EnvidoValue + c2.EnvidoValue + 20;
                if (EnvidoValue < newValue)
                {
                    EnvidoValue = newValue;
                }
            }
        }
        return EnvidoValue;
    }



    /// <summary>
    /// Resuelve la ronda. Recibe el snapshot de cartas y dos funciones para consultar el valor y el equipo.
    /// </summary>
    public int ResolveRound(int[] cardsPlayedBySeats, Func<int, int> getCardValue, Func<int, int> getSeatTeam)
    {
        int highestValue = -2;
        List<int> SeatsWithBestCards = new();
        
        // cardsPlayedBySeats.Length ya nos dice cuántos jugadores hay (totalPlayers)
        for (int seatIndex = 0; seatIndex < cardsPlayedBySeats.Length; seatIndex++)
        {
            int cardId = cardsPlayedBySeats[seatIndex];
            if (cardId < 0) continue; // Este asiento no jugó carta aún

            // USAMOS LA FUNCIÓN QUE NOS PASÓ EL GAMEMANAGER PARA SABER EL VALOR
            int value = getCardValue(cardId);
            
            if (value > highestValue)
            {
                highestValue = value;
                SeatsWithBestCards.Clear();
                SeatsWithBestCards.Add(seatIndex);
            }
            else if (value == highestValue) 
            {
                SeatsWithBestCards.Add(seatIndex);
            }
        }

        if (SeatsWithBestCards.Count == 0) return -1;
        
        if (SeatsWithBestCards.Count == 1) return SeatsWithBestCards[0]; // Un solo ganador

        // --- HAY PARDA (MÁS DE UN GANADOR) ---
        // USAMOS LA FUNCIÓN DEL GAMEMANAGER PARA SABER DE QUÉ EQUIPO ES EL ASIENTO
        int team0 = getSeatTeam(SeatsWithBestCards[0]);
        bool sameTeam = SeatsWithBestCards.TrueForAll(s => getSeatTeam(s) == team0);
        
        if (sameTeam)
        {
            return SeatsWithBestCards[0]; // Gana el que tiró primero si son del mismo equipo
        }

        return -1; // Empate entre equipos rivales
    }


    //? ---- HELPERS ----

    public void AddEnvidoPointsByStage()
    {
        switch (envidoStage)
        {
            case EnvidoStage.Envido:
                envidoPointsInPlay++;
                break;
            case EnvidoStage.RealEnvido:
                envidoPointsInPlay += 2;
                break;
            case EnvidoStage.FaltaEnvido:
                //! CALCULAR VALORES A JUGAR.
                break;
            default:
                break;
        }
    }
    public void EnvidoCalled()
    {
        envidoPointsInPlay++;    
    }   
    public void TrucoAccepted()
    {
        pointsInPlay++;
    }
    public void RestartScoringValues()
    {
        trucoStage = TrucoStage.None;
        envidoStage = EnvidoStage.None;
        pointsInPlay = 1;
        envidoPointsInPlay = 0;
    }
    public int GetPointsInPlay()
    {
        return pointsInPlay;
    }
    public int GetEnvidoPointsInPlay()
    {
        return envidoPointsInPlay;
    }
    public void NextTrucoStage()
    {
        if (trucoStage == TrucoStage.None) trucoStage = TrucoStage.Truco;
        else if (trucoStage == TrucoStage.Truco) trucoStage = TrucoStage.Retruco;
        else trucoStage = TrucoStage.Vale4;
    }
    public void NextEnvidoStage(EnvidoStage nextStage)
    {
        envidoStage = nextStage;
    }

}