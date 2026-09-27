using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorCrushArgumentTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void AbsoluteFloorUsesHexenDefaultForModesOtherThanOneAndTwo(int mode)
    {
        var sim = Room(CompatSurface.HexenCrushDefaults); Start(sim, true, 279, 8, mode);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(1, sim.FloorOf(0)); Assert.Equal(93, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData(false, 8, 1, 4, 93)]
    [InlineData(true, 8, 1, 4, 93)]
    [InlineData(true, 8, 2, 1, 93)]
    [InlineData(true, 1, 1, 4, 100)]
    [InlineData(true, 1, 2, 1, 100)]
    [InlineData(true, 0, 1, 1, 100)]
    [InlineData(true, -1, 1, 1, 100)]
    public void AbsoluteCrushingFloorPreservesNativeDamageOffsetAndZeroDamageCrushing(bool script, int damage, int mode, int expected, int health)
    {
        var sim = Room(); Start(sim, script, 279, damage, mode);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(expected, sim.FloorOf(0)); Assert.Equal(health, sim.Players.Single().Health);
        Assert.Equal(expected == 4, LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(7, 93)]
    public void BlockedInstantRaiseRollsBackButReleasesFloor(int damage, int health)
    {
        var sim = Room(); for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(0, sim.Players.Single().SectorIndex);
        Start(sim, true, 67, damage); sim.Tick();
        Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(health, sim.Players.Single().Health);
        Assert.True(LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(false, 23)]
    [InlineData(true, 23)]
    [InlineData(false, 25)]
    [InlineData(true, 25)]
    [InlineData(false, 24)]
    [InlineData(true, 24)]
    [InlineData(false, 259)]
    [InlineData(true, 259)]
    [InlineData(false, 35)]
    [InlineData(true, 35)]
    public void PositiveCrushDamagesButStopsAtIntermediateObstruction(bool script, int special)
    {
        var sim = Room(); Start(sim, script, special, 7);
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(1, sim.FloorOf(0)); Assert.Equal(93, sim.Players.Single().Health);
        sim.Players.Single().Solid = false;
        for (var i = 0; i < (special == 35 ? 7 : 3); i++) sim.Tick();
        Assert.Equal(special == 35 ? 8 : 4, sim.FloorOf(0));
        Assert.True(LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(23, 0)]
    [InlineData(23, -1)]
    [InlineData(25, 0)]
    [InlineData(25, -10)]
    [InlineData(24, 0)]
    [InlineData(24, -10)]
    [InlineData(259, 0)]
    [InlineData(259, -10)]
    [InlineData(35, 0)]
    [InlineData(35, -10)]
    public void NonpositiveCrushDoesNotDamage(int special, int damage)
    {
        var sim = Room(); Start(sim, false, special, damage);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.Equal(1, sim.FloorOf(0)); Assert.Equal(100, sim.Players.Single().Health);
        Assert.False(LineSpecials.Execute(sim, sim.Players.Single(), LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(257, 0)]
    [InlineData(257, -1)]
    [InlineData(257, 7)]
    [InlineData(260, 0)]
    [InlineData(260, -1)]
    [InlineData(260, 7)]
    public void FixedSpeedFloorsPreserveCrushAndEndpointRelease(int special, int damage)
    {
        var sim = Room(); for (var i = 0; i < 3; i++) sim.Tick();
        Start(sim, true, special, damage); sim.Tick();
        Assert.Equal(0, sim.FloorOf(0));
        Assert.Equal(damage > 0 ? 93 : 100, sim.Players.Single().Health);
        Assert.Equal(special == 260, LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        if (special == 257)
        {
            sim.Players.Single().Solid = false; sim.Tick(); sim.Tick();
            Assert.Equal(4, sim.FloorOf(0));
            Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        }
    }

    [Theory]
    [InlineData(28)]
    [InlineData(99)]
    public void FloorCrusherUsesRawDamageAndNativeModeDefaults(int special)
    {
        foreach (var script in new[] { false, true })
        foreach (var damage in new[] { -1, 0, 7 })
        foreach (var mode in new[] { 0, 1, 2, 3 })
        foreach (var compat in new[] { CompatSurface.None, CompatSurface.HexenCrushDefaults })
        {
            var sim = Room(compat); Start(sim, script, special, damage, mode);
            for (var i = 0; i < 4; i++) sim.Tick();
            var stops = damage < 0 || mode == 2 || mode != 1 && compat == CompatSurface.HexenCrushDefaults;
            Assert.Equal(stops ? 1 : 4, sim.FloorOf(0));
            Assert.Equal(damage > 0 ? 93 : 100, sim.Players.Single().Health);
            Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        }
    }

    private static void Start(AuthoritySimulation sim, bool script, int special, int damage, int mode = 1)
    {
        int[] args = [7, 8, special == 23 ? 4 : special is 35 or 67 ? 1 : 0,
            special is 24 or 25 ? damage : 0, special is 23 or 35 or 67 ? damage : 0];
        if (special is 28 or 99) args = [7, 8, damage, mode, 0];
        if (special == 257) args = [7, 8, 0, damage, 0];
        if (special == 260) args = [7, 0, damage, 124, 0];
        if (special == 259) args = [7, 8, 0, damage, 124];
        if (special == 279) args = [7, 8, 4, damage, mode];
        if (!script)
        {
            Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
            { Special = special, Arg0 = args[0], Arg1 = args[1], Arg2 = args[2], Arg3 = args[3], Arg4 = args[4], PlayerUse = true }, true));
            return;
        }
        int[] words = [13, special, .. args, 1]; var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room(CompatSurface compat = CompatSurface.None)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }, new LevelSector { Index = 1, FloorHeight = 4, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { X1=-128, Y1=-128, X2=128, Y2=-128, SideBack=-1 },
                new LevelLine { X1=128, Y1=-128, X2=128, Y2=128, SideBack=1 },
                new LevelLine { X1=128, Y1=128, X2=-128, Y2=128, SideBack=-1 },
                new LevelLine { X1=-128, Y1=128, X2=-128, Y2=-128, SideBack=-1 }],
            Things = [new LevelThing { Type = 1 }]
        }, compat: compat);
        sim.Players.Single().Height = Fixed.FromInt(127);
        return sim;
    }
}
