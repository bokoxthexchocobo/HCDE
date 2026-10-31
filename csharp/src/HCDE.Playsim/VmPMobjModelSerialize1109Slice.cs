namespace HCDE.Playsim;

/// <summary>
/// Model serializer field policy from <c>p_mobj.cpp</c> (**1801–1950**): anim override, frame type tags,
/// model-data destroy, and view-position keys. Chunk **1109**.
/// </summary>
public static class VmPMobjModelSerialize1109Slice
{
    public const int MissingAnimModelId = -1;

    public enum FrameKind
    {
        Null,
        Interp,
        PrecalculatedIqm
    }

    public sealed class AnimModelOverrideReadContext
    {
        public bool ObjectPresent { get; init; }
        public int Id { get; set; }
    }

    public static void ApplyAnimModelOverrideRead(AnimModelOverrideReadContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (!ctx.ObjectPresent)
            ctx.Id = MissingAnimModelId;
    }

    public sealed class ModelAnimFrameReadContext
    {
        public bool LegacyFirstFrameKey { get; init; }
        public string TypeTag { get; init; } = "nullptr";
        public float Inter { get; init; }
        public int Frame1 { get; init; }
        public int Frame2 { get; init; }
        public FrameKind Kind { get; set; }
        public bool Cleared { get; set; }
    }

    public static void ApplyModelAnimFrameRead(ModelAnimFrameReadContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        if (ctx.LegacyFirstFrameKey)
        {
            ctx.Cleared = true;
            ctx.Kind = FrameKind.Null;
            return;
        }

        switch (ctx.TypeTag)
        {
            case "interp":
                ctx.Kind = FrameKind.Interp;
                ctx.Cleared = false;
                break;
            case "precalcIQM":
                ctx.Kind = FrameKind.Null;
                ctx.Cleared = true;
                break;
            default:
                ctx.Kind = FrameKind.Null;
                ctx.Cleared = true;
                break;
        }
    }

    public static string FrameKindWriteTag(FrameKind kind) =>
        kind == FrameKind.Interp ? "interp" : "nullptr";

    public sealed class ModelDataDestroyContext
    {
        public int ModelCount { get; set; }
        public int GeneratorCount { get; set; }
        public int SkinCount { get; set; }
        public int AnimationCount { get; set; }
    }

    public static void RunModelDataDestroy(ModelDataDestroyContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        ctx.ModelCount = 0;
        ctx.GeneratorCount = 0;
        ctx.SkinCount = 0;
        ctx.AnimationCount = 0;
    }

    public static IReadOnlyList<string> ViewPositionSerializeKeys() => new[] { "offset", "flags" };

    public static IReadOnlyList<string> ModelDataSerializeKeys() => new[]
    {
        "modelDef",
        "models",
        "skinIDs",
        "animationIDs",
        "modelFrameGenerators",
        "modelBoneOverrides",
        "flags",
        "overrideFlagsSet",
        "overrideFlagsClear",
        "curAnim",
        "prevAnim"
    };

    public static bool TrsObjectOpens(string? key) => !string.IsNullOrEmpty(key);

    public static IReadOnlyList<string> TrsFieldKeys() => new[] { "translation", "rotation", "scaling" };

    public static bool ShouldReadAnimModelFields(bool beginObjectOk, bool isReading) =>
        beginObjectOk && !isReading || beginObjectOk && isReading;
}
