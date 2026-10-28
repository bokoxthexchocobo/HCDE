using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary><c>DBot::ThinkForMove</c> from <c>b_think.cpp</c> (movement AI band).</summary>
public static class VmBThink
{
    public const int Ticrate = 35;
    public const int ForwardWalk = 0x1900;
    public const int SideWalk = 0x1800;
    public const int SideRun = 0x2800;
    public const int MfDropoff = VmBBotMove.MfDropoff;
    public const int BifBotExplosive = VmBBot.BifBotExplosive;
    public const double ThinkDistSq = 50000.0 * 50000.0 / (65536.0 * 65536.0);
    public const double AvoidDist = 45000000.0 / 65536.0;
    public const double FriendDist = 15000000.0 / 65536.0;
    public const double GetInCombat = 35000000.0 / 65536.0;
    public const int AfterTics = 2 * Ticrate;
    public const int MaxRoam = 4 * Ticrate;
    public const double ShootFov = 60.0;

    public sealed class PlayerThinkState
    {
        public int Health { get; set; }
        public int Flags { get; set; }
        public object? ReadyWeapon { get; set; }
    }

    public class ThinkForMoveHost
    {
        public VmBBot.BotState Bot { get; } = new();
        public VmBBotMove.BotMoveState Move { get; }
        public PlayerThinkState Player { get; } = new();
        public AuthoritySimulation? Level { get; set; }

        public ThinkForMoveHost()
        {
            var sim = AuthoritySimulation.Start(new PlayLevel
            {
                Sectors = [new LevelSector { CeilingHeight = 128, LightLevel = 160 }],
                Sides = [new LevelSide { Sector = 0 }],
                Things = [new LevelThing { Type = 1 }],
            });
            var player = sim.Players.First();
            Move = new VmBBotMove.BotMoveState { Sim = sim, Player = player };
            Level = sim;
            PlayerActor = player;
        }
        public int DofireCount { get; private set; }
        public int PrBotMoveResult { get; set; }
        public Actor? MateSightTarget { get; set; }
        public Actor? PlayerPickTarget { get; set; }
        public bool PlayerInGameSlot { get; set; }
        public Actor? PlayerActor { get; set; }
        public bool MissileHasPlanarVel { get; set; } = true;

        public virtual bool CheckLos(Actor? target, double fovDegrees) => target != null;

        public virtual bool CheckSight(Actor? from, Actor? to) => from != null && to != null;

        public virtual bool Reachable(Actor? actor) => actor != null;

        public virtual VmBBot.BotWeaponData GetBotInfo(object? weapon) => new();

        public bool EnemyHasPlayer { get; set; }

        public object? EnemyReadyWeapon { get; set; }

        public void Dofire(VmBBotMove.UserCmd cmd) => DofireCount++;

        public int PrBotMove() => PrBotMoveResult;

        public double Distance2D(Actor? other)
        {
            if (other == null)
                return 0;
            var dx = Move.Mo.X - other.X.ToDouble();
            var dy = Move.Mo.Y - other.Y.ToDouble();
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public (double X, double Y) PlayerPos() => (Move.Mo.X, Move.Mo.Y);
    }

    static bool DestIsPickupKind(Actor dest, AuthoritySimulation level, string kind) =>
        PickupCatalog.IsKindOfClassName(
            dest.ClassDoomEdNum,
            kind,
            level.StrifeHealthPickups,
            level.RavenHealthPickups);

