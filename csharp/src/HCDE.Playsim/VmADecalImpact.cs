namespace HCDE.Playsim;

/// <summary><c>DBaseDecal::CloneSelf</c> and <c>DImpactDecal</c> spawn helpers from <c>a_decals.cpp</c> (630–803).</summary>
public static class VmADecalImpact
{
    public const int RfDecalMask = 0x00ff0000;
    public const int WallfNoautodecals = 1 << 0;
    public const int NoTranslation = -1;
    public const int StyleNormal = 0;

    public sealed class CloneSelfOps
    {
        public Func<double, VmADecalBase.DecalState?> CreateThinker { get; init; } = _ => null;
        public Func<VmADecalBase.DecalState, object, double, double, object?, bool> StickToWallValid { get; init; } = (_, _, _, _, _) => false;
        public Action<VmADecalBase.DecalState, object?, object> ApplyTemplate { get; init; } = (_, _, _) => { };
        public Action<VmADecalBase.DecalState> DestroyThinker { get; init; } = _ => { };
    }

    public static VmADecalBase.DecalState? CloneSelf(
        VmADecalBase.DecalState source,
        object? tpl,
        double ix,
        double iy,
        double iz,
        object wall,
        object? ffloor,
        CloneSelfOps ops)
    {
        var decal = ops.CreateThinker(iz);
        if (decal == null)
            return null;
        if (ops.StickToWallValid(decal, wall, ix, iy, ffloor))
        {
            ops.ApplyTemplate(decal, tpl, wall);
            decal.AlphaColor = source.AlphaColor;
            decal.RenderFlags = (decal.RenderFlags & RfDecalMask) | (source.RenderFlags & ~RfDecalMask);
            return decal;
        }
        ops.DestroyThinker(decal);
        return null;
    }

    public sealed class ImpactCloneSelfOps
    {
        public Func<double, VmADecalBase.DecalState?> CreateImpactThinker { get; init; } = _ => null;
        public Func<VmADecalBase.DecalState, object, double, double, object?, bool> StickToWallValid { get; init; } = (_, _, _, _, _) => false;
        public Action<VmADecalBase.DecalState, object?, object> ApplyTemplate { get; init; } = (_, _, _) => { };
        public Action<VmADecalBase.DecalState> DestroyThinker { get; init; } = _ => { };
        public Action<VmADecalBase.DecalState> CheckMax { get; init; } = _ => { };
        public Func<object, int> GetWallFlags { get; init; } = _ => 0;
        public Func<object?, bool> TemplateIsTranslatable { get; init; } = _ => false;
        public Func<object?, int> TemplateShadeColor { get; init; } = _ => 0;
    }

    public static void CheckMax(Func<int> impactCount, int clMaxDecals, Action<int> setImpactCount, Func<VmADecalBase.DecalState?> destroyFirstAutodecal)
    {
        var count = impactCount() + 1;
        setImpactCount(count);
        if (count >= clMaxDecals)
        {
            var thinker = destroyFirstAutodecal();
            if (thinker != null)
                setImpactCount(impactCount() - 1);
        }
    }

    public static void Expired(Func<int> impactCount, Action<int> setImpactCount) =>
        setImpactCount(impactCount() - 1);

    public static VmADecalBase.DecalState? StaticCreateByName(
        int clMaxDecals,
        Func<string, object?> getDecalByName,
        Func<object?, object?> resolveDecal,
        string name,
        StaticCreateContext ctx) =>
        clMaxDecals > 0 && getDecalByName(name) is { } raw && resolveDecal(raw) is { } tpl
            ? StaticCreateByTemplate(tpl, ctx)
            : null;

    public sealed class StaticCreateContext
    {
        public int ClMaxDecals { get; init; }
        public bool ClSpreadDecals { get; init; }
        public int WallFlags { get; init; }
        public bool Permanent { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public double PosZ { get; init; }
        public object Wall { get; init; } = null!;
        public object? Ffloor { get; init; }
        public uint Color { get; set; }
        public int Translation { get; set; }
        public Func<object?, object?> GetLowerDecal { get; init; } = _ => null;
        public Func<object?, int> LowerShadeColor { get; init; } = _ => 0;
        public Func<object?, int> MainShadeColor { get; init; } = _ => 0;
        public Func<object?, bool> TemplateTranslatable { get; init; } = _ => false;
        public Func<double, VmADecalBase.DecalState?> CreateImpactThinker { get; init; } = _ => null;
        public Func<double, VmADecalBase.DecalState?> CreateBaseThinker { get; init; } = _ => null;
        public Func<VmADecalBase.DecalState, object, double, double, object?, bool> StickValid { get; init; } = (_, _, _, _, _) => false;
        public Action<VmADecalBase.DecalState> CheckMaxOnDecal { get; init; } = _ => { };
        public Action<VmADecalBase.DecalState, object?, object> ApplyTemplate { get; init; } = (_, _, _) => { };
        public Action<VmADecalBase.DecalState, int, int, int> SetShadeRgb { get; init; } = (_, _, _, _) => { };
        public Action<VmADecalBase.DecalState, int> SetTranslation { get; init; } = (_, _) => { };
        public Action<VmADecalBase.DecalState, int> SetRenderStyle { get; init; } = (_, _) => { };
        public Func<VmADecalBase.DecalState, bool> PicNumValid { get; init; } = _ => false;
        public VmADecalSpread.SpreadWallOps SpreadWallOps { get; init; } = new();
        public CloneSelfOps BaseCloneOps { get; init; } = new();
        public ImpactCloneSelfOps ImpactCloneOps { get; init; } = new();
        public Func<int, double> TexWidth { get; init; } = _ => 0;
        public Func<int, double> TexLeftOffset { get; init; } = _ => 0;
        public Func<int, bool> TexValid { get; init; } = _ => false;
    }

