namespace HCDE.Net.Core;

public static class SnapshotChecksumLineSpecPolicy
{
    public static uint ComputeRollingHash(GuestWorldStateStore store) =>
        store.LineSpecRollingHash;

    public static uint MixRecord(uint hash, int lineIndex, int special, bool success)
    {
        hash = SnapshotChecksumMixer.MixU32(hash, unchecked((uint)lineIndex));
        hash = SnapshotChecksumMixer.MixU32(hash, unchecked((uint)special));
        return SnapshotChecksumMixer.MixU32(hash, success ? 1u : 0u);
    }

    public static uint PolishRollingHash(uint lineSpecHash, uint presentationEchoHash)
    {
        if (lineSpecHash == 0 || presentationEchoHash == 0)
            return lineSpecHash;

        return SnapshotChecksumMixer.MixU32(lineSpecHash, presentationEchoHash);
    }

    public static uint PolishRollingHashWithActorDelta(uint lineSpecHash, uint actorDeltaHash)
    {
        if (lineSpecHash == 0 || actorDeltaHash == 0)
            return lineSpecHash;

        return SnapshotChecksumMixer.MixU32(lineSpecHash, actorDeltaHash);
    }
}
