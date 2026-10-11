using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseLongRangeAcquisitionTests
{
    [Theory]
    [InlineData(2048)]
    [InlineData(2049)]
    [InlineData(4096)]
    [InlineData(16000)]
    public void DefaultChaseFindsVisiblePlayerWithoutArtificialRangeCap(int distance)
    {
        var (sim, hunter) = Room(distance);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.Equal(sim.Players.Single().Id, hunter.Brain.TargetId);
        Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
        Assert.True(hunter.VelocityX.Raw > 0);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(180, false)]
    public void RestrictedLookStillUsesFacingAtLongRange(int facing, bool acquired)
    {
        var (sim, hunter) = Room(4096); hunter.Angle = BamAngle.FromDegrees(facing);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.DontLookAllAround | MonsterChaseFlags.NoPlayActive, false, false);
        Assert.Equal(acquired ? sim.Players.Single().Id : (uint?)null, hunter.Brain.TargetId);
    }

    [Fact]
    public void NoTargetPlayerRemainsExcludedAtLongRange()
    {
        var (sim, hunter) = Room(4096); sim.Players.Single().NoTarget = true;
        hunter.Brain!.Tick(sim, hunter, useMelee: false, useMissile: false);
        Assert.Null(hunter.Brain.TargetId);
    }

    private static (AuthoritySimulation, Actor) Room(int distance)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = distance }] });
        var hunter = new Actor { Simulation = sim, Brain = new MonsterBrain(MonsterAttack.Melee), ReactionTime = 0 };
        hunter.States.Configure(hunter, [new(-1, 0)], 0);
        return (sim, hunter);
    }
}
