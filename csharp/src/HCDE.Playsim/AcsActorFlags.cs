namespace HCDE.Playsim;

/// <summary>Native <c>CheckActorFlag</c> / <c>ModActorFlag</c> subset for ACS CallFunc.</summary>
internal static class AcsActorFlags
{
    private enum Kind
    {
        Invulnerable,
        UseSpecial,
        StrifeDamage,
        Rip,
        DontRip,
        NoBossRip,
        Pushable,
        CannotPush,
        BlockedBySolidActors,
        BumpSpecial,
        Reflective,
        ThruReflect,
        DontReflect,
        MirrorReflect,
        Deflect,
        ShieldReflect,
        NoShieldReflect,
        AimReflect,
        SeekerMissile,
        SummonedMonster,
        ScreenSeeker,
        Stealth,
        FloatBob,
        NoVerticalMeleeRange,
        Killed,
        FoilInvul,
        SpecialFireDamage,
        Buddha,
        FoilBuddha,
        ForcePain,
        CausePain,
        AllowPain,
        NoDamage,
        Vulnerable,
        MVisBlocked,
        SeeInvisible,
        NoSightCheck,
        HuntPlayers,
        ChaseGoal,
        LaxTelefragDamage,
        Painless,
        NoIceDeath,
        ExtremeDeath,
        NoExtremeDeath,
        IceShatter,
        DontDrain,
        NoRadiusDamage,
        NeverTarget,
        SeeFriendlyMonsters,
        LookAllAround,
        StandStill,
        InConversation,
        DontFollowPlayers,
        CanJump,
        BlockAsPlayer,
        SpecialFloorClip,
        FloorClipEnabled,
        AvoidHazards,
        StayOnLift,
        AvoidMelee,
        CantLeaveFloorPic,
        Frightening,
        Frightened,
        NoFear,
        NoTargetSwitch,
        QuickToRetaliate,
        NoHatePlayers,
        NoTelefrag,
        AlwaysTelefrag,
        NoTeleport,
        CanSlide,
        Dormant,
        HarmFriends,
        NoTrigger,
        NoSectorDamage,
        ForceSectorDamage,
        DoHarmSpecies,
        NoInfighting,
        NoInfightSpecies,
        ForceInfighting,
        IsMonster,
        NoBlockMonsters,
        SpawnCeiling,
        AllowDropOff,
        OnMobj,
        InFloat,
        JustHit,
        JustAttacked,
        InCombat,
        NoSplashAlert,
        DontSplash,
        Touchy,
        Armed,
        DontHarmSpecies,
        DontHarmClass,
        FallDamage,
        Smashable,
        Crashed,
        DontGib,
        TeleStomp,
        NoTeleStomp,
        ActsLikeBridge,
        IceCorpse,
        Shattering,
        NoAutoOffSkullFly,
        NoBlockmap,
        NoSector,
        MoveWithSector,
        CanPass,
        DontOverlap,
        ExplodeCount,
        SkyExplode,
        HitTarget,
        HitMaster,
        HitTracer,
        NoLiftDrop,
        RelativeToFloor,
        Invisible,
        CantSeek,
        NoBlood,
        PuffGetsOwner,
        IsPuff,
        SpawnSoundSource,
        DontThrust,
        DontSeekInvisible,
        Shadow,
        MinVisible,
        PierceArmor,
        NoTarget,
        Ambush,
        Friendly,
        Solid,
        Shootable,
        Floating,
        NoGravity,
        Fly,
        SpawnFloat,
        FlyCheat,
        NoClip,
        NoFriction,
        RetargetAfterSlam,
        OnlySlamSolid,
        FullVolumeActive,
        FullVolumeSee,
        FullVolumeDeath,
        Corpse,
        DontCorpse,
        Falling,
        DontFall,
        AlwaysFast,
        NeverFast,
        Synchronized,
        NoExplodeFloor,
        CeilingHugger,
        FloorHugger,
        NoDropOff,
        Blasted,
        Boss,
        DontBlast,
        ThruActors,
        MThruSpecies,
        ThruSpecies,
        AllowThruBits,
        Ghost,
        ThruGhost,
        NonShootable,
        HitOwner,
        Spectral,
        NoPain,
        Pickup,
        Special,
        Dropped,
        IgnoreSkill,
        AlwaysPickup,
        PickupQuiet,
        NoScreenFlash,
        NoAttenPickupSound,
    }

    public static bool TryGet(Actor actor, string? flagName, out bool value)
    {
        value = false;
        if (actor.Destroyed || string.IsNullOrEmpty(flagName) || !TryMap(flagName, out var kind))
            return false;
        if (kind is Kind.IgnoreSkill or Kind.AlwaysPickup or Kind.PickupQuiet or Kind.NoScreenFlash or Kind.NoAttenPickupSound && !PickupCatalog.IsPickup(actor.DoomEdNum, actor.Simulation?.RavenHealthPickups ?? false)) return false;
        value = Read(actor, kind);
        return true;
    }

    public static bool TrySet(Actor actor, string? flagName, bool set)
    {
        if (actor.Destroyed || string.IsNullOrEmpty(flagName) || !TryMap(flagName, out var kind))
            return false;
        if (kind is Kind.IgnoreSkill or Kind.AlwaysPickup or Kind.PickupQuiet or Kind.NoScreenFlash or Kind.NoAttenPickupSound && !PickupCatalog.IsPickup(actor.DoomEdNum, actor.Simulation?.RavenHealthPickups ?? false)) return false;
        Write(actor, kind, set);
        return true;
    }

