using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageFactorArchiveTests
{
    [Theory]
    [InlineData("fIrE", 10)]
    [InlineData("Ice", 30)]
    [InlineData("Poison", 0)]
    [InlineData("Cold", 0)]
    public void LoadedRulesPreserveScalarAndTypedCalculation(string type, int expected)
    {
        var sim = Room(); var target = sim.Actors[1]; var source = sim.Actors[2];
        target.DamageFactor = Fixed.FromDouble(0.5); source.DamageMultiplier = Fixed.FromInt(2);
        target.SetDamageFactor("Fire", 0.25); target.SetDamageFactor("None", 0.75);
        target.SetDamageFactor("Poison", -1); target.SetDamageFactor("Cold", 0);
        target.StrifeDamage = true; target.SplashGroup = 9; sim.Players.Single().GivePowerBuddha();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(56, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        Assert.True(restored.Actors[1].StrifeDamage); Assert.Equal(9, restored.Actors[1].SplashGroup);
        Assert.Equal(2100, restored.Players.Single().PowerBuddhaTics);
        Assert.Equal(expected, ActorDamage.Apply(restored.Actors[1], 40, restored.Actors[2], damageType: type).HealthLost);
    }

    [Fact]
    public void ScalarOnlyRulesRoundTrip()
    {
        var sim = Room(); sim.Actors[1].DamageFactor = Fixed.FromDouble(0.5);
        sim.Actors[2].DamageMultiplier = Fixed.FromInt(2);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        Assert.Equal(Fixed.FromDouble(0.5), restored.Actors[1].DamageFactor);
        Assert.Equal(Fixed.FromInt(2), restored.Actors[2].DamageMultiplier);
        Assert.Equal(40, ActorDamage.Apply(restored.Actors[1], 40, restored.Actors[2]).HealthLost);
    }

    [Fact]
    public void CaseInsensitiveDuplicateNamesAreRejected()
    {
        var sim = Room(); sim.Actors[0].SetDamageFactor("Fire", 0.5); sim.Actors[0].SetDamageFactor("IceX", 1);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        "fIrE"u8.CopyTo(bytes.AsSpan(start + 40));
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-actor-factor-value", error);
    }

    [Fact]
    public void SnapshotOwnsIndependentFactorTable()
    {
        var sim = Room(); var actor = sim.Actors[1]; actor.SetDamageFactor("Fire", 0.5);
        actor.DamageFactor = Fixed.FromDouble(0.5); var pose = sim.CaptureState();
        actor.SetDamageFactor("Fire", 2); actor.SetDamageFactor("Ice", 0); actor.DamageFactor = Fixed.FromInt(2);
        sim.RestoreState(pose);
        Assert.Equal(10, ActorDamage.Apply(actor, 40, damageType: "Fire").HealthLost);
        Assert.Equal(20, ActorDamage.Apply(actor, 40, damageType: "Ice").HealthLost);
        actor.SetDamageFactor("Fire", 3); sim.RestoreState(pose);
        Assert.Equal(10, ActorDamage.Apply(actor, 40, damageType: "Fire").HealthLost);
    }

    [Fact]
    public void LegacyClearsTablesAndRestoresIdentityScalars()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim);
        sim.Actors[1].SetDamageFactor("Fire", 0); sim.Actors[1].DamageFactor = default;
        sim.Actors[2].DamageMultiplier = default;
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error);
        sim.RestoreState(state); Assert.Equal(legacy, SimSavegame.Write(sim));
        Assert.Equal(40, ActorDamage.Apply(sim.Actors[1], 40, sim.Actors[2], damageType: "Fire").HealthLost);
    }

    [Theory]
    [InlineData(0, "save-actor-factor-name")]
    [InlineData(1, "save-actor-factor-encoding")]
    [InlineData(2, "save-actor-factor-value")]
    [InlineData(3, "save-actor-factor-entries")]
    [InlineData(4, "save-actor-factor-header")]
    public void MalformedFactorDataIsRejected(int corruption, string expected)
    {
        var sim = Room(); sim.Actors[0].SetDamageFactor("Fire", 0.5);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 20), int.MaxValue);
        if (corruption == 1) bytes[start + 24] = 255;
        if (corruption == 2) BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(start + 28), double.NaN);
        if (corruption == 3) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 16), int.MaxValue);
        if (corruption == 4) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 56);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 100 }],
    });
}
