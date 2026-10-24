namespace HCDE.Gamedata;

/// <summary><c>DIntermissionScreen</c> ticker/overlay and text init from <c>intermission.cpp</c>.</summary>
public static class VmIntermissionScreenRuntime
{
    public static int AdvanceTicker(int ticker, int duration)
        => duration > 0 && ticker + 1 >= duration ? -1 : ticker + 1;

    public static bool ShouldShowOverlay(string condition, bool multiplayer, bool playerHasClass, bool classMatches)
    {
        if (condition.Equals("Multiplayer", StringComparison.OrdinalIgnoreCase))
            return multiplayer;
        if (string.IsNullOrEmpty(condition))
            return true;
        if (multiplayer) return false;
        return playerHasClass && classMatches;
    }

    public static VmIntermissionSubtitleLayout ComputeSubtitleLayout(int hudWidth, int hudHeight, int lineCount, int lineHeight)
    {
        var linelen = hudWidth < 640 ? hudWidth * 9 / 10 - 40 : 560;
        var height = 20 + lineCount * lineHeight;
        if (linelen < 560)
            return new VmIntermissionSubtitleLayout(hudWidth / 20, hudHeight * 9 / 10 - height, hudWidth - 2 * (hudWidth / 20), height, linelen);
        var x = hudWidth / 2 - 300;
        var y = Math.Max(0, hudHeight * 9 / 10 - height);
        return new VmIntermissionSubtitleLayout(x, y, 600, height, linelen);
    }

    public static int ComputeTextScreenDuration(int baseDuration, int textDelay, int textSpeed, int characterCount)
        => baseDuration > 0 ? baseDuration + textDelay + textSpeed * characterCount : baseDuration;

    public static int ResolveTextPosition(int configured, int defaultValue, int textWidth, int screenWidth, bool usesDefault)
    {
        if (configured >= 0) return configured;
        var x = defaultValue;
        if (usesDefault && x + textWidth > screenWidth - x)
            x = (screenWidth - textWidth) / 2;
        return x;
    }

    public static byte ComputeFaderOverlayAlpha(int ticker, int duration, VmIntermissionFadeType type)
    {
        if (duration <= 0) return 0;
        var factor = Math.Clamp((double)ticker / duration, 0, 1);
        if (type == VmIntermissionFadeType.In) factor = 1.0 - factor;
        return (byte)(factor * 255);
    }
}

public readonly record struct VmIntermissionSubtitleLayout(int X, int Y, int Width, int Height, int LineLength);
