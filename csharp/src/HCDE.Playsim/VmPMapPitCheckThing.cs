namespace HCDE.Playsim;

/// <summary><c>PIT_CheckThing</c> from <c>p_map.cpp</c> (1393–1780).</summary>
public static class VmPMapPitCheckThing
{
    public const int MfSolid = 1;
    public const int MfSpecial = 2;
    public const int MfShootable = 4;
    public const int Mf6Touchy = 1;
    public const int Mf6Thruactors = 1;
    public const int Mf8Allowthrubits = 1;
    public const int MfSkullfly = 1;
    public const int Mf2Blasted = 1;
    public const int MfMissile = 1;
    public const int MfPickup = 1;
    public const int MfCorpse = 1;
    public const int Mf2Rip = 1;
    public static bool SkipSelf(AActorRef thing, AActorRef mover) =>
        thing.Id == mover.Id;

    public static bool ThruActorsPass(int flags2Thing, int flags2Mover) =>
        ((flags2Thing | flags2Mover) & Mf6Thruactors) != 0;

    public static bool ThruBitsAllow(int thruBits, int flags8) =>
        (thruBits & 1) != 0 && (flags8 & Mf8Allowthrubits) != 0;

    public static bool BlockDistanceClear(double dx, double dy, double blockdist) =>
        Math.Abs(dx) < blockdist && Math.Abs(dy) < blockdist;

    public static bool SameSpeciesPass(int flags6, bool sameSpecies) =>
        (flags6 & 1) != 0 && sameSpecies;

    public static bool NonSolidNonInteractive(int flags, int flags6) =>
        (flags & (MfSolid | MfSpecial | MfShootable)) == 0 && (flags6 & Mf6Touchy) == 0;

    public static bool BridgeStepMonster(bool compatPassobj, bool monster, bool bridge, double topz, double floorz, double actorZ, double step) =>
        !compatPassobj && monster && bridge && topz >= floorz && topz <= actorZ + step;

    public static bool UnblockWhenMovingApart(double newdist, double olddist) => newdist > olddist;

    public static bool PassobjVerticalOverlap(double moverTop, double thingZ, double thingTop) =>
        moverTop > thingZ && moverTop < thingTop;

    public static bool PickupOverUnder(double moverZ, double topz, double thingZ) =>
        moverZ > topz || moverZ < thingZ;

    public static bool TouchyDetonate(
        bool touchy,
        bool solidMover,
        int health,
        bool armedOrSentient,
        bool differentClass,
        bool verticalOverlap) =>
        touchy && solidMover && health > 0 && armedOrSentient && differentClass && verticalOverlap;

    public static bool BumpSpecialPlayer(bool bump, bool isPlayer) => bump && isPlayer;

    public static bool PredictionSolidOnly(bool predicting, bool solid, bool unblocking) =>
        predicting && (!solid || unblocking);

    public static bool BlastedMonsterShove(bool blasted, bool shootable, bool boss, bool monster, bool dontBlast) =>
        blasted && shootable && !boss && monster && !dontBlast;

    public static bool MissileOrMbfBouncer(bool missile, int bounceFlags) =>
        missile || ((bounceFlags & 1) != 0);

    public static bool GhostPass(bool ghost, bool thruGhost) => ghost && thruGhost;

    public static bool RipperSkipsCorpse(bool corpse, bool rip, bool shootable) =>
        corpse && rip && !shootable;

    public static double ProjectileClipHeight(double passHeight, bool clipNegative, double height) =>
        passHeight > 0 ? passHeight : clipNegative ? -passHeight : height;

    public static bool OverObject(double actorZ, double clipTop) => actorZ > clipTop;

    public static bool UnderObject(double actorTop, double thingZ) => actorTop < thingZ;

    public static bool RipperDamageDue(bool rip, bool dontRip, bool boss, bool noBossRip, bool ripLevelOk) =>
        rip && !dontRip && (!boss || !noBossRip) && ripLevelOk;

    public static bool ReflectiveTurnSeeker(bool thruReflect, bool reflective, bool missile) =>
        thruReflect && reflective && missile;

    public static bool PushablePushed(bool pushable, bool cannotPush, bool newPushTime) =>
        pushable && !cannotPush && newPushTime;

    public static bool SolidBlock(bool solid, bool mirrorBlocks, bool noclip, bool moverSolid) =>
        (solid || mirrorBlocks) && (!noclip || mirrorBlocks) && moverSolid;

