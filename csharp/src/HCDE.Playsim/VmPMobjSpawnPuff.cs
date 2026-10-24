namespace HCDE.Playsim;

/// <summary><c>SpawnMapThing</c> index wrapper and <c>P_SpawnPuff</c> from <c>p_mobj.cpp</c> (7095–7106, 7116–7260).</summary>
public static class VmPMobjSpawnPuff
{
    public const int PfNorandomz = 1;
    public const int PfHitsky = 1 << 1;
    public const int PfHitthing = 1 << 2;
    public const int PfHitthingbleed = 1 << 3;
    public const int PfMeleerange = 1 << 4;
    public const int PfTemporary = 1 << 5;
    public const int Mf4Randomize = 1;
    public const int Mf4Allowparticles = 1 << 1;
    public const int Mf5Puffgetsowner = 1;
    public const int Mf7Hittarget = 1;
    public const int Mf7Hitmaster = 1 << 1;
    public const int Mf7Hittracer = 1 << 2;
    public const int Mf9Ispuff = 1;
    public const int RfInvisible = 1;

    public static bool BeginInvasionRegistration(int index) => index == 0;

    public static bool PuffTypeValid(bool puffNull) => !puffNull;

    public static bool ApplyRandomZJitter(int flags) => (flags & PfNorandomz) == 0;

    public static bool RandomizePuffTics(int flags4) => (flags4 & Mf4Randomize) != 0;

    public static int ClampPuffTics(int tics) => tics < 1 ? 1 : tics;

    public static bool AssignVictimTarget(int flags7, bool hasVictim) =>
        hasVictim && (flags7 & Mf7Hittarget) != 0;

    public static bool AssignVictimMaster(int flags7, bool hasVictim) =>
        hasVictim && (flags7 & Mf7Hitmaster) != 0;

    public static bool AssignVictimTracer(int flags7, bool hasVictim) =>
        hasVictim && (flags7 & Mf7Hittracer) != 0;

    public static bool AssignOwnerTarget(int flags5) => (flags5 & Mf5Puffgetsowner) != 0;

    public static double OppositeHitYaw(double hitDirDeg) => hitDirDeg + 180;

    public static bool UseSkyCrashState(int flags, bool hasState) =>
        (flags & PfHitsky) != 0 && hasState;

    public static bool UseWallCrashState(int flags, bool hasState) =>
        (flags & PfHitthing) == 0 && hasState;

    public static bool UseExtremeBleedState(int flags, bool hasState) =>
        (flags & PfHitthingbleed) != 0 && hasState;

    public static bool UseMeleeState(int flags, bool hasMeleeState) =>
        (flags & PfMeleerange) != 0 && hasMeleeState;

    public readonly record struct PuffParticleContext(
        int Flags,
        int UpDown,
        int PuffTypeCvar,
        int Flags4);

    public static bool DrawSplashParticles(PuffParticleContext ctx) =>
        (ctx.Flags & PfTemporary) == 0
        && ctx.PuffTypeCvar != 0
        && ctx.UpDown != 3
        && (ctx.Flags4 & Mf4Allowparticles) != 0;

    public static bool HidePuffAfterSplash(PuffParticleContext ctx) => ctx.PuffTypeCvar == 1;

    public static int ApplyInvisibleRenderFlags(int renderFlags) => renderFlags | RfInvisible;

    public static bool MeleeRumbleFromPlayer(bool meleeRange, bool sourceIsPlayer, bool consoleMoMatch) =>
        meleeRange && sourceIsPlayer && consoleMoMatch;

    public static bool PlaySeeSoundOnHit(int flags, bool seeSoundValid) =>
        (flags & PfHitthing) != 0 && seeSoundValid;

    public static bool PlayAttackSoundFallback(int flags, bool seeSoundValid, bool attackSoundValid) =>
        (flags & PfHitthing) == 0 && !seeSoundValid && attackSoundValid;

    public readonly record struct CrashStatePickContext(
        int Flags,
        bool SkyState,
        bool CrashState,
        bool ExtremeState,
        bool MeleeState);

    public static bool PickSkyCrash(CrashStatePickContext ctx) =>
        UseSkyCrashState(ctx.Flags, ctx.SkyState);

    public static bool PickMissCrash(CrashStatePickContext ctx) =>
        !PickSkyCrash(ctx) && UseWallCrashState(ctx.Flags, ctx.CrashState);

    public static bool PickBleedCrash(CrashStatePickContext ctx) =>
        !PickSkyCrash(ctx) && !PickMissCrash(ctx) && UseExtremeBleedState(ctx.Flags, ctx.ExtremeState);

    public static bool PickMeleeCrash(CrashStatePickContext ctx) =>
        !PickSkyCrash(ctx) && !PickMissCrash(ctx) && !PickBleedCrash(ctx)
        && UseMeleeState(ctx.Flags, ctx.MeleeState);

    public static bool SkipEffectsWhenTemporary(int flags) => (flags & PfTemporary) != 0;
}
