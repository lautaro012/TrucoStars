public abstract class GameRulesBase
{
    protected GameManager gameManager; 
    protected int pointsToWin;
    protected bool playWithFlor;
    protected bool playWithPicaPica;
    public GameRulesBase(GameManager gm, int pointsToWin, bool playWithFlor, bool playWithPicaPica)
    {
        this.gameManager = gm;
        this.pointsToWin = pointsToWin;
        this.playWithFlor = playWithFlor;
        this.playWithPicaPica = playWithPicaPica;
    }

    // 1. INICIALIZACIÓN
    public abstract void StartHand(); 

    // 2. ACCIONES DEL JUGADOR
    public abstract void ProcessCardPlayed(int clientSeat, int cardId, int cardUIIndex);
    public abstract void ProcessTrucoCall(int callerSeat, int callerTeam);
    public abstract void ProcessTrucoConfirmation(int responderSeat, bool accepted);
    public abstract void ProcessEnvidoCall(EnvidoStage call, int callerSeat, int callerTeam);
    public abstract void ProcessEnvidoConfirmation(int responderSeat, bool accepted);
    public abstract void ProcessSurrender(int surrenderSeat);
    public abstract int GetCardId(int clientSeat, int crid);
    public abstract void ResetHandData();
    // 3. MOTORES INTERNOS
    protected abstract void AdvanceTurn(int winnerSeat = -1);
    protected abstract void EvaluateRound();
    protected abstract void CheckForGameWin();
    
}