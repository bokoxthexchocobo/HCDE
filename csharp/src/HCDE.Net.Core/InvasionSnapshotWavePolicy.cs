namespace HCDE.Net.Core;

public readonly struct InvasionMirrorState
{
    public InvasionMirrorState(
        byte state,
        int wave,
        uint waveSpawned,
        uint waveCleared)
    {
        State = state;
        Wave = wave;
        WaveSpawned = waveSpawned;
        WaveCleared = waveCleared;
    }

    public byte State { get; }
    public int Wave { get; }
    public uint WaveSpawned { get; }
    public uint WaveCleared { get; }
}

public static class InvasionSnapshotWavePolicy
{
    public static (uint WaveSpawned, uint WaveCleared) ResolveWaveCounts(
        InvasionMirrorState previous,
        InvasionSnapshotHeader incoming,
        bool isLocalAuthority)
    {
        var spawned = incoming.WaveSpawned;
        var cleared = incoming.WaveCleared;
        if (!isLocalAuthority
            && previous.Wave == (int)incoming.Wave
            && IsRoundActive(incoming.State))
        {
            spawned = Math.Max(previous.WaveSpawned, spawned);
            cleared = Math.Max(previous.WaveCleared, cleared);
        }

        return (spawned, cleared);
    }

    public static bool IsRoundActive(byte state) =>
        state is LiveConstants.InvasionStateSpawning or LiveConstants.InvasionStateCleanup;
}
