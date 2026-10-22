namespace HCDE.Playsim.Tests;

public class VmPortalChangePortalTests
{
    [Fact]
    public void ChangePortalLine_RejectsLinked()
    {
        var port = new VmPortalUpdateFinalize.LinePortalState { Type = VmPortalUpdateFinalize.PorttLinked };
        Assert.False(VmPortalChangePortal.ChangePortalLine(port, 1, null));
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
        VmPortalChangePortal.ChangePortal(3, 0, it, line =>
        {
            touched.Add(line);
            return true;
        });
        Assert.Equal(new[] { 10, 20 }, touched);
    }
}
