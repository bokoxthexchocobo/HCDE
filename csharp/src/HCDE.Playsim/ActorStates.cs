namespace HCDE.Playsim;

public sealed record ActorFrame(int Tics, int NextState, Action<Actor>? Action = null);

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

    public void Configure(Actor actor, IEnumerable<ActorFrame> frames, int initialState)
    {
        if (_entering) throw new InvalidOperationException("Cannot replace a state table during an action.");
        var table = frames.ToArray();
        if (table.Length == 0 || table.Any(frame => frame.Tics < -1 || frame.NextState < -1 || frame.NextState >= table.Length))
            throw new ArgumentException("Invalid actor state table.", nameof(frames));
        if (initialState < 0 || initialState >= table.Length)
            throw new ArgumentOutOfRangeException(nameof(initialState));
        _frames = table;
        Enter(actor, initialState);
    }

    public bool HasState(int state) => state >= 0 && state < _frames.Length;

    public void Enter(Actor actor, int state)
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
                RemainingTics = frame.Tics;
                frame.Action?.Invoke(actor);
                if (actor.Destroyed) return;
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
        if (actor.Destroyed || RemainingTics < 0) return;
        if (RemainingTics > 0) RemainingTics--;
        if (RemainingTics == 0) Enter(actor, _frames[Current].NextState);
    }

    internal void Restore(int state, int tics)
    {
        if (!HasState(state) || tics < -1) throw new InvalidOperationException("Saved actor state is not in the current table.");
        Current = state;
        RemainingTics = tics;
    }

    /// <summary>Native corpse shatter sets <c>tics</c> to 1 without changing the frame.</summary>
    internal void ForceRemainingTics(int tics) => RemainingTics = tics;
}
