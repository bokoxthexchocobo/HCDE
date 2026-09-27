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
        // Ordering belongs to the enclosing snapshot session. Mixing prior maxima
        // with a fresh active count makes retries and authority corrections disagree.
        return (incoming.WaveSpawned, incoming.WaveCleared);
    }

    public static bool IsRoundActive(byte state) =>
        state is LiveConstants.InvasionStateSpawning or LiveConstants.InvasionStateCleanup;
}
