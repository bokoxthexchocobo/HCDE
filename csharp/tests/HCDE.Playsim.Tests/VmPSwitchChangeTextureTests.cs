namespace HCDE.Playsim.Tests;

public class VmPSwitchChangeTextureTests
{
    [Fact]
    public void ChangeSwitchTexture_FalseWhenNoSwitch()
    {
        var quest = true;
        Assert.False(VmPSwitchChangeTexture.ChangeSwitchTexture(
            new VmPSwitchChangeTexture.Side(),
            true,
            0,
            new VmPSwitchChangeTexture.ChangeContext { FindSwitch = _ => null },
            ref quest));
        Assert.False(quest);
    }

    [Fact]
    public void AdvanceFrame_SetsTimerFromFrame()
    {
        var btn = new VmPSwitchChangeTexture.ActiveButton
        {
            SwitchDef = new VmPSwitchChangeTexture.SwitchDef
            {
                NumFrames = 3,
                Frames = { new() { TimeMin = 5, TimeRnd = 0 }, new() { TimeMin = 7 }, new() { TimeMin = 1 } },
            },
        };
        btn.Frame = 0;
        VmPSwitchChangeTexture.AdvanceFrame(btn);
        Assert.Equal(7, btn.Timer);
    }

    [Fact]
    public void AddSwitchPair_LinksDefs()
    {
        var off = new VmPSwitchChangeTexture.SwitchDef();
        var on = new VmPSwitchChangeTexture.SwitchDef();
        var table = new Dictionary<int, VmPSwitchChangeTexture.SwitchDef>();
        VmPSwitchChangeTexture.AddSwitchPair(table, 1, 2, off, on);
        Assert.Same(on, off.PairDef);
        Assert.Same(off, table[2].PairDef);
    }
}
