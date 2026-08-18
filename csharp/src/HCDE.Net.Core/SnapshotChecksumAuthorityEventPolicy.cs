namespace HCDE.Net.Core;

public static class SnapshotChecksumAuthorityEventPolicy
{
    public static uint ComputeRollingHash(GuestWorldStateStore store)
    {
        return store.AuthorityEventRollingHash;
    }

    public static uint MixRecord(uint hash, AuthorityEventRecord record)
    {
        hash = SnapshotChecksumMixer.MixU32(hash, record.ActorId);
        hash = SnapshotChecksumMixer.MixU32(hash, record.ClassId);
        hash = SnapshotChecksumMixer.MixU32(hash, unchecked((uint)record.Health));
        return hash;
    }

    public static uint PolishActorDeltaRollingHash(uint actorDeltaHash, uint authorityEventHash)
    {
        if (actorDeltaHash == 0 || authorityEventHash == 0)
            return actorDeltaHash;

        return SnapshotChecksumMixer.MixU32(actorDeltaHash, authorityEventHash);
    }

    public static uint PolishPresentationEchoRollingHash(uint presentationEchoHash, uint authorityEventHash)
    {
        if (presentationEchoHash == 0 || authorityEventHash == 0)
            return presentationEchoHash;

        return SnapshotChecksumMixer.MixU32(presentationEchoHash, authorityEventHash);
    }
}
