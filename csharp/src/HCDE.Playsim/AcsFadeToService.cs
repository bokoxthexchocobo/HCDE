namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::DoFadeTo</c> wrapper over <see cref="AcsFadeRangeService"/>.</summary>
public static class AcsFadeToService
{
    public static void Apply(AcsFadeRangeService.BlendState blend, int r, int g, int b, int a, int timeAcs)
    {
        var red = Math.Clamp(r, 0, 255);
        var green = Math.Clamp(g, 0, 255);
        var blue = Math.Clamp(b, 0, 255);
        var alpha = Math.Clamp(a, 0, 65536);
        AcsFadeRangeService.Apply(blend, 0, 0, 0, -1, red, green, blue, alpha, timeAcs);
    }
}
