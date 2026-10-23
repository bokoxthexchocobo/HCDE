using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Cajun <c>Reachable</c>, <c>Dofire</c>, and rocket-aim helpers from <c>b_func.cpp</c>.</summary>
public static class VmBBotFuncReachFire
{
    public const double MaxTraverseDist = 100000000.0 / 65536.0;
    public const int MaxMoveHeight = 32;
    public const double DefMeleeRange = 64;
    public const double ShootFov = 60;
    public const double SafeSelfMisdist = 140;
    public const double RocketSpeed = 20;
    public const int BtAttack = 1;
    public const int BifBotReactionSkillThing = 1;
    public const int BifBotExplosive = 2;
    public const int BifBotBfg = 4;
    public const int MfShootable = 1 << 2;

    sealed class PathIntercept
    {
        public double Frac { get; init; }
        public bool IsLine { get; init; }
        public LevelLine? Line { get; init; }
        public Actor? Thing { get; init; }
        public bool Done { get; set; }
    }

    public static bool Reachable(AuthoritySimulation sim, PlayerPawn player, Actor target)
    {
        ArgumentNullException.ThrowIfNull(sim);
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(target);
        if (ReferenceEquals(player, target))
            return false;

        var playerSector = SectorAt(sim, player.SectorIndex);
        var targetSector = SectorAt(sim, target.SectorIndex);
        if (targetSector.CeilingHeight - targetSector.FloorHeight < player.Height.ToDouble())
            return false;

        var lastSector = playerSector;
        var lastZ = lastSector.FloorHeight;
        var estimatedDist = player.Distance2D(target);
        var vx = player.VelocityX.ToDouble();
        var vy = player.VelocityY.ToDouble();
        var x0 = player.X.ToDouble() + vx;
        var y0 = player.Y.ToDouble() + vy;
        var x1 = target.X.ToDouble();
        var y1 = target.Y.ToDouble();

        var intercepts = CollectPathIntercepts(sim, player, x0, y0, x1, y1);
        var reachable = true;
        var scanIndex = 0;
        while (TryNextIntercept(intercepts, ref scanIndex, out var hit))
        {
            var frac = hit.Frac - 4 / MaxTraverseDist;
            var dist = frac * MaxTraverseDist;
            var hitX = x0 + (x1 - x0) * frac;
            var hitY = y0 + (y1 - y0) * frac;

            if (hit.IsLine && hit.Line is { } line)
            {
                var twoSided = (line.Flags & LevelLine.TwoSidedFlag) != 0;
                var blocks = (line.Flags & (LevelLine.BlockingFlag | LevelLine.BlockEverythingFlag | LevelLine.BlockPlayersFlag)) != 0;
                if (!twoSided || blocks)
                    return false;

                var frontSecIdx = sim.Level.Sides[line.SideFront].Sector;
                var backSecIdx = line.SideBack >= 0 ? sim.Level.Sides[line.SideBack].Sector : frontSecIdx;
                var sectorIndex = backSecIdx == lastSector.Index ? frontSecIdx : backSecIdx;
                var sector = SectorAt(sim, sectorIndex);
                var ceiling = sector.CeilingHeight;
                var floor = sector.FloorHeight;
                var specialFit = ceiling == floor && line.Special != 0;
                if (!IsDangerousSector(sector)
                    && floor <= lastZ + MaxMoveHeight
                    && (specialFit || ceiling - floor >= player.Height.ToDouble()))
                {
                    lastZ = floor;
                    lastSector = sector;
                    continue;
                }

                return false;
            }

            if (dist > estimatedDist)
                return true;

            if (hit.Thing is { } thing)
            {
                if (ReferenceEquals(thing, player))
                    continue;
                if (ReferenceEquals(thing, target)
                    && targetSector.FloorHeight <= lastZ + MaxMoveHeight)
                    return true;
                reachable = false;
            }
        }

        return reachable;
    }

