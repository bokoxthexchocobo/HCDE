using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedCanPassTests
{
    [Theory]
    [InlineData("PASSMOBJ", true)]
    [InlineData("passmobj+NOTELEPORT", true)]
    [InlineData("NOTELEPORT", false)]
    public void SecondaryBitsSetClassDefault(string bits, bool expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Equal(expected ? 0x1000u : 0u, patch.Actors.Single(actor => actor.Index == 2).Bits2 & 0x1000u);
        var sim = Room(patch); var actor = sim.Actors.Single(); Assert.Equal(expected, actor.CanPass);
        Assert.Null(sim.CaptureState().Actors.Single().CanPassFlag);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CeilingRecoveryHonorsPatchedDefault(bool enabled)
    {
        var sim = Room(Patch(enabled)); var actor = sim.Actors.Single(); actor.Z = Fixed.FromInt(8);
        var support = sim.AddBot(0, 0); support.Brain = null; support.Height = Fixed.FromInt(16);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            CeilingDirection = 1, TargetCeiling = 256, Speed = 16 });
        LineSpecials.TickMotions(sim); Assert.Equal(enabled ? 16 : 8, actor.Z.ToDouble());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RaiseRestoresPatchedDefault(bool enabled)
    {
        var sim = Room(Patch(enabled)); var actor = sim.Actors.Single();
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse); actor.CanPass = !enabled;
        Assert.True(ThingRaise.Execute(sim, 17, actor, 0, 2)); Assert.Equal(enabled, actor.CanPass);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SaveOverrideAndBaselineRestoreUsePatchedDefault(bool enabled)
    {
        var sim = Room(Patch(enabled)); var actor = sim.Actors.Single();
        var baseline = SimSavegame.Write(sim); SimSavegame.Apply(sim, baseline); var hash = sim.Checksum;
        ActorPropertyActions.ChangeFlag(actor, "CANPASS", !enabled);
        var bytes = SimSavegame.Write(sim); SimSavegame.Apply(sim, bytes);
        Assert.Equal(!enabled, actor.CanPass); Assert.NotEqual(hash, sim.Checksum);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, baseline); Assert.Equal(enabled, actor.CanPass); Assert.Equal(hash, sim.Checksum);
    }

    [Fact]
    public void ReplacingSecondaryBitsClearsPassFlagWithoutChangingPriorPatch()
    {
        var first = Patch(true); var second = DehackedPatch.Apply("Thing 2\nBits = NOTELEPORT\n", first);
        Assert.True(Room(first).Actors.Single().CanPass); Assert.False(Room(second).Actors.Single().CanPass);
    }

    private static DehackedPatchResult Patch(bool enabled) =>
        DehackedPatch.Apply("Thing 2\nBits = " + (enabled ? "PASSMOBJ" : "NOTELEPORT") + "\n");

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
}
