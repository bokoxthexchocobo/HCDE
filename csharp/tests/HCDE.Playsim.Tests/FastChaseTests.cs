using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FastChaseTests
{
    [Theory]
    [InlineData(3, 2, 0, 10)]
    [InlineData(1, 0, 8, 9)]
    public void CountdownPreservesMomentumAndResumesWalkingAtZero(int initial, int count, int x, int walkCount)
    {
        var (sim, actor) = Room(); actor.FastChaseStrafeCount = initial; actor.VelocityY = Fixed.FromInt(13);
        Chase(sim, actor);
        Assert.Equal(count, actor.FastChaseStrafeCount); Assert.Equal(Fixed.FromInt(x), actor.X);
        Assert.Equal(Fixed.FromInt(13), actor.VelocityY); Assert.Equal(walkCount, actor.MoveCount);
        Assert.Null(sim.CaptureState().ChaseRandomState);
    }

    [Theory]
    [InlineData(640)]
    [InlineData(641)]
    public void RangeBoundaryClearsVelocityWithoutRandomDraw(short distance)
    {
        var (sim, actor) = Room(distance); actor.FastChaseStrafeCount = -4;
        actor.VelocityX = actor.VelocityY = Fixed.FromInt(5);
        Chase(sim, actor);
        Assert.Equal(0, actor.FastChaseStrafeCount); Assert.Equal(default, actor.VelocityX);
        Assert.Equal(default, actor.VelocityY); Assert.Null(sim.CaptureState().ChaseRandomState);
    }

    [Fact]
    public void NearbyAttemptsUseExactChaseDrawOrderAndSidewaysSpeed()
    {
        var (sim, actor) = Room(); var (reference, _) = Room();
        for (var i = 0; i < 64; i++)
        {
            var strafes = reference.NextChaseByte() < 100;
            var side = strafes ? reference.NextChaseByte() < 128 ? 1 : -1 : 0;
            actor.X = actor.Y = default; actor.FastChaseStrafeCount = 0; actor.MoveCount = 10;
            Chase(sim, actor);
            Assert.Equal(strafes ? 3 : 0, actor.FastChaseStrafeCount);
            Assert.Equal(Fixed.FromInt(side * 13), actor.VelocityY);
            Assert.Equal(default, actor.VelocityX);
            Assert.Equal(Fixed.FromInt(strafes ? 0 : 8), actor.X);
            Assert.Equal(reference.CaptureState().ChaseRandomState, sim.CaptureState().ChaseRandomState);
        }
    }

    [Fact]
    public void DontMoveSkipsStrafingCountdownAndMomentumChanges()
    {
        var (sim, actor) = Room(); actor.FastChaseStrafeCount = 3; actor.VelocityY = Fixed.FromInt(4);
        Chase(sim, actor, MonsterChaseFlags.DontMove);
        Assert.Equal(3, actor.FastChaseStrafeCount); Assert.Equal(Fixed.FromInt(4), actor.VelocityY);
        Assert.Null(sim.CaptureState().ChaseRandomState); Assert.Equal(9, actor.MoveCount);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(-4)]
    public void SignedCounterRoundTripsWithChecksumAndOlderSaveClearsIt(int count)
    {
        var (sim, actor) = Room(); var old = SimSavegame.Write(sim, true);
        actor.FastChaseStrafeCount = count; sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        var saved = SimSavegame.Write(sim, true); actor.FastChaseStrafeCount = 0;
        SimSavegame.Apply(sim, saved); Assert.Equal(count, actor.FastChaseStrafeCount); Assert.Equal(hash, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Equal(0, actor.FastChaseStrafeCount);
    }

    [Fact]
    public void InvalidArchivePriorVersionRejects()
    {
        var (sim, actor) = Room(); actor.FastChaseStrafeCount = 3; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 281);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-fastchase-header", error);
    }

    private static void Chase(AuthoritySimulation sim, Actor actor, MonsterChaseFlags flags = 0) => actor.Brain!.Tick(sim, actor,
        flags | MonsterChaseFlags.FastChase | MonsterChaseFlags.NoRandomTurn | MonsterChaseFlags.NoPlayActive,
        useMelee: false, useMissile: false, nativeMovement: true);

    private static (AuthoritySimulation, Actor) Room(short distance = 300)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = distance }]
        });
        var actor = sim.AddBot(0, 0); actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MoveCount = 10; actor.MovementSpeed = Fixed.FromInt(8);
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
