using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsFrictionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(81920)]
    [InlineData(-81920)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void SetGetCheckPreservesExactFixedMultiplier(int value)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Run(sim, actor, [3, 0, 3, 42, 3, value, 245,
            3, 7, 3, 0, 3, 42, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, actor.Friction.Raw); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 42, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 42, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(1, false, 9.0625)]
    [InlineData(0.5, false, 4.53125)]
    [InlineData(2, false, 10)]
    [InlineData(0, false, 0)]
    [InlineData(-1, false, 0)]
    [InlineData(0, true, 10)]
    [InlineData(0.5, true, 10)]
    public void MovementMultipliesSurfaceFrictionClampsAndLeavesAirVelocity(double factor, bool airborne, double expected)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Run(sim, actor, [3, 0, 3, 42, 3, Fixed.FromDouble(factor).Raw, 245, 1]);
        actor.Z = Fixed.FromInt(airborne ? 100 : 0); actor.OnGround = !airborne;
        actor.VelocityX = Fixed.FromInt(10); actor.VelocityY = Fixed.FromInt(-10);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(10, actor.X.ToDouble()); Assert.Equal(-10, actor.Y.ToDouble());
        Assert.Equal(expected, actor.VelocityX.ToDouble()); Assert.Equal(-expected, actor.VelocityY.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetIgnoresSetterAndReturnsZero(bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 42, 3, 12345, 245,
            3, 7, 3, tid, 3, 42, 246, 5, 112, 1]);
        Assert.Equal(65536, actor.Friction.Raw); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void TidSetterWritesAllMatchesAndGetterChoosesNewest()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 3001, Id = 42 }, new LevelThing { Type = 3001, Id = 42, X = 128 }],
        });
        var actor = sim.Actors.First();
        Run(sim, actor, [3, 42, 3, 42, 3, 32768, 245, 1]);
        Assert.All(sim.Actors, candidate => Assert.Equal(32768, candidate.Friction.Raw));
        sim.Actors.Last().Friction = new Fixed(12345);
        Run(sim, actor, [3, 7, 3, 42, 3, 42, 246, 3, 12345, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    [Fact]
    public void FrictionParticipatesInRestingChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().Friction = Fixed.FromDouble(0.5);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
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
        Things = [new LevelThing { Type = 1 }],
    });
}