    public static bool PickupTouch(bool special, bool pickup, double thingZ, double topMinusStep) =>
        special && pickup && thingZ < topMinusStep;

    public static bool ReturnPassSolid(bool solid, bool unblocking) => !solid || unblocking;

    public static bool MonsterWalkBridge(bool monster, bool bridge, bool solid) => monster && bridge && solid;

    public static bool PassobjOverlap(bool passobj, bool bridge, bool compat) =>
        (passobj || bridge) && !compat;

    public static bool DummySwitchOverlap(bool dontOverlap) => !dontOverlap;

    public static bool SpecialPickupOver(bool special, double moverZ, double topz) => special && moverZ > topz;

    public static bool ScriptCallbackSolid(bool solid, bool touchy, bool bumpspecial, bool moverSolid) =>
        (solid || touchy || bumpspecial) && moverSolid;

    public static bool TouchyDisarm(bool touchy, bool armed) => touchy && armed;

    public static bool TouchySameMaster(bool masterMatch) => !masterMatch;

    public static bool BumpSpecialThrottle(int maptime, int lastbump) => maptime > lastbump;

    public static bool BlastedDamageMass(int mass) => mass > 0;

    public static bool MthruSpecies(bool flag, bool same) => flag && same;

    public static bool TargetPassPlayer(bool targetPlayer, bool pass) => targetPlayer && pass;

    public static bool BounceActorZeroDamage(bool bounce, bool zero, bool missile) =>
        bounce && (zero || !missile);

    public static bool NonshootableSolid(bool shootable, bool solid) => !shootable && !solid;

    public static bool SpectralMismatch(bool spectral, bool missileSpectral) => spectral && !missileSpectral;

    public static bool DoRipping(bool doRip, bool dontRip) => doRip && !dontRip;

    public static bool PushFactorApply(bool pushable, bool newTime) => pushable && newTime;

    public static bool MaxStepPickupGate(double z, double top, double step) => z < top - step;

    public static bool KilloughPassobj(bool passobj, bool compat) => passobj && !compat;

    public static bool NoclipPassSolid(bool noclip, bool solid) => noclip || !solid;

    public static bool IceCorpseBridge(bool ice, bool corpse) => ice && corpse;

    public static bool FloatMissileSkullfly(bool floatFlag, bool missile, bool skullfly) =>
        !(floatFlag && missile && skullfly);

    public static bool WalkingCrossExpands(int activation) => (activation & 512) != 0;

    public static bool UseThroughLine(int activation) => (activation & 256) != 0;

    public static bool MissilePromoteTeleport(int special, int activation, int type, bool missile) =>
        special == 70 && (activation & 2) != 0 && type == 4 && missile;

    public static bool DoorMonsterSlow(bool monster, int special) => monster && special is 8 or 9;

    public static bool SecretDoorBlocks(bool secret, bool monster) => secret && monster;

    public static bool LaxMonsterSwitch(bool lax, bool monster) => lax && monster;

    public static bool ExpandAnyCross(int activation) => (activation & 512) != 0;

    public static bool WalkingCrossRequiresFloor(bool onFloor, bool onMobj) => onFloor || onMobj;

    public static bool PlayerUseBack(int type) => type == 5;

    public static bool ClearSpecialOnSuccess(bool repeat, bool success) => !repeat && success;

    public static bool ChangeSwitchOnUse(bool success, int activationType) =>
        success && activationType is 1 or 2 or 3;

    public static bool BossRipBlocked(bool boss, bool noBossRip) => boss && noBossRip;

    public static bool PushableNotCannot(bool pushable, bool cannot) => pushable && !cannot;

    public enum PitTraversalOutcome
    {
        KeepGoing,
        StopPass,
        StopBlock,
    }

    public readonly record struct PitEarlyGateContext(
        bool SkipSelf,
        bool ThruActors,
        bool ThruBits,
        bool IgnoreCorpse,
        bool MirrorBlocks,
        bool ThingInteractive,
        bool InBlockDistance,
        bool SameSpeciesPass,
        bool PassThroughPlayer);

