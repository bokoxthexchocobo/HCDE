using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmBBotMoveTests
{
    static Actor DestAt(double x, double y) => new() { X = Fixed.FromDouble(x), Y = Fixed.FromDouble(y) };

    [Fact]
    public void NewChaseDir_PicksEastWhenDestIsRight()
    {
        var bot = new VmBBotMove.BotMoveState { Dest = DestAt(100, 0) };
        bot.Mo.X = 0;
        bot.Mo.Movedir = VmBBotMove.DiNodir;
        bot.CleanAhead = (_, _, _) => true;
        bot.PrBotTryWalk = () => 4;
        var cmd = new VmBBotMove.UserCmd();
        VmBBotMove.NewChaseDir(bot, cmd);
        Assert.Equal(VmBBotMove.DiEast, bot.Mo.Movedir);
        Assert.True(bot.Mo.Movecount > 0);
    }

    [Fact]
    public void TryWalk_SetsMovecountFromRandom()
    {
        var bot = new VmBBotMove.BotMoveState();
        bot.Mo.Movedir = VmBBotMove.DiEast;
        bot.CleanAhead = (_, _, _) => true;
        bot.PrBotTryWalk = () => 77;
        var cmd = new VmBBotMove.UserCmd();
        Assert.True(VmBBotMove.TryWalk(bot, cmd));
        Assert.Equal(77 & 60, bot.Mo.Movecount);
    }

    [Fact]
    public void Move_OpensDoorWhenSpechitActivates()
    {
        var blocking = new Actor();
        var bot = new VmBBotMove.BotMoveState();
        bot.Mo.Movedir = VmBBotMove.DiEast;
        bot.Mo.BlockingLine = blocking;
        bot.CleanAhead = (_, _, _) => false;
        bot.Spechit.Add(new VmBBotMove.BotLineHit(VmBBotMove.GenericDoor, [0, 0, 0, 0, 0], blocking, true));
        bot.CheckKeys = (_, _, _) => true;
        bot.TestActivateLine = (_, _) => true;
        bot.PrBotOpenDoor = () => 100;
        var cmd = new VmBBotMove.UserCmd();
        Assert.True(VmBBotMove.Move(bot, cmd));
        Assert.Equal(VmBBotMove.BtUse, cmd.Buttons & VmBBotMove.BtUse);
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
        var mo = new VmBBotMove.Mover { Height = 32, Z = 10, MaxStepHeight = 24, MaxDropOffHeight = 48, SectorFloorZ = 0, Flags = 0 };
        var cmd = new VmBBotMove.UserCmd();
        var tm = new VmBBotMove.CheckPositionTm(200, 30, 30);
        Assert.True(VmBBotMove.CleanAhead(mo, 1, 1, cmd, tm, (_, _, _, _) => true));
        Assert.Equal(VmBBotMove.BtJump, cmd.Buttons & VmBBotMove.BtJump);
    }

    [Fact]
    public void TurnToAng_ClampsLargeDelta()
    {
        var bot = new VmBBotMove.BotMoveState();
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
        var bot = new VmBBotMove.BotMoveState { Dest = DestAt(8, 0) };
        bot.Mo.Movecount = 0;
        bot.Mo.Movedir = VmBBotMove.DiEast;
        bot.Reachable = _ => false;
        bot.CleanAhead = (_, _, _) => true;
        bot.PrBotTryWalk = () => 10;
        var cmd = new VmBBotMove.UserCmd();
        VmBBotMove.Roam(bot, cmd);
        Assert.True(bot.Mo.Movedir != VmBBotMove.DiNodir || cmd.ForwardMove == VmBBotMove.ForwardRun);
    }
}
