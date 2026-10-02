using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RuntimeWallActionTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide(), new LevelSide { Index = 1 }],
        Lines = [new LevelLine { Tag = 7, SideFront = 0, SideBack = 1 }],
    });

    private static void Invoke(AuthoritySimulation sim, int special, int[] args, int route)
    {
        if (route == 0)
        {
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine { Special = special,
                Arg0 = args[0], Arg1 = args[1], Arg2 = args[2], Arg3 = args[3], Arg4 = args[4], PlayerUse = true }, true));
            return;
        }
        var words = new List<int>();
        if (route == 2)
        {
            foreach (var arg in args) words.AddRange([3, arg]);
            words.AddRange([8, special, 1]);
        }
        else words.AddRange([13, special, ..args, 1]);
        var code = new byte[words.Count * 4];
        for (var i = 0; i < words.Count; i++) BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
    }

    [Theory]
    [InlineData(52, 0, false)]
    [InlineData(52, 1, false)]
    [InlineData(52, 2, false)]
    [InlineData(52, 0, true)]
    [InlineData(52, 1, true)]
    [InlineData(52, 2, true)]
    [InlineData(221, 0, false)]
    [InlineData(221, 1, false)]
    [InlineData(221, 2, false)]
    [InlineData(221, 0, true)]
    [InlineData(221, 1, true)]
    [InlineData(221, 2, true)]
    public void MapAndBothAcsRoutesDecodeNativeRatesAndSide(int special, int route, bool back)
    {
        var sim = Room();
        int[] args = special == 221 ? [back ? -7 : 7, 32, 128, 96, 64]
            : [7, -98304, -32768, back ? -8 : 0, 7];
        Invoke(sim, special, args, route); sim.Tick();
        var selected = sim.Level.Sides[back ? 1 : 0];
        Assert.Equal(-1.5, selected.TopTextureOffsetX);
        Assert.Equal(-1.5, selected.MidTextureOffsetX);
        Assert.Equal(-0.5, selected.BottomTextureOffsetY);
        Assert.Equal(0, sim.Level.Sides[back ? 0 : 1].TopTextureOffsetX);
    }

    [Theory]
    [InlineData(52, 0, false)]
    [InlineData(221, 0, false)]
    [InlineData(52, 999, true)]
    [InlineData(221, -999, true)]
    [InlineData(52, -1, true)]
    [InlineData(221, int.MinValue, true)]
    public void InvalidOrMissingIdsReturnNativeOutcome(int special, int id, bool expected)
    {
        var sim = Room();
        Assert.Equal(expected, WallScrollActions.Execute(sim, special, id, 64, 0, 0, 7));
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void OpposingRatesCancelAndRemoveWallThinkers()
    {
        var sim = Room();
        Invoke(sim, 221, [7, 64, 0, 0, 0], 0); sim.Tick();
        Invoke(sim, 221, [7, 64, 64, 32, 32], 0); sim.Tick();
        Assert.Equal(1, sim.Level.Sides[0].TopTextureOffsetX);
        Assert.Empty(sim.CaptureState().TextureScrolls!);
    }

    [Fact]
    public void RuntimeBothRetainsControlledWallHistoryAndVelocity()
    {
        var sim = Room();
        sim.AppendControlWallScroll(0, 0, 1, 0, true);
        sim.Floors[0] = 2; sim.Tick();
        Invoke(sim, 221, [7, 128, 0, 0, 0], 0);
        sim.Floors[0] = 3; sim.Tick();
        Assert.Equal(6, sim.Level.Sides[0].TopTextureOffsetX);
        var scroll = Assert.Single(sim.CaptureState().TextureScrolls!);
        Assert.Equal(18, scroll.Kind); Assert.Equal(4, scroll.Vdx); Assert.Equal(2, scroll.Dx);
    }
}
