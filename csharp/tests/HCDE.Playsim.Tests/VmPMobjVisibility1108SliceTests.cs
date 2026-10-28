namespace HCDE.Playsim.Tests;

public class VmPMobjVisibility1108SliceTests
{
    [Fact]
    public void RunDisableLocalRendering_OnlyAffectsConsolePlayer()
    {
        var set = false;
        VmPMobjVisibility1108Slice.RunDisableLocalRendering(0, 1, true, v => set = v);
        Assert.False(set);
        VmPMobjVisibility1108Slice.RunDisableLocalRendering(0, 0, true, v => set = v);
        Assert.True(set);
    }

    [Fact]
    public void IsInsideVisibleAngles_SkipsWhenMaskDisabled()
    {
        Assert.True(VmPMobjVisibility1108Slice.IsInsideVisibleAngles(
            new VmPMobjVisibility1108Slice.VisibleAnglesContext
            {
                RenderFlags = 0
            }));
    }

    [Fact]
    public void IsVisibleToPlayer_RequiresTeamMatchInTeamplay()
    {
        var visible = VmPMobjVisibility1108Slice.IsVisibleToPlayer(
            new VmPMobjVisibility1108Slice.VisibleToPlayerContext
            {
                HasConsoleCamera = true,
                RendersLocally = true,
                VisibleToTeam = 2,
                TeamPlay = true,
                PlayerTeamIndex = 0,
                VisibleToClasses = Array.Empty<Type?>()
            });
        Assert.False(visible);
    }

    [Fact]
    public void IsVisibleToPlayer_AllowsWhenClassListMatches()
    {
        var baseType = typeof(object);
        var visible = VmPMobjVisibility1108Slice.IsVisibleToPlayer(
            new VmPMobjVisibility1108Slice.VisibleToPlayerContext
            {
                HasConsoleCamera = true,
                RendersLocally = true,
                CameraHasPlayer = true,
                CameraPawnClass = baseType,
                VisibleToClasses = new[] { baseType },
                IsDescendant = (child, parent) => child == parent
            });
        Assert.True(visible);
    }
}
