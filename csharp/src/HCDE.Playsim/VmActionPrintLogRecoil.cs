namespace HCDE.Playsim;

/// <summary>Print/log/recoil actions from <c>p_actionfunctions.cpp</c> (1381–1547).</summary>
public static class VmActionPrintLogRecoil
{
    public const int StyleTranslucent = 1;
    public const int StyleFuzzy = 2;
    public const int StyleAdd = 3;

    public static double RecoilThrustAngleDegrees(double yawDegrees) => yawDegrees + 180.0;

    public static bool ShouldResolveDollarString(string text) =>
        !string.IsNullOrEmpty(text) && text[0] == '$';

    public static string ResolveDollarString(string text, Func<string, string> lookup) =>
        ShouldResolveDollarString(text) ? lookup(text[1..]) : text;

    public static bool ShouldPrintToLocalView(bool selfLocal, bool targetLocal) =>
        selfLocal || targetLocal;

    public static bool PrintBoldAlwaysShows() => true;

    public static bool ShouldSkipLocalLog(bool localOnly, bool selfLocalView) =>
        localOnly && !selfLocalView;

    public static double SelectMidPrintDuration(double requestedTime, double defaultMidTime) =>
        requestedTime > 0 ? requestedTime : defaultMidTime;

    public static (double savedMidTime, double newMidTime) WithTemporaryMidTime(double requested, double current, double defaultMid) =>
        (current, SelectMidPrintDuration(requested, defaultMid));

    public static int MapTranslucentMode(int mode) =>
        mode == 0 ? StyleTranslucent : mode == 2 ? StyleFuzzy : StyleAdd;

    public static double ClampAlpha(double alpha) => Math.Clamp(alpha, 0.0, 1.0);

    public static (double alpha, int style) ApplySetRenderStyle(double alpha, int mode) =>
        (ClampAlpha(alpha), mode);

    public static bool ShouldClearAlpha1Flag() => true;

    public static bool ShouldLoadNamedFont(int fontName) => fontName != 0;

    public static double RestoreMidPrintTime(double savedMidTime) => savedMidTime;

    public static bool LogLocalGatePasses(bool localOnly, bool selfLocalView) => !ShouldSkipLocalLog(localOnly, selfLocalView);

    public static string FormatLogInt(int num) => num.ToString();

    public static string FormatLogFloat(double num) => num.ToString("G", System.Globalization.CultureInfo.InvariantCulture);

    public const int FtfRemove = 1 << 0;
    public const int FtfClamp = 1 << 1;

    public static double FadeReduceDefault(double reduce) => reduce == 0 ? 0.1 : reduce;

    public static (double newAlpha, bool clampToOne, bool shouldRemove) StepFadeIn(double alpha, double reduce, int flags)
    {
        alpha += reduce;
        if (alpha >= 1.0)
        {
            var clamp = (flags & FtfClamp) != 0;
            var remove = (flags & FtfRemove) != 0;
            return (clamp ? 1.0 : alpha, clamp, remove);
        }

        return (alpha, false, false);
    }

    public static bool FadeInReachedOpaque(double alpha) => alpha >= 1.0;

    public static (double alpha, bool clampToZero, bool shouldRemove) StepFadeOut(double alpha, double reduce, int flags)
    {
        alpha -= reduce;
        if (alpha <= 0)
        {
            var clamp = (flags & FtfClamp) != 0;
            var remove = (flags & FtfRemove) != 0;
            return (clamp ? 0.0 : alpha, clamp, remove);
        }

        return (alpha, false, false);
    }

    public static double StepFadeToward(double alpha, double target, double amount)
    {
        if (alpha > target)
        {
            alpha -= amount;
            return alpha < target ? target : alpha;
        }

        if (alpha < target)
        {
            alpha += amount;
            return alpha > target ? target : alpha;
        }

        return alpha;
    }

    public static (double alpha, bool shouldRemove) FinishFadeTo(double alpha, double target, int flags)
    {
        if ((flags & FtfClamp) != 0)
            alpha = ClampAlpha(alpha);
        var remove = alpha == target && (flags & FtfRemove) != 0;
        return (alpha, remove);
    }

}
