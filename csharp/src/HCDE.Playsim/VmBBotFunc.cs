using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Cajun bot sight, mate, and enemy selection from <c>b_func.cpp</c> (honest subset).</summary>
public static class VmBBotFunc
{
    public const double DarkDist = 5000000 / 65536.0;
    public const int WhatsDark = 50;
    public const double EnemyScanFov = 120.0;
    public const int MfShootable = 1 << 2;

    public static bool CheckLos(AuthoritySimulation sim, Actor viewer, Actor to, double vangleDegrees)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(viewer);
        ArgumentNullException.ThrowIfNull(to);
        if (!viewer.CheckSight(to, ActorSightFlags.SeePastBlockEverything))
            return false;
        if (vangleDegrees >= 360)
            return true;
        if (vangleDegrees == 0)
            return false;
        return viewer.CheckFov(to, vangleDegrees / 2);
    }

    public static bool IsLeader(IReadOnlyList<VmBBot.PlayerSlot> slots, Actor playerMo)
    {
        ArgumentNullException.ThrowIfNull(slots);
        ArgumentNullException.ThrowIfNull(playerMo);
        foreach (var slot in slots)
        {
            if (slot.Bot?.Mate == playerMo)
                return true;
        }
        return false;
    }

    public static Actor? ChooseMate(
        AuthoritySimulation sim,
        VmBBot.BotState bot,
        Actor botMo,
        IReadOnlyList<VmBBot.PlayerSlot> slots,
        bool deathmatch,
        bool teamplay)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(bot);
        ArgumentNullException.ThrowIfNull(botMo);
        ArgumentNullException.ThrowIfNull(slots);

        if (bot.Mate is { Destroyed: false } mate)
        {
            if (mate.Health <= 0)
                bot.Mate = null;
            else
                bot.LastMate = mate;
        }

        if (bot.Mate is { Destroyed: false, Health: > 0 })
            return bot.Mate;

        if (bot.LastMate is { Destroyed: false } last && last.Health <= 0)
            bot.LastMate = null;

        Actor? target = null;
        var closestDist = double.PositiveInfinity;

        foreach (var slot in slots)
        {
            if (!slot.InGame || slot.Mo is not { Destroyed: false } clientMo || ReferenceEquals(botMo, clientMo))
                continue;
            if (clientMo.Health <= 0)
                continue;

            if (deathmatch && !Teammate(botMo, clientMo, deathmatch, teamplay))
                continue;
            if (botMo.Health / 2 > clientMo.Health && deathmatch)
                continue;
            if (IsLeader(slots, clientMo))
                continue;

            if (!CombatTrace.CheckSight(sim, botMo, clientMo, ActorSightFlags.IgnoreVisibility))
                continue;

            var test = clientMo.Distance2D(botMo);
            if (test < closestDist)
            {
                closestDist = test;
                target = clientMo;
            }
        }

        return target;
    }

    public static Actor? FindEnemy(
        AuthoritySimulation sim,
        VmBBot.BotState bot,
        Actor botMo,
        IReadOnlyList<VmBBot.PlayerSlot> slots,
        bool deathmatch,
        bool teamplay)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(bot);
        ArgumentNullException.ThrowIfNull(botMo);
        ArgumentNullException.ThrowIfNull(slots);

        if (!deathmatch)
            return ActorEnemySearchActions.RoughMonsterSearch(botMo, 20);

        var vangle = bot.Allround || bot.Mate != null ? 360.0 : EnemyScanFov;
        bot.Allround = false;

        Actor? target = null;
        var closestDist = double.PositiveInfinity;

        foreach (var slot in slots)
        {
            if (!slot.InGame || slot.Mo is not { Destroyed: false } clientMo || ReferenceEquals(botMo, clientMo))
                continue;
            if (clientMo.Health <= 0)
                continue;
            if (Teammate(botMo, clientMo, deathmatch, teamplay))
                continue;

            if (!CheckLos(sim, botMo, clientMo, vangle))
                continue;

            var temp = clientMo.Distance2D(botMo);
            if (temp > DarkDist
                && clientMo.SectorIndex >= 0
                && clientMo.SectorIndex < sim.Level.Sectors.Count
                && sim.Level.Sectors[clientMo.SectorIndex].LightLevel < WhatsDark)
            {
                continue;
            }

            if (temp < closestDist)
            {
                closestDist = temp;
                target = clientMo;
            }
        }

        return target;
    }

    static bool Teammate(Actor self, Actor other, bool deathmatch, bool teamplay)
    {
        var q = new VmPMobjTeamFriendDamage.TeammateQuery(
            OtherNull: false,
            Deathmatch: deathmatch,
            Teamplay: teamplay,
            SelfPlayer: self is PlayerPawn,
            OtherPlayer: other is PlayerPawn,
            Flags: self.Friendly ? VmPMobjTeamFriendDamage.MfFriendly : 0,
            OtherFlags: other.Friendly ? VmPMobjTeamFriendDamage.MfFriendly : 0,
            MyTeam: VmPMobjTeamFriendDamage.GetTeamFromDesignated(VmPMobjTeamFriendDamage.TeamNone, self.FriendPlayer, 0),
            OtherTeam: VmPMobjTeamFriendDamage.GetTeamFromDesignated(VmPMobjTeamFriendDamage.TeamNone, other.FriendPlayer, 0));
        return VmPMobjTeamFriendDamage.IsTeammate(q);
    }
}
