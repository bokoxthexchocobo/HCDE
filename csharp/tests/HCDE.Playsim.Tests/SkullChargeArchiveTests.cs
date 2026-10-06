using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullChargeArchiveTests
{
    [Fact]
    public void FreshSimulationLoadContinuesChargeWithSavedVelocityAndTarget()
    {
        var sim = Room(); var soul = Soul(sim);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(101, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(); SimSavegame.Apply(loaded, bytes);
        var restored = Soul(loaded);
        Assert.True(restored.Brain!.Charging);
        Assert.Equal(soul.Brain.TargetId, restored.Brain.TargetId);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        sim.Tick(); loaded.Tick();
        Assert.Equal(soul.X, restored.X);
        Assert.Equal(soul.Y, restored.Y);
        Assert.Equal(soul.Z, restored.Z);
        Assert.Equal(soul.VelocityX, restored.VelocityX);
        Assert.True(restored.Brain.Charging);
    }

    [Fact]
    public void SaveWithoutChargeClearsLaterRuntimeChargeWithoutResettingVelocity()
    {
        var sim = Room(); var soul = Soul(sim);
        soul.VelocityX = Fixed.FromInt(3);
        var bytes = SimSavegame.Write(sim);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        SimSavegame.Apply(sim, bytes);
        Assert.False(soul.Brain.Charging);
        Assert.Equal(Fixed.FromInt(3), soul.VelocityX);
    }

    [Fact]
    public void InvalidChargeFlagIsRejected()
    {
        var sim = Room(); Soul(sim).Brain!.StartCharge(Soul(sim), sim.Players.Single());
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-skull-charge-value", error);
    }

    [Fact]
    public void ChargeWithoutBrainRejectsRestoreBeforeChangingHealth()
    {
        var sim = Room(); var soul = Soul(sim);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        var state = sim.CaptureState();
        soul.Brain = null; soul.Health = 17;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(17, soul.Health);
    }

    private static Actor Soul(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 800 }, new LevelThing { Type = 3006, Z = 100 }],
    });
}