    public static void ThinkForMove(ThinkForMoveHost host, VmBBotMove.UserCmd cmd)
    {
        var bot = host.Bot;
        var move = host.Move;
        var level = host.Level;
        move.Dest = bot.Dest;
        move.Enemy = bot.Enemy;
        move.Missile = bot.Missile;

        var stuck = false;
        var dist = bot.Dest != null ? host.Distance2D(bot.Dest) : 0.0;

        if (bot.Missile != null &&
            (!host.MissileHasPlanarVel || !host.CheckLos(bot.Missile, ShootFov * 3 / 2)))
        {
            bot.Sleft = !bot.Sleft;
            bot.Missile = null;
            move.Missile = null;
        }

        if (bot.Missile != null && host.Distance2D(bot.Missile) < AvoidDist)
        {
            VmBBotMove.Pitch(move.Mo, bot.Missile);
            bot.Angle = VmBBotMove.AngleTo(move.Mo, bot.Missile);
            cmd.SideMove = bot.Sleft ? -SideRun : SideRun;
            cmd.ForwardMove = -VmBBotMove.ForwardRun;

            var (ox, oy) = bot.Old;
            var (px, py) = host.PlayerPos();
            if ((px - ox) * (px - ox) + (py - oy) * (py - oy) < ThinkDistSq && bot.TStrafe <= 0)
            {
                bot.TStrafe = 5;
                bot.Sleft = !bot.Sleft;
            }

            if (bot.Enemy != null && host.CheckLos(bot.Enemy, ShootFov))
                host.Dofire(cmd);
        }
        else if (bot.Enemy != null && host.CheckSight(host.PlayerActor, bot.Enemy))
        {
            VmBBotMove.Pitch(move.Mo, bot.Enemy);

            var pickupRoam = false;
            if (bot.Dest is { SpecialPickup: true } destActor && level != null)
            {
                pickupRoam =
                    ((host.Player.Health < bot.Skill.Isp &&
                      (DestIsPickupKind(destActor, level, "Medikit") ||
                       DestIsPickupKind(destActor, level, "Stimpack") ||
                       DestIsPickupKind(destActor, level, "Soulsphere") ||
                       DestIsPickupKind(destActor, level, "Megasphere") ||
                       DestIsPickupKind(destActor, level, "CrystalVial"))) ||
                     DestIsPickupKind(destActor, level, "Invulnerability") ||
                     DestIsPickupKind(destActor, level, "Invisibility") ||
                     DestIsPickupKind(destActor, level, "Megasphere") ||
                     dist < GetInCombat / 4 ||
                     host.GetBotInfo(host.Player.ReadyWeapon).MoveCombatDist == 0) &&
                    (dist < GetInCombat || host.GetBotInfo(host.Player.ReadyWeapon).MoveCombatDist == 0) &&
                    host.Reachable(bot.Dest);
            }

            if (pickupRoam)
            {
                RunRoamBranch(host, cmd);
            }
            else
            {
                bot.Dest = null;
                move.Dest = null;

                if (host.GetBotInfo(host.Player.ReadyWeapon).MoveCombatDist == 0)
                    host.Player.Flags &= ~MfDropoff;

                if (bot.Enemy is not { IsMonster: true })
                    bot.TFight = AfterTics;

                if (bot.TStrafe <= 0 &&
                    (((host.PlayerPos().X - bot.Old.X) * (host.PlayerPos().X - bot.Old.X) +
                      (host.PlayerPos().Y - bot.Old.Y) * (host.PlayerPos().Y - bot.Old.Y)) < ThinkDistSq ||
                     host.PrBotMove() % 30 == 10))
                {
                    stuck = true;
                    bot.TStrafe = 5;
                    bot.Sleft = !bot.Sleft;
                }

                bot.Angle = VmBBotMove.AngleTo(move.Mo, bot.Enemy);

                var enemyIsMonster = bot.Enemy.IsMonster;
                if (host.Player.ReadyWeapon == null ||
                    host.Distance2D(bot.Enemy) > host.GetBotInfo(host.Player.ReadyWeapon).MoveCombatDist)
                {
                    cmd.ForwardMove = enemyIsMonster ? ForwardWalk : VmBBotMove.ForwardRun;
                }
                else if (!stuck)
                {
                    cmd.ForwardMove = enemyIsMonster ? -ForwardWalk : -VmBBotMove.ForwardRun;
                }

                cmd.SideMove = enemyIsMonster
                    ? (bot.Sleft ? -SideWalk : SideWalk)
                    : (bot.Sleft ? -SideRun : SideRun);
                host.Dofire(cmd);
            }
        }
        else if (bot.Mate != null && bot.Enemy == null && (bot.Dest == null || bot.Dest == bot.Mate))
        {
            VmBBotMove.Pitch(move.Mo, bot.Mate);

            if (!host.Reachable(bot.Mate))
            {
                if (bot.Mate == bot.Dest && host.PrBotMove() < 32)
                {
                    bot.Dest = null;
                    move.Dest = null;
                }

                RunRoamBranch(host, cmd);
            }
            else
            {
                bot.Angle = VmBBotMove.AngleTo(move.Mo, bot.Mate);
                var matedist = host.Distance2D(bot.Mate);
                if (matedist > FriendDist * 2)
                    cmd.ForwardMove = VmBBotMove.ForwardRun;
                else if (matedist > FriendDist)
                    cmd.ForwardMove = ForwardWalk;
                else if (matedist < FriendDist - FriendDist / 3)
                    cmd.ForwardMove = -ForwardWalk;
            }
        }
        else
        {
            RunRoamBranch(host, cmd);
        }

        if (bot.TRoam == 0 && bot.Dest != null)
        {
            bot.Prev = bot.Dest;
            bot.Dest = null;
            move.Dest = null;
        }

        if (bot.TFight < AfterTics / 2)
            host.Player.Flags |= MfDropoff;

        bot.Old = host.PlayerPos();
        move.Dest = bot.Dest;
        move.Enemy = bot.Enemy;
        move.Missile = bot.Missile;
    }

