using HCDE.Net.Core;
using HCDE.Net.Transport;
using HCDE.Playsim;

namespace HCDE.Server;

/// <summary>Maps the managed director to the native wire state IDs (not its enum ordinals).</summary>
public static class InvasionSnapshotPublisher
{
    public static InvasionSnapshotHeader Capture(InvasionDirector director) => new(
        flags: 0,
        state: !director.Enabled ? LiveConstants.InvasionStateDisabled : director.Phase switch
        {
            InvasionPhase.Waiting => LiveConstants.InvasionStateWaiting,
            InvasionPhase.Countdown => LiveConstants.InvasionStateCountdown,
            // This subset spawns the whole wave at once, then waits for kills.
            InvasionPhase.Wave => LiveConstants.InvasionStateCleanup,
            InvasionPhase.Intermission => LiveConstants.InvasionStateIntermission,
            InvasionPhase.Victory => LiveConstants.InvasionStateVictory,
            _ => LiveConstants.InvasionStateDisabled,
        },
        stateTics: (uint)director.Cooldown,
        wave: (uint)director.Wave,
        maxWaves: (uint)director.MaxWaves,
        waveBudget: (uint)director.Spawned,
        waveSpawned: (uint)director.Spawned,
        waveCleared: (uint)director.Cleared,
        activeMonsters: (uint)director.ActiveMonsters,
        spawnSpotCount: (ushort)Math.Min(director.SpawnSpotCount, ushort.MaxValue),
        activeSpawnSpotCount: (ushort)Math.Min(director.SpawnSpotCount, ushort.MaxValue),
        spawnPlanBudget: (uint)director.Spawned,
        spawnFlags: director.Wave > 0 ? LiveConstants.InvasionSnapshotSpawnFlagUsingFallback : (byte)0,
        spawnFallbackSource: director.SpawnSpotCount > 0 ? LiveConstants.InvasionSpawnSourceMapSpot : LiveConstants.InvasionSpawnSourceNone);

    public static void ApplyToQuery(InvasionDirector director, ServerQuerySnapshot query)
    {
        var snapshot = Capture(director);
        query.InvasionState = snapshot.State;
        query.InvasionStateName = director.Enabled ? director.Phase.ToString() : "Disabled";
        query.InvasionStateTics = ToQuery(snapshot.StateTics);
        query.InvasionWave = ToQuery(snapshot.Wave);
        query.InvasionMaxWaves = ToQuery(snapshot.MaxWaves);
        query.InvasionWaveBudget = ToQuery(snapshot.WaveBudget);
        query.InvasionWaveSpawned = ToQuery(snapshot.WaveSpawned);
        query.InvasionWaveCleared = ToQuery(snapshot.WaveCleared);
        query.InvasionActiveMonsters = ToQuery(snapshot.ActiveMonsters);
        query.InvasionSpawnSpotCount = snapshot.SpawnSpotCount;
        query.InvasionSpawnActiveSpotCount = snapshot.ActiveSpawnSpotCount;
        query.InvasionSpawnPlanBudget = ToQuery(snapshot.SpawnPlanBudget);
        query.InvasionSpawnFlags = snapshot.SpawnFlags;
    }

    private static ushort ToQuery(uint value) => (ushort)Math.Min(value, ushort.MaxValue);
}
