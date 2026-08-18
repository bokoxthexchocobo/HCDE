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
}
