namespace HCDE.Playsim;

/// <summary>Native actor.zs property actions for managed state tables.</summary>
public static class ActorPropertyActions
{
    public static void HideThing(Actor actor) => actor.Invisible = true;
    public static void UnhideThing(Actor actor) => actor.Invisible = false;

    public static bool SinkMobj(Actor actor, double speed)
    {
        if (!double.IsFinite(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
        if (actor.FloorClip >= actor.Height.ToDouble()) return true;
        actor.FloorClip += speed;
        return false;
    }

    public static bool RaiseMobj(Actor actor, double speed)
    {
        if (!double.IsFinite(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
        if (actor.FloorClip <= 0) return true;
        actor.FloorClip -= speed;
        if (actor.FloorClip > 0) return false;
        actor.FloorClip = 0;
        return true;
    }

    public static void ChangeLinkFlags(Actor actor, int blockmap = -1, int sector = -1)
    {
        if (sector != -1) throw new NotSupportedException("Native sector-link membership is not yet modeled.");
        if (blockmap == -1) return;
        actor.NoBlockmap = blockmap != 0;
        actor.HasBlockmapOverride = true;
    }

    public static void SetFriendly(Actor actor, bool value)
    {
        actor.Friendly = value;
        actor.HasFriendshipOverride = true;
    }

    public static void CopyFriendliness(Actor actor, int pointerSelector = AcsActorPointer.Master)
    {
        if (actor is PlayerPawn || actor.Destroyed) return;
        var source = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation
                ?? throw new InvalidOperationException("Friendship pointer selection requires a simulation."), actor, pointerSelector);
        if (source is not null && !source.Destroyed) CopySupportedFriendship(actor, source);
    }

    internal static void CopySupportedFriendship(Actor actor, Actor source)
    {
        actor.Friendly = source.Friendly;
        actor.FriendPlayer = source.FriendPlayer;
        actor.TidToHate = source.TidToHate;
        actor.NoHatePlayers = source.NoHatePlayers;
    }

    public static void CheckTerrain(Actor actor)
    {
        var sim = actor.Simulation ?? throw new InvalidOperationException("Terrain actions require a simulation.");
        if ((uint)actor.SectorIndex >= (uint)sim.Level.Sectors.Count)
            throw new InvalidOperationException("Terrain action actor has no valid sector.");
        if (actor.Z.ToDouble() != sim.FloorOf(actor.SectorIndex)) return;
        var sector = sim.Level.Sectors[actor.SectorIndex];
        if (sector.DamageAmount >= 1_000_000)
            ActorDamage.Apply(actor, 999, damageType: "InstantDeath");
        else if (sector.Special == 118)
        {
            var angleSpeed = unchecked(sector.Tag - 100);
            var speed = (angleSpeed % 10) / 16.0;
            var radians = (angleSpeed / 10) * (Math.PI / 4);
            actor.VelocityX = Fixed.FromDouble(actor.VelocityX.ToDouble() + Math.Cos(radians) * speed);
            actor.VelocityY = Fixed.FromDouble(actor.VelocityY.ToDouble() + Math.Sin(radians) * speed);
        }
    }

    public static void SetFloatBobPhase(Actor actor, int phase)
    {
        if ((uint)phase > 63) return;
        actor.FloatBobPhase = phase;
        actor.HasFloatBobPhaseOverride = true;
    }

    public static void SetScale(Actor actor, double scaleX, double scaleY = 0,
        int pointerSelector = 0, bool useZero = false)
    {
        if (!double.IsFinite(scaleX)) throw new ArgumentOutOfRangeException(nameof(scaleX));
        if (!double.IsFinite(scaleY)) throw new ArgumentOutOfRangeException(nameof(scaleY));
        var subject = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation
                ?? throw new InvalidOperationException("Scale pointer selection requires a simulation."), actor, pointerSelector);
        if (subject is null || subject.Destroyed) return;
        subject.ScaleX = scaleX;
        subject.ScaleY = scaleY != 0 || useZero ? scaleY : scaleX;
        subject.HasScaleOverride = true;
    }

    public static void SetSpecies(Actor actor, string? species, int pointerSelector = 0)
    {
        var subject = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation
                ?? throw new InvalidOperationException("Species pointer selection requires a simulation."), actor, pointerSelector);
        if (subject is null || subject.Destroyed) return;
        subject.Species = string.IsNullOrEmpty(species) || species.Equals("None", StringComparison.OrdinalIgnoreCase) ? null : species;
        subject.HasSpeciesOverride = true;
    }

    public static bool SetSize(Actor actor, double radius = -1, double height = -1, bool testPosition = false)
    {
        if (!double.IsFinite(radius)) throw new ArgumentOutOfRangeException(nameof(radius));
        if (!double.IsFinite(height)) throw new ArgumentOutOfRangeException(nameof(height));
        var sim = testPosition ? actor.Simulation
            ?? throw new InvalidOperationException("Actor size position testing requires a simulation.") : null;
        var oldRadius = actor.Radius; var oldHeight = actor.Height;
        var newRadius = radius < 0 ? oldRadius : Fixed.FromDouble(radius);
        var newHeight = height < 0 ? oldHeight : Fixed.FromDouble(height);
        actor.Radius = newRadius; actor.Height = newHeight;
        if (sim is not null && !ActorPhysics.CanOccupy(sim, actor))
        {
            actor.Radius = oldRadius; actor.Height = oldHeight;
            return false;
        }
        if (actor is PlayerPawn player) player.FullHeight = newHeight.ToDouble();
        actor.HasSizeOverride = true;
        return true;
    }

    public static void CountdownArg(Actor actor, int index, int? targetState = null)
    {
        if ((uint)index >= 5) return;
        var previous = actor.SpecialArgs[index];
        actor.SpecialArgs[index] = unchecked(previous - 1);
        actor.SpecialChanged = true;
        if (previous != 0) return;
        if (actor is ProjectileActor projectile) projectile.ExplodeForAction();
        else if (actor.Shootable) ActorHealthActions.Die(actor);
        else
        {
            var state = targetState ?? (actor.States.HasState(actor.DeathState) ? actor.DeathState : -1);
            actor.States.Enter(actor, state);
        }
    }

    public static void SetTeleFog(Actor actor, string? sourceClass, string? destinationClass)
    {
        actor.TeleFogSource = sourceClass;
        actor.TeleFogDest = destinationClass;
        actor.HasTeleFogOverride = true;
    }

    public static void SwapTeleFog(Actor actor)
    {
        (actor.TeleFogSource, actor.TeleFogDest) = (actor.TeleFogDest, actor.TeleFogSource);
        actor.HasTeleFogOverride = true;
    }

    public static void SetChaseThreshold(Actor actor, int threshold, bool setDefault = false, int pointerSelector = 0)
    {
        var subject = pointerSelector == AcsActorPointer.Default ? actor
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(actor.Simulation
                ?? throw new InvalidOperationException("Chase threshold pointer selection requires a simulation."),
                actor, pointerSelector);
        if (subject is null || subject.Destroyed) return;
        var brain = subject.Brain
            ?? throw new NotSupportedException("Chase thresholds require a managed monster brain.");
        brain.SetChaseThreshold(threshold, setDefault);
    }

    public static void ClearTarget(Actor actor)
    {
        actor.Brain?.ClearActionTargets();
        actor.LastHeardTargetId = null;
        actor.HasTargetMemoryOverride = true;
    }

    public static void ClearLastHeard(Actor actor)
    { actor.LastHeardTargetId = null; actor.HasTargetMemoryOverride = true; }

    public static void SetArg(Actor actor, int index, int value)
    {
        // Native A_SetArg ignores indices outside the five special arguments.
        if ((uint)index >= 5) return;
        actor.SpecialArgs[index] = value;
        actor.SpecialChanged = true;
    }

    public static void SetSpecial(Actor actor, int special, int arg0 = 0, int arg1 = 0,
        int arg2 = 0, int arg3 = 0, int arg4 = 0)
    {
        actor.Special = special;
        actor.SpecialArgs[0] = arg0;
        actor.SpecialArgs[1] = arg1;
        actor.SpecialArgs[2] = arg2;
        actor.SpecialArgs[3] = arg3;
        actor.SpecialArgs[4] = arg4;
    }

    public static void Turn(Actor actor, double degrees = 0)
    {
        if (!double.IsFinite(degrees)) throw new ArgumentOutOfRangeException(nameof(degrees));
        actor.Angle = new BamAngle(unchecked(actor.Angle.Raw + BamAngle.FromDegrees(degrees).Raw));
    }

    public static void Recoil(Actor actor, double speed)
    {
        if (!double.IsFinite(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
        var radians = unchecked(actor.Angle.Raw + BamAngle.Angle180) * (Math.PI / 0x80000000u);
        actor.VelocityX = Fixed.FromDouble(actor.VelocityX.ToDouble() + Math.Cos(radians) * speed);
        actor.VelocityY = Fixed.FromDouble(actor.VelocityY.ToDouble() + Math.Sin(radians) * speed);
    }

    public static void ChangeFlag(Actor actor, string flagName, bool value)
    {
        if (!AcsActorFlags.TrySet(actor, flagName, value))
            throw new ArgumentException("Unknown or unsupported actor flag.", nameof(flagName));
    }

    public static void SetDamageType(Actor actor, string? damageType) => actor.DamageType = damageType;
    public static void SetMass(Actor actor, int mass) { actor.Mass = mass; actor.HasDefensePropertyOverride = true; }
    public static void SetInvulnerable(Actor actor) { actor.Invulnerable = true; actor.HasDefensePropertyOverride = true; }
    public static void UnsetInvulnerable(Actor actor) { actor.Invulnerable = false; actor.HasDefensePropertyOverride = true; }
    public static void SetShootable(Actor actor) { actor.Shootable = true; actor.NonShootable = false; actor.HasDefensePropertyOverride = true; }
    public static void UnsetShootable(Actor actor) { actor.Shootable = false; actor.NonShootable = true; actor.HasDefensePropertyOverride = true; }
    public static void NoGravity(Actor actor) { actor.NoGravity = true; actor.HasMovementActionOverride = true; }
    public static void Gravity(Actor actor)
    {
        actor.NoGravity = false;
        actor.Gravity = Fixed.FromInt(1);
        actor.HasGravityOverride = true;
        actor.HasMovementActionOverride = true;
    }
    public static void LowGravity(Actor actor)
    {
        actor.NoGravity = false;
        actor.Gravity = Fixed.FromDouble(0.125);
        actor.HasGravityOverride = true;
        actor.HasMovementActionOverride = true;
    }
    public static void SetGravity(Actor actor, double gravity)
    {
        if (!double.IsFinite(gravity)) throw new ArgumentOutOfRangeException(nameof(gravity));
        actor.Gravity = Fixed.FromDouble(Math.Clamp(gravity, 0, 10));
        actor.HasGravityOverride = true;
    }
    public static void SetSolid(Actor actor) { actor.Solid = true; actor.HasMovementActionOverride = true; }
    public static void UnsetSolid(Actor actor) { actor.Solid = false; actor.HasMovementActionOverride = true; }
    public static void SetFloat(Actor actor) { actor.Floating = true; actor.HasMovementActionOverride = true; }
    public static void UnsetFloat(Actor actor) { actor.Floating = false; actor.HasMovementActionOverride = true; }
    public static void SetRipperLevel(Actor actor, int level) => actor.RipperLevel = level;
    public static void SetRipMin(Actor actor, int minimum) => actor.RipLevelMin = minimum;
    public static void SetRipMax(Actor actor, int maximum) => actor.RipLevelMax = maximum;
}
