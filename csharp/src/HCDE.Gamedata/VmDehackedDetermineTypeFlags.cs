namespace HCDE.Gamedata;

/// <summary>Whole-function ports from <c>d_dehacked.cpp</c> DetermineType and PatchThing flag handlers (3969–4209).</summary>
public static class VmDehackedDetermineTypeFlags
{
    public const ulong MfNosector = 1UL << 24;
    public const ulong MfNoblockmap = 1UL << 25;
    public const ulong MfCountkill = 1UL << 22;
    public const ulong MfCountitem = 1UL << 23;
    public const ulong MfFriendly = 1UL << 29;
    public const ulong MfMissile = 1UL << 0;
    public const ulong Mf2Boss = 1UL << 31;
    public const ulong Mf2Noteleport = 1UL << 0;
    public const ulong Mf2Pcross = 1UL << 1;
    public const ulong Mf2Impact = 1UL << 2;
    public const ulong Mf3Noblockmonst = 1UL << 0;
    public const ulong Mf3Noradiusdmg = 1UL << 1;
    public const ulong Mf3Fullvoldeath = 1UL << 2;
    public const ulong Mf4Forceradiusdmg = 1UL << 0;
    public const ulong Mf4Dontharmclass = 1UL << 1;
    public const ulong Mf6Vulnerable = 1UL << 0;
    public const ulong Mf8Fullvolsee = 1UL << 0;
    public const uint FxRocket = 1U;
    public const uint BounceDeh = 1U;
    public const uint BounceClassicDeh = 3U;
    public const uint BounceGrenadeDeh = 5U;
    public const uint RfZdoomtrans = 1U;

    public static int DetermineTypeBinarySearch(int min, int max, ReadOnlySpan<byte> spriteName, IReadOnlyList<ReadOnlyMemory<byte>> mappings)
    {
        while (min <= max)
        {
            var mid = (min + max) / 2;
            var lex = CompareSpriteNames(mappings[mid].Span, spriteName);
            if (lex == 0)
                return mid;
            if (lex < 0)
                min = mid + 1;
            else
                max = mid - 1;
        }

        return -1;
    }

    public static int CompareSpriteNames(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        for (var i = 0; i < 4; i++)
        {
            var l = i < left.Length ? left[i] : (byte)0;
            var r = i < right.Length ? right[i] : (byte)0;
            if (l != r)
                return l.CompareTo(r);
        }

        return 0;
    }

    public static ulong SetNoSector(ulong flags) => flags | MfNosector;

    public static ulong ClearNoSector(ulong flags) => flags & ~MfNosector;

    public static ulong SetNoBlockmap(ulong flags) => flags | MfNoblockmap;

    public static ulong ClearNoBlockmap(ulong flags) => flags & ~MfNoblockmap;

    public static (ulong Flags, int TotalMonsters) SetCountkill(bool countsAsKill, int health, int totalMonsters, ulong flags)
    {
        if (countsAsKill && health > 0)
            totalMonsters--;
        flags |= MfCountkill;
        if (countsAsKill && health > 0)
            totalMonsters++;
        return (flags, totalMonsters);
    }

    public static (ulong Flags, int TotalMonsters) ClearCountkill(bool countsAsKill, int health, int totalMonsters, ulong flags)
    {
        if (countsAsKill && health > 0)
            totalMonsters--;
        flags &= ~MfCountkill;
        if (countsAsKill && health > 0)
            totalMonsters++;
        return (flags, totalMonsters);
    }

    public static (ulong Flags2, ulong Flags3) SetBoss(ulong flags2, ulong flags3) =>
        (flags2 | Mf2Boss, flags3 | Mf3Noradiusdmg);

    public static (ulong Flags2, ulong Flags3) ClearBoss(ulong flags2, ulong flags3) =>
        (flags2 & ~Mf2Boss, flags3 & ~Mf3Noradiusdmg);

    public static (ulong Flags, int TotalItems) SetCountitem(bool hadCountItem, int totalItems, ulong flags)
    {
        if (!hadCountItem)
        {
            flags |= MfCountitem;
            totalItems++;
        }

        return (flags, totalItems);
    }

    public static (ulong Flags, int TotalItems) ClearCountitem(bool hadCountItem, int totalItems, ulong flags)
    {
        if (hadCountItem)
        {
            flags &= ~MfCountitem;
            totalItems--;
        }

        return (flags, totalItems);
    }

