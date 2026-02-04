using System.Collections.Generic;
using UnityEngine;

public class Hand
{
    private int HandIndex; //* NUMERO DE MANO 
    private int CurrentRoundIndex; //* QUE RONDA ACTUAL SE ESTA JUGANDO
    public List<Round> Rounds; //* RONDAS (3)
    private int StartingSeatThisHand; //* ASIENTO 
    private int Team1RoundsWon;
    private int Team2RoundsWon;
    private bool IsClosed = false;
    private int TeamWinner = -1;
    private int firstRoundWinner = 0;

    public Hand(int handIndex, int startingSeat, int totalPlayers)
    {
        HandIndex = handIndex;
        Team1RoundsWon = 0;
        Team2RoundsWon = 0;
        StartingSeatThisHand = startingSeat;
        CurrentRoundIndex = 0;
        Rounds = new List<Round>(3) {
            new(0, startingSeat, totalPlayers)
        };
    }
    public Round CurrentRound => Rounds[CurrentRoundIndex];

    //* GETTERS
    public int GetHandIndex() => HandIndex;
    public int GetTeam1RoundsWon() => Team1RoundsWon;
    public int GetTeam2RoundsWon() => Team2RoundsWon;
    public int GetCurrentRoundIndex() => CurrentRoundIndex;
    public int GetStartingSeatThisHand() => StartingSeatThisHand;
    public int GetFirstRoundWinner() => firstRoundWinner;
    public bool IsHandClosed() => IsClosed;

    //* SETTERS
    public void SetFirstRoundWinner(int winnerTeam) { firstRoundWinner = winnerTeam; }
    public void SetStartingSeatThisHand(int startingSeat) { StartingSeatThisHand = startingSeat; }
    public int NextCurrentRoundIndex()
    {
        CurrentRoundIndex++;
        return CurrentRoundIndex;
    }
    public void AddPointToTeam1() { Team1RoundsWon++; }
    public void AddPointToTeam2() { Team2RoundsWon++; }
    public void CloseHand() { IsClosed = true; }
    public int CalculateWinner()
    {
        if (IsClosed)
        {
            TeamWinner = Team1RoundsWon == 2 ? 1 : 2;
        }
        else
        {
            Debug.LogWarning("-----NO ESTAMOS EN RONDA 3 TODAVIA, O LA MANO NO ESTA CERRADA---------");
            TeamWinner = -1;
        }
        return TeamWinner;
    }

    public int GetWinnerTeam() => TeamWinner;




    //* utils

    public void RegisterRoundWinner(int winnerSeat, int winnerTeam)
    {
        Rounds[CurrentRoundIndex].Close(winnerSeat);
        Debug.Log("\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");
        Debug.Log("REGISTRANDO RONDA " + CurrentRoundIndex + ", Gana asiento " + winnerSeat + " del equipo " + winnerTeam);
        if (winnerSeat >= 0)
        {
            if (winnerTeam == 1) AddPointToTeam1();
            else AddPointToTeam2();

            if (CurrentRoundIndex == 0) firstRoundWinner = winnerTeam;

            CheckForWinner(); //* CHEQUEO SI HAY GANADOR
            return;
        }
        //* EMPATE 
        if (CurrentRoundIndex == 0)
        {
            AddPointToTeam1();
            AddPointToTeam2();
        }
        else
        {
            if (Team1RoundsWon > Team2RoundsWon) //*SI HAY EMPATE EN 2DA RONDA GANA EL QUE GANO LA PRIMERA
            {
                AddPointToTeam1();
            }
            else
            {
                AddPointToTeam2();
            }
        }
        CheckForWinner(); //* CHEQUEO SI HAY GANADOR
    }
    private void CheckForWinner()
    {
        bool isHandOver = Team1RoundsWon == 2 || Team2RoundsWon == 2 || CurrentRoundIndex == 2;
        if (isHandOver)
        {
            CloseHand();
            CalculateWinner();
        }
    }


}
