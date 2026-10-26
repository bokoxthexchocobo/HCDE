namespace HCDE.Playsim;

/// <summary>Whole <c>AActor::Tick</c> from <c>p_mobj.cpp</c> (4523–5075).</summary>
public static class VmPMobjActorTick
{
    public const int MfUnmorphed = 1;
    public const int MfMissile = 1 << 2;
    public const int MfSpecial = 1 << 3;
    public const int MfNoclip = 1 << 4;
    public const int MfNosector = 1 << 5;
    public const int MfNoblockmap = 1 << 6;
    public const int MfNogravity = 1 << 7;
    public const int MfSolid = 1 << 8;
    public const int MfStealth = 1 << 9;
    public const int Mf5Nointeraction = 1;
    public const int Mf2Passmobj = 1;
    public const int Mf2Blasted = 1 << 1;
    public const int Mf2Onmobj = 1;
    public const int Mf3Ismonster = 1 << 2;
    public const int Mf4Vfriction = 1 << 3;
    public const int Mf6Bosscube = 1 << 4;
    public const int Mf6Touchy = 1 << 5;
    public const int Mf6Armed = 1 << 6;
    public const int Mf6Bumpspecial = 1 << 7;
    public const int Mf8Inscrollsec = 1;
    public const int Mf9Decoupledanimations = 1;
    public const int FxRocket = 1;
    public const int FxGrenade = 1 << 1;
    public const int FxVisibilitypulse = 1 << 2;
    public const int CfPredicting = 1;
    public const int CompatfBoomscroll = 1;
    public const int CompatfRavenscroll = 1 << 1;
    public const int CompatfNoPassmobj = 1 << 2;
    public const int ThingspecMonsterTrigger = 1;
    public const int ThingspecMissileTrigger = 1 << 1;
    public const int ScrollNorthSlow = 201;
    public const int ScrollSouthWestFast = 224;
    public const int CarryEast5 = 225;
    public const int CarryWest35 = 244;
    public const int DScrollEastLavaDamage = 84;
    public const int ScrollStrifeCurrent = 118;
    public const double CarryFactor = 32 * 256;

    private static readonly sbyte[] HexenCompatSpeeds = [-25, 0, -10, -5, 0, 5, 10, 0, 25];
    private static readonly (sbyte X, sbyte Y)[] HexenScrollies =
    [
        (0, 1), (0, 2), (0, 4),
        (-1, 0), (-2, 0), (-4, 0),
        (0, -1), (0, -2), (0, -4),
        (1, 0), (2, 0), (4, 0),
        (1, 1), (2, 2), (4, 4),
        (-1, 1), (-2, 2), (-4, 4),
        (-1, -1), (-2, -2), (-4, -4),
        (1, -1), (2, -2), (4, -4),
    ];
    private static readonly byte[] HereticScrollDirs = [6, 9, 1, 4];
    private static readonly byte[] HereticSpeedMuls = [5, 10, 25, 30, 35];

    public sealed class TickActor
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public int Flags8 { get; set; }
        public int Flags9 { get; set; }
        public int Effects { get; set; }
        public int Health { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public double Radius { get; set; }
        public double Height { get; set; }
        public double Alpha { get; set; } = 1;
        public double StealthAlpha { get; set; } = 0.25;
        public double MaxSlopeSteepness { get; set; } = 0.5625;
        public double MaxStepHeight { get; set; } = 24;
        public double Gravity { get; set; } = 1;
        public double SectorGravity { get; set; } = 1;
        public int VisDir { get; set; }
        public int SmokeCounter { get; set; }
        public int Special2 { get; set; }
        public bool HasPlayer { get; set; }
        public int PlayerCheats { get; set; }
        public double ViewHeight { get; set; } = 41;
        public double DeltaViewHeight { get; set; }
        public bool Destroyed { get; set; }
        public bool EuthanizeMe { get; set; }
        public object? State { get; set; }
        public int Freezetics { get; set; }
        public bool Alternative { get; set; }
        public bool Frozen { get; set; }
        public bool ZeroDamage { get; set; }
        public bool Sentient { get; set; }
        public bool CrossingPortal { get; set; }
        public double CummScrollX { get; set; }
        public double CummScrollY { get; set; }
        public object? BlockingMobj { get; set; }
        public object? BlockingFloor { get; set; }
        public object? BlockingCeiling { get; set; }
        public object? OldBlockingFloor { get; set; }
        public object? OldBlockingCeiling { get; set; }
        public int BounceFlags { get; set; }
        public const int BounceActors = 1;
    }

