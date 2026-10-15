using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsDamageReturnTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BuddhaReturnReportsDamageRatherThanSurvivalHealthDelta(bool monster)
    {
        var sim = Room(); Actor target = monster ? sim.AddBot(80, 0) : sim.Players.Single();
        target.Health = 10; target.Buddha = true;
        Assert.Equal(50, Damage(sim, target, 50)); Assert.Equal(1, target.Health);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(50, 10)]
    [InlineData(100, 0)]
    public void ReturnUsesPostArmorDamage(int percent, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = percent;
        Assert.Equal(expected, Damage(sim, player, 20)); Assert.Equal(100 - expected, player.Health);
    }

    [Fact]
    public void StrifeAutoHealthDoesNotReduceReturnedDamage()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        player.Inventory.AddHealthPickup(25, 1, 3);
        Assert.Equal(10, Damage(sim, player, 10)); Assert.Equal(65, player.Health);
    }

    [Fact]
    public void SharedHealthLostContractRemainsIndependent()
    {
        var player = Room().Players.Single(); player.Health = 10; player.Buddha = true;
        var result = ActorDamage.ApplyWithNativeResult(player, 50, out var native);
        Assert.Equal(new DamageResult(9, 0, false), result); Assert.Equal(50, native);
    }

    [Fact]
    public void ProtectedHitReturnsZero()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GodMode = true;
        Assert.Equal(0, Damage(sim, player, 10)); Assert.Equal(100, player.Health);
    }

    private static int Damage(AuthoritySimulation sim, Actor target, int amount)
    {
        var stack = new List<int> { 0, 0, 999, 0, amount, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = target }, ["None"], new(),
            AcsCallFunctions.DamageActor, 6, out var result)); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
