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
}
