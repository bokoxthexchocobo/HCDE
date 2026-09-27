using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSectorPlaneTests
{
    [Theory]
    [InlineData(261, -81920)]
    [InlineData(262, 8437760)]
    public void TaggedQueryUsesFirstMatchAndFractionalHeight(int opcode, int expected)
    {
        var sim = Room();
        AssertQuery(sim, opcode, 7, 999, -999, expected);
    }

    [Theory]
    [InlineData(261, 655360)]
    [InlineData(262, 13156352)]
    public void ZeroTagUsesPointInIntegerMapCoordinates(int opcode, int expected)
    {
        var sim = Room();
        AssertQuery(sim, opcode, 0, 110, 25, expected);
    }

    [Theory]
    [InlineData(261, 99)]
    [InlineData(262, 99)]
    [InlineData(261, 0)]
    [InlineData(262, 0)]
    public void MissingTagOrUnresolvedPointReturnsZero(int opcode, int tag)
    {
        AssertQuery(Room(), opcode, tag, -999, -999, 0);
    }

    [Theory]
    [InlineData(261, 0)]
    [InlineData(261, 1)]
    [InlineData(261, 2)]
    [InlineData(262, 0)]
    [InlineData(262, 1)]
    [InlineData(262, 2)]
    public void IncompleteArgumentsStopFollowingAction(int opcode, int count)
    {
        var words = new List<int>();
        for (var i = 0; i < count; i++) words.AddRange([3, 7]);
        words.AddRange([opcode, 10, 112, 7, 35, 1]);
        var sim = Room(); Run(sim, words.ToArray());
        Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(261, 23, -16384)]
    [InlineData(262, 40, 8372224)]
    public void QueryReadsMovingPlaneInsteadOfMapTemplate(int opcode, int special, int expected)
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
            { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 2, PlayerUse = true }, true));
        sim.Tick();
        AssertQuery(sim, opcode, 7, 0, 0, expected);
        Assert.Equal(-1.25, sim.Level.Sectors[0].FloorHeight);
        Assert.Equal(128.75, sim.Level.Sectors[0].CeilingHeight);
    }

    private static void AssertQuery(AuthoritySimulation sim, int opcode, int tag, int x, int y, int expected)
    {
        // The light tag below the query arguments must survive its three-pop/one-push operation.
        Run(sim, [3, 7, 3, tag, 3, x, 3, y, opcode, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, [], sim.Players.Single()));
        sim.Acs.Tick(sim);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [
            new LevelSector { Tag = 7, FloorHeight = -1.25, CeilingHeight = 128.75, LightLevel = 128 },
            new LevelSector { Index = 1, Tag = 7, FloorHeight = 10, CeilingHeight = 200.75, LightLevel = 128 },
        ],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }],
        Lines = [
            new LevelLine { X1 = -32, Y1 = -32, X2 = 32, Y2 = -32, SideFront = 0, SideBack = -1 },
            new LevelLine { X1 = 32, Y1 = -32, X2 = 32, Y2 = 32, SideFront = 0, SideBack = -1 },
            new LevelLine { X1 = 32, Y1 = 32, X2 = -32, Y2 = 32, SideFront = 0, SideBack = -1 },
            new LevelLine { X1 = -32, Y1 = 32, X2 = -32, Y2 = -32, SideFront = 0, SideBack = -1 },
            new LevelLine { X1 = 100, Y1 = 20, X2 = 120, Y2 = 20, SideFront = 1, SideBack = -1 },
            new LevelLine { X1 = 120, Y1 = 20, X2 = 120, Y2 = 30, SideFront = 1, SideBack = -1 },
            new LevelLine { X1 = 120, Y1 = 30, X2 = 100, Y2 = 30, SideFront = 1, SideBack = -1 },
            new LevelLine { X1 = 100, Y1 = 30, X2 = 100, Y2 = 20, SideFront = 1, SideBack = -1 },
        ],
        Things = [new LevelThing { Type = 1 }],
    });
}
