using HCDE.Gamedata;

namespace HCDE.Playsim;

/// <summary>Native <c>p_actionfunctions.cpp</c> combat pointers (melee, combo, hitscan, spawn projectile).</summary>
public static class ActorCombatActions
{
    private const double MissileRange = 2048;

    [Flags]
    public enum SpawnProjectileFlags
    {
        AimModeMask = 3,
        TrackOwner = 4,
        CheckTargetDead = 8,
        AbsolutePitch = 16,
        OffsetPitch = 32,
        SavePitch = 64,
        AbsoluteAngle = 128,
        BadPitch = 256,
    }

    public static Actor? CustomMeleeAttack(Actor actor, int damage, string? meleeSound = null, string? missSound = null,
        string? damageType = null, bool bleed = false)
    {
        var target = CombatTarget(actor);
        if (target is null) return null;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Melee attack requires a simulation.");
        ActorOrientationActions.FaceTarget(actor);
        if (MonsterBrain.CheckNativeMeleeRange(sim, actor, target))
        {
            if (!string.IsNullOrEmpty(meleeSound) && !meleeSound.Equals("None", StringComparison.OrdinalIgnoreCase))
                ActorSoundActions.PlayWeaponSound(actor, meleeSound);
            var type = string.IsNullOrEmpty(damageType) || damageType.Equals("None", StringComparison.OrdinalIgnoreCase)
                ? "Melee" : damageType;
            ActorDamage.Apply(target, damage, actor, damageType: type, inflictor: actor);
        }
        else if (!string.IsNullOrEmpty(missSound) && !missSound.Equals("None", StringComparison.OrdinalIgnoreCase))
            ActorSoundActions.PlayWeaponSound(actor, missSound);
        return null;
    }

    public static ProjectileActor? CustomComboAttack(Actor actor, string? projectileClass, double spawnHeight, int damage,
        string? meleeSound = null, string? damageType = null, bool bleed = false)
    {
        var target = CombatTarget(actor);
        if (target is null) return null;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Combo attack requires a simulation.");
        ActorOrientationActions.FaceTarget(actor);
        if (MonsterBrain.CheckNativeMeleeRange(sim, actor, target))
        {
            var type = string.IsNullOrEmpty(damageType) || damageType.Equals("None", StringComparison.OrdinalIgnoreCase)
                ? "Melee" : damageType;
            if (!string.IsNullOrEmpty(meleeSound) && !meleeSound.Equals("None", StringComparison.OrdinalIgnoreCase))
                ActorSoundActions.PlayWeaponSound(actor, meleeSound);
            ActorDamage.Apply(target, damage, actor, damageType: type, inflictor: actor);
            return null;
        }
        if (string.IsNullOrEmpty(projectileClass) || projectileClass.Equals("None", StringComparison.OrdinalIgnoreCase))
            return null;
        return SpawnProjectile(actor, projectileClass, spawnHeight, spawnOffsetXy: 0, angleOffset: default,
            flags: SpawnProjectileFlags.AimModeMask, pitch: default, pointerSelector: AcsActorPointer.Target, reference: target);
    }

    [Flags]
    public enum CustomBulletFlags
    {
        NoRandomPuffZ = 1,
        AimFacing = 2,
        NoPitch = 4,
        ExplicitAngle = 8,
        NoRandom = 16,
    }

    /// <summary>Deprecated alias that matches ZScript <c>A_CustomMissile</c> (forces <c>CMF_BADPITCH</c>).</summary>
    public static ProjectileActor? CustomMissile(Actor actor, string projectileClass, double spawnHeight = 32,
        double spawnOffsetXy = 0, BamAngle angleOffset = default, SpawnProjectileFlags flags = 0,
        BamAngle pitch = default, int pointerSelector = AcsActorPointer.Target)
        => SpawnProjectile(actor, projectileClass, spawnHeight, spawnOffsetXy, angleOffset,
            flags | SpawnProjectileFlags.BadPitch, pitch, pointerSelector);

