using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeResurrectionDirectionTests
{
    [Theory]
    [InlineData(0, 45, 0)]
    [InlineData(1, 45, 45)]
    [InlineData(2, 0, 45)]
    [InlineData(3, -45, 45)]
    [InlineData(4, -45, 0)]
    [InlineData(5, -45, -45)]
    [InlineData(6, 0, -45)]
    [InlineData(7, 45, -45)]
    public void NativeProbeFollowsAllEightMovementDirections(byte direction, short x, short y)
    {
        var (sim, corpse, raiser) = Room(x, y); raiser.MoveDirection = direction;
        Assert.True(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single(), nativeMovement: true));
        Assert.False(corpse.IsDead);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(255)]
    public void NoDirectionSkipsCorpseAttemptAndVelocityMutation(byte direction)
    {
        var (sim, corpse, raiser) = Room(20, 0); raiser.MoveDirection = direction;
        corpse.VelocityX = Fixed.FromInt(3);
        Assert.False(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single(), nativeMovement: true));
        Assert.True(corpse.IsDead); Assert.Equal(Fixed.FromInt(3), corpse.VelocityX);
    }

    [Fact]
    public void NativeProbeUsesAbsoluteSpeed()
    {
        var (sim, corpse, raiser) = Room(0, 45); raiser.MoveDirection = 2; raiser.MovementSpeed = Fixed.FromInt(-8);
        Assert.True(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single(), nativeMovement: true));
        Assert.False(corpse.IsDead);
    }

    [Fact]
    public void NativeChaseUsesMovementDirectionWhileLegacyUsesEnemyDirection()
    {
        var (sim, corpse, raiser) = Room(0, 45); raiser.MoveDirection = 2;
        Assert.False(ArchvileActions.TryRaise(sim, raiser, sim.Players.Single()));
        raiser.Brain = MonsterBrain.ForType(64); raiser.SetTargetPointer(sim.Players.Single());
        raiser.ReactionTime = 0; raiser.SpawnState = raiser.IdleState = 0; raiser.SeeState = -1;
        raiser.States.Configure(raiser, [new(-1, 0)], 0);
        raiser.Brain!.Tick(sim, raiser, nativeMovement: true);
        Assert.False(corpse.IsDead); Assert.Equal(MonsterMode.Heal, raiser.Brain.Mode);
        Assert.Equal(sim.Players.Single().Id, raiser.TargetId);
    }

    private static (AuthoritySimulation, Actor, Actor) Room(short x, short y)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = x, Y = y }]
        });
        var corpse = sim.Actors.Single(a => a.ClassDoomEdNum == 3004);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        var raiser = sim.AddBot(0, 0); raiser.MovementSpeed = Fixed.FromInt(8); raiser.Radius = Fixed.FromInt(20);
        return (sim, corpse, raiser);
    }
}
