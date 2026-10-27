using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmBBotFuncReachFireTests
{
    [Fact]
    public void Reachable_TrueInOpenRoom()
    {
        var sim = OpenRoomSim();
        sim.CajunMaster = new VmBBotGame.CajunMasterState();
        var player = sim.Players.First();
        var target = sim.AddBot(64, 0);
        Assert.True(sim.CajunReachable(player, target));
    }

    [Fact]
    public void Reachable_FalseThroughOneSidedWall()
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
        sim.CajunMaster = new VmBBotGame.CajunMasterState();
        var player = sim.Players.First();
        var target = sim.AddBot(64, 0);
        Assert.False(sim.CajunReachable(player, target));
    }

    [Fact]
    public void Dofire_SetsAttackWhenMeleeInRangeAndLos()
    {
        var sim = OpenRoomSim();
        sim.CajunMaster = new VmBBotGame.CajunMasterState();
        sim.CajunBotWeaponInfo["Pistol"] = new VmBBot.BotWeaponData { MoveCombatDist = 0 };
        var player = sim.Players.First();
        var enemy = sim.AddBot(48, 0);
        var bot = new VmBBot.BotState { Enemy = enemy };
        bot.Skill.Reaction = 100;
        bot.Skill.Aiming = 100;
        bot.Skill.Isp = 100;
        var cmd = new VmBBotMove.UserCmd();
        sim.CajunDoFire(player, bot, cmd, "Pistol", () => 0);
        Assert.Equal(VmBBotFuncReachFire.BtAttack, cmd.Buttons & VmBBotFuncReachFire.BtAttack);
    }

    [Fact]
    public void Dofire_WaitsWhileReactionTimerRuns()
    {
        var sim = OpenRoomSim();
        sim.CajunMaster = new VmBBotGame.CajunMasterState();
        sim.CajunBotWeaponInfo["Pistol"] = new VmBBot.BotWeaponData { MoveCombatDist = 0 };
        var player = sim.Players.First();
        var enemy = sim.AddBot(48, 0);
        var bot = new VmBBot.BotState { Enemy = enemy, TReact = 3 };
        var cmd = new VmBBotMove.UserCmd();
        sim.CajunDoFire(player, bot, cmd, "Pistol", () => 0);
        Assert.Equal(0, cmd.Buttons);
    }

    [Fact]
    public void SafeCheckPosition_IgnoresPickupSideEffects()
    {
        var sim = OpenRoomSim();
        var player = sim.Players.First();
        player.CanPickupItems = true;
        Assert.True(VmBBotFuncReachFire.SafeCheckPosition(sim, player, player.X.ToDouble() + 8, player.Y.ToDouble()));
        Assert.True(player.CanPickupItems);
    }

    [Fact]
    public void SetBodyAt_ReusesExistingBodyNode()
    {
        var sim = OpenRoomSim();
        var cajun = new VmBBotGame.CajunMasterState();
        VmBBotFuncReachFire.SetBodyAt(sim, cajun, 10, 20, 0, hostNum: 1);
        var first = cajun.Body1;
        VmBBotFuncReachFire.SetBodyAt(sim, cajun, 30, 40, 0, hostNum: 1);
        Assert.Same(first, cajun.Body1);
        Assert.Equal(30, cajun.Body1!.X.ToDouble());
    }

    [Fact]
    public void FakeFire_ReturnsPositiveDistanceInOpenRoom()
    {
        var sim = OpenRoomSim();
        var player = sim.Players.First();
        var target = sim.AddBot(96, 0);
        var dist = VmBBotFuncReachFire.FakeFire(sim, player, target, new VmBBotMove.UserCmd());
        Assert.True(dist >= VmBBotFuncReachFire.SafeSelfMisdist);
    }

    static AuthoritySimulation OpenRoomSim() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, LightLevel = 160 }],
        Sides = [new LevelSide { Sector = 0 }],
        Things = [new LevelThing { Type = 1, X = -64 }],
    });
}
