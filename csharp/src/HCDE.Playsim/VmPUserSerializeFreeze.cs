namespace HCDE.Playsim;

/// <summary><c>player_t::Serialize</c>, <c>P_FallingDamage</c>, and freeze gate from <c>p_user.cpp</c>.</summary>
public static class VmPUserSerializeFreeze
{
    public const int CfTotallyfrozen = 1;
    public const int GsTitlelevel = 2;
    public const int Level2Monsterfallingdamage = 1;
    public const int Mf8Falldamage = 1;
    public const int SecfNofallingdamage = 1;
    public const int Level3Propermonsterfallingdamage = 1;

    public static bool SerializeRollbackBranch(bool isRollback) => isRollback;

    public static bool SerializeReadUserinfo(bool reading) => reading;

    public static bool SerializeWriteClearsUseButtons(bool writing) => writing;

    public static bool SerializeRestoreFullbrightOnRead(bool reading) => reading;

    public static bool SerializeApplySkinChange(bool skinNotEmpty) => skinNotEmpty;

    public static bool FallingDamageLevelFlag(int levelFlags2, int flag) =>
        (levelFlags2 & flag) != 0;

    public static bool FallingDamageActorFlag(int flags8, int flag) => (flags8 & flag) != 0;

    public static bool FallingDamageSectorBlocks(int sectorFlags, int noFall) =>
        (sectorFlags & noFall) != 0;

    public static bool FallingDamageTelefrag(double velZ) => Math.Abs(velZ) > 35;

    public static int FallingDamageScalar(double velZ) => (int)((Math.Abs(velZ) - 23) * 6);

    public static bool UseProperMonsterDamage(int levelFlags3, int flag) =>
        (levelFlags3 & flag) != 0;

    public static bool TotallyFrozenTitle(int gamestate, int gsTitle) => gamestate == gsTitle;

    public static bool TotallyFrozenCheat(int cheats, int frozen) => (cheats & frozen) != 0;

    public static bool TotallyFrozenActor(bool moFrozen) => moFrozen;

    public readonly record struct FreezeGate(int Gamestate, int Cheats, bool MoFrozen);

    public static bool EvaluateTotallyFrozen(FreezeGate g) =>
        TotallyFrozenTitle(g.Gamestate, GsTitlelevel)
        || TotallyFrozenCheat(g.Cheats, CfTotallyfrozen)
        || TotallyFrozenActor(g.MoFrozen);

    public readonly record struct FallingDamagePick(double VelZ, bool ProperMonster, int TelefragDamage);

    public static int PickFallingDamage(FallingDamagePick ctx) =>
        FallingDamageTelefrag(ctx.VelZ)
            ? ctx.TelefragDamage
            : ctx.ProperMonster
                ? FallingDamageScalar(ctx.VelZ)
                : ctx.TelefragDamage;

    public static bool SerializePersistsCamera(bool rollback) => !rollback;

    public static bool SerializePersistsInventoryTics(bool rollback) => !rollback;

    public static bool SerializeRollbackAttackDown(bool rollback) => rollback;

    public static bool SerializeRollbackUseDown(bool rollback) => rollback;

    public static bool SerializeFullbrightModeRoundTrip(int mode) => mode >= 0;

    public static bool SerializeFragsArray(bool hasArray) => hasArray;

    public static bool SerializePspritesPersist(bool rollback) => !rollback;

    public static bool SerializeSafePosHook(bool rollback) => !rollback;

    public static bool FallingDamageRequiresMonsterLevel(bool levelFlag, bool actorFlag) =>
        levelFlag || actorFlag;

    public static bool FallingDamageSkipSector(bool sectorNoFall) => sectorNoFall;

    public readonly record struct SerializeWriteTail(bool Writing, bool SkinNotEmpty, int FullbrightMode);

    public static bool ShouldClearOldButtons(SerializeWriteTail ctx) => ctx.Writing;

    public static bool ShouldRestoreFullbright(SerializeWriteTail ctx) => !ctx.Writing;

    public static bool ShouldApplySkin(SerializeWriteTail ctx) => ctx.SkinNotEmpty;

    public readonly record struct FallingDamageGate(
        int LevelFlags2,
        int Flags8,
        int SectorFlags,
        double VelZ,
        bool ProperMonster);

    public static bool ShouldApplyFallingDamage(FallingDamageGate g) =>
        FallingDamageRequiresMonsterLevel(
            FallingDamageLevelFlag(g.LevelFlags2, Level2Monsterfallingdamage),
            FallingDamageActorFlag(g.Flags8, Mf8Falldamage))
        && !FallingDamageSkipSector(FallingDamageSectorBlocks(g.SectorFlags, SecfNofallingdamage));

    public static int ComputeFallingDamage(FallingDamageGate g, int telefrag) =>
        ShouldApplyFallingDamage(g)
            ? PickFallingDamage(new FallingDamagePick(g.VelZ, g.ProperMonster, telefrag))
            : 0;

    public static bool TitleLevelFreezes(int gamestate) => TotallyFrozenTitle(gamestate, GsTitlelevel);

    public static bool PlayerMoFreeze(bool frozen) => TotallyFrozenActor(frozen);

    public static bool AnyFreezeReason(FreezeGate g) => EvaluateTotallyFrozen(g);

    public static bool SerializeCmdRoundTrip(bool rollback) => !rollback;

    public static bool SerializePlayerStateField(bool rollback) => !rollback;

    public static bool FallingDamagePositive(int damage) => damage > 0;
}
