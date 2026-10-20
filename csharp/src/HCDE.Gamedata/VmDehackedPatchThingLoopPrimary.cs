namespace HCDE.Gamedata;

/// <summary><c>PatchThing</c> setup and main-loop dispatch (lines 1234–1631) from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingLoopPrimary
{
    public const long IntMin = int.MinValue;
    public const long UintMax = uint.MaxValue;

    public static bool IsNumericInRange(long val) => val >= IntMin && val <= UintMax;

    public static bool IsHitPointsKey(string key, int len) =>
        len == 10 && key.Equals("Hit points", StringComparison.OrdinalIgnoreCase);

    public static bool IsReactionTimeKey(string key, int len) =>
        len == 13 && key.Equals("Reaction time", StringComparison.OrdinalIgnoreCase);

    public static bool IsTranslucencyKey(string key, int len) =>
        len == 12 && key.Equals("Translucency", StringComparison.OrdinalIgnoreCase);

    public static bool IsHeightKey(string key, int len) =>
        len == 6 && key.Equals("Height", StringComparison.OrdinalIgnoreCase);

    public static bool IsMissileDamageKey(string key, int len) =>
        len == 14 && key.Equals("Missile damage", StringComparison.OrdinalIgnoreCase);

    public static bool IsInfightingGroupKey(string key, int len) =>
        len == 16 && key.Equals("infighting group", StringComparison.OrdinalIgnoreCase);

    public static int ClampInfightingGroup(long val)
    {
        if (val < 0) return 0;
        return (int)val;
    }

    public static int NormalizeProjectileGroup(long val) => val < 0 ? -1 : (int)val;

    public static int ClampSplashGroup(long val) => val < 0 ? 0 : (int)val;

    public static double FastSpeedFromVal(long val) =>
        val >= 256 ? val / 65536.0 : val;

    public static double MeleeRangeFromDeh(long val) => val - 20;

    public static bool LineEndsWithFrame(string key, int len) =>
        len > 6 && key.EndsWith(" frame", StringComparison.OrdinalIgnoreCase);

    public static bool LineEndsWithSound(string key, int len) =>
        len > 6 && key.EndsWith(" sound", StringComparison.OrdinalIgnoreCase);

    public static string? MapFrameLabelPrefix(string key)
    {
        if (!LineEndsWithFrame(key, key.Length)) return null;
        if (key.StartsWith("Initial", StringComparison.OrdinalIgnoreCase)) return "Spawn";
        if (key.StartsWith("First moving", StringComparison.OrdinalIgnoreCase)) return "See";
        if (key.StartsWith("Injury", StringComparison.OrdinalIgnoreCase)) return "Pain";
        if (key.StartsWith("Close attack", StringComparison.OrdinalIgnoreCase)) return "Melee";
        if (key.StartsWith("Far attack", StringComparison.OrdinalIgnoreCase)) return "Missile";
        if (key.StartsWith("Death", StringComparison.OrdinalIgnoreCase)) return "Death";
        if (key.StartsWith("Exploding", StringComparison.OrdinalIgnoreCase)) return "XDeath";
        if (key.StartsWith("Respawn", StringComparison.OrdinalIgnoreCase)) return "Raise";
        return null;
    }

    public static bool SkipPlayerMeleeMissile(int thingy, string label) =>
        thingy == 1 && (label is "Melee" or "Missile");

    public static string? MapSoundLabelPrefix(string key)
    {
        if (!LineEndsWithSound(key, key.Length)) return null;
        if (key.StartsWith("Alert", StringComparison.OrdinalIgnoreCase)) return "See";
        if (key.StartsWith("Attack", StringComparison.OrdinalIgnoreCase)) return "Attack";
        if (key.StartsWith("Pain", StringComparison.OrdinalIgnoreCase)) return "Pain";
        if (key.StartsWith("Death", StringComparison.OrdinalIgnoreCase)) return "Death";
        if (key.StartsWith("Action", StringComparison.OrdinalIgnoreCase)) return "Active";
        if (key.StartsWith("Rip", StringComparison.OrdinalIgnoreCase)) return "Rip";
        return null;
    }

    public static bool ShouldMakeStateDefines(bool typeResolved, bool patchedStates) =>
        typeResolved && !patchedStates;

    public static bool LoopContinues(int getLineResult) => getLineResult == 1;

    public static bool ThingTypeResolved(bool resolved) => resolved;

    public static bool ApplyNoIceDeath(bool val) => val;

    public static bool IsRenderStyleKey(string key, int len) =>
        len == 12 && key.Equals("Render Style", StringComparison.OrdinalIgnoreCase);

    public static bool IsObituaryKey(string key) =>
        key.Equals("Obituary", StringComparison.OrdinalIgnoreCase)
        || key.Equals("Melee obituary", StringComparison.OrdinalIgnoreCase)
        || key.Equals("Self obituary", StringComparison.OrdinalIgnoreCase);

    public static float ClampScale(float scale) =>
        Math.Clamp(scale, 1e-6f, 256f);

    public static bool ShortPropertyKey(string key, int len) => len == 5;

    public static bool IsSpeedKey(string key) =>
        key.Equals("Speed", StringComparison.OrdinalIgnoreCase);

    public static bool IsWidthKey(string key) =>
        key.Equals("Width", StringComparison.OrdinalIgnoreCase);

    public static bool IsAlphaKey(string key) =>
        key.Equals("Alpha", StringComparison.OrdinalIgnoreCase);

    public static bool IsScaleKey(string key) =>
        key.Equals("Scale", StringComparison.OrdinalIgnoreCase);

    public static bool IsDecalKey(string key) =>
        key.Equals("Decal", StringComparison.OrdinalIgnoreCase);

    public static bool IsDroppedItemKey(string key, int len) =>
        len == 12 && key.Equals("dropped item", StringComparison.OrdinalIgnoreCase);

    public static bool IsFastSpeedKey(string key, int len) =>
        len == 10 && key.Equals("fast speed", StringComparison.OrdinalIgnoreCase);

    public static bool IsMeleeRangeKey(string key, int len) =>
        len == 11 && key.Equals("melee range", StringComparison.OrdinalIgnoreCase);

    public static bool IsSplashGroupKey(string key, int len) =>
        len == 12 && key.Equals("splash group", StringComparison.OrdinalIgnoreCase);

    public static bool IsProjectileGroupKey(string key, int len) =>
        len == 16 && key.Equals("projectile group", StringComparison.OrdinalIgnoreCase);

    public static bool IsNoIceDeathKey(string key, int len) =>
        len == 12 && key.Equals("No Ice Death", StringComparison.OrdinalIgnoreCase);

    public static bool IsPainChanceKey(string key, int len) =>
        len == 11 && key.Equals("Pain chance", StringComparison.OrdinalIgnoreCase);

    public static int PainChanceFromVal(long val) => (short)val;

    public static bool SoundFromNumericIndex(int index) => index >= 0;

    public static bool TreatLine2AsSoundName(char endptrAt) => endptrAt == '\0';

    public static bool ChangedStatesFlag(bool patched) => patched;

    public static bool UnknownThingLine(bool knownKey) => !knownKey;

    public static string FormatUnknownLine(string key, int thingy) => $"{key} (thing {thingy})";

    public static bool ResolveThingIndex(int thingy) => thingy > 0;

    public static bool UseDummyActor(bool typeMissing) => typeMissing;

    public static bool LogThingOutOfRange(int thingy) => thingy > 0;

    public static double DehToDouble(long acsval) => acsval / 65536.0;

    public static bool MarkHadTranslucency(bool had) => had;

    public static bool MarkHadStyle(bool had) => had;

    public static bool ClearProjectilePassWhenHeightSet(bool hadProj) => !hadProj;

    public static bool DisableProjectilePassHeight() => true;

    public static int LoopGetLineSuccess(int result) => result;

    public static bool KeyLengthMatches(int actual, int expected) => actual == expected;

    public static bool StrIcmpMatch(string a, string b) =>
        a.Equals(b, StringComparison.OrdinalIgnoreCase);

    public static bool IsCrispySoundKey(string key) => key.Contains("crispy", StringComparison.OrdinalIgnoreCase);

    public static bool IsBloodColorKey(string key, int len) =>
        len == 11 && key.Equals("Blood color", StringComparison.OrdinalIgnoreCase);

    public static bool IsConversationKey(string key) => key.StartsWith("Conversation", StringComparison.OrdinalIgnoreCase);

    public static bool IsGravityKey(string key, int len) =>
        len == 7 && key.Equals("Gravity", StringComparison.OrdinalIgnoreCase);

    public static bool IsFrictionKey(string key, int len) =>
        len == 8 && key.Equals("Friction", StringComparison.OrdinalIgnoreCase);

    public static bool IsDamageFactorKey(string key, int len) =>
        len == 13 && key.Equals("Damage factor", StringComparison.OrdinalIgnoreCase);

    public static bool IsDamageMultiplierKey(string key, int len) =>
        len == 17 && key.Equals("Damage multiplier", StringComparison.OrdinalIgnoreCase);

    public static bool IsMaxStepHeightKey(string key, int len) =>
        len == 14 && key.Equals("Max step height", StringComparison.OrdinalIgnoreCase);

    public static bool IsMaxDropoffHeightKey(string key, int len) =>
        len == 17 && key.Equals("Max dropoff height", StringComparison.OrdinalIgnoreCase);

    public static bool IsBounceFactorKey(string key, int len) =>
        len == 13 && key.Equals("Bounce factor", StringComparison.OrdinalIgnoreCase);

    public static bool IsWallBounceFactorKey(string key, int len) =>
        len == 18 && key.Equals("Wall bounce factor", StringComparison.OrdinalIgnoreCase);

    public static bool IsProjectilePassHeightKey(string key, int len) =>
        len == 22 && key.Equals("Projectile pass height", StringComparison.OrdinalIgnoreCase);

    public static bool IsPhysicalHeightKey(string key, int len) =>
        len == 15 && key.Equals("Physical height", StringComparison.OrdinalIgnoreCase);

    public static bool MarkHadPhysHeight(bool v) => v;

    public static bool MarkHadProjHeight(bool v) => v;
}
