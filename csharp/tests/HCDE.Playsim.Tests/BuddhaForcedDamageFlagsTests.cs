using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BuddhaForcedDamageFlagsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ForcedDamageSkipsCallbacksBeforeBuddhaProtection(bool buddha2)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single();
        player.Buddha2 = buddha2;
        var inflictor = new FlagRecordingActor();
        var flags = DamageFlags.Forced | DamageFlags.NoPain | DamageFlags.NoEnhance;

        var result = ActorDamage.Apply(player, 7, flags: flags, inflictor: inflictor);

        Assert.Equal(7, result.HealthLost);
        Assert.Equal(0, inflictor.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForcedDamageSkipsSpecialChecksBeforeProtectedPlayerArmor(bool noDamage)
    {
        var player = new PlayerPawn { Health = 100, Buddha2 = !noDamage, NoDamage = noDamage,
            Spectral = true, Dormant = true, DamageFactor = default };
        player.Inventory.Armor = 50;
        player.Inventory.ArmorSavePercent = 50;
        var inflictor = new FlagRecordingActor();

        var result = ActorDamage.Apply(player, 20, flags: DamageFlags.Forced, inflictor: inflictor);

        Assert.Equal(0, inflictor.Calls);
        Assert.Equal(noDamage ? 0 : 10, result.HealthLost);
        Assert.Equal(noDamage ? 0 : 10, result.ArmorLost);
        Assert.Equal(noDamage ? 50 : 40, player.Inventory.Armor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Buddha2ClearsForcedBeforePlayerInvulnerability(bool godMode)
    {
        var player = new PlayerPawn { Health = 100, Buddha2 = true,
            GodMode = godMode, Invulnerable = !godMode };
        var inflictor = new FlagRecordingActor();

        Assert.Equal(default, ActorDamage.Apply(player, 20, flags: DamageFlags.Forced, inflictor: inflictor));
        Assert.Equal(100, player.Health);
        Assert.Equal(0, inflictor.Calls);
    }

    [Fact]
    public void Buddha2StillSurvivesForcedLethalDamage()
    {
        var player = new PlayerPawn { Health = 10, Buddha2 = true };
        var result = ActorDamage.Apply(player, 20, flags: DamageFlags.Forced);
        Assert.Equal(1, player.Health);
        Assert.False(result.Killed);
    }

    private sealed class FlagRecordingActor : Actor
    {
        public int Calls { get; private set; }
        public DamageFlags Flags { get; private set; }

        public override int DoSpecialDamage(Actor target, int damage, string? damageType, DamageFlags flags)
        {
            Calls++;
            Flags = flags;
            return damage;
        }
    }
}
