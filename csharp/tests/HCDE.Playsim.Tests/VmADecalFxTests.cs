using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmADecalFxTests
{
    [Fact]
    public void FaderTick_NullDecal_DestroySelf()
    {
        var destroyed = false;
        var state = new VmADecalFx.FaderState { DestroySelf = () => destroyed = true };
        VmADecalFx.TickFader(state, 10, false);
        Assert.True(destroyed);
    }

    [Fact]
    public void FaderTick_EndWindow_ExpiresAndDestroysDecal()
    {
        var expired = false;
        var decalDestroyed = false;
        var selfDestroyed = false;
        var decal = new VmADecalFx.DecalRef
        {
            Expired = () => expired = true,
            Destroy = () => decalDestroyed = true,
        };
        var state = new VmADecalFx.FaderState
        {
            TheDecal = decal,
            TimeToStartDecay = 0,
            TimeToEndDecay = 10,
            DestroySelf = () => selfDestroyed = true,
        };
        VmADecalFx.TickFader(state, 10, false);
        Assert.True(expired);
        Assert.True(decalDestroyed);
        Assert.True(selfDestroyed);
    }

    [Fact]
    public void FaderTick_MidFade_SetsAlpha()
    {
        var decal = new VmADecalFx.DecalRef { Alpha = 1 };
        var state = new VmADecalFx.FaderState
        {
            TheDecal = decal,
            TimeToStartDecay = 0,
            TimeToEndDecay = 100,
            StartTrans = 100,
        };
        VmADecalFx.TickFader(state, 50, false);
        Assert.Equal(50, decal.Alpha);
    }

    [Fact]
    public void StretcherTick_AtEnd_AppliesGoals()
    {
        var decal = new VmADecalFx.DecalRef { ScaleX = 1, ScaleY = 1 };
        var destroyed = false;
        var state = new VmADecalFx.StretcherState
        {
            TheDecal = decal,
            TimeToStart = 0,
            TimeToStop = 5,
            GoalX = 2,
            GoalY = 3,
            StretchX = true,
            StretchY = true,
            DestroySelf = () => destroyed = true,
        };
        VmADecalFx.TickStretcher(state, 5, false);
        Assert.Equal(2, decal.ScaleX);
        Assert.Equal(3, decal.ScaleY);
        Assert.True(destroyed);
    }

    [Fact]
    public void SliderTick_AtEnd_SetsZ()
    {
        var decal = new VmADecalFx.DecalRef { Z = 10 };
        var destroyed = false;
        var state = new VmADecalFx.SliderState
        {
            TheDecal = decal,
            TimeToStart = 0,
            TimeToStop = 4,
            DistY = 6,
            DestroySelf = () => destroyed = true,
        };
        VmADecalFx.TickSlider(state, 4, false);
        Assert.Equal(16, decal.Z);
        Assert.True(destroyed);
    }

    [Fact]
    public void ColorerTick_MissingFixedColorFlag_DestroySelf()
    {
        var destroyed = false;
        var state = new VmADecalFx.ColorerState
        {
            TheDecal = new VmADecalFx.DecalRef { RenderStyleFlags = 0 },
            DestroySelf = () => destroyed = true,
        };
        VmADecalFx.TickColorer(state, 5, false);
        Assert.True(destroyed);
    }

    [Fact]
    public void ColorerTick_End_AppliesGoalShade()
    {
        int shade = -1;
        var destroyed = false;
        var state = new VmADecalFx.ColorerState
        {
            TheDecal = new VmADecalFx.DecalRef
            {
                RenderStyleFlags = VmADecalFx.StyleColorIsFixed,
                SetShadeColor = c => shade = c,
            },
            TimeToStartDecay = 0,
            TimeToEndDecay = 1,
            GoalColor = new VmADecalFx.RgbColor(0, 1, 2, 3),
            DestroySelf = () => destroyed = true,
        };
        VmADecalFx.TickColorer(state, 1, false);
        Assert.Equal(1 | (2 << 8) | (3 << 16), shade);
        Assert.True(destroyed);
    }

    [Fact]
    public void ColorerTick_MidBlend_InterpolatesRgb()
    {
        int shade = -1;
        var state = new VmADecalFx.ColorerState
        {
            TheDecal = new VmADecalFx.DecalRef
            {
                RenderStyleFlags = VmADecalFx.StyleColorIsFixed,
                AlphaColor = 0x000000,
                SetShadeRgb = c => shade = c,
            },
            TimeToStartDecay = 0,
            TimeToEndDecay = 10,
            StartColor = new VmADecalFx.RgbColor(0, 0, 0, 0),
            GoalColor = new VmADecalFx.RgbColor(0, 100, 0, 0),
        };
        VmADecalFx.TickColorer(state, 5, false);
        Assert.Equal(50, shade & 0xff);
    }
}
