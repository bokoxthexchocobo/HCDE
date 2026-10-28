namespace HCDE.Gamedata.Tests;

public class VmTextureAnimationRuntimeTests
{
    [Fact]
    public void FindAnimatedDoor_MatchesBaseTexture()
    {
        var doors = new List<VmDoorAnimationEntry> { new() { BaseTexture = "DOOR1" } };
        Assert.NotNull(VmTextureAnimationRuntime.FindAnimatedDoor(doors, "door1"));
    }

    [Fact]
    public void StandaloneAnimation_AdvancesFrame()
    {
        var anim = new VmAnimDefEntry
        {
            AnimationType = "Forward",
            PicFrames = { new VmAnimDefFrame { FrameTexture = "A", SpeedMin = 1 }, new VmAnimDefFrame { FrameTexture = "B", SpeedMin = 1 } }
        };
        var state = new VmStandaloneAnimationState();
        Assert.True(VmTextureAnimationRuntime.InitStandaloneAnimation(anim, 0, 0, state));
        var tex = VmTextureAnimationRuntime.UpdateStandaloneAnimation(anim, state, 1000);
        Assert.False(string.IsNullOrEmpty(tex));
    }
}
