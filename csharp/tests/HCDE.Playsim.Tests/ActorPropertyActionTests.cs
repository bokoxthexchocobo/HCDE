using HCDE.MapLoader;
using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class ActorPropertyActionTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void RipperActionsChangeActualEligibilityAtInclusiveLimits(int level, bool expected)
    {
        var target = new Actor(); var missile = new Actor();
        ActorPropertyActions.SetRipMin(target, 2); ActorPropertyActions.SetRipMax(target, 4);
        ActorPropertyActions.SetRipperLevel(missile, level);
        Assert.Equal(expected, target.CanBeRippedBy(missile));
        ActorPropertyActions.SetRipMin(target, -1); ActorPropertyActions.SetRipMax(target, 0);
        Assert.True(target.CanBeRippedBy(missile));
    }

    [Fact]
    public void SolidityActionControlsCollisionDuringFrameExecution()
    {
        var sim = Room(); var obstacle = sim.AddBot(0, 0); var mover = sim.AddBot(0, 0);
        Assert.False(ActorPhysics.CanOccupy(sim, mover));
        obstacle.States.Configure(obstacle, [new(-1, 0), new(5, 0,
            Action: ActorPropertyActions.UnsetSolid)], 0);
        obstacle.States.Enter(obstacle, 1); Assert.True(ActorPhysics.CanOccupy(sim, mover));
        ActorPropertyActions.SetSolid(obstacle); Assert.False(ActorPhysics.CanOccupy(sim, mover));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FloatActionLeavesGravityIndependentAndSurvivesSave(bool noGravity)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.NoGravity = noGravity;
        ActorPropertyActions.SetFloat(actor); Assert.Equal(noGravity, actor.NoGravity);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.UnsetFloat(actor); sim.RestoreState(state);
        Assert.True(actor.Floating); Assert.Equal(noGravity, actor.NoGravity);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        ActorPropertyActions.UnsetFloat(actor); Assert.Equal(noGravity, actor.NoGravity);
    }

    [Fact]
    public void DamageTypeAndSignedRipperPropertiesRoundTripWithoutClamping()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetDamageType(actor, "Ice"); ActorPropertyActions.SetRipperLevel(actor, -20);
        ActorPropertyActions.SetRipMin(actor, -3); ActorPropertyActions.SetRipMax(actor, 7);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorPropertyActions.SetDamageType(actor, null); ActorPropertyActions.SetRipperLevel(actor, 0);
        sim.RestoreState(state); Assert.Equal("Ice", actor.DamageType); Assert.Equal(-20, actor.RipperLevel);
        Assert.Equal(-3, actor.RipLevelMin); Assert.Equal(7, actor.RipLevelMax);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, 68)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 8)]
    public void InvalidMovementActionTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorPropertyActions.SetFloat(sim.AddBot(0, 0));
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
