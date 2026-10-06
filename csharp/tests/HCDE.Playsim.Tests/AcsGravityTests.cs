using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsGravityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(81920)]
    [InlineData(-81920)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ScriptSetGetAndCheckPreserveExactFixedValue(int value)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Run(sim, actor, [3, 0, 3, 15, 3, value, 245,
            3, 7, 3, 0, 3, 15, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, actor.Gravity.Raw); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 15, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 15, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(0.5, false)]
    [InlineData(2, false)]
    [InlineData(-0.5, false)]
    [InlineData(2, true)]
    public void ScriptGravityChangesAccelerationAndHonorsNoGravity(double gravity, bool noGravity)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Run(sim, actor, [3, 0, 3, 15, 3, Fixed.FromDouble(gravity).Raw, 245, 1]);
        actor.NoGravity = noGravity; actor.Z = Fixed.FromInt(100); actor.OnGround = false;
        actor.VelocityZ = Fixed.FromInt(4);
        ActorPhysics.Step(sim, actor);
        var expected = noGravity ? 4 * ActorPhysics.FlyingFriction : 4 - gravity;
        Assert.Equal(expected, actor.VelocityZ.ToDouble()); Assert.Equal(104, actor.Z.ToDouble());
    }

    [Fact]
    public void ScriptReplacesIceClassGravityDefault()
    {
        var sim = Room(); var chunk = new IceChunkActor(10) { Simulation = sim, Level = sim.Level,
            Z = Fixed.FromInt(100), OnGround = false };
        Assert.Equal(0.125, chunk.Gravity.ToDouble());
        Run(sim, chunk, [3, 0, 3, 15, 3, 65536, 245, 1]);
        chunk.Tick(); Assert.Equal(-1, chunk.VelocityZ.ToDouble());
    }

    [Fact]
    public void TidSetterUpdatesAllMatchesAndGetterUsesNewest()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 3001, Id = 42 }, new LevelThing { Type = 3001, Id = 42, X = 128 }],
        });
        Run(sim, null, [3, 42, 3, 15, 3, 32768, 245, 1]);
        Assert.All(sim.Actors, actor => Assert.Equal(32768, actor.Gravity.Raw));
        sim.Actors.Last().Gravity = new Fixed(12345);
        Run(sim, null, [3, 7, 3, 42, 3, 15, 246, 3, 12345, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetReturnsZeroAndIgnoresSetter(bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 15, 3, 12345, 245,
            3, 7, 3, tid, 3, 15, 246, 5, 112, 1]);
        Assert.Equal(65536, actor.Gravity.Raw); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void GravityParticipatesInChecksumEvenAtRest()
    {
        var first = Room(); var second = Room(); second.Players.Single().Gravity = Fixed.FromInt(2);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static void Run(AuthoritySimulation sim, Actor? actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
