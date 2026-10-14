namespace HCDE.Playsim;

/// <summary>
/// Authority keyframe ring using identity saves to restore supported actors and thinker order.
/// Live clients are not resynced. That remains the C++ rewind gap.
/// </summary>
public sealed class RewindBuffer
{
    // MAXPLAYERS in src/common/engine/i_net.h.
    public const int MaxClientSlots = 64;
    private sealed record Keyframe(int Tic, byte[] Data);
    private readonly List<Keyframe> _frames = new();
    private readonly int?[] _clientViewTics = new int?[MaxClientSlots];
    private long _memoryLimitBytes = 32L * 1024 * 1024;
    private float _captureIntervalSeconds = 1;
    private int? _lastCaptureTic;
    private bool _testingLagCompBracket;
    private int _maxLagCompensationAgeTics = 12;

    public int Capacity { get; set; } = 8;
    public int Count => _frames.Count;
    public bool LagCompensationEnabled { get; set; }
    public int MaxLagCompensationAgeTics
    {
        get => _maxLagCompensationAgeTics;
        set => _maxLagCompensationAgeTics = Math.Clamp(value, 1, GameTicClock.TicRate * 2);
    }
    public float CaptureIntervalSeconds
    {
        get => _captureIntervalSeconds;
        set
        {
            if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(CaptureIntervalSeconds));
            _captureIntervalSeconds = Math.Clamp(value, 0.25f, 30f);
        }
    }
    public long MemoryLimitBytes
    {
        get => _memoryLimitBytes;
        set
        {
            if (value < 1) throw new ArgumentOutOfRangeException(nameof(MemoryLimitBytes));
            _memoryLimitBytes = value;
        }
    }
    // Retained archive payload only; excludes object overhead and temporary capture allocations.
    public long TotalBytes { get; private set; }
    public long EvictionsForMemory { get; private set; }
    public long EvictionsForDepth { get; private set; }
    public long LagCompBracketTests { get; private set; }
    public long LagCompBracketFailures { get; private set; }

    public void Reset()
    {
        _frames.Clear();
        TotalBytes = 0;
        _lastCaptureTic = null;
        Array.Clear(_clientViewTics);
    }

    public bool TryRecordClientViewTic(int playerNum, int viewSequence, int ticDup = 1)
    {
        if (playerNum < 0 || playerNum >= _clientViewTics.Length || viewSequence < 0)
            return false;
        var rawTic = (long)viewSequence * Math.Max(ticDup, 1);
        if (rawTic > int.MaxValue) return false;
        _clientViewTics[playerNum] = (int)rawTic;
        return true;
    }

    public int GetClientViewTic(int playerNum) => playerNum < 0 || playerNum >= _clientViewTics.Length
        ? -1 : _clientViewTics[playerNum] ?? -1;

    public void Capture(AuthoritySimulation sim)
    {
        var data = SimSavegame.Write(sim, includeBotIdentities: true);
        EvictForMemory(data.LongLength);
        _frames.Add(new(sim.Thinkers.Clock.Tic, data));
        TotalBytes += data.LongLength;
        EvictForMemory(0);
        while (_frames.Count > Math.Max(Capacity, 1))
        {
            EvictOldest();
            EvictionsForDepth++;
        }
        _lastCaptureTic = sim.Thinkers.Clock.Tic;
    }

    public bool CaptureIfDue(AuthoritySimulation sim)
    {
        if (!sim.RewindEnabled) return false;
        var intervalTics = Math.Max((int)(CaptureIntervalSeconds * GameTicClock.TicRate), 1);
        if (_lastCaptureTic is { } previous && (long)sim.Thinkers.Clock.Tic - previous < intervalTics)
            return false;
        Capture(sim);
        return true;
    }

    private void EvictForMemory(long pendingBytes)
    {
        while (_frames.Count > 0 && TotalBytes > MemoryLimitBytes - pendingBytes)
        {
            EvictOldest();
            EvictionsForMemory++;
        }
    }

    private void EvictOldest()
    {
        TotalBytes -= _frames[0].Data.LongLength;
        _frames.RemoveAt(0);
    }

    public bool RestoreOldest(AuthoritySimulation sim)
    {
        if (_frames.Count == 0)
            return false;
        return RestoreFrame(sim, _frames[0]);
    }

    public bool RestoreAtOrBefore(AuthoritySimulation sim, int targetTic)
    {
        if (_frames.Count == 0)
            return false;
        return RestoreFrame(sim, _frames[FindHistoricalIndex(targetTic)]);
    }

    private int FindHistoricalIndex(int targetTic)
    {
        var selected = 0;
        var bestTic = int.MinValue;
        for (var i = 0; i < _frames.Count; i++)
        {
            var tic = _frames[i].Tic;
            if (tic <= targetTic && tic > bestTic)
            {
                selected = i;
                bestTic = tic;
            }
        }
        // Native rewind falls back to the oldest stored frame when the request predates history.
        return selected;
    }

    public bool TestLagCompBracket(AuthoritySimulation sim, int playerNum)
    {
        LagCompBracketTests++;
        var success = false;
        try
        {
            var viewTic = GetClientViewTic(playerNum);
            if (_testingLagCompBracket || viewTic < 0 || _frames.Count == 0) return false;
            var frame = _frames[FindHistoricalIndex(viewTic)];
            success = QueryHistoricalState(sim, ReadFrame(frame), () => true);
            return success;
        }
        finally
        {
            if (!success) LagCompBracketFailures++;
        }
    }

    internal bool TryTraceHistoricalHitscan(AuthoritySimulation sim, PlayerPawn attacker,
        BamAngle angle, BamAngle pitch, double range, out CombatTrace.LineAttackHit hit)
    {
        hit = default;
        if (!LagCompensationEnabled || !sim.RewindEnabled || _testingLagCompBracket
            || attacker.Destroyed || !ReferenceEquals(attacker.Simulation, sim) || _frames.Count == 0)
            return false;
        var viewTic = GetClientViewTic(attacker.PlayerNum);
        var age = (long)sim.Thinkers.Clock.Tic - viewTic;
        if (viewTic < 0 || age < 0 || age > MaxLagCompensationAgeTics) return false;
        var frame = _frames[FindHistoricalIndex(viewTic)];
        var historical = ReadFrame(frame);
        if (sim.HasSavedActorAllocatorConflict(historical) || !sim.CanRestoreThinkerMembership(historical))
            return false;
        var historicalHit = QueryHistoricalState(sim, historical, () =>
        {
            var shooter = sim.Players.Single(player => player.Id == attacker.Id);
            return CombatTrace.TraceLineAttack(sim, shooter, angle, pitch, range);
        });
        // Historical actor instances may have been removed while restoring the live world.
        var victim = historicalHit.Victim is { } target
            ? sim.Actors.FirstOrDefault(actor => actor.Id == target.Id && !actor.Destroyed) : null;
        hit = historicalHit with { Victim = victim };
        return true;
    }

    private static SimSaveState ReadFrame(Keyframe frame)
    {
        if (!SimSavegame.TryRead(frame.Data, out var state, out var error))
            throw new InvalidOperationException(error);
        return state;
    }

    private T QueryHistoricalState<T>(AuthoritySimulation sim, SimSaveState historical, Func<T> query)
    {
        var savedTimer = _lastCaptureTic;
        _testingLagCompBracket = true;
        try
        {
            var current = sim.CaptureState(includeBotIdentities: true);
            var transient = sim.CaptureTransientState();
            try
            {
                sim.RestoreState(historical, notify: false);
                return query();
            }
            finally
            {
                sim.RestoreState(current, notify: false);
                sim.RestoreTransientState(transient);
            }
        }
        finally
        {
            _lastCaptureTic = savedTimer;
            _testingLagCompBracket = false;
        }
    }

    private bool RestoreFrame(AuthoritySimulation sim, Keyframe frame)
    {
        SimSavegame.Apply(sim, frame.Data);
        _lastCaptureTic = frame.Tic;
        return true;
    }
}
