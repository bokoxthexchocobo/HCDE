using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoTeleportOverrideSaveTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void RuntimeOverrideSurvivesFreshRestoreAndControlsTeleport(bool player, bool classDefault, bool current)
    {
        var sim = Room(player, classDefault); var actor = sim.Actors.Single(a => a is PlayerPawn || a.ClassDoomEdNum == 3004);
        Assert.True(AcsActorFlags.TrySet(actor, "NOTELEPORT", current));
        sim.RestoreState(sim.CaptureState()); var hash = sim.Checksum;
        var saved = SimSavegame.Write(sim, true);
        var fresh = Room(player, classDefault); SimSavegame.Apply(fresh, saved);
        var restored = fresh.Actors.Single(a => a.Id == actor.Id);
        Assert.Equal(current, restored.NoTeleport); Assert.Equal(hash, fresh.Checksum);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        actor.NoTeleport = !current; SimSavegame.Apply(sim, saved); Assert.Equal(current, actor.NoTeleport);
        Assert.Equal(!current, LineSpecials.Execute(fresh, restored, LineSpecials.Teleport, 0));
        Assert.Equal(current ? -64 : 200, restored.X.ToDouble());
        ActorRaise.ReviveSupported(actor); Assert.Equal(classDefault, actor.NoTeleport);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SaveWithoutOverrideRestoresClassDefaultInsteadOfCurrentValue(bool classDefault)
    {
        var sim = Room(false, classDefault); var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
        var saved = SimSavegame.Write(sim, true);
        Assert.Null(sim.CaptureState().Actors.Single(a => a.Id == actor.Id).NoTeleportOverrideFlag);
        Assert.True(BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)) < 345);
        actor.NoTeleport = !classDefault; SimSavegame.Apply(sim, saved); Assert.Equal(classDefault, actor.NoTeleport);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidOverrideIsRejectedBeforeRestore(int value)
    {
        var sim = Room(false, false); var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
        actor.NoTeleport = true; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-noteleportoverride-value", error);
        var state = sim.CaptureState(); state.Actors[0].NoTeleportOverrideFlag = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state)); Assert.True(actor.NoTeleport);
    }

    [Fact]
    public void ProjectileKeepsExistingSecondaryArchive()
    {
        var sim = Room(false, false); var owner = sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.NoTeleport = false;
        var saved = SimSavegame.Write(sim, true);
        Assert.Null(sim.CaptureState().Actors.Single(a => a.Id == missile.Id).NoTeleportOverrideFlag);
        missile.NoTeleport = true; SimSavegame.Apply(sim, saved); Assert.False(missile.NoTeleport);
    }

    [Fact]
    public void IceChunkStockDefaultAndExplicitOverrideSurviveReconstruction()
    {
        var sim = Room(true, false);
        sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level, Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray(); Assert.NotEmpty(chunks);
        chunks[0].NoTeleport = false;
        var saved = SimSavegame.Write(sim, true); var fresh = Room(true, false); SimSavegame.Apply(fresh, saved);
        foreach (var chunk in chunks)
        {
            var restored = fresh.Actors.OfType<IceChunkActor>().Single(a => a.Id == chunk.Id);
            Assert.Equal(chunk.NoTeleport, restored.NoTeleport);
        }
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    private static AuthoritySimulation Room(bool player, bool classDefault)
    {
        var patch = DehackedPatch.Apply($"Thing {(player ? 1 : 2)}\nBits = {(classDefault ? "NOTELEPORT" : "PASSMOBJ")}\n");
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = (ushort)(player ? 1 : 3004), X = -64 },
                new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
        }, dehacked: patch);
    }
}
