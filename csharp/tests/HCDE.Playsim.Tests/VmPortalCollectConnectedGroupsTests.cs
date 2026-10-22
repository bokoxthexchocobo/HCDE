namespace HCDE.Playsim.Tests;

public class VmPortalCollectConnectedGroupsTests
{
    [Fact]
    public void CollectConnectedGroups_SingleDisplacementGroup()
    {
        var ctx = new VmPortalCollectConnectedGroups.CollectContext { DisplacementSize = 1, StartGroup = 2 };
        var outGroups = new VmPortalCollectConnectedGroups.PortalGroupArray();
        Assert.False(VmPortalCollectConnectedGroups.CollectConnectedGroups(ctx, outGroups));
        Assert.Contains(2, ctx.ProcessMask);
    }

    [Fact]
    public void CollectConnectedGroups_FollowsLinkedPortal()
    {
        var ctx = new VmPortalCollectConnectedGroups.CollectContext
        {
            DisplacementSize = 3,
            StartGroup = 1,
            HasLinkedPortals = true,
            LinkedPortals = new[]
            {
                new VmPortalCollectConnectedGroups.LinkedPortal
                {
                    OriginGroup = 1,
                    DestinationGroup = 2,
                    InRange = true,
                    OnLineSidePass = true,
                },
            },
        };
        var outGroups = new VmPortalCollectConnectedGroups.PortalGroupArray();
        Assert.True(VmPortalCollectConnectedGroups.CollectConnectedGroups(ctx, outGroups));
        Assert.Contains(2, outGroups.Groups);
    }
}
