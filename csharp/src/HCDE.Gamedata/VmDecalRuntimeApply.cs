namespace HCDE.Gamedata;

/// <summary><c>FDecalTemplate::ApplyToDecal</c> from <c>decallib.cpp</c>.</summary>
public sealed class VmDecalInstanceState
{
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
    public string PicName { get; set; } = "";
    public double Alpha { get; set; } = 1;
    public string RenderStyle { get; set; } = "Normal";
    public int RenderFlags { get; set; }
    public string ShadeColor { get; set; } = "";
    public int TranslationId { get; set; }
    public string AnimatorName { get; set; } = "";
}

public static class VmDecalRuntimeApply
{
    public static void ApplyTemplate(VmDecalTemplate template, VmDecalInstanceState state, int preservedFlags, Random? random = null)
    {
        if (template.RenderStyle.Equals("Shaded", StringComparison.OrdinalIgnoreCase))
            state.ShadeColor = template.ShadeColor;
        state.TranslationId = template.TranslationStart.Length > 0 ? 1 : 0;
        state.ScaleX = template.ScaleX;
        state.ScaleY = template.ScaleY;
        state.PicName = template.PicName;
        state.Alpha = template.Alpha;
        state.RenderStyle = template.RenderStyle;
        state.RenderFlags = (template.FlipX ? 1 : 0) | (template.FlipY ? 2 : 0) | (preservedFlags & 0xF0);
        if (template.RandomFlipX || template.RandomFlipY)
        {
            var rng = random ?? Random.Shared;
            if (template.RandomFlipX && rng.Next(2) == 0) state.RenderFlags ^= 1;
            if (template.RandomFlipY && rng.Next(2) == 0) state.RenderFlags ^= 2;
        }

        state.AnimatorName = template.AnimatorName;
    }
}
