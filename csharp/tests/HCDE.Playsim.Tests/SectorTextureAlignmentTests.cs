using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorTextureAlignmentTests
{
    private static AuthoritySimulation Room(bool back = true) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { CeilingHeight = 128, FloorTextureAngle = 0x40000000u },
            new LevelSector { Index = 1, CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }],
        Lines = [new LevelLine { Tag = 7, X1 = 10, Y1 = 20, X2 = 10, Y2 = 84,
            SideFront = 0, SideBack = back ? 1 : -1 }],
    });

    [Theory]
    [InlineData(184, 0, 0xc0000000u, -10)]
    [InlineData(184, 1, 0x40000000u, 10)]
    [InlineData(183, 0, 0xc0000000u, -10)]
    [InlineData(183, -2, 0x40000000u, 10)]
    public void MapActionSetsSelectedPlaneBaseAndPreservesMutableRotation(int special, int side, uint angle, double offset)
    {
        var sim = Room();
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = side, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        var sector = sim.Level.Sectors[side == 0 ? 0 : 1];
        Assert.Equal(angle, special == 184 ? sector.FloorTextureBaseAngle : sector.CeilingTextureBaseAngle);
        Assert.Equal(offset, special == 184 ? sector.FloorTextureBaseOffsetY : sector.CeilingTextureBaseOffsetY, 6);
        Assert.Equal(0u, special == 184 ? sector.CeilingTextureBaseAngle : sector.FloorTextureBaseAngle);
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureAngle);
        Assert.Equal(0, line.Special);
    }

    [Theory]
    [InlineData(7, 1)]
    [InlineData(999, 0)]
    [InlineData(-1, 0)]
    public void MissingSideOrIdFailsAndPreservesAction(int id, int side)
    {
        var sim = Room(back: false);
        var line = new LevelLine { Special = 184, Arg0 = id, Arg1 = side, PlayerUse = true };
        Assert.False(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(184, line.Special);
    }

    [Theory]
    [InlineData(false, 183)]
    [InlineData(true, 183)]
    [InlineData(false, 184)]
    [InlineData(true, 184)]
    public void AcsDirectAndStackActionsAlignPlane(bool stackMode, int special)
    {
        var sim = Room();
        int[] words = stackMode ? [3, 7, 3, 0, 5, special, 1] : [10, special, 7, 0, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        var sector = sim.Level.Sectors[0];
        Assert.Equal(0xc0000000u, special == 184 ? sector.FloorTextureBaseAngle : sector.CeilingTextureBaseAngle);
    }

    [Fact]
    public void AlignmentBaseCombinesWithMapRotationDuringScrolling()
    {
        var sim = Room();
        var line = new LevelLine { Special = 184, Arg0 = 7, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        sim.SetSectorTextureScroll(0, 1, 0, SectorTextureScrollPlane.Floor);
        sim.Tick();
        Assert.Equal(1, sim.Level.Sectors[0].FloorTextureOffsetX, 6);
        Assert.Equal(0, sim.Level.Sectors[0].FloorTextureOffsetY, 6);
    }
}
