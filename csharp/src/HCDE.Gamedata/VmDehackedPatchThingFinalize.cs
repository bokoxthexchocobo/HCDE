namespace HCDE.Gamedata;

/// <summary><c>PatchThing</c> tail validation and post-pass from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingFinalize
{
    public static bool ValidateHeightPair(bool hadPhysHeight, bool hadProjHeight) =>
        hadPhysHeight == hadProjHeight;

    public static double NormalizeSpeed(double speed) =>
        Math.Abs(speed) >= 256 ? speed / 65536.0 : speed;

    public static void ApplyShadowStyleChange(bool becameShadow, bool hadStyle, bool hadTranslucency, VmDehackedThingPatchTarget target)
    {
        if (becameShadow)
        {
            if (!hadStyle) target.RenderStyle = 3;
            if (!hadTranslucency) target.Alpha = 0.5;
        }
        else if (!hadStyle)
        {
            target.RenderStyle = 0;
        }
    }

    public static void ApplyCountKillFlags(bool countKill, VmDehackedThingBitWords words)
    {
        if (countKill)
        {
            words.Flags1 |= 0x00000030;
            words.Changed1 = true;
        }
        else
        {
            words.Flags1 &= ~0x00000030u;
            words.Changed1 = true;
        }
    }

    public static bool ShouldResetCeilingHeight(bool spawnCeiling, bool hadHeight, bool hadPhysHeight, int thingType) =>
        spawnCeiling && !hadHeight && !hadPhysHeight && thingType > 0;

    public static double ResolveOrgHeight(int thingType, IReadOnlyList<double> orgHeights)
    {
        if (thingType <= 0 || thingType > orgHeights.Count) return 0;
        return orgHeights[thingType - 1];
    }

    public static bool IsSpecialThing(uint flags0) => (flags0 & 0x00000020) != 0;

    public static void ApplyCanUseWalls(VmDehackedThingBitWords words)
    {
        words.Flags1 |= 1u << 15;
        words.Changed1 = true;
    }
}
