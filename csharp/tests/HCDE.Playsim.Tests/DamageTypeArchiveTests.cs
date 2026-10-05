using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamageTypeArchiveTests
{
    [Theory]
    [InlineData(false, false, 5)]
    [InlineData(true, false, 10)]
    [InlineData(true, true, 20)]
    public void LoadedCatalogComposesWithActorFactorsAndArmor(bool replace, bool noArmor, int expected)
    {
        var sim = Room(); sim.DamageTypes.Define("Fire", 0.5, replace, noArmor);
        sim.Actors[1].SetDamageFactor("None", 0.5); sim.Actors[1].SplashGroup = 9;
        sim.Actors[1].StrifeDamage = true; sim.Players.Single().GivePowerBuddha();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(57, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        var target = restored.Actors[1]; target.Armor = 100; target.ArmorSavePercent = 50;
        Assert.Equal(expected, ActorDamage.Apply(target, 40, damageType: "fIrE").HealthLost);
        Assert.Equal(9, target.SplashGroup); Assert.True(target.StrifeDamage);
        Assert.Equal(2100, restored.Players.Single().PowerBuddhaTics);
    }

    [Fact]
    public void PoseReplacesCatalogAndDoesNotShareLaterChanges()
    {
        var sim = Room(); sim.DamageTypes.Define("Fire", 0.5); var pose = sim.CaptureState();
        sim.DamageTypes.Define("Fire", 0); sim.DamageTypes.Define("Ice", 0);
        sim.RestoreState(pose);
        Assert.Equal(10, ActorDamage.Apply(sim.Actors[1], 20, damageType: "Fire").HealthLost);
        Assert.Equal(20, ActorDamage.Apply(sim.Actors[1], 20, damageType: "Ice").HealthLost);
        sim.DamageTypes.Define("Fire", 2); sim.RestoreState(pose);
        Assert.Equal(10, ActorDamage.Apply(sim.Actors[1], 20, damageType: "Fire").HealthLost);
    }

    [Fact]
    public void BuiltInDrowningOverrideSurvivesSave()
    {
        var sim = Room(); sim.DamageTypes.Define("Drowning", 0.5, noArmor: false);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        var target = restored.Actors[1]; target.Armor = 100; target.ArmorSavePercent = 50;
        Assert.Equal(5, ActorDamage.Apply(target, 20, damageType: "Drowning").HealthLost);
    }

    [Fact]
    public void ExplicitResetToBuiltInOverridesLevelDefinitionAfterLoad()
    {
        var level = Level(); level.DamageTypes = [new MapInfoDamageType("Drowning", 0)];
        var sim = AuthoritySimulation.Start(level);
        sim.DamageTypes.Define("Drowning", 1, noArmor: true);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(57, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = AuthoritySimulation.Start(level); restored.RestoreState(state);
        Assert.Equal(20, ActorDamage.Apply(restored.Actors[1], 20, damageType: "Drowning").HealthLost);
    }

    [Fact]
    public void LegacyRestoresLevelDefinitionsAndClearsRuntimeOverrides()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(legacy, out var state, out var error), error);
        var level = Level(); level.DamageTypes = [new MapInfoDamageType("Fire", 0.5)];
        var restored = AuthoritySimulation.Start(level);
        restored.DamageTypes.Define("Fire", 0); restored.DamageTypes.Define("Ice", 0);
        restored.RestoreState(state);
        Assert.Equal(10, ActorDamage.Apply(restored.Actors[1], 20, damageType: "Fire").HealthLost);
        Assert.Equal(20, ActorDamage.Apply(restored.Actors[1], 20, damageType: "Ice").HealthLost);
        sim.DamageTypes.Define("Ice", 0); sim.RestoreState(state); Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, "save-damage-type-name")]
    [InlineData(1, "save-damage-type-encoding")]
    [InlineData(2, "save-damage-type-value")]
    [InlineData(3, "save-damage-type-value")]
    [InlineData(4, "save-damage-type-header")]
    public void MalformedDefinitionsAreRejected(int corruption, string expected)
    {
        var sim = Room(); sim.DamageTypes.Define("Fire", 0.5);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        var length = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(start + 8));
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), int.MaxValue);
        if (corruption == 1) bytes[start + 12] = 255;
        if (corruption == 2) BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(start + 12 + length), double.PositiveInfinity);
        if (corruption == 3) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 20 + length), 4);
        if (corruption == 4) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 57);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(Level());
    private static PlayLevel Level() => new()
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 }],
    };
}
