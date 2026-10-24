namespace HCDE.Net.Core;

/// <summary>Net event writers, dynamic buffer, <c>RemoveClass</c>, <c>SelectWeapon</c>, <c>UseFlechette</c> (<c>d_net.cpp</c> 8269–8510).</summary>
public static class VmDNetNetEventWrite
{
    public const int GameStateLevel = 1;
    public const int PlayerStateLive = 0;
    public const int WeaponNameTagBit = 2;
    public const int NumWeaponSlots = 10;
    public const int WstNext = -1;
    public const int WstPrev = -2;

    public static void NetAdoptServerRoomIdNative(
        bool localAuthority,
        int room,
        ref int pendingAdoptRoomId)
    {
        if (localAuthority || room < 0 || room > 255)
            return;
        pendingAdoptRoomId = room;
    }

    public static void NetWriteInt8Native(List<byte> sink, byte value) => sink.Add(value);

    public static void NetWriteInt16Native(List<byte> sink, short value)
    {
        sink.Add((byte)(value >> 8));
        sink.Add((byte)value);
    }

    public static void NetWriteInt32Native(List<byte> sink, int value)
    {
        sink.Add((byte)(value >> 24));
        sink.Add((byte)(value >> 16));
        sink.Add((byte)(value >> 8));
        sink.Add((byte)value);
    }

    public static void NetWriteInt64Native(List<byte> sink, long value)
    {
        for (var shift = 56; shift >= 0; shift -= 8)
            sink.Add((byte)(value >> shift));
    }

    public static void NetWriteFloatNative(List<byte> sink, float value) =>
        NetWriteInt32Native(sink, BitConverter.SingleToInt32Bits(value));

    public static void NetWriteDoubleNative(List<byte> sink, double value) =>
        NetWriteInt64Native(sink, BitConverter.DoubleToInt64Bits(value));

    public static void NetWriteStringNative(List<byte> sink, string value)
    {
        foreach (var ch in value)
            sink.Add((byte)ch);
        sink.Add(0);
    }

    public static void NetWriteBytesNative(List<byte> sink, ReadOnlySpan<byte> block, int len)
    {
        var remaining = len;
        var index = 0;
        while (remaining-- > 0)
            sink.Add(block[index++]);
    }

    public static int RemoveClassNative(
        IEnumerable<RemoveClassActorSnapshot> actors,
        Func<RemoveClassActorSnapshot, bool> isKindOf,
        Action<RemoveClassActorSnapshot>? onDestroy,
        Action<string>? printCannotRemoveLivePlayers,
        out bool sawLivePlayer)
    {
        var removeCount = 0;
        sawLivePlayer = false;
        foreach (var snap in actors)
        {
            if (!isKindOf(snap))
                continue;
            if (snap.HasPlayer)
            {
                sawLivePlayer = true;
                continue;
            }
            if (!snap.IsMapActor)
                continue;
            removeCount++;
            onDestroy?.Invoke(snap);
        }

        if (sawLivePlayer)
            printCannotRemoveLivePlayers?.Invoke("Cannot remove live players!\n");

        return removeCount;
    }

    public static void SelectWeaponNative(
        int player,
        int consolePlayer,
        int slot,
        int gameState,
        bool paused,
        int playerState,
        object? mobj,
        int dmflags2,
        int df2DontCheckAmmo,
        int displayNameTags,
        int nameTagColor,
        object? statusBar,
        object? smallFont,
        object? readyWeapon,
        Func<int, int, bool, object?>? pickWeaponFromSlot,
        Func<object?, object?>? pickNextWeapon,
        Func<object?, object?>? pickPrevWeapon,
        Func<object?, uint, object?>? findInventoryById,
        Func<object?, uint>? getInventoryId,
        Func<object?, string>? getActorTag,
        Action<object?, object?>? useInventory,
        Action<object?, string>? playWeaponChangeSound,
        Action<object?, string, int>? attachWeaponNameHud)
    {
        if (mobj is null || gameState != GameStateLevel || paused || playerState != PlayerStateLive)
            return;

        object? weap = null;
        if (slot >= 0 && slot < NumWeaponSlots)
            weap = pickWeaponFromSlot?.Invoke(slot, dmflags2, (dmflags2 & df2DontCheckAmmo) == 0);
        else if (slot == WstNext)
            weap = pickNextWeapon?.Invoke(mobj);
        else if (slot == WstPrev)
            weap = pickPrevWeapon?.Invoke(mobj);

        if (weap is null)
            return;

        var id = getInventoryId?.Invoke(weap) ?? 0;
        var invItem = findInventoryById?.Invoke(mobj, id);
        if (!ReferenceEquals(invItem, weap))
            return;

        if (player == consolePlayer)
        {
            if (!ReferenceEquals(weap, readyWeapon))
                playWeaponChangeSound?.Invoke(mobj, "misc/weaponchange");

            if ((displayNameTags & WeaponNameTagBit) != 0 && statusBar is not null && smallFont is not null)
                attachWeaponNameHud?.Invoke(statusBar, getActorTag?.Invoke(weap) ?? "", nameTagColor);
        }

        useInventory?.Invoke(mobj, weap);
    }

    public static void UseFlechetteNative(
        int gameState,
        bool paused,
        int playerState,
        object? mobj,
        Func<object?, object?>? getFlechetteItem,
        Func<object?, uint, object?>? findInventoryById,
        Func<object?, uint>? getInventoryId,
        Action<object?, object?>? useInventory)
    {
        if (mobj is null || gameState != GameStateLevel || paused || playerState != PlayerStateLive)
            return;

        var item = getFlechetteItem?.Invoke(mobj);
        if (item is null)
            return;

        var id = getInventoryId?.Invoke(item) ?? 0;
        var invItem = findInventoryById?.Invoke(mobj, id);
        if (ReferenceEquals(invItem, item))
            useInventory?.Invoke(mobj, item);
    }
}

public readonly struct RemoveClassActorSnapshot(bool hasPlayer, bool isMapActor)
{
    public bool HasPlayer { get; } = hasPlayer;
    public bool IsMapActor { get; } = isMapActor;
}
