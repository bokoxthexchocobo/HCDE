using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>
/// Flat-sector cylinder physics. A monster is refused when the destination floor is
/// strictly more than MaxDropOffHeight below the floor it is standing on. Players,
/// projectiles, and floating actors may cross. A blocked player step moves up to the
/// nearest wall and clips along it, then once more if a second wall blocks that slide
/// (<c>FSlide::SlideMove</c> / <c>HitSlideLine</c>, the <c>MF2_SLIDE</c> path).
/// Bounding-box corner traces, icy bounce, slopes, portals, and 3D floors are not implemented.
/// A player already on the ground can step onto a solid non-player whose top is within
/// MaxStepHeight, then stand there (<c>P_TryMove</c> thingblocker / <c>MF2_PASSMOBJ</c>).
/// A grounded monster can do that only when the other actor has <c>MF4_ACTLIKEBRIDGE</c>.
/// An <c>MF_ICECORPSE</c> actor also steps onto a corpse. Other actors walk through corpses.
/// A shootable actor whose headroom is below its height takes 10 crush damage every four tics
/// after movers run (<c>P_CheckPosition</c> / <c>NAME_Crush</c>). Standing riders take the same
/// mover and sector pinch crush as the actor they are on. A carrier's horizontal and vertical
/// step delta moves riders that were standing on it at step start. A per-sector scroll vector
/// carries grounded non-floating actors after thinkers run. Line-triggered scrollers, polyobjects,
/// and touchy detonation are absent.
/// </summary>
public static class ActorPhysics
{
    public const double GroundFriction = 0xE800 / 65536.0;
    public const double Gravity = 1;
    public const double MaxMove = 30;
    /// <summary>Native default sector pinch crush when <c>crushchange</c> is 10.</summary>
    public const int SectorCrushDamage = 10;

    /// <summary>Native rider crush subset. The rider stands on the carrier's top within both radii.</summary>
    public static bool IsStandingOn(Actor carrier, Actor rider)
    {
        if (ReferenceEquals(carrier, rider) || carrier.ThruActors || rider.ThruActors || !rider.BlocksActors) return false;
        var top = carrier.Z.ToDouble() + carrier.Height.ToDouble();
        if (Math.Abs(rider.Z.ToDouble() - top) > 1) return false;
        var reach = carrier.Radius.ToDouble() + rider.Radius.ToDouble();
        var dx = rider.X.ToDouble() - carrier.X.ToDouble();
        var dy = rider.Y.ToDouble() - carrier.Y.ToDouble();
        return dx * dx + dy * dy <= reach * reach;
    }

    /// <summary>Applies the same crush damage to every actor standing on <paramref name="carrier"/>.</summary>
    public static void CrushStandingRiders(AuthoritySimulation sim, Actor carrier, int damage, int tic, string? damageType = null)
    {
        if (damage <= 0) return;
        foreach (var rider in sim.Actors)
        {
            if (!IsStandingOn(carrier, rider)) continue;
            rider.MarkMoverCrush(tic);
            ActorDamage.Apply(rider, damage, damageType: damageType);
        }
    }

    /// <summary>Moves riders that were on <paramref name="carrier"/> before it stepped this tic.</summary>
    public static void CarryStandingRiders(AuthoritySimulation sim, Actor carrier, double dx, double dy, double dz, IReadOnlyList<Actor> riders)
    {
        if (riders.Count == 0 || (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9 && Math.Abs(dz) < 1e-9))
            return;
        foreach (var rider in riders)
        {
            if (ReferenceEquals(rider, carrier) || rider.Destroyed) continue;
            var x = rider.X.ToDouble() + dx;
            var y = rider.Y.ToDouble() + dy;
            if (!TryMove(sim, rider, x, y, out _))
            {
                if (!TryMove(sim, rider, rider.X.ToDouble() + dx, rider.Y.ToDouble(), out _)
                    && !TryMove(sim, rider, rider.X.ToDouble(), rider.Y.ToDouble() + dy, out _))
                    continue;
            }
            if (Math.Abs(dz) > 1e-9)
                rider.Z = Fixed.FromDouble(rider.Z.ToDouble() + dz);
            FitToSector(sim, rider, carryFloor: false);
            RefreshOnMobj(sim, rider);
        }
    }

