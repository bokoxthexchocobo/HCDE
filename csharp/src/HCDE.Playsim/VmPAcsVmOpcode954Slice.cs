namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> HUD size, level info, sky, camera, actor aim/state, and player
/// query opcodes from <c>p_acs.cpp</c> (**10287–10436**). Exclusive to chunk **954**.
/// </summary>
public static class VmPAcsVmOpcode954Slice
{
    public const int LevelInfoParTime = 0;
    public const int LevelInfoClusterNum = 1;
    public const int LevelInfoLevelNum = 2;
    public const int LevelInfoTotalSecrets = 3;
    public const int LevelInfoFoundSecrets = 4;
    public const int LevelInfoTotalItems = 5;
    public const int LevelInfoFoundItems = 6;
    public const int LevelInfoTotalMonsters = 7;
    public const int LevelInfoKilledMonsters = 8;
    public const int LevelInfoSuckTime = 9;

    public const int SetHudSizeStackPop = 3;
    public const int ChangeSkyStackPop = 2;
    public const int SetCameraToTextureStackPop = 3;
    public const int SetActorAngleStackPop = 2;
    public const int SetActorStateStackPop = 2;
    public const int UnresolvedAcsPlayer = -1;

    public const double AimDistBamScale = 0x40000000 / 90.0;

    public static (int Width, int Height) ResolveHudSize(int rawWidth, int rawHeight, int coverStatusBarFlag)
    {
        var width = Math.Abs(rawWidth);
        var height = Math.Abs(rawHeight);
        if (coverStatusBarFlag != 0)
            height = -height;
        return (width, height);
    }

    public static int ResolveLevelInfoValue(int query, in LevelInfoSnapshot level)
    {
        return query switch
        {
            LevelInfoParTime => level.ParTime,
            LevelInfoSuckTime => level.SuckTime,
            LevelInfoClusterNum => level.Cluster,
            LevelInfoLevelNum => level.LevelNum,
            LevelInfoTotalSecrets => level.TotalSecrets,
            LevelInfoFoundSecrets => level.FoundSecrets,
            LevelInfoTotalItems => level.TotalItems,
            LevelInfoFoundItems => level.FoundItems,
            LevelInfoTotalMonsters => level.TotalMonsters,
            LevelInfoKilledMonsters => level.KilledMonsters,
            _ => 0,
        };
    }

    public static bool SkyNameApplies(ReadOnlySpan<char> skyName) =>
        skyName.Length > 0 && skyName[0] != '\0';

    public static bool UseActivatorAsCamera(int tid) => tid == 0;

    public static bool SetActorStateTargetsActivator(int tid) => tid == 0;

    public static int SetActorStateSuccessResult(bool stateFound) => stateFound ? 1 : 0;

    public static int AccumulateSetActorStateCount(int current, bool stateFound) =>
        stateFound ? current + 1 : current;

    public static int ResolvePlayerClass(bool playerResolved, int currentPlayerClass) =>
        playerResolved ? currentPlayerClass : UnresolvedAcsPlayer;

    public static int ResolveGetPlayerInfoStackSlot(bool playerResolved) =>
        playerResolved ? 0 : UnresolvedAcsPlayer;

    public static int AimDistToAcs(double aimDist) => (int)(aimDist * AimDistBamScale);

    public readonly record struct LevelInfoSnapshot(
        int ParTime,
        int SuckTime,
        int Cluster,
        int LevelNum,
        int TotalSecrets,
        int FoundSecrets,
        int TotalItems,
        int FoundItems,
        int TotalMonsters,
        int KilledMonsters);
}
