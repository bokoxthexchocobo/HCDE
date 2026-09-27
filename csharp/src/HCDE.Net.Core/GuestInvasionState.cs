namespace HCDE.Net.Core;

public sealed class GuestInvasionState : IInvasionSnapshotApplySink
{
    public InvasionMirrorState MirrorState { get; private set; }

    public InvasionSpawnDirectory? SpawnDirectory { get; private set; }

    public int ApplyMirrorCalls { get; private set; }
    public uint StateTics { get; private set; }
    public uint ActiveMonsters { get; private set; }
    public uint MaxWaves { get; private set; }
    public uint PendingWave { get; private set; }

    public bool ApplyMirror(
        InvasionSnapshotHeader header,
        uint waveSpawned,
        uint waveCleared)
    {
        ApplyMirrorCalls++;
        MirrorState = new InvasionMirrorState(header.State, (int)header.Wave, waveSpawned, waveCleared);
        StateTics = header.StateTics;
        ActiveMonsters = header.ActiveMonsters;
        MaxWaves = header.MaxWaves;
        PendingWave = header.State == LiveConstants.InvasionStateCountdown
            ? (header.Wave == uint.MaxValue ? uint.MaxValue : header.Wave + 1) : 0;
        return true;
    }

    public bool ApplySpawnDirectory(InvasionSpawnDirectory directory)
    {
        SpawnDirectory = directory;
        return true;
    }
}