    private static bool TryMap(string flagName, out Kind kind)
    {
        if (flagName.StartsWith("Actor.", StringComparison.OrdinalIgnoreCase))
            return TryMapUnqualified(flagName[6..], out kind)
                && kind is not (Kind.IgnoreSkill or Kind.AlwaysPickup or Kind.PickupQuiet or Kind.NoScreenFlash or Kind.NoAttenPickupSound);
        return TryMapUnqualified(flagName, out kind);
    }

    private static bool TryMapUnqualified(string flagName, out Kind kind)
    {
        if (flagName.Equals("USESPECIAL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.UseSpecial; return true; }
        if (flagName.Equals("NOVERTICALMELEERANGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoVerticalMeleeRange; return true; }
        if (flagName.Equals("KILLED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Killed; return true; }
        if (flagName.Equals("PIERCEARMOR", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.PierceArmor; return true; }
        if (flagName.Equals("RIP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Rip; return true; }
        if (flagName.Equals("PUSHABLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Pushable; return true; }
        if (flagName.Equals("CANNOTPUSH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.CannotPush; return true; }
        if (flagName.Equals("BLOCKEDBYSOLIDACTORS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.BlockedBySolidActors; return true; }
        if (flagName.Equals("BUMPSPECIAL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.BumpSpecial; return true; }
        if (flagName.Equals("REFLECTIVE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Reflective; return true; }
        if (flagName.Equals("THRUREFLECT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ThruReflect; return true; }
        if (flagName.Equals("DONTREFLECT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontReflect; return true; }
        if (flagName.Equals("MIRRORREFLECT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.MirrorReflect; return true; }
        if (flagName.Equals("DEFLECT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Deflect; return true; }
        if (flagName.Equals("SHIELDREFLECT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ShieldReflect; return true; }
        if (flagName.Equals("NOSHIELDREFLECT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoShieldReflect; return true; }
        if (flagName.Equals("AIMREFLECT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AimReflect; return true; }
        if (flagName.Equals("SEEKERMISSILE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SeekerMissile; return true; }
        if (flagName.Equals("SUMMONEDMONSTER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SummonedMonster; return true; }
        if (flagName.Equals("SCREENSEEKER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ScreenSeeker; return true; }
        if (flagName.Equals("STEALTH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Stealth; return true; }
        if (flagName.Equals("FLOATBOB", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FloatBob; return true; }
        if (flagName.Equals("NOBOSSRIP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoBossRip; return true; }
        if (flagName.Equals("DONTRIP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontRip; return true; }
        if (flagName.Equals("STRIFEDAMAGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.StrifeDamage; return true; }
        if (flagName.Equals("NOLIFTDROP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoLiftDrop; return true; }
        if (flagName.Equals("RELATIVETOFLOOR", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.RelativeToFloor; return true; }
        if (flagName.Equals("CANPASS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.CanPass; return true; }
        if (flagName.Equals("DONTOVERLAP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontOverlap; return true; }
        if (flagName.Equals("EXPLOCOUNT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ExplodeCount; return true; }
        if (flagName.Equals("SKYEXPLODE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SkyExplode; return true; }
        if (flagName.Equals("HITTARGET", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.HitTarget; return true; }
        if (flagName.Equals("HITMASTER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.HitMaster; return true; }
        if (flagName.Equals("HITTRACER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.HitTracer; return true; }
        if (flagName.Equals("MOVEWITHSECTOR", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.MoveWithSector; return true; }
        if (flagName.Equals("NOSECTOR", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoSector; return true; }
        if (flagName.Equals("NOBLOCKMAP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoBlockmap; return true; }
        if (flagName.Equals("INVISIBLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Invisible; return true; }
        if (flagName.Equals("CANTSEEK", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.CantSeek; return true; }
        if (flagName.Equals("SPAWNSOUNDSOURCE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SpawnSoundSource; return true; }
        if (flagName.Equals("DONTTHRUST", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontThrust; return true; }
        if (flagName.Equals("DONTSEEKINVISIBLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontSeekInvisible; return true; }
        if (flagName.Equals("ISPUFF", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.IsPuff; return true; }
        if (flagName.Equals("PUFFGETSOWNER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.PuffGetsOwner; return true; }
        if (flagName.Equals("NOBLOOD", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoBlood; return true; }
        if (flagName.Equals("SHADOW", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Shadow; return true; }
        if (flagName.Equals("MINVISIBLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.MinVisible; return true; }
        if (flagName.Equals("NOSPLASHALERT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoSplashAlert; return true; }
        if (flagName.Equals("DONTSPLASH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontSplash; return true; }
        if (flagName.Equals("TOUCHY", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Touchy; return true; }
        if (flagName.Equals("ARMED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Armed; return true; }
        if (flagName.Equals("DONTHARMSPECIES", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontHarmSpecies; return true; }
        if (flagName.Equals("DONTHARMCLASS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontHarmClass; return true; }
        if (flagName.Equals("FALLDAMAGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FallDamage; return true; }
        if (flagName.Equals("SMASHABLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Smashable; return true; }
        if (flagName.Equals("CRASHED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Crashed; return true; }
        if (flagName.Equals("DONTGIB", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontGib; return true; }
        if (flagName.Equals("TELESTOMP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.TeleStomp; return true; }
        if (flagName.Equals("NOTELESTOMP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoTeleStomp; return true; }
        if (flagName.Equals("INCOMBAT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.InCombat; return true; }
        if (flagName.Equals("JUSTATTACKED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.JustAttacked; return true; }
        if (flagName.Equals("JUSTHIT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.JustHit; return true; }
        if (flagName.Equals("ACTLIKEBRIDGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ActsLikeBridge; return true; }
        if (flagName.Equals("ICECORPSE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.IceCorpse; return true; }
        if (flagName.Equals("SHATTERING", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Shattering; return true; }
        if (flagName.Equals("NOAUTOOFFSKULLFLY", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoAutoOffSkullFly; return true; }
        if (flagName.Equals("ISMONSTER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.IsMonster; return true; }
        if (flagName.Equals("NOBLOCKMONST", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoBlockMonsters; return true; }
        if (flagName.Equals("SPAWNCEILING", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SpawnCeiling; return true; }
        if (flagName.Equals("DROPOFF", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AllowDropOff; return true; }
        if (flagName.Equals("ONMOBJ", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.OnMobj; return true; }
        if (flagName.Equals("INFLOAT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.InFloat; return true; }
        if (flagName.Equals("HARMFRIENDS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.HarmFriends; return true; }
        if (flagName.Equals("NOTRIGGER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoTrigger; return true; }
        if (flagName.Equals("NOSECTORDAMAGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoSectorDamage; return true; }
        if (flagName.Equals("FORCESECTORDAMAGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ForceSectorDamage; return true; }
        if (flagName.Equals("DOHARMSPECIES", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DoHarmSpecies; return true; }
        if (flagName.Equals("NOINFIGHTING", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoInfighting; return true; }
        if (flagName.Equals("NOINFIGHTSPECIES", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoInfightSpecies; return true; }
        if (flagName.Equals("FORCEINFIGHTING", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ForceInfighting; return true; }
        if (flagName.Equals("NOTELEFRAG", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoTelefrag; return true; }
        if (flagName.Equals("ALWAYSTELEFRAG", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AlwaysTelefrag; return true; }
        if (flagName.Equals("NOTELEPORT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoTeleport; return true; }
        if (flagName.Equals("SLIDESONWALLS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.CanSlide; return true; }
        if (flagName.Equals("DORMANT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Dormant; return true; }
        if (flagName.Equals("NEVERTARGET", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NeverTarget; return true; }
        if (flagName.Equals("SEEFRIENDLYMONSTERS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SeeFriendlyMonsters; return true; }
        if (flagName.Equals("LOOKALLAROUND", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.LookAllAround; return true; }
        if (flagName.Equals("STANDSTILL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.StandStill; return true; }
        if (flagName.Equals("INCONVERSATION", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.InConversation; return true; }
        if (flagName.Equals("DONTFOLLOWPLAYERS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontFollowPlayers; return true; }
        if (flagName.Equals("NOFEAR", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoFear; return true; }
        if (flagName.Equals("FRIGHTENED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Frightened; return true; }
        if (flagName.Equals("FRIGHTENING", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Frightening; return true; }
        if (flagName.Equals("CANTLEAVEFLOORPIC", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.CantLeaveFloorPic; return true; }
        if (flagName.Equals("AVOIDMELEE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AvoidMelee; return true; }
        if (flagName.Equals("STAYONLIFT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.StayOnLift; return true; }
        if (flagName.Equals("AVOIDHAZARDS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AvoidHazards; return true; }
        if (flagName.Equals("FLOORCLIP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FloorClipEnabled; return true; }
        if (flagName.Equals("SPECIALFLOORCLIP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SpecialFloorClip; return true; }
        if (flagName.Equals("BLOCKASPLAYER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.BlockAsPlayer; return true; }
        if (flagName.Equals("CANJUMP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.CanJump; return true; }
        if (flagName.Equals("NOTARGETSWITCH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoTargetSwitch; return true; }
        if (flagName.Equals("QUICKTORETALIATE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.QuickToRetaliate; return true; }
        if (flagName.Equals("NOHATEPLAYERS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoHatePlayers; return true; }
        if (flagName.Equals("DONTDRAIN", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontDrain; return true; }
        if (flagName.Equals("NORADIUSDMG", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoRadiusDamage; return true; }
        if (flagName.Equals("NOICEDEATH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoIceDeath; return true; }
        if (flagName.Equals("EXTREMEDEATH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ExtremeDeath; return true; }
        if (flagName.Equals("NOEXTREMEDEATH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoExtremeDeath; return true; }
        if (flagName.Equals("ICESHATTER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.IceShatter; return true; }
        if (flagName.Equals("BUDDHA", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Buddha; return true; }
        if (flagName.Equals("FOILBUDDHA", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FoilBuddha; return true; }
        if (flagName.Equals("FORCEPAIN", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ForcePain; return true; }
        if (flagName.Equals("PAINLESS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Painless; return true; }
        if (flagName.Equals("FOILINVUL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FoilInvul; return true; }
        if (flagName.Equals("SPECIALFIREDAMAGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SpecialFireDamage; return true; }
        if (flagName.Equals("QUIET", StringComparison.OrdinalIgnoreCase)
            || flagName.Equals("INVENTORY.QUIET", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.PickupQuiet; return true; }
        if (flagName.Equals("NOSCREENFLASH", StringComparison.OrdinalIgnoreCase)
            || flagName.Equals("INVENTORY.NOSCREENFLASH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoScreenFlash; return true; }
        if (flagName.Equals("NOATTENPICKUPSOUND", StringComparison.OrdinalIgnoreCase)
            || flagName.Equals("INVENTORY.NOATTENPICKUPSOUND", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoAttenPickupSound; return true; }
        if (flagName.Equals("ALWAYSPICKUP", StringComparison.OrdinalIgnoreCase)
            || flagName.Equals("INVENTORY.ALWAYSPICKUP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AlwaysPickup; return true; }
        if (flagName.Equals("IGNORESKILL", StringComparison.OrdinalIgnoreCase)
            || flagName.Equals("INVENTORY.IGNORESKILL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.IgnoreSkill; return true; }
        if (flagName.Equals("DROPPED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Dropped; return true; }
        if (flagName.Equals("SPECTRAL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Spectral; return true; }
        if (flagName.Equals("HITOWNER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.HitOwner; return true; }
        if (flagName.Equals("NONSHOOTABLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NonShootable; return true; }
        if (flagName.Equals("GHOST", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Ghost; return true; }
        if (flagName.Equals("THRUGHOST", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ThruGhost; return true; }
        if (flagName.Equals("ALLOWTHRUBITS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AllowThruBits; return true; }
        if (flagName.Equals("THRUSPECIES", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ThruSpecies; return true; }
        if (flagName.Equals("MTHRUSPECIES", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.MThruSpecies; return true; }
        if (flagName.Equals("THRUACTORS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ThruActors; return true; }
        if (flagName.Equals("BOSS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Boss; return true; }
        if (flagName.Equals("DONTBLAST", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontBlast; return true; }
        if (flagName.Equals("BLASTED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Blasted; return true; }
        if (flagName.Equals("FLOORHUGGER", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FloorHugger; return true; }
        if (flagName.Equals("NODROPOFF", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoDropOff; return true; }
        if (flagName.Equals("CEILINGHUGGER", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.CeilingHugger;
            return true;
        }
        if (flagName.Equals("NOEXPLODEFLOOR", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.NoExplodeFloor;
            return true;
        }
        if (flagName.Equals("INVULNERABLE", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Invulnerable;
            return true;
        }
        if (flagName.Equals("NOTARGET", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.NoTarget;
            return true;
        }
        if (flagName.Equals("AMBUSH", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Ambush;
            return true;
        }
        if (flagName.Equals("FRIENDLY", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Friendly;
            return true;
        }
        if (flagName.Equals("SOLID", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Solid;
            return true;
        }
        if (flagName.Equals("SHOOTABLE", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Shootable;
            return true;
        }
        if (flagName.Equals("FLOAT", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Floating;
            return true;
        }
        if (flagName.Equals("DONTFALL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontFall; return true; }
        if (flagName.Equals("ALWAYSFAST", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AlwaysFast; return true; }
        if (flagName.Equals("NEVERFAST", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NeverFast; return true; }
        if (flagName.Equals("SYNCHRONIZED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Synchronized; return true; }
        if (flagName.Equals("FALLING", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Falling; return true; }
        if (flagName.Equals("DONTCORPSE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.DontCorpse; return true; }
        if (flagName.Equals("CORPSE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Corpse; return true; }
        if (flagName.Equals("NOFRICTION", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoFriction; return true; }
        if (flagName.Equals("FULLVOLACTIVE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FullVolumeActive; return true; }
        if (flagName.Equals("RETARGETAFTERSLAM", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.RetargetAfterSlam; return true; }
        if (flagName.Equals("CAUSEPAIN", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.CausePain; return true; }
        if (flagName.Equals("ALLOWPAIN", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.AllowPain; return true; }
        if (flagName.Equals("NOSIGHTCHECK", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoSightCheck; return true; }
        if (flagName.Equals("CHASEGOAL", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.ChaseGoal; return true; }
        if (flagName.Equals("HUNTPLAYERS", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.HuntPlayers; return true; }
        if (flagName.Equals("SEEINVISIBLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SeeInvisible; return true; }
        if (flagName.Equals("MVISBLOCKED", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.MVisBlocked; return true; }
        if (flagName.Equals("VULNERABLE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Vulnerable; return true; }
        if (flagName.Equals("NODAMAGE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoDamage; return true; }
        if (flagName.Equals("LAXTELEFRAGDMG", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.LaxTelefragDamage; return true; }
        if (flagName.Equals("ONLYSLAMSOLID", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.OnlySlamSolid; return true; }
        if (flagName.Equals("FULLVOLSEE", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FullVolumeSee; return true; }
        if (flagName.Equals("FULLVOLDEATH", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FullVolumeDeath; return true; }
        if (flagName.Equals("FLY", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.Fly; return true; }
        if (flagName.Equals("SPAWNFLOAT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.SpawnFloat; return true; }
        if (flagName.Equals("FLYCHEAT", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.FlyCheat; return true; }
        if (flagName.Equals("NOCLIP", StringComparison.OrdinalIgnoreCase))
        { kind = Kind.NoClip; return true; }
        if (flagName.Equals("NOGRAVITY", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.NoGravity;
            return true;
        }
        if (flagName.Equals("NOPAIN", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.NoPain;
            return true;
        }

        if (flagName.Equals("PICKUP", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Pickup;
            return true;
        }
        if (flagName.Equals("SPECIAL", StringComparison.OrdinalIgnoreCase))
        {
            kind = Kind.Special;
            return true;
        }
        kind = default;
        return false;
    }

    private static bool Read(Actor actor, Kind kind) => kind switch
    {
        Kind.UseSpecial => actor.UseSpecial,
        Kind.Invulnerable => actor.Invulnerable,
        Kind.FoilInvul => actor.FoilInvul,
        Kind.SpecialFireDamage => actor.SpecialFireDamage,
        Kind.Buddha => actor.Buddha,
        Kind.FoilBuddha => actor.FoilBuddha,
        Kind.ForcePain => actor.ForcePain,
        Kind.CausePain => actor.CausePain,
        Kind.AllowPain => actor.AllowPain,
        Kind.NoSightCheck => actor.NoSightCheck,
        Kind.HuntPlayers => actor.HuntPlayers,
        Kind.ChaseGoal => actor.ChaseGoal,
        Kind.Vulnerable => actor.Vulnerable,
        Kind.SeeInvisible => actor.SeeInvisible,
        Kind.MVisBlocked => actor.MVisBlocked,
        Kind.NoDamage => actor.NoDamage,
        Kind.LaxTelefragDamage => actor.LaxTelefragDamage,
        Kind.Painless => actor.Painless,
        Kind.NoIceDeath => actor.NoIceDeath,
        Kind.ExtremeDeath => actor.ExtremeDeath,
        Kind.NoExtremeDeath => actor.NoExtremeDeath,
        Kind.IceShatter => actor.IceShatter,
        Kind.DontDrain => actor.DontDrain,
        Kind.NoRadiusDamage => actor.NoRadiusDamage,
        Kind.NeverTarget => actor.NeverTarget,
        Kind.SeeFriendlyMonsters => actor.SeeFriendlyMonsters,
        Kind.LookAllAround => actor.LookAllAround,
        Kind.StandStill => actor.StandStill,
        Kind.InConversation => actor.InConversation,
        Kind.DontFollowPlayers => actor.DontFollowPlayers,
        Kind.CanJump => actor.CanJump,
        Kind.BlockAsPlayer => actor.BlockAsPlayer,
        Kind.SpecialFloorClip => actor.SpecialFloorClip,
        Kind.FloorClipEnabled => actor.FloorClipEnabled,
        Kind.AvoidHazards => actor.AvoidHazards,
        Kind.StayOnLift => actor.StayOnLift,
        Kind.AvoidMelee => actor.AvoidMelee,
        Kind.CantLeaveFloorPic => actor.CantLeaveFloorPic,
        Kind.Frightening => actor.Frightening,
        Kind.Frightened => actor.Frightened,
        Kind.NoFear => actor.NoFear,
        Kind.NoTargetSwitch => actor.NoTargetSwitch,
        Kind.QuickToRetaliate => actor.QuickToRetaliate,
        Kind.NoHatePlayers => actor.NoHatePlayers,
        Kind.NoTelefrag => actor.NoTelefrag,
        Kind.AlwaysTelefrag => actor.AlwaysTelefrag,
        Kind.NoTeleport => actor.NoTeleport,
        Kind.CanSlide => actor.CanSlide,
        Kind.Dormant => actor.Dormant,
        Kind.HarmFriends => actor.HarmFriends,
        Kind.NoTrigger => actor.NoTrigger,
        Kind.NoSectorDamage => actor.NoSectorDamage,
        Kind.ForceSectorDamage => actor.ForceSectorDamage,
        Kind.DoHarmSpecies => actor.DoHarmSpecies,
        Kind.NoInfighting => actor.NoInfighting,
        Kind.NoInfightSpecies => actor.NoInfightSpecies,
        Kind.ForceInfighting => actor.ForceInfighting,
        Kind.IsMonster => actor.IsMonster,
        Kind.NoBlockMonsters => actor.NoBlockMonsters,
        Kind.SpawnCeiling => actor.SpawnCeiling,
        Kind.AllowDropOff => actor.AllowDropOff,
        Kind.OnMobj => actor.OnMobj,
        Kind.InFloat => actor.InFloat,
        Kind.JustHit => actor.JustHit,
        Kind.JustAttacked => actor.JustAttacked,
        Kind.InCombat => actor.InCombat,
        Kind.NoSplashAlert => actor.NoSplashAlert,
        Kind.DontSplash => actor.DontSplash,
        Kind.Touchy => actor.Touchy,
        Kind.Armed => actor.Armed,
        Kind.DontHarmSpecies => actor.DontHarmSpecies,
        Kind.DontHarmClass => actor.DontHarmClass,
        Kind.FallDamage => actor.FallDamage,
        Kind.Smashable => actor.Smashable,
        Kind.Crashed => actor.Crashed,
        Kind.DontGib => actor.DontGib,
        Kind.TeleStomp => actor.TeleStomp,
        Kind.NoTeleStomp => actor.NoTeleStomp,
        Kind.ActsLikeBridge => actor.ActsLikeBridge,
        Kind.IceCorpse => actor.IceCorpse,
        Kind.Shattering => actor.Shattering,
        Kind.NoAutoOffSkullFly => actor.NoAutoOffSkullFly,
        Kind.NoSector => actor.NoSector,
        Kind.NoBlockmap => actor.NoBlockmap,
        Kind.MoveWithSector => actor.MoveWithSector,
        Kind.CanPass => actor.CanPass,
        Kind.DontOverlap => actor.DontOverlap,
        Kind.ExplodeCount => actor.ExplodeCount,
        Kind.SkyExplode => actor.SkyExplode,
        Kind.HitTarget => actor.HitTarget,
        Kind.HitMaster => actor.HitMaster,
        Kind.HitTracer => actor.HitTracer,
        Kind.NoLiftDrop => actor.NoLiftDrop,
        Kind.RelativeToFloor => actor.RelativeToFloor,
        Kind.Invisible => actor.Invisible,
        Kind.IsPuff => actor.IsPuff,
        Kind.NoBlood => actor.NoBlood,
        Kind.PuffGetsOwner => actor.PuffGetsOwner,
        Kind.CantSeek => actor.CantSeek,
        Kind.SpawnSoundSource => actor.SpawnSoundSource,
        Kind.DontThrust => actor.DontThrust,
        Kind.DontSeekInvisible => actor.DontSeekInvisible,
        Kind.Shadow => actor.Shadow,
        Kind.MinVisible => actor.MinVisible,
        Kind.PierceArmor => actor.PierceArmor,
        Kind.StrifeDamage => actor.StrifeDamage,
        Kind.Rip => actor.Rip,
        Kind.DontRip => actor.DontRip,
        Kind.NoBossRip => actor.NoBossRip,
        Kind.Pushable => actor.Pushable,
        Kind.CannotPush => actor.CannotPush,
        Kind.BlockedBySolidActors => actor.BlockedBySolidActors,
        Kind.BumpSpecial => actor.BumpSpecial,
        Kind.Reflective => actor.Reflective,
        Kind.ThruReflect => actor.ThruReflect,
        Kind.DontReflect => actor.DontReflect,
        Kind.MirrorReflect => actor.MirrorReflect,
        Kind.Deflect => actor.Deflect,
        Kind.ShieldReflect => actor.ShieldReflect,
        Kind.NoShieldReflect => actor.NoShieldReflect,
        Kind.AimReflect => actor.AimReflect,
        Kind.SeekerMissile => actor.SeekerMissile,
        Kind.SummonedMonster => actor.SummonedMonster,
        Kind.ScreenSeeker => actor.ScreenSeeker,
        Kind.Stealth => actor.Stealth,
        Kind.FloatBob => actor.FloatBob,
        Kind.NoVerticalMeleeRange => actor.NoVerticalMeleeRange,
        Kind.Killed => actor.Killed,
        Kind.NoTarget => actor.NoTarget,
        Kind.Ambush => actor.Ambush,
        Kind.Friendly => actor.Friendly,
        Kind.Solid => actor.Solid,
        Kind.Shootable => actor.Shootable,
        Kind.Floating => actor.Floating,
        Kind.NoGravity => actor.NoGravity,
        Kind.Fly => actor.Fly,
        Kind.SpawnFloat => actor.SpawnFloat,
        Kind.FlyCheat => actor.FlyCheat,
        Kind.NoClip => actor.NoClip,
        Kind.NoFriction => actor.NoFriction,
        Kind.FullVolumeActive => actor.FullVolumeActive,
        Kind.RetargetAfterSlam => actor.RetargetAfterSlam,
        Kind.OnlySlamSolid => actor.OnlySlamSolid,
        Kind.FullVolumeSee => actor.FullVolumeSee,
        Kind.FullVolumeDeath => actor.FullVolumeDeath,
        Kind.Corpse => actor.Corpse,
        Kind.DontCorpse => actor.DontCorpse,
        Kind.Falling => actor.Falling,
        Kind.DontFall => actor.DontFall,
        Kind.AlwaysFast => actor.AlwaysFast,
        Kind.NeverFast => actor.NeverFast,
        Kind.Synchronized => actor.Synchronized,
        Kind.NoExplodeFloor => actor.NoExplodeFloor,
        Kind.CeilingHugger => actor.CeilingHugger,
        Kind.FloorHugger => actor.FloorHugger,
        Kind.NoDropOff => actor.NoDropOff,
        Kind.Blasted => actor.Blasted,
        Kind.Boss => actor.Boss,
        Kind.DontBlast => actor.DontBlast,
        Kind.ThruActors => actor.ThruActors,
        Kind.MThruSpecies => actor.MThruSpecies,
        Kind.ThruSpecies => actor.ThruSpecies,
        Kind.AllowThruBits => actor.AllowThruBits,
        Kind.Ghost => actor.Ghost,
        Kind.ThruGhost => actor.ThruGhost,
        Kind.NonShootable => actor.NonShootable,
        Kind.HitOwner => actor.HitOwner,
        Kind.Spectral => actor.Spectral,
        Kind.NoPain => actor.NoPain,
        Kind.Pickup => actor.CanPickupItems,
        Kind.Special => actor.SpecialPickup,
        Kind.Dropped => actor.Dropped,
        Kind.PickupQuiet => actor.PickupQuiet,
        Kind.NoScreenFlash => actor.NoScreenFlash,
        Kind.NoAttenPickupSound => actor.NoAttenPickupSound,
        Kind.IgnoreSkill => actor.IgnoreAmmoSkill,
        Kind.AlwaysPickup => actor.AlwaysPickupOverride ?? PickupCatalog.AlwaysPickup(actor.DoomEdNum, actor.Simulation?.StrifeHealthPickups ?? false),
        _ => false,
    };

    private static void Write(Actor actor, Kind kind, bool value)
    {
        switch (kind)
        {
            case Kind.Invulnerable: actor.Invulnerable = value; actor.HasDefensePropertyOverride = true; break;
            case Kind.FoilInvul: actor.FoilInvul = value; break;
            case Kind.SpecialFireDamage: actor.SpecialFireDamage = value; break;
            case Kind.UseSpecial: actor.UseSpecial = value; break;
            case Kind.Buddha: actor.Buddha = value; break;
            case Kind.FoilBuddha: actor.FoilBuddha = value; break;
            case Kind.ForcePain: actor.ForcePain = value; break;
            case Kind.CausePain: actor.CausePain = value; break;
            case Kind.AllowPain: actor.AllowPain = value; break;
            case Kind.NoSightCheck: actor.NoSightCheck = value; break;
            case Kind.HuntPlayers: actor.HuntPlayers = value; break;
            case Kind.ChaseGoal: actor.ChaseGoal = value; break;
            case Kind.Vulnerable: actor.Vulnerable = value; break;
            case Kind.SeeInvisible: actor.SeeInvisible = value; break;
            case Kind.MVisBlocked: actor.MVisBlocked = value; break;
            case Kind.NoDamage: actor.NoDamage = value; break;
            case Kind.LaxTelefragDamage: actor.LaxTelefragDamage = value; break;
            case Kind.Painless: actor.Painless = value; break;
            case Kind.NoIceDeath: actor.NoIceDeath = value; break;
            case Kind.ExtremeDeath: actor.ExtremeDeath = value; break;
            case Kind.NoExtremeDeath: actor.NoExtremeDeath = value; break;
            case Kind.IceShatter: actor.IceShatter = value; break;
            case Kind.DontDrain: actor.DontDrain = value; break;
            case Kind.NoRadiusDamage: actor.NoRadiusDamage = value; break;
            case Kind.NeverTarget: actor.NeverTarget = value; break;
            case Kind.SeeFriendlyMonsters: actor.SeeFriendlyMonsters = value; break;
            case Kind.LookAllAround: actor.LookAllAround = value; break;
            case Kind.StandStill: actor.StandStill = value; break;
            case Kind.InConversation: actor.InConversation = value; break;
            case Kind.DontFollowPlayers: actor.DontFollowPlayers = value; break;
            case Kind.CanJump: actor.CanJump = value; break;
            case Kind.BlockAsPlayer: actor.BlockAsPlayer = value; break;
            case Kind.SpecialFloorClip: actor.SpecialFloorClip = value; break;
            case Kind.FloorClipEnabled: actor.FloorClipEnabled = value; break;
            case Kind.AvoidHazards: actor.AvoidHazards = value; break;
            case Kind.StayOnLift: actor.StayOnLift = value; break;
            case Kind.AvoidMelee: actor.AvoidMelee = value; break;
            case Kind.CantLeaveFloorPic: actor.CantLeaveFloorPic = value; break;
            case Kind.Frightening: actor.Frightening = value; break;
            case Kind.Frightened: actor.Frightened = value; break;
            case Kind.NoFear: actor.NoFear = value; break;
            case Kind.NoTargetSwitch: actor.NoTargetSwitch = value; break;
            case Kind.QuickToRetaliate: actor.QuickToRetaliate = value; break;
            case Kind.NoHatePlayers: actor.NoHatePlayers = value; actor.HasFriendshipOverride = true; break;
            case Kind.NoTelefrag: actor.NoTelefrag = value; break;
            case Kind.AlwaysTelefrag: actor.AlwaysTelefrag = value; break;
            case Kind.NoTeleport: actor.NoTeleport = value; break;
            case Kind.CanSlide: actor.CanSlide = value; break;
            case Kind.Dormant: actor.Dormant = value; break;
            case Kind.HarmFriends: actor.HarmFriends = value; break;
            case Kind.NoTrigger: actor.NoTrigger = value; break;
            case Kind.NoSectorDamage: actor.NoSectorDamage = value; break;
            case Kind.ForceSectorDamage: actor.ForceSectorDamage = value; break;
            case Kind.DoHarmSpecies: actor.DoHarmSpecies = value; break;
            case Kind.NoInfighting: actor.NoInfighting = value; break;
            case Kind.NoInfightSpecies: actor.NoInfightSpecies = value; break;
            case Kind.ForceInfighting: actor.ForceInfighting = value; break;
            case Kind.IsMonster: actor.IsMonster = value; break;
            case Kind.NoBlockMonsters: actor.NoBlockMonsters = value; break;
            case Kind.SpawnCeiling: actor.SpawnCeiling = value; break;
            case Kind.AllowDropOff: actor.AllowDropOff = value; break;
            case Kind.OnMobj: actor.OnMobj = value; break;
            case Kind.InFloat: actor.InFloat = value; break;
            case Kind.JustHit: actor.JustHit = value; break;
            case Kind.JustAttacked: actor.JustAttacked = value; break;
            case Kind.InCombat: actor.InCombat = value; break;
            case Kind.NoSplashAlert: actor.NoSplashAlert = value; break;
            case Kind.DontSplash: actor.DontSplash = value; break;
            case Kind.Touchy: actor.Touchy = value; break;
            case Kind.Armed: actor.Armed = value; break;
            case Kind.DontHarmSpecies: actor.DontHarmSpecies = value; break;
            case Kind.DontHarmClass: actor.DontHarmClass = value; break;
            case Kind.FallDamage: actor.FallDamage = value; break;
            case Kind.Smashable: actor.Smashable = value; break;
            case Kind.Crashed: actor.Crashed = value; break;
            case Kind.DontGib: actor.DontGib = value; break;
            case Kind.TeleStomp: actor.TeleStomp = value; break;
            case Kind.NoTeleStomp: actor.NoTeleStomp = value; break;
            case Kind.ActsLikeBridge: actor.ActsLikeBridge = value; break;
            case Kind.IceCorpse: actor.IceCorpse = value; break;
            case Kind.Shattering: actor.Shattering = value; break;
            case Kind.NoAutoOffSkullFly: actor.NoAutoOffSkullFly = value; break;
            case Kind.NoSector: ActorPropertyActions.ChangeLinkFlags(actor, sector: value ? 1 : 0); break;
            case Kind.NoLiftDrop: actor.NoLiftDrop = value; break;
            case Kind.RelativeToFloor: actor.RelativeToFloor = value; break;
            case Kind.CanPass: actor.CanPass = value; break;
            case Kind.DontOverlap: actor.DontOverlap = value; break;
            case Kind.ExplodeCount: actor.ExplodeCount = value; break;
            case Kind.SkyExplode: actor.SkyExplode = value; break;
            case Kind.HitTarget: actor.HitTarget = value; break;
            case Kind.HitMaster: actor.HitMaster = value; break;
            case Kind.HitTracer: actor.HitTracer = value; break;
            case Kind.MoveWithSector: actor.MoveWithSector = value; break;
            case Kind.NoBlockmap: ActorPropertyActions.ChangeLinkFlags(actor, value ? 1 : 0); break;
            case Kind.Invisible: actor.Invisible = value; break;
            case Kind.CantSeek: actor.CantSeek = value; break;
            case Kind.SpawnSoundSource: actor.SpawnSoundSource = value; break;
            case Kind.DontThrust: actor.DontThrust = value; break;
            case Kind.DontSeekInvisible: actor.DontSeekInvisible = value; break;
            case Kind.IsPuff: actor.IsPuff = value; break;
            case Kind.NoBlood: actor.NoBlood = value; break;
            case Kind.PuffGetsOwner: actor.PuffGetsOwner = value; break;
            case Kind.Shadow: actor.Shadow = value; break;
            case Kind.MinVisible: actor.MinVisible = value; break;
            case Kind.PierceArmor: actor.PierceArmor = value; break;
            case Kind.StrifeDamage: actor.StrifeDamage = value; break;
            case Kind.Rip: actor.Rip = value; break;
            case Kind.DontRip: actor.DontRip = value; break;
            case Kind.NoBossRip: actor.NoBossRip = value; break;
            case Kind.Pushable: actor.Pushable = value; break;
            case Kind.CannotPush: actor.CannotPush = value; break;
            case Kind.BlockedBySolidActors: actor.BlockedBySolidActors = value; break;
            case Kind.BumpSpecial: actor.BumpSpecial = value; break;
            case Kind.Reflective: actor.Reflective = value; break;
            case Kind.ThruReflect: actor.ThruReflect = value; break;
            case Kind.DontReflect: actor.DontReflect = value; break;
            case Kind.MirrorReflect: actor.MirrorReflect = value; break;
            case Kind.Deflect: actor.Deflect = value; break;
            case Kind.ShieldReflect: actor.ShieldReflect = value; break;
            case Kind.NoShieldReflect: actor.NoShieldReflect = value; break;
            case Kind.AimReflect: actor.AimReflect = value; break;
            case Kind.SeekerMissile: actor.SeekerMissile = value; break;
            case Kind.SummonedMonster: actor.SummonedMonster = value; break;
            case Kind.ScreenSeeker: actor.ScreenSeeker = value; break;
            case Kind.Stealth: actor.Stealth = value; break;
            case Kind.FloatBob: actor.FloatBob = value; break;
            case Kind.NoVerticalMeleeRange: actor.NoVerticalMeleeRange = value; break;
            case Kind.Killed: actor.Killed = value; break;
            case Kind.NoTarget: actor.NoTarget = value; break;
            case Kind.Ambush: actor.Ambush = value; break;
            case Kind.Friendly: ActorPropertyActions.SetFriendly(actor, value); break;
            case Kind.Solid: actor.Solid = value; actor.HasMovementActionOverride = true; break;
            case Kind.Shootable: actor.Shootable = value; actor.HasDefensePropertyOverride = true; break;
            case Kind.Floating: actor.Floating = value; actor.HasMovementActionOverride = true; break;
            case Kind.DontFall: actor.DontFall = value; break;
            case Kind.AlwaysFast: actor.AlwaysFast = value; break;
            case Kind.NeverFast: actor.NeverFast = value; break;
            case Kind.Synchronized: actor.Synchronized = value; break;
            case Kind.Falling: actor.Falling = value; break;
            case Kind.DontCorpse: actor.DontCorpse = value; break;
            case Kind.Corpse: actor.Corpse = value; break;
            case Kind.NoFriction: actor.NoFriction = value; break;
            case Kind.FullVolumeActive: actor.FullVolumeActive = value; break;
            case Kind.RetargetAfterSlam: actor.RetargetAfterSlam = value; break;
            case Kind.OnlySlamSolid: actor.OnlySlamSolid = value; break;
            case Kind.FullVolumeSee: actor.FullVolumeSee = value; break;
            case Kind.FullVolumeDeath: actor.FullVolumeDeath = value; break;
            case Kind.Fly: actor.Fly = value; break;
            case Kind.SpawnFloat: actor.SpawnFloat = value; break;
            case Kind.FlyCheat: actor.FlyCheat = value; break;
            case Kind.NoClip: actor.NoClip = value; break;
            case Kind.NoGravity: actor.NoGravity = value; actor.HasMovementActionOverride = true; break;
            case Kind.NoExplodeFloor: actor.NoExplodeFloor = value; break;
            case Kind.CeilingHugger: actor.CeilingHugger = value; break;
            case Kind.FloorHugger: actor.FloorHugger = value; break;
            case Kind.NoDropOff: actor.NoDropOff = value; break;
            case Kind.Blasted: actor.Blasted = value; break;
            case Kind.Boss: actor.Boss = value; break;
            case Kind.DontBlast: actor.DontBlast = value; break;
            case Kind.ThruActors: actor.ThruActors = value; break;
            case Kind.MThruSpecies: actor.MThruSpecies = value; break;
            case Kind.ThruSpecies: actor.ThruSpecies = value; break;
            case Kind.AllowThruBits: actor.AllowThruBits = value; break;
            case Kind.Ghost: actor.Ghost = value; break;
            case Kind.ThruGhost: actor.ThruGhost = value; break;
            case Kind.NonShootable: actor.NonShootable = value; actor.HasDefensePropertyOverride = true; break;
            case Kind.HitOwner: actor.HitOwner = value; break;
            case Kind.Spectral: actor.Spectral = value; break;
            case Kind.NoPain: actor.NoPain = value; break;
            case Kind.Pickup: actor.CanPickupItems = value; break;
            case Kind.Special: actor.SpecialPickup = value; break;
            case Kind.Dropped: actor.Dropped = value; break;
            case Kind.PickupQuiet: actor.PickupQuiet = value; break;
            case Kind.NoScreenFlash: actor.NoScreenFlash = value; break;
            case Kind.NoAttenPickupSound: actor.NoAttenPickupSound = value; break;
            case Kind.IgnoreSkill: actor.IgnoreAmmoSkill = value; break;
            case Kind.AlwaysPickup: actor.AlwaysPickupOverride = value; break;
        }
    }
}
