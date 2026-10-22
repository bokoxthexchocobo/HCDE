namespace HCDE.Playsim;

/// <summary>Whole <c>AActor::Tick</c> from <c>p_mobj.cpp</c> (4523–5075).</summary>
public static class VmPMobjActorTick
{
    public const int Mf5Nointeraction = 1;
    public const int MfUnmorphed = 1 << 1;
    public const int MfMissile = 1 << 2;
    public const int MfSpecial = 1 << 3;
    public const int MfNoclip = 1 << 4;
    public const int MfNosector = 1 << 5;
    public const int MfNoblockmap = 1 << 6;
    public const int MfNogravity = 1 << 7;
    public const int MfSolid = 1 << 8;
    public const int MfStealth = 1 << 9;
    public const int Mf2Passmobj = 1;
    public const int Mf2Blasted = 1 << 1;
    public const int Mf3Ismonster = 1 << 2;
    public const int Mf4Vfriction = 1 << 3;
    public const int Mf6Bosscube = 1 << 4;
    public const int Mf6Touchy = 1 << 5;
    public const int Mf6Armed = 1 << 6;
    public const int Mf8Inscrollsec = 1;
    public const int Mf9Decoupledanimations = 1;
    public const int FxRocket = 1;
    public const int FxGrenade = 1 << 1;
    public const int FxVisibilitypulse = 1 << 2;
    public const int CfPredicting = 1;
    public const int CompatfBoomscroll = 1;
    public const int CompatfRavenscroll = 1 << 1;
    public const int CompatfNoPassmobj = 1 << 2;

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
        public int VisDir { get; set; }
        public int SmokeCounter { get; set; }
        public int Special2 { get; set; }
        public bool HasPlayer { get; set; }
        public int PlayerCheats { get; set; }
        public bool Destroyed { get; set; }
        public bool EuthanizeMe { get; set; }
        public object? State { get; set; }
        public int Freezetics { get; set; }
        public bool Alternative { get; set; }
        public bool Frozen { get; set; }
        public bool ZeroDamage { get; set; }
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

    public sealed class TickState
    {
        public required TickActor Actor { get; init; }
        public int LevelCompatFlags { get; init; }
        public bool CoopAuthorityVisual { get; init; }
        public bool InvasionMirrorBlocking { get; init; }
        public Func<bool>? CheckUnmorph { get; init; }
        public Action? TickBehaviors { get; init; }
        public Action? WalkInventoryDoEffect { get; init; }
        public Action? SpawnRocketSmoke { get; init; }
        public Action? SpawnGrenadeSmoke { get; init; }
        public Action? BotTick { get; init; }
        public Func<TickActor, double, double, double>? AverageScroll { get; init; }
        public Func<TickActor, double>? XYMovement { get; init; }
        public Func<TickActor, double, bool>? CheckOnMobj { get; init; }
        public Action<TickActor, double>? ZMovement { get; init; }
        public Action<TickActor, object?>? PlayerLandedOnThing { get; init; }
        public Action<TickActor>? Crash { get; init; }
        public Action<TickActor, object?>? ActivateBumpSpecial { get; init; }
        public Func<TickActor, object?, bool>? BounceActor { get; init; }
        public Action<TickActor, object?>? ExplodeMissile { get; init; }
        public Action<TickActor, object?>? DoMissileDamage { get; init; }
        public Action<TickActor, object?>? TriggerSecActFloor { get; init; }
        public Action<TickActor, object?>? TriggerSecActCeiling { get; init; }
        public Action<TickActor>? VelFromAngleMin { get; init; }
    }

