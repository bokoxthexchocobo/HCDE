using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SelfDamageFactorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.5, 3)]
    [InlineData(2, 14)]
    public void SelfHitScalesAndTruncatesBeforeOrdinaryDamage(double factor, int expected)
    {
        var actor = new Actor { Health = 100, NoPain = true, SelfDamageFactor = Fixed.FromDouble(factor) };
        Assert.Equal(expected, ActorDamage.Apply(actor, 7, actor).HealthLost);
        Assert.Equal(100 - expected, actor.Health);
    }

    [Theory]
    [InlineData(DamageFlags.Forced)]
    [InlineData(DamageFlags.NoFactor)]
    public void SelfFactorStillAppliesWhenOrdinaryFactorsAreBypassed(DamageFlags flags)
    {
        var actor = new Actor { Health = 100, NoPain = true, SelfDamageFactor = Fixed.FromDouble(0.5), DamageFactor = default };
        Assert.Equal(3, ActorDamage.Apply(actor, 7, actor, flags).HealthLost);
    }

    [Fact]
    public void ScalingPrecedesSourceTargetFactorsAndArmor()
    {
        var actor = new Actor { Health = 100, NoPain = true, SelfDamageFactor = Fixed.FromDouble(0.5),
            DamageMultiplier = Fixed.FromInt(2), DamageFactor = Fixed.FromDouble(0.5), Armor = 50, ArmorSavePercent = 50 };
        var result = ActorDamage.Apply(actor, 9, actor);
        Assert.Equal(2, result.HealthLost); Assert.Equal(2, result.ArmorLost);
    }

    [Fact]
    public void InflictorIdentityDoesNotReplaceSourceIdentityAndTelefragKeepsRawDamage()
    {
        var actor = new Actor { Health = 100, NoPain = true, SelfDamageFactor = default };
        Assert.Equal(7, ActorDamage.Apply(actor, 7, new Actor(), inflictor: actor).HealthLost);
        actor.Health = 100;
        ActorDamage.Apply(actor, ActorDamage.TelefragDamage, actor);
        Assert.Equal(100 - ActorDamage.TelefragDamage, actor.Health);
    }

    [Fact]
    public void SavedFactorRestoresAndInvalidPriorVersionIsRejected()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var actor = sim.Players.Single(); actor.SelfDamageFactor = Fixed.FromDouble(0.5); actor.NoPain = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(151, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.SelfDamageFactor = Fixed.FromInt(2); SimSavegame.Apply(sim, bytes);
        Assert.Equal(0.5, actor.SelfDamageFactor.ToDouble()); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Equal(7, ActorDamage.Apply(actor, 15, actor).HealthLost);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size), 151);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-self-damage-factor-invalid", error);
    }
}
