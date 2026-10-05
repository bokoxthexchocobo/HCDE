namespace HCDE.Playsim;

/// <summary>Native <c>P_Thing_CanRaise</c> subset aligned with archvile resurrection gates.</summary>
internal static class ActorRaise
{
    internal static bool HasRaiseState(Actor actor) =>
        !actor.Destroyed
        && actor is not PlayerPawn
        && actor.IsDead
        && actor.RaiseDuration > 0
        && actor.SpawnHealth() > 0
        && (actor.States.RemainingTics == -1 || actor.States.CurrentCanRaise)
        && actor.Brain != null;

    public static bool CanRaise(AuthoritySimulation sim, Actor actor) =>
        HasRaiseState(actor) && CheckPosition(sim, actor);

    internal static bool CheckPosition(AuthoritySimulation sim, Actor actor, bool heightOnly = false)
    {
        var height = actor.Height; var radius = actor.Radius; var solid = actor.Solid;
        actor.Height = actor.ResurrectionHeight ?? height;
        if (!heightOnly) actor.Radius = actor.ResurrectionRadius ?? radius;
        actor.Solid = true;
        var fits = ActorPhysics.CanOccupy(sim, actor);
        actor.Height = height; actor.Radius = radius; actor.Solid = solid;
        return fits;
    }

    internal static void ReviveSupported(Actor corpse, bool restoreDimensions = true)
    {
        var verticalVelocity = corpse.VelocityZ;
        if (restoreDimensions)
        {
            corpse.Height = corpse.ResurrectionHeight ?? corpse.Height;
            corpse.Radius = corpse.ResurrectionRadius ?? corpse.Radius;
        }
        corpse.Health = corpse.SpawnHealth();
        var collisionFlags = corpse.ResurrectionCollisionFlags ?? 3;
        corpse.Solid = (collisionFlags & 1) != 0;
        corpse.Shootable = (collisionFlags & 2) != 0;
        corpse.NoBlockmap = (collisionFlags & 4) != 0;
        corpse.NoBlockMonsters = (collisionFlags & 8) != 0;
        corpse.Dropped = (collisionFlags & 16) != 0;
        corpse.IsMonster = (collisionFlags & 32) != 0;
        corpse.SpawnCeiling = (collisionFlags & 64) != 0;
        var defenseFlags = corpse.ResurrectionDefenseFlags ?? 0;
        corpse.Invulnerable = (defenseFlags & 1) != 0;
        corpse.Dormant = (defenseFlags & 2) != 0;
        corpse.NoRadiusDamage = (defenseFlags & 4) != 0;
        corpse.Friendly = (defenseFlags & 8) != 0 || corpse.SpawnFriendly;
        corpse.Ambush = (defenseFlags & 16) != 0;
        corpse.Boss = (defenseFlags & 32) != 0;
        var movementFlags = corpse.ResurrectionMovementFlags ?? 0;
        corpse.NoGravity = (movementFlags & 1) != 0;
        corpse.Floating = (movementFlags & 2) != 0;
        corpse.NoTeleport = (movementFlags & 4) != 0;
        corpse.CanSlide = (movementFlags & 8) != 0;
        corpse.AllowDropOff = (movementFlags & 16) != 0;
        corpse.JustHit = false;
        corpse.IceCorpse = false;
        corpse.Shattering = false;
        corpse.Buddha = false;
        corpse.FoilBuddha = false;
        corpse.FoilInvul = false;
        corpse.PierceArmor = false;
        corpse.StrifeDamage = false;
        corpse.Rip = corpse.DontRip = corpse.NoBossRip = false;
        corpse.RipperLevel = corpse.RipLevelMin = corpse.RipLevelMax = 0;
        corpse.Pushable = corpse.CannotPush = false;
        corpse.PushFactor = 0.25;
        corpse.ProjectilePassHeight = default;
        corpse.SpecialFireDamage = false;
        corpse.DamageType = null;
        corpse.ForcePain = false;
        corpse.NoPain = false;
        corpse.Painless = false;
        corpse.ThruActors = false;
        corpse.ThruSpecies = false;
        corpse.NoTarget = false;
        corpse.NeverTarget = false;
        corpse.NoTargetSwitch = false;
        corpse.QuickToRetaliate = false;
        corpse.NoHatePlayers = false;
        corpse.NoTelefrag = false;
        corpse.AlwaysTelefrag = false;
        corpse.DontBlast = false;
        corpse.Blasted = false;
        corpse.NoDropOff = false;
        corpse.NonShootable = false;
        corpse.ActsLikeBridge = false;
        corpse.NoIceDeath = false;
        corpse.ExtremeDeath = false;
        corpse.NoExtremeDeath = false;
        corpse.Ghost = false;
        corpse.ThruGhost = false;
        corpse.Spectral = false;
        corpse.CanPickupItems = corpse.SpawnCanPickupItems;
        corpse.SpecialPickup = corpse.SpawnSpecialPickup;
        corpse.HarmFriends = false;
        corpse.NoTrigger = false;
        corpse.OnMobj = false;
        corpse.NoSectorDamage = false;
        corpse.ForceSectorDamage = false;
        corpse.InFloat = false;
        corpse.VerticalFriction = false;
        corpse.DoHarmSpecies = false;
        corpse.IceShatter = false;
        corpse.AllowThruBits = false;
        corpse.NoExplodeFloor = false;
        corpse.CeilingHugger = false;
        corpse.FloorHugger = false;
        corpse.MThruSpecies = false;
        corpse.HitOwner = false;
        corpse.NoInfighting = false;
        corpse.NoInfightSpecies = false;
        corpse.ForceInfighting = false;
        corpse.InScrollSector = false;
        corpse.DontDrain = false;
        corpse.Brain!.Revive(corpse);
        corpse.VelocityZ = verticalVelocity;
    }

    public static bool CanRaiseAll(AuthoritySimulation sim, int tid, Actor? activator)
    {
        if (tid == 0)
        {
            var actor = activator is { Destroyed: false } ? activator : null;
            return actor == null || CanRaise(sim, actor);
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
        {
            if (!CanRaise(sim, actor))
                return false;
        }

        return true;
    }
}
