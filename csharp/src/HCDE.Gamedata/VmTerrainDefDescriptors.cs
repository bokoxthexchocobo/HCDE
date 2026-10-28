namespace HCDE.Gamedata;

/// <summary><c>TerrainKeywords</c> / <c>TerrainParser</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainDefDescriptors
{
    public static readonly string[] Keywords =
    [
        "}",
        "splash",
        "damageamount",
        "damagetype",
        "damagetimemask",
        "footclip",
        "stepvolume",
        "walksteptics",
        "runsteptics",
        "leftstepsounds",
        "rightstepsounds",
        "liquid",
        "friction",
        "allowprotection",
        "damageonland",
        "stepsounds",
        "stepdistance",
        "stepdistanceminvel",
    ];

    public static readonly VmTerrainGenericDescriptor[] Parser =
    [
        new() { Type = VmTerrainGenericType.End },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplySplash },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyDamageAmount },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyDamageType },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyDamageTimeMask },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyFootClip },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyStepVolume },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyWalkStepTics },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyRunStepTics },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyLeftStepSound },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyRightStepSound },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyLiquid },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyFriction },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyAllowProtection },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyDamageOnLand },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyStepSound },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyStepDistance },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyStepDistanceMinVel },
    ];

    private static VmTerrainDef Def(VmTerrainGenericContext ctx) => (VmTerrainDef)ctx.Target;

    private static void ApplySplash(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
    {
        var name = VmTerrainGenericParser.ReadString(t, ref i, e);
        Def(ctx).SplashIndex = ctx.Database.FindSplash(name);
    }

    private static void ApplyDamageAmount(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).DamageAmount = VmTerrainGenericParser.ReadInt(t, ref i, e);

    private static void ApplyDamageType(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
    {
        var raw = VmTerrainGenericParser.ReadString(t, ref i, e);
        Def(ctx).DamageType = raw.Equals("Lava", StringComparison.OrdinalIgnoreCase) ? "Fire" : raw;
    }

    private static void ApplyDamageTimeMask(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).DamageTimeMask = VmTerrainGenericParser.ReadInt(t, ref i, e);

    private static void ApplyFootClip(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).FootClip = VmTerrainGenericParser.ReadDouble(t, ref i, e);

    private static void ApplyStepVolume(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).StepVolume = (float)VmTerrainGenericParser.ReadDouble(t, ref i, e);

    private static void ApplyWalkStepTics(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).WalkStepTics = VmTerrainGenericParser.ReadInt(t, ref i, e);

    private static void ApplyRunStepTics(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).RunStepTics = VmTerrainGenericParser.ReadInt(t, ref i, e);

    private static void ApplyLeftStepSound(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplyRightStepSound(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplyLiquid(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).IsLiquid = true;

    private static void ApplyFriction(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
    {
        var input = VmTerrainGenericParser.ReadDouble(t, ref i, e);
        VmTerrainFrictionParser.ApplyFrictionInput(input, out var friction, out var move);
        var def = Def(ctx);
        def.Friction = friction;
        def.MoveFactor = move;
    }

    private static void ApplyAllowProtection(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).AllowProtection = true;

    private static void ApplyDamageOnLand(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).DamageOnLand = true;

    private static void ApplyStepSound(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => Def(ctx).StepSound = VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplyStepDistance(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => VmTerrainGenericParser.ReadDouble(t, ref i, e);

    private static void ApplyStepDistanceMinVel(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => VmTerrainGenericParser.ReadDouble(t, ref i, e);
}
