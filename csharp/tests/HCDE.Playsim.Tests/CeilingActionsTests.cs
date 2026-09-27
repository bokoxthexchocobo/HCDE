using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CeilingActionsTests
{
    private static AuthoritySimulation Room(double ceiling = 64, bool extended = false) => AuthoritySimulation.Start(new PlayLevel {
        Format = extended ? MapDataFormat.UdmfText : MapDataFormat.DoomBinary, Namespace = extended ? "ZDoom" : "Doom",
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = ceiling }], Things = [new LevelThing { Type = 1 }],
    });

    private static bool Activate(AuthoritySimulation sim, int special, bool use = false) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine { Special = special, Tag = 7 }, use);

    [Fact]
    public void SlowCrusherDamagesEveryFourTicsAndSlowsToEighthUnits()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(Activate(sim, 25));
        for (var tick = 0; tick < 8; tick++) sim.Tick();
        Assert.Equal(56, sim.CeilingOf(0)); Assert.Equal(100, player.Health);
        sim.Tick(); Assert.Equal(55, sim.CeilingOf(0)); Assert.Equal(100, player.Health);
        sim.Tick(); Assert.Equal(54.875, sim.CeilingOf(0)); Assert.Equal(100, player.Health);
        for (var tick = 0; tick < 3; tick++) sim.Tick();
        Assert.Equal(54.5, sim.CeilingOf(0)); Assert.Equal(90, player.Health);
        for (var tick = 0; tick < 3; tick++) sim.Tick();
        Assert.Equal(80, player.Health);
    }

    [Fact]
    public void FastCrusherReturnsAndRepeatsWithoutSlowdown()
    {
        var sim = Room(16); sim.Players.Single().Solid = false;
        Assert.True(Activate(sim, 6));
        for (var tick = 0; tick < 4; tick++) sim.Tick();
        Assert.Equal(8, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(10, sim.CeilingOf(0));
        for (var tick = 0; tick < 3; tick++) sim.Tick();
        Assert.Equal(16, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(14, sim.CeilingOf(0));
    }

    [Fact]
    public void StopAndResumeRetainPositionDirectionAndSlowdown()
    {
        var sim = Room();
        Assert.True(Activate(sim, 73));
        for (var tick = 0; tick < 10; tick++) sim.Tick();
        var height = sim.CeilingOf(0); var health = sim.Players.Single().Health;
        Assert.True(Activate(sim, 74));
        for (var tick = 0; tick < 10; tick++) sim.Tick();
        Assert.Equal(height, sim.CeilingOf(0)); Assert.Equal(health, sim.Players.Single().Health);
        Assert.True(Activate(sim, 73));
        sim.Tick(); Assert.Equal(height - 0.125, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(41, true, 0)]
    [InlineData(43, true, 0)]
    [InlineData(145, false, 0)]
    [InlineData(44, false, 8)]
    [InlineData(72, false, 8)]
    public void NonDamagingCeilingsWaitForObstructionThenFinish(int special, bool use, double destination)
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(Activate(sim, special, use));
        for (var tick = 0; tick < 20; tick++) sim.Tick();
        Assert.Equal(56, sim.CeilingOf(0)); Assert.Equal(100, player.Health);
        player.Solid = false;
        for (var tick = 0; tick < 70; tick++) sim.Tick();
        Assert.Equal(destination, sim.CeilingOf(0)); Assert.Equal(100, player.Health);
    }

    [Fact]
    public void IndependentPlanesMoveTogetherAndRejectDuplicatePlaneThinkers()
    {
        var sim = Room(128, extended: true); var player = sim.Players.Single();
        var ceiling = new LevelLine { Special = 40, Arg0 = 7, Arg1 = 4, Arg2 = 16, PlayerUse = true, Repeat = true };
        var floor = new LevelLine { Special = 23, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, ceiling, true));
        Assert.False(LineSpecials.ActivateMapLine(sim, player, ceiling, true));
        Assert.True(LineSpecials.ActivateMapLine(sim, player, floor, true));
        sim.Tick();
        Assert.Equal(127.5, sim.CeilingOf(0)); Assert.Equal(1, sim.FloorOf(0)); Assert.Equal(1, player.Z.ToDouble());
    }

    [Fact]
    public void CrushRaiseStayUsesHalfSpeedReturnAndTerminates()
    {
        var sim = Room(16, extended: true); var player = sim.Players.Single(); player.Solid = false;
        Assert.True(LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = 45, Arg0 = 7, Arg1 = 8, Arg2 = 10, PlayerUse = true }, true));
        for (var tick = 0; tick < 8; tick++) sim.Tick();
        Assert.Equal(8, sim.CeilingOf(0)); sim.Tick(); Assert.Equal(8.5, sim.CeilingOf(0));
        for (var tick = 0; tick < 24; tick++) sim.Tick();
        Assert.Equal(16, sim.CeilingOf(0));
    }

    [Fact]
    public void PauseChangesChecksumBeforePlanePositionsDiverge()
    {
        var first = Room(128); var second = Room(128);
        Activate(first, 73); Activate(second, 73); Activate(first, 74);
        // Refresh checksums through pose restore without advancing either plane.
        first.RestoreState(first.CaptureState()); second.RestoreState(second.CaptureState());
        Assert.Equal(first.CeilingOf(0), second.CeilingOf(0));
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    [Fact]
    public void FractionalPlanesRoundTripInVersionFiveAndVersionFourStillLoads()
    {
        var state = new SimSaveState(); state.Sectors.Add((0.125, 128.875));
        var bytes = SimSavegame.Write(state);
        Assert.Equal(6, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var restored, out var error), error);
        Assert.Equal(state.Sectors, restored.Sectors);
        var legacy = new byte[32];
        "HCSV"u8.CopyTo(legacy);
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), 4);
        BinaryPrimitives.WriteInt32LittleEndian(legacy.AsSpan(16), 1);
        BinaryPrimitives.WriteInt16LittleEndian(legacy.AsSpan(20), -12);
        BinaryPrimitives.WriteInt16LittleEndian(legacy.AsSpan(22), 100);
        Assert.True(SimSavegame.TryRead(legacy, out restored, out error), error);
        Assert.Equal((-12d, 100d), Assert.Single(restored.Sectors));
    }

    [Fact]
    public void FractionalFloorSpeedAndCeilingFloorGapArePreserved()
    {
        var sim = Room(16, extended: true); var player = sim.Players.Single(); player.Solid = false;
        Assert.True(LineSpecials.ActivateMapLine(sim, player, new LevelLine {
            Special = 23, Arg0 = 7, Arg1 = 1, Arg2 = 1, PlayerUse = true,
        }, true));
        sim.Tick(); Assert.Equal(0.125, sim.FloorOf(0));
        for (var i = 0; i < 7; i++) sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, player, new LevelLine {
            Special = 254, Arg0 = 7, Arg1 = 8, Arg4 = 8, PlayerUse = true,
        }, true));
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(9, sim.CeilingOf(0)); // The explicit gap of 8 must not become the default gap of 0.
    }

    [Fact]
    public void SimultaneousPlanesCannotInvertSectorAndRemovedCeilingCanRestart()
    {
        var sim = Room(64, extended: true); var player = sim.Players.Single(); player.Solid = false;
        bool Use(int special, int arg1, int arg2) => LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = special, Arg0 = 7, Arg1 = arg1, Arg2 = arg2, PlayerUse = true }, true);
        Assert.True(Use(40, 8, 16));
        Assert.True(Use(44, 2, 0)); // Remove, not pause.
        Assert.True(Use(40, 512, 64));
        Assert.True(Use(23, 512, 64));
        sim.Tick();
        Assert.True(sim.FloorOf(0) <= sim.CeilingOf(0));
        Assert.NotEmpty(SimSavegame.Write(sim));
    }

    [Fact]
    public void ObstructedFinalCeilingStepRollsBackButCompletesNativeLeg()
    {
        var sim = Room(57, extended: true); var player = sim.Players.Single();
        var line = new LevelLine { Special = 40, Arg0 = 7, Arg1 = 16, Arg2 = 2, PlayerUse = true, Repeat = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        sim.Tick(); Assert.Equal(57, sim.CeilingOf(0)); Assert.Equal(100, player.Health);
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true)); // Prior thinker terminated despite rollback.
    }

    [Theory]
    [InlineData(double.NaN, 128)]
    [InlineData(0, double.PositiveInfinity)]
    [InlineData(129, 128)]
    [InlineData(-40000, 128)]
    public void InvalidPlaneRestoreIsRejectedBeforeMutation(double floor, double ceiling)
    {
        var sim = Room(); var checksum = sim.Checksum;
        var state = new SimSaveState { Tic = 500 }; state.Sectors.Add((floor, ceiling));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Thinkers.Clock.Tic); Assert.Equal(checksum, sim.Checksum);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }
}