    public sealed class OnMobjActor
    {
        public double Top { get; init; }
        public int Flags6 { get; init; }
        public int ActivationType { get; init; }
        public int LastBump { get; set; }
    }

    public sealed class SectorCarryNode
    {
        public int SectorIndex { get; init; }
        public int Special { get; init; }
        public int SectorTag { get; init; }
        public bool HasHeightSec { get; init; }
        public double FloorAtActor { get; init; }
        public double FloorAtPos { get; init; }
        public double WaterHeight { get; init; }
        public bool NoGravity { get; init; }
        public bool IsAboveFloor { get; init; }
    }

    public sealed class FloorPlaneSample
    {
        public double FC { get; init; }
        public double NormalX { get; init; }
        public double NormalY { get; init; }
        public double ZAtFloorSector { get; init; }
    }

    public sealed class SteepSlopeSector
    {
        public double FC { get; init; }
        public double ZAtPoint { get; init; }
    }

    public sealed class TickState
    {
        public required TickActor Actor { get; init; }
        public int LevelCompatFlags { get; init; }
        public int MapTime { get; init; }
        public int TicRate { get; init; } = 35;
        public bool LevelHasScrolls { get; init; }
        public bool BotTickEligible { get; init; }
        public bool CoopAuthorityVisual { get; init; }
        public bool InvasionMirrorBlocking { get; init; }
        public Func<bool>? CheckUnmorph { get; init; }
        public Action? TickBehaviors { get; init; }
        public Action? WalkInventoryDoEffect { get; init; }
        public Action? AdvanceDecoupledModelAnim { get; init; }
        public Action<TickActor>? SpawnRocketSmoke { get; init; }
        public Action<TickActor>? SpawnGrenadeSmoke { get; init; }
        public Action<TickActor>? BotTick { get; init; }
        public Func<TickActor, IReadOnlyList<SectorCarryNode>>? TouchingSectors { get; init; }
        public Func<int, (double X, double Y)>? LevelScrollAt { get; init; }
        public Func<TickActor, FloorPlaneSample>? FindFloorPlane { get; init; }
        public Func<TickActor, IReadOnlyList<SteepSlopeSector>>? TouchingSlopeSectors { get; init; }
        public Func<TickActor, double>? XYMovement { get; init; }
        public Func<TickActor, object?>? CheckOnMobj { get; init; }
        public Action<TickActor, double>? ZMovement { get; init; }
        public Action<TickActor, OnMobjActor>? PlayerLandedOnThing { get; init; }
        public Action<TickActor, double>? AdjustViewOnMobj { get; init; }
        public Func<TickActor, OnMobjActor, bool>? ActivateBumpSpecial { get; init; }
        public Action<TickActor>? Crash { get; init; }
        public Func<TickActor, OnMobjActor, bool>? BounceActor { get; init; }
        public Action<TickActor, object?>? ExplodeMissile { get; init; }
        public Action<TickActor, object?>? DoMissileDamage { get; init; }
        public Action<TickActor, object?>? TriggerSecActFloor { get; init; }
        public Action<TickActor, object?>? TriggerSecActCeiling { get; init; }
        public Action<TickActor>? VelFromAngleMin { get; init; }
        public Action<TickActor, double, double>? NoInteractionMove { get; init; }
        public Action<TickActor, bool>? CheckPortalTransition { get; init; }
    }

