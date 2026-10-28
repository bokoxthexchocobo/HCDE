namespace HCDE.Gamedata.Tests;

public class VmEMapInfoRegistryTests
{
    [Fact]
    public void RememberMap_TracksNames()
    {
        VmEMapInfoRegistry.Clear();
        VmEMapInfoRegistry.RememberMap("MAP01");
        Assert.True(VmEMapInfoRegistry.Contains("map01"));
    }
}