    /// <summary>Maps the early <c>PIT_CheckThing</c> filters before overlap handling.</summary>
    public static PitTraversalOutcome EvaluateEarlyGates(PitEarlyGateContext ctx)
    {
        if (ctx.SkipSelf) return PitTraversalOutcome.KeepGoing;
        if (ctx.ThruActors) return PitTraversalOutcome.KeepGoing;
        if (ctx.ThruBits) return PitTraversalOutcome.KeepGoing;
        if (ctx.IgnoreCorpse) return PitTraversalOutcome.KeepGoing;
        if (!ctx.MirrorBlocks && !ctx.ThingInteractive) return PitTraversalOutcome.KeepGoing;
        if (!ctx.InBlockDistance) return PitTraversalOutcome.KeepGoing;
        if (ctx.SameSpeciesPass) return PitTraversalOutcome.KeepGoing;
        if (ctx.PassThroughPlayer) return PitTraversalOutcome.KeepGoing;
        return PitTraversalOutcome.StopPass;
    }

    public readonly record struct PitBridgeUnblockContext(
        bool RestrictedPortal,
        bool CompatNoPassobj,
        bool MonsterBridgeSolid,
        bool BridgeStepOk,
        bool FromPmoveOrPlayer,
        bool ThingSolidOrMirror,
        bool SamePosition,
        bool OldOverlap,
        double NewDist,
        double OldDist,
        bool VerticalOverlap,
        bool DontOverlapBoth);

    public static (bool StepBridge, bool Unblocking) EvaluateBridgeAndUnblock(PitBridgeUnblockContext ctx)
    {
        var step = false;
        var unblocking = false;
        if (ctx.RestrictedPortal) return (step, unblocking);
        if (BridgeStepMonster(ctx.CompatNoPassobj, ctx.MonsterBridgeSolid, true, 0, 0, 0, 0) && ctx.BridgeStepOk)
            step = true;
        if (ctx.FromPmoveOrPlayer && ctx.ThingSolidOrMirror)
        {
            if (ctx.SamePosition) unblocking = true;
            else if (ctx.OldOverlap && UnblockWhenMovingApart(ctx.NewDist, ctx.OldDist))
                unblocking = ctx.VerticalOverlap || ctx.DontOverlapBoth;
        }
        return (step, unblocking);
    }

    public readonly record struct PitPassobjContext(
        bool PassobjOrBridge,
        bool CompatNoPassobj,
        bool IsMissile,
        bool Rip,
        bool DontRip,
        bool NoBossRip,
        bool Boss,
        bool SpecialPickup,
        double MoverZ,
        double TopZ,
        double ThingZ,
        double MoverTop,
        bool DontOverlapPair,
        bool Unblocking);

    public static PitTraversalOutcome EvaluatePassobjVertical(PitPassobjContext ctx)
    {
        if (!PassobjOverlap(ctx.PassobjOrBridge, false, ctx.CompatNoPassobj))
            return PitTraversalOutcome.KeepGoing;
        if (ctx.IsMissile && ctx.Rip && !ctx.DontRip && !(ctx.NoBossRip && ctx.Boss))
        {
            if (ctx.SpecialPickup && PickupOverUnder(ctx.MoverZ, ctx.TopZ, ctx.ThingZ))
                return PitTraversalOutcome.KeepGoing;
        }
        if (ctx.SpecialPickup && PickupOverUnder(ctx.MoverZ, ctx.TopZ, ctx.ThingZ))
            return PitTraversalOutcome.KeepGoing;
        if (!ctx.DontOverlapPair && (ctx.MoverZ >= ctx.TopZ || ctx.MoverTop <= ctx.ThingZ))
            return PitTraversalOutcome.KeepGoing;
        if (ctx.DontOverlapPair)
            return ctx.Unblocking ? PitTraversalOutcome.StopPass : PitTraversalOutcome.StopBlock;
        return PitTraversalOutcome.StopPass;
    }

    public readonly record struct PitMissilePathContext(
        bool IsMissile,
        int BounceMbf,
        bool ThingSolid,
        bool Nonshootable,
        bool Ghost,
        bool ThruGhost,
        bool MthruSpecies,
        bool SameSpeciesAsTarget,
        bool CorpseRipSkip,
        bool TargetPassPlayer,
        double MoverZ,
        double ClipHeight,
        double ThingZ,
        double MoverTop,
        int BounceActors,
        bool ZeroDamage,
        bool Hitowner,
        bool SameTarget,
        int SpecialHit,
        bool CanAttack,
        bool Shootable,
        bool Spectral,
        bool MissileSpectral,
        bool DoRip,
        bool DontRip,
        bool RipLevelOk,
        bool Boss,
        bool NoBossRip,
        bool FirstRip,
        bool Pushable,
        bool CannotPush,
        bool Reflective,
        bool ThruReflect,
        bool Seeker);