    public static void Tick(TickState st)
    {
        var mo = st.Actor;

        if (mo.Alternative && !mo.HasPlayer)
        {
            if ((mo.Flags & MfUnmorphed) != 0)
                return;
            if (st.CheckUnmorph?.Invoke() == true)
                return;
        }

        if (mo.Freezetics > 0)
        {
            mo.Freezetics--;
            return;
        }

        if ((mo.Flags9 & Mf9Decoupledanimations) != 0)
            st.AdvanceDecoupledModelAnim?.Invoke();

        if (mo.State == null)
        {
            mo.Destroyed = true;
            return;
        }

        if (!mo.Frozen)
            st.TickBehaviors?.Invoke();

        if ((mo.Flags5 & Mf5Nointeraction) != 0)
        {
            TickNoInteraction(st, mo);
            return;
        }

        if (mo.HasPlayer)
            mo.CrossingPortal = false;

        if (!mo.HasPlayer || (mo.PlayerCheats & CfPredicting) == 0)
            st.WalkInventoryDoEffect?.Invoke();

        if ((mo.Flags & MfUnmorphed) != 0)
            return;

        if (mo.Frozen)
        {
            HandleFrozenBossCube(st, mo);
            return;
        }

        HandleRocketGrenadeTrail(st, mo);
        ApplyVerticalFriction(mo);
        PulseVisibility(mo, st.TicRate);

        if (st.BotTickEligible)
            st.BotTick?.Invoke(mo);

        var scroll = AccumulateSectorCarry(st, mo);
        mo.CummScrollX = scroll.X;
        mo.CummScrollY = scroll.Y;

        ApplySteepSlopePush(st, mo);

        if ((mo.Flags & MfMissile) != 0 && mo.VelX == 0 && mo.VelY == 0 && !mo.ZeroDamage)
            st.VelFromAngleMin?.Invoke(mo);

        mo.BlockingMobj = null;
        mo.OldBlockingFloor = mo.BlockingFloor;
        mo.OldBlockingCeiling = mo.BlockingCeiling;
        mo.BlockingFloor = null;
        mo.BlockingCeiling = null;

        var oldFloorZ = st.XYMovement?.Invoke(mo) ?? mo.FloorZ;
        if (mo.EuthanizeMe)
            return;

        if (mo.BlockingFloor != null && !ReferenceEquals(mo.BlockingFloor, mo.OldBlockingFloor)
            && (!mo.HasPlayer || (mo.PlayerCheats & CfPredicting) == 0))
            st.TriggerSecActFloor?.Invoke(mo, mo.BlockingFloor);
        if (mo.BlockingCeiling != null && !ReferenceEquals(mo.BlockingCeiling, mo.OldBlockingCeiling)
            && (!mo.HasPlayer || (mo.PlayerCheats & CfPredicting) == 0))
            st.TriggerSecActCeiling?.Invoke(mo, mo.BlockingCeiling);

        if (mo.VelX == 0 && mo.VelY == 0)
            HandleRestingActor(mo);

        var vertical = mo.VelZ != 0 || mo.BlockingMobj != null || mo.Z != mo.FloorZ;
        if (vertical)
            HandleVerticalMotion(st, mo, oldFloorZ);
        else if (mo.Z <= mo.FloorZ)
        {
            st.Crash?.Invoke(mo);
            if (mo.EuthanizeMe)
                mo.Destroyed = true;
        }
    }

    static void TickNoInteraction(TickState st, TickActor mo)
    {
        if (mo.Frozen)
        {
            HandleFrozenBossCube(st, mo);
            return;
        }

        if (st.CoopAuthorityVisual)
        {
            mo.Flags8 &= ~Mf8Inscrollsec;
            return;
        }

        if (mo.VelX != 0 || mo.VelY != 0 || (mo.Flags & MfNoblockmap) == 0)
        {
            if (!st.InvasionMirrorBlocking)
            {
                mo.Flags |= MfNoblockmap;
                st.NoInteractionMove?.Invoke(mo, mo.VelX, mo.VelY);
                st.CheckPortalTransition?.Invoke(mo, false);
            }
        }
        mo.Flags8 &= ~Mf8Inscrollsec;
    }

