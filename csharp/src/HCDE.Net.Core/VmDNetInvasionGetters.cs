namespace HCDE.Net.Core;

/// <summary>Invasion query helpers (<c>d_net.cpp</c> <c>Net_GetInvasion*</c> 9701–9911).</summary>
public static class VmDNetInvasionGetters
{
    public const uint OfEuthanizeMe = 1;

    public static int RawInvasionStateNative(int invasionState) => invasionState;

    public static int InvasionStateTicsNative(
        bool invasionAuthority,
        bool countdown,
        int invasionStateTics,
        int cutsceneCountdown) =>
        countdown && !invasionAuthority ? invasionStateTics : cutsceneCountdown;

    public static int ClassicInvasionStateNative(int invasionState) =>
        invasionState switch
        {
            1 => 5,
            2 or 3 => 6,
            4 => 7,
            5 => 8,
            6 => 9,
            _ => 0,
        };

    public static int NonNegativeWaveNative(int wave) => Math.Max(wave, 0);

    public static int NonNegativeMaxWavesNative(int maxWaves) => Math.Max(maxWaves, 0);

    public static int NonNegativeBudgetNative(int budget) => Math.Max(budget, 0);

    public static int NonNegativeSpawnedNative(int spawned) => Math.Max(spawned, 0);

    public static int NonNegativeClearedNative(int cleared) => Math.Max(cleared, 0);

    public static int ReplicatedActiveMonsterCountNative(bool invasionAuthority, int replicated, int directorCount) =>
        !invasionAuthority ? Math.Max(replicated, 0) : Math.Max(directorCount, 0);

    public const uint InvWavefBoss = 1;

    public static bool IsBossWaveNative(uint waveFlags, uint bossFlag) => (waveFlags & bossFlag) != 0;

    public static bool ArchvileMonsterEligibleNative(int health, uint objectFlags, uint euthanizeMe) =>
        health > 0 && (objectFlags & euthanizeMe) == 0;

    public static int IncrementArchvileCountNative(int count) => count + 1;

    public static int ClientReplicatedArchvileCountNative(int replicated) => Math.Max(replicated, 0);

    public static bool UseReplicatedArchvileCountNative(bool invasionAuthority) => !invasionAuthority;

    public static int CountArchvilesAmongActiveMonstersNative(
        ReadOnlySpan<ArchvileMonsterSnapshot> activeMonsters,
        uint euthanizeMe,
        bool archvileClassResolved)
    {
        if (!archvileClassResolved)
            return 0;
        var count = 0;
        foreach (ref readonly var snap in activeMonsters)
        {
            if (snap.Mobj is null)
                continue;
            if (!ArchvileMonsterEligibleNative(snap.Health, snap.ObjectFlags, euthanizeMe))
                continue;
            if (!snap.IsArchvileKind)
                continue;
            count = IncrementArchvileCountNative(count);
        }
        return count;
    }

    public static int SpawnSpotCountNative(int totalSpotCount) => Math.Max(totalSpotCount, 0);

    public static int ActiveSpawnSpotCountNative(int activeSpotCount) => Math.Max(activeSpotCount, 0);

    public static int SpawnPlanBudgetNative(int totalSpawnBudget) => Math.Max(totalSpawnBudget, 0);

    public static bool InvasionSpawnHasActiveTagNative(int activeTag) => activeTag != 0;

    public static bool InvasionControlAllowedNative(bool invasionAuthority, bool invasionModeEnabled) =>
        invasionAuthority && invasionModeEnabled;

    public static int ResolveControlInvasionNative(
        bool invasionAuthority,
        bool invasionModeEnabled,
        int action,
        int invctlNextWave,
        int invctlVictory,
        int invctlFailure)
    {
        if (!InvasionControlAllowedNative(invasionAuthority, invasionModeEnabled))
            return 0;
        if (action == invctlNextWave)
            return 1;
        if (action == invctlVictory)
            return 1;
        if (action == invctlFailure)
            return 1;
        return InvasionControlDefaultNative(action, invctlNextWave, invctlVictory, invctlFailure);
    }

    public static int InvasionControlNextWaveNative(int invctlNextWave, int action) =>
        action == invctlNextWave ? 1 : 0;

    public static int InvasionControlVictoryNative(int invctlVictory, int action) =>
        action == invctlVictory ? 1 : 0;

    public static int InvasionControlFailureNative(int invctlFailure, int action) =>
        action == invctlFailure ? 1 : 0;

    public static int InvasionControlDefaultNative(int action, int invctlNextWave, int invctlVictory, int invctlFailure) =>
        action != invctlNextWave && action != invctlVictory && action != invctlFailure ? 0 : 1;
}

public readonly struct ArchvileMonsterSnapshot(object? mobj, int health, uint objectFlags, bool isArchvileKind)
{
    public object? Mobj { get; } = mobj;
    public int Health { get; } = health;
    public uint ObjectFlags { get; } = objectFlags;
    public bool IsArchvileKind { get; } = isArchvileKind;
}
