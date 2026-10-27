namespace HCDE.Playsim;

/// <summary>
/// Fog close, sector glow, health specials, and <c>LineSpecials[]</c> head from <c>p_lnspec.cpp</c> (**3455–3604**); chunk **934**.
/// </summary>
public static class VmPLnspecEnvGlowHealth934Slice
{
    public const int SecPartFloor = 0;
    public const int SecPartCeiling = 1;
    public const int SecPart3d = 2;
    public const int GlowColorInvalidate = -1;

    public static bool GlobalFogSwitchExhausted(int type) => type is not (0 or 1 or 2);

    public static int PackGlowRgb(int r, int g, int b) => (r << 16) | (g << 8) | b;

    public static int ResolveGlowColor(int height, int r, int g, int b) =>
        height < 0 ? GlowColorInvalidate : PackGlowRgb(r, g, b);

    public static int ClampLineHealth(int health) => health < 0 ? 0 : health;

    public static int ClampSectorHealth(int health) => health < 0 ? 0 : health;

    public enum SectorHealthTarget
    {
        None,
        Ceiling,
        Floor,
        ThreeD,
    }

    public static SectorHealthTarget SectorHealthPart(int part) => part switch
    {
        SecPartCeiling => SectorHealthTarget.Ceiling,
        SecPartFloor => SectorHealthTarget.Floor,
        SecPart3d => SectorHealthTarget.ThreeD,
        _ => SectorHealthTarget.None,
    };

    public readonly record struct GlowPlaneInvoke(int Color, float Height, bool Ceiling);

    public static GlowPlaneInvoke BuildFloorGlow(int height, int r, int g, int b) =>
        new(ResolveGlowColor(height, r, g, b), height, false);

    public static GlowPlaneInvoke BuildCeilingGlow(int height, int r, int g, int b) =>
        new(ResolveGlowColor(height, r, g, b), height, true);

    public readonly record struct LineHealthInvoke(int Health, bool PropagateGroup);

    public static LineHealthInvoke BuildLineHealth(int health, bool hasHealthGroup) =>
        new(ClampLineHealth(health), hasHealthGroup);

    public readonly record struct SectorHealthInvoke(SectorHealthTarget Target, int Health, bool PropagateGroup);

    public static SectorHealthInvoke? BuildSectorHealth(int part, int health, bool ceilingGroup, bool floorGroup, bool threeDGroup)
    {
        var target = SectorHealthPart(part);
        if (target == SectorHealthTarget.None)
            return null;
        var clamped = ClampSectorHealth(health);
        var group = target switch
        {
            SectorHealthTarget.Ceiling => ceilingGroup,
            SectorHealthTarget.Floor => floorGroup,
            SectorHealthTarget.ThreeD => threeDGroup,
            _ => false,
        };
        return new(target, clamped, group);
    }

    public static readonly string[] LineSpecialDispatchThrough57 =
    [
        "LS_NOP",
        "LS_NOP",
        "LS_Polyobj_RotateLeft",
        "LS_Polyobj_RotateRight",
        "LS_Polyobj_Move",
        "LS_NOP",
        "LS_Polyobj_MoveTimes8",
        "LS_Polyobj_DoorSwing",
        "LS_Polyobj_DoorSlide",
        "LS_NOP",
        "LS_Door_Close",
        "LS_Door_Open",
        "LS_Door_Raise",
        "LS_Door_LockedRaise",
        "LS_Door_Animated",
        "LS_Autosave",
        "LS_NOP",
        "LS_Thing_Raise",
        "LS_StartConversation",
        "LS_Thing_Stop",
        "LS_Floor_LowerByValue",
        "LS_Floor_LowerToLowest",
        "LS_Floor_LowerToNearest",
        "LS_Floor_RaiseByValue",
        "LS_Floor_RaiseToHighest",
        "LS_Floor_RaiseToNearest",
        "LS_Stairs_BuildDown",
        "LS_Stairs_BuildUp",
        "LS_Floor_RaiseAndCrush",
        "LS_Pillar_Build",
        "LS_Pillar_Open",
        "LS_Stairs_BuildDownSync",
        "LS_Stairs_BuildUpSync",
        "LS_ForceField",
        "LS_ClearForceField",
        "LS_Floor_RaiseByValueTimes8",
        "LS_Floor_LowerByValueTimes8",
        "LS_Floor_MoveToValue",
        "LS_Ceiling_Waggle",
        "LS_Teleport_ZombieChanger",
        "LS_Ceiling_LowerByValue",
        "LS_Ceiling_RaiseByValue",
        "LS_Ceiling_CrushAndRaise",
        "LS_Ceiling_LowerAndCrush",
        "LS_Ceiling_CrushStop",
        "LS_Floor_CrushStop",
        "LS_Ceiling_MoveToValue",
        "LS_NOP",
        "LS_GlassBreak",
        "LS_NOP",
        "LS_Sector_SetLink",
        "LS_Scroll_Wall",
        "LS_Line_SetTextureOffset",
        "LS_Sector_ChangeFlags",
        "LS_Line_SetBlocking",
        "LS_Line_SetTextureScale",
        "LS_NOP",
    ];

    public static bool DispatchOpcodeInRange(int opcode) =>
        opcode >= 0 && opcode < LineSpecialDispatchThrough57.Length;

    public static string? DispatchName(int opcode) =>
        DispatchOpcodeInRange(opcode) ? LineSpecialDispatchThrough57[opcode] : null;

    public static bool DispatchIsNop(int opcode)
    {
        var name = DispatchName(opcode);
        return name == "LS_NOP";
    }

    public static bool DispatchIsDoorCluster(int opcode) => opcode is >= 10 and <= 14;

    public static int CountDispatchEntries() => LineSpecialDispatchThrough57.Length;
}
