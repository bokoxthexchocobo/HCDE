using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSpeedTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(81920)]
    [InlineData(-81920)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void SetGetCheckPreserveExactFixedSpeed(int value)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Run(sim, actor, [3, 0, 3, 1, 3, value, 245,
            3, 7, 3, 0, 3, 1, 246, 3, value, 19, 5, 112, 1]);
        Assert.Equal(value, actor.MovementSpeed.Raw); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 1, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 1, 3, value ^ 1, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(2)]
    [InlineData(-1)]
    public void PlayerThrustUsesScriptSpeed(double speed)
    {
        var sim = Room(); var player = sim.Players.Single(); Set(sim, player, speed);
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.Equal(speed, player.X.ToDouble());
        Assert.Equal(speed * ActorPhysics.GroundFriction, player.VelocityX.ToDouble());
    }

    [Fact]
    public void MonsterChaseSpeedRetainsManagedQuarterScaleAndCatalogDefaults()
    {
        var sim = Room(); var actor = sim.AddBot(128, 0); Set(sim, actor, 12);
        Assert.Equal(3, actor.ChaseSpeed); actor.ChaseSpeed = 2.5;
        Assert.Equal(10, actor.MovementSpeed.ToDouble());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-5)]
    public void ProjectileSpeedChangesFutureAimWithoutRewritingCurrentVelocity(double speed)
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Rocket);
        Assert.Equal(20, missile.Speed); var velocity = missile.VelocityX;
        Set(sim, missile, speed); Assert.Equal(velocity, missile.VelocityX);
        missile.Aim(null); Assert.Equal(speed, missile.Speed); Assert.Equal(speed, missile.VelocityX.ToDouble());
    }

    [Fact]
    public void SpeedParticipatesInChecksumWithoutMovement()
    {
        var first = Room(); var second = Room(); second.Players.Single().MovementSpeed = Fixed.FromInt(2);
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetIgnoresSetterAndReturnsZero(bool destroyed)
    {
        var sim = Room(); var player = sim.Players.Single(); if (destroyed) player.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, player, [3, tid, 3, 1, 3, 12345, 245,
            3, 7, 3, tid, 3, 1, 246, 5, 112, 1]);
        Assert.Equal(65536, player.MovementSpeed.Raw); Assert.Equal(0, sim.LightOf(0));
    }

    private static void Set(AuthoritySimulation sim, Actor actor, double speed) =>
        Run(sim, actor, [3, 0, 3, 1, 3, Fixed.FromDouble(speed).Raw, 245, 1]);
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
