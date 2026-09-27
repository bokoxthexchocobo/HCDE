using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsCeilingTests
{
    [Theory]
    [InlineData(42, 0)]
    [InlineData(43, 1)]
    [InlineData(45, 2)]
    public void CrusherClearanceAboveStartUsesNativeDirectionAndLegLifecycle(int special, int completionTicks)
    {
        var sim = NeighborRoom(4, [4]);
        CallAndWait(sim, false, special, [7, 8, 10, 1]);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
        Assert.Equal(completionTicks == 1 ? 35 : 128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(completionTicks == 1 ? 8 : 4, sim.CeilingOf(0));
        Assert.Equal(completionTicks == 0 ? 128 : 35, sim.LightOf(0));
        sim.Tick(); Assert.Equal(completionTicks == 2 ? 4 : 8, sim.CeilingOf(0));
        Assert.Equal(completionTicks == 0 ? 1 : 0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false, 97)]
    [InlineData(true, 97)]
    [InlineData(false, 104)]
    [InlineData(true, 104)]
    [InlineData(false, 168)]
    [InlineData(true, 168)]
    public void DistanceCrusherAcceptsTargetAboveStartingCeiling(bool stack, int special)
    {
        var sim = NeighborRoom(16, [16]);
        CallAndWait(sim, stack, special, special == 97 ? [7, 8, 10, 24, 1] : [7, 24, 8, 10, 1]);
        sim.Tick(); Assert.Equal(24, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(special == 97 ? 24 : 16, sim.CeilingOf(0));
        Assert.Equal(special == 97 ? 35 : 128, sim.LightOf(0));
        Assert.Equal(special == 97 ? 0 : 1, sim.Acs.RunningCount);
    }

    [Fact]
    public void MapCrusherWithClearanceAboveStartConsumesOneShot()
    {
        var sim = NeighborRoom(4, [4]);
        var line = new LevelLine { Special = 43, Arg0 = 7, Arg1 = 8, Arg2 = 10, Arg3 = 1, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void NegativeUpwardFloorGapCanReachBelowOwnFloor(int route)
    {
        var sim = NeighborRoom(16, [16]);
        if (route == 2)
        {
            var line = new LevelLine { Special = 267, Arg0 = 7, Arg3 = -8, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, 267, [7, 0, 0, -8]);
        sim.Tick(); Assert.Equal(-8, sim.CeilingOf(0)); Assert.Equal(0, sim.FloorOf(0));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(262)]
    [InlineData(265)]
    [InlineData(266)]
    public void UpwardNeighborTargetsAreNotClampedToOwnFloor(int special)
    {
        var sim = NeighborRoom(16, [-8], floors: [-8]);
        CallAndWait(sim, false, special, [7, 8, 0]);
        sim.Tick(); Assert.Equal(-8, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void LoweringFloorRevealsOriginalCeilingTargetRatherThanCreationTimeClamp()
    {
        var sim = NeighborRoom(16, [16]);
        CallAndWait(sim, false, 254, [7, 8, 0, 0, -8]);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 20, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
        for (var i = 0; i < 23; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(-8, sim.FloorOf(0)); Assert.Equal(-8, sim.CeilingOf(0));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void NegativeUpwardFloorGapStillRollsBackWhenActorBlocksIt()
    {
        var sim = NeighborRoom(64, [64], player: true); sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 267, [7, 0, 99, -8]); sim.Tick();
        Assert.Equal(64, sim.CeilingOf(0)); Assert.Equal(100, sim.Players.Single().Health);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0, 192, 16, 2)]
    [InlineData(1, 192, 16, 2)]
    [InlineData(2, 192, 16, 2)]
    [InlineData(0, 254, 0, 4)]
    [InlineData(1, 254, 0, 4)]
    [InlineData(2, 254, 0, 4)]
    public void NegativeLoweringGapOffsetsTargetAndClampsAtOwnFloor(int route, int special, int expected, int ticks)
    {
        var sim = NeighborRoom(32, [64, 64], floors: [8, 24]);
        if (route == 2)
        {
            var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 64, Arg4 = -8, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, special, [7, 64, 0, 0, -8]);
        for (var i = 0; i < ticks - 1; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false, 97)]
    [InlineData(true, 97)]
    [InlineData(false, 104)]
    [InlineData(true, 104)]
    [InlineData(false, 168)]
    [InlineData(true, 168)]
    public void NegativeCrusherDistanceReachesFloorAndRetainsLoopBehavior(bool stack, int special)
    {
        var sim = NeighborRoom(16, [16]);
        CallAndWait(sim, stack, special, special == 97 ? [7, 16, 10, -8, 1] : [7, -8, 16, 10, 1]);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(0, sim.CeilingOf(0)); Assert.Equal(special == 97 ? 35 : 128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(special == 97 ? 0 : 2, sim.CeilingOf(0));
        Assert.Equal(special == 97 ? 0 : 1, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0, 263)]
    [InlineData(1, 263)]
    [InlineData(2, 263)]
    [InlineData(0, 267)]
    [InlineData(1, 267)]
    [InlineData(2, 267)]
    public void InstantTargetSpecialsReachOppositeSideTargetsOnFirstTick(int route, int special)
    {
        var sim = NeighborRoom(16, [24, 32]);
        if (route == 2)
        {
            var line = new LevelLine { Special = special, Arg0 = 7, Arg3 = 4, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, special, [7, 0, 0, 4]);
        Assert.Equal(16, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(special == 263 ? 32 : 4, sim.CeilingOf(0));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 8)]
    [InlineData(267, 24)]
    public void InstantTargetSpecialsUseNativeTwoUnitSpeedForOrdinaryDirection(int special, int expected)
    {
        var sim = NeighborRoom(16, [0, 8]);
        CallAndWait(sim, false, special, [7, 0, 0, 24]);
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void ToFloorInstantRollsBackBlockedUpwardThinkerWithoutCrushDamage()
    {
        var sim = NeighborRoom(64, [64], player: true);
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 267, [7, 0, 99, 48]);
        sim.Tick(); Assert.Equal(64, sim.CeilingOf(0)); Assert.Equal(100, sim.Players.Single().Health);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0, 192)]
    [InlineData(1, 192)]
    [InlineData(2, 192)]
    [InlineData(0, 265)]
    [InlineData(1, 265)]
    [InlineData(2, 265)]
    [InlineData(0, 266)]
    [InlineData(1, 266)]
    [InlineData(2, 266)]
    public void NeighborHeightCeilingVariantsUseCorrectPlaneAndGap(int route, int special)
    {
        var sim = NeighborRoom(special == 192 ? 64.5 : 16.5, [40, 24.5, 32], floors: [8.5, 16.5, 24.5]);
        var speed = special == 192 ? 64 : 16;
        var gap = special == 192 ? 8 : 0;
        if (route == 2)
        {
            var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = speed, Arg4 = gap, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, special, [7, speed, 0, 0, gap]);
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(special == 192 ? 32.5 : 24.5, sim.CeilingOf(0));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(265)]
    [InlineData(266)]
    public void RaisingNeighborVariantsClampLowerDestinationOnFirstTick(int special)
    {
        var sim = NeighborRoom(64, [48, 56], floors: [40, 48]);
        CallAndWait(sim, false, special, [7, 8, 0]);
        sim.Tick(); Assert.Equal(48, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(192)]
    [InlineData(265)]
    [InlineData(266)]
    public void NeighborHeightCeilingRejectsUnsupportedChanges(int special)
    {
        var sim = NeighborRoom(16, [24], floors: [8]);
        Add(sim, 1, 3, 7, 3, 7, 3, 8, 3, 1, 3, 0, 3, 0, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.LightOf(0)); sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void LowerToLowestSelectsLowestNeighborRatherThanNearest(int route)
    {
        var sim = NeighborRoom(32.5, [40, 24.5, 8.5, 16.5]);
        if (route == 2)
        {
            var line = new LevelLine { Special = 253, Arg0 = 7, Arg1 = 64, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, 253, [7, 64, 0, 0]);
        sim.Tick(); Assert.Equal(24.5, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(16.5, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(8.5, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(253, 24, 32, 24)]
    [InlineData(262, 0, 8, 8)]
    public void FixedDirectionCeilingClampsOppositeSideTargetOnFirstTick(int special, int first, int second, int expected)
    {
        var sim = NeighborRoom(16, [first, second]);
        CallAndWait(sim, false, special, [7, 8, 0, 0]);
        Assert.Equal(16, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0, 56, 100)]
    [InlineData(10, 48, 90)]
    public void LowerToLowestUsesCrushArgument(int damage, int expected, int health)
    {
        var sim = NeighborRoom(64, [48], player: true); sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 253, [7, 16, 0, damage]);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(damage > 0 ? 0 : 1, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false, 0, 54.875)]
    [InlineData(true, 0, 56)]
    [InlineData(true, -1, 56)]
    [InlineData(true, 4, 56)]
    [InlineData(true, 1, 54)]
    [InlineData(true, 2, 56)]
    [InlineData(true, 3, 54.875)]
    public void HexenCrushDefaultsAreExplicitAndRespectModeOverrides(bool hexen, int mode, double expected)
    {
        var sim = NeighborRoom(64, [64], player: true,
            compat: hexen ? CompatSurface.HexenCrushDefaults : CompatSurface.None);
        sim.Tick(); Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 43, [7, 8, 10, mode]);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(expected, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(3, true)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void HexenCrushStopDefaultsRemoveUnlessPauseIsExplicit(int mode, bool removes)
    {
        var sim = NeighborRoom(16, [16], compat: CompatSurface.HexenCrushDefaults);
        CallAndWait(sim, false, 41, [7, 8, 8]);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 44, Arg0 = 7, Arg1 = mode, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
        Assert.Equal(removes ? 35 : 128, sim.LightOf(0));
        Assert.Equal(removes ? 0 : 1, sim.Acs.RunningCount);
    }

    [Fact]
    public void CompatibilityDefaultsParticipateInSimulationChecksum()
    {
        var normal = NeighborRoom(16, [16]);
        var hexen = NeighborRoom(16, [16], compat: CompatSurface.HexenCrushDefaults);
        var repeat = NeighborRoom(16, [16], compat: CompatSurface.HexenCrushDefaults);
        Assert.NotEqual(normal.Checksum, hexen.Checksum); Assert.Equal(hexen.Checksum, repeat.Checksum);
    }

    [Theory]
    [InlineData(43, -1)]
    [InlineData(43, 4)]
    [InlineData(97, -1)]
    [InlineData(97, 4)]
    [InlineData(104, -1)]
    [InlineData(104, 4)]
    [InlineData(168, -1)]
    [InlineData(168, 4)]
    [InlineData(196, -1)]
    [InlineData(196, 4)]
    [InlineData(197, -1)]
    [InlineData(197, 4)]
    public void OutOfRangeCrusherModeUsesNativeDefaultSlowdown(int special, int mode)
    {
        var sim = NeighborRoom(64, [64], player: true); sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        int[] args = special switch
        {
            43 => [7, 8, 10, mode],
            97 => [7, 8, 10, 8, mode],
            104 or 168 => [7, 8, 8, 10, mode],
            _ => [7, 8, 8, 10, mode],
        };
        CallAndWait(sim, false, special, args);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(54.875, sim.CeilingOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0, 8, 54.875)]
    [InlineData(1, 8, 54)]
    [InlineData(2, 8, 56)]
    [InlineData(3, 8, 54.875)]
    [InlineData(4, 16, 44)]
    public void CrusherModeDefaultsRetainExplicitOverridesAndSpeedCondition(int mode, int speed, double expected)
    {
        var sim = NeighborRoom(64, [64], player: true); sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 43, [7, speed, 10, mode]);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(expected, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(40)]
    [InlineData(41)]
    [InlineData(47)]
    [InlineData(69)]
    [InlineData(42)]
    [InlineData(196)]
    public void ZeroSpeedCeilingSucceedsAndKeepsOwnershipUntilStopped(int special)
    {
        var sim = NeighborRoom(16, [16]);
        Add(sim, 1, 3, 7, 3, 7, 3, 0, 3, 8, 3, 0, 3, 0, 263, special, 5, 112,
            62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(1, sim.LightOf(0));
        Assert.Equal(1, sim.Acs.RunningCount);
        Assert.False(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 276, Arg0 = 7, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(195)]
    [InlineData(196)]
    public void ZeroReturnSpeedCrusherReachesFloorThenKeepsOwnership(int special)
    {
        var sim = NeighborRoom(16, [16]);
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, Arg2 = 0, Arg3 = 10, Arg4 = 1, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 8; i++) sim.Tick(); Assert.Equal(0, sim.CeilingOf(0));
        for (var i = 0; i < 8; i++) sim.Tick(); Assert.Equal(0, sim.CeilingOf(0));
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 276, Arg0 = 7, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(40)]
    [InlineData(41)]
    public void ZeroSpeedAtDestinationStillCompletesOnFirstTick(int special)
    {
        var sim = NeighborRoom(16, [16]);
        CallAndWait(sim, false, special, [7, 0, 0, 0, 0]); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false, 47, 24, 0, 24)]
    [InlineData(true, 47, 8, 0, 8)]
    [InlineData(false, 69, 3, 0, 24)]
    [InlineData(true, 69, 1, 0, 8)]
    [InlineData(false, 47, 8, 1, -8)]
    [InlineData(true, 69, 1, -1, -8)]
    [InlineData(false, 47, -8, 1, 8)]
    [InlineData(true, 69, -1, 0, -8)]
    public void AbsoluteCeilingUsesSignedWorldHeight(bool stack, int special, int height, int negative, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, FloorHeight = -32, CeilingHeight = 16, LightLevel = 128 }] });
        CallAndWait(sim, stack, special, [7, 16, height, negative, 0]);
        var ticks = Math.Abs(expected - 16) / 2;
        for (var i = 0; i < ticks - 1; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(47, 24)]
    [InlineData(69, 3)]
    [InlineData(280, 24)]
    public void AbsoluteCeilingMapActivationUsesAbsoluteDestination(int special, int height)
    {
        var sim = NeighborRoom(16, [16]);
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, Arg2 = height, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        for (var i = 0; i < 4; i++) sim.Tick(); Assert.Equal(24, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(47, int.MinValue, 1, 0)]
    [InlineData(69, 536870913, 0, 0)]
    [InlineData(47, 24, 0, 1)]
    [InlineData(69, 3, 0, 1)]
    public void AbsoluteCeilingRejectsOverflowAndUnsupportedChanges(int special, int height, int negative, int change)
    {
        var sim = Room();
        Add(sim, 1, 3, 7, 3, 7, 3, 8, 3, height, 3, negative, 3, change, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.LightOf(0)); sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(1, 48, 90)]
    [InlineData(2, 56, 90)]
    [InlineData(3, 48, 90)]
    public void AbsoluteCrushingCeilingStopsOnlyForHexenCrushMode(int mode, int expected, int health)
    {
        var sim = NeighborRoom(64, [64], player: true); sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 280, [7, 16, 48, 10, mode]);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(mode == 2 ? 1 : 0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0, 193, 0)]
    [InlineData(1, 193, 0)]
    [InlineData(2, 193, 0)]
    [InlineData(0, 194, 32)]
    [InlineData(1, 194, 32)]
    [InlineData(2, 194, 32)]
    public void InstantCeilingMovesFullDistanceOnFirstTick(int route, int special, int expected)
    {
        var sim = NeighborRoom(16, [16]);
        if (route == 2)
        {
            var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = -99, Arg2 = 2, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, special, [7, -99, 2, 0, 0]);
        sim.Acs.Tick(sim); Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(193)]
    [InlineData(194)]
    public void ZeroDistanceInstantCeilingStillOwnsPlaneUntilTick(int special)
    {
        var sim = NeighborRoom(16, [16]);
        CallAndWait(sim, false, special, [7, 0, 0, 0, 0]); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0));
        Assert.False(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0, 2, 64, 100)]
    [InlineData(10, 2, 48, 90)]
    [InlineData(10, 9, 64, 90)]
    public void InstantLowerCompletesEvenWhenBlockedAndHonorsCrushArgument(int damage, int distance, int expected, int health)
    {
        var sim = NeighborRoom(64, [64], player: true);
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 193, [7, 0, distance, 0, damage]);
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
    }

    [Theory]
    [InlineData(40)]
    [InlineData(41)]
    [InlineData(198)]
    [InlineData(199)]
    [InlineData(254)]
    [InlineData(262)]
    public void ZeroTravelCeilingReportsSuccessAndRetainsOwnershipUntilTick(int special)
    {
        var sim = NeighborRoom(16, [16]);
        var gap = special == 254 ? 16 : 0;
        Add(sim, 1, 3, 7, 3, 7, 3, 8, 3, 0, 3, 0, 3, gap, 263, special, 5, 112,
            62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        var raise = new LevelLine { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true };
        Assert.False(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), raise, true));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), raise, true));
    }

    [Theory]
    [InlineData(40)]
    [InlineData(41)]
    [InlineData(198)]
    [InlineData(199)]
    public void ZeroDistanceMapCeilingConsumesOneShot(int special)
    {
        var sim = NeighborRoom(16, [16]);
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 8, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(0, line.Special); sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(43, 1)]
    [InlineData(45, 2)]
    [InlineData(42, 0)]
    public void ZeroTravelCrusherPreservesItsLegLifecycle(int special, int completionTicks)
    {
        var sim = NeighborRoom(8, [8]);
        CallAndWait(sim, false, special, [7, 8, 10, 1]);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
        for (var tick = 1; tick <= 4; tick++)
        {
            sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
            Assert.Equal(completionTicks > 0 && tick >= completionTicks ? 35 : 128, sim.LightOf(0));
        }
        if (completionTicks == 0)
        {
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = 276, Arg0 = 7, PlayerUse = true }, true));
            sim.Tick(); Assert.Equal(35, sim.LightOf(0));
        }
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 252, 8)]
    [InlineData(382, 252, 8)]
    [InlineData(263, 264, 24)]
    [InlineData(382, 264, 24)]
    public void NoTravelNearestCeilingReportsSuccessAndOwnsPlaneUntilTick(int opcode, int special, int neighbor)
    {
        var sim = NeighborRoom(16, [16, neighbor]);
        Add(sim, 1, 3, 7, 3, 7, 3, 16, 3, 0, 3, 0, 3, 0, opcode, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
        var raise = new LevelLine { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true };
        Assert.False(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), raise, true));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), raise, true));
        sim.Tick(); Assert.Equal(17, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(252, 8)]
    [InlineData(264, 24)]
    public void NoTravelNearestMapActivationConsumesOneShotAndBlocksTagWaitUntilTick(int special, int neighbor)
    {
        var sim = NeighborRoom(16, [16, neighbor]);
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 8, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        sim.Tick(); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(16, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(252, 8)]
    [InlineData(264, 24)]
    public void PausedNoTravelNearestCeilingKeepsTagWaitBlockedUntilRemoved(int special, int neighbor)
    {
        var sim = NeighborRoom(16, [16, neighbor]);
        CallAndWait(sim, false, special, [7, 8, 0, 0]);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 44, Arg0 = 7, Arg1 = 1, PlayerUse = true }, true));
        sim.Tick(); sim.Tick(); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 276, Arg0 = 7, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.Equal(16, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(0, 252, 24.5)]
    [InlineData(1, 252, 24.5)]
    [InlineData(2, 252, 24.5)]
    [InlineData(0, 264, 8.5)]
    [InlineData(1, 264, 8.5)]
    [InlineData(2, 264, 8.5)]
    public void NearestCeilingChoosesClosestHeightInRequestedDirection(int route, int special, double expected)
    {
        var sim = NeighborRoom(16.5, [40, 0, 24.5, 8.5, 16.5]);
        if (route == 2)
        {
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = special, Arg0 = 7, Arg1 = 16, PlayerUse = true }, true));
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, special, [7, 16, 0, 0]);
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(special == 252 ? 22.5 : 10.5, sim.CeilingOf(0));
        Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 252)]
    [InlineData(382, 264)]
    public void NearestCeilingRejectsUnsupportedChangeWithoutMoving(int opcode, int special)
    {
        var sim = NeighborRoom(16, [8, 24]);
        Add(sim, 1, 3, 7, 3, 7, 3, 16, 3, 1, 3, 0, 3, 0, opcode, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.LightOf(0)); sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(252, 8)]
    [InlineData(264, 24)]
    public void NearestCeilingDoesNotMoveInOppositeDirection(int special, int neighbor)
    {
        var sim = NeighborRoom(16, [16, neighbor]);
        CallAndWait(sim, false, special, [7, 16, 0, 0]); sim.Tick();
        Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(10, 48, 90)]
    [InlineData(0, 56, 100)]
    public void LowerToNearestUsesFourthArgumentForCrushDamage(int damage, int expectedCeiling, int expectedHealth)
    {
        var sim = NeighborRoom(64, [48], player: true);
        sim.Tick(); Assert.Equal(0, sim.Players.Single().SectorIndex);
        CallAndWait(sim, false, 264, [7, 16, 0, damage]);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(expectedCeiling, sim.CeilingOf(0));
        Assert.Equal(expectedHealth, sim.Players.Single().Health);
        Assert.Equal(damage > 0 ? 0 : 1, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation NeighborRoom(double ceiling, double[] neighbors, bool player = false,
        CompatSurface compat = CompatSurface.None, double[]? floors = null)
    {
        var sectors = new List<LevelSector> { new() { Tag = 7, CeilingHeight = ceiling, LightLevel = 128 } };
        sectors.AddRange(neighbors.Select((height, i) => new LevelSector { Index = i + 1, CeilingHeight = height, FloorHeight = floors?[i] ?? 0 }));
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary, Sectors = sectors,
            Sides = sectors.Select((_, i) => new LevelSide { Sector = i }).ToArray(),
            Lines = player ? [new LevelLine { X1 = -128, Y1 = -128, X2 = 128, Y2 = -128, SideFront = 0, SideBack = -1 },
                new LevelLine { X1 = 128, Y1 = -128, X2 = 128, Y2 = 128, SideFront = 0, SideBack = 1, Flags = 4 },
                new LevelLine { X1 = 128, Y1 = 128, X2 = -128, Y2 = 128, SideFront = 0, SideBack = -1 },
                new LevelLine { X1 = -128, Y1 = 128, X2 = -128, Y2 = -128, SideFront = 0, SideBack = -1 }]
                : neighbors.Select((_, i) => new LevelLine { SideFront = 0, SideBack = i + 1, Flags = 4 }).ToArray(),
            Things = player ? [new LevelThing { Type = 1 }] : [],
        }, compat: compat);
    }

    [Theory]
    [InlineData(0, 198, 24)]
    [InlineData(1, 198, 24)]
    [InlineData(2, 198, 24)]
    [InlineData(0, 199, 8)]
    [InlineData(1, 199, 8)]
    [InlineData(2, 199, 8)]
    public void TimesEightCeilingScalesDistanceAndReleasesTagWait(int route, int special, int expected)
    {
        var sim = Room();
        if (route == 2)
        {
            var line = new LevelLine
            { Special = special, Arg0 = 7, Arg1 = 16, Arg2 = 1, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
            Assert.Equal(0, line.Special);
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, special, [7, 16, 1, 0, 0]);
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(special == 198 ? 22 : 10, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(198, -1, 0)]
    [InlineData(199, -1, 0)]
    [InlineData(198, 536870913, 0)]
    [InlineData(199, 536870913, 0)]
    [InlineData(198, 1, 1)]
    [InlineData(199, 1, 1)]
    public void TimesEightCeilingRejectsInvalidDistanceAndUnsupportedChange(int special, int amount, int change)
    {
        var sim = Room();
        Add(sim, 1, 3, 7, 3, 7, 3, 8, 3, amount, 3, change, 3, 0, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.LightOf(0)); sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TimesEightLowerIgnoresDamageArgumentAndWaitsForObstruction(bool script)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 64 }],
            Things = [new LevelThing { Type = 1 }],
        });
        sim.Tick(); Assert.Equal(0, sim.Players.Single().SectorIndex);
        if (script) CallAndWait(sim, false, 199, [7, 16, 2, 0, 99]);
        else Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
        { Special = 199, Arg0 = 7, Arg1 = 16, Arg2 = 2, Arg4 = 99, PlayerUse = true }, true));
        for (var i = 0; i < 12; i++) sim.Tick();
        Assert.Equal(56, sim.CeilingOf(0)); Assert.Equal(100, sim.Players.Single().Health);
        if (script) Assert.Equal(1, sim.Acs.RunningCount);
        sim.Players.Single().Solid = false;
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(48, sim.CeilingOf(0));
        if (script) { Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount); }
    }

    [Theory]
    [InlineData(0, 195)]
    [InlineData(1, 195)]
    [InlineData(2, 195)]
    [InlineData(0, 196)]
    [InlineData(1, 196)]
    [InlineData(2, 196)]
    [InlineData(0, 197)]
    [InlineData(1, 197)]
    [InlineData(2, 197)]
    [InlineData(0, 255)]
    [InlineData(1, 255)]
    [InlineData(2, 255)]
    public void SeparateSpeedCrusherReachesFloorAndUsesIndependentReturnSpeed(int route, int special)
    {
        var sim = Room();
        if (route == 2)
        {
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = special, Arg0 = 7, Arg1 = 32, Arg2 = 16, Arg3 = 10, Arg4 = 1, PlayerUse = true }, true));
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, special, [7, 32, 16, 10, 1]);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(0, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(2, sim.CeilingOf(0));
        for (var i = 0; i < 6; i++) sim.Tick();
        Assert.Equal(14, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
        var loop = special is 196 or 197;
        Assert.Equal(loop ? 128 : 35, sim.LightOf(0));
        sim.Tick(); Assert.Equal(loop ? 12 : 16, sim.CeilingOf(0));
        Assert.Equal(loop ? 1 : 0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void LowerAndCrushDistanceStopsAtRequestedGap(int route)
    {
        var sim = Room();
        if (route == 2)
        {
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = 97, Arg0 = 7, Arg1 = 16, Arg2 = 10, Arg3 = 4, Arg4 = 1, PlayerUse = true }, true));
            Add(sim, 1, 62, 7, 10, 112, 7, 35, 1);
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        }
        else CallAndWait(sim, route == 1, 97, [7, 16, 10, 4, 1]);
        for (var i = 0; i < 5; i++) sim.Tick();
        Assert.Equal(6, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(4, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        sim.Tick(); Assert.Equal(4, sim.CeilingOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(45, 52)]
    [InlineData(195, 52)]
    [InlineData(255, 52)]
    [InlineData(196, 53.875)]
    [InlineData(197, 53.875)]
    [InlineData(97, 53.875)]
    public void ModeThreeSlowsOnlyLowerOnlyAndLoopingCrushers(int special, double expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 64 }],
            Things = [new LevelThing { Type = 1 }],
        });
        sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        var args = special == 45 ? new[] { 7, 16, 10, 3 }
            : special == 97 ? new[] { 7, 16, 10, 4, 3 } : new[] { 7, 16, 8, 10, 3 };
        CallAndWait(sim, false, special, args);
        for (var i = 0; i < 6; i++) sim.Tick();
        Assert.Equal(expected, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(false, 104)]
    [InlineData(true, 104)]
    [InlineData(false, 168)]
    [InlineData(true, 168)]
    public void DistanceCrusherUsesRequestedGapAndEqualReturnSpeed(bool stack, int special)
    {
        var sim = Room(); CallAndWait(sim, stack, special, [7, 4, 16, 10, 1]);
        for (var i = 0; i < 6; i++) sim.Tick();
        Assert.Equal(4, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(6, sim.CeilingOf(0));
        for (var i = 0; i < 5; i++) sim.Tick();
        Assert.Equal(16, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(14, sim.CeilingOf(0));
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LowerToFloorHonorsGapAndReleasesTagWait(bool stack)
    {
        var sim = Room(); CallAndWait(sim, stack, 254, [7, 16, 0, 0, 4]);
        for (var i = 0; i < 5; i++) sim.Tick();
        Assert.Equal(6, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(4, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RaiseToHighestUsesNeighborAndReleasesTagWait(bool stack)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 16, LightLevel = 128 },
                new LevelSector { CeilingHeight = 24 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { SideFront = 0, SideBack = 1, Flags = 4 }],
        });
        CallAndWait(sim, stack, 262, [7, 16, 0]);
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(22, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(24, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 254)]
    [InlineData(382, 254)]
    [InlineData(263, 262)]
    [InlineData(382, 262)]
    public void GeometryChangeFlagsFailWithoutStartingCeiling(int opcode, int special)
    {
        var sim = Room();
        Add(sim, 1, 3, 7, 3, 7, 3, 16, 3, 1, 3, 0, 3, 4, opcode, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(0, sim.LightOf(0)); sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
    }

    private static void CallAndWait(AuthoritySimulation sim, bool stack, int special, int[] args)
    {
        var words = new List<int>();
        if (stack) { foreach (var arg in args) words.AddRange([3, arg]); words.AddRange([3 + args.Length, special]); }
        else { words.AddRange([8 + args.Length, special]); words.AddRange(args); }
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, 1, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
    }

    [Theory]
    [InlineData(false, 43, 8, 4)]
    [InlineData(true, 43, 8, 4)]
    [InlineData(false, 45, 16, 12)]
    [InlineData(true, 45, 16, 12)]
    public void ScriptCrusherFinishesExpectedLegsBeforeReleasingTagWait(bool stack, int special, int expected, int ticks)
    {
        var sim = Room(); var words = new List<int>(); int[] args = [7, 16, 10, 1];
        if (stack) { foreach (var arg in args) words.AddRange([3, arg]); words.AddRange([7, special]); }
        else { words.AddRange([12, special]); words.AddRange(args); }
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, 1, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < ticks - 1; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void ScriptLoopingCrusherResumesOriginalMotionAndTagWaitRequiresRemoval()
    {
        var sim = Room(); Add(sim, 1, 12, 42, 7, 16, 10, 1, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 4; i++) sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
        Add(sim, 2, 10, 44, 7, 1, 1); Assert.True(sim.Acs.TryExecute(2, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
        Add(sim, 3, 12, 42, 7, 64, 99, 2, 1); Assert.True(sim.Acs.TryExecute(3, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(9, sim.CeilingOf(0)); // Original return speed and direction survive.
        for (var i = 0; i < 12; i++) sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        Add(sim, 4, 9, 276, 7, 1); Assert.True(sim.Acs.TryExecute(4, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 42)]
    [InlineData(382, 43)]
    [InlineData(263, 45)]
    public void CrusherResultFormsReturnSuccess(int opcode, int special)
    {
        var sim = Room(); Add(sim, 1, 3, 7, 3, 7, 3, 16, 3, 10, 3, 1, 3, 0, opcode, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(1, sim.LightOf(0));
        sim.Tick(); Assert.Equal(14, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(false, 40, 12)]
    [InlineData(true, 40, 12)]
    [InlineData(false, 41, 20)]
    [InlineData(true, 41, 20)]
    public void ScriptCeilingCallsHoldTagWaitUntilArrival(bool stack, int special, int expected)
    {
        var sim = Room(); var words = new List<int>(); int[] args = [7, 16, 4];
        if (stack) { foreach (var arg in args) words.AddRange([3, arg]); words.AddRange([6, special]); }
        else { words.AddRange([11, special]); words.AddRange(args); }
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, 1, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void ScriptPauseKeepsTagWaitBlockedAndRemoveReleasesIt()
    {
        var sim = Room(); Add(sim, 1, 11, 41, 7, 8, 10, 10, 44, 7, 1, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); sim.Tick(); Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        Add(sim, 2, 9, 276, 7, 1); Assert.True(sim.Acs.TryExecute(2, []));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 7, 0, 1)]
    [InlineData(382, 7, 0, 1)]
    [InlineData(263, 999, 0, 0)]
    [InlineData(382, 0, 0, 0)]
    [InlineData(263, 7, 1, 0)]
    public void ResultFormsReportSuccessAndUnsupportedChange(int opcode, int tag, int change, int expected)
    {
        var sim = Room(); Add(sim, 1, 3, 7, 3, tag, 3, 8, 3, 4, 3, change, 3, 0, opcode, 41, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); Assert.Equal(expected, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected == 1 ? 17 : 16, sim.CeilingOf(0));
    }

    private static void Add(AuthoritySimulation sim, int number, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, CeilingHeight = 16, LightLevel = 128 }] });
}
