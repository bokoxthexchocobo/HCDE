namespace HCDE.Client.Tests;

public class VmDMainRenderDmflagsTests
{
    [Fact]
    public void LevelHasDynamicLights_RequiresPolicyAndSources()
    {
        Assert.True(VmDMainRenderDmflags.LevelHasDynamicLights(true, false, 1));
        Assert.False(VmDMainRenderDmflags.LevelHasDynamicLights(false, true, 1));
    }

    [Fact]
    public void ShouldBroadcastFov_OnlyAuthorityWithFlag()
    {
        Assert.True(VmDMainRenderDmflags.ShouldBroadcastFov(2, 2, true));
        Assert.False(VmDMainRenderDmflags.ShouldBroadcastFov(2, 2, false));
    }

    [Fact]
    public void ResolveBroadcastFov_DefaultsToNinety()
    {
        Assert.Equal(90f, VmDMainRenderDmflags.ResolveBroadcastFov(0));
    }

    [Fact]
    public void ShouldStopAutomap_WhenFlagAndActive()
    {
        Assert.True(VmDMainRenderDmflags.ShouldStopAutomap(1, 1, true));
        Assert.False(VmDMainRenderDmflags.ShouldStopAutomap(0, 1, true));
    }

    [Fact]
    public void ApplyHudHidden_ForcesFullscreenHud()
    {
        var (blocks, sprite, messages) = VmDMainRenderDmflags.ApplyHudHidden();
        Assert.Equal(12, blocks);
        Assert.False(sprite);
        Assert.False(messages);
    }

    [Fact]
    public void CompatMode_BoomPreset_IncludesTrace()
    {
        var p = VmDMainCompatMode.ResolvePreset(3);
        Assert.NotEqual(0, p.CompatFlags);
        Assert.NotEqual(0, p.CompatFlags2);
    }
}
