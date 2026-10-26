using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Live playsim wire from <see cref="Actor"/> to <see cref="VmPMobjActorTick"/> (chunk 728).</summary>
public static class ActorPmobjTickBridge
{
    public static bool UsesManagedTick(Actor actor) => actor is not ProjectileActor;

    public static void Execute(AuthoritySimulation sim, Actor actor)
    {
        var mo = FromActor(actor);
        var st = BuildState(sim, actor, mo);
        VmPMobjActorTick.Tick(st);
        ApplyToActor(actor, mo);
    }

    static VmPMobjActorTick.TickActor FromActor(Actor actor)
    {
        var player = actor as PlayerPawn;
        return new VmPMobjActorTick.TickActor
        {
            Flags = PackFlags(actor),
            Flags2 = (actor.Blasted ? VmPMobjActorTick.Mf2Blasted : 0)
                | (actor.OnMobj ? VmPMobjActorTick.Mf2Onmobj : 0)
                | (actor.CanPass ? VmPMobjActorTick.Mf2Passmobj : 0),
            Flags3 = actor.IsMonster ? VmPMobjActorTick.Mf3Ismonster : 0,
            Flags4 = actor.VerticalFriction ? VmPMobjActorTick.Mf4Vfriction : 0,
            Flags5 = actor.NoInteraction ? VmPMobjActorTick.Mf5Nointeraction : 0,
            Flags6 = PackFlags6(actor),
            Flags8 = actor.InScrollSector ? VmPMobjActorTick.Mf8Inscrollsec : 0,
            Flags9 = 0,
            Effects = (actor.VisibilityPulse ? VmPMobjActorTick.FxVisibilitypulse : 0),
            Health = actor.Health,
            VelX = actor.VelocityX.ToDouble(),
            VelY = actor.VelocityY.ToDouble(),
            VelZ = actor.VelocityZ.ToDouble(),
            Z = actor.Z.ToDouble(),
            FloorZ = actor.Simulation is { } s ? s.FloorOf(actor.SectorIndex) : 0,
            Radius = actor.Radius.ToDouble(),
            Height = actor.Height.ToDouble(),
            Alpha = actor.Alpha,
            StealthAlpha = actor.StealthAlpha,
            MaxSlopeSteepness = 0.5625,
            MaxStepHeight = actor.MaxStepHeight.ToDouble(),
            Gravity = ActorPhysics.Gravity,
            SectorGravity = actor.SectorIndex >= 0 && actor.SectorIndex < actor.Level?.Sectors.Count
                ? actor.Level.Sectors[actor.SectorIndex].Gravity : 1,
            VisDir = actor.VisibilityDirection,
            SmokeCounter = 0,
            Special2 = 0,
            HasPlayer = player is not null,
            PlayerCheats = 0,
            ViewHeight = player?.ViewHeight ?? 41,
            DeltaViewHeight = player?.DeltaViewHeight ?? 0,
            Destroyed = actor.Destroyed,
            EuthanizeMe = actor.Destroyed,
            State = actor.States,
            Freezetics = actor.FreezeTics,
            Alternative = false,
            Frozen = player is { Frozen: true } or { TotallyFrozen: true },
            ZeroDamage = actor.IsZeroDamage(),
            Sentient = actor.IsMonster || player is not null,
            CummScrollX = 0,
            CummScrollY = 0,
            BounceFlags = 0,
        };
    }

    static int PackFlags(Actor actor) =>
        (actor is ProjectileActor ? VmPMobjActorTick.MfMissile : 0)
        | (actor.SpecialPickup ? VmPMobjActorTick.MfSpecial : 0)
        | (actor.NoClip ? VmPMobjActorTick.MfNoclip : 0)
        | (actor.NoSector ? VmPMobjActorTick.MfNosector : 0)
        | (actor.NoBlockmap ? VmPMobjActorTick.MfNoblockmap : 0)
        | (actor.NoGravity ? VmPMobjActorTick.MfNogravity : 0)
        | (actor.Solid ? VmPMobjActorTick.MfSolid : 0)
        | (actor.Stealth ? VmPMobjActorTick.MfStealth : 0);

    static int PackFlags6(Actor actor) =>
        (actor.Touchy ? VmPMobjActorTick.Mf6Touchy : 0)
        | (actor.Armed ? VmPMobjActorTick.Mf6Armed : 0)
        | (actor.BumpSpecial ? VmPMobjActorTick.Mf6Bumpspecial : 0);

