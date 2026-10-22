namespace HCDE.Playsim;

/// <summary>Model ID and model-animation archive helpers from <c>p_mobj.cpp</c> (1749–1918).</summary>
public static class VmPMobjSerializeModel
{
    public sealed class ModelArchiveContext
    {
        public bool Writing { get; init; }
        public bool HasKey { get; init; }
        public string? ModelPath { get; init; }
        public string? ModelNamespace { get; init; }
        public Func<string, string, bool, int>? FindModel { get; init; }
        public Action<string, string>? WriteModelPath { get; init; }
    }

    public static int SerializeModelId(ModelArchiveContext ctx, int id)
    {
        if (ctx.Writing)
        {
            if (id >= 0 && ctx.ModelPath != null)
                ctx.WriteModelPath?.Invoke("model", ctx.ModelPath);
            return id;
        }

        if (!ctx.HasKey)
            return -1;
        if (ctx.ModelPath != null && ctx.ModelNamespace != null && ctx.FindModel != null)
            return ctx.FindModel(ctx.ModelPath, ctx.ModelNamespace, true);
        return -1;
    }

    public sealed class ModelAnimFields
    {
        public int FirstFrame { get; set; }
        public int LastFrame { get; set; }
        public int LoopFrame { get; set; }
        public int StartFrame { get; set; }
        public int Flags { get; set; }
        public double Framerate { get; set; }
        public int StartTic { get; set; }
        public int SwitchOffset { get; set; }
    }

    public static ModelAnimFields SerializeModelAnim(ModelAnimFields anim, bool writing)
    {
        if (!writing)
            return anim;
        return anim;
    }

    public enum ModelAnimFrameKind
    {
        Null,
        Interp,
        PrecalcIqm,
    }

    public static ModelAnimFrameKind ReadModelAnimFrameType(string? type)
    {
        if (string.IsNullOrEmpty(type) || type == "nullptr")
            return ModelAnimFrameKind.Null;
        if (type == "interp")
            return ModelAnimFrameKind.Interp;
        if (type == "precalcIQM")
            return ModelAnimFrameKind.Null;
        return ModelAnimFrameKind.Null;
    }

    public static ModelAnimFrameKind WriteModelAnimFrameType(ModelAnimFrameKind kind) => kind switch
    {
        ModelAnimFrameKind.Interp => ModelAnimFrameKind.Interp,
        ModelAnimFrameKind.PrecalcIqm => ModelAnimFrameKind.Null,
        _ => ModelAnimFrameKind.Null,
    };

    public static bool LegacyFirstFrameKeyPresent(bool hasFirstFrameKey) => hasFirstFrameKey;

    public static bool ShouldClearLegacyInterp(bool reading, bool hasFirstFrameKey) =>
        reading && hasFirstFrameKey;
}
