namespace HCDE.Net.Core;

public sealed class GuestInvasionState : IInvasionSnapshotApplySink
{
    public InvasionMirrorState MirrorState { get; private set; }

    public InvasionSpawnDirectory? SpawnDirectory { get; private set; }

    public int ApplyMirrorCalls { get; private set; }

    public bool ApplyMirror(
        InvasionSnapshotHeader header,
        uint waveSpawned,
        uint waveCleared)
    {
        ApplyMirrorCalls++;
        MirrorState = new InvasionMirrorState(header.State, (int)header.Wave, waveSpawned, waveCleared);
        return true;
    }

    public bool ApplySpawnDirectory(InvasionSpawnDirectory directory)
    {
        SpawnDirectory = directory;
        return true;
    }
}
