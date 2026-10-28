namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchThingStateLabelsTests
{
    [Fact]
    public void TryMapFrameLabel_Spawn()
    {
        var labels = new Dictionary<string, int>();
        Assert.True(VmDehackedPatchThingStateLabels.TryMapFrameLabel("Initial frame", 12, labels));
        Assert.Equal(12, labels["Spawn"]);
    }

    [Fact]
    public void TryMapSoundLabel_Death()
    {
        var sounds = new Dictionary<string, int>();
        Assert.True(VmDehackedPatchThingStateLabels.TryMapSoundLabel("Death sound", 5, sounds));
        Assert.Equal(5, sounds["Death"]);
    }
}
