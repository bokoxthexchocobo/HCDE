using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmBThinkTests
{
    [Fact]
    public void ThinkForMove_MissileAvoid_Backpedals()
    {
        var host = new VmBThink.ThinkForMoveHost();
        var missile = new Actor { X = Fixed.FromDouble(10), Y = Fixed.FromDouble(0) };
        host.Move.Mo.X = 0;
        host.Move.Mo.Y = 0;
        host.Bot.Missile = missile;
        host.MissileHasPlanarVel = true;
        var cmd = new VmBBotMove.UserCmd();
        VmBThink.ThinkForMove(host, cmd);
        Assert.Equal(-VmBBotMove.ForwardRun, cmd.ForwardMove);
        Assert.NotEqual(0, cmd.SideMove);
    }

    [Fact]
    public void ThinkForMove_FightEnemy_ForwardRunAndDofire()
    {
        var host = new FightHost();
        var enemy = new Actor { X = Fixed.FromDouble(100), Y = Fixed.FromDouble(0) };
        host.Move.Mo.X = 0;
        host.Bot.Enemy = enemy;
        host.PlayerActor = new Actor();
        host.GetBotInfoResult = new VmBBot.BotWeaponData { MoveCombatDist = 64 };
        var cmd = new VmBBotMove.UserCmd();
        VmBThink.ThinkForMove(host, cmd);
        Assert.Equal(VmBBotMove.ForwardRun, cmd.ForwardMove);
        Assert.Equal(1, host.DofireCount);
    }

    [Fact]
    public void ThinkForMove_MateFollow_RunWhenFar()
    {
        var host = new VmBThink.ThinkForMoveHost();
        var mate = new Actor { X = Fixed.FromDouble(500), Y = Fixed.FromDouble(0) };
        host.Move.Mo.X = 0;
        host.Bot.Mate = mate;
        host.Bot.Dest = mate;
        var cmd = new VmBBotMove.UserCmd();
        VmBThink.ThinkForMove(host, cmd);
        Assert.Equal(VmBBotMove.ForwardRun, cmd.ForwardMove);
    }

    [Fact]
    public void ThinkForMove_ClearsDestWhenRoamTimerZero()
    {
        var host = new VmBThink.ThinkForMoveHost();
        var dest = new Actor();
        host.Bot.Dest = dest;
        host.Bot.TRoam = 0;
        var cmd = new VmBBotMove.UserCmd();
        VmBThink.ThinkForMove(host, cmd);
        Assert.Null(host.Bot.Dest);
        Assert.Equal(dest, host.Bot.Prev);
    }

    sealed class FightHost : VmBThink.ThinkForMoveHost
    {
        public VmBBot.BotWeaponData GetBotInfoResult { get; set; } = new();

        public override bool CheckSight(Actor? from, Actor? to) => true;

        public override VmBBot.BotWeaponData GetBotInfo(object? weapon) => GetBotInfoResult;
    }
}
