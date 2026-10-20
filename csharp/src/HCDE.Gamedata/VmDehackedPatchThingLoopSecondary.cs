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

    public static bool ShouldInstallStates(bool patchedStates) => patchedStates;

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

    public static bool NonSentientNoBlood(bool sentient) => !sentient;

    public static bool ApplyCanUseWalls() => true;

    public static bool FinalizeShadowStyle(bool hadStyle, bool becameShadow) =>
        !hadStyle && becameShadow;

    public static double DefaultShadowAlpha() => 0.5;

    public static bool UseNormalStyleWhenUnshadow(bool hadStyle, bool clearedShadow) =>
        !hadStyle && clearedShadow;

    public static bool IdKeySetsEdnum(int val) => val >= short.MinValue;

    public static bool MassFromVal(int val) => val >= 0;
}
