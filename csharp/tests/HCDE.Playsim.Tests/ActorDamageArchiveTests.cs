using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageArchiveTests
{
    [Fact]
    public void MonsterConstantDamageUsesItsOwnSpawnBaseline()
    {
        var sim = Room(); var monster = sim.AddBot(100, 0, 3006); monster.Brain = null;
        var original = monster.Damage; var before = SimSavegame.Write(sim);
        monster.SetDamage(47); var bytes = SimSavegame.Write(sim);
        var loaded = Room(); var restored = loaded.AddBot(100, 0, 3006); restored.Brain = null;
        SimSavegame.Apply(loaded, bytes); Assert.Equal(47, restored.Damage);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        SimSavegame.Apply(loaded, before); Assert.Equal(original, restored.Damage);
        Assert.Equal(before, SimSavegame.Write(loaded));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void SignedDamageRoundTripsAndOmittedActorResets(int damage)
    {
        var sim = Room(); var missile = Missile(sim); var bytesBefore = SimSavegame.Write(sim);
        missile.SetDamage(damage);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(104, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(); var restored = Missile(loaded);
        loaded.Players.Single().Damage = 99;
        SimSavegame.Apply(loaded, bytes);
        Assert.Equal(damage, restored.Damage); Assert.Equal(0, loaded.Players.Single().Damage);
        Assert.Equal(bytes, SimSavegame.Write(loaded));
        SimSavegame.Apply(loaded, bytesBefore);
        Assert.Equal(5, restored.Damage);
        Assert.Equal(bytesBefore, SimSavegame.Write(loaded));
    }

    [Fact]
    public void RestoredConstantControlsMissileDamageAndRandomness()
    {
        var sim = Room(); Missile(sim).SetDamage(19); var bytes = SimSavegame.Write(sim);
        var loaded = Room(); var restored = Missile(loaded); SimSavegame.Apply(loaded, bytes);
        var prediction = Room(); Missile(prediction);
        var expected = ((int)prediction.NextCombatRandom() & 7) + 1;
        Assert.Equal(expected * 19, restored.GetMissileDamage(7, 1));
        Assert.Equal(prediction.CombatRandomState, loaded.CombatRandomState);
    }

    [Fact]
    public void OmittedConstantResetsToPatchedSpawnDamage()
    {
        var sim = Room("Thing 35\nMissile damage = 23\n");
        var missile = Missile(sim); Assert.Equal(23, missile.Damage);
        var bytes = SimSavegame.Write(sim); missile.SetDamage(99);
        SimSavegame.Apply(sim, bytes); Assert.Equal(23, missile.Damage);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0, "save-actor-damage-size")]
    [InlineData(1, "save-actor-damage-header")]
    [InlineData(2, "save-actor-damage-value")]
    public void MalformedTrailerFailsBeforeMutation(int corruption, string expected)
    {
        var sim = Room(); var missile = Missile(sim); missile.SetDamage(19);
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (corruption == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        if (corruption == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 104);
        if (corruption == 2) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
        Assert.Equal(expected, Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes)).Message);
        Assert.Equal(19, missile.Damage);
    }

    private static ProjectileActor Missile(AuthoritySimulation sim) => sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
    private static AuthoritySimulation Room(string? patch = null) => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: 42,
        dehacked: patch is null ? null : DehackedPatch.Apply(patch));
}