    static void HandleFrozenBossCube(TickState st, TickActor mo)
    {
        if ((mo.Flags6 & Mf6Bosscube) != 0)
            mo.Special2++;
        if ((mo.Flags9 & Mf9Decoupledanimations) != 0)
            st.AdvanceDecoupledModelAnim?.Invoke();
    }

    static void HandleRocketGrenadeTrail(TickState st, TickActor mo)
    {
        if ((mo.Effects & FxRocket) != 0)
        {
            if (++mo.SmokeCounter == 4)
            {
                mo.SmokeCounter = 0;
                st.SpawnRocketSmoke?.Invoke(mo);
            }
        }
        else if ((mo.Effects & FxGrenade) != 0 && ++mo.SmokeCounter == 8)
        {
            mo.SmokeCounter = 0;
            st.SpawnGrenadeSmoke?.Invoke(mo);
        }
    }

    static void ApplyVerticalFriction(TickActor mo)
    {
        if ((mo.Flags4 & Mf4Vfriction) == 0 || mo.Health <= 0)
            return;
        if (Math.Abs(mo.VelZ) < 0.25)
        {
            mo.VelZ = 0;
            mo.Flags4 &= ~Mf4Vfriction;
        }
        else
            mo.VelZ *= 0xE800 / 65536.0;
    }

    static void PulseVisibility(TickActor mo, int ticRate)
    {
        if ((mo.Effects & FxVisibilitypulse) != 0)
        {
            if (mo.VisDir > 0)
            {
                mo.Alpha += 1 / 32.0;
                if (mo.Alpha >= 1) { mo.Alpha = 1; mo.VisDir = -1; }
            }
            else
            {
                mo.Alpha -= 1 / 32.0;
                if (mo.Alpha <= 0.25) { mo.Alpha = 0.25; mo.VisDir = 1; }
            }
        }
        else if ((mo.Flags & MfStealth) != 0)
        {
            if (mo.VisDir > 0)
            {
                mo.Alpha += 2.0 / ticRate;
                if (mo.Alpha > 1) { mo.Alpha = 1; mo.VisDir = 0; }
            }
            else if (mo.VisDir < 0)
            {
                mo.Alpha -= 1.5 / ticRate;
                if (mo.Alpha < mo.StealthAlpha) { mo.Alpha = mo.StealthAlpha; mo.VisDir = 0; }
            }
        }
    }

    public static (double X, double Y) AccumulateSectorCarry(TickState st, TickActor mo)
    {
        var cummX = 0.0;
        var cummY = 0.0;
        var countX = 0;
        var countY = 0;

        var inScroll = (mo.Flags8 & Mf8Inscrollsec) != 0 && st.LevelHasScrolls;
        if (!((inScroll || mo.HasPlayer) && (mo.Flags & MfNoclip) == 0 && (mo.Flags & MfNosector) == 0))
            return (cummX, cummY);

        mo.Flags8 &= ~Mf8Inscrollsec;
        var nodes = st.TouchingSectors?.Invoke(mo);
        if (nodes is null || nodes.Count == 0)
            return (cummX, cummY);

        foreach (var node in nodes)
        {
            var scroll = st.LevelScrollAt?.Invoke(node.SectorIndex) ?? (0.0, 0.0);
            if (mo.HasPlayer)
                scroll = AddPlayerScrollSpecial(st.LevelCompatFlags, node.Special, node.SectorTag, scroll);

            if (scroll.X == 0 && scroll.Y == 0)
                continue;
            if (node.NoGravity && !node.HasHeightSec)
                continue;
            if (node.IsAboveFloor)
            {
                if (!node.HasHeightSec)
                    continue;
                if (node.WaterHeight > node.FloorAtPos && mo.Z >= node.WaterHeight)
                    continue;
            }

            cummX += scroll.X;
            cummY += scroll.Y;
            if (scroll.X != 0) countX++;
            if (scroll.Y != 0) countY++;
        }

        return AverageSectorScroll(mo.HasPlayer, st.LevelCompatFlags, countX, countY, cummX, cummY);
    }

