using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ZeroForcedDamageTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(0, true)]
    public void ForcedZeroDamageStillRunsForcePainWithoutHealthOrArmorLoss(int damage, bool noPain)
    {
        var target = new Actor { Health = 100, Armor = 50, ArmorSavePercent = 50,
            Invulnerable = true, Dormant = true, Spectral = true, ReactionTime = 12,
            PainChance = 0, PainThreshold = 1000 };
        var flags = DamageFlags.Forced | (noPain ? DamageFlags.NoPain : DamageFlags.None);
        var result = ActorDamage.Apply(target, damage, flags: flags, inflictor: new Actor { ForcePain = true });
        Assert.Equal(default, result); Assert.Equal(100, target.Health); Assert.Equal(50, target.Armor);
        Assert.Equal(noPain ? target.SpawnState : target.PainState, target.States.Current);
        Assert.Equal(0, target.ReactionTime);
    }

    [Fact]
    public void ZeroForcedHitStopsChargingMomentum()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }] });
        var soul = sim.Actors.Single(actor => actor.DoomEdNum == 3006);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        Assert.NotEqual(default, soul.VelocityX);
        var health = soul.Health;
        ActorDamage.Apply(soul, 0, flags: DamageFlags.Forced);
        Assert.Equal(default, soul.VelocityX); Assert.Equal(default, soul.VelocityY);
        Assert.Equal(default, soul.VelocityZ); Assert.Equal(health, soul.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroForcedHitHonorsFrozenCorpseIceShatterPermission(bool iceShatter)
    {
        var corpse = new Actor { Health = 0, IceCorpse = true };
        corpse.VelocityX = Fixed.FromInt(7);
        ActorDamage.Apply(corpse, 0, flags: DamageFlags.Forced, damageType: "Ice",
            inflictor: new Actor { IceShatter = iceShatter });
        Assert.Equal(iceShatter, corpse.Shattering);
        Assert.Equal(iceShatter ? default : Fixed.FromInt(7), corpse.VelocityX);
    }
}
