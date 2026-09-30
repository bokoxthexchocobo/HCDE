using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorScrollCarryTests
{
    [Fact]
    public void GroundedPlayerMovesWithSectorScroll()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var startX = player.X.ToDouble();
        sim.SetSectorScroll(0, 3, -2);
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        Assert.Equal(64 - 2, player.Y.ToDouble());
    }

    [Fact]
    public void ScrollFloorLineSpecialSetsCarryOnTaggedSectors()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 96,
            Arg2 = -64,
            Arg3 = 1,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        Assert.Equal(64 - 2, player.Y.ToDouble());
    }

    [Fact]
    public void ScrollFloorCarryModeTwoSetsCarryLikeNative()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 64,
            Arg2 = 0,
            Arg3 = 2,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 2, player.X.ToDouble());
    }

    [Fact]
    public void HexenScrollAddsToLineDrivenCarryForPlayers()
    {
        var sim = Room(tag: 7, hexenSpecial: HexenSectorScroll.NorthSlow);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 32,
            Arg2 = 0,
            Arg3 = 1,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        var startY = player.Y.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 1, player.X.ToDouble());
        Assert.Equal(startY + 0.5, player.Y.ToDouble());
    }

    [Fact]
    public void ScrollFloorCarryModeZeroClearsScroll()
    {
        var sim = Room(tag: 7);
        sim.SetSectorScroll(0, 5, 0);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 0,
            Arg2 = 0,
            Arg3 = 0,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX, player.X.ToDouble());
    }

    [Fact]
    public void HexenNorthSlowScrollMovesPlayersOnly()
    {
        var sim = Room(hexenSpecial: HexenSectorScroll.NorthSlow);
        var player = sim.Players.Single();
        var bot = sim.AddBot(96, 64);
        var startY = player.Y.ToDouble();
        var botStartY = bot.Y.ToDouble();
        sim.Tick();
        Assert.Equal(startY + 0.5, player.Y.ToDouble());
        Assert.Equal(botStartY, bot.Y.ToDouble());
    }

    [Fact]
    public void ScrollCeilingLineActivatesWithoutCarry()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollCeiling,
            Arg0 = 7,
            Arg1 = 96,
            Arg2 = -64,
            Arg3 = 0,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX, player.X.ToDouble());
        Assert.Equal(64, player.Y.ToDouble());
    }

    [Fact]
    public void GroundedMonsterMovesWithSectorScroll()
    {
        var sim = Room();
        var bot = sim.AddBot(96, 64);
        var startX = bot.X.ToDouble();
        sim.SetSectorScroll(0, 2, 1);
        sim.Tick();
        Assert.Equal(startX + 2, bot.X.ToDouble());
        Assert.Equal(64 + 1, bot.Y.ToDouble());
    }

    [Fact]
    public void ScrollFloorLineSpecialUpdatesCarryRate()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var fast = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 96,
            Arg2 = 0,
            Arg3 = 1,
            PlayerUse = true,
        };
        var slow = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 32,
            Arg2 = 0,
            Arg3 = 1,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, fast, use: true));
        sim.Tick();
        var afterFast = player.X.ToDouble();
        Assert.True(LineSpecials.ActivateMapLine(sim, player, slow, use: true));
        sim.Tick();
        Assert.Equal(afterFast + 1, player.X.ToDouble());
    }

    [Fact]
    public void SectorScrollCarryPersistsUntilCleared()
    {
        var sim = Room();
        var player = sim.Players.Single();
        sim.SetSectorScroll(0, 2, 0);
        var startX = player.X.ToDouble();
        sim.Tick();
        sim.Tick();
        Assert.Equal(startX + 4, player.X.ToDouble());
    }

    [Fact]
    public void MultipleCarryScrollsOnSameSectorAccumulateEachTic()
    {
        var sim = Room();
        var player = sim.Players.Single();
        sim.AppendSectorCarryScroll(0, 2, 0);
        sim.AppendSectorCarryScroll(0, 1, 1);
        var startX = player.X.ToDouble();
        var startY = player.Y.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        Assert.Equal(startY + 1, player.Y.ToDouble());
    }

    [Fact]
    public void FloatingActorIgnoresSectorScroll()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Floating = true;
        var startX = player.X.ToDouble();
        sim.SetSectorScroll(0, 5, 0);
        sim.Tick();
        Assert.Equal(startX, player.X.ToDouble());
    }

    private static AuthoritySimulation Room(int tag = 0, short hexenSpecial = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = (short)tag, Special = hexenSpecial, FloorHeight = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
    });
}
