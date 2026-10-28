namespace HCDE.Playsim;

/// <summary><c>P_ExplodeMissile</c> and <c>AActor::FloorBounceMissile</c> from <c>p_mobj.cpp</c> (1958–2096, 2139–2274).</summary>
public static class VmPMobjExplodeBounce
{
    public const int Mf3Explocount = 1;
    public const int Mf3Skyexplode = 1 << 1;
    public const int Mf3Fullvoldeath = 1 << 2;
    public const int Mf5Dehexplosion = 1;
    public const int Mf4Randomize = 1;
    public const int MfMissile = 1;
    public const int MfShootable = 1 << 1;
    public const int Mf7Hittarget = 1;
    public const int Mf7Hitmaster = 1 << 1;
    public const int Mf7Hittracer = 1 << 2;
    public const int MfCorpse = 1 << 3;
    public const int Mf6Killed = 1 << 4;
    public const int MfNoblood = 1 << 5;
    public const int MfInbounce = 1 << 6;
    public const int MfNogravity = 1 << 7;
    public const int BounceFloors = 1;
    public const int BounceCeilings = 1 << 1;
    public const int BounceNotOnSky = 1 << 2;
    public const int BounceHereticType = 1 << 3;
    public const int BounceMbf = 1 << 4;
    public const int BounceKeepAngle = 1 << 5;
    public const int BounceModifyPitch = 1 << 6;
    public const int BounceUseBounceState = 1 << 7;
    public const int BounceAutoOff = 1 << 8;
    public const int BounceAutoOffFloorOnly = 1 << 9;
    public const int BounceExplodeOnWater = 1 << 10;
    public const int BounceCanBounceWater = 1 << 11;
    public const int BounceTypeMask = 0xff;

