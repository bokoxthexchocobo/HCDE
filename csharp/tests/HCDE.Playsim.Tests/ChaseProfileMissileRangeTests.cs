using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseProfileMissileRangeTests
{
    [Theory]
    [InlineData(64, 896, true)]
    [InlineData(64, 897, true)]
    [InlineData(64, 1087, true)]
    [InlineData(64, 1088, true)]
    [InlineData(64, 1089, false)]
    [InlineData(66, 100, false)]
    [InlineData(66, 259, false)]
    [InlineData(66, 260, true)]
    [InlineData(66, 261, true)]
    public void ProfileRangePropertiesUseNativeAdjustedDistance(int type, int distance, bool attack)
    {
        var (sim, hunter) = Room(type, distance);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, true);
        Assert.Equal(attack ? MonsterMode.Windup : MonsterMode.Chase, hunter.Brain.Mode);
        Assert.Equal(attack, hunter.Brain.WindupTics > 0);
        Assert.Equal(sim.Players.Single().Id, hunter.Brain.TargetId);
    }

    [Theory]
    [InlineData(64, 1089)]
    [InlineData(66, 259)]
    public void JustHitRetaliationBypassesProfileRangeProperties(int type, int distance)
    {
        var (sim, hunter) = Room(type, distance);
        hunter.JustHit = true;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, true);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        Assert.False(hunter.JustHit);
    }

    [Fact]
    public void CloseRevenantStillSelectsMelee()
    {
        var (sim, hunter) = Room(66, 40);
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        Assert.Contains(sim.PendingSoundRequests, request => request.Sound == "skeleton/swing");
    }

    private static (AuthoritySimulation, Actor) Room(int type, int distance)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = distance }, new LevelThing { Type = type }],
        });
        var hunter = sim.Actors.Single(actor => actor.DoomEdNum == type);
        hunter.ReactionTime = 0;
        return (sim, hunter);
    }
}
