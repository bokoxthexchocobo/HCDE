using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsFloorTests
{
    [Theory]
    [InlineData(0, 24)]
    [InlineData(1, 24)]
    [InlineData(2, 24)]
    [InlineData(0, 256)]
    [InlineData(1, 256)]
    [InlineData(2, 256)]
    public void HighestNeighborFloorSelectsMaximumAndPreservesDirection(int route, int special)
    {
        var sim = NearestRoom(special == 24 ? 16.5 : 32.5, [0, 24.5, 8.5]);
        var words = new List<int>();
        if (route == 2)
        {
            var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        }
        else if (route == 1) words.AddRange([3, 7, 3, 16, 3, 0, 6, special]);
        else words.AddRange([11, special, 7, 16, 0]);
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(24.5, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(24, 0, 8)]
    [InlineData(256, 24, 32)]
    public void HighestNeighborFloorClampsOppositeSideTargetOnFirstTick(int special, int first, int second)
    {
        var sim = NearestRoom(16, [first, second]);
        Add(sim, 11, special, 7, 8, 0, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(second, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
    }

    [Theory]
    [InlineData(24)]
    [InlineData(256)]
    public void HighestNeighborFloorRejectsUnsupportedChange(int special)
    {
        var sim = NearestRoom(16, [24]);
        Add(sim, 3, 7, 3, 7, 3, 8, 3, 1, 3, 0, 3, 0, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(16, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(32, 40, 128)]
    [InlineData(144, 160, 128)]
    public void LowerToLowestKeepsDownwardDirectionForHigherNeighbor(int first, int second, int ceiling)
    {
        var sim = NearestRoom(16, [first, second], ceiling);
        Add(sim, 11, 21, 7, 8, 0, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(first, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(23, 24)]
    [InlineData(37, 40)]
    public void RaisingCeilingRevealsOriginalFloorDestination(int special, int amount)
    {
        var sim = NearestRoom(16, [16], ceiling: 24);
        Add(sim, 13, special, 7, 8, amount, 0, 0, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 16, Arg2 = 16, PlayerUse = true }, true));
        for (var i = 0; i < 23; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(40, sim.FloorOf(0)); Assert.Equal(40, sim.CeilingOf(0));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Fact]
    public void AbsoluteCrushingFloorStackResultAndTagWaitComplete()
    {
        var sim = Room(16);
        Add(sim, 3, 7, 3, 7, 3, 16, 3, 24, 3, 8, 3, 1, 382, 279, 5, 112,
            62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(1, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(24, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(false, 37, 24, 0, 24)]
    [InlineData(true, 37, 8, 0, 8)]
    [InlineData(false, 68, 3, 0, 24)]
    [InlineData(true, 68, 1, 0, 8)]
    [InlineData(false, 37, 8, 1, -8)]
    [InlineData(true, 68, 1, -1, -8)]
    [InlineData(false, 37, -8, 1, 8)]
    [InlineData(true, 68, -1, 0, -8)]
    public void AbsoluteFloorUsesSignedWorldHeight(bool stack, int special, int height, int negative, int expected)
    {
        var sim = Room(16); var words = new List<int>();
        if (stack) words.AddRange([3, 7, 3, 16, 3, height, 3, negative, 3, 0, 8, special]);
        else words.AddRange([13, special, 7, 16, height, negative, 0]);
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        var duration = Math.Abs(expected - 16) / 2;
        for (var i = 0; i < duration - 1; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(37, 24)]
    [InlineData(68, 3)]
    public void AbsoluteFloorMapActivationUsesWorldHeight(int special, int height)
    {
        var sim = Room(16);
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, Arg2 = height, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        for (var i = 0; i < 4; i++) sim.Tick(); Assert.Equal(24, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(37, int.MinValue, 1, 0)]
    [InlineData(68, 536870913, 0, 0)]
    [InlineData(37, 24, 0, 1)]
    [InlineData(68, 3, 0, 1)]
    public void AbsoluteFloorRejectsOverflowAndChanges(int special, int height, int negative, int change)
    {
        var sim = Room(16);
        Add(sim, 3, 7, 3, 7, 3, 8, 3, height, 3, negative, 3, change, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(16, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(0, 66, 8)]
    [InlineData(1, 66, 8)]
    [InlineData(2, 66, 8)]
    [InlineData(0, 67, 24)]
    [InlineData(1, 67, 24)]
    [InlineData(2, 67, 24)]
    public void InstantFloorUsesScaledDistanceOnFirstTickAndIgnoresSpeedArgument(int route, int special, int expected)
    {
        var sim = Room(16); var words = new List<int>();
        if (route == 2)
        {
            var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = -99, Arg2 = 1, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        }
        else if (route == 1) words.AddRange([3, 7, 3, -99, 3, 1, 3, 0, 3, 0, 8, special]);
        else words.AddRange([13, special, 7, -99, 1, 0, 0]);
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(66)]
    [InlineData(67)]
    public void ZeroDistanceInstantFloorCompletesOnFirstTick(int special)
    {
        var sim = Room(16); Add(sim, 11, special, 7, 0, 0, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(128, sim.LightOf(0)); Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        sim.Tick(); Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(20, 8)]
    [InlineData(21, 0)]
    [InlineData(22, 0)]
    [InlineData(23, 8)]
    [InlineData(25, 0)]
    [InlineData(35, 1)]
    [InlineData(36, 1)]
    public void ZeroSpeedFloorReportsSuccessAndKeepsTagWaitBlocked(int special, int amount)
    {
        var sim = NearestRoom(16, [8, 24]);
        Add(sim, 3, 7, 3, 7, 3, 0, 3, amount, 3, 0, 3, 0, 263, special, 5, 112,
            62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(1, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(129, sim.CeilingOf(0)); Assert.Equal(16, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(23)]
    public void ZeroSpeedFloorAtDestinationCompletesOnFirstTick(int special)
    {
        var sim = NearestRoom(16, [16]);
        var line = new LevelLine { Special = special, Arg0 = 7, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        sim.Tick(); Assert.Equal(16, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(23)]
    [InlineData(25)]
    [InlineData(35)]
    [InlineData(36)]
    public void NoTravelFloorSucceedsAndBlocksCompetingFloorUntilTick(int special)
    {
        var sim = NearestRoom(16, [16]);
        Add(sim, 3, 7, 3, 7, 3, 8, 3, 0, 3, 0, 3, 0, 263, special, 5, 112,
            62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0)); Assert.Equal(1, sim.Acs.RunningCount);
        Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        sim.Tick(); Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(23)]
    [InlineData(35)]
    [InlineData(36)]
    public void ZeroDistanceMapFloorConsumesOneShotWithoutBlockingCeiling(int special)
    {
        var sim = NearestRoom(16, [16]);
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 8, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(129, sim.CeilingOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(0, 35, 24)]
    [InlineData(1, 35, 24)]
    [InlineData(2, 35, 24)]
    [InlineData(0, 36, 8)]
    [InlineData(1, 36, 8)]
    [InlineData(2, 36, 8)]
    public void TimesEightFloorScalesDistanceWithoutScalingSpeed(int route, int special, int expected)
    {
        var sim = Room(16); var words = new List<int>();
        if (route == 2)
        {
            var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, Arg2 = 1, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        }
        else if (route == 1) words.AddRange([3, 7, 3, 16, 3, 1, 3, 0, 3, 0, 8, special]);
        else words.AddRange([13, special, 7, 16, 1, 0, 0]);
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(35, -1, 0)]
    [InlineData(36, -1, 0)]
    [InlineData(35, 536870913, 0)]
    [InlineData(36, 536870913, 0)]
    [InlineData(35, 1, 1)]
    [InlineData(36, 1, 1)]
    public void TimesEightFloorRejectsInvalidDistanceAndChanges(int special, int distance, int change)
    {
        var sim = Room(16);
        Add(sim, 3, 7, 3, 7, 3, 8, 3, distance, 3, change, 3, 0, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(16, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void LowerToNearestUsesClosestLowerFractionalNeighbor(int route)
    {
        var sim = NearestRoom(24.5, [0, 16.5, 24.5, 40]);
        var words = new List<int>();
        if (route == 2)
        {
            var line = new LevelLine { Special = 22, Arg0 = 7, Arg1 = 16, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(0, line.Special);
        }
        else if (route == 1) words.AddRange([3, 7, 3, 16, 3, 0, 6, 22]);
        else words.AddRange([11, 22, 7, 16, 0]);
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        Assert.Equal(18.5, sim.FloorOf(0)); sim.Tick();
        Assert.Equal(16.5, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263)]
    [InlineData(382)]
    public void LowerToNearestWithoutLowerNeighborOwnsFloorUntilTick(int opcode)
    {
        var sim = NearestRoom(16, [16, 24]);
        Add(sim, 3, 7, 3, 7, 3, 8, 3, 0, 3, 0, 3, 0, opcode, 22, 5, 112, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(1, sim.LightOf(0));
        Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        sim.Tick(); Assert.Equal(16, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Fact]
    public void LowerToNearestRejectsChangeWithoutStartingMover()
    {
        var sim = NearestRoom(16, [8]);
        Add(sim, 3, 7, 3, 7, 3, 8, 3, 1, 3, 0, 3, 0, 263, 22, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(16, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void LowerToHighestAppliesAdjustmentAndExactHereticOption(int route)
    {
        (double current, int adjustment, int heretic, double expected)[] cases =
        [
            (24.5, 130, 0, 18.5), (24.5, 126, 0, 14.5),
            (16.5, 126, 0, 16.5), (16.5, 126, 1, 14.5),
            (16.5, 126, 2, 16.5), (16.5, 130, 1, 18.5),
        ];
        foreach (var item in cases)
        {
            var sim = NearestRoom(item.current, [0, 16.5, 8]);
            var words = new List<int>();
            int[] args = [7, 16, item.adjustment, item.heretic, 0];
            if (route == 2)
            {
                var line = new LevelLine { Special = 242, Arg0 = 7, Arg1 = 16,
                    Arg2 = item.adjustment, Arg3 = item.heretic, PlayerUse = true };
                Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
                Assert.Equal(0, line.Special);
            }
            else if (route == 1)
            {
                foreach (var arg in args) words.AddRange([3, arg]);
                words.AddRange([8, 242]);
            }
            else { words.AddRange([13, 242]); words.AddRange(args); }
            words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
            Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
            var ticks = Math.Max(1, (int)Math.Ceiling((item.current - item.expected) / 2));
            for (var i = 1; i < ticks; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
            sim.Tick();
            Assert.Equal(item.expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
            Assert.Equal(0, sim.Acs.RunningCount);
            Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        }
    }

    [Theory]
    [InlineData(0, 238)]
    [InlineData(1, 238)]
    [InlineData(2, 238)]
    [InlineData(0, 258)]
    [InlineData(1, 258)]
    [InlineData(2, 258)]
    [InlineData(0, 259)]
    [InlineData(1, 259)]
    [InlineData(2, 259)]
    public void CeilingTargetedFloorsPreserveDirectionGapAndTagWait(int route, int special)
    {
        // Neighbor minimum below, above and equal to the sector ceiling.
        foreach (var neighbor in new[] { 10.5, 40.5, 24.5 })
        {
            var sim = NearestRoom(special == 258 ? 20.5 : 0.5, [0, 0], 24.5, [neighbor, neighbor + 8]);

            var expected = special == 238 ? Math.Min(24.5, neighbor) : special == 258 ? neighbor : 18.5;
            int[] args = [7, 16, 0, 0, 6]; var words = new List<int>();
            if (route == 2)
            {
                var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, Arg4 = 6, PlayerUse = true };
                Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
                Assert.Equal(0, line.Special);
            }
            else if (route == 1)
            {
                foreach (var arg in args) words.AddRange([3, arg]);
                words.AddRange([8, special]);
            }
            else { words.AddRange([13, special]); words.AddRange(args); }
            words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
            var distance = special == 258 ? 20.5 - expected : expected - 0.5;
            var ticks = Math.Max(1, (int)Math.Ceiling(distance / 2));
            for (var i = 1; i < ticks; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
            sim.Tick(); Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
            Assert.Equal(0, sim.Acs.RunningCount);
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(),
                new LevelLine { Special = 20, Arg0 = 7, Arg1 = 8, Arg2 = 1, PlayerUse = true }, true));
        }
    }

    [Theory]
    [InlineData(238)]
    [InlineData(258)]
    [InlineData(259)]
    public void CeilingTargetedFloorsRejectUnsupportedChange(int special)
    {
        var sim = NearestRoom(0, [0]);
        Add(sim, 3, 7, 3, 7, 3, 8, 3, 1, 3, 0, 3, 0, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(0, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(0, 257)]
    [InlineData(1, 257)]
    [InlineData(2, 257)]
    [InlineData(0, 260)]
    [InlineData(1, 260)]
    [InlineData(2, 260)]
    public void FixedSpeedFloorsUseNativeArgumentsAndDirection(int route, int special)
    {
        foreach (var initial in new[] { 0.5, 8.5, 16.5 })
        {
            var sim = NearestRoom(initial, [12.5, 8.5, 24.5], 32.5);
            var expected = special == 257 ? 8.5 : 24.5;
            int[] args = special == 257 ? [7, 255, 0, 0, 0] : [7, 0, 0, 8, 123];
            var words = new List<int>();
            if (route == 2)
            {
                var line = new LevelLine { Special = special, Arg0 = args[0], Arg1 = args[1],
                    Arg2 = args[2], Arg3 = args[3], Arg4 = args[4], PlayerUse = true };
                Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
                Assert.Equal(0, line.Special);
            }
            else if (route == 1)
            {
                foreach (var arg in args) words.AddRange([3, arg]);
                words.AddRange([8, special]);
            }
            else { words.AddRange([13, special]); words.AddRange(args); }
            words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
            Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
            var ticks = special == 257 ? Math.Max(1, (int)Math.Ceiling((expected - initial) / 2)) : 1;
            for (var i = 1; i < ticks; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
            sim.Tick(); Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
            Assert.Equal(0, sim.Acs.RunningCount);
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(),
                new LevelLine { Special = 20, Arg0 = 7, Arg1 = 8, Arg2 = 1, PlayerUse = true }, true));
        }
    }

    [Theory]
    [InlineData(-8, 40.5, true)]
    [InlineData(16, 16.5, true)]
    [InlineData(24, 16.5, false)]
    public void InstantCeilingFloorRetainsSignedGapAndZeroSpeed(int gap, double expected, bool completed)
    {
        var sim = NearestRoom(16.5, [0], 32.5);
        Add(sim, 13, 260, 7, 0, 0, gap, 0, 62, 7, 10, 112, 7, 35, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(completed ? 35 : 128, sim.LightOf(0));
        Assert.Equal(completed ? 0 : 1, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(257, 0, 1)]
    [InlineData(260, 1, 0)]
    public void FixedSpeedFloorsRejectChangeInNativeArgumentSlot(int special, int arg1, int arg2)
    {
        var sim = NearestRoom(0, [8]);
        Add(sim, 3, 7, 3, 7, 3, arg1, 3, arg2, 3, 0, 3, 0, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(0, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(0, 28)]
    [InlineData(1, 28)]
    [InlineData(2, 28)]
    [InlineData(0, 99)]
    [InlineData(1, 99)]
    [InlineData(2, 99)]
    public void FloorCrushersSelectNativeCeilingAndClearance(int route, int special)
    {
        // The Doom variant compares the neighbor minus clearance with the own
        // ceiling before substituting own ceiling minus clearance.
        foreach (var neighbor in new[] { 12.5, 24.5, 28.5, 40.5 })
        {
            var sim = NearestRoom(0.5, [0, 0], 24.5, [neighbor + 4, neighbor]);
            var expected = special == 28 || neighbor - 8 > 24.5 ? 16.5 : neighbor - 8;
            var words = new List<int>(); int[] args = [7, 16, 10, 1, 0];
            if (route == 2)
            {
                var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, Arg2 = 10, Arg3 = 1, PlayerUse = true };
                Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
                Assert.Equal(0, line.Special);
            }
            else if (route == 1)
            {
                foreach (var arg in args) words.AddRange([3, arg]);
                words.AddRange([8, special]);
            }
            else { words.AddRange([13, special]); words.AddRange(args); }
            words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
            Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
            var ticks = (int)((expected - 0.5) / 2);
            for (var i = 1; i < ticks; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
            sim.Tick(); Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
            Assert.Equal(0, sim.Acs.RunningCount);
            Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        }
    }

    private static AuthoritySimulation NearestRoom(double floor, double[] neighbors, double ceiling = 128, double[]? neighborCeilings = null)
    {
        var sectors = new List<LevelSector> { new() { Tag = 7, FloorHeight = floor, CeilingHeight = ceiling, LightLevel = 128 } };
        sectors.AddRange(neighbors.Select((height, i) => new LevelSector { Index = i + 1, FloorHeight = height, CeilingHeight = neighborCeilings?[i] ?? Math.Max(ceiling, height) }));
        return AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary, Sectors = sectors,
            Sides = sectors.Select((_, i) => new LevelSide { Sector = i }).ToArray(),
            Lines = neighbors.Select((_, i) => new LevelLine { SideFront = 0, SideBack = i + 1, Flags = 4 }).ToArray(),
        });
    }

    [Theory]
    [InlineData(false, 20, 6, 2)]
    [InlineData(true, 20, 6, 2)]
    [InlineData(false, 23, 6, 14)]
    [InlineData(true, 23, 6, 14)]
    [InlineData(false, 21, 0, 0)]
    [InlineData(true, 25, 0, 16)]
    [InlineData(false, 62, 2, 16)]
    [InlineData(true, 62, 2, 16)]
    public void ScriptFloorAndLiftCallsCompleteBeforeTagWaitContinues(bool stack, int special, int arg2, int expected)
    {
        var sim = Room(special == 62 ? 16 : 8); var words = new List<int>(); int[] args = [7, 16, arg2, 0, 0];
        if (stack) { foreach (var arg in args) words.AddRange([3, arg]); words.AddRange([8, special]); }
        else { words.AddRange([13, special]); words.AddRange(args); }
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        int duration = special == 62 ? 10 : special is 20 or 23 ? 3 : 4;
        for (var i = 0; i < duration - 1; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(35, sim.LightOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(20, 2, 1, 0)]
    [InlineData(21, 1, 0, 0)]
    [InlineData(23, 2, 1, 10)]
    [InlineData(25, 1, 10, 0)]
    [InlineData(62, -1, 0, 0)]
    public void UnsupportedArgumentsReturnFalseWithoutStartingMotion(int special, int arg2, int arg3, int arg4)
    {
        var sim = Room(8); Add(sim, 3, 7, 3, 7, 3, 16, 3, arg2, 3, arg3, 3, arg4, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(8, sim.FloorOf(0));
    }

    [Fact]
    public void TruncatedDirectFloorCallDoesNotStartMovement()
    {
        var sim = Room(8); Add(sim, 11, 23, 7, 16);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Add(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
    }
    private static AuthoritySimulation Room(int floor) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, FloorHeight = floor, CeilingHeight = 128, LightLevel = 128 },
            new LevelSector { Index = 1, CeilingHeight = 128 }, new LevelSector { Index = 2, FloorHeight = 16, CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }, new LevelSide { Sector = 2 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1 }, new LevelLine { SideFront = 0, SideBack = 2 }]
    });
}
