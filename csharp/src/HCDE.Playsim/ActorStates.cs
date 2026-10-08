namespace HCDE.Playsim;

public sealed record ActorFrame(int Tics, int NextState, Action<Actor>? Action = null, bool CanRaise = false,
    bool? FullBright = null, Func<Actor, int?>? StateAction = null, bool Fast = false, bool Slow = false, bool NoDelay = false, int TicRange = 0,
    bool ActorUsable = true)
{
    public int GetTics(Actor actor)
    {
        var tics = GetUnscaledTics(actor);
        return actor.IsFast() && Fast ? tics - (tics >> 1)
            : actor.IsSlow() && Slow ? unchecked(tics << 1) : tics;
    }

    internal int GetUnscaledTics(Actor actor)
    {
        if (TicRange is < 0 or > ushort.MaxValue) throw new InvalidOperationException("Invalid actor tic range.");
        var tics = Tics;
        if (TicRange != 0)
        {
            var sim = actor.Simulation ?? throw new InvalidOperationException("Random frame timing requires a simulation.");
            var random = actor.IsClientSide() ? sim.NextClientStateRandom() : sim.NextStateRandom();
            tics = unchecked(tics + (int)(random % ((uint)TicRange + 1)));
        }
        return tics;
    }
}

/// <summary>Timed state frames: -1 tics holds; zero tics chains immediately; next -1 removes.</summary>
public sealed class ActorStateMachine
{
    public const int Spawn = 0;
    public const int Pain = 1;
    public const int Death = 2;
    public const int Corpse = 3;
    private ActorFrame[] _frames =
    {
        new(-1, Spawn), new(4, Spawn), new(6, Corpse), new(-1, Corpse),
    };
    private bool _entering;
    private int? _queued;

    public int Current { get; private set; } = Spawn;
    public int RemainingTics { get; private set; } = -1;
    public bool CurrentCanRaise => HasState(Current) && _frames[Current].CanRaise;
    internal bool CurrentFullBright => HasState(Current) && _frames[Current].FullBright.GetValueOrDefault();

    public void Configure(Actor actor, IEnumerable<ActorFrame> frames, int initialState, bool noFunction = false)
    {
        ConfigureTable(frames, initialState);
        actor.HandleNoDelay = noFunction;
        Enter(actor, initialState, noFunction);
    }

    /// <summary>Native spawn initialization keeps the initial frame without actions or immediate chaining.</summary>
    public void ConfigureSpawn(Actor actor, IEnumerable<ActorFrame> frames, int initialState)
    {
        ConfigureTable(frames, initialState);
        var frame = _frames[initialState];
        var tics = frame.GetUnscaledTics(actor);
        Current = initialState;
        RemainingTics = tics;
        actor.FullBright = frame.FullBright.GetValueOrDefault();
        actor.HandleNoDelay = true;
    }

    private void ConfigureTable(IEnumerable<ActorFrame> frames, int initialState)
    {
        if (_entering) throw new InvalidOperationException("Cannot replace a state table during an action.");
        var table = frames.ToArray();
        if (table.Length == 0 || table.Any(frame => frame.Tics < -1 || frame.TicRange is < 0 or > ushort.MaxValue || frame.NextState < -1 || frame.NextState >= table.Length
            || frame.Action is not null && frame.StateAction is not null))
            throw new ArgumentException("Invalid actor state table.", nameof(frames));
        if (initialState < 0 || initialState >= table.Length)
            throw new ArgumentOutOfRangeException(nameof(initialState));
        _frames = table;
    }

    internal bool CheckNoDelay(Actor actor)
    {
        if (!HasState(Current) || !_frames[Current].NoDelay) return !actor.Destroyed;
        var frame = _frames[Current];
        frame.Action?.Invoke(actor);
        var returnedState = frame.StateAction?.Invoke(actor);
        if (actor.Destroyed) return false;
        if (returnedState is { } state)
        {
            if (!HasState(state)) throw new InvalidOperationException("Actor action returned an invalid state.");
            Enter(actor, state);
        }
        return !actor.Destroyed;
    }

    public bool HasState(int state) => state >= 0 && state < _frames.Length;

    /// <summary>Native SetState nofunction skips actions for this entry's immediate chain.</summary>
    public void Enter(Actor actor, int state, bool noFunction = false)
    {
        if (state < -1 || state >= _frames.Length)
            throw new ArgumentOutOfRangeException(nameof(state));
        if (_entering)
        {
            _queued = state;
            return;
        }
        _entering = true;
        try
        {
            for (var transitions = 0; transitions < 1024; transitions++)
            {
                _queued = null;
                Current = state;
                if (state == -1)
                {
                    RemainingTics = -1;
                    actor.Destroy();
                    return;
                }
                var frame = _frames[state];
                if (!frame.ActorUsable)
                {
                    System.Diagnostics.Trace.TraceError("Actor {0} entered state {1}, which is not permitted for actor use.", actor.Id, state);
                    Current = -1;
                    RemainingTics = -1;
                    actor.Destroy();
                    return;
                }
                RemainingTics = frame.GetTics(actor);
                actor.FullBright = frame.FullBright.GetValueOrDefault();
                if (!noFunction) frame.Action?.Invoke(actor);
                var returnedState = noFunction ? null : frame.StateAction?.Invoke(actor);
                if (actor.Destroyed) return;
                if (returnedState is { } jump)
                {
                    if (!HasState(jump)) throw new InvalidOperationException("Actor action returned an invalid state.");
                    state = jump;
                    continue;
                }
                if (_queued is { } next) { state = next; continue; }
                if (RemainingTics != 0) return;
                state = frame.NextState;
            }
            RemainingTics = -1;
            throw new InvalidOperationException("Actor state chain exceeded 1024 immediate transitions.");
        }
        finally { _entering = false; _queued = null; }
    }

    public void Tick(Actor actor)
    {
        if (actor.Destroyed || RemainingTics == -1) return;
        RemainingTics = unchecked(RemainingTics - 1);
        if (RemainingTics <= 0) Enter(actor, _frames[Current].NextState);
    }

    internal void Restore(Actor actor, int state, int tics)
    {
        if (!HasState(state)) throw new InvalidOperationException("Saved actor state is not in the current table.");
        Current = state;
        RemainingTics = tics;
        if (_frames[state].FullBright is { } fullBright) actor.FullBright = fullBright;
    }

    /// <summary>Native corpse shatter sets <c>tics</c> to 1 without changing the frame.</summary>
    internal void ForceRemainingTics(int tics) => RemainingTics = tics;

    /// <summary>Native A_SetTics when called from this actor's state action.</summary>
    public void SetTics(int tics)
    {
        RemainingTics = tics;
    }
}
