namespace HCDE.Playsim.Tests;

public class PlayerDamageReactionProtectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UltimateGodModeSuppressesPainAfterDamageIsReducedToZero(bool forcePain)
    {
        var player = new PlayerPawn { Health = 100, GodMode2 = true, DamageFactor = default,
            PainChance = 256, ReactionTime = 12 };
        var state = player.States.Current;
        var inflictor = new Actor { CausePain = true, ForcePain = forcePain };
        Assert.Equal(default, ActorDamage.Apply(player, 10, inflictor: inflictor));
        Assert.Equal(state, player.States.Current); Assert.Equal(12, player.ReactionTime);
        Assert.False(player.JustHit);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerNoPainAndGodModeSuppressForcedPainButPreserveDamage(bool godMode)
    {
        var player = new PlayerPawn { Health = 100, GodMode = godMode, NoPain = !godMode,
            PainChance = 256 };
        var state = player.States.Current;
        var result = ActorDamage.Apply(player, 10, flags: DamageFlags.Forced,
            inflictor: new Actor { ForcePain = true, CausePain = true });
        Assert.Equal(10, result.HealthLost); Assert.Equal(90, player.Health);
        Assert.Equal(state, player.States.Current); Assert.False(player.JustHit);
    }
}
