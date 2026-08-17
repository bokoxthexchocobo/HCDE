namespace HCDE.Net.Core;

public sealed class GuestPresentationEchoState : IPresentationEchoApplySink
{
    public PresentationEchoInventoryItem[]? LastInventoryItems { get; private set; }
    public byte? LastInventoryPlayerSlot { get; private set; }
    public PresentationEchoPlayerRecord? LastWeaponPlayer { get; private set; }

    public bool ReconcileInventory(byte playerSlot, PresentationEchoInventoryItem[] items)
    {
        LastInventoryPlayerSlot = playerSlot;
        LastInventoryItems = items.ToArray();
        return true;
    }

    public bool FollowWeapon(PresentationEchoPlayerRecord player)
    {
        LastWeaponPlayer = player;
        return true;
    }
}
