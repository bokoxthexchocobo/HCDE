using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LandMonsterBlockingLineTests
{
    [Theory]
    [InlineData(false, false, false, false, false)]
    [InlineData(true, false, false, false, true)]
    [InlineData(true, true, false, false, false)]
    [InlineData(true, false, true, false, false)]
    [InlineData(true, false, false, true, false)]
    public void MonsterMovementAndProbeRespectLandBlocking(bool enabled, bool floating,
        bool noGravity, bool noBlockMonsters, bool blocked)
    {
        var sim = Room(enabled);
        sim.Players.Single().Y = Fixed.FromInt(100);
        var actor = sim.AddBot(-40, 0);
        actor.Floating = floating;
        actor.NoGravity = noGravity;
        actor.NoBlockMonsters = noBlockMonsters;
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, xOffset: 40));
        Assert.Equal(-40, actor.X.ToDouble());
        Assert.Equal(!blocked, ActorPhysics.TryMove(sim, actor, 40, 0, out _));
    }

    [Fact]
    public void PlayerWithExemptionClearedUsesNativeLandBlockingRule()
    {
        var sim = Room(true);
        var player = sim.Players.Single();
        player.NoBlockMonsters = false;
        Assert.Equal(2, ActorJumpActions.CheckBlock(player, 2, xOffset: 40));
        Assert.False(ActorPhysics.TryMove(sim, player, 40, 0, out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UdmfImportsSecondaryFlag(bool enabled)
    {
        var text = "namespace = \"ZDoom\"; vertex { x = 0; y = -100; } vertex { x = 0; y = 100; } "
            + "sector { heightceiling = 128; } sector { heightceiling = 128; } "
            + "sidedef { sector = 0; } sidedef { sector = 1; } "
            + "linedef { v1 = 0; v2 = 1; sidefront = 0; sideback = 1; blocklandmonsters = "
            + (enabled ? "true" : "false") + "; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var line = LevelBuilder.FromUdmf(map, "MAP01").Lines.Single();
        Assert.Equal(enabled ? LevelLine.BlockLandMonstersFlag2 : 0, line.Flags2);
        Assert.Equal(0, line.Flags & LevelLine.BlockingFlag);
    }

    private static AuthoritySimulation Room(bool enabled)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(new LevelLine
            {
                X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
                Flags2 = LevelLine.BlockLandMonstersFlag2
            }).ToArray(),
            Things = [new LevelThing { Type = 1, X = -40 }]
        }, compat: enabled ? CompatSurface.Mbf21 : CompatSurface.None);
    }
}
