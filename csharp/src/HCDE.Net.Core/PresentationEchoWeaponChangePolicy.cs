using System.Text;

namespace HCDE.Net.Core;

public readonly struct PresentationEchoLastState
{
    public PresentationEchoLastState(
        ReadOnlySpan<byte> readyWeaponName,
        ReadOnlySpan<byte> pspriteOwnerName,
        uint pspriteStateOffset,
        ushort weaponState,
        bool initialized)
    {
        ReadyWeaponName = readyWeaponName.Length == 0 ? Array.Empty<byte>() : readyWeaponName.ToArray();
        PspriteOwnerName = pspriteOwnerName.Length == 0 ? Array.Empty<byte>() : pspriteOwnerName.ToArray();
        PspriteStateOffset = pspriteStateOffset;
        WeaponState = weaponState;
        Initialized = initialized;
    }

    public byte[] ReadyWeaponName { get; }
    public byte[] PspriteOwnerName { get; }
    public uint PspriteStateOffset { get; }
    public ushort WeaponState { get; }
    public bool Initialized { get; }
}

public static class PresentationEchoWeaponChangePolicy
{
    public static byte ComputeWeaponChangeFlags(
        in PresentationEchoLastState lastState,
        ReadOnlySpan<byte> readyWeaponName,
        ReadOnlySpan<byte> pspriteOwnerName,
        uint pspriteStateOffset,
        ushort weaponState)
    {
        if (!lastState.Initialized)
            return (byte)(LiveConstants.WeaponChangeReadyClass | LiveConstants.WeaponChangeForceReseat);

        var flags = (byte)0;
        if (!readyWeaponName.SequenceEqual(lastState.ReadyWeaponName))
            flags |= LiveConstants.WeaponChangeReadyClass;

        var forceReseat = (flags & LiveConstants.WeaponChangeReadyClass) != 0;
        if (!forceReseat
            && !pspriteOwnerName.IsEmpty
            && readyWeaponName.SequenceEqual(pspriteOwnerName)
            && !pspriteOwnerName.SequenceEqual(lastState.PspriteOwnerName))
        {
            forceReseat = true;
        }

        if (!forceReseat
            && (weaponState & LiveConstants.WeaponStateReady) == 0
            && (lastState.WeaponState & LiveConstants.WeaponStateReady) != 0)
        {
            forceReseat = true;
        }

        if (!forceReseat
            && !pspriteOwnerName.IsEmpty
            && !pspriteOwnerName.SequenceEqual(lastState.PspriteOwnerName)
            && pspriteStateOffset != lastState.PspriteStateOffset)
        {
            forceReseat = true;
        }

        if (forceReseat)
            flags |= LiveConstants.WeaponChangeForceReseat;

        return flags;
    }

    public static PresentationEchoLastState CreateLastState(PresentationEchoPlayerRecord player) =>
        new(
            player.ReadyWeaponName,
            player.PspriteOwnerName,
            player.PspriteStateOffset,
            player.WeaponState,
            initialized: true);
}