    public static (ulong Flags, ulong Flags3, int TotalMonsters) SetFriendly(
        bool countsAsKill,
        int health,
        int totalMonsters,
        ulong flags,
        ulong flags3)
    {
        if (countsAsKill && health > 0)
            totalMonsters--;
        flags |= MfFriendly;
        flags3 |= Mf3Noblockmonst;
        if (countsAsKill && health > 0)
            totalMonsters++;
        return (flags, flags3, totalMonsters);
    }

    public static (ulong Flags, ulong Flags3, int TotalMonsters) ClearFriendly(
        bool countsAsKill,
        int health,
        int totalMonsters,
        ulong flags,
        ulong flags3)
    {
        if (countsAsKill && health > 0)
            totalMonsters--;
        flags &= ~MfFriendly;
        flags3 &= ~Mf3Noblockmonst;
        if (countsAsKill && health > 0)
            totalMonsters++;
        return (flags, flags3, totalMonsters);
    }

    public static (ulong Flags8, ulong Flags3) SetFullVol(ulong flags8, ulong flags3) =>
        (flags8 | Mf8Fullvolsee, flags3 | Mf3Fullvoldeath);

    public static (ulong Flags8, ulong Flags3) ClearFullVol(ulong flags8, ulong flags3) =>
        (flags8 & ~Mf8Fullvolsee, flags3 & ~Mf3Fullvoldeath);

    public static (double Alpha, uint RenderFlags) SetTranslucent(uint renderFlags) =>
        (0.5, renderFlags & ~RfZdoomtrans);

    public static (double Alpha, uint RenderFlags) ClearTranslucent(uint renderFlags) =>
        (1.0, renderFlags & ~RfZdoomtrans);

    public static bool CheckTranslucent(uint renderFlags, double alpha, double epsilon = 1e-6) =>
        (renderFlags & RfZdoomtrans) == 0 && alpha < 1.0 - epsilon;

    public static int SetTranslation1(int current, int t1, int t2, int t3) =>
        current == t2 || current == t3 ? t3 : t1;

    public static int ClearTranslation1(int current, int t2, int t3, int noTranslation) =>
        current == t3 || current == t2 ? t2 : noTranslation;

    public static bool CheckTranslation1(int current, int t1, int t3) =>
        current == t1 || current == t3;

    public static int SetTranslation2(int current, int t1, int t2, int t3) =>
        current == t1 || current == t3 ? t3 : t2;

    public static int ClearTranslation2(int current, int t1, int t3, int noTranslation) =>
        current == t3 || current == t1 ? t1 : noTranslation;

    public static bool CheckTranslation2(int current, int t2, int t3) =>
        current == t2 || current == t3;

    public static (ulong F6, ulong F3, ulong F4, uint Effects, uint BounceFlags) SetBounces(
        ulong flags6,
        ulong flags3,
        ulong flags4,
        uint effects,
        bool isMissile)
    {
        flags6 |= Mf6Vulnerable;
        flags3 |= Mf3Noblockmonst;
        flags4 |= Mf4Forceradiusdmg | Mf4Dontharmclass;
        effects &= ~FxRocket;
        var bounce = isMissile ? BounceClassicDeh : BounceGrenadeDeh;
        return (flags6, flags3, flags4, effects, bounce);
    }

    public static (ulong F6, ulong F3, ulong F4, uint BounceFlags) ClearBounces(ulong flags6, ulong flags3, ulong flags4) =>
        (flags6 & ~Mf6Vulnerable, flags3 & ~Mf3Noblockmonst, flags4 & ~(Mf4Forceradiusdmg | Mf4Dontharmclass), 0U);

    public static (ulong Flags, ulong Flags2, uint BounceFlags) SetMissile(ulong flags, ulong flags2, uint bounceFlags)
    {
        flags |= MfMissile;
        flags2 |= Mf2Noteleport | Mf2Pcross | Mf2Impact;
        if ((bounceFlags & BounceDeh) != 0)
            bounceFlags = BounceClassicDeh;
        return (flags, flags2, bounceFlags);
    }

    public static (ulong Flags, ulong Flags2) ClearMissile(ulong flags, ulong flags2)
    {
        flags &= ~MfMissile;
        flags2 &= ~(Mf2Noteleport | Mf2Pcross | Mf2Impact);
        return (flags, flags2);
    }
}
