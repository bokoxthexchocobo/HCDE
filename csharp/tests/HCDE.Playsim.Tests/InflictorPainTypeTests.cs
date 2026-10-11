using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InflictorPainTypeTests
{
    [Theory]
    [InlineData("Ice", 2)]
    [InlineData("iCe", 2)]
    [InlineData("None", 1)]
    [InlineData("", 1)]
    [InlineData(null, 1)]
    public void OverrideSelectsTypedPainWithoutChangingDamageType(string? painType, int state)
    {
        var target = Target(); target.Armor = 20; target.ArmorSavePercent = 50;
        target.SetDamageFactor("Fire", 0.5); target.SetDamageFactor("Ice", 0);
        target.SetTypedPain("Fire", 1, 256); target.SetTypedPain("Ice", 2, 256);
        var result = ActorDamage.Apply(target, 20, damageType: "Fire", inflictor: new Actor { PainType = painType });
        Assert.Equal(5, result.HealthLost); Assert.Equal(5, result.ArmorLost);
        Assert.Equal(state, target.States.Current);
    }

    [Fact]
    public void OverrideUsesItsPainChanceButWoundUsesIncomingType()
    {
        var target = Target(); target.SetTypedPain("Fire", 1, 256); target.SetTypedPain("Ice", 2, 0);
        var inflictor = new Actor { PainType = "Ice" };
        ActorDamage.Apply(target, 1, damageType: "Fire", inflictor: inflictor);
        Assert.Equal(0, target.States.Current);
        target.WoundHealth = 100; target.SetTypedWound("Fire", 3);
        ActorDamage.Apply(target, 1, damageType: "Fire", inflictor: inflictor);
        Assert.Equal(3, target.States.Current);
    }

    [Fact]
    public void PainTypeSurvivesSaveAndRejectsInvalidUtf8()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma); missile.PainType = "Ice";
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(148, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        missile.PainType = "Fire"; SimSavegame.Apply(sim, bytes);
        Assert.Equal("Ice", missile.PainType); Assert.Equal(bytes, SimSavegame.Write(sim));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        // First actor is null; the second actor's UTF-8 name follows its length.
        bytes[bytes.Length - size + 16] = 0xff;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-pain-type-invalid", error);
    }

    private static Actor Target()
    {
        var actor = new Actor { Health = 100, PainChance = 256 };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2), new(-1, 3)], 0);
        return actor;
    }
}