    public static (double X, double Y) AddPlayerScrollSpecial(
        int compatFlags,
        int special,
        int sectorTag,
        (double X, double Y) scroll)
    {
        var scrollX = scroll.X;
        var scrollY = scroll.Y;
        if (special >= ScrollNorthSlow && special <= ScrollSouthWestFast)
        {
            var scrollType = special - ScrollNorthSlow;
            if ((compatFlags & CompatfRavenscroll) != 0)
            {
                scrollX -= HexenCompatSpeeds[HexenScrollies[scrollType].X + 4] * (1.0 / CarryFactor);
                scrollY += HexenCompatSpeeds[HexenScrollies[scrollType].Y + 4] * (1.0 / CarryFactor);
            }
            else
            {
                scrollX -= HexenScrollies[scrollType].X * 0.5;
                scrollY += HexenScrollies[scrollType].Y * 0.5;
            }
        }
        else if (special >= CarryEast5 && special <= CarryWest35)
        {
            var scrollType = special - CarryEast5;
            var dir = HereticScrollDirs[scrollType / 5];
            var carrySpeed = HereticSpeedMuls[scrollType % 5] * (1.0 / CarryFactor);
            if (scrollType < 5 && (compatFlags & CompatfRavenscroll) == 0)
                carrySpeed = (1 << ((scrollType % 5) + 15)) / 65536.0;
            scrollX += carrySpeed * ((dir & 3) - 1);
            scrollY += carrySpeed * (((dir & 12) >> 2) - 1);
        }
        else if (special == DScrollEastLavaDamage)
        {
            scrollX += (compatFlags & CompatfRavenscroll) != 0 ? 28.0 / CarryFactor : 12.0 / CarryFactor;
        }
        else if (special == ScrollStrifeCurrent)
        {
            var (speed, angleDeg) = VmActorTerrainSetSizeModel.ScrollStrifeCurrentThrust(sectorTag - 100);
            var rad = angleDeg * (Math.PI / 180.0);
            scrollX += Math.Cos(rad) * speed;
            scrollY += Math.Sin(rad) * speed;
        }

        return (scrollX, scrollY);
    }

    static void ApplySteepSlopePush(TickState st, TickActor mo)
    {
        if ((mo.Flags & MfSolid) == 0 || (mo.Flags & (MfNoclip | MfNogravity | MfNoblockmap)) != 0)
            return;
        if (mo.VelZ > 0 || mo.Z != mo.FloorZ)
            return;

        var plane = st.FindFloorPlane?.Invoke(mo);
        if (plane is null || plane.FC >= mo.MaxSlopeSteepness
            || plane.ZAtFloorSector > mo.FloorZ)
            return;

        var doPush = true;
        if (plane.FC > mo.MaxSlopeSteepness * 2 / 3)
        {
            var sectors = st.TouchingSlopeSectors?.Invoke(mo);
            if (sectors != null)
            {
                foreach (var sec in sectors)
                {
                    if (sec.FC >= mo.MaxSlopeSteepness && sec.ZAtPoint >= mo.Z - mo.MaxStepHeight)
                    {
                        doPush = false;
                        break;
                    }
                }
            }
        }

        if (doPush)
        {
            mo.VelX += plane.NormalX;
            mo.VelY += plane.NormalY;
        }
    }

    static void HandleRestingActor(TickActor mo)
    {
        if ((mo.Flags2 & Mf2Blasted) != 0)
            mo.Flags2 &= ~Mf2Blasted;
        if ((mo.Flags6 & Mf6Touchy) != 0 && !mo.Sentient)
            mo.Flags6 |= Mf6Armed;
    }