    public static VmADecalBase.DecalState? StaticCreateByTemplate(object? tpl, StaticCreateContext ctx)
    {
        if (tpl == null)
            return null;
        if (!(ctx.ClMaxDecals > 0 && (ctx.WallFlags & WallfNoautodecals) == 0) && !ctx.Permanent)
            return null;

        if (ctx.GetLowerDecal(tpl) is { } lowerTpl)
        {
            var lowerColor = ctx.MainShadeColor(tpl) != ctx.LowerShadeColor(lowerTpl) ? 0u : ctx.Color;
            StaticCreateByTemplate(lowerTpl, CopyCtx(ctx, lowerColor));
        }

        var decal = ctx.Permanent ? ctx.CreateBaseThinker(ctx.PosZ) : ctx.CreateImpactThinker(ctx.PosZ);
        if (decal == null)
            return null;
        if (!ctx.StickValid(decal, ctx.Wall, ctx.PosX, ctx.PosY, ctx.Ffloor))
            return null;
        if (!ctx.Permanent)
            ctx.CheckMaxOnDecal(decal);
        ctx.ApplyTemplate(decal, tpl, ctx.Wall);
        if (ctx.Color != 0)
            ctx.SetShadeRgb(decal, (int)(ctx.Color & 0xff), (int)((ctx.Color >> 8) & 0xff), (int)((ctx.Color >> 16) & 0xff));
        if (ctx.Translation != NoTranslation && ctx.MainShadeColor(tpl) == 0 && ctx.TemplateTranslatable(tpl))
        {
            ctx.SetTranslation(decal, ctx.Translation);
            ctx.SetRenderStyle(decal, StyleNormal);
        }
        if (!ctx.ClSpreadDecals || !ctx.PicNumValid(decal))
            return decal;

        VmADecalSpread.Spread(
            decal,
            tpl,
            ctx.Wall,
            ctx.PosX,
            ctx.PosY,
            ctx.PosZ,
            ctx.Ffloor,
            sourceIsImpact: true,
            decal.ScaleX,
            ctx.TexWidth,
            ctx.TexLeftOffset,
            ctx.TexValid,
            ctx.SpreadWallOps,
            ctx.BaseCloneOps,
            ctx.ImpactCloneOps);
        return decal;
    }

    public static VmADecalBase.DecalState? ImpactCloneSelf(
        VmADecalBase.DecalState source,
        object? tpl,
        double ix,
        double iy,
        double iz,
        object wall,
        object? ffloor,
        ImpactCloneSelfOps ops)
    {
        if ((ops.GetWallFlags(wall) & WallfNoautodecals) != 0)
            return null;
        var decal = ops.CreateImpactThinker(iz);
        if (decal == null)
            return null;
        if (ops.StickToWallValid(decal, wall, ix, iy, ffloor))
        {
            ops.CheckMax(decal);
            ops.ApplyTemplate(decal, tpl, wall);
            decal.AlphaColor = source.AlphaColor;
            if (ops.TemplateShadeColor(tpl) == 0 && ops.TemplateIsTranslatable(tpl))
            {
                decal.Translation = source.Translation;
                decal.RenderStyle = StyleNormal;
            }
            decal.RenderFlags = (decal.RenderFlags & RfDecalMask) | (source.RenderFlags & ~RfDecalMask);
            return decal;
        }
        ops.DestroyThinker(decal);
        return null;
    }

    static StaticCreateContext CopyCtx(StaticCreateContext ctx, uint color) => new()
    {
        ClMaxDecals = ctx.ClMaxDecals,
        ClSpreadDecals = ctx.ClSpreadDecals,
        WallFlags = ctx.WallFlags,
        Permanent = ctx.Permanent,
        PosX = ctx.PosX,
        PosY = ctx.PosY,
        PosZ = ctx.PosZ,
        Wall = ctx.Wall,
        Ffloor = ctx.Ffloor,
        Color = color,
        Translation = ctx.Translation,
        GetLowerDecal = ctx.GetLowerDecal,
        LowerShadeColor = ctx.LowerShadeColor,
        MainShadeColor = ctx.MainShadeColor,
        TemplateTranslatable = ctx.TemplateTranslatable,
        CreateImpactThinker = ctx.CreateImpactThinker,
        CreateBaseThinker = ctx.CreateBaseThinker,
        StickValid = ctx.StickValid,
        CheckMaxOnDecal = ctx.CheckMaxOnDecal,
        ApplyTemplate = ctx.ApplyTemplate,
        SetShadeRgb = ctx.SetShadeRgb,
        SetTranslation = ctx.SetTranslation,
        SetRenderStyle = ctx.SetRenderStyle,
        PicNumValid = ctx.PicNumValid,
        SpreadWallOps = ctx.SpreadWallOps,
        BaseCloneOps = ctx.BaseCloneOps,
        ImpactCloneOps = ctx.ImpactCloneOps,
        TexWidth = ctx.TexWidth,
        TexLeftOffset = ctx.TexLeftOffset,
        TexValid = ctx.TexValid,
    };
}
