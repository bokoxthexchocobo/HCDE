namespace HCDE.Playsim.Tests;

public class VmPortalChangeDestinationTests
{
    [Fact]
    public void ChangePortalLine_RejectsLinked()
    {
        var port = new VmPortalChangeDestination.LinePortalPort { Type = VmPortalChangeDestination.PorttLinked };
        Assert.False(VmPortalChangeDestination.ChangePortalLine(port, 0, 1, 1, (_, _) => 2, null, null));
    }

    [Fact]
    public void ClipLineToPortal_RejectsParallelLine()
    {
        Assert.True(VmPortalChangeDestination.ClipLineToPortal(
            0, 0, 10, 0,
            0, 5, 10, 5,
            10, 0,
            0, 0));
    }

    [Fact]
    public void ChangePortal_AdvancesLineIdIterator()
    {
        var hash = new int[VmPortalBuildBlockmap.TagHashSize];
        for (var i = 0; i < hash.Length; i++)
            hash[i] = -1;
        var entries = new[]
        {
            new VmPortalBuildBlockmap.LineTagIdEntry(3, 10, 1),
            new VmPortalBuildBlockmap.LineTagIdEntry(3, 20, -1),
        };
        hash[3 % VmPortalBuildBlockmap.TagHashSize] = 0;
        var it = new VmPortalBuildBlockmap.LineIdIterator(3, hash, entries);
        var touched = new List<int>();
        VmPortalChangeDestination.ChangePortal(3, 5, 0, it, line =>
        {
            touched.Add(line);
            return true;
        });
        Assert.Equal(new[] { 10, 20 }, touched);
    }
}
