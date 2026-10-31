namespace HCDE.Client;

/// <summary>
/// <c>startDisplay</c> player center, <c>clearMarks</c>, <c>LevelInit</c> scale, window min/max,
/// line thickness, resolution rescale, and the follow-pan responder gate from <c>am_map.cpp</c>
/// (<b>1351–1500</b>). Chunk <b>1130</b>.
/// </summary>
public static class VmAmMapDisplayInit1130Slice
{
    public const int MaxPlayers = 64;
    public const string DefaultBackground = "AUTOPAGE";

    public static int FindInitialPlayer(int consolePlayer, ReadOnlySpan<bool> playerInGame)
    {
        var pnum = consolePlayer;
        if ((uint)pnum >= (uint)playerInGame.Length || !playerInGame[pnum])
        {
            pnum = 0;
            while (pnum < playerInGame.Length && !playerInGame[pnum])
                pnum++;
        }

        if (pnum >= playerInGame.Length)
            throw new InvalidOperationException("automap startDisplay requires a player in game.");
        return pnum;
    }

    public static (double X, double Y) CenterOnCamera(double cameraX, double cameraY, double windowW, double windowH)
    {
        return (cameraX - windowW / 2, cameraY - windowH / 2);
    }

    public static void RememberWindow(double x, double y, double w, double h, out double oldX, out double oldY, out double oldW, out double oldH)
    {
        oldX = x;
        oldY = y;
        oldW = w;
        oldH = h;
    }

    public static bool ClearMarks(double[] markX, out int nextMarkIndex, bool firstMarkTextureValid)
    {
        for (var i = markX.Length - 1; i >= 0; i--)
            markX[i] = -1;
        nextMarkIndex = 0;
        return firstMarkTextureValid;
    }

    public static string BackgroundTextureName(string? mapBackground)
    {
        if (string.IsNullOrEmpty(mapBackground))
            return DefaultBackground;
        return mapBackground;
    }

    public static (double ScaleMapToFrame, double ScaleFrameToMap) InitialLevelScale(double minScale, double maxScale)
    {
        var scale = minScale / 0.7;
        if (scale > maxScale)
            scale = minScale;
        return (scale, 1 / scale);
    }

    public static (double ScaleMapToFrame, double ScaleFrameToMap) MinOut(double minScale)
    {
        return (minScale, 1 / minScale);
    }

    public static (double ScaleMapToFrame, double ScaleFrameToMap) MaxOut(double maxScale)
    {
        return (maxScale, 1 / maxScale);
    }

    public static int LineThickness(bool hasStatusBar, double scaleX, double scaleY)
    {
        if (!hasStatusBar)
            return 1;
        var thickness = (int)Math.Round(Math.Min(scaleX, scaleY), MidpointRounding.ToEven);
        if (thickness <= 0)
            return 1;
        return thickness;
    }

    public static bool TryNewResolution(
        double oldMinScale,
        double scaleMapToFrame,
        double newMinScale,
        double newMaxScale,
        out double scale,
        out double scaleFrameToMap)
    {
        if (oldMinScale == 0)
        {
            scale = scaleMapToFrame;
            scaleFrameToMap = 0;
            return false;
        }

        scale = scaleMapToFrame * newMinScale / oldMinScale;
        if (scale < newMinScale)
            scale = newMinScale;
        else if (scale > newMaxScale)
            scale = newMaxScale;
        scaleFrameToMap = 1 / scale;
        return true;
    }

    public static bool SuppressesFollowPan(bool automapActive, bool keyEvent, bool followPlayer, string? binding)
    {
        if (!automapActive || !keyEvent || !followPlayer || string.IsNullOrEmpty(binding))
            return false;
        return binding.StartsWith("+am_pan", StringComparison.OrdinalIgnoreCase);
    }
}