    public static void Dofire(
        AuthoritySimulation sim,
        VmBBotGame.CajunMasterState cajun,
        PlayerPawn player,
        VmBBot.BotState bot,
        VmBBotMove.UserCmd cmd,
        string readyWeaponClass,
        IReadOnlyDictionary<string, VmBBot.BotWeaponData> botWeaponInfo,
        Func<int> prBotDoFire)
    {
        var enemy = bot.Enemy;
        if (enemy is not { Destroyed: false, Shootable: true } || enemy.Health <= 0)
            return;

        var weapon = GetBotInfo(readyWeaponClass, botWeaponInfo);
        if (player.DamageCount > bot.Skill.Isp)
        {
            bot.FirstShot = true;
            return;
        }

        if (bot.FirstShot && (weapon.Flags & BifBotReactionSkillThing) == 0)
            bot.TReact = (100 - bot.Skill.Reaction + 1) / ((prBotDoFire() % 3) + 3);
        bot.FirstShot = false;
        if (bot.TReact > 0)
            return;

        var noFire = true;
        var dist = player.Distance2D(
            enemy,
            player.VelocityX.ToDouble() - enemy.VelocityX.ToDouble(),
            player.VelocityY.ToDouble() - enemy.VelocityY.ToDouble());

        if (weapon.MoveCombatDist == 0)
            noFire = dist > DefMeleeRange * 4;
        else if ((weapon.Flags & BifBotBfg) != 0)
        {
            if (prBotDoFire() % 200 <= bot.Skill.Reaction
                && VmBBotFunc.CheckLos(sim, player, enemy, ShootFov))
                noFire = false;
        }
        else if (weapon.ProjectileTypeName != null)
        {
            if ((weapon.Flags & BifBotExplosive) != 0)
            {
                var an = FireRox(sim, cajun, player, bot, enemy, cmd, prBotDoFire);
                if (an is not null)
                {
                    bot.Angle = an.Value;
                    if (Math.Abs(DeltaAngle(an.Value, player.Angle.ToDegrees())) < 12)
                    {
                        bot.TRocket = 9;
                        noFire = false;
                    }
                }
            }

            dist = player.Distance2D(enemy);
            var fm = dist / RocketSpeed;
            SetBodyAt(sim, cajun, enemy.X.ToDouble() + enemy.VelocityX.ToDouble() * fm * 2,
                enemy.Y.ToDouble() + enemy.VelocityY.ToDouble() * fm * 2, enemy.Z.ToDouble(), hostNum: 1);
            bot.Angle = AngleTo(player, cajun.Body1!);
            if (VmBBotFunc.CheckLos(sim, player, enemy, ShootFov))
                noFire = false;
        }
        else
        {
            bot.Angle = AngleTo(player, enemy);
            var aimingPenalty = 0;
            if (enemy.Shadow)
                aimingPenalty += (prBotDoFire() % 25) + 10;
            if (enemy.SectorIndex >= 0 && enemy.SectorIndex < sim.Level.Sectors.Count
                && sim.Level.Sectors[enemy.SectorIndex].LightLevel < VmBBotFunc.WhatsDark)
                aimingPenalty += prBotDoFire() % 40;
            if (player.DamageCount > 0)
                aimingPenalty += player.DamageCount;
            var aimingValue = bot.Skill.Aiming - aimingPenalty;
            if (aimingValue <= 0)
                aimingValue = 1;
            var spread = ShootFov / 2 - aimingValue * ShootFov / 200;
            if (spread <= 0)
                spread = 1;
            if (bot.Increase)
                bot.Angle += spread;
            else
                bot.Angle -= spread;
            if (Math.Abs(DeltaAngle(bot.Angle, player.Angle.ToDegrees())) < 4)
                bot.Increase = !bot.Increase;
            if (VmBBotFunc.CheckLos(sim, player, enemy, ShootFov / 2))
                noFire = false;
        }

        if (!noFire)
            cmd.Buttons |= BtAttack;
    }

