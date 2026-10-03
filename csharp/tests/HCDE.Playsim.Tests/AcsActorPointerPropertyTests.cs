using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorPointerPropertyTests
{
    [Theory]
    [InlineData(26)]
    [InlineData(27)]
    public void ProjectilePropertiesReadPointerTidAndIgnoreSetters(int property)
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.Actors.Single(actor => actor.ThingId == 42);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        var expected = property == 26 ? 3 : 42;
        Run(sim, missile, [3, 0, 3, property, 3, 999, 245,
            3, 7, 3, 0, 3, property, 246, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0)); Assert.Same(owner, missile.Owner); Assert.Equal(target.Id, missile.TracerTargetId);
        Run(sim, missile, [3, 7, 3, 0, 3, property, 3, expected, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, missile, [3, 7, 3, 0, 3, property, 3, expected + 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(26)]
    [InlineData(27)]
    public void QueriesUseCurrentReferencedActorTidRatherThanCachedValue(int property)
    {
        var sim = Room(); var owner = sim.Players.Single(); var target = sim.Actors.Single(actor => actor.ThingId == 42);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        var referenced = property == 26 ? (Actor)owner : target; referenced.ThingId = 123;
        Query(sim, missile, 0, property, 123); referenced.Destroy(); Query(sim, missile, 0, property, 0);
    }

    [Theory]
    [InlineData(26, false)]
    [InlineData(27, false)]
    [InlineData(26, true)]
    [InlineData(27, true)]
    public void MissingOrDestroyedActorQueriesReturnZero(int property, bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        Query(sim, actor, destroyed ? 0 : 999, property, 0);
    }

    [Fact]
    public void OrdinaryMissileHasOwnerTargetButNoTracer()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Query(sim, missile, 0, 26, 3); Query(sim, missile, 0, 27, 0);
    }

    [Fact]
    public void ActorWithoutBrainOrTracerReportsZero()
    {
        var sim = Room(); var actor = sim.Players.Single();
        Query(sim, actor, 0, 26, 0); Query(sim, actor, 0, 27, 0);
    }

    private static void Query(AuthoritySimulation sim, Actor actor, int tid, int property, int expected)
    {
        Run(sim, actor, [3, 7, 3, tid, 3, property, 246, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }
    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1, Id = 3 }, new LevelThing { Type = 3001, Id = 42, X = 128 }],
    });
}
