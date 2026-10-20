namespace HCDE.Gamedata;

/// <summary><c>SplashKeywords</c> / <c>SplashParser</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainSplashDescriptors
{
    public static readonly string[] Keywords =
    [
        "}",
        "smallsound",
        "smallclip",
        "sound",
        "smallclass",
        "baseclass",
        "chunkclass",
        "chunkxvelshift",
        "chunkyvelshift",
        "chunkzvelshift",
        "chunkbasezvel",
        "noalert",
    ];

    public static readonly VmTerrainGenericDescriptor[] Parser =
    [
        new() { Type = VmTerrainGenericType.End },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplySmallSound },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplySmallClip },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplySound },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplySmallClass },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyBaseClass },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyChunkClass },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyChunkXVel },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyChunkYVel },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyChunkZVel },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyChunkBaseZVel },
        new() { Type = VmTerrainGenericType.Custom, Custom = ApplyNoAlert },
    ];

    private static void ApplySmallSound(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).SmallSplashSound = VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplySound(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).NormalSplashSound = VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplySmallClip(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).SmallSplashClip = VmTerrainGenericParser.ReadDouble(t, ref i, e);

    private static void ApplySmallClass(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).SmallSplashClass = VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplyBaseClass(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).SplashBaseClass = VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplyChunkClass(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).SplashChunkClass = VmTerrainGenericParser.ReadString(t, ref i, e);

    private static void ApplyChunkXVel(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).ChunkXVelShift = VmTerrainGenericParser.ReadByte(t, ref i, e);

    private static void ApplyChunkYVel(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).ChunkYVelShift = VmTerrainGenericParser.ReadByte(t, ref i, e);

    private static void ApplyChunkZVel(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).ChunkZVelShift = VmTerrainGenericParser.ReadByte(t, ref i, e);

    private static void ApplyChunkBaseZVel(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
        => ((VmSplashDef)ctx.Target).ChunkBaseZVel = VmTerrainGenericParser.ReadDouble(t, ref i, e);

    private static void ApplyNoAlert(IReadOnlyList<string> t, ref int i, VmTerrainGenericContext ctx, List<string> e)
    {
        ((VmSplashDef)ctx.Target).NoAlert = true;
        if (i < t.Count && t[i] is "1" or "0" or "true" or "false")
            i++;
    }
}
