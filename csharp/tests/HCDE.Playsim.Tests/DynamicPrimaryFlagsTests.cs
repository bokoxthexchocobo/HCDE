using HCDE.Gamedata;
using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class DynamicPrimaryFlagsTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("SOLID+SHOOTABLE+COUNTKILL")]
    [InlineData("NOBLOCKMAP+NOGRAVITY+FLOAT+COUNTKILL")]
    [InlineData("SPECIAL+PICKUP+DROPPED+DROPOFF+AMBUSH")]
    [InlineData("FRIEND+SOLID+SHOOTABLE+COUNTKILL")]
    [InlineData("STEALTH+SOLID+SHOOTABLE+COUNTKILL")]
    public void DynamicPrimaryFlagsMatchMapSpawn(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }] }, dehacked: patch);
        var map = Assert.Single(sim.Actors); var bot = sim.AddBot(100, 0);
        Assert.Equal(map.Solid, bot.Solid); Assert.Equal(map.Shootable, bot.Shootable);
        Assert.Equal(map.NoBlockmap, bot.NoBlockmap); Assert.Equal(map.NoGravity, bot.NoGravity);
        Assert.Equal(map.Floating, bot.Floating); Assert.Equal(map.AllowDropOff, bot.AllowDropOff);
        Assert.Equal(map.SpecialPickup, bot.SpecialPickup); Assert.Equal(map.CanPickupItems, bot.CanPickupItems);
        Assert.Equal(map.Dropped, bot.Dropped); Assert.Equal(map.Ambush, bot.Ambush);
        Assert.Equal(map.Friendly, bot.Friendly); Assert.Equal(map.NoBlockMonsters, bot.NoBlockMonsters);
        Assert.Equal(map.IsMonster, bot.IsMonster);
    }
    [Fact]
    public void PrimaryFlagsClearMonsterClassificationBeforeDormantInitialization()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = SOLID+SHOOTABLE+DORMANT\n");
        var sim = AuthoritySimulation.Start(new PlayLevel(), dehacked: patch); var bot = sim.AddBot(0, 0);
        Assert.False(bot.IsMonster); Assert.False(bot.Dormant);
        var health = bot.Health; ActorDamage.Apply(bot, 1); Assert.Equal(health - 1, bot.Health);
    }
    [Fact]
    public void CeilingSpawnUsesPatchedFlagAndActorHeight()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var patch = DehackedPatch.Apply("Thing 2\nBits = SPAWNCEILING+NOGRAVITY+COUNTKILL+6\n");
        var sim = AuthoritySimulation.Start(geometry, dehacked: patch); var bot = sim.AddBot(-40, 0);
        Assert.True(bot.SpawnCeiling); Assert.True(bot.NoGravity);
        Assert.Equal(sim.CeilingOf(bot.SectorIndex) - bot.Height.ToDouble(), bot.Z.ToDouble());
        Assert.False(bot.OnGround);
    }
}