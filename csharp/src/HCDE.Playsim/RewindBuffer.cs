namespace HCDE.Playsim;

/// <summary>
/// Authority keyframe ring. Restore puts actor poses and sector heights back.
/// Live clients are not resynced. That remains the C++ rewind gap.
/// </summary>
public sealed class RewindBuffer
{
    private readonly List<byte[]> _frames = new();

    public int Capacity { get; init; } = 8;
    public int Count => _frames.Count;

    public void Capture(AuthoritySimulation sim)
    {
        _frames.Add(SimSavegame.Write(sim));
        if (_frames.Count > Capacity)
            _frames.RemoveAt(0);
    }

    public bool RestoreOldest(AuthoritySimulation sim)
    {
        if (_frames.Count == 0)
            return false;
        SimSavegame.Apply(sim, _frames[0]);
        return true;
    }
}
