using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LineActionsSaveTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(31)]
    public void MutableActionsRestoreIntoFreshMapWithMatchingChecksum(int flags)
    {
        var sim = Room(); new SimLineAction(-7, int.MinValue, flags).Apply(sim.Level.Lines[0]);
        sim.RestoreState(sim.CaptureState()); var bytes = SimSavegame.Write(sim, true);
        var restored = Room(); SimSavegame.Apply(restored, bytes);
        Assert.Equal(new SimLineAction(-7, int.MinValue, flags), SimLineAction.Capture(restored.Level.Lines[0]));
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Equal(bytes, SimSavegame.Write(restored, true));
    }

    [Fact]
    public void ConsumedOneShotActionCannotReactivateAfterFreshMapRestore()
    {
        var sim = Room(); var actor = sim.Players.Single(); var line = sim.Level.Lines[0];
        Assert.True(LineSpecials.ActivateMapLine(sim, actor, line, use: true)); Assert.Equal(0, line.Special);
        var restored = Room(); SimSavegame.Apply(restored, SimSavegame.Write(sim, true));
        Assert.Equal(0, restored.Level.Lines[0].Special);
        Assert.False(LineSpecials.ActivateMapLine(restored, restored.Players.Single(), restored.Level.Lines[0], use: true));
    }

    [Fact]
    public void SaveWithoutExtensionResetsMapDefaultsAndDoesNotEmitTrailer()
    {
        var sim = Room(); var state = sim.CaptureState(); Assert.Null(state.LineActions);
        var old = SimSavegame.Write(sim, true); var initial = SimLineAction.Capture(sim.Level.Lines[0]);
        new SimLineAction(0, 12, 31).Apply(sim.Level.Lines[0]); SimSavegame.Apply(sim, old);
        Assert.Equal(initial, SimLineAction.Capture(sim.Level.Lines[0])); Assert.Null(sim.CaptureState().LineActions);
        Assert.Equal(old, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void PreviouslyUnhashedActivationFlagsAffectChecksum(int mask)
    {
        var sim = Room(); var original = sim.Checksum;
        new SimLineAction(18, 0, mask).Apply(sim.Level.Lines[0]); sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(original, sim.Checksum);
    }

    [Fact]
    public void InvalidFlagsRejectBeforeMutationInStructuredAndBinaryState()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.LineActions = [new SimLineAction(0, 0, 32)]; state.Sectors[0] = (32, 128);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
        sim.Level.Lines[0].Special = 0; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 16), 32);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-lineactions-state", error);
    }

    [Fact]
    public void LineCountMismatchRejectsBeforeClockOrGeometryMutation()
    {
        var sim = Room(); var state = sim.CaptureState(); state.LineActions = []; state.Sectors[0] = (32, 128);
        var before = SimSavegame.Write(sim, true);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128, Tag = 1 }],
        Lines = [new LevelLine { Special = 18, Tag = 1, X1 = 100, Y1 = -128, X2 = 100, Y2 = 128, SideBack = -1 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
