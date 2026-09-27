using System.Text;
using HCDE.MapLoader.Tests;

namespace HCDE.Net.Core.Tests;

public class ExtendedMapBootstrapTests
{
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
