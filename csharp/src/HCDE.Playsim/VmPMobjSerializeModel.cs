namespace HCDE.Playsim;

/// <summary>Model archive serialize helpers from <c>p_mobj.cpp</c> (1749–1918).</summary>
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

    public static int SerializeModelId(ModelArchiveContext ctx, string key, int id)
    {
        if (ctx.Writing)
        {
            if (id >= 0 && ctx.ModelPath != null)
                ctx.WriteModelPath?.Invoke(key, ctx.ModelPath);
            return id;
        }

        if (!ctx.HasKey)
            return -1;
        if (ctx.ModelPath != null && ctx.ModelNamespace != null && ctx.FindModel != null)
            return ctx.FindModel(ctx.ModelPath, ctx.ModelNamespace, true);
        return -1;
    }

    public sealed class BoneOverrideComponent
    {
        public int Mode { get; set; }
        public int PrevMode { get; set; }
        public int SwitchTic { get; set; }
        public int InterpLen { get; set; }
        public int Prev { get; set; }
        public int Cur { get; set; }
    }

    public static BoneOverrideComponent SerializeBoneOverrideComponent(
        string key,
        BoneOverrideComponent comp,
        IModelArchiveFieldSink arc)
    {
        arc.BeginObject(key);
        if (arc.Writing)
        {
            arc.WriteInt("mode", comp.Mode);
            arc.WriteInt("prev_mode", comp.PrevMode);
            arc.WriteInt("switchtic", comp.SwitchTic);
            arc.WriteInt("interplen", comp.InterpLen);
            arc.WriteInt("prev", comp.Prev);
            arc.WriteInt("cur", comp.Cur);
        }
        else
        {
            comp.Mode = arc.ReadInt("mode", comp.Mode);
            comp.PrevMode = arc.ReadInt("prev_mode", comp.PrevMode);
            comp.SwitchTic = arc.ReadInt("switchtic", comp.SwitchTic);
            comp.InterpLen = arc.ReadInt("interplen", comp.InterpLen);
            comp.Prev = arc.ReadInt("prev", comp.Prev);
            comp.Cur = arc.ReadInt("cur", comp.Cur);
        }

        arc.EndObject();
        return comp;
    }

    public sealed class BoneOverrideFields
    {
        public BoneOverrideComponent Translation { get; set; } = new();
        public BoneOverrideComponent Rotation { get; set; } = new();
        public BoneOverrideComponent Scaling { get; set; } = new();
    }

    public static BoneOverrideFields SerializeBoneOverride(string key, BoneOverrideFields mod, IModelArchiveFieldSink arc)
    {
        arc.BeginObject(key);
        mod.Translation = SerializeBoneOverrideComponent("translation", mod.Translation, arc);
        mod.Rotation = SerializeBoneOverrideComponent("rotation", mod.Rotation, arc);
        mod.Scaling = SerializeBoneOverrideComponent("scaling", mod.Scaling, arc);
        arc.EndObject();
        return mod;
    }

    public sealed class TrsFields
    {
        public double TranslationX { get; set; }
        public double TranslationY { get; set; }
        public double TranslationZ { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public double ScalingX { get; set; } = 1;
        public double ScalingY { get; set; } = 1;
        public double ScalingZ { get; set; } = 1;
    }

    public static TrsFields SerializeTrs(string key, TrsFields trs, IModelArchiveFieldSink arc)
    {
        arc.BeginObject(key);
        var tx = trs.TranslationX;
        var ty = trs.TranslationY;
        var tz = trs.TranslationZ;
        var rx = trs.RotationX;
        var ry = trs.RotationY;
        var rz = trs.RotationZ;
        var sx = trs.ScalingX;
        var sy = trs.ScalingY;
        var sz = trs.ScalingZ;
        arc.ArcVector3("translation", ref tx, ref ty, ref tz);
        arc.ArcVector3("rotation", ref rx, ref ry, ref rz);
        arc.ArcVector3("scaling", ref sx, ref sy, ref sz);
        trs.TranslationX = tx;
        trs.TranslationY = ty;
        trs.TranslationZ = tz;
        trs.RotationX = rx;
        trs.RotationY = ry;
        trs.RotationZ = rz;
        trs.ScalingX = sx;
        trs.ScalingY = sy;
        trs.ScalingZ = sz;
        arc.EndObject();
        return trs;
    }

    public sealed class ModelOverrideFields
    {
        public int ModelId { get; set; } = -1;
        public int[] SurfaceSkinIds { get; set; } = Array.Empty<int>();
    }

    public static ModelOverrideFields SerializeModelOverride(
        string key,
        ModelOverrideFields mo,
        ModelArchiveContext ctx,
        IModelArchiveFieldSink arc)
    {
        arc.BeginObject(key);
        mo.ModelId = SerializeModelId(ctx, "model", mo.ModelId);
        if (arc.Writing)
            arc.WriteIntArray("surfaceSkinIDs", mo.SurfaceSkinIds);
        else
            mo.SurfaceSkinIds = arc.ReadIntArray("surfaceSkinIDs", mo.SurfaceSkinIds);
        arc.EndObject();
        return mo;
    }

    public sealed class AnimModelOverrideFields
    {
        public int Id { get; set; } = -1;
    }

    public static AnimModelOverrideFields SerializeAnimModelOverride(
        string key,
        AnimModelOverrideFields amo,
        ModelArchiveContext ctx,
        IModelArchiveFieldSink arc)
    {
        var ok = arc.BeginObject(key);
        if (!arc.Writing && !ok)
        {
            amo.Id = -1;
            return amo;
        }

        if (ok)
        {
            amo.Id = SerializeModelId(ctx, "model", amo.Id);
            arc.EndObject();
        }

        return amo;
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

    public static ModelAnimFields SerializeModelAnim(string key, ModelAnimFields ao, IModelArchiveFieldSink arc)
    {
        arc.BeginObject(key);
        if (arc.Writing)
        {
            arc.WriteInt("firstFrame", ao.FirstFrame);
            arc.WriteInt("lastFrame", ao.LastFrame);
            arc.WriteInt("loopFrame", ao.LoopFrame);
            arc.WriteInt("startFrame", ao.StartFrame);
            arc.WriteInt("flags", ao.Flags);
            arc.WriteDouble("framerate", ao.Framerate);
            arc.WriteInt("startTic", ao.StartTic);
            arc.WriteInt("switchOffset", ao.SwitchOffset);
        }
        else
        {
            ao.FirstFrame = arc.ReadInt("firstFrame", ao.FirstFrame);
            ao.LastFrame = arc.ReadInt("lastFrame", ao.LastFrame);
            ao.LoopFrame = arc.ReadInt("loopFrame", ao.LoopFrame);
            ao.StartFrame = arc.ReadInt("startFrame", ao.StartFrame);
            ao.Flags = arc.ReadInt("flags", ao.Flags);
            ao.Framerate = arc.ReadDouble("framerate", ao.Framerate);
            ao.StartTic = arc.ReadInt("startTic", ao.StartTic);
            ao.SwitchOffset = arc.ReadInt("switchOffset", ao.SwitchOffset);
        }

        arc.EndObject();
        return ao;
    }

    public enum ModelAnimFrameKind
    {
        Null,
        Interp,
        PrecalcIqm,
    }

    public sealed class ModelAnimFrameInterpFields
    {
        public double Inter { get; set; }
        public int Frame1 { get; set; }
        public int Frame2 { get; set; }
    }

    public sealed class ModelAnimFrameState
    {
        public ModelAnimFrameKind Kind { get; set; } = ModelAnimFrameKind.Null;
        public ModelAnimFrameInterpFields? Interp { get; set; }
    }

    public static bool ShouldClearLegacyInterp(bool reading, bool hasFirstFrameKey) =>
        reading && hasFirstFrameKey;

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

    public static string WriteModelAnimFrameTypeToken(ModelAnimFrameKind kind) => kind switch
    {
        ModelAnimFrameKind.Interp => "interp",
        ModelAnimFrameKind.PrecalcIqm => "nullptr",
        _ => "nullptr",
    };

    public static ModelAnimFrameState SerializeModelAnimFrame(
        string key,
        ModelAnimFrameState state,
        IModelArchiveFieldSink arc,
        bool hasFirstFrameKeyOnRead)
    {
        arc.BeginObject(key);
        if (!arc.Writing)
        {
            if (ShouldClearLegacyInterp(true, hasFirstFrameKeyOnRead))
            {
                state.Kind = ModelAnimFrameKind.Null;
                state.Interp = null;
            }
            else
            {
                var type = arc.ReadString("type", "nullptr");
                state.Kind = ReadModelAnimFrameType(type);
                if (state.Kind == ModelAnimFrameKind.Interp)
                {
                    state.Interp = new ModelAnimFrameInterpFields
                    {
                        Inter = arc.ReadDouble("inter", 0),
                        Frame1 = arc.ReadInt("frame1", 0),
                        Frame2 = arc.ReadInt("frame2", 0),
                    };
                }
                else
                {
                    state.Interp = null;
                }
            }
        }
        else
        {
            if (state.Kind == ModelAnimFrameKind.Null || state.Kind == ModelAnimFrameKind.PrecalcIqm)
            {
                arc.WriteString("type", "nullptr");
            }
            else if (state.Kind == ModelAnimFrameKind.Interp && state.Interp != null)
            {
                arc.WriteString("type", "interp");
                arc.WriteDouble("inter", state.Interp.Inter);
                arc.WriteInt("frame1", state.Interp.Frame1);
                arc.WriteInt("frame2", state.Interp.Frame2);
            }
            else
            {
                arc.WriteString("type", "nullptr");
            }
        }

        arc.EndObject();
        return state;
    }
}