    public static void Tick(TickState st)
    {
        var mo = st.Actor;
        if (mo.Freezetics > 0)
        {
            mo.Freezetics--;
            return;
        }

        if (mo.Alternative && !mo.HasPlayer)
        {
            if (st.CheckUnmorph?.Invoke() == true)
                return;
        }

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
            _ = false;

        if (!mo.HasPlayer || (mo.PlayerCheats & CfPredicting) == 0)
            st.WalkInventoryDoEffect?.Invoke();

        if ((mo.Flags & MfUnmorphed) != 0)
            return;

        if (mo.Frozen)
        {
            HandleFrozenBossCube(mo);
            return;
        }

        HandleRocketGrenadeTrail(st, mo);
        ApplyVerticalFriction(mo);
        PulseVisibility(mo);
        st.BotTick?.Invoke(mo);

        var scroll = st.AverageScroll?.Invoke(mo, mo.CummScrollX, mo.CummScrollY) ?? (0.0, 0.0);
        ApplySteepSlopePush(mo);

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

        if (mo.BlockingFloor != null && !ReferenceEquals(mo.BlockingFloor, mo.OldBlockingFloor))
            st.TriggerSecActFloor?.Invoke(mo, mo.BlockingFloor);
        if (mo.BlockingCeiling != null && !ReferenceEquals(mo.BlockingCeiling, mo.OldBlockingCeiling))
            st.TriggerSecActCeiling?.Invoke(mo, mo.BlockingCeiling);

        if (mo.VelX == 0 && mo.VelY == 0)
            HandleRestingActor(mo);

        if (mo.VelZ != 0 || mo.BlockingMobj != null || mo.Z != mo.FloorZ)
            HandleVerticalMotion(st, mo, oldFloorZ);
    }

    static void TickNoInteraction(TickState st, TickActor mo)
    {
        if (mo.Frozen)
        {
            HandleFrozenBossCube(mo);
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
                mo.Flags |= MfNoblockmap;
        }
        mo.Flags8 &= ~Mf8Inscrollsec;
    }

    static void HandleFrozenBossCube(TickActor mo)
    {
        if ((mo.Flags6 & Mf6Bosscube) != 0)
            mo.Special2++;
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

    static void PulseVisibility(TickActor mo)
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
                mo.Alpha += 2.0 / 35;
                if (mo.Alpha > 1) { mo.Alpha = 1; mo.VisDir = 0; }
            }
            else if (mo.VisDir < 0)
            {
                mo.Alpha -= 1.5 / 35;
                if (mo.Alpha < mo.StealthAlpha) { mo.Alpha = mo.StealthAlpha; mo.VisDir = 0; }
            }
        }
    }

    static void ApplySteepSlopePush(TickActor mo)
    {
        if ((mo.Flags & MfSolid) == 0 || (mo.Flags & (MfNoclip | MfNogravity | MfNoblockmap)) != 0)
            return;
        if (mo.VelZ > 0 || mo.Z != mo.FloorZ)
            return;
        _ = mo.FloorZ;
    }

    static void HandleRestingActor(TickActor mo)
    {
        if ((mo.Flags2 & Mf2Blasted) != 0)
            mo.Flags2 &= ~Mf2Blasted;
        if ((mo.Flags6 & Mf6Touchy) != 0)
            mo.Flags6 |= Mf6Armed;
    }

    static void HandleVerticalMotion(TickState st, TickActor mo, double oldFloorZ)
    {
        var passMobj = ((mo.Flags2 & Mf2Passmobj) != 0 || (mo.Flags & MfSpecial) != 0);
        if (passMobj)
        {
            var onmo = st.CheckOnMobj?.Invoke(mo);
            if (onmo == null)
            {
                st.ZMovement?.Invoke(mo, oldFloorZ);
                mo.Flags2 &= ~1;
            }
            else
                HandleOnMobj(st, mo, onmo, oldFloorZ);
        }
        else
            st.ZMovement?.Invoke(mo, oldFloorZ);

        if (mo.EuthanizeMe)
            mo.Destroyed = true;
    }

    static void HandleOnMobj(TickState st, TickActor mo, object onmo, double oldFloorZ)
    {
        if (mo.VelZ != 0 && (mo.BounceFlags & TickActor.BounceActors) != 0)
        {
            if ((mo.Flags & MfMissile) != 0)
                st.DoMissileDamage?.Invoke(mo, onmo);
            if (st.BounceActor?.Invoke(mo, onmo) != true && (mo.Flags & MfMissile) != 0)
                st.ExplodeMissile?.Invoke(mo, onmo);
        }
        else
        {
            mo.Flags2 |= Mf2Passmobj;
            mo.VelZ = 0;
            st.Crash?.Invoke(mo);
        }
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
}
