namespace HCDE.Gamedata.Tests;

public class VmDecalRuntimeApplyTests
{
    [Fact]
    public void ApplyTemplate_CopiesShadeScaleAndPreservesClipFlags()
    {
        var template = new VmDecalTemplate
        {
            RenderStyle = "Shaded",
            ShadeColor = "Blood",
            ScaleX = 2,
            ScaleY = 0.5,
            PicName = "DECAL1",
            Alpha = 0.8,
            TranslationStart = "112:128",
            AnimatorName = "Fader1"
        };
        var state = new VmDecalInstanceState();
        VmDecalRuntimeApply.ApplyTemplate(template, state, preservedFlags: 0x30);
        Assert.Equal("Blood", state.ShadeColor);
        Assert.Equal(2, state.ScaleX);
        Assert.Equal(0.5, state.ScaleY);
        Assert.Equal("DECAL1", state.PicName);
        Assert.Equal(0.8, state.Alpha);
        Assert.Equal(1, state.TranslationId);
        Assert.Equal(0x30, state.RenderFlags & 0xF0);
        Assert.Equal("Fader1", state.AnimatorName);
    }

    [Fact]
    public void ApplyTemplate_RandomFlipUsesInjectedRandom()
    {
        var template = new VmDecalTemplate { RandomFlipX = true };
        var state = new VmDecalInstanceState();
        VmDecalRuntimeApply.ApplyTemplate(template, state, 0, new AlwaysOneRandom());
        Assert.Equal(1, state.RenderFlags & 1);
    }

    private sealed class AlwaysOneRandom : Random
    {
        public override int Next(int maxExclusive) => 0;
    }
}