    static void ApplyToActor(Actor actor, VmPMobjActorTick.TickActor mo)
    {
        actor.VelocityX = Fixed.FromDouble(mo.VelX);
        actor.VelocityY = Fixed.FromDouble(mo.VelY);
        actor.VelocityZ = Fixed.FromDouble(mo.VelZ);
        actor.Z = Fixed.FromDouble(mo.Z);
        actor.FreezeTics = mo.Freezetics;
        actor.InScrollSector = (mo.Flags8 & VmPMobjActorTick.Mf8Inscrollsec) != 0;
        actor.NoBlockmap = (mo.Flags & VmPMobjActorTick.MfNoblockmap) != 0;
        actor.VerticalFriction = (mo.Flags4 & VmPMobjActorTick.Mf4Vfriction) != 0;
        actor.Blasted = (mo.Flags2 & VmPMobjActorTick.Mf2Blasted) != 0;
        actor.OnMobj = (mo.Flags2 & VmPMobjActorTick.Mf2Onmobj) != 0;
        actor.Touchy = (mo.Flags6 & VmPMobjActorTick.Mf6Touchy) != 0;
        actor.Armed = (mo.Flags6 & VmPMobjActorTick.Mf6Armed) != 0;
        actor.Alpha = mo.Alpha;
        actor.VisibilityDirection = mo.VisDir;
        if (mo.Destroyed)
            actor.Destroy();
    }

    static VmPMobjActorTick.TickState BuildState(AuthoritySimulation sim, Actor actor, VmPMobjActorTick.TickActor mo) =>
        new()
        {
            Actor = mo,
            LevelCompatFlags = PackCompat(sim),
            MapTime = sim.Thinkers.Clock.Tic,
            TicRate = GameTicClock.TicRate,
            LevelHasScrolls = sim.SectorScrollX.Any(v => v != 0) || sim.SectorScrollY.Any(v => v != 0)
                || sim.Level.Sectors.Any(s => s.Special >= VmPMobjActorTick.ScrollNorthSlow),
            BotTickEligible = false,
            CoopAuthorityVisual = false,
            InvasionMirrorBlocking = false,
            TickBehaviors = () => RunBehaviors(sim, actor),
            WalkInventoryDoEffect = null,
            AdvanceDecoupledModelAnim = null,
            SpawnRocketSmoke = null,
            SpawnGrenadeSmoke = null,
            BotTick = null,
            TouchingSectors = _ => BuildTouchingSectors(sim, actor),
            LevelScrollAt = sector => LevelScrollForActor(sim, actor, sector),
            FindFloorPlane = _ => FindFloorPlane(sim, actor)
                ?? new VmPMobjActorTick.FloorPlaneSample { FC = 1, ZAtFloorSector = sim.FloorOf(actor.SectorIndex) },
            TouchingSlopeSectors = _ => TouchingSlopeSectors(sim, actor),
            XYMovement = _ => RunXYMovement(sim, actor, mo),
            CheckOnMobj = _ => WrapOnMobj(ActorPhysics.CheckOnMobj(sim, actor)),
            ZMovement = (_, _) => { },
            PlayerLandedOnThing = (_, onmo) => PlayerLanded(sim, actor, onmo),
            AdjustViewOnMobj = (a, top) => AdjustView(actor, top),
            ActivateBumpSpecial = (_, onmo) => TryBump(sim, actor, onmo),
            Crash = _ => actor.Crash(),
            BounceActor = (_, onmo) => false,
            ExplodeMissile = (_, _) => { },
            DoMissileDamage = (_, _) => { },
            TriggerSecActFloor = (_, _) => { },
            TriggerSecActCeiling = (_, _) => { },
            VelFromAngleMin = _ => actor.VelFromAngle(1.0 / Fixed.Unit),
            NoInteractionMove = (_, dx, dy) => NoInteractionMove(sim, actor, dx, dy),
            CheckPortalTransition = (_, _) => { },
        };

    static int PackCompat(AuthoritySimulation sim)
    {
        var flags = 0;
        if (sim.Compat.HasFlag(CompatSurface.BoomScroll)) flags |= VmPMobjActorTick.CompatfBoomscroll;
        if (sim.Compat.HasFlag(CompatSurface.NoPassMobj)) flags |= VmPMobjActorTick.CompatfNoPassmobj;
        return flags;
    }

    static void RunBehaviors(AuthoritySimulation sim, Actor actor)
    {
        if (actor.PendingSpawnDeath)
        {
            actor.AdvancePendingSpawnDeathTick();
            return;
        }

        actor.States.Tick(actor);
        if (actor.Destroyed) return;
        if (!actor.PendingSpawnDeath)
        {
            if (!actor.Dormant)
                actor.Brain?.Tick(sim, actor, nativeMovement: sim.NativeMonsterMovement);
            else if (!actor.IsDead)
                actor.Brain?.AdvanceRaiseFrame();
        }
    }

