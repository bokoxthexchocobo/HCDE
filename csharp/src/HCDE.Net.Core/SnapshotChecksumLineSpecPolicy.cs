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

    public static uint PolishRollingHashWithAuthorityEvent(uint lineSpecHash, uint authorityEventHash)
    {
        if (lineSpecHash == 0 || authorityEventHash == 0)
            return lineSpecHash;

        return SnapshotChecksumMixer.MixU32(lineSpecHash, authorityEventHash);
    }

    public static uint PolishActorDeltaRollingHash(uint actorDeltaHash, uint lineSpecHash)
    {
        if (actorDeltaHash == 0 || lineSpecHash == 0)
            return actorDeltaHash;

        return SnapshotChecksumMixer.MixU32(actorDeltaHash, lineSpecHash);
    }

    public static uint PolishAuthorityEventRollingHash(uint authorityEventHash, uint lineSpecHash)
    {
        if (authorityEventHash == 0 || lineSpecHash == 0)
            return authorityEventHash;

        return SnapshotChecksumMixer.MixU32(authorityEventHash, lineSpecHash);
    }
}