    /// <summary>Native <c>Level-&gt;Scrolls</c> carry subset for grounded actors.</summary>
    internal static void ApplySectorScroll(AuthoritySimulation sim, Actor actor)
    {
        if (actor.Destroyed || actor.Floating || !actor.OnGround || actor is ProjectileActor)
            return;
        if (actor is not PlayerPawn && !actor.InScrollSector)
            return;
        if (!TrySectorCarryDelta(sim, actor, out var dx, out var dy))
            return;
        TryMove(sim, actor, actor.X.ToDouble() + dx, actor.Y.ToDouble() + dy, out _);
    }

    internal static bool TrySectorCarryDelta(AuthoritySimulation sim, Actor actor, out double dx, out double dy)
    {
        dx = dy = 0;
        var radius = actor.Radius.ToDouble();
        var x = actor.X.ToDouble();
        var y = actor.Y.ToDouble();
        var touching = TouchingSectorIndices(sim.Level, x, y, radius);
        if (touching.Count == 0)
            return false;

        var countX = 0;
        var countY = 0;
        foreach (var sector in touching)
        {
            if ((uint)sector >= (uint)sim.SectorScrollX.Length)
                continue;
            var (scrollX, scrollY) = sim.SumCarryScrollForActor(sector, actor);
            if (actor is PlayerPawn && (uint)sector < (uint)sim.Level.Sectors.Count
                && HexenSectorScroll.TryGetPlayerCarryDelta(sim.Level.Sectors[sector].Special, out var hexDx, out var hexDy))
            {
                scrollX += hexDx;
                scrollY += hexDy;
            }
            if (Math.Abs(scrollX) > 1e-9)
            {
                dx += scrollX;
                countX++;
            }
            if (Math.Abs(scrollY) > 1e-9)
            {
                dy += scrollY;
                countY++;
            }
        }

        var averageMultiSector = actor is PlayerPawn || !sim.Compat.HasFlag(CompatSurface.BoomScroll);
        if (averageMultiSector)
        {
            if (countX > 1)
                dx /= countX;
            if (countY > 1)
                dy /= countY;
        }
        return Math.Abs(dx) > 1e-9 || Math.Abs(dy) > 1e-9;
    }

    /// <summary>Center plus radius samples; native <c>touching_sectorlist</c> subset.</summary>
    internal static HashSet<int> TouchingSectorIndices(PlayLevel level, double x, double y, double radius)
    {
        var sectors = new HashSet<int>();
        void Sample(double px, double py)
        {
            var sector = SectorAt(level, px, py);
            if (sector >= 0)
                sectors.Add(sector);
        }
        Sample(x, y);
        if (radius > 0)
        {
            Sample(x + radius, y);
            Sample(x - radius, y);
            Sample(x, y + radius);
            Sample(x, y - radius);
        }
        return sectors;
    }

    internal static void RefreshOnMobj(AuthoritySimulation sim, Actor actor)
    {
        if (actor.Floating || actor is ProjectileActor)
        {
            actor.OnMobj = false;
            return;
        }
        var floor = sim.FloorOf(actor.SectorIndex);
        var support = SupportFloor(sim, actor);
        actor.OnMobj = support > floor + 1e-4;
    }

    public static int SectorAt(PlayLevel level, double x, double y)
    {
        if (level.Sectors.Count == 0) return -1;
        for (var sector = 0; sector < level.Sectors.Count; sector++)
        {
            var inside = false;
            foreach (var line in level.Lines)
            {
                var front = SideSector(level, line.SideFront);
                var back = SideSector(level, line.SideBack);
                if (front == back || (front != sector && back != sector)) continue;
                if ((line.Y1 > y) != (line.Y2 > y)
                    && x < (line.X2 - line.X1) * (y - line.Y1) / (line.Y2 - line.Y1) + line.X1)
                    inside = !inside;
            }
            if (inside) return sector;
        }
        // Synthetic levels without enclosing geometry may still define one sector.
        return level.Sectors.Count == 1 ? 0 : -1;
    }

