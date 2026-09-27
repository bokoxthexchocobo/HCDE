using HCDE.Playsim;

namespace HCDE.Server;

/// <summary>Accumulates exact 35 Hz simulation tics from a monotonic millisecond clock.</summary>
public sealed class ServerTicScheduler
{
    private ulong _lastMilliseconds;
    private ulong _pendingMilliTics;

    public ServerTicScheduler(ulong nowMilliseconds) => _lastMilliseconds = nowMilliseconds;

    public int TakeDueTics(ulong nowMilliseconds)
    {
        if (nowMilliseconds < _lastMilliseconds)
            return 0;
        var elapsed = nowMilliseconds - _lastMilliseconds;
        _lastMilliseconds = nowMilliseconds;
        _pendingMilliTics += elapsed * GameTicClock.TicRate;
        // Limit each batch to keep cancellation and networking responsive. Retain debt.
        var due = (int)Math.Min(_pendingMilliTics / 1000, 8UL);
        _pendingMilliTics -= (ulong)due * 1000;
        return due;
    }
}
