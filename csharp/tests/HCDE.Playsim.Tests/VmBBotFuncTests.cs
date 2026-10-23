using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmBBotFuncTests
{
    [Fact]
    public void CheckLos_RequiresSightAndFov()
    {
        var (sim, viewer, target) = OpenRoom();
        Assert.True(VmBBotFunc.CheckLos(sim, viewer, target, 90));
        Assert.False(VmBBotFunc.CheckLos(sim, viewer, target, 0));
        Assert.True(VmBBotFunc.CheckLos(sim, viewer, target, 360));
    }

    [Fact]
    public void CheckLos_BlockedLineFails()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines =
            [
                new LevelLine
                {
                    X1 = 0, Y1 = -128, X2 = 0, Y2 = 128,
                    SideFront = 0, SideBack = 1,
                    Flags = LevelLine.TwoSidedFlag | LevelLine.BlockEverythingFlag,
                },
            ],
            Things = [new LevelThing { Type = 1, X = -64 }],
        });
        var viewer = sim.Players.First();
        var target = sim.AddBot(64, 0);
        Assert.False(CombatTrace.CheckSight(sim, viewer, target, ActorSightFlags.SeePastBlockEverything));
        Assert.False(VmBBotFunc.CheckLos(sim, viewer, target, 360));
    }

    [Fact]
    public void IsLeader_WhenAnotherBotTargetsPlayer()
    {
        var leader = new Actor();
        var slots = new[]
        {
            new VmBBot.PlayerSlot { Bot = new VmBBot.BotState { Mate = leader } },
            new VmBBot.PlayerSlot(),
        };
        Assert.True(VmBBotFunc.IsLeader(slots, leader));
        Assert.False(VmBBotFunc.IsLeader(slots, new Actor()));
    }

    [Fact]
    public void ChooseMate_PicksVisibleCoopPartner()
    {
        var (sim, botMo, slots, bot) = TwoPlayerCoop();
        var mate = VmBBotFunc.ChooseMate(sim, bot, botMo, slots, deathmatch: false, teamplay: false);
        Assert.NotNull(mate);
        Assert.Equal(sim.Players.Last(), mate);
    }

    [Fact]
    public void FindEnemy_DeathmatchPicksClosestVisibleOpponent()
    {
        var (sim, botMo, target) = OpenRoom();
        target.Friendly = false;
        var bot = new VmBBot.BotState { Allround = true };
        var slots = new[]
        {
            new VmBBot.PlayerSlot { InGame = true, Mo = botMo },
            new VmBBot.PlayerSlot { InGame = true, Mo = target },
        };
        Assert.True(VmBBotFunc.CheckLos(sim, botMo, target, 360));
        Assert.Same(target, VmBBotFunc.FindEnemy(sim, bot, botMo, slots, deathmatch: true, teamplay: false));
    }

    [Fact]
    public void FindEnemy_CoopUsesRoughMonsterSearch()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 5, 5), []),
            Things = [],
        });
        var botMo = sim.AddBot(192, 192);
        var target = sim.AddBot(210, 210);
        var bot = new VmBBot.BotState();
        Assert.Same(target, VmBBotFunc.FindEnemy(sim, bot, botMo, [], deathmatch: false, teamplay: false));
    }

    static (AuthoritySimulation, Actor, Actor) OpenRoom()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, LightLevel = 160 }],
            Sides = [new LevelSide { Sector = 0 }],
            Things = [new LevelThing { Type = 1, X = -64 }],
        });
        var viewer = sim.Players.First();
        var target = sim.AddBot(64, 0);
        return (sim, viewer, target);
    }

    static (AuthoritySimulation sim, Actor botMo, VmBBot.PlayerSlot[] slots, VmBBot.BotState bot) TwoPlayerCoop()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, LightLevel = 160 }],
            Sides = [new LevelSide { Sector = 0 }],
            Things =
            [
                new LevelThing { Type = 1, X = -64, Y = 0 },
                new LevelThing { Type = 1, X = 64, Y = 0 },
            ],
        });
        var botMo = sim.AddBot(-64, 0);
        var bot = new VmBBot.BotState();
        var slots = new[]
        {
            new VmBBot.PlayerSlot { InGame = true, Mo = botMo },
            new VmBBot.PlayerSlot { InGame = true, Mo = sim.Players.Last() },
        };
        return (sim, botMo, slots, bot);
    }

}
