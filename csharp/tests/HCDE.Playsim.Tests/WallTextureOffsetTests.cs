using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WallTextureOffsetTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector()],
        Sides = [new LevelSide { TopTextureOffsetX = 10, TopTextureOffsetY = 20,
            MidTextureOffsetX = 30, MidTextureOffsetY = 40, BottomTextureOffsetX = 50, BottomTextureOffsetY = 60 },
            new LevelSide { Index = 1 }],
        Lines = [new LevelLine { Tag = 7, SideFront = 0, SideBack = 1 }],
    });

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(7, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(7, true)]
    public void MapActionHonorsPartMaskAndAddMode(int parts, bool add)
    {
        var sim = Room();
        var line = new LevelLine { Special = 53, Arg0 = 7, Arg1 = -98304, Arg2 = 32768,
            Arg4 = parts | (add ? 8 : 0), PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        var side = sim.Level.Sides[0];
        Assert.Equal((parts & 1) != 0 ? (add ? 10 : 0) - 1.5 : 10, side.TopTextureOffsetX);
        Assert.Equal((parts & 2) != 0 ? (add ? 30 : 0) - 1.5 : 30, side.MidTextureOffsetX);
        Assert.Equal((parts & 4) != 0 ? (add ? 50 : 0) - 1.5 : 50, side.BottomTextureOffsetX);
        Assert.Equal((parts & 1) != 0 ? (add ? 20 : 0) + 0.5 : 20, side.TopTextureOffsetY);
        Assert.Equal((parts & 2) != 0 ? (add ? 40 : 0) + 0.5 : 40, side.MidTextureOffsetY);
        Assert.Equal((parts & 4) != 0 ? (add ? 60 : 0) + 0.5 : 60, side.BottomTextureOffsetY);
        Assert.Equal(0, sim.Level.Sides[1].TopTextureOffsetX);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsPathsHonorBackSideAndNoChangeSentinel(bool stack)
    {
        var sim = Room();
        sim.Level.Sides[1].MidTextureOffsetY = 15;
        int[] args = [7, 131072, 32767 << 16, 1, 2];
        var words = new List<int>();
        if (stack)
        {
            foreach (var arg in args) words.AddRange([3, arg]);
            words.AddRange([8, 53, 1]);
        }
        else words.AddRange([13, 53, ..args, 1]);
        var code = new byte[words.Count * 4];
        for (var i = 0; i < words.Count; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        Assert.Equal(2, sim.Level.Sides[1].MidTextureOffsetX);
        Assert.Equal(15, sim.Level.Sides[1].MidTextureOffsetY);
        Assert.Equal(30, sim.Level.Sides[0].MidTextureOffsetX);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(7, -1, false)]
    [InlineData(7, 2, false)]
    [InlineData(999, 0, true)]
    [InlineData(-1, 0, true)]
    public void ValidationMatchesNativeReturnSemantics(int id, int side, bool expected)
    {
        var sim = Room();
        var line = new LevelLine { Special = 53, Arg0 = id, Arg3 = side, Arg4 = 7, PlayerUse = true };
        Assert.Equal(expected, LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(10, sim.Level.Sides[0].TopTextureOffsetX);
    }
}
