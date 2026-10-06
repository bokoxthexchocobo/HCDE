using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSizeSaveTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(12.5, 30.5)]
    [InlineData(20, 56)]
    public void ExplicitSizesRoundTripIncludingNormalDefaults(double radius, double height)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.True(ActorPropertyActions.SetSize(actor, radius, height));
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Radius = Fixed.FromInt(10); actor.Height = Fixed.FromInt(10); sim.RestoreState(state);
        Assert.Equal(radius, actor.Radius.ToDouble()); Assert.Equal(height, actor.Height.ToDouble());
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void PlayerStandingHeightRestoresAlongsideCurrentHeight()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); ActorPropertyActions.SetSize(player, 12, 40);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        player.FullHeight = 56; player.Height = Fixed.FromInt(56); sim.RestoreState(state);
        Assert.Equal(40, player.FullHeight); Assert.Equal(40, player.Height.ToDouble());
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void FailedSizeTestDoesNotAddPersistenceRecord()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.False(ActorPropertyActions.SetSize(actor, height: 129, testPosition: true));
        Assert.Null(sim.CaptureState().Actors.Single().Size);
    }

    [Theory]
    [InlineData(0, 75)]
    [InlineData(4, int.MaxValue)]
    [InlineData(8, 2)]
    [InlineData(12, -1)]
    [InlineData(24, -1)]
    public void MalformedSizeTrailerIsRejected(int offset, int value)
    {
        var sim = Room(); ActorPropertyActions.SetSize(sim.AddBot(0, 0), 12, 30);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-size-", error);
    }

    [Fact]
    public void NonfiniteStandingHeightRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var state = sim.CaptureState();
        state.Actors.Single().Size = new(1, double.NaN, 1); actor.Health = 42;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(42, actor.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