    public static void SetBodyAt(
        AuthoritySimulation sim,
        VmBBotGame.CajunMasterState cajun,
        double x,
        double y,
        double z,
        int hostNum)
    {
        if (hostNum == 1)
        {
            if (cajun.Body1 is { Destroyed: false } body)
            {
                body.X = Fixed.FromDouble(x);
                body.Y = Fixed.FromDouble(y);
                body.Z = Fixed.FromDouble(z);
            }
            else
            {
                cajun.Body1 = SpawnBodyNode(sim, x, y, z);
            }
        }
        else if (hostNum == 2)
        {
            if (cajun.Body2 is { Destroyed: false } body)
            {
                body.X = Fixed.FromDouble(x);
                body.Y = Fixed.FromDouble(y);
                body.Z = Fixed.FromDouble(z);
            }
            else
            {
                cajun.Body2 = SpawnBodyNode(sim, x, y, z);
            }
        }
    }

    public static double FakeFire(AuthoritySimulation sim, Actor source, Actor dest, VmBBotMove.UserCmd cmd)
    {
        var speed = RocketSpeed;
        var dx = dest.X.ToDouble() - source.X.ToDouble();
        var dy = dest.Y.ToDouble() - source.Y.ToDouble();
        var len = Math.Sqrt(dx * dx + dy * dy);
        if (len < 1e-9)
            return 0;
        dx = dx / len * speed;
        dy = dy / len * speed;
        var dist = 0.0;
        var x = source.X.ToDouble();
        var y = source.Y.ToDouble();
        while (dist < SafeSelfMisdist)
        {
            dist += speed;
            x += dx;
            y += dy;
            if (!SafeCheckPosition(sim, source, x, y))
                break;
        }

        return dist;
    }

    public static double? FireRox(
        AuthoritySimulation sim,
        VmBBotGame.CajunMasterState cajun,
        PlayerPawn player,
        VmBBot.BotState bot,
        Actor enemy,
        VmBBotMove.UserCmd cmd,
        Func<int> _)
    {
        SetBodyAt(sim, cajun,
            player.X.ToDouble(),
            player.Y.ToDouble(),
            player.Z.ToDouble() + player.Height.ToDouble() / 2, hostNum: 2);
        var actor = cajun.Body2!;
        var dist = actor.Distance2D(enemy);
        if (dist < SafeSelfMisdist)
            return null;
        var m = (dist + 1) / RocketSpeed;
        SetBodyAt(sim, cajun,
            enemy.X.ToDouble() + enemy.VelocityX.ToDouble() * (m + 2),
            enemy.Y.ToDouble() + enemy.VelocityY.ToDouble() * (m + 2),
            enemy.Z.ToDouble(), hostNum: 1);
        if (actor.CheckSight(cajun.Body1!, ActorSightFlags.IgnoreVisibility)
            && SafeCheckPosition(sim, player, actor.X.ToDouble(), actor.Y.ToDouble())
            && FakeFire(sim, actor, cajun.Body1!, cmd) >= SafeSelfMisdist)
            return AngleTo(actor, cajun.Body1!);
        if (actor.CheckSight(enemy)
            && FakeFire(sim, player, enemy, cmd) >= SafeSelfMisdist)
            return AngleTo(player, enemy);
        return null;
    }

    public static bool SafeCheckPosition(AuthoritySimulation sim, Actor actor, double x, double y)
    {
        var saved = actor.CanPickupItems;
        actor.CanPickupItems = false;
        var ok = ActorPhysics.TryMove(sim, actor, x, y, out _);
        actor.CanPickupItems = saved;
        return ok;
    }

    static VmBBot.BotWeaponData GetBotInfo(string weaponClass, IReadOnlyDictionary<string, VmBBot.BotWeaponData> table) =>
        table.TryGetValue(weaponClass, out var data) ? data : new VmBBot.BotWeaponData();

