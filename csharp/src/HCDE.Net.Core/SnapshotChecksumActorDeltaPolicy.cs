namespace HCDE.Net.Core;

public static class SnapshotChecksumActorDeltaPolicy
{
    public static uint ComputeRollingHash(GuestWorldStateStore store) =>
        store.ActorDeltaRollingHash;

    public static uint MixRecord(uint hash, ActorDeltaRecord record)
    {
        hash = SnapshotChecksumMixer.MixU32(hash, record.ActorId);
        hash = SnapshotChecksumMixer.MixU32(hash, record.ClassId);
        hash = SnapshotChecksumMixer.MixU32(hash, record.FieldMask);
        hash = SnapshotChecksumMixer.MixU32(hash, unchecked((uint)record.Health));
        return hash;
    }

    public static uint PolishPresentationEchoRollingHash(uint presentationEchoHash, uint actorDeltaHash)
    {
        if (presentationEchoHash == 0 || actorDeltaHash == 0)
            return presentationEchoHash;

        return SnapshotChecksumMixer.MixU32(presentationEchoHash, actorDeltaHash);
    }
}
