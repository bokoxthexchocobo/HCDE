using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterClassificationSaveTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ClassificationSurvivesRuntimeChangeAndReconstruction(bool monster, bool reconstruct)
    {
        var sim = Room(); var actor = Target(sim); actor.IsMonster = monster;
        var saved = SimSavegame.Write(sim, true);
        if (reconstruct) { actor.Destroy(); sim.Tick(); } else actor.IsMonster = !monster;
        SimSavegame.Apply(sim, saved); Assert.Equal(monster, Target(sim).IsMonster);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(4194304, true)]
    public void OlderSnapshotRestoresPatchedSpawnDefault(int bits, bool expected)
    {
        var sim = Room(DehackedPatch.Apply($"Thing 2\nBits = {bits}\n")); var actor = Target(sim);
        var saved = SimSavegame.Write(sim); actor.IsMonster = !expected; SimSavegame.Apply(sim, saved);
        Assert.Equal(expected, actor.IsMonster);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredClassificationControlsInfightingDamage(bool monster)
    {
        var sim = Room(); var target = Target(sim); var source = sim.AddBot(200, 0, 3001);
        target.NoInfighting = true; target.IsMonster = monster;
        var saved = SimSavegame.Write(sim); target.IsMonster = !monster; SimSavegame.Apply(sim, saved);
        Assert.Equal(monster ? 0 : 1, ActorDamage.Apply(target, 1, source, inflictor: source).HealthLost);
    }

    [Fact]
    public void PlayerClassificationOverrideSurvivesRestore()
    {
        var sim = Room(); var player = sim.Players.Single(); player.IsMonster = true;
        var saved = SimSavegame.Write(sim); player.IsMonster = false; SimSavegame.Apply(sim, saved);
        Assert.True(player.IsMonster); Assert.Equal(saved, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidStructuredFlagRejectsBeforeMutation(int value)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].MonsterClassificationFlag = value;
        var checksum = sim.Checksum;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(0, 242, "save-monsterclassification-header")]
    [InlineData(8, 2, "save-monsterclassification-value")]
    public void MalformedArchiveIsRejected(int offset, int value, string expected)
    {
        var sim = Room(); Target(sim).IsMonster = false; var saved = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(saved, 242);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal(expected, error);
    }

    private static Actor Target(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3004 && !a.Destroyed);
    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }]
    }, dehacked: patch);
}
