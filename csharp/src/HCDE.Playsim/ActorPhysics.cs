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
/// Falling players use the supported <c>P_CheckOnmobj</c> prediction and square actor-support bounds.
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
    internal static double TerrainMovementScale(AuthoritySimulation sim, Actor actor)
        => TerrainMovementFactors(sim, actor).Factor / (2048 / 65536.0);

    internal static (double Friction, double Factor) TerrainMovementFactors(AuthoritySimulation sim, Actor actor)
    {
        var flying = actor.Fly && actor.NoGravity && actor is not PlayerPawn { NoClip2: true };
        var terrain = !actor.NoGravity && !actor.NoClip && actor.Z.ToDouble() <= sim.FloorOf(actor.SectorIndex)
            ? sim.Level.FloorTerrainDefinition(actor.SectorIndex) : null;
        var friction = terrain is { Friction: > 0 } ? terrain.Friction : GroundFriction;
        var factor = terrain is { Friction: > 0 } ? terrain.MoveFactor : 2048 / 65536.0;
        if (flying) { friction = FlyingFriction; factor = 2048 / 65536.0; }
        if (actor.Friction.Raw != 65536)
        {
            friction = Math.Clamp(friction * actor.Friction.ToDouble(), 0, 1);
            factor = FrictionToMoveFactor(friction);
        }
        if (friction < GroundFriction)
        {
            var x = actor.VelocityX.ToDouble(); var y = actor.VelocityY.ToDouble();
            var speed = Math.Sqrt(x * x + y * y);
            const double threshold = 15000 / 65536.0;
            factor *= speed > threshold * 4 ? 8 : speed > threshold * 2 ? 4 : speed > threshold ? 2 : 1;
        }
        return (friction, factor);
    }

    /// <summary>Native P_GetFriction conversion after an actor friction multiplier.</summary>
    internal static double FrictionToMoveFactor(double friction)
    {
        var raw = friction * 65536;
        var factor = friction >= GroundFriction
            ? ((0x10092 - raw) * 1024) / 4352 + 568
            : ((raw - 0xDB34) * 10) / 0x80;
        return Math.Max(32, factor) / 65536;
    }

    public const double GroundFriction = 0xE800 / 65536.0;
    public const double FlyingFriction = 0xEB00 / 65536.0;
    public const double Gravity = 1;
    public const double MaxMove = 30;
    /// <summary>Native default sector pinch crush when <c>crushchange</c> is 10.</summary>
    public const int SectorCrushDamage = 10;

    /// <summary>Native rider crush subset. The rider stands on the carrier's top within both radii.</summary>
    public static bool IsStandingOn(Actor carrier, Actor rider)
    {
        if (!CanCarryRider(carrier, rider)) return false;
        var top = carrier.Z.ToDouble() + carrier.Height.ToDouble();
        if (Math.Abs(rider.Z.ToDouble() - top) > 1) return false;
        var reach = carrier.Radius.ToDouble() + rider.Radius.ToDouble();
        var dx = rider.X.ToDouble() - carrier.X.ToDouble();
        var dy = rider.Y.ToDouble() - carrier.Y.ToDouble();
        return Math.Abs(dx) < reach && Math.Abs(dy) < reach;
    }

    private static bool CanCarryRider(Actor carrier, Actor rider) =>
        !ReferenceEquals(carrier, rider) && carrier.Solid && !carrier.Destroyed && rider.IsBlockmapActor
        && !carrier.SpecialPickup && !carrier.ThruActors && !rider.ThruActors
        && (!rider.SpecialPickup || carrier.ActsLikeBridge)
        && (!carrier.Corpse || rider.IceCorpse)
        && (!rider.Corpse || carrier.IceCorpse)
        && (carrier.CanPass || rider.CanPass || carrier.IsMonster || rider.IsMonster)
        && !rider.SharesEnabledThruBits(carrier)
        && !(rider.ThruSpecies && rider.SharesContactSpecies(carrier))
        && (rider.BlocksActors || IsSolidNonCorpseObstacle(rider)
            || carrier.IceCorpse && IsCorpseObstacle(rider));

    /// <summary>Applies the same crush damage to every actor standing on <paramref name="carrier"/>.</summary>
    public static void CrushStandingRiders(AuthoritySimulation sim, Actor carrier, int damage, int tic, string? damageType = null)
    {
        if (damage <= 0 || carrier.NoClip) return;
        var crushDamage = string.Equals(damageType, "Crush", StringComparison.OrdinalIgnoreCase);
        foreach (var rider in sim.Actors)
        {
            if (!IsStandingOn(carrier, rider)) continue;
            if (crushDamage && !rider.Shootable) continue;
            rider.MarkMoverCrush(tic);
            ActorDamage.Apply(rider, damage, damageType: damageType);
            if (crushDamage) ActorSoundActions.CrushPain(rider);
        }
    }

    /// <summary>Moves riders that were on <paramref name="carrier"/> before it stepped this tic.</summary>
    public static void CarryStandingRiders(AuthoritySimulation sim, Actor carrier, double dx, double dy, double dz, IReadOnlyList<Actor> riders)
    {
        if (!carrier.Solid || carrier.Destroyed || carrier.NoClip || carrier.SpecialPickup || riders.Count == 0
            || (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9 && Math.Abs(dz) < 1e-9))
            return;
        foreach (var rider in riders)
        {
            if (!CanCarryRider(carrier, rider)) continue;
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
        if (actor.Destroyed || actor.NoSector || actor.NoClip || actor.Floating || !actor.OnGround || actor is ProjectileActor)
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
        if (actor is ProjectileActor)
        {
            actor.OnMobj = false;
            return;
        }
        var floor = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble()).Floor;
        var support = SupportFloor(sim, actor);
        actor.OnMobj = support > floor + 1e-4;
        if (!actor.OnMobj && (actor.CanPass || actor.SpecialPickup)
            && !sim.Compat.HasFlag(CompatSurface.NoPassMobj)
            && actor.VelocityZ.Raw == 0 && actor.Z.ToDouble() != floor)
        {
            var blocker = CheckOnMobj(sim, actor);
            actor.OnMobj = blocker is not null && (actor is not PlayerPawn || blocker is not PlayerPawn)
                && (blocker.Z.ToDouble() + blocker.Height.ToDouble() - actor.Z.ToDouble() > actor.MaxStepHeight.ToDouble()
                    || blocker.Z.ToDouble() + blocker.Height.ToDouble() + actor.Height.ToDouble()
                        > SectorSupportCeiling(sim, actor));
        }
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
        if (actor.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(sim, actor);
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
        if (actor.Brain?.Charging == true)
        {
            StepCharge(sim, actor);
            if (!actor.Destroyed && actor.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(sim, actor);
            return;
        }
        var startX = actor.X.ToDouble();
        var startY = actor.Y.ToDouble();
        var startZ = actor.Z.ToDouble();
        var oldFloor = FlatOpening(sim, actor, actor.SectorIndex, startX, startY).Floor;
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
        var noClip2 = actor is PlayerPawn { NoClip2: true };
        var floor = SupportFloor(sim, actor);
        var z = actor.Z.ToDouble();
        var vz = actor.VelocityZ.ToDouble();
        var airbornePlayer = actor is PlayerPawn && !noClip2 && z > floor && !actor.OnMobj && !(actor.Fly && actor.NoGravity);
        var hitTerrainFloor = !actor.OnGround && floor == sim.FloorOf(actor.SectorIndex)
            && z > floor && vz < 0 && z + vz <= floor;
        if (actor.OnGround && z < floor && floor - z <= actor.MaxStepHeight.ToDouble())
        {
            z = floor;
            if (vz < 0) vz = 0;
        }
        var support = (actor.CanPass || actor.SpecialPickup)
            && !sim.Compat.HasFlag(CompatSurface.NoPassMobj)
            && actor is not ProjectileActor && (vz != 0 || z != sim.FloorOf(actor.SectorIndex))
            ? CheckOnMobj(sim, actor) : null;
        var blockedOnActor = support is not null && (actor is not PlayerPawn || support is not PlayerPawn);
        var landedOnActor = blockedOnActor && support is not null
            && support.Z.ToDouble() + support.Height.ToDouble() - z <= actor.MaxStepHeight.ToDouble();
        if (landedOnActor && support is not null)
        {
            // The native on-mobj branch lands before ordinary Z movement applies gravity.
            z = support.Z.ToDouble() + support.Height.ToDouble();
        }
        // Native stops vertical motion on a blocker even when its top exceeds the legal step.
        if (blockedOnActor) vz = 0;
        z += vz;
        // P_ZMovement advances with the current velocity before FallAndSink changes it.
        if (!blockedOnActor && !actor.NoGravity && z > floor)
        {
            var gravity = Gravity * (sim.LevelGravity / 800) * actor.Gravity.ToDouble()
                * ((uint)actor.SectorIndex < (uint)sim.Level.Sectors.Count ? sim.Level.Sectors[actor.SectorIndex].Gravity : 1);
            // FallAndSink doubles only the first acceleration from rest off the previous floor.
            vz -= vz == 0 && oldFloor > floor && z == oldFloor ? gravity + gravity : gravity;
        }
        actor.Z = Fixed.FromDouble(z);
        actor.VelocityZ = Fixed.FromDouble(vz);
        if (hitTerrainFloor && !blockedOnActor) TerrainDamage.Land(sim, actor);
        if (actor.Brain?.Mode == MonsterMode.Chase && (actor.X.ToDouble() != startX || actor.Y.ToDouble() != startY)) actor.InFloat = false;
        if (!blockedOnActor) FloatTowardTarget(sim, actor);
        if (!blockedOnActor && !actor.NoFriction && actor is PlayerPawn && actor.NoGravity && actor.Z.ToDouble() > floor)
            actor.VelocityZ = Fixed.FromDouble(actor.VelocityZ.ToDouble() * FlyingFriction);
        FitToSector(sim, actor, carryFloor: false);
        if (noClip2) actor.OnGround = true;
        var groundFriction = GroundFriction;
        if (!actor.NoGravity && !actor.NoClip && actor.Z.ToDouble() <= sim.FloorOf(actor.SectorIndex)
            && sim.Level.FloorTerrainDefinition(actor.SectorIndex) is { Friction: > 0 } terrain)
            groundFriction = terrain.Friction;
        var friction = actor.Fly && actor.NoGravity && !noClip2 ? Math.Clamp(FlyingFriction * actor.Friction.ToDouble(), 0, 1)
            : actor.OnGround ? Math.Clamp(groundFriction * actor.Friction.ToDouble(), 0, 1) : 1;
        if (airbornePlayer)
            friction = sim.AirControl is > (1.0 / 256) and { } control ? control * -0.0941 + 1.0004 : 1;
        var ledgeSliding = false;
        if ((actor.Corpse || actor.Falling) && (Math.Abs(vx) > 0.25 || Math.Abs(vy) > 0.25))
        {
            var opening = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble());
            ledgeSliding = opening.Floor > sim.FloorOf(actor.SectorIndex) && opening.DropOff != opening.Floor;
        }
        var skipFriction = actor is ProjectileActor || actor.NoFriction || ledgeSliding;
        var appliesFriction = actor.OnGround || actor.Fly && actor.NoGravity;
        var hasMovementInput = actor is PlayerPawn player && player.HasMovementInput;
        var stop = !airbornePlayer && !skipFriction && appliesFriction && !hasMovementInput
            && Math.Abs(vx) < 0.0625 && Math.Abs(vy) < 0.0625;
        actor.VelocityX = Fixed.FromDouble(stop ? 0 : skipFriction ? vx : vx * friction);
        actor.VelocityY = Fixed.FromDouble(stop ? 0 : skipFriction ? vy : vy * friction);
        CarryStandingRiders(sim, actor, actor.X.ToDouble() - startX, actor.Y.ToDouble() - startY, actor.Z.ToDouble() - startZ, riders);
        RefreshOnMobj(sim, actor);
    }

    private static void FloatTowardTarget(AuthoritySimulation sim, Actor actor)
    {
        if (actor.Dormant || actor.Destroyed) return;
        var z = actor.Z.ToDouble();
        actor.Z = Fixed.FromDouble(z + FloatingTargetAdjustment(sim, actor, z));
    }

    private static double FloatingTargetAdjustment(AuthoritySimulation sim, Actor actor, double z)
    {
        if (!actor.Floating || actor.InFloat || actor.Brain?.Charging == true
            || AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target) is not { } target) return 0;
        var dx = target.X.ToDouble() - actor.X.ToDouble(); var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var delta = target.Z.ToDouble() + target.Height.ToDouble() / 2 - z;
        // P_ZMovement compares the target's center to the floater's base, not its center.
        if (delta == 0 || distance >= Math.Abs(delta) * 3) return 0;
        if (!double.IsFinite(actor.FloatSpeed)) throw new InvalidOperationException("Float speed must be finite.");
        return Math.Sign(delta) * actor.FloatSpeed;
    }

    /// <summary>
    /// Sector floor, or a solid actor's top when someone is standing on it or stepping onto it.
    /// Players step onto solid non-players; monsters step onto bridges. CANPASS actors
    /// can also remain standing on ordinary actors after landing from above.
    /// An <see cref="Actor.IceCorpse"/> also uses a corpse top. The top has to be within
    /// <see cref="Actor.MaxStepHeight"/> and leave headroom.
    /// </summary>
    private static double SupportFloor(AuthoritySimulation sim, Actor actor)
    {
        var opening = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble());
        var floor = opening.Floor;
        if (sim.Compat.HasFlag(CompatSurface.NoPassMobj)
            || actor.ThruActors || actor is ProjectileActor) return floor;
        var player = actor is PlayerPawn;
        var x = actor.X.ToDouble();
        var y = actor.Y.ToDouble();
        var z = actor.Z.ToDouble();
        var radius = actor.Radius.ToDouble();
        var ceiling = opening.Ceiling;
        foreach (var other in sim.Actors)
        {
            if (ReferenceEquals(actor, other) || other.ThruActors
                || actor.SharesEnabledThruBits(other)
                || actor.ThruSpecies && actor.SharesContactSpecies(other) || !other.IsBlockmapActor) continue;
            if (other.SpecialPickup) continue;
            if (other.Corpse && !actor.IceCorpse) continue;
            if (!(actor.CanPass || actor.SpecialPickup)
                && !(actor is not PlayerPawn && actor.IsMonster && !actor.Floating && !actor.NoGravity
                    && actor.Brain?.Charging != true && other.ActsLikeBridge)) continue;
            if (actor.SpecialPickup && !other.ActsLikeBridge) continue;
            var corpse = actor.IceCorpse && IsCorpseObstacle(other);
            if ((!other.BlocksActors && !corpse && !IsSolidNonCorpseObstacle(other))
                || (player && other is PlayerPawn && !other.IsDead)) continue;
            var top = other.Z.ToDouble() + other.Height.ToDouble();
            var standing = top <= z + 1e-4;
            if (actor.Floating && !standing) continue;
            if (actor.Brain?.Charging == true && !standing) continue;
            if (!player && actor.NoGravity && !corpse && !standing) continue;
            if (!player && !actor.IsMonster && !corpse && !standing) continue;
            if (!player && !other.ActsLikeBridge && !corpse && !standing) continue;
            var reach = radius + other.Radius.ToDouble();
            var dx = x - other.X.ToDouble();
            var dy = y - other.Y.ToDouble();
            if (Math.Abs(dx) >= reach || Math.Abs(dy) >= reach) continue;
            if (top + actor.Height.ToDouble() > ceiling) continue;
            var rise = top - z;
            if (rise > actor.MaxStepHeight.ToDouble()) continue;
            if (top <= z + 1e-4 || actor.OnGround)
                floor = Math.Max(floor, top);
        }
        return floor;
    }

    /// <summary>A flagged solid corpse. Living movers ignore it. <see cref="Actor.IceCorpse"/> does not.</summary>
    private static bool IsCorpseObstacle(Actor other) =>
        other.Corpse && !other.Destroyed && other.Solid
        && other.DoomEdNum != LineSpecials.TeleportDestType
        && other.DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(other.DoomEdNum, other.Simulation?.RavenHealthPickups ?? false);

    private static bool IsSolidNonCorpseObstacle(Actor actor) =>
        actor.Solid && !actor.Destroyed && !actor.Corpse
        && actor.DoomEdNum != LineSpecials.TeleportDestType
        && actor.DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(actor.DoomEdNum, actor.Simulation?.RavenHealthPickups ?? false);

    internal static bool BlocksSectorGeometry(Actor actor) =>
        (actor.ActsLikeBridge || actor.Shootable) && (actor.BlocksActors || IsSolidNonCorpseObstacle(actor));

    internal static double SectorSupportFloor(AuthoritySimulation sim, Actor actor) =>
        FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble()).Floor;

    internal static double SectorSupportCeiling(AuthoritySimulation sim, Actor actor) =>
        FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble()).Ceiling;

    internal static bool TryDropOffEscapeVector(AuthoritySimulation sim, Actor actor, out double dx, out double dy)
    {
        dx = dy = 0;
        var z = actor.Z.ToDouble(); var maximum = actor.MaxDropOffHeight.ToDouble();
        var opening = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble());
        if (sim.Compat.HasFlag(CompatSurface.DropOff) || actor.AllowDropOff || actor.Floating || actor.OnMobj || z > opening.Floor
            || opening.Floor - opening.DropOff <= maximum) return false;
        foreach (var line in sim.Level.Lines)
        {
            if (line.OneSided || !BoxCrossesLine(actor.X.ToDouble(), actor.Y.ToDouble(), actor.Radius.ToDouble(), line)) continue;
            var front = SideSector(sim.Level, line.SideFront); var back = SideSector(sim.Level, line.SideBack);
            if (front < 0 || back < 0) continue;
            var frontFloor = sim.FloorOf(front); var backFloor = sim.FloorOf(back);
            var sign = backFloor == z && frontFloor < z - maximum ? 1
                : frontFloor == z && backFloor < z - maximum ? -1 : 0;
            if (sign == 0) continue;
            var lx = line.X2 - line.X1; var ly = line.Y2 - line.Y1;
            var length = Math.Sqrt(lx * lx + ly * ly);
            if (length == 0) continue;
            dx -= sign * 32 * ly / length;
            dy += sign * 32 * lx / length;
        }
        return dx != 0 || dy != 0;
    }

    internal static bool TouchesSector(AuthoritySimulation sim, Actor actor, int sector)
    {
        if (actor.SectorIndex == sector) return true;
        foreach (var line in sim.Level.Lines)
        {
            if (line.OneSided || line.X1 == line.X2 && line.Y1 == line.Y2
                || SideSector(sim.Level, line.SideFront) != sector
                && SideSector(sim.Level, line.SideBack) != sector) continue;
            if (BoxCrossesLine(actor.X.ToDouble(), actor.Y.ToDouble(),
                Math.Max(0, actor.Radius.ToDouble()), line)) return true;
        }
        return false;
    }

    internal static bool BlocksLoweringCeiling(AuthoritySimulation sim, Actor actor, double ceiling) =>
        actor.Z.ToDouble() + actor.Height.ToDouble() > ceiling
        && (actor.ActsLikeBridge || SectorSupportFloor(sim, actor) + actor.Height.ToDouble() > ceiling);

    /// <summary>Native PIT_CeilingRaise recovery, including the modeled PASSMOBJ overlap subset.</summary>
    internal static void ApplyCeilingRaise(AuthoritySimulation sim, int sector, double moveAmount)
    {
        foreach (var actor in sim.Actors)
        {
            if (!actor.IsSectorMoverActor || actor.ActsLikeBridge
                || actor is ProjectileActor || !TouchesSector(sim, actor, sector)) continue;
            var opening = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble());
            var z = actor.Z.ToDouble(); var height = actor.Height.ToDouble();
            if (z < opening.Floor && z + height >= opening.Ceiling - moveAmount && !actor.NoLiftDrop)
                actor.Z = Fixed.FromDouble(Math.Min(opening.Floor, opening.Ceiling - height));
            else if (actor.CanPass && (actor.BlocksActors || IsSolidNonCorpseObstacle(actor))
                && z + height < opening.Ceiling)
            {
                var support = FindZBlocker(sim, actor, quick: false);
                if (support is null || support.Z.ToDouble() > z
                    || support.Z.ToDouble() + support.Height.ToDouble() <= z) continue;
                actor.Z = Fixed.FromDouble(Math.Min(opening.Ceiling - height,
                    support.Z.ToDouble() + support.Height.ToDouble()));
            }
            else continue;
            actor.OnGround = actor.Z.ToDouble() <= opening.Floor && actor.VelocityZ.Raw <= 0;
            RefreshOnMobj(sim, actor);
        }
    }

    /// <summary>Native PIT_CeilingLower subset without recursive actor-stack pushing.</summary>
    internal static void ApplyCeilingLower(AuthoritySimulation sim, int sector)
    {
        foreach (var actor in sim.Actors)
        {
            if (!actor.IsSectorMoverActor || actor.ActsLikeBridge || actor is ProjectileActor
                || !TouchesSector(sim, actor, sector)) continue;
            var opening = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble());
            if (actor.Z.ToDouble() + actor.Height.ToDouble() <= opening.Ceiling) continue;
            actor.Z = Fixed.FromDouble(Math.Max(opening.Floor, opening.Ceiling - actor.Height.ToDouble()));
            actor.OnGround = actor.Z.ToDouble() <= opening.Floor && actor.VelocityZ.Raw <= 0;
            RefreshOnMobj(sim, actor);
        }
    }

    internal static double RaisedFloorActorZ(Actor actor, double oldFloor, double floor)
    {
        var z = actor.Z.ToDouble();
        return z > floor && actor.NoGravity && actor.RelativeToFloor
            ? z + floor - oldFloor : Math.Max(z, floor);
    }

    /// <summary>Native PIT_FloorRaise relative-height branch; intersecting actors use the normal fit.</summary>
    internal static void ApplyRelativeFloorRaise(AuthoritySimulation sim, Actor actor, double oldFloor)
    {
        var floor = SectorSupportFloor(sim, actor);
        if (floor == oldFloor || !actor.IsSectorMoverActor || !actor.NoGravity
            || !actor.RelativeToFloor || actor.Z.ToDouble() <= floor) return;
        actor.Z = Fixed.FromDouble(RaisedFloorActorZ(actor, oldFloor, floor));
        actor.OnGround = false;
        RefreshOnMobj(sim, actor);
    }

    /// <summary>Native PIT_FloorDrop actor carry rules for flat sectors.</summary>
    internal static void ApplyFloorDrop(AuthoritySimulation sim, Actor actor, double oldFloor, double moveAmount,
        bool dropActors)
    {
        var floor = SectorSupportFloor(sim, actor);
        if (floor == oldFloor || actor.ActsLikeBridge) return;
        var z = actor.Z.ToDouble();
        if (actor.IsSectorMoverActor)
        {
            if (actor.VelocityZ.Raw == 0 && (!actor.NoGravity || z == oldFloor && !actor.NoLiftDrop))
            {
                if (actor.NoGravity || actor.MoveWithSector || (dropActors || moveAmount < 9) && z - floor <= moveAmount)
                    actor.Z = Fixed.FromDouble(floor);
            }
            else if (z != oldFloor && !actor.NoLiftDrop && actor.NoGravity && actor.RelativeToFloor)
                actor.Z = Fixed.FromDouble(z + floor - oldFloor);
        }
        actor.OnGround = actor.Z.ToDouble() <= SupportFloor(sim, actor) && actor.VelocityZ.Raw <= 0;
        RefreshOnMobj(sim, actor);
    }

    /// <summary>Snaps grounded actors to sector and mobj support. Call carriers before riders.</summary>
    public static void FitToSector(AuthoritySimulation sim, Actor actor, bool carryFloor = true, bool sectorPinchCrush = false)
    {
        var floor = SupportFloor(sim, actor);
        var ceiling = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble()).Ceiling;
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
        if (actor.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(sim, actor);
        if (sectorPinchCrush && actor.IsSectorMoverActor && actor.SectorIndex >= 0 && BlocksSectorGeometry(actor)
            && ceiling - floor < actor.Height.ToDouble() - 1e-4
            && actor.MoverCrushTic != sim.Thinkers.Clock.Tic
            && !sim.Motions.Any(m => m.SectorIndex == actor.SectorIndex)
            && (sim.Thinkers.Clock.Tic & 3) == 0)
        {
            var tic = sim.Thinkers.Clock.Tic;
            ActorDamage.Apply(actor, SectorCrushDamage, damageType: "Crush");
            ActorSoundActions.CrushPain(actor);
            CrushStandingRiders(sim, actor, SectorCrushDamage, tic, "Crush");
        }
    }

    private static void StepCharge(AuthoritySimulation sim, Actor actor)
    {
        var vx = actor.VelocityX.ToDouble(); var vy = actor.VelocityY.ToDouble(); var vz = actor.VelocityZ.ToDouble();
        if (vx == 0 && vy == 0 && !actor.NoAutoOffSkullFly)
        {
            ActorSkullActions.StopStationaryCharge(actor);
            return;
        }
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Max(Math.Abs(vx), Math.Abs(vy)), Math.Abs(vz)) / 2));
        for (var i = 0; i < steps; i++)
        {
            var previousZ = actor.Z;
            actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + vz / steps);
            var floor = sim.FloorOf(actor.SectorIndex);
            var ceiling = actor.SectorIndex >= 0 ? sim.CeilingOf(actor.SectorIndex) : double.PositiveInfinity;
            if (actor.Z.ToDouble() <= floor)
            {
                actor.Z = Fixed.FromDouble(floor);
                vz = vz < 0 ? 0 : -vz; actor.VelocityZ = Fixed.FromDouble(vz);
            }
            if (actor.Z.ToDouble() + actor.Height.ToDouble() > ceiling)
            {
                actor.Z = Fixed.FromDouble(ceiling - actor.Height.ToDouble());
                vz = -vz;
                if (vz > 0) vz = 0;
                actor.VelocityZ = Fixed.FromDouble(vz);
            }
            Actor? victim = null;
            if (actor.Z.ToDouble() < floor
                || !TryMove(sim, actor, actor.X.ToDouble() + vx / steps, actor.Y.ToDouble() + vy / steps, out _, out victim, invokeSlam: true))
            {
                actor.Z = previousZ;
                if (victim == null) actor.Brain!.StopCharge(actor);
                return;
            }
            actor.OnGround = actor.Z.ToDouble() <= sim.FloorOf(actor.SectorIndex);
        }
    }

    internal static bool TryMove(AuthoritySimulation sim, Actor actor, double x, double y, out LevelLine? wall) =>
        TryMove(sim, actor, x, y, out wall, out _);

    /// <summary>Supported touched-sector ceiling for native player stand-up gating.</summary>
    internal static double CeilingAtActor(AuthoritySimulation sim, Actor actor)
        => FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble()).Ceiling;

    /// <summary>PlayerPawn.CrouchMove fit test. The actor keeps its current height.</summary>
    internal static bool FitsAtHeight(AuthoritySimulation sim, Actor actor, double height)
    {
        if (!double.IsFinite(height) || height < 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Fit height must be finite and nonnegative.");
        var saved = actor.Height;
        actor.Height = Fixed.FromDouble(height);
        try { return TryMove(sim, actor, actor.X.ToDouble(), actor.Y.ToDouble(), out _, out _); }
        finally { actor.Height = saved; }
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
            var fudge = frac - (1.0 / 32);
            if (fudge > 0)
            {
                if (!TryMove(sim, actor, x + dx * fudge, y + dy * fudge, out _))
                {
                    StairStep(sim, actor, ref vx, ref vy, x, y, dx, dy);
                    return;
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
            if (!SlideLineBlocks(sim, actor, line)) continue;
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
        if (actor.NoClip) return actor.Brain?.Charging != true || !HasPositionBlocker(sim, actor);
        var x = actor.X.ToDouble(); var y = actor.Y.ToDouble(); var z = actor.Z.ToDouble();
        var radius = actor.Radius.ToDouble(); var height = actor.Height.ToDouble();
        var sector = SectorAt(sim.Level, x, y);
        if (z < sim.FloorOf(sector) || sector >= 0 && z + height > sim.CeilingOf(sector)) return false;
        return !HasPositionBlocker(sim, actor);
    }

    internal static bool HasPositionBlocker(AuthoritySimulation sim, Actor actor)
        => FindPositionBlocker(sim, actor, out var line) is not null || line;

    internal static bool FlatMoveProbeFits(AuthoritySimulation sim, Actor actor)
    {
        if (actor.NoClip) return true;
        var sector = SectorAt(sim.Level, actor.X.ToDouble(), actor.Y.ToDouble());
        var (floor, ceiling, dropOff) = FlatOpening(sim, actor, sector, actor.X.ToDouble(), actor.Y.ToDouble());
        var height = actor.Height.ToDouble(); var z = actor.Z.ToDouble();
        if (actor.Fly && actor.NoGravity && z + height > ceiling) return false;
        if (actor.FloorHugger) z = floor;
        else if (actor.CeilingHugger) z = ceiling - height;
        if (ceiling - floor < height || ceiling - z < height) return false;
        if (!actor.FloorHugger || actor.NoDropOff)
        {
            if (floor - z > actor.MaxStepHeight.ToDouble()) return false;
            if (actor is ProjectileActor && !actor.FloorHugger && floor > z) return false;
            if (z < floor)
            {
                var oldZ = actor.Z;
                try
                {
                    actor.Z = Fixed.FromDouble(floor);
                    if (FindZBlocker(sim, actor) is not null) return false;
                }
                finally { actor.Z = oldZ; }
            }
            if (z >= floor && !actor.Floating && !actor.AllowDropOff
                && z - dropOff > actor.MaxDropOffHeight.ToDouble()) return false;
        }
        return true;
    }

    /// <summary>Flat-sector P_CheckOnmobj / P_FakeZMovement probe, without native collision callbacks.</summary>
    public static Actor? CheckOnMobj(AuthoritySimulation sim, Actor actor)
    {
        var oldZ = actor.Z;
        var z = oldZ.ToDouble() + actor.VelocityZ.ToDouble();
        z += FloatingTargetAdjustment(sim, actor, z);
        var opening = FlatOpening(sim, actor, actor.SectorIndex, actor.X.ToDouble(), actor.Y.ToDouble());
        var floor = opening.Floor;
        if (actor is PlayerPawn && actor.NoGravity && z > floor)
            z += Math.Sin(4.5 * sim.Thinkers.Clock.Tic * Math.PI / 180);
        if (z <= floor) z = floor;
        if (actor.SectorIndex >= 0 && z + actor.Height.ToDouble() > opening.Ceiling)
            z = opening.Ceiling - actor.Height.ToDouble();
        var probeZ = Fixed.FromDouble(z);
        try
        {
            actor.Z = probeZ;
            return FindZBlocker(sim, actor, quick: false);
        }
        finally { actor.Z = oldZ; }
    }

    /// <summary>Supported P_TestMobjZ rules; a full scan selects the highest overlapping blocker.</summary>
    public static Actor? FindZBlocker(AuthoritySimulation sim, Actor actor, bool quick = true)
    {
        if (actor.ThruActors || actor.NoClip) return null;
        Actor? blocker = null;
        var x = actor.X.ToDouble(); var y = actor.Y.ToDouble(); var z = actor.Z.ToDouble();
        foreach (var other in sim.Actors)
        {
            if (other == actor || other.Destroyed || !other.IsBlockmapActor || !other.Solid || other.ThruActors
                || other.SpecialPickup || actor.SharesEnabledThruBits(other)
                || actor.ThruSpecies && actor.SharesContactSpecies(other)
                || other.Corpse && !actor.IceCorpse || actor.SpecialPickup && !other.ActsLikeBridge) continue;
            var reach = actor.Radius.ToDouble() + other.Radius.ToDouble();
            if (Math.Abs(x - other.X.ToDouble()) >= reach || Math.Abs(y - other.Y.ToDouble()) >= reach) continue;
            if (z > other.Z.ToDouble() + other.Height.ToDouble() || z + actor.Height.ToDouble() <= other.Z.ToDouble()) continue;
            if (actor is ProjectileActor missile)
            {
                if (other.NonShootable || missile.ThruGhost && other.Ghost
                    || other.Spectral && !missile.Spectral
                    || missile.MThruSpecies && missile.Owner?.SharesContactSpecies(other) == true
                    || !missile.HitOwner && ReferenceEquals(missile.Owner, other)) continue;
                var passHeight = other.ProjectilePassHeight.ToDouble();
                var clipHeight = passHeight > 0 ? passHeight
                    : passHeight < 0 && sim.Compat.HasFlag(CompatSurface.MissileClip)
                        ? -passHeight : other.Height.ToDouble();
                if (z > other.Z.ToDouble() + clipHeight || missile.Rip && other.CanBeRippedBy(missile)) continue;
            }
            if (blocker is not null && other.Z.ToDouble() + other.Height.ToDouble()
                < blocker.Z.ToDouble() + blocker.Height.ToDouble()) continue;
            blocker = other;
            if (quick) break;
        }
        return blocker;
    }

    internal static Actor? FindPositionBlocker(AuthoritySimulation sim, Actor actor, out bool line, bool squareBounds = false)
    {
        if (actor.NoClip && actor.Brain?.Charging != true) { line = false; return null; }
        var x = actor.X.ToDouble(); var y = actor.Y.ToDouble(); var z = actor.Z.ToDouble();
        var radius = actor.Radius.ToDouble(); var height = actor.Height.ToDouble();
        line = false;
        var probeActors = !squareBounds || actor.Solid || actor is ProjectileActor
            || actor.Brain?.Charging == true || actor.Blasted;
        var blocker = actor.ThruActors || !probeActors ? null : sim.Actors.FirstOrDefault(other => !ReferenceEquals(actor, other) && !other.ThruActors && other.IsBlockmapActor
            && !actor.SharesEnabledThruBits(other)
            && !(actor.ThruSpecies && actor.SharesContactSpecies(other))
            && (squareBounds && actor is ProjectileActor missile
                ? MissileBlocksPosition(sim, missile, other)
                : (other.BlocksActors || IsSolidNonCorpseObstacle(other)) && !ClearsActorHeight(sim, actor, other, z, stationary: true))
            && (squareBounds
                ? Math.Abs(x - other.X.ToDouble()) < radius + other.Radius.ToDouble()
                    && Math.Abs(y - other.Y.ToDouble()) < radius + other.Radius.ToDouble()
                : Math.Pow(x - other.X.ToDouble(), 2) + Math.Pow(y - other.Y.ToDouble(), 2)
                    < Math.Pow(radius + other.Radius.ToDouble(), 2)));
        if (blocker is not null) return blocker;
        if (actor.NoClip) return null;
        line = sim.Level.Lines.Any(wall => Blocks(sim, actor, wall)
            && (squareBounds ? BoxCrossesLine(x, y, radius, wall)
                : DistanceSquared(x, y, wall.X1, wall.Y1, wall.X2, wall.Y2) < radius * radius));
        return null;
    }

    private static bool BoxCrossesLine(double x, double y, double radius, LevelLine line)
    {
        var left = x - radius; var right = x + radius; var bottom = y - radius; var top = y + radius;
        if (left >= Math.Max(line.X1, line.X2) || right <= Math.Min(line.X1, line.X2)
            || top <= Math.Min(line.Y1, line.Y2) || bottom >= Math.Max(line.Y1, line.Y2)) return false;
        var dx = line.X2 - line.X1; var dy = line.Y2 - line.Y1;
        if (dx == 0) return (right < line.X1) != (left < line.X1);
        if (dy == 0) return (top > line.Y1) != (bottom > line.Y1);
        bool Side(double px, double py) => (py - line.Y1) * dx + (line.X1 - px) * dy > 1.0 / 65536;
        return dx * dy >= 0 ? Side(left, top) != Side(right, bottom)
            : Side(right, top) != Side(left, bottom);
    }

    private static bool CanPassActorHeight(AuthoritySimulation sim, Actor actor, Actor other) =>
        (actor.CanPass || other.ActsLikeBridge) && !sim.Compat.HasFlag(CompatSurface.NoPassMobj);

    private static bool ClearsActorHeight(AuthoritySimulation sim, Actor actor, Actor other, double z, bool stationary = false)
    {
        if (!CanPassActorHeight(sim, actor, other)) return false;
        if (actor.DontOverlap && other.DontOverlap && !other.SpecialPickup)
            return stationary && actor is PlayerPawn;
        var top = other.Z.ToDouble() + other.Height.ToDouble();
        var actorTop = z + actor.Height.ToDouble();
        return other.SpecialPickup ? z > top || actorTop < other.Z.ToDouble()
            : z >= top || actorTop <= other.Z.ToDouble();
    }

    internal static bool MissileUsesActorHeightPassage(AuthoritySimulation sim, ProjectileActor missile, Actor other) =>
        CanPassActorHeight(sim, missile, other)
        && (!missile.Rip || other.DontRip || missile.NoBossRip && other.Boss);

    internal static bool MissileOverlapBlocks(AuthoritySimulation sim, ProjectileActor missile, Actor other) =>
        MissileUsesActorHeightPassage(sim, missile, other) && missile.DontOverlap && other.DontOverlap
        && !other.SpecialPickup;

    private static bool MissileBlocksPosition(AuthoritySimulation sim, ProjectileActor missile, Actor other)
    {
        if (other.Destroyed || !(other.Solid || other.Shootable)) return false;
        if (MissileOverlapBlocks(sim, missile, other))
            return true;
        if (MissileUsesActorHeightPassage(sim, missile, other)
            && ClearsActorHeight(sim, missile, other, missile.Z.ToDouble())) return false;
        if (other.NonShootable
            || missile.ThruGhost && other.Ghost
            || missile.MThruSpecies && missile.Owner?.SharesContactSpecies(other) == true
            || !missile.HitOwner && ReferenceEquals(missile.Owner, other)) return false;
        if (missile.Rip && other.Corpse && !other.Shootable) return false;
        var passHeight = other.ProjectilePassHeight.ToDouble();
        var clipHeight = passHeight > 0 ? passHeight
            : passHeight < 0 && sim.Compat.HasFlag(CompatSurface.MissileClip)
                ? -passHeight : other.Height.ToDouble();
        if (missile.Z.ToDouble() > other.Z.ToDouble() + clipHeight
            || missile.Z.ToDouble() + missile.Height.ToDouble() < other.Z.ToDouble()) return false;
        // Native attack restrictions block before the shootable SPECTRAL passage branch.
        if (!other.CanAttackHurtFrom(missile.Owner)) return true;
        return !other.Shootable || !other.Spectral || missile.Spectral;
    }

    private sealed class WanderMoveProbe
    {
        public bool FloatOk;
        public double Floor;
    }

    internal static bool TryWanderMove(AuthoritySimulation sim, Actor actor, double x, double y,
        out bool floatOk, out double floor, bool avoidingDropOff = false)
    {
        var probe = actor.Floating || actor.CanJump ? new WanderMoveProbe() : null;
        var moved = TryMove(sim, actor, x, y, out _, out _, wanderProbe: probe, avoidingDropOff: avoidingDropOff);
        floatOk = probe?.FloatOk ?? false;
        floor = probe?.Floor ?? 0;
        return moved;
    }

    private static bool TryMove(AuthoritySimulation sim, Actor actor, double x, double y, out LevelLine? wall, out Actor? blocker,
        bool invokeSlam = false, WanderMoveProbe? wanderProbe = null, bool avoidingDropOff = false)
    {
        wall = null;
        blocker = null;
        if (actor.NoClip && actor.Brain?.Charging != true)
        {
            actor.X = Fixed.FromDouble(x); actor.Y = Fixed.FromDouble(y);
            actor.SectorIndex = SectorAt(sim.Level, x, y);
            if (actor.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(sim, actor);
            return true;
        }
        var ox = actor.X.ToDouble();
        var oy = actor.Y.ToDouble();
        var radius = Math.Max(0, actor.Radius.ToDouble());
        foreach (var line in sim.Level.Lines)
        {
            if (actor.NoClip || !Blocks(sim, actor, line, ignoreMoveHeight: wanderProbe is not null)) continue;
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
        var opening = FlatOpening(sim, actor, sector, x, y);
        var dropOffBlocked = false;
        // Include the lower floor of a line touched by the destination bounding box.
        if (!actor.NoClip && sector >= 0 && actor.SectorIndex >= 0 &&
            (actor.NoDropOff || !actor.AllowDropOff && !actor.Floating && actor is not ProjectileActor))
        {
            var floorz = opening.Floor;
            if (actor.OnMobj)
                floorz = Math.Max(actor.Z.ToDouble(), floorz);
            var drop = floorz - opening.DropOff;
            var oldOpening = avoidingDropOff ? FlatOpening(sim, actor, actor.SectorIndex, ox, oy) : default;
            var blocked = avoidingDropOff
                ? oldOpening.Floor - opening.Floor > actor.MaxDropOffHeight.ToDouble()
                    || oldOpening.DropOff - opening.DropOff > actor.MaxDropOffHeight.ToDouble()
                : drop > actor.MaxDropOffHeight.ToDouble() && !actor.Blasted;
            if (blocked)
            {
                if (wanderProbe is null) return false;
                dropOffBlocked = true;
            }
        }
        var moveZ = actor.Z.ToDouble();
        var flyBlocked = !actor.NoClip && sector >= 0 && actor.Fly && actor.NoGravity
            && moveZ + actor.Height.ToDouble() > opening.Ceiling;
        if (flyBlocked && wanderProbe is null) return false;
        if (sector >= 0)
        {
            if (actor.FloorHugger) moveZ = opening.Floor;
            else if (actor.CeilingHugger) moveZ = opening.Ceiling - actor.Height.ToDouble();
        }
        if (!actor.NoClip && sector >= 0 && actor is ProjectileActor && !actor.FloorHugger
            && opening.Floor > moveZ) return false;
        var moveFloor = opening.Floor;
        var z = actor.Brain?.Charging == true ? moveZ : Math.Max(moveZ, moveFloor);
        var heightBlocked = !actor.NoClip && (flyBlocked || sector >= 0 && (z < moveFloor || z - moveZ > actor.MaxStepHeight.ToDouble()
            || z + actor.Height.ToDouble() > opening.Ceiling));
        if (heightBlocked && wanderProbe is null) return false;
        var solidMover = actor.BlocksActors || IsSolidNonCorpseObstacle(actor);
        if (!actor.ThruActors && (solidMover || actor.Blasted && !actor.IsDead && !actor.Destroyed))
        {
            IReadOnlyList<Actor> contacts = invokeSlam ? sim.Actors.ToArray() : sim.Actors;
            foreach (var other in contacts)
            {
                var corpse = actor.IceCorpse && IsCorpseObstacle(other);
                var chargeContact = actor.Brain?.Charging == true && (other.Solid || other.Shootable || other.SpecialPickup);
                var blastTarget = actor.Blasted && other.Shootable && other.IsMonster
                    && !other.Boss && !other.DontBlast && !other.IsDead && !other.Destroyed;
                if (ReferenceEquals(actor, other) || other.ThruActors
                    || actor.SharesEnabledThruBits(other)
                    || actor.ThruSpecies && actor.SharesContactSpecies(other) || !other.IsBlockmapActor
                    || (!chargeContact && !blastTarget && !(solidMover && (other.BlocksActors || IsSolidNonCorpseObstacle(other) || corpse)))
                    || ClearsActorHeight(sim, actor, other, wanderProbe is null ? z : actor.Z.ToDouble())) continue;
                var reach = radius + other.Radius.ToDouble();
                var cx = other.X.ToDouble();
                var cy = other.Y.ToDouble();
                var before = (ox - cx) * (ox - cx) + (oy - cy) * (oy - cy);
                var after = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                if (!(actor.Brain?.Charging == true && after < reach * reach - 1e-8
                    || DistanceSquared(cx, cy, ox, oy, x, y) < reach * reach - 1e-8
                    && (before >= reach * reach || after < before - 1e-8)))
                    continue;
                if (invokeSlam && actor.Brain?.Charging == true)
                {
                    var pass = actor.Slam(other);
                    if (actor.Destroyed || actor.IsDead) { blocker = other; return false; }
                    if (pass) continue;
                    blocker = other; return false;
                }
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
                var ceiling = opening.Ceiling;
                var rise = top - actor.Z.ToDouble();
                var playerStep = actor is PlayerPawn && other is not PlayerPawn && !corpse;
                var bridgeStep = actor is not PlayerPawn && actor is not ProjectileActor
                    && actor.IsMonster && !actor.NoGravity && actor.Brain?.Charging != true && other.ActsLikeBridge;
                if ((actor.CanPass || bridgeStep) && !sim.Compat.HasFlag(CompatSurface.NoPassMobj)
                    && !(actor.DontOverlap && other.DontOverlap && !other.SpecialPickup)
                    && actor.OnGround && !actor.Floating && (playerStep || bridgeStep || corpse)
                    && rise > 0 && rise <= actor.MaxStepHeight.ToDouble()
                    && top + actor.Height.ToDouble() <= ceiling)
                    continue;
                blocker = other;
                return false;
            }
        }
        if (wanderProbe is not null)
        {
            wanderProbe.FloatOk = opening.Ceiling - opening.Floor >= actor.Height.ToDouble();
            wanderProbe.Floor = opening.Floor;
        }
        if (heightBlocked || dropOffBlocked) return false;
        if (!actor.NoClip && actor.CantLeaveFloorPic && sector >= 0 && actor.SectorIndex >= 0
            && (!string.Equals(sim.Level.Sectors[sector].FloorPic, sim.Level.Sectors[actor.SectorIndex].FloorPic,
                StringComparison.OrdinalIgnoreCase) || opening.Floor != actor.Z.ToDouble())) return false;
        actor.X = Fixed.FromDouble(x);
        actor.Y = Fixed.FromDouble(y);
        actor.Z = Fixed.FromDouble(z);
        if (sector != actor.SectorIndex)
        {
            actor.SectorIndex = sector;
            actor.OnGround = z <= opening.Floor && actor.VelocityZ.Raw <= 0;
        }
        if (actor.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(sim, actor);
        return true;
    }

    private static (double Floor, double Ceiling, double DropOff) FlatOpening(
        AuthoritySimulation sim, Actor actor, int sector, double x, double y)
    {
        var floor = sim.FloorOf(sector);
        var ceiling = sector >= 0 ? sim.CeilingOf(sector) : double.PositiveInfinity;
        var dropOff = floor;
        var radius = Math.Max(0, actor.Radius.ToDouble());
        foreach (var line in sim.Level.Lines)
        {
            if (line.OneSided || !BoxCrossesLine(x, y, radius, line)) continue;
            var front = SideSector(sim.Level, line.SideFront);
            var back = SideSector(sim.Level, line.SideBack);
            if (front >= 0 && back >= 0)
            {
                floor = Math.Max(floor, LineOpeningFloor(sim, actor, line, front, back));
                ceiling = Math.Min(ceiling, Math.Min(sim.CeilingOf(front), sim.CeilingOf(back)));
                dropOff = Math.Min(dropOff, Math.Min(sim.FloorOf(front), sim.FloorOf(back)));
            }
        }
        return (floor, ceiling, dropOff);
    }

    internal static bool SlideLineBlocks(AuthoritySimulation sim, Actor actor, LevelLine line)
        => line.OneSided
            ? !LineSpecials.IsBackSide(line, actor.X.ToDouble(), actor.Y.ToDouble())
            : Blocks(sim, actor, line, railingPriority: false);

    private static bool Blocks(AuthoritySimulation sim, Actor actor, LevelLine line, bool railingPriority = true,
        bool ignoreMoveHeight = false)
    {
        var projectile = actor is ProjectileActor;
        if (line.OneSided) return true;
        var railing = railingPriority && UsesRailingOpening(actor, line);
        if (!railing && (line.Flags & LevelLine.BlockEverythingFlag) != 0) return true;
        if (!railing && actor.BlockAsPlayer && (line.Flags & (LevelLine.BlockingFlag | LevelLine.BlockPlayersFlag)) != 0) return true;
        if (!railing && projectile && (line.Flags & LevelLine.BlockProjectileFlag) != 0) return true;
        if (!railing && !projectile && (line.Flags & LevelLine.BlockingFlag) != 0) return true;
        if (!railing && actor is PlayerPawn && sim.Compat.HasFlag(CompatSurface.Mbf21)
            && (line.Flags & LevelLine.BlockPlayersFlag) != 0) return true;
        var ignoreMonsterBlocking = actor.NoBlockMonsters
            || actor.Friendly && sim.Compat.HasFlag(CompatSurface.NoBlockFriends);
        if (!railing && (line.Flags & LevelLine.BlockMonstersFlag) != 0 && !projectile
            && !ignoreMonsterBlocking) return true;
        if (!railing && !projectile && !ignoreMonsterBlocking
            && sim.Compat.HasFlag(CompatSurface.Mbf21) && !actor.Floating && !actor.NoGravity
            && (line.Flags2 & LevelLine.BlockLandMonstersFlag2) != 0) return true;
        if (!railing && !projectile && actor.Floating && (line.Flags & LevelLine.BlockFloatersFlag) != 0) return true;
        var front = SideSector(sim.Level, line.SideFront);
        var back = SideSector(sim.Level, line.SideBack);
        if (front < 0 || back < 0) return false;
        var floor = railingPriority ? LineOpeningFloor(sim, actor, line, front, back)
            : Math.Max(sim.FloorOf(front), sim.FloorOf(back));
        var ceiling = Math.Min(sim.CeilingOf(front), sim.CeilingOf(back));
        if (ignoreMoveHeight) return ceiling - floor < actor.Height.ToDouble();
        if (!railingPriority)
        {
            var oldZ = actor.Z;
            var height = actor.Height.ToDouble();
            if (ceiling - floor < height || ceiling < oldZ.ToDouble() + height
                || floor - oldZ.ToDouble() > actor.MaxStepHeight.ToDouble()) return true;
            if (oldZ.ToDouble() < floor)
            {
                try
                {
                    actor.Z = Fixed.FromDouble(floor);
                    return FindZBlocker(sim, actor) is not null;
                }
                finally { actor.Z = oldZ; }
            }
            return false;
        }
        var z = actor.FloorHugger ? floor : actor.CeilingHugger ? ceiling - actor.Height.ToDouble() : actor.Z.ToDouble();
        var bottom = Math.Max(z, floor);
        return floor - z > actor.MaxStepHeight.ToDouble()
            || bottom + actor.Height.ToDouble() > ceiling;
    }

    private static double LineOpeningFloor(AuthoritySimulation sim, Actor actor, LevelLine line, int front, int back)
    {
        var floor = Math.Max(sim.FloorOf(front), sim.FloorOf(back));
        if (UsesRailingOpening(actor, line)
            && (!sim.Compat.HasFlag(CompatSurface.Railing) || floor == sim.FloorOf(actor.SectorIndex)))
            floor += 32;
        return floor;
    }

    private static bool UsesRailingOpening(Actor actor, LevelLine line)
        => (line.Flags & LevelLine.RailingFlag) != 0 && (actor is not ProjectileActor
            || (line.Flags & (LevelLine.BlockEverythingFlag | LevelLine.BlockProjectileFlag
                | (actor.BlockAsPlayer ? LevelLine.BlockingFlag | LevelLine.BlockPlayersFlag : 0))) != 0);

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