    public sealed class MissileActor
    {
        public int Threshold { get; set; }
        public int DefThreshold { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public int Effects { get; set; }
        public int Flags { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public int Flags7 { get; set; }
        public int BounceFlags { get; set; }
        public int BounceCount { get; set; }
        public double BounceFactor { get; set; } = 0.7;
        public double Mass { get; set; } = 100;
        public double Speed { get; set; }
        public double Z { get; set; }
        public double FloorZ { get; set; }
        public object? FloorPic { get; set; }
        public object? CeilingPic { get; set; }
        public object? Target { get; set; }
        public object? Master { get; set; }
        public object? Tracer { get; set; }
        public object? DecalGenerator { get; set; }
        public bool DeathSoundValid { get; set; }
        public bool EuthanizeMe { get; set; }
        public int Tics { get; set; } = 5;
        public double Alpha { get; set; } = 1;
        public int RenderStyle { get; set; }
        public string? NextState { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class ExplodeContext
    {
        public required MissileActor Missile { get; init; }
        public object? Line { get; init; }
        public object? HitTarget { get; init; }
        public int TargetFlags { get; init; }
        public int TargetFlags6 { get; init; }
        public bool OnSky { get; init; }
        public string DamageType { get; init; } = "";
        public bool MissileDecals { get; init; }
        public int LineHorizonSpecial { get; init; } = 27;
        public int LineSpecial { get; init; }
        public bool AddRocketExplosion { get; init; }
        public int DehExplosionStyle { get; init; } = 255;
        public double DehExplosionAlpha { get; init; } = 0.6666;
        public Action<MissileActor, object?>? ProjectileHitLinedef { get; init; }
        public Func<MissileActor, string, string?>? FindState { get; init; }
        public Action<MissileActor>? Destroy { get; init; }
        public Action<MissileActor, object?, int, object?>? CreateDecal { get; init; }
        public Action<MissileActor>? PlayDeathSound { get; init; }
        public Action<MissileActor, string?>? SetState { get; init; }
        public Func<int>? ExplodeMissileRandom { get; init; }
    }

    public static bool ExplodeMissile(ExplodeContext ctx)
    {
        var mo = ctx.Missile;
        if (ctx.Line != null)
            ctx.ProjectileHitLinedef?.Invoke(mo, ctx.Line);

        if ((mo.Flags3 & Mf3Explocount) != 0)
        {
            if (++mo.Threshold < mo.DefThreshold)
                return false;
        }

        mo.VelX = mo.VelY = mo.VelZ = 0;
        mo.Effects = 0;
        mo.Flags &= ~MfShootable;

        string? nextState = null;
        if (ctx.HitTarget != null)
        {
            if ((mo.Flags7 & Mf7Hittarget) != 0) mo.Target = ctx.HitTarget;
            if ((mo.Flags7 & Mf7Hitmaster) != 0) mo.Master = ctx.HitTarget;
            if ((mo.Flags7 & Mf7Hittracer) != 0) mo.Tracer = ctx.HitTarget;
            nextState = PickTargetDeathState(mo, ctx);
        }

        if (nextState == null)
            nextState = ctx.FindState?.Invoke(mo, ctx.DamageType);

        if (ctx.OnSky || (ctx.Line != null && ctx.LineSpecial == ctx.LineHorizonSpecial))
        {
            if ((mo.Flags3 & Mf3Skyexplode) == 0)
            {
                ctx.Destroy?.Invoke(mo);
                mo.Destroyed = true;
                return true;
            }
            nextState = ctx.FindState?.Invoke(mo, "Sky");
        }

        if (ctx.Line != null && ctx.MissileDecals && mo.DecalGenerator != null)
            ctx.CreateDecal?.Invoke(mo, ctx.Line, 0, null);

        if (mo.DeathSoundValid)
            ctx.PlayDeathSound?.Invoke(mo);

        ctx.SetState?.Invoke(mo, nextState);
        if (mo.EuthanizeMe)
            return true;

        if ((mo.Flags5 & Mf5Dehexplosion) != 0)
            ApplyDehExplosionStyle(mo, ctx);

        if ((mo.Flags4 & Mf4Randomize) != 0)
        {
            var rnd = ctx.ExplodeMissileRandom?.Invoke() ?? 0;
            mo.Tics -= (rnd & 3) * 35 / 35;
            if (mo.Tics < 1) mo.Tics = 1;
        }

        mo.Flags &= ~MfMissile;
        return true;
    }

    static string? PickTargetDeathState(MissileActor mo, ExplodeContext ctx)
    {
        if ((ctx.TargetFlags & (MfShootable | MfCorpse)) != 0 || (ctx.TargetFlags6 & Mf6Killed) != 0)
        {
            if ((ctx.TargetFlags & MfNoblood) != 0)
                return ctx.FindState?.Invoke(mo, "Crash");
            return ctx.FindState?.Invoke(mo, "Death.Extreme");
        }
        return null;
    }

    static void ApplyDehExplosionStyle(MissileActor mo, ExplodeContext ctx)
    {
        if (ctx.DehExplosionStyle == 255)
        {
            if (ctx.AddRocketExplosion)
            {
                mo.RenderStyle = 1;
                mo.Alpha = 1;
            }
            else
            {
                mo.RenderStyle = 2;
                mo.Alpha = 0.6666;
            }
        }
        else
        {
            mo.RenderStyle = ctx.DehExplosionStyle;
            mo.Alpha = ctx.DehExplosionAlpha;
        }
    }

    public sealed class FloorBounceContext
    {
        public required MissileActor Missile { get; init; }
        public double NormX { get; init; }
        public double NormY { get; init; }
        public double NormZ { get; init; }
        public bool Is3DFloor { get; init; }
        public double Gravity { get; init; } = 1;
        public Func<MissileActor, int, int>? SpecialBounceHit { get; init; }
        public Func<MissileActor, int, bool>? ProjectileHitPlane { get; init; }
        public Func<MissileActor, bool>? HitFloor { get; init; }
        public Action<MissileActor>? ExplodeMissile { get; init; }
        public Action<MissileActor>? CallDie { get; init; }
        public Action<MissileActor>? Destroy { get; init; }
        public Action<MissileActor, bool, double>? PlayBounceSound { get; init; }
        public Func<MissileActor, string?>? FindBounceState { get; init; }
        public Action<MissileActor, string?>? SetState { get; init; }
        public Func<MissileActor, double>? MbfBounceFactor { get; init; }
        public Func<MissileActor, double, double, double>? AngleFromVel { get; init; }
    }

    /// <summary>Returns true when the missile was destroyed.</summary>
    public static bool FloorBounceMissile(FloorBounceContext ctx)
    {
        var self = ctx.Missile;
        if ((self.Flags & MfMissile) != 0)
        {
            var hit = ctx.SpecialBounceHit?.Invoke(self, -1) ?? -1;
            if (hit == 1) return false;
            if (hit == 0) return true;
        }

        if (ctx.ProjectileHitPlane?.Invoke(self, -1) == true && self.BounceCount > 0)
        {
            self.VelX = self.VelY = self.VelZ = 0;
            self.Speed = 0;
            self.BounceCount = 0;
            if ((self.Flags & MfMissile) != 0)
                ctx.ExplodeMissile?.Invoke(self);
            else
                ctx.CallDie?.Invoke(self);
            return true;
        }

        if (self.Z <= self.FloorZ && ctx.HitFloor?.Invoke(self) == true)
        {
            if ((self.BounceFlags & BounceExplodeOnWater) != 0)
            {
                if ((self.Flags & MfMissile) != 0) ctx.ExplodeMissile?.Invoke(self);
                else ctx.CallDie?.Invoke(self);
                return true;
            }
            if ((self.BounceFlags & BounceCanBounceWater) == 0)
            {
                ctx.Destroy?.Invoke(self);
                self.Destroyed = true;
                return true;
            }
        }

        var normZ = ctx.Is3DFloor ? -ctx.NormZ : ctx.NormZ;
        var onSky = normZ < 0
            ? self.CeilingPic != null && ReferenceEquals(self.CeilingPic, self.FloorPic)
            : self.FloorPic != null;

        if (normZ < 0)
        {
            if ((self.BounceFlags & BounceCeilings) == 0) return true;
        }
        else if ((self.BounceFlags & BounceFloors) == 0)
            return true;

        if (onSky && (self.BounceFlags & BounceNotOnSky) != 0)
        {
            ctx.Destroy?.Invoke(self);
            self.Destroyed = true;
            return true;
        }

        if (self.BounceCount > 0 && --self.BounceCount == 0)
        {
            if ((self.Flags & MfMissile) != 0) ctx.ExplodeMissile?.Invoke(self);
            else ctx.CallDie?.Invoke(self);
            return true;
        }

        var dot = self.VelX * ctx.NormX + self.VelY * ctx.NormY + self.VelZ * normZ;
        dot *= 2;
        if ((self.BounceFlags & (BounceHereticType | BounceMbf)) != 0)
        {
            self.VelX -= ctx.NormX * dot;
            self.VelY -= ctx.NormY * dot;
            self.VelZ -= normZ * dot;
            if ((self.BounceFlags & BounceKeepAngle) == 0)
                ctx.AngleFromVel?.Invoke(self, self.VelX, self.VelY);
            if ((self.BounceFlags & BounceMbf) == 0)
            {
                self.Flags |= MfInbounce;
                ctx.SetState?.Invoke(self, ctx.FindBounceState?.Invoke(self));
                self.Flags &= ~MfInbounce;
                return false;
            }
            self.VelZ *= ctx.MbfBounceFactor?.Invoke(self) ?? 1;
        }
        else
        {
            self.VelX = (self.VelX - ctx.NormX * dot) * self.BounceFactor;
            self.VelY = (self.VelY - ctx.NormY * dot) * self.BounceFactor;
            self.VelZ = (self.VelZ - normZ * dot) * self.BounceFactor;
            if ((self.BounceFlags & BounceKeepAngle) == 0)
                ctx.AngleFromVel?.Invoke(self, self.VelX, self.VelY);
        }

        ctx.PlayBounceSound?.Invoke(self, true, 1.0);

        if ((self.BounceFlags & BounceUseBounceState) != 0)
            ctx.SetState?.Invoke(self, ctx.FindBounceState?.Invoke(self));

        if ((self.BounceFlags & BounceMbf) != 0)
        {
            if (Math.Abs(self.VelZ) < self.Mass * ctx.Gravity / 64)
                self.VelZ = 0;
        }
        else if ((self.BounceFlags & (BounceAutoOff | BounceAutoOffFloorOnly)) != 0)
        {
            if (normZ > 0 || (self.BounceFlags & BounceAutoOff) != 0)
            {
                if ((self.Flags & MfNogravity) == 0 && (normZ < 0 || dot < 3))
                    self.BounceFlags &= ~BounceTypeMask;
            }
        }

        return false;
    }

    public static bool ExplodeCountGated(int flags3, int threshold, int defThreshold) =>
        (flags3 & Mf3Explocount) != 0 && ++threshold < defThreshold;
}
