using HCDE.MapLoader;
using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

public class SectorSoundTargetTests
{
    [Fact]
    public void NoSectorListenerUsesSectorNoiseWithoutBeingDirectlyMarked()
    {
        var (sim, player, listener) = Room(); listener.NoSector = true;
        SoundPropagation.Alert(sim, player); Assert.Null(listener.LastHeardTargetId); Assert.Null(listener.TargetId);
        Assert.Equal(player.Id, sim.SectorSoundTargetId(0)); listener.Brain!.Tick(sim, listener);
        Assert.Equal(player.Id, listener.TargetId);
    }

    [Fact]
    public void ListenerFiltersDoNotPreventSectorMemory()
    {
        var (sim, player, listener) = Room(); listener.NoSplashAlert = true;
        SoundPropagation.Alert(sim, player, player, 1, true);
        Assert.Null(listener.LastHeardTargetId); Assert.Equal(player.Id, sim.SectorSoundTargetId(0));
    }

    [Fact]
    public void NullTargetClearsSectorMemory()
    {
        var (sim, player, _) = Room(); SoundPropagation.Alert(sim, player);
        SoundPropagation.Alert(sim, player, null); Assert.Null(sim.SectorSoundTargetId(0));
    }

    [Fact]
    public void SavedSectorNoiseAndOlderSaveRestoreCorrectly()
    {
        var (sim, player, listener) = Room(); listener.NoSector = true; var older = SimSavegame.Write(sim, true);
        SoundPropagation.Alert(sim, player); var saved = SimSavegame.Write(sim, true);
        SoundPropagation.Alert(sim, player, null); SimSavegame.Apply(sim, saved);
        Assert.Equal(player.Id, sim.SectorSoundTargetId(0)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        listener.Brain!.Tick(sim, listener); Assert.Equal(player.Id, listener.TargetId);
        SimSavegame.Apply(sim, older); Assert.Null(sim.SectorSoundTargetId(0));
    }

    [Fact]
    public void DestroyedSourceDoesNotLeaveLiveSectorPointer()
    {
        var (sim, _, listener) = Room(); SoundPropagation.Alert(sim, listener); listener.Destroy();
        Assert.Null(sim.SectorSoundTargetId(0));
        Assert.Empty(sim.CaptureState(true).SectorSoundTargets!);
    }

    [Fact]
    public void SectorMemoryParticipatesInChecksumWithoutListenerWake()
    {
        var (sim, player, listener) = Room(); listener.NoSector = true;
        sim.RestoreState(sim.CaptureState(true)); var before = sim.Checksum;
        SoundPropagation.Alert(sim, player); sim.RestoreState(sim.CaptureState(true));
        Assert.NotEqual(before, sim.Checksum); Assert.Null(listener.TargetId);
    }

    [Fact]
    public void InvalidSectorIndexIsRejected()
    {
        var (sim, player, _) = Room(); SoundPropagation.Alert(sim, player); var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 99);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Fact]
    public void StructuredInvalidTargetIsRejectedBeforeRestore()
    {
        var (sim, _, _) = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState(true);
        state.SectorSoundTargets = new Dictionary<int, uint> { [0] = uint.MaxValue };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Listener) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }] });
        return (sim, sim.Players.Single(), sim.Actors.Single(a => a.ClassDoomEdNum == 3004));
    }
}