    static IReadOnlyList<VmPMobjActorTick.SectorCarryNode> BuildTouchingSectors(AuthoritySimulation sim, Actor actor)
    {
        var radius = actor.Radius.ToDouble();
        var x = actor.X.ToDouble();
        var y = actor.Y.ToDouble();
        var z = actor.Z.ToDouble();
        var list = new List<VmPMobjActorTick.SectorCarryNode>();
        foreach (var sector in ActorPhysics.TouchingSectorIndices(sim.Level, x, y, radius))
        {
            if ((uint)sector >= (uint)sim.Level.Sectors.Count) continue;
            var sec = sim.Level.Sectors[sector];
            var floorAtActor = sim.FloorOf(sector);
            list.Add(new VmPMobjActorTick.SectorCarryNode
            {
                SectorIndex = sector,
                Special = sec.Special,
                SectorTag = sec.Tag,
                HasHeightSec = false,
                FloorAtActor = floorAtActor,
                FloorAtPos = floorAtActor,
                WaterHeight = floorAtActor,
                IsAboveFloor = z > floorAtActor + 1e-6,
                NoGravity = actor.NoGravity,
            });
        }
        return list;
    }

    static VmPMobjActorTick.FloorPlaneSample? FindFloorPlane(AuthoritySimulation sim, Actor actor)
    {
        if (actor.SectorIndex < 0) return null;
        var floor = sim.FloorOf(actor.SectorIndex);
        if (Math.Abs(actor.Z.ToDouble() - floor) > 1e-6) return null;
        return new VmPMobjActorTick.FloorPlaneSample
        {
            FC = 0,
            NormalX = 0,
            NormalY = 0,
            ZAtFloorSector = floor,
        };
    }

    static IReadOnlyList<VmPMobjActorTick.SteepSlopeSector> TouchingSlopeSectors(AuthoritySimulation sim, Actor actor)
    {
        var radius = actor.Radius.ToDouble();
        var x = actor.X.ToDouble();
        var y = actor.Y.ToDouble();
        return ActorPhysics.TouchingSectorIndices(sim.Level, x, y, radius)
            .Select(sector => new VmPMobjActorTick.SteepSlopeSector
            {
                FC = 0,
                ZAtPoint = sim.FloorOf(sector),
            })
            .ToArray();
    }

    static double RunXYMovement(AuthoritySimulation sim, Actor actor, VmPMobjActorTick.TickActor mo)
    {
        var oldFloor = sim.FloorOf(actor.SectorIndex);
        // Sector carry displacement runs after thinkers via <see cref="ActorPhysics.ApplySectorScroll"/>.
        ActorPhysics.Step(sim, actor);
        mo.VelX = actor.VelocityX.ToDouble();
        mo.VelY = actor.VelocityY.ToDouble();
        mo.VelZ = actor.VelocityZ.ToDouble();
        mo.Z = actor.Z.ToDouble();
        mo.FloorZ = sim.FloorOf(actor.SectorIndex);
        return oldFloor;
    }

    static VmPMobjActorTick.OnMobjActor? WrapOnMobj(Actor? support) =>
        support is null ? null : new VmPMobjActorTick.OnMobjActor
        {
            Top = support.Z.ToDouble() + support.Height.ToDouble(),
            Flags6 = support.BumpSpecial ? VmPMobjActorTick.Mf6Bumpspecial : 0,
            ActivationType = support.ActivationType,
            LastBump = support.LastBumpTic,
        };

    static void PlayerLanded(AuthoritySimulation sim, Actor actor, VmPMobjActorTick.OnMobjActor onmo)
    {
        if (actor is not PlayerPawn player) return;
        var support = sim.Actors.FirstOrDefault(a => a.Z.ToDouble() + a.Height.ToDouble() == onmo.Top);
        if (support is null) return;
        player.PlayerSquatView(support);
        FallingDamage.Apply(actor);
        actor.PlayerLandedMakeRumble(support);
        actor.PlayerLandedMakeGruntSound(support);
    }

    static void AdjustView(Actor actor, double top)
    {
        if (actor is PlayerPawn player)
            player.ViewHeight -= top - actor.Z.ToDouble();
    }

    static bool TryBump(AuthoritySimulation sim, Actor actor, VmPMobjActorTick.OnMobjActor onmo)
    {
        var support = sim.Actors.FirstOrDefault(a =>
            Math.Abs(a.Z.ToDouble() + a.Height.ToDouble() - onmo.Top) < 1e-6);
        if (support is null) return false;
        ActorSpecialActions.TryBump(sim, support, actor);
        return true;
    }

    static void NoInteractionMove(AuthoritySimulation sim, Actor actor, double dx, double dy)
    {
        actor.X = Fixed.FromDouble(actor.X.ToDouble() + dx);
        actor.Y = Fixed.FromDouble(actor.Y.ToDouble() + dy);
        actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + actor.VelocityZ.ToDouble());
        ActorPhysics.FitToSector(sim, actor, carryFloor: false);
    }

    /// <summary>Scroll vector for one sector and actor (used by <see cref="VmPMobjActorTick"/>).</summary>
    internal static (double X, double Y) LevelScrollForActor(AuthoritySimulation sim, Actor actor, int sector)
    {
        if ((uint)sector >= (uint)sim.SectorScrollX.Length) return (0, 0);
        var (dx, dy) = sim.SumCarryScrollForActor(sector, actor);
        return (sim.SectorScrollX[sector] + dx, sim.SectorScrollY[sector] + dy);
    }
}
