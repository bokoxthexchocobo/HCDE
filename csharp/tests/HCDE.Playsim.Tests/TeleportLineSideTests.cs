using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportLineSideTests
{
    [Theory]
    [InlineData(MapDataFormat.DoomBinary, 39)]
    [InlineData(MapDataFormat.DoomBinary, 97)]
    [InlineData(MapDataFormat.HexenBinary, 70)]
    [InlineData(MapDataFormat.UdmfText, 70)]
    public void BackSideRejectsWithoutConsumingLineAndFrontSideTeleports(MapDataFormat format, int special)
    {
        var sim = Room(format);
        var player = Assert.Single(sim.Players);
        var line = new LevelLine { Special = special, Tag = 7, Arg1 = 7, PlayerCross = true };
        player.ReactionTime = 7;
        player.VelocityX = Fixed.FromInt(3);
        var x = player.X;
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, false, backSide: true));
        Assert.Equal(x, player.X);
        Assert.Equal(3, player.VelocityX.ToDouble());
        Assert.Equal(7, player.ReactionTime);
        Assert.Equal(special, line.Special);
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, false, backSide: false));
        Assert.Equal(200, player.X.ToDouble());
        Assert.Equal(18, player.ReactionTime);
        Assert.Equal(special == 97 ? 97 : 0, line.Special);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    public void MissingSideArgumentUsesActorPosition(int x, bool expected)
    {
        var sim = Room(MapDataFormat.HexenBinary);
        var player = Assert.Single(sim.Players);
        player.X = Fixed.FromInt(x);
        var line = new LevelLine { X1 = 0, Y1 = -64, X2 = 0, Y2 = 64, Special = 70, Arg1 = 7, PlayerCross = true };
        Assert.Equal(expected, LineSpecials.ActivateMapLine(sim, player, line, false));
    }

    private static AuthoritySimulation Room(MapDataFormat format) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = format, Namespace = "ZDoom",
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}