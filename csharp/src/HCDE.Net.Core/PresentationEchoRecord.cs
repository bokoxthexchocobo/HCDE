namespace HCDE.Net.Core;

public readonly struct PresentationEchoInventoryItem
{
    public PresentationEchoInventoryItem(
        byte flags,
        uint amount,
        ReadOnlySpan<byte> className,
        ReadOnlySpan<ushort> hexenSlots = default)
    {
        Flags = flags;
        Amount = amount;
        ClassName = className.Length == 0 ? Array.Empty<byte>() : className.ToArray();
        if (hexenSlots.Length == 0)
        {
            HexenSlots = Array.Empty<ushort>();
        }
        else
        {
            HexenSlots = hexenSlots.ToArray();
        }
    }

    public byte Flags { get; }
    public uint Amount { get; }
    public byte[] ClassName { get; }
    public ushort[] HexenSlots { get; }

    public bool IsWeapon => (Flags & LiveConstants.PresentationEchoInventoryFlagWeapon) != 0;
    public bool IsArmor => (Flags & LiveConstants.PresentationEchoInventoryFlagArmor) != 0;
}

public readonly struct PresentationEchoPlayerRecord
{
    public PresentationEchoPlayerRecord(
        byte playerNum,
        uint readyWeaponNameIndex,
        uint pendingWeaponNameIndex,
        uint pspriteStateNameIndex,
        short pspriteTics,
        ushort weaponState,
        byte playerState,
        short viewHeight,
        uint pspriteStateOffset,
        ReadOnlySpan<byte> pspriteOwnerName,
        ReadOnlySpan<byte> readyWeaponName,
        byte weaponChangeFlags)
    {
        PlayerNum = playerNum;
        ReadyWeaponNameIndex = readyWeaponNameIndex;
        PendingWeaponNameIndex = pendingWeaponNameIndex;
        PspriteStateNameIndex = pspriteStateNameIndex;
        PspriteTics = pspriteTics;
        WeaponState = weaponState;
        PlayerState = playerState;
        ViewHeight = viewHeight;
        PspriteStateOffset = pspriteStateOffset;
        PspriteOwnerName = pspriteOwnerName.Length == 0 ? Array.Empty<byte>() : pspriteOwnerName.ToArray();
        ReadyWeaponName = readyWeaponName.Length == 0 ? Array.Empty<byte>() : readyWeaponName.ToArray();
        WeaponChangeFlags = weaponChangeFlags;
    }

    public byte PlayerNum { get; }
    public uint ReadyWeaponNameIndex { get; }
    public uint PendingWeaponNameIndex { get; }
    public uint PspriteStateNameIndex { get; }
    public short PspriteTics { get; }
    public ushort WeaponState { get; }
    public byte PlayerState { get; }
    public short ViewHeight { get; }
    public uint PspriteStateOffset { get; }
    public byte[] PspriteOwnerName { get; }
    public byte[] ReadyWeaponName { get; }
    public byte WeaponChangeFlags { get; }
}

public readonly struct PresentationEchoBlock
{
    public PresentationEchoBlock(
        byte? inventoryPlayerSlot,
        PresentationEchoInventoryItem[] inventoryItems,
        PresentationEchoPlayerRecord[] players)
    {
        InventoryPlayerSlot = inventoryPlayerSlot;
        InventoryItems = inventoryItems ?? Array.Empty<PresentationEchoInventoryItem>();
        Players = players ?? Array.Empty<PresentationEchoPlayerRecord>();
    }

    public byte? InventoryPlayerSlot { get; }
    public PresentationEchoInventoryItem[] InventoryItems { get; }
    public PresentationEchoPlayerRecord[] Players { get; }
}
