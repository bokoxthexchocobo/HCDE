using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnCeilingDefaultsSaveTests
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
    public void DefaultPlacementAndRuntimeFlagSurviveFreshRestore(bool projectile, bool classDefault, bool current)
    {
        var (sim, actor) = Room(projectile, classDefault);
        Assert.Equal(classDefault, actor.SpawnCeiling);
        Assert.Equal(projectile ? 32 : classDefault ? 256 - actor.Height.ToDouble() : 0, actor.Z.ToDouble());
        actor.SpawnCeiling = current; sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(projectile, classDefault);
        SimSavegame.Apply(fresh, saved); Assert.Equal(current, loaded.SpawnCeiling); Assert.Equal(checksum, fresh.Checksum);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        actor.SpawnCeiling = !current; SimSavegame.Apply(sim, saved); Assert.Equal(current, actor.SpawnCeiling);
        ActorRaise.ReviveSupported(actor); Assert.Equal(classDefault, actor.SpawnCeiling);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyWithoutOverrideResetsCurrentFlagToClassDefault(bool classDefault)
    {
        var (sim, actor) = Room(true, classDefault); var saved = SimSavegame.Write(sim, true);
        Assert.Null(sim.CaptureState().Actors.Single(a => a.Id == actor.Id).SpawnCeilingOverrideFlag);
        actor.SpawnCeiling = !classDefault; SimSavegame.Apply(sim, saved); Assert.Equal(classDefault, actor.SpawnCeiling);
    }

    [Fact]
    public void NumericProjectileBitAndPrimaryReplacementAreApplied()
    {
        var baseline = DehackedPatch.Apply("Thing 35\nBits = 768\n");
        var inherited = DehackedPatch.Apply("Thing 35\nBits = PASSMOBJ\n", baseline);
        var cleared = DehackedPatch.Apply("Thing 35\nBits = NOGRAVITY\n", inherited);
        foreach (var patch in new[] { baseline, inherited, cleared })
        {
            Assert.Empty(patch.Errors);
            var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3001 }] }, dehacked: patch);
            Assert.Equal(patch != cleared, sim.SpawnProjectile(sim.Actors.Single(), ProjectileKind.Plasma).SpawnCeiling);
        }
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void InvalidOverrideIsRejected(int value)
    {
        var (sim, actor) = Room(false, false); actor.SpawnCeiling = true;
        var saved = SimSavegame.Write(sim, true); Assert.Equal(351, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-spawnceilingoverride-value", error);
        var state = sim.CaptureState(); state.Actors[0].SpawnCeilingOverrideFlag = value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room(bool projectile, bool classDefault)
    {
        var patch = DehackedPatch.Apply($"Thing {(projectile ? 35 : 2)}\nBits = {(classDefault ? "SPAWNCEILING+NOGRAVITY+6" : "NOGRAVITY+6")}\n");
        Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = (ushort)(projectile ? 3001 : 3004) }] }, dehacked: patch);
        var actor = sim.Actors.Single(); return (sim, projectile ? sim.SpawnProjectile(actor, ProjectileKind.Plasma) : actor);
    }
}
