using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProximityConcreteClassTests
{
    [Theory]
    [InlineData("Clip")]
    [InlineData("Rocket")]
    [InlineData("BulletPuff")]
    [InlineData("IceChunk")]
    public void ConcreteClassCountsAndPointerSelectionSurviveFreshSave(string name)
    {
        var sim = Room(); Actor selected = name switch {
            "Clip" => sim.Actors.Single(a => a.DoomEdNum == PickupCatalog.Clip),
            "Rocket" => sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket),
            "BulletPuff" => sim.SpawnHitscanPuff(50, 0, 20),
            _ => SpawnChunk(sim),
        };
        selected.X = Fixed.FromInt(50); selected.Y = default; selected.Z = Fixed.FromInt(20);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var player = world.Players.Single();
            var expected = world.Actors.Count(a => !a.Destroyed && a.Id != player.Id
                && AcsCallFunctions.ClassNameOf(a).Equals(name, StringComparison.OrdinalIgnoreCase)
                && ActorJumpActions.CheckIfCloser(player, a, 100, true));
            Assert.True(expected > 0);
            Assert.Equal(expected, VmPThingsRaiseRemoveWarpProximity.CountProximity(player, name.ToLowerInvariant(), 100, flags: 4));
            Assert.True(VmPThingsRaiseRemoveWarpProximity.Check(player, name, 100, flags: 4 | 64 | 1024));
            Assert.NotNull(player.TargetId);
            Assert.Equal(name, AcsCallFunctions.ClassNameOf(world.Actors.Single(a => a.Id == player.TargetId)));
        }
    }

    [Fact]
    public void RocketAncestryIncludesCyberRocketOnlyWhenRequested()
    {
        var sim = Room(); var player = sim.Players.Single();
        sim.SpawnProjectile(player, ProjectileKind.CyberRocket);
        Assert.Equal(0, VmPThingsRaiseRemoveWarpProximity.CountProximity(player, "Rocket", 100));
        Assert.Equal(1, VmPThingsRaiseRemoveWarpProximity.CountProximity(player, "Rocket", 100, flags: 1));
    }

    [Theory]
    [InlineData("Rocket")]
    [InlineData("BulletPuff")]
    [InlineData("IceChunk")]
    public void ValidAbsentClassesStillSupportZeroCountChecks(string name)
    {
        var sim = Room(); Assert.True(VmPThingsRaiseRemoveWarpProximity.Check(sim.Players.Single(), name, 100, count: 0));
        Assert.False(VmPThingsRaiseRemoveWarpProximity.Check(sim.Players.Single(), "Unknown", 100, count: 0));
    }

    private static Actor SpawnChunk(AuthoritySimulation sim)
    {
        sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        return sim.Actors.OfType<IceChunkActor>().First();
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Clip, X = 50 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