    public static void CustomBulletAttack(Actor actor, double spreadXy, double spreadZ, int bullets, int damagePerBullet,
        double range = 0, CustomBulletFlags flags = 0, int pointerSelector = AcsActorPointer.Target)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Bullet attack requires a simulation.");
        if (range <= 0) range = MissileRange;
        var reference = AcsActorPointer.Resolve(sim, actor, pointerSelector);
        if (reference is null && (flags & CustomBulletFlags.AimFacing) == 0) return;
        if ((flags & CustomBulletFlags.AimFacing) == 0) ActorOrientationActions.Face(actor, reference);
        var baseYaw = actor.Angle.ToDegrees();
        var dx = (reference ?? actor).X.ToDouble() - actor.X.ToDouble();
        var dy = (reference ?? actor).Y.ToDouble() - actor.Y.ToDouble();
        var pitch = (flags & CustomBulletFlags.NoPitch) != 0
            ? default
            : BamAngle.FromDegrees(-Math.Atan2((reference ?? actor).Center() - actor.AttackOriginZ(),
                Math.Max(1, Math.Sqrt(dx * dx + dy * dy))) * 180 / Math.PI);
        if (actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var attackSound)
            && !string.IsNullOrEmpty(attackSound) && !attackSound.Equals("None", StringComparison.OrdinalIgnoreCase))
            ActorSoundActions.PlayWeaponSound(actor, attackSound);
        for (var i = 0; i < bullets; i++)
        {
            var yawSpread = (flags & CustomBulletFlags.ExplicitAngle) != 0
                ? spreadXy : sim.NextCombatSpread() * spreadXy;
            var pitchSpread = (flags & CustomBulletFlags.ExplicitAngle) != 0
                ? spreadZ : sim.NextCombatSpread() * spreadZ;
            var damage = (flags & CustomBulletFlags.NoRandom) != 0
                ? damagePerBullet : damagePerBullet * (1 + (int)(sim.NextCombatRandom() % 3));
            var angle = BamAngle.FromDegrees(baseYaw + yawSpread);
            var aimPitch = BamAngle.FromDegrees(pitch.ToDegrees() + pitchSpread);
            var hit = CombatTrace.TraceLineAttack(sim, actor, angle, aimPitch, range);
            if (hit.Victim is { } victim)
                LineAttackImpact.ApplyActorDamage(sim, actor, victim, hit, angle, damage, "Hitscan", melee: false,
                    noRandomPuffZ: (flags & CustomBulletFlags.NoRandomPuffZ) != 0);
            else
            {
                GeometryLineAttack.Apply(sim, hit, damage);
                LineAttackImpact.SpawnGeometryPuff(sim, actor, hit, angle, aimPitch,
                    noRandomPuffZ: (flags & CustomBulletFlags.NoRandomPuffZ) != 0);
            }
        }
    }

    public static void BulletAttack(Actor actor)
    {
        var target = CombatTarget(actor);
        if (target is null) return;
        var sim = actor.Simulation ?? throw new InvalidOperationException("Bullet attack requires a simulation.");
        ActorOrientationActions.FaceTarget(actor);
        var dx = target.X.ToDouble() - actor.X.ToDouble();
        var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        var pitch = BamAngle.FromDegrees(-Math.Atan2(target.Center() - actor.AttackOriginZ(),
            Math.Max(1, Math.Sqrt(dx * dx + dy * dy))) * 180 / Math.PI);
        if (actor.ActorSounds.TryGetValue(ActorSoundType.Attack, out var attackSound)
            && !string.IsNullOrEmpty(attackSound) && !attackSound.Equals("None", StringComparison.OrdinalIgnoreCase))
            ActorSoundActions.PlayWeaponSound(actor, attackSound);
        var shots = actor.GetMissileDamage(0, 1);
        for (var i = 0; i < shots; i++)
        {
            var spread = sim.NextCombatSpread() * (5.625 / 256);
            var damage = 3 * (1 + (int)(sim.NextCombatRandom() % 5));
            var angle = BamAngle.FromDegrees(actor.Angle.ToDegrees() + spread);
            var hit = CombatTrace.TraceLineAttack(sim, actor, angle, pitch, MissileRange);
            if (hit.Victim is { } victim)
                LineAttackImpact.ApplyActorDamage(sim, actor, victim, hit, angle, damage, "Hitscan", melee: false);
            else
            {
                GeometryLineAttack.Apply(sim, hit, damage);
                LineAttackImpact.SpawnGeometryPuff(sim, actor, hit, angle, pitch);
            }
        }
    }

    public static ProjectileActor? SpawnProjectile(Actor actor, string projectileClass, double spawnHeight,
        double spawnOffsetXy, BamAngle angleOffset, SpawnProjectileFlags flags, BamAngle pitch,
        int pointerSelector, Actor? reference = null)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Spawn projectile requires a simulation.");
        reference ??= AcsActorPointer.Resolve(sim, actor, pointerSelector);
        var aimMode = (int)(flags & SpawnProjectileFlags.AimModeMask);
        if (reference is null && aimMode != 2)
        {
            if ((flags & SpawnProjectileFlags.CheckTargetDead) != 0 && actor.States.HasState(actor.SeeState)
                && (actor.Health > 0 || !actor.IsMonster))
                actor.States.Enter(actor, actor.SeeState);
            return null;
        }
        if (!TryResolveProjectileKind(projectileClass, out var kind))
            return null;
        var savedX = actor.X; var savedY = actor.Y; var savedZ = actor.Z;
        var yaw = actor.Angle.ToDegrees();
        var offsetAngle = BamAngle.FromDegrees(yaw - 90);
        var offsetX = spawnOffsetXy * Math.Cos(offsetAngle.ToDegrees() * Math.PI / 180);
        var offsetY = spawnOffsetXy * Math.Sin(offsetAngle.ToDegrees() * Math.PI / 180);
        ProjectileActor? missile;
        try
        {
            switch (aimMode)
            {
                case 2:
                    actor.X = Fixed.FromDouble(actor.X.ToDouble() + offsetX);
                    actor.Y = Fixed.FromDouble(actor.Y.ToDouble() + offsetY);
                    missile = sim.SpawnProjectile(actor, kind, target: null);
                    flags |= SpawnProjectileFlags.AbsolutePitch;
                    break;
                case 1:
                    missile = sim.SpawnProjectile(actor, kind, reference);
                    break;
                default:
                    var zBump = spawnHeight + actor.GetBobOffset() - 32;
                    actor.X = Fixed.FromDouble(actor.X.ToDouble() + offsetX);
                    actor.Y = Fixed.FromDouble(actor.Y.ToDouble() + offsetY);
                    actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + zBump);
                    missile = sim.SpawnProjectile(actor, kind, reference);
                    break;
            }
        }
        finally
        {
            actor.X = savedX; actor.Y = savedY; actor.Z = savedZ;
        }
        if (missile is null) return null;
        ApplySpawnProjectileVelocity(actor, missile, flags, pitch, angleOffset);
        AssignSpawnProjectileOwner(actor, missile, (flags & SpawnProjectileFlags.TrackOwner) != 0);
        MissileSpawnCheck.Check(sim, missile, actor, actor.Radius.ToDouble());
        return missile.Destroyed ? null : missile;
    }

    private static void ApplySpawnProjectileVelocity(Actor actor, ProjectileActor missile, SpawnProjectileFlags flags,
        BamAngle pitch, BamAngle angleOffset)
    {
        var useAbsolutePitch = (flags & (SpawnProjectileFlags.AbsolutePitch | SpawnProjectileFlags.OffsetPitch)) != 0;
        double missileSpeed;
        if (useAbsolutePitch)
        {
            var pitchDegrees = pitch.ToDegrees();
            if ((flags & SpawnProjectileFlags.OffsetPitch) != 0)
                pitchDegrees += (flags & SpawnProjectileFlags.BadPitch) != 0
                    ? -missile.PitchDegrees : missile.PitchDegrees;
            if ((flags & SpawnProjectileFlags.BadPitch) != 0)
            {
                missileSpeed = Math.Abs(Math.Cos(pitchDegrees * Math.PI / 180) * missile.Speed);
                missile.VelocityZ = Fixed.FromDouble(Math.Sin(pitchDegrees * Math.PI / 180) * missile.Speed);
            }
            else
            {
                missileSpeed = Math.Abs(Math.Cos(pitchDegrees * Math.PI / 180) * missile.Speed);
                missile.VelocityZ = Fixed.FromDouble(-Math.Sin(pitchDegrees * Math.PI / 180) * missile.Speed);
            }
            if ((flags & SpawnProjectileFlags.SavePitch) != 0) missile.PitchDegrees = pitchDegrees;
        }
        else missileSpeed = Math.Sqrt(missile.VelocityX.ToDouble() * missile.VelocityX.ToDouble()
            + missile.VelocityY.ToDouble() * missile.VelocityY.ToDouble());
        var yaw = (flags & SpawnProjectileFlags.AbsoluteAngle) != 0
            ? angleOffset.ToDegrees() : missile.Angle.ToDegrees() + angleOffset.ToDegrees();
        missile.Angle = BamAngle.FromDegrees(yaw);
        missile.VelFromAngle(missileSpeed);
    }

    private static void AssignSpawnProjectileOwner(Actor actor, ProjectileActor missile, bool trackOwner)
    {
        if (actor is ProjectileActor && trackOwner)
        {
            var owner = actor;
            while (owner is ProjectileActor chain && chain.Owner is { } nextOwner)
                owner = nextOwner;
            missile.ReassignOwner(owner is ProjectileActor projectile ? projectile.Owner ?? owner : owner);
        }
        if (actor.SeekerMissile && missile.SeekerMissile)
        {
            var tracer = AcsActorPointer.Resolve(actor.Simulation!, actor, AcsActorPointer.Target);
            if (tracer is not null) missile.RestoreTracerTarget(tracer.Id);
        }
    }

    internal static bool TryClassNameForProjectileType(int spawnType, out string className)
    {
        foreach (var candidate in Enum.GetValues<ProjectileKind>())
        {
            var editor = new ProjectileActor(new Actor(), candidate).DoomEdNum;
            if (editor != spawnType) continue;
            className = ProjectileActor.NativeClassNameOf(candidate);
            return true;
        }
        className = string.Empty;
        return false;
    }

    internal static bool TryResolveProjectileKind(string projectileClass, out ProjectileKind kind)
    {
        if (DoomActorCatalog.TryEditorNumberForClassName(projectileClass, out var editor))
        {
            foreach (var candidate in Enum.GetValues<ProjectileKind>())
            {
                var probe = new ProjectileActor(new Actor(), candidate);
                if (probe.DoomEdNum == editor) { kind = candidate; return true; }
            }
        }
        foreach (var candidate in Enum.GetValues<ProjectileKind>())
        {
            if (string.Equals(ProjectileActor.NativeClassNameOf(candidate), projectileClass, StringComparison.OrdinalIgnoreCase))
            {
                kind = candidate;
                return true;
            }
        }
        kind = default;
        return false;
    }

    private static Actor? CombatTarget(Actor actor) =>
        AcsActorPointer.Resolve(actor.Simulation
            ?? throw new InvalidOperationException("Combat actions require a simulation."), actor, AcsActorPointer.Target);
}