    private static int SideSector(PlayLevel level, int side) =>
        (uint)side < (uint)level.Sides.Count ? level.Sides[side].Sector : -1;

    public static void PlaceOnFloor(AuthoritySimulation sim, Actor actor)
    {
        actor.SectorIndex = SectorAt(sim.Level, actor.X.ToDouble(), actor.Y.ToDouble());
        actor.Z = Fixed.FromDouble(sim.FloorOf(actor.SectorIndex));
        actor.OnGround = true;
        actor.RememberPosition();
    }

    public static void Step(AuthoritySimulation sim, Actor actor)
    {
        if (actor.VerticalFriction && !actor.IsDead)
        {
            var vertical = actor.VelocityZ.ToDouble();
            if (Math.Abs(vertical) < 0.25) { actor.VelocityZ = default; actor.VerticalFriction = false; }
            else actor.VelocityZ = Fixed.FromDouble(vertical * GroundFriction);
        }
        if (actor.Brain?.Charging == true) { StepCharge(sim, actor); return; }
        var startX = actor.X.ToDouble();
        var startY = actor.Y.ToDouble();
        var startZ = actor.Z.ToDouble();
        var riders = sim.Actors.Where(candidate => IsStandingOn(actor, candidate)).ToArray();
        var vx = Math.Clamp(actor.VelocityX.ToDouble(), -MaxMove, MaxMove);
        var vy = Math.Clamp(actor.VelocityY.ToDouble(), -MaxMove, MaxMove);
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(vx), Math.Abs(vy)) / 2));
        for (var i = 0; i < steps; i++)
        {
            var x = actor.X.ToDouble();
            var y = actor.Y.ToDouble();
            var dx = vx / steps;
            var dy = vy / steps;
            if (!TryMove(sim, actor, x + dx, y + dy, out var wall))
            {
                if (wall != null && (actor.CanSlide || actor.Blasted) && actor is not ProjectileActor)
                    SlideFromBlock(sim, actor, ref vx, ref vy, steps, x, y, dx, dy);
                else if (wall != null)
                {
                    // Actors without MF2_SLIDE keep the existing single clip.
                    var lx = wall.X2 - wall.X1;
                    var ly = wall.Y2 - wall.Y1;
                    var length = lx * lx + ly * ly;
                    var scale = length > 0 ? (vx * lx + vy * ly) / length : 0;
                    vx = lx * scale;
                    vy = ly * scale;
                    if (!TryMove(sim, actor, x + vx / steps, y + vy / steps, out _)) vx = vy = 0;
                }
                else
                {
                    if (!TryMove(sim, actor, x + dx, y, out _)) vx = 0;
                    if (!TryMove(sim, actor, actor.X.ToDouble(), y + dy, out _)) vy = 0;
                }
            }
        }
        var floor = SupportFloor(sim, actor);
        var z = actor.Z.ToDouble();
        var vz = actor.VelocityZ.ToDouble();
        if (actor.OnGround && z < floor && floor - z <= actor.MaxStepHeight.ToDouble())
        {
            z = floor;
            if (vz < 0) vz = 0;
        }
        if (!actor.NoGravity && (z > floor || vz != 0)) vz -= Gravity * actor.Gravity.ToDouble()
            * ((uint)actor.SectorIndex < (uint)sim.Level.Sectors.Count ? sim.Level.Sectors[actor.SectorIndex].Gravity : 1);
        z += vz;
        if (z < floor)
        {
            z = floor;
            if (vz < 0) vz = 0;
        }
        actor.Z = Fixed.FromDouble(z);
        actor.VelocityZ = Fixed.FromDouble(vz);
        if (actor.Brain?.Mode == MonsterMode.Chase && (actor.X.ToDouble() != startX || actor.Y.ToDouble() != startY)) actor.InFloat = false;
        FloatTowardTarget(sim, actor);
        FitToSector(sim, actor, carryFloor: false);
        var friction = actor.OnGround ? Math.Clamp(GroundFriction * actor.Friction.ToDouble(), 0, 1) : 1;
        actor.VelocityX = Fixed.FromDouble(Math.Abs(vx * friction) < 0.0625 ? 0 : vx * friction);
        actor.VelocityY = Fixed.FromDouble(Math.Abs(vy * friction) < 0.0625 ? 0 : vy * friction);
        CarryStandingRiders(sim, actor, actor.X.ToDouble() - startX, actor.Y.ToDouble() - startY, actor.Z.ToDouble() - startZ, riders);
        RefreshOnMobj(sim, actor);
    }

    private static void FloatTowardTarget(AuthoritySimulation sim, Actor actor)
    {
        if (!actor.Floating || actor.InFloat || actor.IsDead || actor.Destroyed || actor.Brain is not { Enabled: true, Charging: false } brain
            || !double.IsFinite(actor.FloatSpeed) || actor.FloatSpeed <= 0) return;
        var target = sim.Actors.FirstOrDefault(candidate => candidate.Id == brain.TargetId && candidate.CanTakeDamage);
        if (target == null) return;
        var dx = target.X.ToDouble() - actor.X.ToDouble(); var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var delta = target.Z.ToDouble() + target.Height.ToDouble() / 2 - actor.Z.ToDouble();
        // P_ZMovement compares the target's center to the floater's base, not its center.
        if (delta != 0 && distance < Math.Abs(delta) * 3)
            actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + Math.Sign(delta) * actor.FloatSpeed);
    }

    /// <summary>
    /// Sector floor, or a solid actor's top when someone is standing on it or stepping onto it.
    /// Players use any solid non-player. Monsters use <see cref="Actor.ActsLikeBridge"/> only.
    /// An <see cref="Actor.IceCorpse"/> also uses a corpse top. The top has to be within
    /// <see cref="Actor.MaxStepHeight"/> and leave headroom.
    /// </summary>
    private static double SupportFloor(AuthoritySimulation sim, Actor actor)
    {
        var floor = sim.FloorOf(actor.SectorIndex);
        if (actor.Floating || actor.ThruActors || actor is ProjectileActor) return floor;
        var player = actor is PlayerPawn;
        var x = actor.X.ToDouble();
        var y = actor.Y.ToDouble();
        var z = actor.Z.ToDouble();
        var radius = actor.Radius.ToDouble();
        var ceiling = actor.SectorIndex >= 0 ? sim.CeilingOf(actor.SectorIndex) : double.PositiveInfinity;
        foreach (var other in sim.Actors)
        {
            if (ReferenceEquals(actor, other) || other.ThruActors || !other.IsBlockmapActor) continue;
            var corpse = actor.IceCorpse && IsCorpseObstacle(other);
            if ((!other.BlocksActors && !corpse) || (other is PlayerPawn && !other.IsDead)) continue;
            if (!player && !other.ActsLikeBridge && !corpse) continue;
            var reach = radius + other.Radius.ToDouble();
            var dx = x - other.X.ToDouble();
            var dy = y - other.Y.ToDouble();
            if (dx * dx + dy * dy >= reach * reach) continue;
            var top = other.Z.ToDouble() + other.Height.ToDouble();
            if (top + actor.Height.ToDouble() > ceiling) continue;
            var rise = top - z;
            if (rise > actor.MaxStepHeight.ToDouble()) continue;
            if (top <= z + 1e-4 || actor.OnGround)
                floor = Math.Max(floor, top);
        }
        return floor;
    }

    /// <summary>A dead solid actor. Living movers ignore it. <see cref="Actor.IceCorpse"/> does not.</summary>
    private static bool IsCorpseObstacle(Actor other) =>
        other.IsDead && !other.Destroyed && other.Solid
        && other.DoomEdNum != LineSpecials.TeleportDestType
        && other.DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(other.DoomEdNum);

    /// <summary>Snaps grounded actors to sector and mobj support. Call carriers before riders.</summary>
    public static void FitToSector(AuthoritySimulation sim, Actor actor, bool carryFloor = true, bool sectorPinchCrush = false)
    {
        var floor = SupportFloor(sim, actor);
        var ceiling = actor.SectorIndex < 0 ? double.PositiveInfinity : sim.CeilingOf(actor.SectorIndex);
        var z = actor.Z.ToDouble();
        if (carryFloor && actor.OnGround) z = floor;
        if (z + actor.Height.ToDouble() > ceiling)
        {
            z = ceiling - actor.Height.ToDouble();
            if (actor.VelocityZ.Raw > 0) actor.VelocityZ = default;
        }
        if (z <= floor)
        {
            z = floor;
            if (actor.VelocityZ.Raw < 0) actor.VelocityZ = default;
        }
        actor.Z = Fixed.FromDouble(z);
        actor.OnGround = z <= floor && actor.VelocityZ.Raw <= 0;
        RefreshOnMobj(sim, actor);
        if (sectorPinchCrush && actor.SectorIndex >= 0 && actor.BlocksActors
            && ceiling - floor < actor.Height.ToDouble() - 1e-4
            && actor.MoverCrushTic != sim.Thinkers.Clock.Tic
            && !sim.Motions.Any(m => m.SectorIndex == actor.SectorIndex)
            && (sim.Thinkers.Clock.Tic & 3) == 0)
        {
            var tic = sim.Thinkers.Clock.Tic;
            ActorDamage.Apply(actor, SectorCrushDamage, damageType: "Crush");
            CrushStandingRiders(sim, actor, SectorCrushDamage, tic, "Crush");
        }
    }

    private static void StepCharge(AuthoritySimulation sim, Actor actor)
    {
        var vx = actor.VelocityX.ToDouble(); var vy = actor.VelocityY.ToDouble(); var vz = actor.VelocityZ.ToDouble();
        if (vx == 0 && vy == 0 && !actor.NoAutoOffSkullFly)
        {
            actor.Brain!.StopCharge(actor);
            return;
        }
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Max(Math.Abs(vx), Math.Abs(vy)), Math.Abs(vz)) / 2));
        for (var i = 0; i < steps; i++)
        {
            var previousZ = actor.Z;
            actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + vz / steps);
            var floor = sim.FloorOf(actor.SectorIndex);
            var ceiling = actor.SectorIndex >= 0 ? sim.CeilingOf(actor.SectorIndex) : double.PositiveInfinity;
            if (actor.Z.ToDouble() < floor)
            {
                actor.Z = Fixed.FromDouble(floor);
                vz = 0; actor.VelocityZ = default;
            }
            if (actor.Z.ToDouble() + actor.Height.ToDouble() > ceiling)
            {
                actor.Z = Fixed.FromDouble(ceiling - actor.Height.ToDouble());
                vz = -Math.Abs(vz); actor.VelocityZ = Fixed.FromDouble(vz);
            }
            Actor? victim = null;
            if (actor.Z.ToDouble() < floor
                || !TryMove(sim, actor, actor.X.ToDouble() + vx / steps, actor.Y.ToDouble() + vy / steps, out _, out victim))
            {
                actor.Z = previousZ;
                actor.Brain!.StopCharge(actor);
                if (victim?.CanTakeDamage == true)
                    ActorDamage.Apply(victim, (int)Math.Clamp((long)actor.Damage * (1 + (int)(sim.NextCombatRandom() % 8)), 0, int.MaxValue), actor, inflictor: actor);
                return;
            }
            actor.OnGround = actor.Z.ToDouble() <= sim.FloorOf(actor.SectorIndex);
        }
    }

    internal static bool TryMove(AuthoritySimulation sim, Actor actor, double x, double y, out LevelLine? wall) =>
        TryMove(sim, actor, x, y, out wall, out _);

    /// <summary>PlayerPawn.CrouchMove fit test. The actor keeps its current height.</summary>
    internal static bool FitsAtHeight(AuthoritySimulation sim, Actor actor, double height)
    {
        var saved = actor.Height;
        actor.Height = Fixed.FromDouble(height);
        var fits = TryMove(sim, actor, actor.X.ToDouble(), actor.Y.ToDouble(), out _, out _);
        actor.Height = saved;
        return fits;
    }

    /// <summary>
    /// Two attempts of <c>FSlide::SlideMove</c>. The line reached first along this step is
    /// the slide wall, not the first blocking line in the map. <c>HitSlideLine</c> then
    /// drops the into-wall part of the velocity. A second wall gets one more clip.
    /// The trace is the actor center, not the three bounding-box corners, and ice does not bounce.
    /// </summary>
    private static void SlideFromBlock(
        AuthoritySimulation sim, Actor actor, ref double vx, ref double vy,
        int steps, double x, double y, double dx, double dy)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var radius = Math.Max(0, actor.Radius.ToDouble());
            var line = NearestApproachingLine(sim, actor, x, y, dx, dy, radius);
            if (line == null)
            {
                StairStep(sim, actor, ref vx, ref vy, x, y, dx, dy);
                return;
            }

            var frac = ContactFraction(x, y, dx, dy, line, radius);
            if (frac > 0)
            {
                if (!TryMove(sim, actor, x + dx * frac, y + dy * frac, out _))
                {
                    var fudge = frac - (1.0 / 32);
                    if (fudge > 0)
                        TryMove(sim, actor, x + dx * fudge, y + dy * fudge, out _);
                }
            }

            ClipToLine(line, ref vx, ref vy);
            var remain = Math.Clamp(1 - frac, 0, 1);
            dx = vx / steps * remain;
            dy = vy / steps * remain;
            x = actor.X.ToDouble();
            y = actor.Y.ToDouble();
            if (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9)
                return;
            if (TryMove(sim, actor, x + dx, y + dy, out _))
                return;
        }

        StairStep(sim, actor, ref vx, ref vy, actor.X.ToDouble(), actor.Y.ToDouble(), dx, dy);
    }

    private static void StairStep(
        AuthoritySimulation sim, Actor actor, ref double vx, ref double vy,
        double x, double y, double dx, double dy)
    {
        // FSlide::SlideMove stairstep after the slide retries: Y, then X.
        if (!TryMove(sim, actor, x, y + dy, out _)) vy = 0;
        if (!TryMove(sim, actor, actor.X.ToDouble() + dx, actor.Y.ToDouble(), out _)) vx = 0;
    }

    private static LevelLine? NearestApproachingLine(
        AuthoritySimulation sim, Actor actor, double x, double y, double dx, double dy, double radius)
    {
        LevelLine? best = null;
        var bestFrac = 1.0;
        foreach (var line in sim.Level.Lines)
        {
            if (!Blocks(sim, actor, line)) continue;
            var frac = ContactFraction(x, y, dx, dy, line, radius);
            if (frac < bestFrac)
            {
                bestFrac = frac;
                best = line;
            }
        }
        return best;
    }

    private static double ContactFraction(double x, double y, double dx, double dy, LevelLine line, double radius)
    {
        var limit = radius * radius - 1e-8;
        double At(double t) => DistanceSquared(x + dx * t, y + dy * t, line.X1, line.Y1, line.X2, line.Y2);
        var start = At(0);
        if (start < limit)
            return At(1) < start - 1e-8 ? 0 : 1;
        if (At(1) >= limit)
            return 1;
        var lo = 0.0;
        var hi = 1.0;
        for (var i = 0; i < 16; i++)
        {
            var mid = (lo + hi) / 2;
            if (At(mid) >= limit) lo = mid;
            else hi = mid;
        }
        return hi;
    }

    private static void ClipToLine(LevelLine line, ref double vx, ref double vy)
    {
        var lx = line.X2 - line.X1;
        var ly = line.Y2 - line.Y1;
        if (lx == 0) { vx = 0; return; }
        if (ly == 0) { vy = 0; return; }
        var length = lx * lx + ly * ly;
        if (length <= 0) { vx = vy = 0; return; }
        var scale = (vx * lx + vy * ly) / length;
        vx = lx * scale;
        vy = ly * scale;
    }

    internal static bool CanOccupy(AuthoritySimulation sim, Actor actor)
    {
        var x = actor.X.ToDouble(); var y = actor.Y.ToDouble(); var z = actor.Z.ToDouble();
        var radius = actor.Radius.ToDouble(); var height = actor.Height.ToDouble();
        var sector = SectorAt(sim.Level, x, y);
        if (z < sim.FloorOf(sector) || sector >= 0 && z + height > sim.CeilingOf(sector)) return false;
        if (sim.Level.Lines.Any(line => Blocks(sim, actor, line)
            && DistanceSquared(x, y, line.X1, line.Y1, line.X2, line.Y2) < radius * radius)) return false;
        return actor.ThruActors || !sim.Actors.Any(other => !ReferenceEquals(actor, other) && !other.ThruActors && other.IsBlockmapActor && other.BlocksActors
            && z < other.Z.ToDouble() + other.Height.ToDouble() && z + height > other.Z.ToDouble()
            && Math.Pow(x - other.X.ToDouble(), 2) + Math.Pow(y - other.Y.ToDouble(), 2)
                < Math.Pow(radius + other.Radius.ToDouble(), 2));
    }

    private static bool TryMove(AuthoritySimulation sim, Actor actor, double x, double y, out LevelLine? wall, out Actor? blocker)
    {
        wall = null;
        blocker = null;
        var ox = actor.X.ToDouble();
        var oy = actor.Y.ToDouble();
        var radius = Math.Max(0, actor.Radius.ToDouble());
        foreach (var line in sim.Level.Lines)
        {
            if (!Blocks(sim, actor, line)) continue;
            var before = DistanceSquared(ox, oy, line.X1, line.Y1, line.X2, line.Y2);
            var after = DistanceSquared(x, y, line.X1, line.Y1, line.X2, line.Y2);
            var swept = Math.Min(Math.Min(before, after), Math.Min(
                DistanceSquared(line.X1, line.Y1, ox, oy, x, y),
                DistanceSquared(line.X2, line.Y2, ox, oy, x, y)));
            // Permit actors already touching a wall to move parallel or away from it.
            if ((swept < radius * radius - 1e-8 && (before >= radius * radius || after < before - 1e-8))
                || LineSlide.Crosses(ox, oy, x, y, line))
            {
                wall = line;
                if (actor.Blasted) ActorDamage.Apply(actor, actor.Mass >> 5, damageType: "Melee");
                return false;
            }
        }
        var sector = SectorAt(sim.Level, x, y);
        // P_TryMove refuses floorz - dropoffz > MaxDropOffHeight unless MF_DROPOFF, MF_FLOAT, or MF_MISSILE.
        // Flat maps use the current floor against the destination floor. A drop of exactly the limit is allowed.
        if (sector >= 0 && actor.SectorIndex >= 0 &&
            (actor.NoDropOff || !actor.AllowDropOff && !actor.Floating && actor is not ProjectileActor))
        {
            var floorz = sim.FloorOf(actor.SectorIndex);
            if (actor.OnMobj)
                floorz = Math.Max(actor.Z.ToDouble(), floorz);
            var drop = floorz - sim.FloorOf(sector);
            if (drop > actor.MaxDropOffHeight.ToDouble() && !actor.Blasted)
                return false;
        }
        var z = actor.Brain?.Charging == true ? actor.Z.ToDouble() : Math.Max(actor.Z.ToDouble(), sim.FloorOf(sector));
        if (sector >= 0 && (z < sim.FloorOf(sector) || z - actor.Z.ToDouble() > actor.MaxStepHeight.ToDouble()
            || z + actor.Height.ToDouble() > sim.CeilingOf(sector))) return false;
        if (!actor.ThruActors && (actor.BlocksActors || actor.Blasted && !actor.IsDead && !actor.Destroyed))
        {
            foreach (var other in sim.Actors)
            {
                var corpse = actor.IceCorpse && IsCorpseObstacle(other);
                var blastTarget = actor.Blasted && other.Shootable && other.IsMonster
                    && !other.Boss && !other.DontBlast && !other.IsDead && !other.Destroyed;
                if (ReferenceEquals(actor, other) || other.ThruActors || !other.IsBlockmapActor
                    || (!blastTarget && !(actor.BlocksActors && (other.BlocksActors || corpse)))
                    || z >= other.Z.ToDouble() + other.Height.ToDouble()
                    || z + actor.Height.ToDouble() <= other.Z.ToDouble()) continue;
                var reach = radius + other.Radius.ToDouble();
                var cx = other.X.ToDouble();
                var cy = other.Y.ToDouble();
                var before = (ox - cx) * (ox - cx) + (oy - cy) * (oy - cy);
                var after = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                if (!(actor.Brain?.Charging == true && after < reach * reach - 1e-8
                    || DistanceSquared(cx, cy, ox, oy, x, y) < reach * reach - 1e-8
                    && (before >= reach * reach || after < before - 1e-8)))
                    continue;
                // PIT_CheckThing transfers horizontal momentum before rejecting a blasted collision.
                if (blastTarget)
                {
                    other.VelocityX = Fixed.FromDouble(other.VelocityX.ToDouble() + actor.VelocityX.ToDouble());
                    other.VelocityY = Fixed.FromDouble(other.VelocityY.ToDouble() + actor.VelocityY.ToDouble());
                    if (Math.Abs(other.VelocityX.ToDouble()) + Math.Abs(other.VelocityY.ToDouble()) > 3)
                    {
                        ActorDamage.Apply(other, actor.Mass / 100 + 1, actor, inflictor: actor);
                        ActorDamage.Apply(actor, (other.Mass / 100 + 1) >> 2, other, inflictor: other);
                    }
                    blocker = other; return false;
                }
                // P_TryMove lets a grounded player onto a non-player when the top is a legal step
                // and the head still fits. PIT_CheckThing lets a grounded monster onto MF4_ACTLIKEBRIDGE.
                // P_TestMobjZ lets MF_ICECORPSE land on a corpse.
                var top = other.Z.ToDouble() + other.Height.ToDouble();
                var ceiling = sector >= 0 ? sim.CeilingOf(sector) : double.PositiveInfinity;
                var rise = top - actor.Z.ToDouble();
                var playerStep = actor is PlayerPawn && other is not PlayerPawn && !corpse;
                var bridgeStep = actor is not PlayerPawn && actor is not ProjectileActor && other.ActsLikeBridge;
                if (actor.OnGround && !actor.Floating && (playerStep || bridgeStep || corpse)
                    && rise > 0 && rise <= actor.MaxStepHeight.ToDouble()
                    && top + actor.Height.ToDouble() <= ceiling)
                    continue;
                blocker = other;
                return false;
            }
        }
        actor.X = Fixed.FromDouble(x);
        actor.Y = Fixed.FromDouble(y);
        actor.Z = Fixed.FromDouble(z);
        if (sector != actor.SectorIndex)
        {
            actor.SectorIndex = sector;
            actor.OnGround = z <= sim.FloorOf(sector) && actor.VelocityZ.Raw <= 0;
        }
        return true;
    }

    private static bool Blocks(AuthoritySimulation sim, Actor actor, LevelLine line)
    {
        var projectile = actor is ProjectileActor;
        if (line.OneSided || (line.Flags & LevelLine.BlockEverythingFlag) != 0) return true;
        if (projectile && (line.Flags & LevelLine.BlockProjectileFlag) != 0) return true;
        if (!projectile && (line.Flags & LevelLine.BlockingFlag) != 0) return true;
        if ((line.Flags & LevelLine.BlockMonstersFlag) != 0 && actor is not PlayerPawn && !projectile
            && !actor.NoBlockMonsters) return true;
        if (!projectile && actor.Floating && (line.Flags & LevelLine.BlockFloatersFlag) != 0) return true;
        var front = SideSector(sim.Level, line.SideFront);
        var back = SideSector(sim.Level, line.SideBack);
        if (front < 0 || back < 0) return false;
        var floor = Math.Max(sim.FloorOf(front), sim.FloorOf(back));
        var ceiling = Math.Min(sim.CeilingOf(front), sim.CeilingOf(back));
        var bottom = Math.Max(actor.Z.ToDouble(), floor);
        return floor - actor.Z.ToDouble() > actor.MaxStepHeight.ToDouble()
            || bottom + actor.Height.ToDouble() > ceiling;
    }

    private static double DistanceSquared(double x, double y, double ax, double ay, double bx, double by)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var length = dx * dx + dy * dy;
        var t = length == 0 ? 0 : Math.Clamp(((x - ax) * dx + (y - ay) * dy) / length, 0, 1);
        dx = x - ax - t * dx;
        dy = y - ay - t * dy;
        return dx * dx + dy * dy;
    }
}
