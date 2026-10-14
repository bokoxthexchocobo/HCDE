using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RewindClientViewTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 63)]
    [InlineData(2, 0)]
    [InlineData(3, 63)]
    public void RecordsSequenceInRawGameTicksAndKeepsClientsIndependent(int ticDup, int slot)
    {
        var rewind = new RewindBuffer();
        Assert.Equal(-1, rewind.GetClientViewTic(slot));
        Assert.True(rewind.TryRecordClientViewTic(slot, 17, ticDup));
        Assert.Equal(17 * Math.Max(ticDup, 1), rewind.GetClientViewTic(slot));
        Assert.Equal(-1, rewind.GetClientViewTic(slot == 0 ? 63 : 0));
    }

    [Theory]
    [InlineData(-1, 4, 1)]
    [InlineData(64, 4, 1)]
    [InlineData(0, -1, 1)]
    [InlineData(0, int.MaxValue, 2)]
    public void InvalidRecordReturnsFalseWithoutChangingPreviouslyRecordedState(int slot, int sequence, int ticDup)
    {
        var rewind = new RewindBuffer(); Assert.True(rewind.TryRecordClientViewTic(0, 7));
        Assert.False(rewind.TryRecordClientViewTic(slot, sequence, ticDup));
        Assert.Equal(7, rewind.GetClientViewTic(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void InvalidClientLookupReturnsNativeUnknownSentinel(int slot)
    {
        Assert.Equal(-1, new RewindBuffer().GetClientViewTic(slot));
    }

    [Fact]
    public void ZeroSequenceIsKnownAndOlderRecordsReplaceNewerRecordsLikeNative()
    {
        var rewind = new RewindBuffer();
        Assert.True(rewind.TryRecordClientViewTic(0, 0)); Assert.Equal(0, rewind.GetClientViewTic(0));
        Assert.True(rewind.TryRecordClientViewTic(0, 100)); Assert.Equal(100, rewind.GetClientViewTic(0));
        Assert.True(rewind.TryRecordClientViewTic(0, 20)); Assert.Equal(20, rewind.GetClientViewTic(0));
    }

    [Fact]
    public void NonpositiveTicDupClampsAndLargestRepresentableTickRemainsValid()
    {
        var rewind = new RewindBuffer();
        Assert.True(rewind.TryRecordClientViewTic(0, int.MaxValue, int.MinValue));
        Assert.Equal(int.MaxValue, rewind.GetClientViewTic(0));
        Assert.True(rewind.TryRecordClientViewTic(0, int.MaxValue / 3, 3));
        Assert.Equal(int.MaxValue / 3 * 3, rewind.GetClientViewTic(0));
        Assert.False(rewind.TryRecordClientViewTic(0, int.MaxValue / 3 + 1, 3));
        Assert.Equal(int.MaxValue / 3 * 3, rewind.GetClientViewTic(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitResetAndDisableInvalidateAllClientViews(bool disable)
    {
        var sim = Room(); sim.RewindEnabled = true;
        for (var slot = 0; slot < RewindBuffer.MaxClientSlots; slot++)
            Assert.True(sim.Rewind.TryRecordClientViewTic(slot, slot));
        if (disable) sim.RewindEnabled = false; else sim.Rewind.Reset();
        for (var slot = 0; slot < RewindBuffer.MaxClientSlots; slot++)
            Assert.Equal(-1, sim.Rewind.GetClientViewTic(slot));
    }

    [Fact]
    public void RecordingDoesNotRequireEnabledCaptureOrChangeSavedWorldState()
    {
        var sim = Room(); var before = Save(sim);
        Assert.False(sim.RewindEnabled);
        Assert.True(sim.Rewind.TryRecordClientViewTic(0, 12, 2));
        Assert.Equal(24, sim.Rewind.GetClientViewTic(0));
        Assert.Equal(before, Save(sim)); Assert.Equal(0, sim.Rewind.Count);
    }

    [Fact]
    public void ConvertedClientViewTickSelectsPriorFrameAndSurvivesWorldRestore()
    {
        var sim = Room(); sim.Rewind.Capture(sim);
        for (var i = 0; i < 4; i++) sim.Tick();
        sim.Rewind.Capture(sim); var expected = Save(sim);
        for (var i = 0; i < 4; i++) sim.Tick();
        sim.Rewind.Capture(sim);
        Assert.True(sim.Rewind.TryRecordClientViewTic(0, 3, 2));

        Assert.True(sim.Rewind.RestoreAtOrBefore(sim, sim.Rewind.GetClientViewTic(0)));

        Assert.Equal(4, sim.Thinkers.Clock.Tic); Assert.Equal(expected, Save(sim));
        Assert.Equal(6, sim.Rewind.GetClientViewTic(0));
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
