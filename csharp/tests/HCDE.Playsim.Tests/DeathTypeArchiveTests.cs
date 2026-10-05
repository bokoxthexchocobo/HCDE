using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathTypeArchiveTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Fact]
    public void SpecialFireFlagRoundTripsAndStillRestrictsPlayerDeath()
    {
        var original = Room();
        original.Players.Single().SpecialFireDamage = true;
        var bytes = SimSavegame.Write(original);
        Assert.Equal(48, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room();
        restored.RestoreState(state);
        Assert.True(restored.Players.Single().SpecialFireDamage);
        var victim = new PlayerPawn { Health = 10 };
        victim.SetTypedDeath("Fire", 3);
        ActorDamage.Apply(victim, 10, damageType: "Fire", inflictor: restored.Players.Single());
        Assert.Equal(2, victim.States.Current);
    }

    [Fact]
    public void OldArchiveClearsSpecialFireFlag()
    {
        var sim = Room();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        sim.Players.Single().SpecialFireDamage = true;
        sim.RestoreState(state);
        Assert.False(sim.Players.Single().SpecialFireDamage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidSpecialFireBitsAreRejected(int flag)
    {
        var sim = Room();
        sim.Players.Single().SpecialFireDamage = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), flag);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-special-fire-flags", error);
    }

    [Fact]
    public void PersistentDamageTypeSurvivesBinaryRestore()
    {
        var original = Room();
        original.Players.Single().DamageType = "Ice";
        original.Players.Single().DeathType = "Fire";
        var bytes = SimSavegame.Write(original);
        Assert.Equal(47, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room();
        restored.RestoreState(state);
        Assert.Equal("Ice", restored.Players.Single().DamageType);
        Assert.Equal("Fire", restored.Players.Single().DeathType);
    }

    [Fact]
    public void ArchiveRestoresOverrideIntoAnotherSimulationAndControlsDeath()
    {
        var original = Room();
        original.Players.Single().DeathType = "Fire";
        var bytes = SimSavegame.Write(original);
        Assert.Equal(46, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var restored = Room();
        restored.RestoreState(state);
        Assert.Equal("Fire", restored.Players.Single().DeathType);
        var target = new Actor { Health = 20, DeathState = -1 };
        target.SetTypedDeath("Fire", 3);
        Assert.True(ActorDamage.Apply(target, 20, damageType: "Normal", inflictor: restored.Players.Single()).Killed);
    }

    [Fact]
    public void OldArchiveClearsExistingOverrideAndPreservesDefaultBytes()
    {
        var original = Room();
        var plain = SimSavegame.Write(original);
        original.Players.Single().DeathType = "none";
        Assert.Equal(plain, SimSavegame.Write(original));
        Assert.True(SimSavegame.TryRead(plain, out var state, out var error), error);
        original.Players.Single().DeathType = "Fire";
        original.RestoreState(state);
        Assert.Null(original.Players.Single().DeathType);
    }

    [Fact]
    public void PoseRestoreRestoresOverride()
    {
        var sim = Room();
        sim.Players.Single().DeathType = "Ice";
        var state = sim.CaptureState();
        sim.Players.Single().DeathType = "Fire";
        sim.RestoreState(state);
        Assert.Equal("Ice", sim.Players.Single().DeathType);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedNameLengthOrEncodingIsRejected(bool invalidEncoding)
    {
        var sim = Room();
        sim.Players.Single().DeathType = "Fire";
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (invalidEncoding) bytes[start + 12] = 255;
        else BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), int.MaxValue);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(invalidEncoding ? "save-death-type-encoding" : "save-death-type-name", error);
    }
}
