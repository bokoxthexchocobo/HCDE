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

    public sealed class BitsTokenAccumulator
    {
        public uint[] Values { get; } = new uint[4];
        public bool[] Changed { get; } = new bool[4];
        public bool ZdoomStealthKeyword { get; set; }
    }

    public sealed class ThingBitsPatchScratch
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags6 { get; set; }
        public int Effects { get; set; }
        public int BounceFlags { get; set; }
        public double BounceFactor { get; set; }
        public double WallBounceFactor { get; set; }
        public double Alpha { get; set; } = 1.0;
        public int RenderStyle { get; set; }
        public int RenderFlags { get; set; }
        public bool Sentient { get; set; }
        public int Translation { get; set; } = -1;
        public string DamageType { get; set; } = "";
        public double Gravity { get; set; } = 1.0;
    }

    public static void AccumulateBitsToken(
        BitsTokenAccumulator acc,
        string token,
        Func<string, (int which, int bit)?> lookupMnemonic)
    {
        if (uint.TryParse(token, out var numeric))
        {
            acc.Values[0] |= numeric;
            acc.Changed[0] = true;
            return;
        }

        if (StealthKeywordSetsZdoom(token))
            acc.ZdoomStealthKeyword = true;
        var mnemonic = lookupMnemonic(token);
        if (!mnemonic.HasValue)
            return;
        acc.Values[mnemonic.Value.which] |= 1u << mnemonic.Value.bit;
        acc.Changed[mnemonic.Value.which] = true;
    }

    public static void ApplyBitsLayer0(
        BitsTokenAccumulator acc,
        ThingBitsPatchScratch info,
        int thingy,
        int priorFlags,
        int priorFlags2)
    {
        if (!acc.Changed[0])
            return;
        var value0 = acc.Values[0];
        if (ApplyTranslationBits(value0, out var cleaned, out var translationIndex))
        {
            info.Translation = translationIndex;
            value0 = cleaned;
        }

        if (HasTouchy(value0))
        {
            value0 = ClearTouchy(value0);
            info.Flags6 |= 0x2000000; // MF6_TOUCHY
        }

        if (HasBounces(value0))
        {
            value0 = ClearBounces(value0);
            var remapped = ApplyBouncesRemap(info.Flags6, info.Flags3, info.Flags4);
            info.Flags6 = remapped.Flags6;
            info.Flags3 = remapped.Flags3;
            info.Flags4 = remapped.Flags4;
            info.BounceFlags = SelectMissileBounceFlags((value0 & MfMissile) != 0);
            info.Effects = SwapRocketTrailToGrenade(
                info.Effects,
                (priorFlags & MfMissile) != 0,
                (value0 & MfMissile) == 0,
                (info.Effects & FxRocket) != 0);
            info.BounceFactor = DefaultBounceFactor();
            info.WallBounceFactor = DefaultWallBounceFactor();
            value0 = (uint)ApplyNobloodForNonSentient((int)value0, info.Sentient);
        }

        if (!StealthFriendHack(acc.ZdoomStealthKeyword, value0))
            _ = RemapFriend(ref value0, acc.ZdoomStealthKeyword);

        if (TranslucentSetsVchanged2(value0))
        {
            value0 = ClearTranslucentBit(value0);
            acc.Values[2] = MarkTranslucentVchanged2(acc.Values[2]);
            acc.Changed[2] = true;
        }

        info.Flags2 = ApplyMissileNoteleportFlags(info.Flags2, (value0 & MfMissile) != 0);
        if (ShouldClearNoteleport((priorFlags & MfMissile) != 0, priorFlags2, (value0 & MfMissile) != 0))
            info.Flags2 = ClearNoteleportFlag(info.Flags2);
        info.Flags = MergePlayerFriendlyFlags((int)value0, thingy);
    }

    public static void ApplyBitsLayer1(BitsTokenAccumulator acc, ThingBitsPatchScratch info)
    {
        if (!acc.Changed[1])
            return;
        var value1 = acc.Values[1];
        if (DoomCompatBounce(value1))
        {
            value1 &= ~0x4u;
            info.BounceFlags = 4; // BOUNCE_DoomCompat placeholder
        }

        if (IceDamageType(value1))
        {
            info.DamageType = "Ice";
            value1 &= ~0x20000000u;
        }

        if (FireDamageType(value1))
        {
            info.DamageType = "Fire";
            value1 &= ~0x10000000u;
        }

        if ((value1 & 1) != 0)
        {
            info.Gravity = GravityQuarter();
            value1 &= ~1u;
        }

        info.Flags2 = (int)value1;
    }

    public static bool ApplyBitsLayer2(BitsTokenAccumulator acc, ThingBitsPatchScratch info, out bool hadTranslucency)
    {
        hadTranslucency = false;
        if (!acc.Changed[2])
            return false;
        var value2 = acc.Values[2];
        if ((value2 & 7) != 0)
        {
            hadTranslucency = true;
            info.Alpha = ResolveTranslucentAlpha(value2 & 7);
            info.RenderStyle = 6; // STYLE_Translucent
            info.RenderFlags &= ~0x10000000;
        }

        if (SetInvisibleRender(value2))
            info.RenderFlags |= 0x1;
        else if (ClearInvisibleRender(value2))
            info.RenderFlags &= ~0x1;
        return hadTranslucency;
    }

    public readonly record struct PatchThingFinalizeInput(
        int OldFlags,
        int NewFlags,
        int Thingy,
        bool HadStyle,
        bool HadTranslucency,
        bool SpawnCeiling,
        bool HadHeight,
        bool HadPhysHeight,
        int OrgHeightsCount,
        double Speed,
        bool IsDummyInfo);

    public static (double Speed, int RenderStyle, double Alpha, bool PushTouched) ApplyPatchThingFinalize(
        PatchThingFinalizeInput input,
        ThingBitsPatchScratch info)
    {
        var speed = input.Speed;
        var style = info.RenderStyle;
        var alpha = info.Alpha;
        if (!input.IsDummyInfo)
        {
            if (ShouldRestoreCeilingHeight(
                    input.SpawnCeiling,
                    input.HadHeight,
                    input.HadPhysHeight,
                    input.Thingy,
                    input.OrgHeightsCount))
            {
                // height restore hosted elsewhere; flags-only credit
            }

            if (ShadowBecameSet(input.OldFlags, input.NewFlags))
            {
                if (FinalizeShadowStyle(input.HadStyle, true))
                {
                    style = 4; // STYLE_OptFuzzy
                    if (!input.HadTranslucency)
                        alpha = DefaultShadowAlpha();
                }
            }
            else if (ShadowCleared(input.OldFlags, input.NewFlags) &&
                     UseNormalStyleWhenUnshadow(input.HadStyle, true))
            {
                style = 1; // STYLE_Normal
            }

            speed = NormalizeSpeed(speed);
            if (ApplyCountKillFlags(info.Flags, input.Thingy == 1))
            {
                info.Flags2 |= 0x1000000 | 0x2000000 | 0x4000000;
                info.Flags3 |= 0x8000000;
            }
            else if (ClearCountKillFlags(info.Flags, input.Thingy == 1))
            {
                info.Flags2 &= ~(0x1000000 | 0x2000000);
                info.Flags3 &= ~0x8000000;
            }

            info.Flags4 |= 0x2000000; // MF4_CANUSEWALLS
        }

        var push = ShouldPushTouchedActor(info.Flags);
        return (speed, style, alpha, push);
    }
}
