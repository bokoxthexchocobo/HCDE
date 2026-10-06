using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorKillTargetTests
{
    [Theory]
    [InlineData(false, false, 0, null, 0)]
    [InlineData(true, false, 0, null, 50)]
    [InlineData(true, false, 1, null, 0)]
    [InlineData(false, true, 0, null, 1)]
    [InlineData(false, true, 8, null, 0)]
    [InlineData(false, false, 4, null, 50)]
    [InlineData(false, false, 0, "ZombieMan", 50)]
    [InlineData(false, false, 16, "ZombieMan", 0)]
    public void KillTargetHonorsProtectionAndFilterRules(bool invulnerable, bool buddha, int flags, string? filter, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var caller = sim.AddBot(0, 0, 3004); var target = sim.AddBot(100, 0, 3001, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7); target.Health = 50;
        target.Invulnerable = invulnerable; target.Buddha = buddha;
        target.DamageFactor = Fixed.FromDouble(0.5);
        ActorHealthActions.KillTarget(caller, flags: flags, classFilter: filter);
        Assert.Equal(expected, target.Health); Assert.Equal(20, caller.Health);
    }
}