    /// <summary>Missile and MBF-bouncer branch through rip, reflect, and infight gates.</summary>
    public static PitTraversalOutcome EvaluateMissilePath(PitMissilePathContext ctx)
    {
        if (!MissileOrMbfBouncer(ctx.IsMissile, ctx.BounceMbf)) return PitTraversalOutcome.KeepGoing;
        if (ctx.Nonshootable) return PitTraversalOutcome.StopPass;
        if (GhostPass(ctx.Ghost, ctx.ThruGhost)) return PitTraversalOutcome.StopPass;
        if (MthruSpecies(true, ctx.SameSpeciesAsTarget)) return PitTraversalOutcome.StopPass;
        if (ctx.CorpseRipSkip) return PitTraversalOutcome.StopPass;
        if (ctx.TargetPassPlayer) return PitTraversalOutcome.StopPass;
        if (OverObject(ctx.MoverZ, ctx.ThingZ + ctx.ClipHeight)) return PitTraversalOutcome.StopPass;
        if (UnderObject(ctx.MoverTop, ctx.ThingZ)) return PitTraversalOutcome.StopPass;
        if (BounceActorZeroDamage((ctx.BounceActors & 1) != 0, ctx.ZeroDamage, ctx.IsMissile))
        {
            var pass = (ctx.SameTarget && !ctx.Hitowner) || !ctx.ThingSolid;
            if (pass && ctx.SpecialHit != 0) return PitTraversalOutcome.StopPass;
            if (!pass && ctx.SpecialHit == 0) return PitTraversalOutcome.StopBlock;
        }
        switch (ctx.SpecialHit)
        {
            case 0: return PitTraversalOutcome.StopBlock;
            case 1: return PitTraversalOutcome.StopPass;
        }
        if (ctx.SameTarget && !ctx.Hitowner) return PitTraversalOutcome.StopPass;
        if (!ctx.CanAttack) return PitTraversalOutcome.StopBlock;
        if (!ctx.Shootable) return ctx.ThingSolid ? PitTraversalOutcome.StopBlock : PitTraversalOutcome.StopPass;
        if (SpectralMismatch(ctx.Spectral, ctx.MissileSpectral)) return PitTraversalOutcome.StopPass;
        if (DoRipping(ctx.DoRip, ctx.DontRip) && RipperDamageDue(ctx.DoRip, ctx.DontRip, ctx.Boss, ctx.NoBossRip, ctx.RipLevelOk))
        {
            if (!BossRipBlocked(ctx.Boss, ctx.NoBossRip) && ctx.FirstRip)
            {
                if (PushablePushed(ctx.Pushable, ctx.CannotPush, true)) { }
                return PitTraversalOutcome.StopPass;
            }
        }
        if (ReflectiveTurnSeeker(ctx.ThruReflect, ctx.Reflective, ctx.IsMissile) && ctx.Seeker) { }
        return PitTraversalOutcome.StopBlock;
    }

    public readonly record struct PitSolidPickupContext(
        bool MirrorBlocks,
        bool ThingSolid,
        bool Noclip,
        bool MoverSolid,
        bool BlockedBySolidActors,
        bool Special,
        bool Pickup,
        double ThingZ,
        double TopMinusStep,
        bool Unblocking);

    /// <summary>Solid blocking, pickup touch, and killough passobj tail.</summary>
    public static PitTraversalOutcome EvaluateSolidAndPickup(PitSolidPickupContext ctx)
    {
        var solid = SolidBlock(ctx.ThingSolid, ctx.MirrorBlocks, ctx.Noclip, ctx.MoverSolid);
        if (!solid && ctx.BlockedBySolidActors)
            solid = ctx.MoverSolid;
        if (PickupTouch(ctx.Special, ctx.Pickup, ctx.ThingZ, ctx.TopMinusStep)) { }
        return ReturnPassSolid(solid, ctx.Unblocking)
            ? PitTraversalOutcome.StopPass
            : PitTraversalOutcome.StopBlock;
    }
}

/// <summary>Lightweight stand-in for tests.</summary>
public readonly record struct AActorRef(int Id);
