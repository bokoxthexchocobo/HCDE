using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WallTextureScaleTests
{
    [Fact]
    public void RuntimeMultiplyUsesLoadedWallScales()
    {
        const string text = """
            namespace = "ZDoom";
            vertex { x = 0; y = 0; }
            vertex { x = 64; y = 0; }
            sector { heightceiling = 128; }
            sidedef { sector = 0; scalex_mid = 3; scaley_mid = -2; scalex_top = 4; }
            linedef { id = 7; v1 = 0; v2 = 1; sidefront = 0; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        var line = new LevelLine { Special = 56, Arg0 = 7, Arg1 = 32768, Arg2 = 131072, Arg4 = 10, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        var side = Assert.Single(sim.Level.Sides);
        Assert.Equal(1.5, side.MidTextureScaleX);
        Assert.Equal(-4, side.MidTextureScaleY);
        Assert.Equal(4, side.TopTextureScaleX);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector()],
        Sides = [new LevelSide { TopTextureScaleX = 2, TopTextureScaleY = 3,
            MidTextureScaleX = 4, MidTextureScaleY = 5, BottomTextureScaleX = 6, BottomTextureScaleY = 7 },
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
    public void MapActionSetsOrMultipliesOnlySelectedParts(int mask, bool multiply)
    {
        var sim = Room();
        var line = new LevelLine { Special = 56, Arg0 = 7, Arg1 = -32768, Arg2 = 0,
            Arg4 = mask | (multiply ? 8 : 0), PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        var side = sim.Level.Sides[0];
        Assert.Equal((mask & 1) != 0 ? (multiply ? 2 : 1) * -0.5 : 2, side.TopTextureScaleX);
        Assert.Equal((mask & 2) != 0 ? (multiply ? 4 : 1) * -0.5 : 4, side.MidTextureScaleX);
        Assert.Equal((mask & 4) != 0 ? (multiply ? 6 : 1) * -0.5 : 6, side.BottomTextureScaleX);
        Assert.Equal((mask & 1) != 0 ? (multiply ? 0 : 1) : 3, side.TopTextureScaleY);
        Assert.Equal((mask & 2) != 0 ? (multiply ? 0 : 1) : 5, side.MidTextureScaleY);
        Assert.Equal((mask & 4) != 0 ? (multiply ? 0 : 1) : 7, side.BottomTextureScaleY);
        Assert.Equal(1, sim.Level.Sides[1].TopTextureScaleX);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsPathsPreserveSentinelAxisOnBackSide(bool stack)
    {
        var sim = Room();
        sim.Level.Sides[1].MidTextureScaleY = 3;
        int[] args = [7, 32767 << 16, 131072, 1, 10];
        var words = new List<int>();
        if (stack)
        {
            foreach (var arg in args) words.AddRange([3, arg]);
            words.AddRange([8, 56, 1]);
        }
        else words.AddRange([13, 56, ..args, 1]);
        var code = new byte[words.Count * 4];
        for (var i = 0; i < words.Count; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        Assert.Equal(1, sim.Level.Sides[1].MidTextureScaleX);
        Assert.Equal(6, sim.Level.Sides[1].MidTextureScaleY);
        Assert.Equal(5, sim.Level.Sides[0].MidTextureScaleY);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(7, -1, false)]
    [InlineData(7, 2, false)]
    [InlineData(999, 0, true)]
    [InlineData(-1, 0, true)]
    public void ValidationMatchesNative(int id, int side, bool expected)
    {
        var sim = Room();
        var line = new LevelLine { Special = 56, Arg0 = id, Arg3 = side, Arg4 = 7, PlayerUse = true };
        Assert.Equal(expected, LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(2, sim.Level.Sides[0].TopTextureScaleX);
    }
}
