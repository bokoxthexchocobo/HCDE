using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerUseRangeSaveTests
{
    [Theory]
    [InlineData(64, 64.000000001)]
    [InlineData(-64, -64.000000001)]
    [InlineData(1000000, 1000001)]
    public void ChecksumDistinguishesRangesThatLoseFixedPointPrecision(double first, double second)
    {
        Assert.Equal(Fixed.FromDouble(first), Fixed.FromDouble(second));
        var a = Room(); var b = Room();
        a.Players.Single().UseRange = first; b.Players.Single().UseRange = second;
        a.Tick(); b.Tick();
        Assert.NotEqual(a.Checksum, b.Checksum);
        var bytes = SimSavegame.Write(b);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        b.Players.Single().UseRange = first;
        b.RestoreState(state);
        Assert.Equal(second, b.Players.Single().UseRange);
        a.Players.Single().UseRange = second;
        a.Tick(); b.Tick();
        Assert.Equal(a.Checksum, b.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 1 }],
        Sectors = [new LevelSector { CeilingHeight = 128 }],
    });

    [Theory]
    [InlineData(-64)]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(128.123456789)]
    public void ChangedRangeRoundTripsExactly(double range)
    {
        var sim = Room(); var player = sim.Players.Single(); player.UseRange = range;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(83, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        player.UseRange = 99; sim.RestoreState(state);
        Assert.Equal(range, player.UseRange);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void LegacyAbsentRangePreservesCurrentValue()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Null(state.Actors.Single().UseRange);
        sim.Players.Single().UseRange = -100; sim.RestoreState(state);
        Assert.Equal(-100, sim.Players.Single().UseRange);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MalformedRangeTrailerIsRejected(int kind)
    {
        var sim = Room(); sim.Players.Single().UseRange = 20;
        var bytes = SimSavegame.Write(sim);
        var start = bytes.Length - BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        if (kind == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 83);
        if (kind == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 2);
        if (kind == 2) BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(start + 12), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Fact]
    public void InvalidMemoryRangeRejectsBeforeHealthMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); var state = sim.CaptureState();
        state.Actors.Single().UseRange = double.PositiveInfinity;
        player.Health = 77;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(77, player.Health);
    }
}
