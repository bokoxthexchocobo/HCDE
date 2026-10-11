namespace HCDE.Playsim.Tests;

public class ZeroForcedWoundTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, -5)]
    [InlineData(true, 0)]
    public void ZeroForcedDamageStillSelectsTypedWound(bool player, int damage)
    {
        Actor target = player ? new PlayerPawn() : new Actor();
        target.Health = 50; target.WoundHealth = 50; target.ReactionTime = 12;
        target.SetTypedWound("Fire", 3);
        var result = ActorDamage.Apply(target, damage, flags: DamageFlags.Forced, damageType: "Fire");
        Assert.Equal(default, result); Assert.Equal(50, target.Health);
        Assert.Equal(3, target.States.Current); Assert.Equal(12, target.ReactionTime);
        Assert.Null(target.LastDamageSourceId);
    }

    [Fact]
    public void PlayerNoPainStillBlocksZeroForcedWound()
    {
        var target = new PlayerPawn { Health = 50, WoundHealth = 50, NoPain = true };
        target.SetTypedWound("Fire", 3); var initial = target.States.Current;
        ActorDamage.Apply(target, 0, flags: DamageFlags.Forced, damageType: "Fire");
        Assert.Equal(initial, target.States.Current);
    }
}
