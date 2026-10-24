namespace HCDE.Net.Core;

/// <summary>Guest solo cutscene recovery from <c>Net_TickCutsceneClientRecovery</c>.</summary>
public sealed class CutsceneSoloClientRecoverySession
{
    public const uint RecoveryDelayMs = 1500;

    private ulong _readySinceMs;

    public bool TryForceEndScreenJob(
        bool isAuthority,
        bool netGame,
        bool demoPlayback,
        bool cutsceneGameState,
        bool runnerActive,
        int consolePlayer,
        bool consoleReady,
        int humanParticipants,
        ulong nowMs)
    {
        if (isAuthority || !netGame || demoPlayback)
            return false;
        if (!cutsceneGameState || !runnerActive)
            return false;
        if (consolePlayer < 0 || !consoleReady)
            return false;
        if (humanParticipants > 1)
            return false;

        if (_readySinceMs == 0)
        {
            _readySinceMs = nowMs;
            return false;
        }

        if (nowMs - _readySinceMs < RecoveryDelayMs)
            return false;

        _readySinceMs = 0;
        return true;
    }

    public void Reset() => _readySinceMs = 0;
}
