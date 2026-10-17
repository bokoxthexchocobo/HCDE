using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ConcreteSpeciesIdentityTests
{
    [Theory]
    [InlineData(PickupCatalog.Clip, "Clip")]
    [InlineData(85, "TechLamp")]
    [InlineData(PatrolMarkers.PointType, "PatrolPoint")]
    [InlineData(20, "DeadDoomImp")]
    [InlineData(58, "Demon")]
    [InlineData(69, "BaronOfHell")]
    [InlineData(-1, "Rocket")]
    [InlineData(-2, "CyberRocket")]
    [InlineData(-3, "BulletPuff")]
    [InlineData(-4, "IceChunk")]
    public void NativeSpeciesFallbackSupportsChecksOverridesAndFiltersAcrossFreshSave(int type, string expected)
    {
        var sim = Room(type); Actor actor;
        if (type >= 0) actor = sim.Actors.Single(a => a.ThingId == 9);
        else if (type is -1 or -2) actor = sim.SpawnProjectile(sim.Players.Single(),
            type == -1 ? ProjectileKind.Rocket : ProjectileKind.CyberRocket);
        else if (type == -3) actor = sim.SpawnHitscanPuff(128, 0, 20);
        else
        {
            sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
                Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
            actor = sim.Actors.OfType<IceChunkActor>().First();
        }
        var saved = SimSavegame.Write(sim, true); var fresh = Room(type); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var subject = world.Actors.Single(a => a.Id == actor.Id);
            Assert.Equal(expected, subject.SpeciesName);
            Assert.Equal(7, ActorJumpActions.CheckSpecies(subject, 7, expected.ToLowerInvariant()));
            ActorPropertyActions.SetSpecies(subject, "CustomSquad");
            Assert.Equal(7, ActorJumpActions.CheckSpecies(subject, 7, "customsquad"));
            Assert.Null(ActorJumpActions.CheckSpecies(subject, 7, expected));
            ActorPropertyActions.SetSpecies(subject, "None"); Assert.Equal(expected, subject.SpeciesName);
            ActorRemovalActions.Remove(subject, flags: 8 | 32, speciesFilter: expected); Assert.False(subject.Destroyed);
            ActorRemovalActions.Remove(subject, flags: 8, speciesFilter: expected); Assert.True(subject.Destroyed);
        }
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel {
        Things = type >= 0 ? [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 128, Id = 9 }]
            : [new LevelThing { Type = 1 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
