namespace HCDE.Playsim;

/// <summary><c>MorphPointerSubstitution</c> tail and <c>SpawnPlayer</c> bookends from <c>p_mobj.cpp</c> (6191–6339, 6423–6560).</summary>
public static class VmPMobjMorphSpawn
{
    public const int Mf4Noskin = 1;
    public const int PstLive = 0;
    public const int PstEnter = 1;
    public const int PstReborn = 2;
    public const int SpfTempplayer = 1;
    public const int SpfWeaponfullyup = 1 << 1;
    public const int Level2Allowrespawn = 1;
    public const int CfChasecam = 1;

    public readonly record struct MorphLinkContext(bool FromHadAlternative, bool FromHadPlayer);

    public static bool ClearUnmorphedOnUnmorph(MorphLinkContext ctx) => ctx.FromHadAlternative;

    public static bool SetUnmorphedDummyLink(MorphLinkContext ctx) => !ctx.FromHadAlternative;

    public static bool TransferPlayerBody(MorphLinkContext ctx) => ctx.FromHadPlayer;

    public readonly record struct SpawnPlayerGateContext(
        bool MthingNull,
        int PlayerNum,
        int MaxPlayers,
        bool InGame);

    public static bool SpawnPlayerAllowed(SpawnPlayerGateContext ctx) =>
        !ctx.MthingNull
        && ctx.PlayerNum >= 0
        && ctx.PlayerNum < ctx.MaxPlayers
        && ctx.InGame;

    public static bool ApplySkinSprite(int flags4) => (flags4 & Mf4Noskin) == 0;

    public static bool RedirectCamera(bool inGame, bool cameraWasOldActor) =>
        inGame && cameraWasOldActor;

    public static bool EnableChasecam(bool demoPlayback, bool demoNew, bool chaseDemoCvar) =>
        (demoPlayback || demoNew) && chaseDemoCvar;

    public static bool SetupPspritesEligible(int spawnFlags, bool oldActorNull) =>
        (spawnFlags & SpfTempplayer) == 0 || oldActorNull;

    public readonly record struct CoopRespawnInventoryContext(
        bool Multiplayer,
        int LevelFlags2,
        bool SinglePlayerRespawnCvar,
        bool SkillAllowsRespawn,
        int PlayerState,
        bool OldActorNotNull);

    public static bool FilterCoopRespawnInventory(CoopRespawnInventoryContext ctx) =>
        (ctx.Multiplayer
         || (ctx.LevelFlags2 & Level2Allowrespawn) != 0
         || ctx.SinglePlayerRespawnCvar
         || ctx.SkillAllowsRespawn)
        && ctx.PlayerState == PstReborn
        && ctx.OldActorNotNull;

    public static bool DestroyLeftoverInventory(bool oldActorNotNull, bool usedCoopFilter) =>
        oldActorNotNull && !usedCoopFilter;

    public static bool CallOnRespawn(int state) => state == PstReborn || state == PstEnter;

    public static bool AttachStatusBar(bool statusBarNotNull, bool consoleOrBarPlayer, bool matchesBarPlayer) =>
        statusBarNotNull && (consoleOrBarPlayer || matchesBarPlayer);

    public static bool SinkBelowCeiling(double top, double ceilingZ) => top > ceilingZ;

    public static double SinkZ(double ceilingZ, double height) => ceilingZ - height;

    public readonly record struct SpawnScriptContext(
        int SpawnFlags,
        bool OldActorNull,
        int PlayerState,
        bool SavegameRestore);

    public static bool RunEnterScripts(SpawnScriptContext ctx) =>
        ScriptEligible(ctx) && (ctx.PlayerState == PstEnter
            || (ctx.PlayerState == PstLive && !ctx.SavegameRestore));

    public static bool RunRebornScripts(SpawnScriptContext ctx) =>
        ScriptEligible(ctx) && ctx.PlayerState == PstReborn && !ctx.OldActorNull;

    public static bool ScriptEligible(SpawnScriptContext ctx) =>
        (ctx.SpawnFlags & SpfTempplayer) == 0 || ctx.OldActorNull;

    public static bool HeldWeaponUsable(bool weaponNull, bool weaponEuthanized) =>
        !weaponNull && !weaponEuthanized;

    public static bool PspriteWeaponFullyUp(int spawnFlags) =>
        (spawnFlags & SpfWeaponfullyup) != 0;

    public readonly record struct DestroyOldActorContext(
        bool OldActorNotNull,
        bool CoopFilterRan);

    public static bool StopOldBehaviors(DestroyOldActorContext ctx) =>
        DestroyLeftoverInventory(ctx.OldActorNotNull, ctx.CoopFilterRan);

    public static bool ClearBehaviorsOnDestroy(DestroyOldActorContext ctx) =>
        ctx.OldActorNotNull && !ctx.CoopFilterRan;

    public const int MfUnmorphed = 1;

    public readonly record struct MorphAlternativeFinalizeContext(bool FromHadAlternative);

    public static bool ClearDummyUnmorphFlag(MorphAlternativeFinalizeContext ctx) => ctx.FromHadAlternative;

    public static bool LinkMorphPartners(MorphAlternativeFinalizeContext ctx) => !ctx.FromHadAlternative;

    public static int ApplyUnmorphFlags(int toFlags, bool unmorphing) =>
        unmorphing ? toFlags & ~MfUnmorphed : toFlags | MfUnmorphed;

    public readonly record struct CoopHeldWeaponContext(bool RememberLastWeapon, bool WeaponNull, bool WeaponEuthanized);

    public static bool PassHeldWeaponToCoopFilter(CoopHeldWeaponContext ctx) =>
        ctx.RememberLastWeapon && HeldWeaponUsable(ctx.WeaponNull, ctx.WeaponEuthanized);

    public static bool PassNullHeldWeapon(CoopHeldWeaponContext ctx) =>
        ctx.RememberLastWeapon && ctx.WeaponNull;

    public readonly record struct RebornMaintenanceContext(bool RunRebornScripts);

    public static bool ClearActorLastHeard(RebornMaintenanceContext ctx) => ctx.RunRebornScripts;

    public static bool ClearSectorTargets(RebornMaintenanceContext ctx) => ctx.RunRebornScripts;

    public static bool SubstitutePlayerPointers(RebornMaintenanceContext ctx) => ctx.RunRebornScripts;

    public static bool FireRespawnEvent(RebornMaintenanceContext ctx) => ctx.RunRebornScripts;

    public static bool AttachBarForConsole(bool statusBarNotNull, bool consolePlayer, bool barMatchesPlayer) =>
        AttachStatusBar(statusBarNotNull, consolePlayer, barMatchesPlayer);

    public static bool SkipDestroyWhenCoopFiltered(bool oldActorNotNull, bool coopFilterRan) =>
        oldActorNotNull && coopFilterRan;
}
