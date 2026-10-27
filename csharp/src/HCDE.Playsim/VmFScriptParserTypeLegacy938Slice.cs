namespace HCDE.Playsim;

/// <summary>
/// Type-coercion SF_* and Legacy explosion/attack helpers from <c>fragglescript/t_func.cpp</c> (**2835–2984**); chunk **938**.
/// </summary>
public static class VmFScriptParserTypeLegacy938Slice
{
    public static bool CoerceRequiresOneArg(int argc) => argc >= 1;

    public static int ReturnTypeMobj() => VmFScriptParser.SvtMobj;

    public static int ReturnTypeString() => VmFScriptParser.SvtString;

    public static int ReturnTypeInt() => VmFScriptParser.SvtInt;

    public static int ReturnTypeFixed() => VmFScriptParser.SvtFixed;

    public static string? StringCoerceValue(int argType, string? direct, string? converted) =>
        argType == VmFScriptParser.SvtString ? direct : converted;

    public static bool SpawnExplosionRequiresThreeArgs(int argc) => argc >= 3;

    public static double SpawnExplosionZ(int argc, double providedZ, double floorZ) =>
        argc > 3 ? providedZ : floorZ;

    public static bool SpawnExplosionSucceeded(object? spawn) => spawn != null;

    public static bool RadiusAttackRequiresThreeArgs(int argc) => argc >= 3;

    public static bool RadiusAttackCanRun(object? spot, object? source) =>
        spot != null && source != null;

    public static bool SetObjPositionRequiresTwoArgs(int argc) => argc >= 2;

    public static double SetObjPositionY(int argc, double yArg, double currentY) =>
        argc >= 3 ? yArg : currentY;

    public static double SetObjPositionZ(int argc, double zArg, double currentZ) =>
        argc >= 4 ? zArg : currentZ;

    public static object? TestLocationActor(int argc, object? argActor, object? trigger) =>
        argc > 0 ? argActor : trigger;

    public static int TestLocationReturn(bool locationValid) => locationValid ? 1 : 0;

    public readonly record struct CoerceInvoke(int ReturnType, object? Value);

    public static CoerceInvoke BuildMobjCoerce(object? actor) => new(ReturnTypeMobj(), actor);

    public static CoerceInvoke? BuildStringCoerce(int argType, string? direct, string? converted) =>
        new(ReturnTypeString(), StringCoerceValue(argType, direct, converted));

    public readonly record struct SpawnExplosionInvoke(double X, double Y, double Z, int ReturnState);

    public static SpawnExplosionInvoke? BuildSpawnExplosion(
        int argc,
        double x,
        double y,
        double providedZ,
        double floorZ,
        object? spawn,
        int deathState)
    {
        if (!SpawnExplosionRequiresThreeArgs(argc))
            return null;
        var z = SpawnExplosionZ(argc, providedZ, floorZ);
        if (!SpawnExplosionSucceeded(spawn))
            return new(x, y, z, 0);
        return new(x, y, z, deathState);
    }

    public readonly record struct SetObjPositionInvoke(double X, double Y, double Z);

    public static SetObjPositionInvoke? BuildSetObjPosition(
        int argc,
        object? mobj,
        double x,
        double y,
        double z,
        double currentY,
        double currentZ)
    {
        if (!SetObjPositionRequiresTwoArgs(argc) || mobj == null)
            return null;
        return new(x, SetObjPositionY(argc, y, currentY), SetObjPositionZ(argc, z, currentZ));
    }
}
