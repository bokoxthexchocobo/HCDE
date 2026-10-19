namespace HCDE.Net.Core;

/// <summary>Lag-state fraction modifiers from <c>d_net.cpp</c> (<c>Net_Modify*Frac</c>).</summary>
public enum NetLagState : byte
{
    None = 0,
    Skipping = 1,
    Waiting = 2,
}

public static class NetLagPresentationPolicy
{
    public const int MaxSendTics = 3;
    public const int TicDup = 1;

    public static bool IsWaiting(NetLagState lagState) => lagState == NetLagState.Waiting;

    public static double ModifyFrac(NetLagState lagState, double ticFrac) =>
        lagState < NetLagState.Waiting ? ticFrac : 1.0;

    public static double ModifyObjectFrac(NetLagState lagState, double ticFrac, bool clientSideObject) =>
        lagState is NetLagState.None or NetLagState.Skipping || clientSideObject ? ticFrac : 1.0;

    public static double ModifyParticleFrac(NetLagState lagState, double ticFrac) =>
        lagState is NetLagState.None or NetLagState.Skipping ? ticFrac : 0.0;
}

public sealed class AuthorityWaitWatchdogSession
{
    public int EnterTic { get; private set; }
    public int AuthorityWaitGraceUntilTic { get; private set; }
    public int LastGameUpdateTic { get; private set; }

    public void Reset(string? reason, int enterTic, NetRoomCommandResetSession roomSession, bool trace = true)
    {
        LastGameUpdateTic = enterTic;
        EnterTic = enterTic;
        AuthorityWaitGraceUntilTic = enterTic + NetLagPresentationPolicy.MaxSendTics * NetLagPresentationPolicy.TicDup * 3;
        roomSession.ClearStaleWaitingFlags();
        LastResetReason = reason;
        if (trace)
            TraceCount++;
    }

    public string? LastResetReason { get; private set; }
    public int TraceCount { get; private set; }

    public bool IsWithinGrace(int gameTic) => gameTic <= AuthorityWaitGraceUntilTic;
}
