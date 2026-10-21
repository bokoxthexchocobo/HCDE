namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandSummonTests
{
    [Fact]
    public void ApplyCheckAutosaveNative_WritesWhenEligible()
    {
        byte? written = null;
        VmDNetDoCommandSummon.ApplyCheckAutosaveNative(
            netgame: false, demoplayback: false, disableAutosave: 0, autosaveCount: 1,
            playerState: 0, liveState: 0, deathmatch: false, b => written = b);
        Assert.NotNull(written);
    }

    [Fact]
    public void ApplySummonDoCommandNative_NoOpsWithoutSource()
    {
        VmDNetDoCommandSummon.ApplySummonDoCommandNative(
            cmd: 0, player: 0, demSummon2: 0, demSummonFoe2: 0, demSummonFriend: 0, demSummonFriend2: 0,
            demSummonMbf: 0, demSummonFoe: 0, actorClassName: "imp", extended: new SummonExtendedFields(0, 0, 0, []),
            sourceYaw: 0, source: null, findActorClass: null, getDefaultFlags: null, getDefaultRadius: null,
            getSourceRadius: null, vec3Angle: null, spawnActor: null, findClass: null, isVisualThinkerDescendant: null,
            newVisualThinker: null, spawnPlayerMissile: null, setConsoleThingSpawnFlag: null, countsAsKill: null,
            decrementTotalMonsters: null, setFriendPlayer: null, addFriendlyFlag: null, setLastHeard: null,
            restoreSpawnHealth: null, clearFriendlyFlag: null, setSpawnYaw: null, setSpecial: null, setSpawnArg: null,
            setTid: null, addNoBlockMonsters: null, setVisualThinkerPosition: null);
    }

    [Fact]
    public void ApplyRunSpecialNative_ClampsArgs()
    {
        int? invoked = null;
        VmDNetDoCommandSummon.ApplyRunSpecialNative(
            specialNumber: 1,
            argCount: 6,
            player: 0,
            consolePlayer: 0,
            streamArgs: [1, 2, 3, 4, 5, 99],
            checkCheatMode: _ => false,
            executeSpecial: (_, a0, _, _, _, _) => invoked = a0);
        Assert.Equal(1, invoked);
    }

    [Fact]
    public void ApplySummonDoCommandNative_SpawnsActorWhenResolved()
    {
        object? spawned = null;
        VmDNetDoCommandSummon.ApplySummonDoCommandNative(
            cmd: 0, player: 0, demSummon2: 0, demSummonFoe2: 0, demSummonFriend: 0, demSummonFriend2: 0,
            demSummonMbf: 0, demSummonFoe: 0, actorClassName: "imp", extended: new SummonExtendedFields(0, 0, 0, []),
            sourceYaw: 0, source: "src", findActorClass: _ => "type", getDefaultFlags: _ => 0,
            getDefaultRadius: _ => 16, getSourceRadius: _ => 8,
            vec3Angle: (_, _, _) => (1, 2, 3),
            spawnActor: (_, _, _) => { spawned = "actor"; return spawned; },
            findClass: null, isVisualThinkerDescendant: null, newVisualThinker: null,
            spawnPlayerMissile: null, setConsoleThingSpawnFlag: null, countsAsKill: null,
            decrementTotalMonsters: null, setFriendPlayer: null, addFriendlyFlag: null, setLastHeard: null,
            restoreSpawnHealth: null, clearFriendlyFlag: null, setSpawnYaw: null, setSpecial: null, setSpawnArg: null,
            setTid: null, addNoBlockMonsters: null, setVisualThinkerPosition: null);
        Assert.NotNull(spawned);
    }

    [Fact]
    public void ApplyFovDoCommandNative_PrintsWhenChanged()
    {
        string? printed = null;
        VmDNetDoCommandSummon.ApplyFovDoCommandNative(
            newFov: 110f, desiredFov: 90f, player: 0, authoritySlot: true,
            networkClients: [0, 1], setDesiredFov: null, printFov: s => printed = s);
        Assert.Contains("110", printed);
    }

    [Fact]
    public void ApplyCheckAutosaveNative_SkipsDuringNetgame()
    {
        byte? written = null;
        VmDNetDoCommandSummon.ApplyCheckAutosaveNative(
            netgame: true, demoplayback: false, disableAutosave: 0, autosaveCount: 1,
            playerState: 0, liveState: 0, deathmatch: false, b => written = b);
        Assert.Null(written);
    }
}
