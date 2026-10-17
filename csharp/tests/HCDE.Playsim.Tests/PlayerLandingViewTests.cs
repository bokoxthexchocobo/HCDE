using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerLandingViewTests
{
    [Theory]
    [InlineData(-16, false, null, -2)]
    [InlineData(-8, false, null, 0)]
    [InlineData(-16, true, null, 0)]
    [InlineData(-16, false, "Ice", 0)]
    public void FloorViewImpulseHonorsThresholdGravityAndIceReturn(int velocity, bool noGravity,
        string? damageType, double expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoGravity = noGravity;
        player.DamageType = damageType; player.Z = Fixed.FromInt(1); player.OnGround = false;
        player.VelocityZ = Fixed.FromInt(velocity); ActorPhysics.Step(sim, player);
        Assert.Equal(expected, player.DeltaViewHeight); Assert.Equal(41, player.ViewHeight);
    }

    [Fact]
    public void ViewRecoversAcrossCommandsInsteadOfResettingEachTic()
    {
        var sim = Room(); var player = sim.Players.Single(); player.VelocityZ = Fixed.FromInt(-16);
        player.PlayerSquatView(); player.VelocityZ = default;
        sim.Tick(); Assert.Equal(39, player.ViewHeight); Assert.Equal(-1.75, player.DeltaViewHeight);
        sim.Tick(); Assert.Equal(37.25, player.ViewHeight); Assert.Equal(-1.5, player.DeltaViewHeight);
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.Equal(41, player.ViewHeight); Assert.Equal(0, player.DeltaViewHeight);
    }

    [Fact]
    public void ExtremeSquatClampsToHalfViewAndStartsRecovery()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DeltaViewHeight = -100;
        player.AdvanceViewHeight();
        Assert.Equal(20.5, player.ViewHeight); Assert.Equal(0.25 + 1.0 / 65536, player.DeltaViewHeight);
    }

    [Fact]
    public void ZeroCrossingUsesNativeEpsilon()
    {
        var sim = Room(); var player = sim.Players.Single(); player.DeltaViewHeight = -0.25;
        player.AdvanceViewHeight(); Assert.Equal(40.75, player.ViewHeight);
        Assert.Equal(1.0 / 65536, player.DeltaViewHeight);
    }

    [Fact]
    public void SaveResumesViewRecoveryAndOldSaveClearsDelta()
    {
        var sim = Room(); var player = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        player.DeltaViewHeight = -2; sim.Tick();
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(316, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        for (var i = 0; i < 20; i++)
        {
            sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum);
            Assert.Equal(player.ViewHeight, restored.Players.Single().ViewHeight);
        }
        SimSavegame.Apply(restored, older); Assert.Equal(0, restored.Players.Single().DeltaViewHeight);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidDeltaIsRejectedBeforeRestore(double value)
    {
        var sim = Room(); var snapshot = sim.CaptureState(); snapshot.Actors[0].DeltaViewHeight = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(snapshot));
        Assert.Equal(0, sim.Players.Single().DeltaViewHeight);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
