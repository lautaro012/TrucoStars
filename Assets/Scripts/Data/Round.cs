using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Round
{
    private int RoundIndex; //* NUMERO DE RONDA 
    private int leaderSeat; //* LIDER DE ESTA RONDA
    private int winnerSeat; //* GANADOR DE ESTA RONDA
    private int[] cardsPlayedBySeats; //* CARTAS JUGADAS POR CADA JUGADOR EN ESTA RONDA
    private int expectedCards;
    private int playCount = 0;
    List<int> playOrderSeatIndices; //* ORDEN EN QUE FUERON JUGADAS
    bool roundClosed = false; //*ronda terminada

    public Round(int roundIndex, int leaderSeat, int totalPlayers)
    {
        RoundIndex = roundIndex;
        this.leaderSeat = leaderSeat;
        winnerSeat = -1;
        cardsPlayedBySeats = Enumerable.Repeat(-1, totalPlayers).ToArray();
        expectedCards = totalPlayers;
        playOrderSeatIndices = new List<int>(totalPlayers);
    }

    public bool TryPlay(int seatIndex, int cardId) {

        if (roundClosed)
        {
            Debug.LogError("LA RONDA ESTA CERRADA, NO SE PUEDE JUGAR: " + seatIndex + " " + cardId);
            return false;
        }
        if (seatIndex < 0 || seatIndex >= cardsPlayedBySeats.Length)
        {
            Debug.LogError("SEAT INDEX NEGATIVO O MAYOR AL NUMERO DE CARTAS PERMITIDO POR RONDA");
            return false;
        }
        if (cardsPlayedBySeats[seatIndex] != -1)
        {
            Debug.LogError("EL SEAT YA JUGO UNA CARTA ");
            return false;
        }
        cardsPlayedBySeats[seatIndex] = cardId;
        playOrderSeatIndices.Add(seatIndex);
        playCount++;
        return true;
    }

    public bool IsComplete() {
        return playCount == expectedCards;
    }
    public int GetCardOfSeat(int seatIndex) => cardsPlayedBySeats[seatIndex];
    public int GetSeatOfCard(int cardId) => playOrderSeatIndices[cardsPlayedBySeats[cardId]];
    public int GetLeaderSeat() => leaderSeat;
    public IReadOnlyList<int> PlayOrder => playOrderSeatIndices;
    public int GetLeaderSet => leaderSeat;

    public void Close(int winnerSeat) {
        roundClosed = true;
        this.winnerSeat = winnerSeat;
    }
    public bool IsRoundClosed() => roundClosed;

    public int[] SnapshotCards() => (int[])cardsPlayedBySeats.Clone();
}