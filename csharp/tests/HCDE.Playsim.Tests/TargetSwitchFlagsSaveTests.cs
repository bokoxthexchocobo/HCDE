using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TargetSwitchFlagsSaveTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(7, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(7, true)]
    public void FlagCombinationsSurviveIdentityReconstruction(int flags, bool reconstruct)
    {
        var sim = Room(); var actor = Hunter(sim); Set(actor, flags); var saved = SimSavegame.Write(sim, true);
        if (reconstruct) { actor.Destroy(); sim.Tick(); } else Set(actor, flags ^ 7);
        SimSavegame.Apply(sim, saved); actor = Hunter(sim);
        Assert.Equal(flags, Flags(actor)); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredNeverTargetStillControlsRetaliation(bool neverTarget)
    {
        var sim = Room(); var hunter = Hunter(sim); var source = sim.Players.Single();
        source.NeverTarget = neverTarget; var saved = SimSavegame.Write(sim); source.NeverTarget = !neverTarget;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(!neverTarget, hunter.OkayToSwitchTarget(source, hunter.Brain!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredQuickRetaliationStillBypassesThreshold(bool quick)
    {
        var sim = Room(); var hunter = Hunter(sim); hunter.Brain!.SetChaseThreshold(100, false);
        hunter.QuickToRetaliate = quick; var saved = SimSavegame.Write(sim); hunter.QuickToRetaliate = !quick;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(quick, hunter.OkayToSwitchTarget(sim.Players.Single(), hunter.Brain));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredNoTargetSwitchRetainsExistingTarget(bool noSwitch)
    {
        var sim = Room(); var hunter = Hunter(sim); var player = sim.Players.Single();
        hunter.SetTargetPointer(player); hunter.Brain!.SetChaseThreshold(0, false); hunter.NoTargetSwitch = noSwitch;
        var saved = SimSavegame.Write(sim); hunter.NoTargetSwitch = !noSwitch; SimSavegame.Apply(sim, saved);
        Assert.Equal(!noSwitch, hunter.OkayToSwitchTarget(player, hunter.Brain));
    }

    [Fact]
    public void OlderSnapshotClearsLaterFlags()
    {
        var sim = Room(); var hunter = Hunter(sim); var saved = SimSavegame.Write(sim);
        Set(hunter, 7); SimSavegame.Apply(sim, saved); Assert.Equal(0, Flags(hunter));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void InvalidStructuredBitsRejectBeforeMutation(int flags)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].TargetSwitchFlags = flags;
        var checksum = sim.Checksum;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(0, 239, "save-targetswitch-header")]
    [InlineData(8, 8, "save-targetswitch-value")]
    public void MalformedArchiveIsRejected(int offset, int value, string expected)
    {
        var sim = Room(); Set(Hunter(sim), 7); var saved = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(saved, 239);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal(expected, error);
    }

    private static int Flags(Actor actor) => (actor.NeverTarget ? 1 : 0) | (actor.NoTargetSwitch ? 2 : 0) | (actor.QuickToRetaliate ? 4 : 0);
    private static void Set(Actor actor, int flags)
    {
        actor.NeverTarget = (flags & 1) != 0; actor.NoTargetSwitch = (flags & 2) != 0; actor.QuickToRetaliate = (flags & 4) != 0;
    }
    private static Actor Hunter(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3004 && !a.Destroyed);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }]
    });
}
