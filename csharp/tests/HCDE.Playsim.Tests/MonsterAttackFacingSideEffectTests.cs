using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MonsterAttackFacingSideEffectTests
{
    [Theory]
    [InlineData(3004, 10)]
    [InlineData(9, 10)]
    [InlineData(65, 10)]
    [InlineData(84, 20)]
    [InlineData(7, 20)]
    [InlineData(3001, 16)]
    public void ProfileAttackFacingClearsAmbushAndPreservesDefaultPitch(int type, int shotTic)
    {
        var (sim, hunter) = Room(type);
        hunter.Ambush = true;
        hunter.PitchDegrees = 17;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        for (var tic = 0; tic < shotTic - 1; tic++) hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.False(hunter.Ambush);
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.False(hunter.Ambush);
        Assert.Equal(17, hunter.PitchDegrees);
        Assert.Equal(0, hunter.Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(3004)]
    [InlineData(3001)]
    public void DisabledAttacksDoNotClearAmbush(int type)
    {
        var (sim, hunter) = Room(type);
        hunter.Ambush = true;
        for (var tic = 0; tic < 25; tic++)
            hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive, false, false);
        Assert.True(hunter.Ambush);
    }

    [Fact]
    public void AssignedGoalDoesNotRunAttackFacingSideEffects()
    {
        var (sim, hunter) = Room(3004);
        hunter.Ambush = true;
        hunter.GoalId = sim.Players.Single().Id;
        for (var tic = 0; tic < 15; tic++) hunter.Brain!.Tick(sim, hunter);
        Assert.True(hunter.Ambush);
    }

    private static (AuthoritySimulation, Actor) Room(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type }],
        });
        var hunter = sim.Actors.Single(actor => actor.DoomEdNum == type);
        hunter.ReactionTime = 0;
        return (sim, hunter);
    }
}
