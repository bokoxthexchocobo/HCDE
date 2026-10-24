namespace HCDE.Playsim;

public static class VmPPSpriteOverlayTranslation
{
    public const int NoTranslation = 0;
    public const int InvalidTranslation = -1;
    public const int StyleCount = 20;

    public sealed class OverlayActionContext
    {
        public bool CallFromPsprite { get; init; }
        public int PspIndex { get; init; }
        public Func<object?, int, VmPPSpriteLayers.PspriteModel?>? FindPsprite { get; init; }
    }

    public sealed class OverlayPlayerModel
    {
        public Func<int, VmPPSpriteLayers.PspriteModel?>? FindPsprite { get; set; }
    }

    public static int ResolveLayer(int layer, int pspIndex) => layer != 0 ? layer : pspIndex;

    public static VmPPSpriteLayers.PspriteModel? ResolvePsprite(
        OverlayActionContext ctx,
        object? player,
        int layer)
    {
        if (!ctx.CallFromPsprite || player == null)
            return null;
        var resolved = ResolveLayer(layer, ctx.PspIndex);
        return ctx.FindPsprite?.Invoke(player, resolved);
    }

    public static int OverlayTranslation(
        OverlayActionContext ctx,
        object? player,
        int layer,
        string trname,
        Func<string, int>? findCustomTranslation)
    {
        var pspr = ResolvePsprite(ctx, player, layer);
        if (pspr == null)
            return 0;

        if (trname.Length == 0)
        {
            pspr.Translation = NoTranslation;
            return 0;
        }

        var tnum = findCustomTranslation?.Invoke(trname) ?? InvalidTranslation;
        if (tnum != InvalidTranslation)
            pspr.Translation = tnum;
        return 0;
    }

    public static double GetOverlayPosition(OverlayPlayerModel? player, int layer, bool getY)
    {
        if (player == null || layer == 0)
            return 0;

        var pspr = player.FindPsprite?.Invoke(layer);
        if (pspr == null)
            return 0;
        return getY ? pspr.Y : pspr.X;
    }

    public static double OverlayX(OverlayActionContext ctx, OverlayPlayerModel? player, int layer)
    {
        if (!ctx.CallFromPsprite)
            return 0;
        return GetOverlayPosition(player, ResolveLayer(layer, ctx.PspIndex), false);
    }

    public static double OverlayY(OverlayActionContext ctx, OverlayPlayerModel? player, int layer)
    {
        if (!ctx.CallFromPsprite)
            return 0;
        return GetOverlayPosition(player, ResolveLayer(layer, ctx.PspIndex), true);
    }

    public static int OverlayId(OverlayActionContext ctx) => ctx.CallFromPsprite ? ctx.PspIndex : 0;

    public static void OverlayAlpha(
        OverlayActionContext ctx,
        OverlayPlayerModel? player,
        int layer,
        double alph)
    {
        if (!ctx.CallFromPsprite)
            return;
        var pspr = ResolvePsprite(ctx, player, layer);
        if (pspr != null)
            pspr.Alpha = Math.Clamp(alph, 0.0, 1.0);
    }

    public static double OverlayAlphaRead(
        OverlayActionContext ctx,
        OverlayPlayerModel? player,
        int layer)
    {
        if (!ctx.CallFromPsprite)
            return 0;
        var pspr = ResolvePsprite(ctx, player, layer);
        return pspr?.Alpha ?? 0;
    }

    public static void OverlayRenderStyle(
        OverlayActionContext ctx,
        OverlayPlayerModel? player,
        int layer,
        int style)
    {
        if (!ctx.CallFromPsprite)
            return;
        var pspr = ResolvePsprite(ctx, player, layer);
        if (pspr == null || style >= StyleCount || style < 0)
            return;
        pspr.RenderStyle = style;
    }

    public static bool Overlay(
        OverlayPlayerModel? player,
        object? stateOwner,
        int layer,
        object? state,
        bool dontoverride,
        Func<object?, object?, int, VmPPSpriteLayers.PspriteModel?>? createPsprite,
        Action<VmPPSpriteLayers.PspriteModel, object?>? setState)
    {
        if (player == null || dontoverride && player.FindPsprite?.Invoke(layer) != null)
            return false;

        var pspr = createPsprite?.Invoke(player, stateOwner, layer);
        if (pspr == null)
            return false;
        setState?.Invoke(pspr, state);
        return true;
    }
}
