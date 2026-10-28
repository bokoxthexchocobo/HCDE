namespace HCDE.Playsim.Tests;

public class VmPMobjOrientationShadeTests
{
    [Fact]
    public void SetShade_PacksRgb()
    {
        var actor = new VmPMobjOrientationShade.OrientationActor();
        VmPMobjOrientationShade.SetShade(actor, 0x004080ff, (_, _, _) => 7);
        Assert.Equal(0x074080ffu, actor.Fillcolor);
    }

    [Fact]
    public void ClampPitch_NonPlayerUses89()
    {
        var actor = new VmPMobjOrientationShade.OrientationActor();
        Assert.Equal(-89, VmPMobjOrientationShade.ClampPitch(actor, -120));
    }

    [Fact]
    public void SetPitch_InterpSetsCheat()
    {
        var actor = new VmPMobjOrientationShade.OrientationActor
        {
            Player = new(),
            Pitch = 0
        };
        VmPMobjOrientationShade.SetPitch(actor, 10, VmPMobjOrientationShade.SpfInterpolate, null);
        Assert.Equal(10, actor.Pitch);
        Assert.NotEqual(0, actor.Player.Cheats & VmPMobjOrientationShade.CfInterpview);
    }

    [Fact]
    public void SetViewPitch_UpdatesView()
    {
        var actor = new VmPMobjOrientationShade.OrientationActor { Player = new(), ViewPitch = 0 };
        VmPMobjOrientationShade.SetViewPitch(actor, 5, VmPMobjOrientationShade.SpfInterpolate);
        Assert.Equal(5, actor.ViewPitch);
    }

    [Fact]
    public void GetFov_NoInterpCheat()
    {
        var actor = new VmPMobjOrientationShade.OrientationActor
        {
            Player = new() { Fov = 110, Cheats = VmPMobjOrientationShade.CfNofovinterp }
        };
        Assert.Equal(110, VmPMobjOrientationShade.GetFov(actor, 0.5));
    }

    [Fact]
    public void GetFov_LerpsForMonster()
    {
        var actor = new VmPMobjOrientationShade.OrientationActor { PrevFov = 40, CameraFov = 60 };
        Assert.Equal(50, VmPMobjOrientationShade.GetFov(actor, 0.5));
    }
}
