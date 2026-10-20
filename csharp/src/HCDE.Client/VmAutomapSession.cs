namespace HCDE.Client;

/// <summary>
/// Automap session init, scale, input from <c>am_map.cpp</c> (1337–1600).
/// </summary>
public static class VmAutomapSession
{
    public static int ResolveInitialPlayer(IReadOnlyList<bool> inGame, int consolePlayer)
    {
        if (consolePlayer >= 0 && consolePlayer < inGame.Count && inGame[consolePlayer])
            return consolePlayer;

        for (var i = 0; i < inGame.Count; i++)
        {
            if (inGame[i])
                return i;
        }

        return consolePlayer;
    }

    public static (double x, double y) CenterOnCamera(double cameraX, double cameraY, double mapW, double mapH) =>
        (cameraX - mapW / 2, cameraY - mapH / 2);

    public static void ClearMarkArray(Span<double> markX, double emptyValue = -1)
    {
        for (var i = 0; i < markX.Length; i++)
            markX[i] = emptyValue;
    }

    public static double PickInitialScale(double minScale, double maxScale)
    {
        var scale = minScale / 0.7;
        return scale > maxScale ? minScale : scale;
    }

    public static int ScaleLineThickness(double statusBarScaleX, double statusBarScaleY)
    {
        var sc = Math.Min(statusBarScaleX, statusBarScaleY);
        var scaled = (int)Math.Round(sc, MidpointRounding.ToEven);
        return scaled <= 0 ? 1 : scaled;
    }

    public static double RescaleAfterResolution(double scaleMtof, double oldMin, double newMin) =>
        oldMin == 0 ? scaleMtof : scaleMtof * newMin / oldMin;

    public static double ClampScale(double scale, double minScale, double maxScale) =>
        Math.Clamp(scale, minScale, maxScale);

    public static bool ShouldIgnorePanBindInFollow(bool followPlayer, string? bindName) =>
        followPlayer && bindName != null && bindName.StartsWith("+am_pan", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldReleaseMainBinding(string? mainBind) =>
        mainBind == null || mainBind.Length == 0 || mainBind[0] != '+';

    public static double ResolveZoomMultiplier(
        int amZoomDir,
        double zoomInConst,
        double zoomOutConst,
        bool zoomInHeld,
        bool zoomOutHeld,
        double delta)
    {
        if (amZoomDir > 0)
            return zoomInConst * amZoomDir;
        if (amZoomDir < 0)
            return zoomOutConst / -amZoomDir;
        if (zoomInHeld)
            return 1 + (zoomInConst - 1) * delta;
        if (zoomOutHeld)
            return 1 + (zoomOutConst - 1) * delta;
        return 1;
    }

    public static double ApplyZoom(double scaleMtof, double multiplier) => scaleMtof * multiplier;

    public static (double x, double y) FollowPlayerMapCenter(
        double cameraX,
        double cameraY,
        double viewHeightDelta,
        double mapW,
        double mapH,
        double fFollow,
        bool rotateActive,
        double sinYaw,
        double cosYaw)
    {
        var sx = cameraX;
        var sy = cameraY;
        if (rotateActive)
        {
            var rx = sx;
            var ry = sy;
            sx = rx * cosYaw - ry * sinYaw;
            sy = rx * sinYaw + ry * cosYaw;
        }

        return (sx - mapW / 2 + fFollow * viewHeightDelta, sy - mapH / 2);
    }
}
