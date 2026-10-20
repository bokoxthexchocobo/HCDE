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

    public static bool SkullflySlam(bool skullfly) => skullfly;

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

    public static bool RestrictedPortalBlocksBridge(bool restricted) => restricted;

    public static bool CompatNoPassobj(bool compat) => compat;

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

    public static bool PredictingStopsSolid(bool predicting) => predicting;

    public static bool BlastedDamageMass(int mass) => mass > 0;

    public static bool MissileHitNonshootable(bool nonshootable) => nonshootable;

    public static bool MthruSpecies(bool flag, bool same) => flag && same;

    public static bool TargetPassPlayer(bool targetPlayer, bool pass) => targetPlayer && pass;

    public static bool BounceActorZeroDamage(bool bounce, bool zero, bool missile) =>
        bounce && (zero || !missile);

    public static int SpecialMissileHitZero(int hit) => hit;

    public static int SpecialMissileHitOne(int hit) => hit;

    public static bool InfightAllowsTarget(bool canHurt) => canHurt;

    public static bool NonshootableSolid(bool shootable, bool solid) => !shootable && !solid;

    public static bool SpectralMismatch(bool spectral, bool missileSpectral) => spectral && !missileSpectral;

    public static bool DoRipping(bool doRip, bool dontRip) => doRip && !dontRip;

    public static bool PushFactorApply(bool pushable, bool newTime) => pushable && newTime;

    public static bool ConsoleRumblePush(bool localPlayer) => localPlayer;

    public static bool ConsoleRumblePushed(bool localPlayer) => localPlayer;

    public static bool MaxStepPickupGate(double z, double top, double step) => z < top - step;

    public static bool KilloughPassobj(bool passobj, bool compat) => passobj && !compat;

    public static bool NoclipPassSolid(bool noclip, bool solid) => noclip || !solid;

    public static bool MirrorBlocksWhenEnabled(bool mirror) => mirror;

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

    public static bool NetDiagTracePredict(bool success) => success;

    public static bool ClearSpecialOnSuccess(bool repeat, bool success) => !repeat && success;

    public static bool ChangeSwitchOnUse(bool success, int activationType) =>
        success && activationType is 1 or 2 or 3;

    public static bool HitownerAllowed(bool hitowner) => hitowner;

    public static bool BossRipBlocked(bool boss, bool noBossRip) => boss && noBossRip;

    public static bool LastRippedOnce(bool firstRip) => firstRip;

    public static bool PushableNotCannot(bool pushable, bool cannot) => pushable && !cannot;

    public static bool SpechitClearAfterRip(bool rip) => rip;
}

/// <summary>Lightweight stand-in for tests.</summary>
public readonly record struct AActorRef(int Id);
