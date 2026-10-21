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
            source: null, findActorClass: null, getDefaultFlags: null, getDefaultRadius: null, getSourceRadius: null,
            vec3Angle: null, spawnActor: null, findClass: null, isVisualThinkerDescendant: null, newVisualThinker: null,
            spawnPlayerMissile: null, applySpawnedState: null);
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
}
