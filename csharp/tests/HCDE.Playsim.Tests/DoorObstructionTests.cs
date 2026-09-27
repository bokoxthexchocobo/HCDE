using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoorObstructionTests
{
    [Fact]
    public void TagWaitStaysBlockedThroughCloseWaitAndReopen()
    {
        var sim = Room(1); sim.Players.Single().Solid = false;
        Assert.True(Activate(sim, Reopening(1)));
        int[] words = [62, 7, 1]; var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 11; i++) { sim.Tick(); Assert.Equal(1, sim.Acs.RunningCount); }
        Assert.Equal(6, sim.CeilingOf(0)); sim.Tick();
        Assert.Equal(8, sim.CeilingOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 8)]
    public void CloseWaitOpenReturnsToOriginalHeightAfterOcticsDelay(int delay, int ticks)
    {
        var sim = Room(1); sim.Players.Single().Solid = false;
        var line = Reopening(delay); Assert.True(Activate(sim, line)); Assert.Equal(0, line.Special);
        for (var i = 0; i < 4; i++) sim.Tick(); Assert.Equal(0, sim.CeilingOf(0));
        for (var i = 0; i < ticks; i++) { sim.Tick(); Assert.Equal(0, sim.CeilingOf(0)); }
        sim.Tick(); Assert.Equal(2, sim.CeilingOf(0));
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(8, sim.CeilingOf(0)); Assert.True(Activate(sim, Reopening(delay)));
    }

    [Fact]
    public void CloseWaitOpenIntermediateObstructionReopensWithoutWaiting()
    {
        var sim = Room(3); Assert.True(Activate(sim, Reopening(8)));
        sim.Tick(); sim.Tick(); Assert.Equal(4, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(4, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(6, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0)); Assert.True(Activate(sim, Reopening(8)));
    }

    [Fact]
    public void CloseWaitOpenBlockedEndpointWaitsBeforeReturning()
    {
        var sim = Room(1); Assert.True(Activate(sim, Reopening(1)));
        for (var i = 0; i < 4; i++) sim.Tick(); Assert.Equal(2, sim.CeilingOf(0));
        for (var i = 0; i < 4; i++) sim.Tick(); Assert.Equal(2, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(4, sim.CeilingOf(0));
        sim.Tick(); sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
        Assert.True(Activate(sim, Reopening(1)));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(int.MaxValue, 0)]
    [InlineData(1, 7)]
    public void UnsupportedReopeningParametersDoNotConsumeLine(int delay, int lightTag)
    {
        var sim = Room(1); var line = Reopening(delay, lightTag); var checksum = sim.Checksum;
        Assert.False(Activate(sim, line)); Assert.Equal(249, line.Special); Assert.Equal(checksum, sim.Checksum);
    }

    private static LevelLine Reopening(int delay, int lightTag = 0) => new()
    { Special = 249, Arg0 = 7, Arg1 = 16, Arg2 = delay, Arg3 = lightTag, PlayerUse = true };

    [Fact]
    public void CloseOnlyWaitsAtIntermediateObstructionThenContinuesDown()
    {
        var sim = Room(3); var line = CloseLine();
        Assert.True(Activate(sim, line)); Assert.Equal(0, line.Special);
        sim.Tick(); sim.Tick(); Assert.Equal(4, sim.CeilingOf(0));
        sim.Tick(); sim.Tick(); Assert.Equal(4, sim.CeilingOf(0));
        Assert.False(Activate(sim, CloseLine()));
        sim.Players.Single().Solid = false;
        sim.Tick(); Assert.Equal(2, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(0, sim.CeilingOf(0)); Assert.True(Activate(sim, CloseLine()));
    }

    [Theory]
    [InlineData(64)]
    [InlineData(80)]
    public void CloseOnlyBlockedEndpointRollsBackAndReleasesCeiling(int speed)
    {
        var sim = Room(1); Assert.True(Activate(sim, CloseLine(speed)));
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0)); Assert.Equal(100, sim.Players.Single().Health);
        Assert.True(Activate(sim, CloseLine(speed)));
    }

    [Fact]
    public void CloseOnlyDoesNotRequireNeighborAndHonorsRepeatAndActivation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, CeilingHeight = 8 }],
            Things = [new LevelThing { Type = 1 }]
        });
        sim.Players.Single().Solid = false; var line = CloseLine(repeat: true);
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, false));
        Assert.True(Activate(sim, line)); Assert.Equal(10, line.Special);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(0, sim.CeilingOf(0)); Assert.True(Activate(sim, line));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(16, 7)]
    public void UnsupportedCloseParametersDoNotMutateMap(int speed, int lightTag)
    {
        var sim = Room(1); var checksum = sim.Checksum;
        var line = new LevelLine { Special = 10, Arg0 = 7, Arg1 = speed, Arg2 = lightTag, PlayerUse = true };
        Assert.False(Activate(sim, line)); Assert.Equal(10, line.Special); Assert.Equal(checksum, sim.Checksum);
    }

    private static LevelLine CloseLine(int speed = 16, bool repeat = false) => new()
    { Special = 10, Arg0 = 7, Arg1 = speed, PlayerUse = true, Repeat = repeat };
    private static bool Activate(AuthoritySimulation sim, LevelLine line) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true);

    [Theory]
    [InlineData(64, 8)]
    [InlineData(80, 6)]
    public void BlockedCloseEndpointReleasesDoorAfterRollback(int speed, int expected)
    {
        var sim = Room(1); Assert.True(Start(sim, speed));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(expected, sim.CeilingOf(0));
        Assert.Equal(100, sim.Players.Single().Health);
        Assert.True(Start(sim, speed));
    }

    [Fact]
    public void IntermediateCloseObstructionReopensAndKeepsOwnership()
    {
        var sim = Room(9); Assert.True(Start(sim, 64));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
        Assert.False(Start(sim, 64));
        sim.Tick(); Assert.Equal(16, sim.CeilingOf(0));
        Assert.Equal(100, sim.Players.Single().Health);
    }

    private static bool Start(AuthoritySimulation sim, int speed) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
        { Special = 12, Arg0 = 7, Arg1 = speed, Arg2 = 0, PlayerUse = true }, true);

    private static AuthoritySimulation Room(int height)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 8 }, new LevelSector { Index = 1, CeilingHeight = 20 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { X1=-128, Y1=-128, X2=128, Y2=-128, SideFront=0, SideBack=-1 },
                new LevelLine { X1=128, Y1=-128, X2=128, Y2=128, SideFront=0, SideBack=1 },
                new LevelLine { X1=128, Y1=128, X2=-128, Y2=128, SideFront=0, SideBack=-1 },
                new LevelLine { X1=-128, Y1=128, X2=-128, Y2=-128, SideFront=0, SideBack=-1 }],
            Things = [new LevelThing { Type = 1 }]
        });
        sim.Players.Single().Height = Fixed.FromInt(height); sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        return sim;
    }
}
