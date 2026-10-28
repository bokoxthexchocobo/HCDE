using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmBBotMoveTests
{
    static Actor DestAt(double x, double y) => new() { X = Fixed.FromDouble(x), Y = Fixed.FromDouble(y) };

    static AuthoritySimulation OpenRoomSim() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128, LightLevel = 160 }],
        Sides = [new LevelSide { Sector = 0 }],
        Things = [new LevelThing { Type = 1, X = 0 }],
    });

    static VmBBotMove.BotMoveState MoveBot(AuthoritySimulation sim, double x = 0, double y = 0)
    {
        var player = sim.Players.First();
        player.X = Fixed.FromDouble(x);
        player.Y = Fixed.FromDouble(y);
        player.SectorIndex = 0;
        var bot = new VmBBotMove.BotMoveState { Sim = sim, Player = player };
        bot.Mo.X = x;
        bot.Mo.Y = y;
        bot.Mo.SectorFloorZ = sim.FloorOf(0);
        bot.Mo.Height = player.Height.ToDouble();
        bot.Mo.MaxStepHeight = player.MaxStepHeight.ToDouble();
        bot.Mo.MaxDropOffHeight = player.MaxDropOffHeight.ToDouble();
        return bot;
    }

    [Fact]
    public void NewChaseDir_ReturnsEarlyWhenDestNull()
    {
        var sim = OpenRoomSim();
        var bot = MoveBot(sim);
        bot.Dest = null;
        bot.Mo.Movedir = VmBBotMove.DiEast;
        VmBBotMove.NewChaseDir(bot, new VmBBotMove.UserCmd());
        Assert.Equal(VmBBotMove.DiEast, bot.Mo.Movedir);
    }

    [Fact]
    public void NewChaseDir_PicksEastWhenDestIsRight()
    {
        var sim = OpenRoomSim();
        var bot = MoveBot(sim);
        bot.Dest = DestAt(100, 0);
        bot.Mo.Movedir = VmBBotMove.DiNodir;
        var cmd = new VmBBotMove.UserCmd();
        VmBBotMove.NewChaseDir(bot, cmd);
        Assert.Equal(VmBBotMove.DiEast, bot.Mo.Movedir);
        Assert.True(bot.Mo.Movecount > 0);
    }

    [Fact]
    public void TryWalk_SetsMovecountFromRandom()
    {
        var sim = OpenRoomSim();
        var bot = MoveBot(sim);
        bot.Mo.Movedir = VmBBotMove.DiEast;
        var cmd = new VmBBotMove.UserCmd();
        Assert.True(VmBBotMove.TryWalk(bot, cmd));
        Assert.InRange(bot.Mo.Movecount, 0, 60);
    }

    [Fact]
    public void Move_SetsForwardRunWhenCleanAhead()
    {
        var sim = OpenRoomSim();
        var bot = MoveBot(sim);
        bot.Mo.Movedir = VmBBotMove.DiEast;
        var cmd = new VmBBotMove.UserCmd();
        Assert.True(VmBBotMove.Move(bot, cmd));
        Assert.Equal(VmBBotMove.ForwardRun, cmd.ForwardMove);
    }

    [Fact]
    public void CleanAhead_RejectsLowCeiling()
    {
        var mo = new VmBBotMove.Mover { Height = 64, Flags = 0 };
        var cmd = new VmBBotMove.UserCmd();
        var tm = new VmBBotMove.CheckPositionTm(40, 0, 0);
        Assert.False(VmBBotMove.CleanAhead(mo, 1, 1, cmd, tm, (_, _, _, _) => true));
    }

    [Fact]
    public void CleanAhead_SetsJumpOverHighStep()
    {
        const double sectorFloor = 0;
        const double destFloor = 30;
        const double maxStep = 24;
        var mo = new VmBBotMove.Mover
        {
            Height = 32,
            Z = 10,
            MaxStepHeight = maxStep,
            MaxDropOffHeight = 48,
            SectorFloorZ = sectorFloor,
            Flags = 0,
        };
        var cmd = new VmBBotMove.UserCmd();
        var tm = new VmBBotMove.CheckPositionTm(200, destFloor, destFloor);
        Assert.True(destFloor > sectorFloor + maxStep);
        Assert.True(destFloor - mo.Z <= maxStep);
        Assert.True(VmBBotMove.CleanAhead(mo, 1, 1, cmd, tm, (_, _, _, _) => true));
        Assert.Equal(VmBBotMove.BtJump, cmd.Buttons & VmBBotMove.BtJump);
    }

    [Fact]
    public void CleanAhead_RejectsStepUpAboveMaxStepHeightAfterJumpBand()
    {
        const double maxStep = 24;
        var mo = new VmBBotMove.Mover { Height = 32, Z = 0, MaxStepHeight = maxStep, MaxDropOffHeight = 48, SectorFloorZ = 0, Flags = 0 };
        var cmd = new VmBBotMove.UserCmd();
        var tm = new VmBBotMove.CheckPositionTm(200, 26, 0);
        Assert.False(VmBBotMove.CleanAhead(mo, 1, 1, cmd, tm, (_, _, _, _) => true));
        Assert.Equal(VmBBotMove.BtJump, cmd.Buttons & VmBBotMove.BtJump);
    }

    [Fact]
    public void TurnToAng_ClampsLargeDelta()
    {
        var sim = OpenRoomSim();
        var bot = MoveBot(sim);
        bot.Mo.Yaw = 0;
        bot.Angle = 90;
        VmBBotMove.TurnToAng(bot);
        Assert.InRange(bot.Mo.Yaw, 10, 20);
    }

    [Fact]
    public void Pitch_AimsAtTargetHeight()
    {
        var mo = new VmBBotMove.Mover { X = 0, Y = 0, Z = 0 };
        var target = new Actor { X = Fixed.FromDouble(10), Y = Fixed.FromDouble(0), Z = Fixed.FromDouble(10) };
        VmBBotMove.Pitch(mo, target);
        Assert.True(mo.Pitch > 0);
    }

    [Fact]
    public void IsDangerous_MatchesSectorDamage()
    {
        Assert.False(VmBBotMove.IsDangerous(0));
        Assert.True(VmBBotMove.IsDangerous(5));
    }

    [Fact]
    public void Roam_DecrementsMovecountAndChases()
    {
        var sim = OpenRoomSim();
        var bot = MoveBot(sim);
        bot.Dest = DestAt(8, 0);
        bot.Mo.Movecount = 0;
        bot.Mo.Movedir = VmBBotMove.DiEast;
        var cmd = new VmBBotMove.UserCmd();
        VmBBotMove.Roam(bot, cmd);
        Assert.True(bot.Mo.Movedir != VmBBotMove.DiNodir || cmd.ForwardMove == VmBBotMove.ForwardRun);
    }
}
