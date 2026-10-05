using System.Text;
using HCDE.MapLoader.Tests;
using HCDE.MapLoader;

namespace HCDE.Net.Core.Tests;

public class ExtendedMapBootstrapTests
{
    [Theory]
    [InlineData(BinaryThingFlagFormat.Doom)]
    [InlineData(BinaryThingFlagFormat.Strife)]
    public void SelectedBinaryFlagFormatSeedsPlayersAndSectors(BinaryThingFlagFormat format)
    {
        var store = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(TestWadBuilder.BuildMinimalMapWad("MAP01"),
            "MAP01", store, out var error, format), error);
        Assert.Equal(160, store.Sectors[0].LightLevel);
        Assert.True(store.Players.ContainsKey(0));
    }

    [Fact]
    public void UnsupportedFlagFormatRejectsWithoutSeedingGuestState()
    {
        var store = new GuestWorldStateStore();
        Assert.False(MapLoadBootstrap.TrySeedGuestWorldState(TestWadBuilder.BuildMinimalMapWad("MAP01"),
            "MAP01", store, out var error, (BinaryThingFlagFormat)99));
        Assert.Equal("unsupported-binary-thing-flag-format", error);
        Assert.Empty(store.Players);
        Assert.Empty(store.Sectors);
    }
    [Fact]
    public void HexenMapBootstrapsNetworkSectors()
    {
        var store = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(HexenLevelTests.HexenWad(), "MAP01", store, out var error), error);
        Assert.Equal(160, store.Sectors[0].LightLevel);
    }

    [Fact]
    public void UdmfMapBootstrapsPlayersAndSectors()
    {
        var wad = MapsModsTests.Wad(("MAP01", []), ("TEXTMAP", Encoding.UTF8.GetBytes("""
            namespace = "ZDoom";
            sector { heightceiling=128; lightlevel=192; }
            thing { type=1; coop=true; }
            """)));
        var store = new GuestWorldStateStore();
        Assert.True(MapLoadBootstrap.TrySeedGuestWorldState(wad, "MAP01", store, out var error), error);
        Assert.Equal(192, store.Sectors[0].LightLevel);
        Assert.True(store.Players.ContainsKey(0));
    }
}
