namespace HCDE.Playsim.Tests;

public class PowerOwnerDeathOrderingTests
{
    [Fact]
    public void InitialGibQuerySeesPowersButHeightAndSecondQuerySeeCleanup()
    {
        var player = new ObservingPlayer(); player.GivePowerDamage(); player.GivePowerProtection();
        ActorDamage.Apply(player, 1000, flags: DamageFlags.Forced);
        Assert.Equal(new[] { "gib:True", "height:False", "gib:False" }, player.Observations);
    }

    [Fact]
    public void HeightHookCanGrantNewPowerWithoutLaterCleanupErasingIt()
    {
        var player = new GrantingPlayer();
        ActorDamage.Apply(player, 1000, flags: DamageFlags.Forced);
        Assert.True(player.IsDead); Assert.Equal(PlayerPawn.DamagePowerDuration, player.PowerDamageTics);
    }

    [Fact]
    public void AllRepresentedPowerTimersAreClearedBeforeHeightHook()
    {
        var player = new AllPowersPlayer
        {
            PowerBuddhaTics = 100, PowerDamageTics = 100, PowerProtectionTics = 100,
            PowerInfiniteAmmoTics = 100, PowerDrainTics = 100, PowerProsperityTics = 100,
            PowerSpreadTics = 100, PowerRegenerationTics = 100, PowerDoubleFiringSpeedTics = 100,
            PowerHighJumpTics = 100, PowerHighJumpPendingRemoval = true
        };
        ActorDamage.Apply(player, 1000, flags: DamageFlags.Forced);
        Assert.True(player.Observed);
    }

    private sealed class ObservingPlayer : PlayerPawn
    {
        public List<string> Observations { get; } = [];
        public override int GetGibHealth()
        {
            Observations.Add($"gib:{PowerDamageTics > 0 && PowerProtectionTics > 0}"); return base.GetGibHealth();
        }
        public override Fixed GetDeathHeight()
        {
            Observations.Add($"height:{PowerDamageTics > 0 || PowerProtectionTics > 0}"); return base.GetDeathHeight();
        }
    }

    private sealed class GrantingPlayer : PlayerPawn
    {
        public override Fixed GetDeathHeight() { GivePowerDamage(); return base.GetDeathHeight(); }
    }

    private sealed class AllPowersPlayer : PlayerPawn
    {
        public bool Observed { get; private set; }
        public override Fixed GetDeathHeight()
        {
            Assert.All(new[] { PowerBuddhaTics, PowerDamageTics, PowerProtectionTics, PowerInfiniteAmmoTics,
                PowerDrainTics, PowerProsperityTics, PowerSpreadTics, PowerRegenerationTics,
                PowerDoubleFiringSpeedTics, PowerHighJumpTics }, value => Assert.Equal(0, value));
            Assert.False(PowerHighJumpPendingRemoval); Observed = true; return base.GetDeathHeight();
        }
    }
}
