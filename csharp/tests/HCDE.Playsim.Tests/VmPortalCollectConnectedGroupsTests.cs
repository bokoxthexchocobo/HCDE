namespace HCDE.Playsim.Tests;

public class VmPortalCollectConnectedGroupsTests
{
    [Fact]
    public void CollectConnectedGroups_SingleDisplacement_NoOp()
    {
        var outGroups = new VmPortalCollectConnectedGroups.PortalGroupArray();
        var mask = new VmPortalCollectConnectedGroups.BitMask();
        Assert.False(VmPortalCollectConnectedGroups.CollectConnectedGroups(
            new VmPortalCollectConnectedGroups.CollectContext { DisplacementSize = 1 },
            mask,
            new List<VmPortalCollectConnectedGroups.LinkedPortal>(),
            new List<int>(),
            0, 0, 0, 0, 64, 32, outGroups));
        Assert.True(mask.GetBit(0));
    }

    [Fact]
    public void CollectConnectedGroups_LinkedPortalAddsDestinationGroup()
    {
        var outGroups = new VmPortalCollectConnectedGroups.PortalGroupArray();
        var mask = new VmPortalCollectConnectedGroups.BitMask();
        var found = new List<VmPortalCollectConnectedGroups.LinkedPortal>();
        var ctx = new VmPortalCollectConnectedGroups.CollectContext
        {
            DisplacementSize = 4,
            LinkedPortals = new[]
            {
                new VmPortalCollectConnectedGroups.LinkedPortal
                {
                    OriginLineIndex = 1,
                    OriginFrontPortalGroup = 1,
                    DestinationFrontPortalGroup = 2,
                },
            },
            DisplacementAt = (_, _) => new VmPortalCollectConnectedGroups.Displacement { IsSet = true },
            InRange = (_, _, _) => true,
            BoxOnLineSide = (_, _, _, _) => -1,
        };
        Assert.True(VmPortalCollectConnectedGroups.CollectConnectedGroups(
            ctx, mask, found, new List<int>(), 1, 0, 0, 0, 64, 32, outGroups));
        Assert.Contains(2, outGroups.Groups);
    }
}
