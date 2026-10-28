namespace HCDE.Playsim.Tests;

public class VmPortalDisplacementConnectTests
{
    [Fact]
    public void CollectSectors_FloodFillsGroup()
    {
        var a = new VmPortalDisplacementConnect.SectorGroupNode { Id = 1 };
        var b = new VmPortalDisplacementConnect.SectorGroupNode { Id = 2 };
        a.Neighbors.Add(b);
        b.Neighbors.Add(a);
        Assert.True(VmPortalDisplacementConnect.CollectSectors(3, a, new Dictionary<int, VmPortalDisplacementConnect.SectorGroupNode>()));
        Assert.Equal(3, a.PortalGroup);
        Assert.Equal(3, b.PortalGroup);
    }

    [Fact]
    public void GetPortalOffsetPosition_ReturnsDeltaWhenNoNeighborPortalLines()
    {
        var ctx = new VmPortalDisplacementConnect.PortalOffsetContext
        {
            BlockmapContainsLines = true,
            BlockmapDx = 4,
            BlockmapDy = 4,
            GetBlock = (_, _) => (0, 0),
            NeighborContainsLines = (_, _) => false,
        };
        var (x, y) = VmPortalDisplacementConnect.GetPortalOffsetPosition(ctx, 1, 2, 10, 20);
        Assert.Equal(11, x);
        Assert.Equal(22, y);
    }

    [Fact]
    public void ConnectPortalGroups_FillsIndirectDisplacement()
    {
        var table = new VmPortalDisplacementConnect.DisplacementCell[4, 4];
        table[1, 2].IsSet = true;
        table[1, 2].X = 10;
        table[2, 3].IsSet = true;
        table[2, 3].IsSet = true;
        table[2, 3].Y = 5;
        Assert.False(VmPortalDisplacementConnect.ConnectPortalGroups(table));
        Assert.True(table[1, 3].IsSet);
        Assert.Equal(10, table[1, 3].X);
        Assert.Equal(5, table[1, 3].Y);
    }
}
