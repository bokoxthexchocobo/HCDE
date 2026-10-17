namespace HCDE.Playsim;

/// <summary>Native <c>P_Thing_CanRaise</c> subset aligned with archvile resurrection gates.</summary>
internal static class ActorRaise
{
    internal static bool HasRaiseState(Actor actor) =>
        !actor.Destroyed
        && actor is not PlayerPawn
        && actor.Corpse
        && (actor.RaiseState >= 0 ? actor.GetRaiseState().HasValue : actor.RaiseDuration > 0)
        && (actor.RaiseState >= 0 || actor.SpawnHealth() > 0)
        && (actor.States.RemainingTics == -1 || actor.States.CurrentCanRaise)
        && (actor.RaiseState >= 0 || actor.Brain != null);

    public static bool CanRaise(AuthoritySimulation sim, Actor actor) =>
        HasRaiseState(actor) && CheckPosition(sim, actor);

    internal static bool CanResurrect(Actor? raiser, Actor? corpse) =>
        raiser != null && raiser.CanResurrect(corpse, false)
        && (corpse == null || ReferenceEquals(raiser, corpse) || corpse.CanResurrect(raiser, true));

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
        // Native Revive assigns health without running a state action mid-restoration.
        corpse.RestoreHealth(corpse.SpawnHealth());
        corpse.DeathDamageType = null;
        corpse.Killed = false;
        corpse.Corpse = corpse.DefaultCorpse;
        corpse.Vulnerable = false;
        corpse.MVisBlocked = false;
        corpse.SeeInvisible = false;
        corpse.NoBlood = corpse.DefaultNoBlood;
        corpse.IsPuff = corpse.DefaultIsPuff;
        corpse.DontCorpse = false;
        corpse.DontFall = corpse.ClassDoomEdNum == 3006;
        corpse.FullVolumeDeath = corpse.DefaultFullVolumeDeath;
        corpse.FullVolumeSee = corpse.DefaultFullVolumeSee;
        corpse.FullVolumeActive = false;
        var collisionFlags = corpse.ResurrectionCollisionFlags ?? 3;
        corpse.Solid = (collisionFlags & 1) != 0;
        corpse.Shootable = (collisionFlags & 2) != 0;
        corpse.NoBlockmap = (collisionFlags & 4) != 0;
        corpse.NoSector = (collisionFlags & 128) != 0;
        corpse.CanPass = (collisionFlags & 256) != 0;
        corpse.DontOverlap = false;
        corpse.ExplodeCount = false;
        corpse.SkyExplode = false;
        corpse.HitTarget = corpse.HitMaster = corpse.HitTracer = false;
        corpse.NoBlockMonsters = (collisionFlags & 8) != 0;
        corpse.Dropped = (collisionFlags & 16) != 0;
        corpse.IsMonster = (collisionFlags & 32) != 0;
        corpse.SpawnCeiling = (collisionFlags & 64) != 0;
        var defenseFlags = corpse.ResurrectionDefenseFlags ?? 0;
        corpse.Invulnerable = (defenseFlags & 1) != 0;
        corpse.Dormant = (defenseFlags & 2) != 0;
        corpse.NoRadiusDamage = (defenseFlags & 4) != 0;
        corpse.Friendly = corpse.DefaultFriendly;
        corpse.Ambush = (defenseFlags & 16) != 0;
        corpse.Shadow = corpse.DefaultShadow;
        corpse.CantSeek = false;
        corpse.SpawnSoundSource = false;
        corpse.DontThrust = false;
        corpse.DontSeekInvisible = false;
        corpse.MinVisible = false;
        corpse.Boss = (defenseFlags & 32) != 0;
        var movementFlags = corpse.ResurrectionMovementFlags ?? 0;
        corpse.NoGravity = (movementFlags & 1) != 0;
        corpse.Floating = (movementFlags & 2) != 0;
        corpse.NoTeleport = (movementFlags & 4) != 0;
        corpse.CanSlide = (movementFlags & 8) != 0;
        corpse.AllowDropOff = (movementFlags & 16) != 0;
        corpse.JustHit = (corpse.DefaultCombatFlags & 1) != 0;
        corpse.JustAttacked = (corpse.DefaultCombatFlags & 2) != 0;
        corpse.InCombat = false;
        corpse.NoSplashAlert = false;
        corpse.DontSplash = false;
        corpse.DontHarmSpecies = false;
        corpse.DontHarmClass = false;
        corpse.DontGib = false;
        corpse.Crashed = false;
        corpse.Smashable = false;
        corpse.FallDamage = false;
        corpse.TeleStomp = corpse.DefaultTeleStomp;
        corpse.NoTeleStomp = false;
        corpse.Touchy = corpse.DefaultTouchy;
        corpse.Armed = false;
        corpse.IceCorpse = false;
        corpse.Shattering = false;
        corpse.Buddha = false;
        corpse.FoilBuddha = false;
        corpse.FoilInvul = false;
        corpse.PierceArmor = false;
        corpse.StrifeDamage = false;
        corpse.Rip = corpse.DefaultRip;
        corpse.DontRip = corpse.NoBossRip = false;
        corpse.RipperLevel = corpse.RipLevelMin = corpse.RipLevelMax = 0;
        corpse.Pushable = (corpse.DefaultPushFlags & 1) != 0;
        corpse.CannotPush = (corpse.DefaultPushFlags & 2) != 0;
        corpse.BlockedBySolidActors = false;
        corpse.BumpSpecial = false;
        corpse.Reflective = corpse.DefaultReflective;
        corpse.ThruReflect = corpse.DontReflect = false;
        corpse.MirrorReflect = corpse.Deflect = false;
        corpse.ShieldReflect = corpse.NoShieldReflect = false;
        corpse.AimReflect = false;
        corpse.SeekerMissile = corpse.DefaultSeeker;
        corpse.SummonedMonster = false;
        corpse.ScreenSeeker = false;
        corpse.Stealth = corpse.DefaultStealth;
        corpse.FloatBob = corpse.DefaultFloatBob;
        corpse.MoveWithSector = corpse.DefaultMoveWithSector;
        corpse.NoLiftDrop = corpse.RelativeToFloor = false;
        corpse.NoVerticalMeleeRange = false;
        corpse.PushFactor = 0.25;
        corpse.SpecialFireDamage = false;
        corpse.DamageType = null;
        corpse.PainType = null;
        corpse.ForcePain = false;
        corpse.CausePain = false;
        corpse.AllowPain = false;
        corpse.NoDamage = false;
        corpse.LaxTelefragDamage = false;
        corpse.NoPain = false;
        corpse.Painless = false;
        corpse.ThruActors = false;
        corpse.ThruSpecies = false;
        corpse.NoTarget = false;
        corpse.FlyCheat = false;
        corpse.Fly = corpse.DefaultFly;
        corpse.SpawnFloat = corpse.DefaultSpawnFloat;
        corpse.NoClip = corpse.DefaultNoClip;
        corpse.NeverTarget = false;
        corpse.SeeFriendlyMonsters = false;
        corpse.LookAllAround = corpse.StandStill = false;
        corpse.InConversation = false;
        corpse.DontFollowPlayers = false;
        corpse.CanJump = false;
        corpse.BlockAsPlayer = false;
        corpse.FloorClipEnabled = corpse.DefaultFloorClip;
        corpse.SpecialFloorClip = false;
        corpse.AvoidHazards = false;
        corpse.StayOnLift = false;
        corpse.AvoidMelee = false; corpse.StrafeCount = 0;
        corpse.CantLeaveFloorPic = corpse.DefaultFloorPic;
        corpse.NoFear = corpse.Frightened = corpse.Frightening = false;
        corpse.NoTargetSwitch = false;
        corpse.QuickToRetaliate = false;
        corpse.NoHatePlayers = false;
        corpse.NoSightCheck = false;
        corpse.HuntPlayers = false;
        corpse.ChaseGoal = false;
        corpse.NoTelefrag = false;
        corpse.AlwaysTelefrag = false;
        corpse.DontBlast = false;
        corpse.Blasted = corpse.DefaultBlasted;
        corpse.NoDropOff = false;
        corpse.NonShootable = corpse.DefaultNonShootable;
        corpse.ActsLikeBridge = false;
        corpse.NoIceDeath = false;
        corpse.ExtremeDeath = false;
        corpse.NoExtremeDeath = false;
        corpse.Ghost = false;
        corpse.ThruGhost = corpse.DefaultThruGhost;
        corpse.Spectral = false;
        corpse.CanPickupItems = corpse.SpawnCanPickupItems;
        corpse.SpecialPickup = corpse.SpawnSpecialPickup;
        corpse.HarmFriends = false;
        corpse.NoTrigger = false;
        corpse.OnMobj = false;
        corpse.NoSectorDamage = false;
        corpse.ForceSectorDamage = false;
        corpse.InFloat = corpse.DefaultInFloat;
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
        corpse.AlwaysFast = corpse.NeverFast = false;
        corpse.Synchronized = false;
        corpse.HandleNoDelay = false;
        corpse.NoInfightSpecies = false;
        corpse.ForceInfighting = false;
        corpse.InScrollSector = false;
        corpse.DontDrain = false;
        corpse.SetTargetPointer(null);
        corpse.StoredTargetId = null;
        if (corpse is PlayerPawn player)
        {
            player.PoisonCount = 0;
            player.PoisonSourceId = null;
            player.PoisonDamageType = null;
            player.PoisonPainType = null;
        }
        corpse.Brain?.Revive(corpse);
        corpse.LastDamageSourceId = null;
        corpse.VelocityZ = verticalVelocity;
        corpse.OnRevive();
        corpse.Simulation?.NotifyActorRevived(corpse);
    }

    internal static void EnterRaiseState(Actor corpse, int? raiseState)
    {
        var state = raiseState ?? corpse.SpawnState;
        if (corpse.States.HasState(state)) corpse.States.Enter(corpse, state);
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
