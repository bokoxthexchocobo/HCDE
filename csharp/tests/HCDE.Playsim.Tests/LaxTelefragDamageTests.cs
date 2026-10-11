using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LaxTelefragDamageTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void FlagAllowsSelfOrTargetFactorToReduceTelefrag(bool lax, bool self)
    {
        var actor = new Actor { Health = 100, NoPain = true, LaxTelefragDamage = lax };
        if (self) actor.SelfDamageFactor = default; else actor.DamageFactor = default;
        ActorDamage.Apply(actor, ActorDamage.TelefragDamage, self ? actor : null);
        Assert.Equal(lax ? 100 : 100 - ActorDamage.TelefragDamage, actor.Health);
    }

    [Fact]
    public void PlayerCallbackAndArmorReduceLaxTelefrag()
    {
        var actor = new ReplacingPlayer { Health = 100, LaxTelefragDamage = true, NoPain = true };
        actor.Inventory.Armor = 20; actor.Inventory.ArmorSavePercent = 50;
        var result = ActorDamage.Apply(actor, ActorDamage.TelefragDamage);
        Assert.Equal(1, actor.Calls); Assert.Equal(5, result.HealthLost); Assert.Equal(5, result.ArmorLost);
        Assert.Equal(95, actor.Health);
    }

    [Fact]
    public void ForcedDamageStillBypassesTargetModifierAndCallback()
    {
        var actor = new ReplacingPlayer { Health = 100, LaxTelefragDamage = true, DamageFactor = default };
        ActorDamage.Apply(actor, ActorDamage.TelefragDamage, flags: DamageFlags.Forced);
        Assert.Equal(0, actor.Calls); Assert.Equal(100 - ActorDamage.TelefragDamage, actor.Health);
    }

    [Fact]
    public void PlayerBuddhaStillUsesOriginalTelefragAmount()
    {
        var actor = new ReplacingPlayer { Health = 5, LaxTelefragDamage = true, Buddha = true };
        ActorDamage.Apply(actor, ActorDamage.TelefragDamage);
        Assert.Equal(-5, actor.Health); Assert.True(actor.IsDead);
    }

    [Fact]
    public void FlagSurvivesSaveAndRejectsMalformedBoolean()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var actor = sim.Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.LAXTELEFRAGDMG", true));
        Assert.True(AcsActorFlags.TryGet(actor, "laxtelefragdmg", out var value)); Assert.True(value);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(152, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.LaxTelefragDamage = false; SimSavegame.Apply(sim, bytes);
        Assert.True(actor.LaxTelefragDamage); Assert.Equal(bytes, SimSavegame.Write(sim));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        bytes[bytes.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-lax-telefrag-damage-invalid", error);
    }

    private sealed class ReplacingPlayer : PlayerPawn
    {
        public int Calls { get; private set; }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        { Calls++; return 10; }
    }
}
