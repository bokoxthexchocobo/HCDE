using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlockmapSupportTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ExcludedBridgeDoesNotProvidePlayerOrMonsterSupport(bool monster, bool excluded)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }],
        });
        var bridge = Assert.Single(sim.Actors);
        bridge.Height = Fixed.FromInt(24); bridge.ActsLikeBridge = true; bridge.NoBlockmap = excluded;
        Actor rider = monster ? new Actor { Health = 100, Solid = true } : new PlayerPawn();
        rider.SectorIndex = 0; rider.OnGround = true;
        ActorPhysics.FitToSector(sim, rider);
        Assert.Equal(excluded ? 0 : 24, rider.Z.ToDouble());
        Assert.Equal(!excluded, rider.OnMobj);
    }

    [Fact]
    public void ExcludedRiderCanStillFindRegisteredSupport()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }],
        });
        var bridge = Assert.Single(sim.Actors); bridge.Height = Fixed.FromInt(24); bridge.ActsLikeBridge = true;
        var rider = new Actor { Health = 100, Solid = true, NoBlockmap = true, SectorIndex = 0, OnGround = true };
        ActorPhysics.FitToSector(sim, rider);
        Assert.Equal(24, rider.Z.ToDouble());
        Assert.True(rider.OnMobj);
    }
}