    static void RunRoamBranch(ThinkForMoveHost host, VmBBotMove.UserCmd cmd)
    {
        var bot = host.Bot;
        var move = host.Move;
        var level = host.Level;
        bot.FirstShot = true;

        if (bot.Enemy != null && host.CheckLos(bot.Enemy, ShootFov * 3 / 2))
            host.Dofire(cmd);

        if (bot.Dest is { SpecialPickup: false } deadDest && deadDest.Health < 0)
        {
            bot.Dest = null;
            move.Dest = null;
        }

        if (bot.Dest == null)
        {
            if (bot.TFight > 0 && bot.Enemy != null)
            {
                if (host.EnemyHasPlayer)
                {
                    var enemyInfo = host.GetBotInfo(host.EnemyReadyWeapon);
                    if (((host.EnemyReadyWeapon != null && (enemyInfo.Flags & BifBotExplosive) != 0) ||
                         host.PrBotMove() % 100 > bot.Skill.Isp) &&
                        host.GetBotInfo(host.Player.ReadyWeapon).MoveCombatDist != 0)
                    {
                        bot.Dest = bot.Enemy;
                        move.Dest = bot.Enemy;
                    }
                    else
                        bot.Angle = VmBBotMove.AngleTo(move.Mo, bot.Enemy);
                }
                else
                {
                    bot.Dest = bot.Enemy;
                    move.Dest = bot.Enemy;
                }
            }
            else
            {
                var r = host.PrBotMove();
                if (r < 128 && level != null)
                {
                    var item = level.PickBotRoamInventoryDestination(r);
                    if (item != null)
                    {
                        bot.Dest = item;
                        move.Dest = item;
                    }
                }
                else if (bot.Mate != null && (r < 179 || host.CheckSight(host.PlayerActor, bot.Mate)))
                {
                    bot.Dest = bot.Mate;
                    move.Dest = bot.Mate;
                }
                else if (host.PlayerInGameSlot && host.PlayerPickTarget != null && host.PlayerPickTarget.Health > 0)
                {
                    bot.Dest = host.PlayerPickTarget;
                    move.Dest = host.PlayerPickTarget;
                }
            }

            if (bot.Dest != null)
                bot.TRoam = MaxRoam;
        }

        if (bot.Dest != null)
            VmBBotMove.Roam(move, cmd);
    }
}
