namespace HCDE.Playsim;

/// <summary>Whole static <c>P_ZMovement</c> from <c>p_mobj.cpp</c> (2955–3212).</summary>
public static class VmPMobjZMovement
{
    public const int MfFloat = 1;
    public const int MfMissile = 1 << 1;
    public const int MfNoclip = 1 << 2;
    public const int MfNogravity = 1 << 3;
    public const int MfSkullfly = 1 << 4;
    public const int Mf2Dormant = 1;
    public const int Mf2Floatbob = 1 << 1;
    public const int Mf2Floorclip = 1 << 2;
    public const int Mf3Ismonster = 1;
    public const int Mf3Noexplodefloor = 1 << 1;
    public const int Mf3Floorhugger = 1 << 2;
    public const int Mf3Ceilinghugger = 1 << 3;
    public const int Mf3Skyexplode = 1 << 4;
    public const int Mf5Nodropoff = 1;
    public const int Mf7Smashable = 1;
    public const int Mf8Nofriction = 1;
    public const int BounceFloors = 1;
    public const int BounceCeilings = 1 << 1;
    public const int BounceMbf = 1 << 2;
    public const int CfPredicting = 1;
    public const double FrictionFly = 0xE800 / 65536.0;
    public const double ZUnderflow = -32768;

    public sealed class ZActor
    {
        public double Z { get; set; }
        public double Height { get; set; }
        public double Top => Z + Height;
        public double VelZ { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double DropoffZ { get; set; }
        public double FloatSpeed { get; set; }
        public double LandingSpeed { get; set; } = 8;
        public double SpecialF1 { get; set; }
        public double FlyBob { get; set; }
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags5 { get; set; }
        public int Flags7 { get; set; }
        public int Flags8 { get; set; }
        public int BounceFlags { get; set; }
        public int WaterLevel { get; set; }
        public int Health { get; set; }
        public int Tics { get; set; }
        public string DamageType { get; set; } = "";
        public bool HasPlayer { get; set; }
        public int PlayerCheats { get; set; }
        public double ViewHeight { get; set; }
        public double DeltaViewHeight { get; set; }
        public int JumpTics { get; set; }
        public object? Target { get; set; }
        public double TargetCenterZ { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public object? BlockingFloor { get; set; }
        public object? BlockingCeiling { get; set; }
        public object? FloorPic { get; set; }
        public object? CeilingPic { get; set; }
        public object? SecActTarget { get; set; }
        public bool Destroyed { get; set; }
        public bool NoClip2 { get; set; }
        public bool FViewBob { get; set; } = true;
        public int MapTime { get; set; }
        public int CompatFlags2EmulateMikoPortals { get; set; }
        public int BcompatFloatbob { get; set; }
        public object? SkyFlat { get; set; }
    }

    public sealed class ZMovementState
    {
        public required ZActor Actor { get; init; }
        public double OldFloorZ { get; init; }
        public double Gravity { get; init; } = 1;
        public Action<ZActor, double, double>? SmoothStepUpView { get; init; }
        public Action<ZActor, double, double>? FallAndSink { get; init; }
        public Func<ZActor, object?, bool>? FloorBounceMissile { get; init; }
        public Action<ZActor>? HitFloor { get; init; }
        public Action<ZActor, int>? ProjectileHitPlane { get; init; }
        public Action<ZActor, object?, object?, bool>? ExplodeMissile { get; init; }
        public Action<ZActor>? Destroy { get; init; }
        public Action<ZActor, string>? TriggerSectorAction { get; init; }
        public Action<ZActor, double, bool>? Check3DFloorHit { get; init; }
        public Action<ZActor, double, bool>? Check3DCeilingHit { get; init; }
        public Action<ZActor>? MonsterFallingDamage { get; init; }
        public Action<ZActor, int, string?>? DamageMobj { get; init; }
        public Action<ZActor, object?>? PlayerLanded { get; init; }
        public Action<ZActor>? Crash { get; init; }
        public Action<ZActor, double>? AdjustFloorClip { get; init; }
        public Action<ZActor, double>? CheckFakeFloorTriggers { get; init; }
        public Func<ZActor, double>? SectorFriction { get; init; }
        public Func<ZActor, double, double>? FlyBobOffset { get; init; }
    }

    public static void ZMovement(ZMovementState st)
    {
        var mo = st.Actor;
        var oldZ = mo.Z;

        if (mo.HasPlayer && mo.Z < mo.FloorZ)
            st.SmoothStepUpView?.Invoke(mo, mo.FloorZ - mo.Z, mo.DeltaViewHeight);

        mo.Z += mo.VelZ;

        if ((mo.CompatFlags2EmulateMikoPortals & 1) != 0 && mo.Z < ZUnderflow)
            mo.Z = mo.CeilingZ;

        st.FallAndSink?.Invoke(mo, st.Gravity, st.OldFloorZ);

        if (mo.SpecialF1 > 0 && (mo.Flags2 & Mf2Floatbob) != 0 && mo.BcompatFloatbob != 0)
            mo.Z = mo.FloorZ + mo.SpecialF1;

        FloatTowardTarget(st, mo);

        if (mo.HasPlayer && (mo.Flags & MfNogravity) != 0 && mo.Z > mo.FloorZ)
        {
            if (!mo.NoClip2 && mo.FViewBob)
                mo.Z += st.FlyBobOffset?.Invoke(mo, mo.MapTime) ?? 0;
            if ((mo.Flags8 & Mf8Nofriction) == 0)
                mo.VelZ *= FrictionFly;
        }

        if (mo.WaterLevel > 0 && (mo.Flags & MfNogravity) == 0 && (mo.Flags8 & Mf8Nofriction) == 0)
        {
            var friction = st.SectorFriction?.Invoke(mo) ?? FrictionFly;
            mo.VelZ *= friction;
        }

        if (mo.Z <= mo.FloorZ)
            HandleFloorClip(st, mo);

        if ((mo.Flags2 & Mf2Floorclip) != 0)
            st.AdjustFloorClip?.Invoke(mo, mo.Z);

        if (mo.Top > mo.CeilingZ)
            HandleCeilingClip(st, mo);

        st.CheckFakeFloorTriggers?.Invoke(mo, oldZ);
    }

    static void FloatTowardTarget(ZMovementState st, ZActor mo)
    {
        if ((mo.Flags & MfFloat) == 0 || (mo.Flags2 & Mf2Dormant) != 0 || mo.Target == null)
            return;
        if ((mo.Flags & MfSkullfly) != 0)
            return;
        var dist = Distance2D(mo.X, mo.Y, mo.TargetX, mo.TargetY);
        var delta = mo.TargetCenterZ - mo.Z;
        if (delta < 0 && dist < -(delta * 3))
            mo.Z -= mo.FloatSpeed;
        else if (delta > 0 && dist < delta * 3)
            mo.Z += mo.FloatSpeed;
    }

    static void HandleFloorClip(ZMovementState st, ZActor mo)
    {
        if ((!mo.HasPlayer || (mo.PlayerCheats & CfPredicting) == 0)
            && mo.SecActTarget != null)
            st.TriggerSectorAction?.Invoke(mo, "HitFloor");

        st.Check3DFloorHit?.Invoke(mo, mo.FloorZ, true);
        if (mo.Z > mo.FloorZ)
            return;

        mo.BlockingFloor = mo.SecActTarget;
        if ((mo.Flags & MfMissile) != 0 && (mo.Flags & MfNoclip) == 0)
        {
            mo.Z = mo.FloorZ;
            if ((mo.BounceFlags & BounceFloors) != 0)
            {
                if (st.FloorBounceMissile?.Invoke(mo, null) == true)
                    return;
            }
            else if ((mo.Flags3 & Mf3Noexplodefloor) != 0)
            {
                st.HitFloor?.Invoke(mo);
                mo.VelZ = 0;
                return;
            }
            else if ((mo.Flags3 & Mf3Floorhugger) != 0 && (mo.Flags5 & Mf5Nodropoff) == 0)
                return;
            else
            {
                var onSky = ReferenceEquals(mo.FloorPic, mo.SkyFlat);
                if (onSky && (mo.Flags3 & Mf3Skyexplode) == 0)
                {
                    st.Destroy?.Invoke(mo);
                    mo.Destroyed = true;
                    return;
                }
                st.HitFloor?.Invoke(mo);
                st.ProjectileHitPlane?.Invoke(mo, 0);
                st.ExplodeMissile?.Invoke(mo, null, null, onSky && (mo.Flags3 & Mf3Skyexplode) != 0);
                mo.Destroyed = true;
                return;
            }
        }
        else if ((mo.BounceFlags & BounceMbf) != 0 && mo.VelZ != 0)
            st.FloorBounceMissile?.Invoke(mo, null);

        if ((mo.Flags3 & Mf3Ismonster) != 0 && mo.VelZ < -23)
            st.MonsterFallingDamage?.Invoke(mo);

        mo.Z = mo.FloorZ;
        if (mo.VelZ < 0)
        {
            st.HitFloor?.Invoke(mo);
            if (mo.DamageType == "Ice" && mo.VelZ < mo.LandingSpeed)
            {
                mo.Tics = 1;
                mo.VelZ = 0;
                return;
            }
            if ((mo.Flags7 & Mf7Smashable) != 0)
                st.DamageMobj?.Invoke(mo, mo.Health, "Smash");
            if (mo.HasPlayer)
            {
                if (mo.JumpTics < 0 || mo.VelZ < mo.LandingSpeed)
                    mo.JumpTics = 7;
                if (mo.VelZ < mo.LandingSpeed && (mo.Flags & MfNogravity) == 0)
                    st.PlayerLanded?.Invoke(mo, null);
            }
            mo.VelZ = 0;
        }

        if ((mo.Flags & MfSkullfly) != 0)
            mo.VelZ = -mo.VelZ;

        st.Crash?.Invoke(mo);
    }

    static void HandleCeilingClip(ZMovementState st, ZActor mo)
    {
        if ((!mo.HasPlayer || (mo.PlayerCheats & CfPredicting) == 0) && mo.SecActTarget != null)
            st.TriggerSectorAction?.Invoke(mo, "HitCeiling");

        st.Check3DCeilingHit?.Invoke(mo, mo.CeilingZ, true);
        if (mo.Top <= mo.CeilingZ)
            return;

        mo.BlockingCeiling = mo.SecActTarget;
        mo.Z = mo.CeilingZ - mo.Height;
        if ((mo.BounceFlags & BounceCeilings) != 0)
        {
            st.FloorBounceMissile?.Invoke(mo, null);
            return;
        }

        if ((mo.Flags & MfSkullfly) != 0)
            mo.VelZ = -mo.VelZ;
        if (mo.VelZ > 0)
            mo.VelZ = 0;

        if ((mo.Flags & MfMissile) != 0 && (mo.Flags & MfNoclip) == 0)
        {
            if ((mo.Flags3 & Mf3Ceilinghugger) != 0)
                return;
            var onSky = ReferenceEquals(mo.CeilingPic, mo.SkyFlat);
            if (onSky && (mo.Flags3 & Mf3Skyexplode) == 0)
            {
                st.Destroy?.Invoke(mo);
                mo.Destroyed = true;
                return;
            }
            st.ProjectileHitPlane?.Invoke(mo, 1);
            st.ExplodeMissile?.Invoke(mo, null, null, onSky);
            mo.Destroyed = true;
        }
    }

    static double Distance2D(double x1, double y1, double x2, double y2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static bool SmoothStepUpApplies(bool hasPlayer, double z, double floorZ) =>
        hasPlayer && z < floorZ;

    public static bool MissileFloorExplode(int flags, int flags3, int flags5, int bounceFlags) =>
        (flags & MfMissile) != 0
        && (bounceFlags & BounceFloors) == 0
        && (flags3 & Mf3Noexplodefloor) == 0
        && !((flags3 & Mf3Floorhugger) != 0 && (flags5 & Mf5Nodropoff) == 0);
}
