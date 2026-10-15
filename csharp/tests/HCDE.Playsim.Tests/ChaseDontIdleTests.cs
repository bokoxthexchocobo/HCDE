using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseDontIdleTests
{
    [Theory]
    [InlineData(false, 512, true)]
    [InlineData(true, 0, true)]
    [InlineData(false, 0, false)]
    [InlineData(false, 528, false)]
    [InlineData(true, 16, false)]
    public void TargetlessChaseUsesNativeWanderFallback(bool friendly, int flags, bool moves)
    {
        var (sim, actor) = Room(); actor.Friendly = friendly;
        actor.Brain!.Tick(sim, actor, (MonsterChaseFlags)flags, nativeMovement: true);
        Assert.Equal(Fixed.FromInt(moves ? 8 : 0), actor.X);
        Assert.Equal(moves ? 9 : 10, actor.MoveCount);
        Assert.Equal(!moves, actor.InCombat);
        Assert.Null(actor.TargetId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WanderFallbackHonorsStandStillAndConversation(bool conversation)
    {
        var (sim, actor) = Room(); actor.InConversation = conversation; actor.StandStill = !conversation;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontIdle, nativeMovement: true);
        Assert.Equal(default, actor.X); Assert.Equal(10, actor.MoveCount);
        Assert.Equal(conversation, actor.InCombat);
    }

    [Fact]
    public void ChaseDirectionFlagsAreNotForwardedToOrdinaryWander()
    {
        var (sim, actor) = Room(); actor.MoveDirection = 2;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontIdle | MonsterChaseFlags.NoDirectionTurn, nativeMovement: true);
        Assert.Equal(BamAngle.FromDegrees(45), actor.Angle);
        Assert.Equal(Fixed.FromInt(8), actor.Y);
    }

    [Fact]
    public void UnsupportedLegacyUseFailsBeforeMutation()
    {
        var (sim, actor) = Room();
        Assert.Throws<NotSupportedException>(() => actor.Brain!.Tick(sim, actor, MonsterChaseFlags.DontIdle));
        Assert.Equal(10, actor.MoveCount); Assert.True(actor.InCombat);
    }

    [Fact]
    public void FriendlyAutomaticTicksWanderWithNativePolicy()
    {
        var (sim, actor) = Room(); actor.Friendly = true;
        sim.Tick(); Assert.Equal(Fixed.FromInt(8), actor.X);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] },
            spawnOptions: new SpawnOptions(NativeMonsterMovement: true));
        var actor = sim.AddBot(0, 0);
        actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        actor.ReactionTime = 0; actor.MoveCount = 10; actor.MoveDirection = 0;
        actor.MovementSpeed = Fixed.FromInt(8); actor.InCombat = true;
        actor.SpawnState = actor.IdleState = 0; actor.SeeState = -1;
        actor.States.Configure(actor, [new(-1, 0)], 0);
        return (sim, actor);
    }
}
