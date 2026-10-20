namespace HCDE.Gamedata.Tests;

public class VmDoomEdMapinfoResolverTests
{
    [Fact]
    public void TryGetSpawnableClassName_UsesNumberedTable()
    {
        var table = new Dictionary<int, string> { [1] = "Imp" };
        Assert.Equal("Imp", VmDoomEdMapinfoResolver.TryGetSpawnableClassName(1, table, new Dictionary<int, string>()));
    }
}
