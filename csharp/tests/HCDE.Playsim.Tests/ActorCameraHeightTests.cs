using System.Buffers.Binary;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class ActorCameraHeightTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-8)]
    [InlineData(45.25)]
    public void NonplayerQueryUsesExplicitCameraHeight(double height)
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); actor.CameraHeight = height;
        Assert.Equal(height, actor.GetCameraHeight()); Assert.Equal((int)Math.Floor(height * 65536), Query(sim, actor));
    }

    [Fact]
    public void DefaultSentinelFollowsCurrentActorHeight()
    {
        var actor = new Actor { Height = Fixed.FromInt(60) }; Assert.Equal(30, actor.GetCameraHeight());
        actor.Height = Fixed.FromInt(20); Assert.Equal(10, actor.GetCameraHeight());
        actor.CameraHeight = 7; Assert.Equal(7, actor.GetCameraHeight());
        actor.CameraHeight = Actor.DefaultCameraHeight; Assert.Equal(10, actor.GetCameraHeight());
    }

    [Fact]
    public void PlayerAcsQueryRemainsDefaultViewHeight()
    {
        var sim = Room(); var player = sim.Players.Single(); player.CameraHeight = 12;
        Assert.Equal(12, player.GetCameraHeight()); Assert.Equal(41 * 65536, Query(sim, player));
    }

    [Fact]
    public void CameraHeightRoundTripsWithChecksumAndOldSaveResetsIt()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); var older = SimSavegame.Write(sim, true);
        sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        actor.CameraHeight = 17.5; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        Assert.Equal(301, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        SimSavegame.Apply(sim, older); Assert.Equal(Actor.DefaultCameraHeight, actor.CameraHeight);
        SimSavegame.Apply(sim, saved); Assert.Equal(17.5, actor.CameraHeight); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfiniteHeightsRejectBeforeMutation(double value)
    {
        var sim = Room(); var actor = sim.Players.Single(); Assert.Throws<ArgumentOutOfRangeException>(() => actor.CameraHeight = value);
        var state = sim.CaptureState(); state.Actors[0].CameraHeight = value;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        actor.CameraHeight = 12; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-cameraheight-state", error);
    }

    [Fact]
    public void ExplicitHeightRequiresGeometryState()
    {
        var sim = Room(); var state = sim.CaptureState(); state.GeometryHealth = null; state.Actors[0].CameraHeight = 5;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static int Query(AuthoritySimulation sim, Actor actor)
    {
        var stack = new List<int> { 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, [], new(),
            AcsCallFunctions.GetActorViewHeight, 1, out var result)); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
