namespace HCDE.Playsim;

/// <summary>Model archive and view-position serialize helpers from <c>p_mobj.cpp</c> (1749–1950).</summary>
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

    public sealed class BoneOverrideComponent
    {
        public int Mode { get; set; }
        public int PrevMode { get; set; }
        public int SwitchTic { get; set; }
        public int InterpLen { get; set; }
        public int Prev { get; set; }
        public int Cur { get; set; }
    }

    public sealed class BoneOverrideFields
    {
        public BoneOverrideComponent Translation { get; set; } = new();
        public BoneOverrideComponent Rotation { get; set; } = new();
        public BoneOverrideComponent Scaling { get; set; } = new();
    }

    public static void SerializeBoneOverrideComponent(BoneOverrideComponent comp, bool writing)
    {
        if (!writing)
            return;
        _ = comp.Mode;
        _ = comp.PrevMode;
        _ = comp.SwitchTic;
        _ = comp.InterpLen;
        _ = comp.Prev;
        _ = comp.Cur;
    }

    public static void SerializeBoneOverride(BoneOverrideFields fields, bool writing)
    {
        SerializeBoneOverrideComponent(fields.Translation, writing);
        SerializeBoneOverrideComponent(fields.Rotation, writing);
        SerializeBoneOverrideComponent(fields.Scaling, writing);
    }

    public sealed class TrsFields
    {
        public double TranslationX { get; set; }
        public double RotationX { get; set; }
        public double ScalingX { get; set; } = 1;
    }

    public static TrsFields SerializeTrs(TrsFields trs, bool writing)
    {
        if (!writing)
            return trs;
        return trs;
    }

    public sealed class ModelOverrideFields
    {
        public int ModelId { get; set; } = -1;
        public int[] SurfaceSkinIds { get; set; } = Array.Empty<int>();
    }

    public static ModelOverrideFields SerializeModelOverride(ModelArchiveContext ctx, ModelOverrideFields mo)
    {
        mo.ModelId = SerializeModelId(ctx, mo.ModelId);
        return mo;
    }

    public sealed class AnimModelOverrideFields
    {
        public int Id { get; set; } = -1;
    }

    public static AnimModelOverrideFields SerializeAnimModelOverride(ModelArchiveContext ctx, AnimModelOverrideFields amo, bool objectOk)
    {
        if (!objectOk)
        {
            amo.Id = -1;
            return amo;
        }

        amo.Id = SerializeModelId(ctx, amo.Id);
        return amo;
    }

    public sealed class ActorModelDataFields
    {
        public int ModelDef { get; set; }
        public int Flags { get; set; }
        public int OverrideFlagsSet { get; set; }
        public int OverrideFlagsClear { get; set; }
        public bool ModelsCleared { get; set; }
        public bool GeneratorsCleared { get; set; }
        public bool SkinIdsCleared { get; set; }
        public bool AnimationIdsCleared { get; set; }
    }

    public static ActorModelDataFields SerializeActorModelData(ActorModelDataFields data, bool writing)
    {
        if (!writing)
            return data;
        _ = data.ModelDef;
        _ = data.Flags;
        _ = data.OverrideFlagsSet;
        _ = data.OverrideFlagsClear;
        return data;
    }

    public static ActorModelDataFields ClearActorModelDataOnDestroy(ActorModelDataFields data)
    {
        data.ModelsCleared = true;
        data.GeneratorsCleared = true;
        data.SkinIdsCleared = true;
        data.AnimationIdsCleared = true;
        return data;
    }

    public sealed class ViewPositionFields
    {
        public double OffsetX { get; set; }
        public int Flags { get; set; }
    }

    public static ViewPositionFields SerializeViewPosition(ViewPositionFields vp, bool writing)
    {
        if (!writing)
            return vp;
        _ = vp.OffsetX;
        _ = vp.Flags;
        return vp;
    }
}