    static void HandleVerticalMotion(TickState st, TickActor mo, double oldFloorZ)
    {
        var passMobj = ((mo.Flags2 & Mf2Passmobj) != 0 || (mo.Flags & MfSpecial) != 0)
            && (st.LevelCompatFlags & CompatfNoPassmobj) == 0;
        if (passMobj)
        {
            var onmoObj = st.CheckOnMobj?.Invoke(mo);
            if (onmoObj is null)
            {
                st.ZMovement?.Invoke(mo, oldFloorZ);
                mo.Flags2 &= ~Mf2Onmobj;
            }
            else
                HandleOnMobj(st, mo, onmoObj, oldFloorZ);
        }
        else
            st.ZMovement?.Invoke(mo, oldFloorZ);

        if (mo.EuthanizeMe)
            mo.Destroyed = true;
    }

    static void HandleOnMobj(TickState st, TickActor mo, object onmoObj, double oldFloorZ)
    {
        if (onmoObj is not OnMobjActor onmo)
            return;

        if (mo.HasPlayer && mo.VelZ < st.Actor.Gravity * mo.SectorGravity * (-1.0 / 100)
            && (mo.Flags & MfNogravity) == 0)
            st.PlayerLandedOnThing?.Invoke(mo, onmo);

        if (onmo.Top - mo.Z <= mo.MaxStepHeight)
        {
            if (mo.HasPlayer)
                st.AdjustViewOnMobj?.Invoke(mo, onmo.Top);
            mo.Z = onmo.Top;
        }

        if ((onmo.Flags6 & Mf6Bumpspecial) != 0 && BumpMayActivate(st, mo, onmo))
        {
            if (!mo.HasPlayer || (mo.PlayerCheats & CfPredicting) == 0)
            {
                if (st.ActivateBumpSpecial?.Invoke(mo, onmo) == true)
                    onmo.LastBump = st.MapTime + st.TicRate;
            }
        }

        if (mo.VelZ != 0 && (mo.BounceFlags & TickActor.BounceActors) != 0)
        {
            if ((mo.Flags & MfMissile) != 0)
                st.DoMissileDamage?.Invoke(mo, onmoObj);
            if (st.BounceActor?.Invoke(mo, onmo) != true && (mo.Flags & MfMissile) != 0)
                st.ExplodeMissile?.Invoke(mo, onmoObj);
        }
        else
        {
            mo.Flags2 |= Mf2Onmobj;
            mo.VelZ = 0;
            st.Crash?.Invoke(mo);
        }
    }

    static bool BumpMayActivate(TickState st, TickActor mo, OnMobjActor onmo)
    {
        if (mo.HasPlayer)
            return VmPMapPitCheckThing.BumpSpecialThrottle(st.MapTime, onmo.LastBump);
        if ((onmo.ActivationType & ThingspecMonsterTrigger) != 0 && (mo.Flags3 & Mf3Ismonster) != 0)
            return VmPMapPitCheckThing.BumpSpecialThrottle(st.MapTime, onmo.LastBump);
        if ((onmo.ActivationType & ThingspecMissileTrigger) != 0 && (mo.Flags & MfMissile) != 0)
            return VmPMapPitCheckThing.BumpSpecialThrottle(st.MapTime, onmo.LastBump);
        return false;
    }

    public static (double X, double Y) AverageSectorScroll(
        bool hasPlayer,
        int compatFlags,
        int countX,
        int countY,
        double cummX,
        double cummY)
    {
        if (hasPlayer || (compatFlags & CompatfBoomscroll) == 0)
        {
            if (countX > 1) cummX /= countX;
            if (countY > 1) cummY /= countY;
        }
        return (cummX, cummY);
    }

    public static bool MissileNeedsMinVelocity(int flags, double velX, double velY, bool zeroDamage) =>
        (flags & MfMissile) != 0 && velX == 0 && velY == 0 && !zeroDamage;

    public static bool ShouldApplySteepSlopePush(
        int flags,
        double velZ,
        double z,
        double floorZ,
        double planeFC,
        double maxSlope) =>
        (flags & MfSolid) != 0
        && (flags & (MfNoclip | MfNogravity | MfNoblockmap)) == 0
        && velZ <= 0
        && z == floorZ
        && planeFC < maxSlope;
}
