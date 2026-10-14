using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoTargetSaveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredPlayerFlagControlsVisualAcquisition(bool noTarget)
    {
        var sim = Room(); var player = sim.Players.Single(); var hunter = Monster(sim);
        player.NoTarget = noTarget; var saved = SimSavegame.Write(sim); player.NoTarget = !noTarget;
        SimSavegame.Apply(sim, saved); hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(noTarget ? (uint?)null : player.Id, hunter.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagSurvivesMonsterReconstruction(bool noTarget)
    {
        var sim = Room(); var actor = Monster(sim); actor.NoTarget = noTarget;
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(noTarget, Monster(sim).NoTarget); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredFlagRetainsHatedTidException(bool hated)
    {
        var sim = Room(); var hunter = Monster(sim); var other = sim.AddBot(200, 0, 3001);
        other.ThingId = 7; other.NoTarget = true; if (hated) hunter.TidToHate = 7;
        var saved = SimSavegame.Write(sim); other.NoTarget = false; SimSavegame.Apply(sim, saved);
        Assert.Equal(hated, hunter.OkayToSwitchTarget(other, hunter.Brain!));
    }

    [Fact]
    public void OlderSnapshotClearsLaterNoTargetFlags()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim);
        foreach (var actor in sim.Actors) actor.NoTarget = true;
        SimSavegame.Apply(sim, saved); Assert.All(sim.Actors, actor => Assert.False(actor.NoTarget));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidStructuredFlagRejectsBeforeMutation(int value)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].NoTargetFlag = value;
        var checksum = sim.Checksum;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(0, 241, "save-notarget-header")]
    [InlineData(8, 2, "save-notarget-value")]
    public void MalformedArchiveIsRejected(int offset, int value, string expected)
    {
        var sim = Room(); sim.Players.Single().NoTarget = true; var saved = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(saved, 241);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal(expected, error);
    }

    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3004 && !a.Destroyed);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }]
    });
}