    static Actor SpawnBodyNode(AuthoritySimulation sim, double x, double y, double z)
    {
        var body = new Actor
        {
            X = Fixed.FromDouble(x),
            Y = Fixed.FromDouble(y),
            Z = Fixed.FromDouble(z),
            Height = Fixed.FromInt(1),
            Radius = Fixed.FromInt(1),
            Simulation = sim,
            Id = sim.AllocateCajunAuxActorId(),
        };
        sim.RegisterCajunAuxActor(body);
        return body;
    }

    static LevelSector SectorAt(AuthoritySimulation sim, int index) =>
        sim.Level.Sectors[index < 0 || index >= sim.Level.Sectors.Count ? 0 : index];

    static bool IsDangerousSector(LevelSector sector) => sector.DamageAmount > 0;

    static double AngleTo(Actor from, Actor to) =>
        Math.Atan2(to.Y.ToDouble() - from.Y.ToDouble(), to.X.ToDouble() - from.X.ToDouble()) * (180 / Math.PI);

    static double DeltaAngle(double a, double b)
    {
        var d = a - b;
        while (d > 180) d -= 360;
        while (d < -180) d += 360;
        return d;
    }

    static List<PathIntercept> CollectPathIntercepts(AuthoritySimulation sim, Actor mover, double x0, double y0, double x1, double y1)
    {
        var list = new List<PathIntercept>();
        var trace = new VmPMapUtlLineOpening.Divline(x0, y0, x1 - x0, y1 - y0);
        foreach (var line in sim.Level.Lines)
        {
            var dl = new VmPMapUtlLineOpening.Divline(line.X1, line.Y1, line.X2 - line.X1, line.Y2 - line.Y1);
            var frac = VmPMapUtlLineOpening.InterceptVector(trace, dl);
            if (frac < 0 || frac > 1)
                continue;
            list.Add(new PathIntercept { Frac = frac, IsLine = true, Line = line });
        }

        foreach (var thing in sim.Actors)
        {
            if (thing.Destroyed || ReferenceEquals(thing, mover))
                continue;
            var radius = thing.Radius.ToDouble();
            var cx = thing.X.ToDouble();
            var cy = thing.Y.ToDouble();
            if (DistancePointToSegment(cx, cy, x0, y0, x1, y1) > radius)
                continue;
            var frac = ProjectFracOnSegment(cx, cy, x0, y0, x1, y1);
            list.Add(new PathIntercept { Frac = frac, IsLine = false, Thing = thing });
        }

        return list;
    }

    static bool TryNextIntercept(List<PathIntercept> intercepts, ref int scanIndex, out PathIntercept hit)
    {
        PathIntercept? best = null;
        var dist = double.MaxValue;
        for (var i = scanIndex; i < intercepts.Count; i++)
        {
            var scan = intercepts[i];
            if (scan.Frac < dist && !scan.Done)
            {
                dist = scan.Frac;
                best = scan;
            }
        }

        if (dist > 1 || best is null)
        {
            hit = null!;
            return false;
        }

        best.Done = true;
        hit = best;
        scanIndex++;
        return true;
    }

    static double DistancePointToSegment(double px, double py, double x0, double y0, double x1, double y1)
    {
        var dx = x1 - x0;
        var dy = y1 - y0;
        var len2 = dx * dx + dy * dy;
        if (len2 < 1e-9)
            return Math.Sqrt((px - x0) * (px - x0) + (py - y0) * (py - y0));
        var t = Math.Clamp(((px - x0) * dx + (py - y0) * dy) / len2, 0, 1);
        var nx = x0 + t * dx;
        var ny = y0 + t * dy;
        return Math.Sqrt((px - nx) * (px - nx) + (py - ny) * (py - ny));
    }

    static double ProjectFracOnSegment(double px, double py, double x0, double y0, double x1, double y1)
    {
        var dx = x1 - x0;
        var dy = y1 - y0;
        var len2 = dx * dx + dy * dy;
        if (len2 < 1e-9)
            return 0;
        return Math.Clamp(((px - x0) * dx + (py - y0) * dy) / len2, 0, 1);
    }
}
