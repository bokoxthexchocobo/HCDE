using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class EffectClassIdentityTests
{
    [Theory]
    [InlineData(false, "BulletPuff")]
    [InlineData(true, "IceChunk")]
    public void EffectsExposeNativeClassToAcsAndActionFiltersAcrossFreshSave(bool ice, string name)
    {
        var sim = Room(); Actor effect;
        if (ice)
        {
            sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
                Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
            effect = sim.Actors.OfType<IceChunkActor>().First();
        }
        else effect = sim.SpawnHitscanPuff(200, 0, 20);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var actor = world.Actors.Single(a => a.Id == effect.Id);
            var globals = new AcsGlobalStrings(); var stack = new List<int> { 0 };
            Assert.True(AcsCallFunctions.TryInvoke(world, stack, new() { Value = actor }, [], globals,
                AcsCallFunctions.GetActorClass, 1, out var result));
            Assert.Equal(name, globals.Get(result)); Assert.Empty(stack);
            foreach (var query in new[] { name.ToLowerInvariant(), "Actor", "None" })
            {
                stack.AddRange([0, 0]);
                Assert.True(AcsCallFunctions.TryInvoke(world, stack, new() { Value = actor }, [query], globals,
                    AcsCallFunctions.CheckActorClass, 2, out result));
                Assert.Equal(query == name.ToLowerInvariant() ? 1 : 0, result); Assert.Empty(stack);
            }
            ActorRemovalActions.Remove(actor, flags: 8, classFilter: "Actor"); Assert.False(actor.Destroyed);
            ActorRemovalActions.Remove(actor, flags: 8 | 16, classFilter: name); Assert.False(actor.Destroyed);
            ActorRemovalActions.Remove(actor, flags: 8, classFilter: name); Assert.True(actor.Destroyed);
            Assert.Equal("None", AcsCallFunctions.ClassNameOf(actor));
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
    }, rngSeed: 42);
}
