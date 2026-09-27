namespace HCDE.Playsim;

public enum InvasionPhase
{
    Waiting = 0,
    Wave = 1,
    Intermission = 2,
    Countdown = 3,
    Victory = 4,
}

/// <summary>
/// Spawns bot waves at MapSpot things, or at (64, 64) when the map has none.
/// A wave ends only after its own actors die or are removed, followed by a timed intermission.
/// Default off. Not the C++ invasion director.
/// </summary>
public sealed class InvasionDirector
{
    public const int SpawnSpotType = 9001;
    public const int WaveGapTics = 35;
    private readonly HashSet<uint> _waveActors = new();

    public bool Enabled { get; set; }
    public InvasionPhase Phase { get; private set; } = InvasionPhase.Waiting;
    public int Wave { get; private set; }
    public int Cooldown { get; private set; }
    public int Spawned { get; private set; }
    public int ActiveMonsters { get; private set; }
    public int Cleared => Spawned - ActiveMonsters;
    public int MaxWaves { get; private set; }
    public int CountdownTics { get; private set; }
    public int IntermissionTics { get; private set; } = WaveGapTics;
    public int SpawnSpotCount { get; private set; }

    public void Configure(int maxWaves, int countdownTics, int intermissionTics)
    {
        if (maxWaves < 0 || countdownTics < 0 || intermissionTics < 0)
            throw new ArgumentOutOfRangeException(nameof(maxWaves), "Wave limit and durations must be nonnegative.");
        if (Phase != InvasionPhase.Waiting)
            throw new InvalidOperationException("Configure invasion before the first wave.");
        MaxWaves = maxWaves;
        CountdownTics = countdownTics;
        IntermissionTics = intermissionTics;
    }

    public void Tick(AuthoritySimulation sim)
    {
        if (!Enabled || sim.Exited || Phase == InvasionPhase.Victory)
            return;
        if (Phase == InvasionPhase.Waiting && CountdownTics > 0)
        {
            Phase = InvasionPhase.Countdown;
            Cooldown = CountdownTics;
            return;
        }
        if (Phase == InvasionPhase.Wave)
        {
            ActiveMonsters = sim.Actors.Count(actor => _waveActors.Contains(actor.Id) && !actor.IsDead && !actor.Destroyed);
            if (ActiveMonsters == 0)
            {
                Phase = MaxWaves > 0 && Wave >= MaxWaves ? InvasionPhase.Victory : InvasionPhase.Intermission;
                Cooldown = Phase == InvasionPhase.Victory ? 0 : IntermissionTics;
            }
            return;
        }
        if (Cooldown > 0)
        {
            Cooldown--;
            if (Cooldown > 0)
                return;
        }

        Wave++;
        Phase = InvasionPhase.Wave;
        _waveActors.Clear();
        var spots = sim.Actors.Where(actor => actor.DoomEdNum == SpawnSpotType && !actor.Destroyed).ToArray();
        SpawnSpotCount = spots.Length;
        if (spots.Length == 0)
            _waveActors.Add(sim.AddBot(64, 64).Id);
        else
        {
            foreach (var spot in spots)
                _waveActors.Add(sim.AddBot(spot.X.ToDouble(), spot.Y.ToDouble()).Id);
        }

        Spawned = _waveActors.Count;
        ActiveMonsters = Spawned;
    }
}
