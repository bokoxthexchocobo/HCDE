namespace HCDE.MapLoader.Tests;

public class MapLoaderConstantsTests
{
    [Fact]
    public void DefaultMapName_IsMap01()
    {
        Assert.Equal("MAP01", MapLoaderConstants.DefaultMapName);
        Assert.Equal(8, MapLoaderConstants.MaxMapNameLength);
    }
}
