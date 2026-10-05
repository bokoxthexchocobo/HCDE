namespace HCDE.Playsim.Tests;

public class ArmorCancellationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullArmorCancellationStopsWoundPainAndAttackerRecording(bool playerTarget)
    {
        Actor target = playerTarget ? new PlayerPawn() : new Actor();
        target.Health = 100;
        target.WoundHealth = 100;
        target.SetTypedWound("Fire", 3);
        target.LastDamageSourceId = 42;
        if (target is PlayerPawn player)
        {
            player.Inventory.Armor = 20;
            player.Inventory.ArmorSavePercent = 100;
        }
        else
        {
            target.Armor = 20;
            target.ArmorSavePercent = 100;
        }
        var initialState = target.States.Current;
        var result = ActorDamage.Apply(target, 20, new Actor(), damageType: "Fire",
            inflictor: new Actor { ForcePain = true });
        Assert.Equal(new DamageResult(0, 20, false), result);
        Assert.Equal(100, target.Health);
        Assert.Equal(initialState, target.States.Current);
        Assert.Equal((uint?)42, target.LastDamageSourceId);
    }
}
