namespace HCDE.Gamedata;

public sealed class VmDehackedThingBitWords
{
    public uint Flags0 { get; set; }
    public uint Flags1 { get; set; }
    public uint Flags2 { get; set; }
    public bool Changed0 { get; set; }
    public bool Changed1 { get; set; }
    public bool Changed2 { get; set; }
    public string? BounceStyle { get; set; }
}

/// <summary><c>PatchThing</c> <c>Bits</c> mnemonic parsing from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingBits
{
    public const uint MfTouchy = 0x10000000;
    public const uint MfBounces = 0x20000000;
    public const uint MfFriend = 0x40000000;
    public const uint MfTranslucent = 0x80000000;
    public const uint MfMissile = 0x00000020;

    public static void ApplyMnemonic(string mnemonic, IReadOnlyList<(string Name, int Word, int Bit)> bitNames, VmDehackedThingBitWords words)
    {
        foreach (var bit in bitNames)
        {
            if (!bit.Name.Equals(mnemonic, StringComparison.OrdinalIgnoreCase)) continue;
            switch (bit.Word)
            {
                case 0: words.Flags0 |= 1u << bit.Bit; words.Changed0 = true; break;
                case 1: words.Flags1 |= 1u << bit.Bit; words.Changed1 = true; break;
                case 2: words.Flags2 |= 1u << bit.Bit; words.Changed2 = true; break;
            }

            return;
        }
    }

    public static void ParseTokenList(string line, IReadOnlyList<(string Name, int Word, int Bit)> bitNames, VmDehackedThingBitWords words)
    {
        foreach (var token in line.Split([',', '|', '+', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (uint.TryParse(token, out var numeric))
            {
                words.Flags0 |= numeric;
                words.Changed0 = true;
                continue;
            }

            ApplyMnemonic(token, bitNames, words);
        }
    }

    public static void RemapMbfFlags(VmDehackedThingBitWords words, bool isMissile, bool isSentient)
    {
        if ((words.Flags0 & MfTouchy) != 0)
        {
            words.Flags0 &= ~MfTouchy;
            words.Flags1 |= 1u << 6;
            words.Changed1 = true;
        }

        if ((words.Flags0 & MfBounces) != 0)
        {
            words.Flags0 &= ~MfBounces;
            words.Flags1 |= 1u << 7;
            words.Changed1 = true;
            if (!isSentient) words.Flags0 |= 1u << 3;
            words.BounceStyle = isMissile ? "Classic" : "Grenade";
        }

        if ((words.Flags0 & MfFriend) != 0)
        {
            words.Flags0 &= ~MfFriend;
            words.Flags0 |= 1u << 29;
        }

        if ((words.Flags0 & MfTranslucent) != 0)
        {
            words.Flags0 &= ~MfTranslucent;
            words.Flags2 |= 2;
            words.Changed2 = true;
        }
    }

    public static double? ResolveTranslucentAlpha(uint flags2)
    {
        if ((flags2 & 1) != 0) return 0.25;
        if ((flags2 & 2) != 0) return 0.5;
        if ((flags2 & 4) != 0) return 0.75;
        return null;
    }

    public static void ApplyMissileCompatFlags(VmDehackedThingBitWords words, bool hadMissile, bool removingMissile)
    {
        if ((words.Flags0 & MfMissile) != 0)
        {
            words.Flags1 |= 0x0000000e;
            words.Changed1 = true;
        }

        if (hadMissile && removingMissile)
            words.Flags1 &= ~0x00000008u;
    }

    public static void ApplyFlags1Word(uint value1, VmDehackedThingPatchTarget target, VmDehackedThingBitWords words)
    {
        if ((value1 & 0x00000004) != 0)
            target.BounceFlags = 1;
        if ((value1 & 0x20000000) != 0) target.DamageType = "Ice";
        if ((value1 & 0x10000000) != 0) target.DamageType = "Fire";
        if ((value1 & 0x00000001) != 0) target.Gravity = 0.25;
        words.Flags1 = (words.Flags1 & ~value1) | value1;
        words.Changed1 = true;
    }

    public static void ApplyTranslucentRender(uint flags2, VmDehackedThingPatchTarget target)
    {
        var alpha = ResolveTranslucentAlpha(flags2 & 7);
        if (alpha is null) return;
        target.Alpha = alpha.Value;
        target.RenderStyle = 1;
    }

    public static uint ApplyPlayerFriendly(uint flags0, bool isPlayer) =>
        isPlayer ? flags0 | (1u << 29) : flags0;
}
