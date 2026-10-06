using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerBlockingLineTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void PlayerBlockingUsesMbf21GateInMovementAndProbe(bool enabled, bool blocked)
    {
        var sim = Room(enabled); var player = sim.Players.Single();
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(player, 2, xOffset: 40));
        Assert.Equal(-40, player.X.ToDouble());
        Assert.Equal(!blocked, ActorPhysics.TryMove(sim, player, 40, 0, out _));
    }

    [Fact]
    public void PlayerBlockingDoesNotBlockMonsters()
    {
        var sim = Room(true); sim.Players.Single().Y = Fixed.FromInt(100);
        var monster = sim.AddBot(-40, 0);
        Assert.Null(ActorJumpActions.CheckBlock(monster, 2, xOffset: 40));
        Assert.True(ActorPhysics.TryMove(sim, monster, 40, 0, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UdmfImportsPlayerBlockingFlag(bool enabled)
    {
        var text = "namespace = \"ZDoom\"; vertex { x = 0; y = -100; } vertex { x = 0; y = 100; } "
            + "sector { heightceiling = 128; } sector { heightceiling = 128; } "
            + "sidedef { sector = 0; } sidedef { sector = 1; } "
            + "linedef { v1 = 0; v2 = 1; sidefront = 0; sideback = 1; blockplayers = "
            + (enabled ? "true" : "false") + "; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(enabled, (level.Lines.Single().Flags & LevelLine.BlockPlayersFlag) != 0);
    }

    private static AuthoritySimulation Room(bool enabled)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1, Flags = LevelLine.BlockPlayersFlag }).ToArray(),
            Things = [new LevelThing { Type = 1, X = -40 }]
        }, compat: enabled ? CompatSurface.Mbf21 : CompatSurface.None);
    }
}
