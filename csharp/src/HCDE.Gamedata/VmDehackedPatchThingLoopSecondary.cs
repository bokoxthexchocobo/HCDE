namespace HCDE.Gamedata;

/// <summary><c>PatchThing</c> Bits/Mass/ID and post-loop finalize (1632–1906) from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingLoopSecondary
{
    public const uint MfTranslation = 0x0c000000u;
    public const int MfTransshift = 26;
    public const uint MfTouchy = 0x10000000u;
    public const uint MfBounces = 0x20000000u;
    public const uint MfFriend = 0x40000000u;
    public const uint MfTranslucent = 0x80000000u;
    public const int MfMissile = 0x200000;
    public const int MfFriendly = 0x40;
    public const int MfShadow = 0x40000;
    public const int MfSpecial = 0x20;
    public const int MfCountkill = 0x800000;
    public const int MfSpawnceiling = 0x2;

    public static bool IsMassKey(string key, int len) =>
        len == 4 && key.Equals("Mass", StringComparison.OrdinalIgnoreCase);

    public static bool IsBitsKey(string key, int len) =>
        len == 4 && key.Equals("Bits", StringComparison.OrdinalIgnoreCase);

    public static bool IsIdKey(string key, int len) =>
        len == 4 && key.Equals("ID #", StringComparison.OrdinalIgnoreCase);

    public static uint ParseBitsToken(string token, uint acc) =>
        uint.TryParse(token, out var v) ? acc | v : acc;

    public static bool StealthKeywordSetsZdoom(string token) =>
        token.Equals("STEALTH", StringComparison.OrdinalIgnoreCase);

    public static bool ApplyTranslationBits(uint value, out uint cleaned, out int translationIndex)
    {
        cleaned = value;
        translationIndex = -1;
        if ((value & MfTranslation) == 0) return false;
        translationIndex = (int)(((value & MfTranslation) >> MfTransshift) - 1u);
        cleaned = value & ~MfTranslation;
        return true;
    }

    public static bool HasTouchy(uint value) => (value & MfTouchy) != 0;

    public static uint ClearTouchy(uint value) => value & ~MfTouchy;

    public static bool HasBounces(uint value) => (value & MfBounces) != 0;

    public static uint ClearBounces(uint value) => value & ~MfBounces;

    public static bool RemapFriend(ref uint value, bool zdoomStealth)
    {
        if (zdoomStealth && (value & 0x40000000) != 0) return true;
        if ((value & MfFriend) == 0) return false;
        value &= ~MfFriend;
        value |= MfFriendly;
        return true;
    }

    public static double ResolveTranslucentAlpha(uint value2)
    {
        if ((value2 & 1) != 0) return 0.25;
        if ((value2 & 2) != 0) return 0.5;
        if ((value2 & 4) != 0) return 0.75;
        return 1.0;
    }

    public static bool HeightPairInvalid(bool hadPhys, bool hadProj) =>
        hadPhys != hadProj;

    public static bool ShouldRestoreCeilingHeight(bool spawnCeiling, bool hadHeight, bool hadPhys, int thingy, int orgHeightsCount) =>
        spawnCeiling && !hadHeight && !hadPhys && thingy > 0 && thingy <= orgHeightsCount;

    public static bool ShadowBecameSet(int oldFlags, int newFlags) =>
        (newFlags & MfShadow) != 0 && (oldFlags & MfShadow) == 0;

    public static bool ShadowCleared(int oldFlags, int newFlags) =>
        (newFlags & MfShadow) == 0 && (oldFlags & MfShadow) != 0;

    public static double NormalizeSpeed(double speed) =>
        Math.Abs(speed) >= 256 ? speed / 65536.0 : speed;

    public static bool ShouldPushTouchedActor(int flags) => (flags & MfSpecial) != 0;

    public static bool ApplyCountKillFlags(int flags, bool isPlayer) =>
        !isPlayer && (flags & MfCountkill) != 0;

    public static bool ClearCountKillFlags(int flags, bool isPlayer) =>
        !isPlayer && (flags & MfCountkill) == 0;

public static bool PlayerAlwaysFriendly(int thingy) => thingy == 1;

    public static int MergePlayerFriendlyFlags(int value0, int thingy) =>
        thingy == 1 ? value0 | MfFriendly : value0;

    public static bool MissileSetsNoteleport(int value0, int flags2) =>
        (value0 & MfMissile) != 0;

    public static bool ClearNoteleportWhenUnmissiled(int flags, int flags2, int value0) =>
        (flags & MfMissile) != 0 && (flags2 & 0x8000000) != 0 && (value0 & MfMissile) == 0;

    public static double GravityQuarter() => 0.25;

    public static bool IceDamageType(uint value1) => (value1 & 0x20000000) != 0;

    public static bool FireDamageType(uint value1) => (value1 & 0x10000000) != 0;

    public static bool DoomCompatBounce(uint value1) => (value1 & 0x4) != 0;

    public static bool SetInvisibleRender(uint value2) => (value2 & 8) != 0;

    public static bool ClearInvisibleRender(uint value2) => (value2 & 8) == 0;

    public static bool GrenadeTrailSwap(bool wasRocket, bool notMissile, bool rocketFx) =>
        wasRocket && notMissile && rocketFx;

    public static double DefaultBounceFactor() => 0.45;

    public static double DefaultWallBounceFactor() => 0.85;


public static bool FinalizeShadowStyle(bool hadStyle, bool becameShadow) =>
        !hadStyle && becameShadow;

    public static double DefaultShadowAlpha() => 0.5;

    public static bool UseNormalStyleWhenUnshadow(bool hadStyle, bool clearedShadow) =>
        !hadStyle && clearedShadow;

    public static bool IdKeySetsEdnum(int val) => val >= short.MinValue;

    public static bool MassFromVal(int val) => val >= 0;

    public const int Mf6Vulnerable = 0x2000000;
    public const int Mf3Noblockmonst = 0x200000;
    public const int Mf4Forceradiusdmg = 0x400000;
    public const int Mf4Dontharmclass = 0x8000000;
    public const int BounceClassicDeh = 0x11;
    public const int BounceGrenadeDeh = 0x12;
    public const int MfNoblood = 0x8;
    public const uint MfStealth = 0x80000000u;
    public const int Mf2Impact = 0x2000000;
    public const int Mf2Pcross = 0x4000000;
    public const int Mf2Noteleport = 0x8000000;
    public const int FxRocket = 0x1;
    public const int FxGrenade = 0x2;

    public static (int Flags6, int Flags3, int Flags4) ApplyBouncesRemap(int flags6, int flags3, int flags4) =>
        (flags6 | Mf6Vulnerable, flags3 | Mf3Noblockmonst, flags4 | Mf4Forceradiusdmg | Mf4Dontharmclass);

    public static int SelectMissileBounceFlags(bool isMissile) =>
        isMissile ? BounceClassicDeh : BounceGrenadeDeh;

    public static int SwapRocketTrailToGrenade(int effects, bool wasMissile, bool isMissile, bool hasRocketFx) =>
        wasMissile && !isMissile && hasRocketFx ? (effects & ~FxRocket) | FxGrenade : effects;

    public static int ApplyNobloodForNonSentient(int value0, bool sentient) =>
        sentient ? value0 : value0 | MfNoblood;

    public static bool StealthFriendHack(bool zdoomFlags, uint value0) =>
        zdoomFlags && (value0 & MfStealth) != 0;

    public static int ApplyFriendRemap(int value0, bool zdoomStealth)
    {
        if (zdoomStealth) return value0;
        var v = (uint)value0;
        RemapFriend(ref v, false);
        return (int)v;
    }

    public static int ApplyMissileNoteleportFlags(int flags2, bool value0Missile) =>
        value0Missile ? flags2 | Mf2Impact | Mf2Pcross | Mf2Noteleport : flags2;

    public static bool ShouldClearNoteleport(bool infoIsMissile, int flags2, bool newMissile) =>
        infoIsMissile && (flags2 & Mf2Noteleport) != 0 && !newMissile;

    public static int ClearNoteleportFlag(int flags2) => flags2 & ~Mf2Noteleport;

    public static bool TranslucentSetsVchanged2(uint value0) => (value0 & MfTranslucent) != 0;

    public static uint ClearTranslucentBit(uint value0) => value0 & ~MfTranslucent;

    public static uint MarkTranslucentVchanged2(uint value2) => value2 | 2;
}
