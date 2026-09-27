using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SynchronizedStairTests
{
    [Theory]
    [InlineData(217, 1)]
    [InlineData(270, -1)]
    [InlineData(273, 1)]
    public void StairDelayPausesAfterEachStepInterval(int special, int sign)
    {
        var sim = Room(); Assert.True(Activate(sim, Delayed(special)));
        sim.Tick(); Assert.Equal(sign, sim.FloorOf(1));
        sim.Tick(); Assert.Equal(sign * 2, sim.FloorOf(1));
        sim.Tick(); sim.Tick(); Assert.Equal(sign * 2, sim.FloorOf(1));
        sim.Tick(); Assert.Equal(sign * 3, sim.FloorOf(1));
        sim.Tick(); Assert.Equal(sign * 4, sim.FloorOf(1));
        Assert.False(Activate(sim, Delayed(special))); // Last step still owns the chain lock.
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(sign * 6, sim.FloorOf(2)); Assert.True(Activate(sim, Delayed(special)));
    }

    [Theory]
    [InlineData(217, 1)]
    [InlineData(270, -1)]
    public void ResetDuringPauseDefersReturnOnlyOnTransitionTick(int special, int sign)
    {
        var sim = Room(); Assert.True(Activate(sim, Delayed(special, reset: 3)));
        sim.Tick(); sim.Tick(); Assert.Equal(sign * 2, sim.FloorOf(1));
        sim.Tick(); Assert.Equal(sign * 2, sim.FloorOf(1));
        sim.Tick(); Assert.Equal(sign, sim.FloorOf(1));
        sim.Tick(); Assert.Equal(0, sim.FloorOf(1)); Assert.True(Activate(sim, Delayed(special)));
    }

    [Fact]
    public void SubTickStepIntervalDoesNotIntroducePause()
    {
        var sim = Room(); Assert.True(Activate(sim, new LevelLine
            { Special = 217, Arg0 = 7, Arg1 = 32, Arg2 = 2, Arg3 = 100, PlayerUse = true }));
        sim.Tick(); Assert.Equal(4, sim.FloorOf(2));
        sim.Tick(); Assert.Equal(6, sim.FloorOf(2)); Assert.True(Activate(sim, Delayed(217)));
    }

    [Fact]
    public void NegativeDelayRejectsWithoutStartingMovers()
    {
        var sim = Room(); var line = new LevelLine { Special = 217, Arg0 = 7, Arg1 = 8, Arg2 = 2, Arg3 = -1, PlayerUse = true };
        var checksum = sim.Checksum; Assert.False(Activate(sim, line));
        Assert.Equal(checksum, sim.Checksum); Assert.Equal(217, line.Special);
    }

    private static LevelLine Delayed(int special, int reset = 0) => new()
    { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 2, Arg3 = 2, Arg4 = reset, PlayerUse = true };

    [Theory]
    [InlineData(217)]
    [InlineData(270)]
    [InlineData(273)]
    [InlineData(271)]
    [InlineData(272)]
    public void ResetCountdownStartsAtActivationAndRetainsFloorUntilReturn(int special)
    {
        var sim = Room(); var sync = special is 271 or 272; var sign = special is 270 or 272 ? -1 : 1;
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 8,
            Arg3 = sync ? 12 : 0, Arg4 = sync ? 0 : 12, PlayerUse = true };
        Assert.True(Activate(sim, line));
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.Equal(sign * 8, sim.FloorOf(0));
        Assert.False(LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 7));
        sim.Tick(); Assert.Equal(sign * 7, sim.FloorOf(0));
        for (var i = 0; i < 7; i++) sim.Tick();
        Assert.Equal(0, sim.FloorOf(0));
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(0, sim.FloorOf(1)); Assert.Equal(0, sim.FloorOf(2));
        Assert.True(Activate(sim, Trigger(false)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EarlyResetReversesUnfinishedSyncChain(bool down)
    {
        var sim = Room(); Assert.True(Activate(sim, Trigger(down, reset: 3)));
        sim.Tick(); sim.Tick(); Assert.Equal(down ? -2 : 2, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(down ? -1 : 1, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(1)); Assert.Equal(0, sim.FloorOf(2));
        Assert.True(Activate(sim, Trigger(down)));
    }

    [Fact]
    public void ResetCountdownParticipatesInChecksumBeforePositionsDiffer()
    {
        var a = Room(); var b = Room();
        Assert.True(Activate(a, Trigger(false, reset: 12))); Assert.True(Activate(b, Trigger(false, reset: 13)));
        a.Tick(); b.Tick(); Assert.Equal(a.FloorOf(0), b.FloorOf(0)); Assert.NotEqual(a.Checksum, b.Checksum);
    }

    [Fact]
    public void TagWaitRemainsBlockedWhileBuiltStepWaitsForReset()
    {
        var sim = Room(); Assert.True(Activate(sim, Trigger(false, reset: 12)));
        int[] words = [62, 7, 1]; var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 18; i++) sim.Tick();
        Assert.Equal(1, sim.Acs.RunningCount); Assert.Equal(1, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(0, sim.Acs.RunningCount); Assert.Equal(0, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void DistanceScaledSpeedFinishesChainTogether(bool down, bool uneven)
    {
        var offset = uneven ? 4 : 0; var sim = Room(down ? -offset : offset);
        var line = Trigger(down); Assert.True(Activate(sim, line)); Assert.Equal(0, line.Special);
        sim.Tick(); var sign = down ? -1 : 1;
        Assert.Equal(sign, sim.FloorOf(0));
        Assert.Equal(sign * (offset + (16 - offset) / 8.0), sim.FloorOf(1));
        Assert.Equal(sign * 3, sim.FloorOf(2));
        for (var i = 1; i < 8; i++) sim.Tick();
        Assert.Equal(sign * 8, sim.FloorOf(0)); Assert.Equal(sign * 16, sim.FloorOf(1)); Assert.Equal(sign * 24, sim.FloorOf(2));
        Assert.True(Activate(sim, Trigger(down)));
    }

    [Fact]
    public void RepeatLineRejectsBusyChainThenCanRestartAfterCompletion()
    {
        var sim = Room(); var line = Trigger(false, repeat: true);
        Assert.True(Activate(sim, line)); Assert.Equal(271, line.Special); Assert.False(Activate(sim, line));
        for (var i = 0; i < 8; i++) sim.Tick(); Assert.True(Activate(sim, line));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeResetRejectsWithoutConsumingLine(bool down)
    {
        var sim = Room(); var line = Trigger(down, reset: -1); var checksum = sim.Checksum;
        Assert.False(Activate(sim, line)); Assert.Equal(down ? 272 : 271, line.Special);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void SignedSyncSpeedPreservesOppositeSideTargetsAndReset(bool down, bool reset)
    {
        var sign = down ? -1 : 1;
        var sim = Room(sign * 32); var line = Trigger(down, reset: reset ? 3 : 0);
        Assert.True(Activate(sim, line)); Assert.Equal(0, line.Special);
        sim.Tick(); Assert.Equal(sign * 16, sim.FloorOf(1));
        Assert.Equal(sign, sim.FloorOf(0)); Assert.Equal(sign * 3, sim.FloorOf(2));
        sim.Tick(); Assert.Equal(sign * 16, sim.FloorOf(1));
        if (reset)
        {
            sim.Tick(); Assert.Equal(sign * 32, sim.FloorOf(1));
            sim.Tick(); Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(2));
        }
        else
        {
            for (var i = 0; i < 6; i++) sim.Tick();
            Assert.Equal(sign * 8, sim.FloorOf(0)); Assert.Equal(sign * 24, sim.FloorOf(2));
        }
        Assert.True(Activate(sim, Trigger(down)));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LargeNegativeSyncSpeedUsesAssignedDirectionBeforeReset(bool down)
    {
        var sign = down ? -1 : 1; var sim = Room(sign * 32);
        Assert.True(Activate(sim, new LevelLine
        { Special = down ? 272 : 271, Arg0 = 7, Arg1 = 1024, Arg2 = 8, Arg3 = 2, PlayerUse = true }));
        // Signed speed is 128 * (16 - 32) / 8 = -256 for this step.
        sim.Tick(); Assert.Equal(sign * -224, sim.FloorOf(1));
        sim.Tick(); Assert.Equal(sign * 32, sim.FloorOf(1));
        Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(2));
        Assert.True(Activate(sim, Trigger(down)));
    }

    [Fact]
    public void StepAlreadyAtTargetDoesNotPreventOtherStepsCompleting()
    {
        var sim = Room(16); Assert.True(Activate(sim, Trigger(false)));
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(16, sim.FloorOf(1)); Assert.Equal(24, sim.FloorOf(2));
        Assert.True(Activate(sim, Trigger(false)));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void OrdinaryStairKeepsAssignedDirectionAtOppositeSideTargetAndReset(bool down, bool reset)
    {
        var sign = down ? -1 : 1;
        var sim = Room(sign * 32);
        Assert.True(Activate(sim, new LevelLine
        { Special = down ? 270 : 217, Arg0 = 7, Arg1 = 8, Arg2 = 8, Arg4 = reset ? 3 : 0, PlayerUse = true }));
        sim.Tick();
        // The middle step is already past its target in the assigned build direction.
        Assert.Equal(sign * 16, sim.FloorOf(1)); Assert.Equal(sign, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(sign * 16, sim.FloorOf(1));
        if (reset)
        {
            sim.Tick();
            // Native reset flips the assigned direction even when returning to
            // the original height means clamping in the opposite direction.
            Assert.Equal(sign * 32, sim.FloorOf(1)); Assert.Equal(sign, sim.FloorOf(0));
            sim.Tick(); Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(2));
            Assert.True(Activate(sim, new LevelLine
            { Special = down ? 270 : 217, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }));
        }
        else
        {
            for (var i = 0; i < 22; i++) sim.Tick();
            Assert.Equal(sign * 8, sim.FloorOf(0)); Assert.Equal(sign * 24, sim.FloorOf(2));
        }
    }

    [Fact]
    public void StairRetainsRequestedTargetWhileConcurrentCeilingOpens()
    {
        var sim = Room(ceiling: 4);
        Assert.True(Activate(sim, new LevelLine
        { Special = 217, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }));
        Assert.True(Activate(sim, new LevelLine
        { Special = 41, Arg0 = 7, Arg1 = 16, Arg2 = 12, PlayerUse = true }));
        sim.Players.Single().Solid = false;
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(16, sim.CeilingOf(0)); Assert.Equal(8, sim.FloorOf(0));
    }

    private static LevelLine Trigger(bool down, bool repeat = false, int reset = 0) => new()
    { Special = down ? 272 : 271, Arg0 = 7, Arg1 = 8, Arg2 = 8, Arg3 = reset, PlayerUse = true, Repeat = repeat };
    private static bool Activate(AuthoritySimulation sim, LevelLine line) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true);
    private static AuthoritySimulation Room(int middleFloor = 0, int ceiling = 128) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = ceiling, FloorPic = "STEP" },
            new LevelSector { Index = 1, FloorHeight = middleFloor, CeilingHeight = 128, FloorPic = "STEP" },
            new LevelSector { Index = 2, CeilingHeight = 128, FloorPic = "STEP" }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }, new LevelSide { Sector = 2 }],
        Lines = [new LevelLine { Flags = LevelLine.TwoSidedFlag, SideFront = 0, SideBack = 1 },
            new LevelLine { Flags = LevelLine.TwoSidedFlag, SideFront = 1, SideBack = 2 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
