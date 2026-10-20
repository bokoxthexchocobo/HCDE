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
}
