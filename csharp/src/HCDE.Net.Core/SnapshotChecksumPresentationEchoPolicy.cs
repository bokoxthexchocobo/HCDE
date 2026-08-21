namespace HCDE.Net.Core;

public static class SnapshotChecksumPresentationEchoPolicy
{
    public static uint ComputeRollingHash(GuestWorldStateStore store) =>
        store.PresentationEchoRollingHash;

    public static uint MixBlock(uint hash, PresentationEchoBlock block)
    {
        hash = SnapshotChecksumMixer.MixU32(hash, (uint)block.Players.Length);
        hash = SnapshotChecksumMixer.MixU32(hash, (uint)block.InventoryItems.Length);
        if (block.InventoryPlayerSlot is { } inventorySlot)
            hash = SnapshotChecksumMixer.MixU32(hash, inventorySlot);

        foreach (var item in block.InventoryItems)
        {
            hash = SnapshotChecksumMixer.MixU32(hash, item.Flags);
            hash = SnapshotChecksumMixer.MixU32(hash, item.Amount);
        }

        foreach (var player in block.Players)
        {
            hash = SnapshotChecksumMixer.MixU32(hash, player.PlayerNum);
            hash = SnapshotChecksumMixer.MixU32(hash, player.WeaponChangeFlags);
            hash = SnapshotChecksumMixer.MixU32(hash, player.WeaponState);
        }

        return hash;
    }

    public static uint PolishRollingHash(uint echoHash, uint lineSpecHash)
    {
        if (echoHash == 0 || lineSpecHash == 0)
            return echoHash;

        return SnapshotChecksumMixer.MixU32(echoHash, lineSpecHash);
    }

    public static uint PolishRollingHashWithAuthorityEvent(uint echoHash, uint authorityEventHash)
    {
        if (echoHash == 0 || authorityEventHash == 0)
            return echoHash;

        return SnapshotChecksumMixer.MixU32(echoHash, authorityEventHash);
    }

    public static uint PolishActorDeltaRollingHash(uint actorDeltaHash, uint presentationEchoHash)
    {
        if (actorDeltaHash == 0 || presentationEchoHash == 0)
            return actorDeltaHash;

        return SnapshotChecksumMixer.MixU32(actorDeltaHash, presentationEchoHash);
    }
}
