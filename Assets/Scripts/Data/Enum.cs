using UnityEngine;

public enum GameState
{
    None,                 // Esperando que se conecten los jugadores
    SetupHand,            // Repartiendo cartas y calculando envidos ocultos
    PlayingTurn,          // Estado normal: Esperando que un jugador tire carta o cante
    WaitingEnvido,        // JUEGO PAUSADO: Alguien cantó Envido
    WaitingTruco,         // JUEGO PAUSADO: Alguien cantó Truco
    RoundFinished,        // Terminó una de las 3 rondas (se decide quién gana)
    HandFinished          // Terminó la mano completa (se suman puntos al score)
}
public enum Scene
{
    MainScene,
    LobbyScene,
    LoadingScene
}

public enum RoundState {
    None,
    RoundStarted,
    RoundFinished
}
public enum TrucoStage
{
    None,
    Truco,
    Retruco,
    Vale4
}
public enum EnvidoStage
{
    None,
    Envido,
    EnvidoEnvido,
    RealEnvido,
    FaltaEnvido
}
public enum Truco
{
    ARGENTINO,
    BRASILERO
}