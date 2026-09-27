using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GenericStairTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 0)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    public void DirectionAndTextureOptionsWorkAcrossCallPaths(int options, int route)
    {
        var sim = Room(); var line = Trigger(options); var words = new List<int>();
        int[] args = [7, 8, 4, options, 0];
        if (route == 2) Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        else if (route == 1)
        {
            foreach (var arg in args) words.AddRange([3, arg]);
            words.AddRange([8, 204]);
        }
        else { words.AddRange([13, 204]); words.AddRange(args); }
        words.Add(1); var bytes = new byte[words.Count * 4];
        for (var i = 0; i < words.Count; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        for (var i = 0; i < 8; i++) sim.Tick();
        var sign = (options & 1) != 0 ? 1 : -1;
        Assert.Equal(sign * 4, sim.FloorOf(0)); Assert.Equal((options & 2) != 0 ? sign * 8 : 0, sim.FloorOf(1));
    }

    [Fact]
    public void RepeatDirectionFlipsOnlyOnSuccessAndKeepsTextureOption()
    {
        var sim = Room(); var line = Trigger(3, repeat: true);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(2, line.Arg3); Assert.Equal(204, line.Special);
        Assert.False(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(2, line.Arg3);
        for (var i = 0; i < 8; i++) sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true)); Assert.Equal(3, line.Arg3);
        for (var i = 0; i < 12; i++) sim.Tick();
        Assert.Equal(0, sim.FloorOf(0)); Assert.Equal(-4, sim.FloorOf(1));
    }

    [Fact]
    public void NextDirectionParticipatesInChecksum()
    {
        var a = Room(); var b = Room();
        a.Level.Lines[0].Arg3 = 2; b.Level.Lines[0].Arg3 = 3;
        a.Tick(); b.Tick(); Assert.NotEqual(a.Checksum, b.Checksum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SimulationsOwnTheirConsumedSpecialsAndRepeatDirection(bool repeat)
    {
        var template = new PlayLevel
        {
            MapName = "MAP01", Format = MapDataFormat.HexenBinary, Namespace = "zdoom",
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
            Lines = [Trigger(3, repeat)], Things = [new LevelThing { Type = 1 }],
        };
        var a = AuthoritySimulation.Start(template); var b = AuthoritySimulation.Start(template);
        Assert.True(LineSpecials.ActivateMapLine(a, a.Players.Single(), a.Level.Lines[0], true));
        Assert.Equal(repeat ? 204 : 0, a.Level.Lines[0].Special);
        Assert.Equal(repeat ? 2 : 3, a.Level.Lines[0].Arg3);
        Assert.Equal(204, template.Lines[0].Special); Assert.Equal(3, template.Lines[0].Arg3);
        Assert.Equal(204, b.Level.Lines[0].Special); Assert.Equal(3, b.Level.Lines[0].Arg3);
        Assert.Same(a.Level, a.Players.Single().Level);
        Assert.Same(b.Level, b.Players.Single().Level);
        Assert.Equal(template.MapName, a.Level.MapName); Assert.Equal(template.Namespace, a.Level.Namespace);
        Assert.True(LineSpecials.ActivateMapLine(b, b.Players.Single(), b.Level.Lines[0], true));
        for (var i = 0; i < 4; i++) { a.Tick(); b.Tick(); }
        Assert.Equal(4, a.FloorOf(0)); Assert.Equal(a.Checksum, b.Checksum);
        var fresh = AuthoritySimulation.Start(template);
        Assert.Equal(204, fresh.Level.Lines[0].Special); Assert.Equal(3, fresh.Level.Lines[0].Arg3);
    }

    private static LevelLine Trigger(int options, bool repeat = false) => new()
    { Special = 204, Arg0 = 7, Arg1 = 8, Arg2 = 4, Arg3 = options, Repeat = repeat, PlayerUse = true };
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, FloorPic = "A" },
            new LevelSector { Index = 1, CeilingHeight = 128, FloorPic = "B" }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1, Flags = 4 }],
    });
}
