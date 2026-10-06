using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGravitySaveTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0.125)]
    [InlineData(1)]
    [InlineData(10)]
    public void ActionMultiplierAndDisabledFlagRoundTrip(double gravity)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.NoGravity(actor); ActorPropertyActions.SetGravity(actor, gravity);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Gravity = Fixed.FromInt(3); actor.NoGravity = false;
        sim.RestoreState(state);
        Assert.Equal(gravity, actor.Gravity.ToDouble()); Assert.True(actor.NoGravity);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void DirectOverrideRestoresNeighborDefaultAndExactBytes()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var other = sim.AddBot(100, 0);
        actor.Gravity = Fixed.FromDouble(-0.5);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Gravity = default; other.Gravity = Fixed.FromInt(4); sim.RestoreState(state);
        Assert.Equal(-0.5, actor.Gravity.ToDouble()); Assert.Equal(1, other.Gravity.ToDouble());
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void UnchangedDefaultRetainsLegacyLayout()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.Null(sim.CaptureState().Actors.Single().GravityRaw);
        var bytes = SimSavegame.Write(sim);
        Assert.NotEqual(70, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Gravity = Fixed.FromInt(4); sim.RestoreState(state);
        Assert.Equal(1, actor.Gravity.ToDouble());
    }

    [Theory]
    [InlineData(0, 70)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    public void InvalidTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorPropertyActions.LowGravity(sim.AddBot(0, 0));
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.StartsWith("save-gravity-", error);
    }

    [Theory]
    [InlineData(65536)]
    [InlineData(-65536)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void AcsAssignmentsPreserveRawValuesIncludingNormalReset(int value)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Gravity = Fixed.FromInt(2); actor.NoGravity = true;
        AcsActorProperties.Set(sim, actor, 0, AcsActorProperties.Gravity, value);
        Assert.Equal(value, AcsActorProperties.Get(sim, actor, 0, AcsActorProperties.Gravity));
        Assert.True(actor.NoGravity);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Gravity = default; sim.RestoreState(state);
        Assert.Equal(value, actor.Gravity.Raw);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void BaselineRestoreUndoesLaterGravityAndResumesNormalAcceleration()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(60); actor.OnGround = false;
        var bytes = SimSavegame.Write(sim);
        ActorPropertyActions.SetGravity(actor, -0.5);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(1, actor.Gravity.ToDouble()); Assert.False(actor.HasGravityOverride);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        ActorPhysics.Step(sim, actor);
        Assert.Equal(60, actor.Z.ToDouble()); Assert.Equal(-1, actor.VelocityZ.ToDouble());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
