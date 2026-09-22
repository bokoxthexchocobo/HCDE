namespace HCDE.Playsim;

public enum InvasionPhase
{
    Waiting = 0,
    Wave = 1,
}

/// <summary>
/// Spawns one bot wave at thing type <see cref="SpawnSpotType"/>, or at (64, 64) when the map has none.
/// Default off. Not the C++ invasion director.
/// </summary>
public sealed class InvasionDirector
{
    public const int SpawnSpotType = 3004;
    private const int WaveGapTics = 35;

    public bool Enabled { get; set; }
    public InvasionPhase Phase { get; private set; } = InvasionPhase.Waiting;
    public int Wave { get; private set; }
    public int Cooldown { get; private set; }

    public void Tick(AuthoritySimulation sim)
    {
        if (!Enabled || sim.Exited)
            return;
        if (Cooldown > 0)
        {
            Cooldown--;
            return;
        }

        Wave++;
        Phase = InvasionPhase.Wave;
        var spots = sim.Level.Things.Where(thing => thing.Type == SpawnSpotType).ToArray();
        if (spots.Length == 0)
            sim.AddBot(64, 64);
        else
        {
            foreach (var spot in spots)
                sim.AddBot(spot.X, spot.Y);
        }

        Cooldown = WaveGapTics;
    }
}
