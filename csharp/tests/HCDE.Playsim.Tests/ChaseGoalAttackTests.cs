using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseGoalAttackTests
{
    [Theory]
    [InlineData(3004, 40)]
    [InlineData(3004, 200)]
    [InlineData(3001, 40)]
    [InlineData(3001, 200)]
    [InlineData(3002, 40)]
    public void NativeProfilesDoNotStartAttacksAgainstGoals(int type, int distance)
    {
        var (sim, hunter, target) = Room(type, distance);
        hunter.GoalId = target.Id;
        var health = target.Health;
        for (var tic = 0; tic < 35; tic++)
        {
            hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
            Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
            Assert.Equal(0, hunter.Brain.WindupTics);
            if (hunter.GoalId is null) { Assert.Null(hunter.TargetId); break; }
        }
        Assert.Equal(health, target.Health);
        Assert.Empty(sim.Actors.OfType<ProjectileActor>());
    }

    [Theory]
    [InlineData(MonsterAttack.Melee)]
    [InlineData(MonsterAttack.Hitscan)]
    [InlineData(MonsterAttack.Fireball)]
    public void GenericAttacksAreSuppressedForGoalTargets(MonsterAttack attack)
    {
        var (sim, hunter, target) = Room(3004, 40);
        hunter.Brain = new MonsterBrain(attack);
        hunter.ReactionTime = 0;
        hunter.Brain.SetTargetThingId(sim, target.ThingId);
        hunter.GoalId = target.Id;
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Null(hunter.GoalId); Assert.Null(hunter.TargetId);
        Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
        Assert.Equal(0, hunter.Brain.WindupTics);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AssigningGoalCancelsWindupAndRemovingGoalAllowsAttack(bool generic)
    {
        var (sim, hunter, target) = Room(3004, 200);
        if (generic)
        {
            hunter.Brain = new MonsterBrain(MonsterAttack.Hitscan);
            hunter.ReactionTime = 0;
            hunter.Brain.SetTargetThingId(sim, target.ThingId);
        }
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        hunter.GoalId = target.Id;
        var health = target.Health;
        for (var tic = 0; tic < 35; tic++) hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(health, target.Health);
        Assert.Equal(MonsterMode.Chase, hunter.Brain.Mode);
        Assert.Equal(0, hunter.Brain.WindupTics);
        hunter.GoalId = null;
        hunter.Brain.Tick(sim, hunter);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
    }

    private static (AuthoritySimulation, Actor, Actor) Room(int type, int distance)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = type }, new LevelThing { Type = 3001, X = distance, Id = 7 }],
        });
        var hunter = sim.Actors[0]; var target = sim.Actors[1];
        hunter.ReactionTime = 0;
        hunter.Brain!.SetTargetThingId(sim, 7);
        return (sim, hunter, target);
    }
}
