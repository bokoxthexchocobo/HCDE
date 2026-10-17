using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DecorationAncestryTests
{
    [Theory]
    [InlineData(12, "GibbedMarine")]
    [InlineData(18, "ZombieMan")]
    [InlineData(19, "ShotgunGuy")]
    [InlineData(20, "DoomImp")]
    [InlineData(21, "Demon")]
    [InlineData(22, "Cacodemon")]
    [InlineData(23, "LostSoul")]
    [InlineData(59, "Meat2")]
    [InlineData(60, "Meat4")]
    [InlineData(61, "Meat3")]
    [InlineData(62, "Meat5")]
    [InlineData(63, "BloodyTwitch")]
    public void NativeDecorationParentsMatchOnlyAncestryQueriesAcrossFreshSave(int type, string parent)
    {
        var sim = Room(type); var saved = SimSavegame.Write(sim, true); var fresh = Room(type); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var player = world.Players.Single(); var decoration = world.Actors.Single(a => a.ThingId == 9);
            Assert.Equal(0, ActorProximity.CountProximity(player, parent, 256, flags: 4));
            Assert.Equal(1, ActorProximity.CountProximity(player, parent.ToLowerInvariant(), 256, flags: 1 | 4));
            ActorRemovalActions.Remove(decoration, flags: 8, classFilter: parent); Assert.False(decoration.Destroyed);
        }
    }

    [Theory]
    [InlineData(PatrolMarkers.PointType, "PatrolPoint")]
    [InlineData(PatrolMarkers.SpecialType, "PatrolSpecial")]
    public void PatrolMarkersExposeNativeClassAfterFreshRestore(int type, string name)
    {
        var sim = Room(type); var saved = SimSavegame.Write(sim, true); var fresh = Room(type); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var marker = world.Actors.Single(a => a.ThingId == 9);
            Assert.Equal(name, AcsCallFunctions.ClassNameOf(marker));
            Assert.Equal(1, ActorProximity.CountProximity(world.Players.Single(), name, 256));
            ActorRemovalActions.Remove(marker, flags: 8, classFilter: name); Assert.True(marker.Destroyed);
        }
    }

    [Fact]
    public void SimilarHangingDecorationsDoNotGainUnrelatedParents()
    {
        var sim = Room(59); var player = sim.Players.Single();
        Assert.Equal(0, ActorProximity.CountProximity(player, "Meat3", 256, flags: 1 | 4));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 128, Id = 9 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
