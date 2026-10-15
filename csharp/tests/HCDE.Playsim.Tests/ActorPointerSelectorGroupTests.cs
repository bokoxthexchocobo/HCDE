using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class ActorPointerSelectorGroupTests
{
    [Theory]
    [InlineData(AcsActorPointer.Target | AcsActorPointer.Master)]
    [InlineData(AcsActorPointer.Tracer | AcsActorPointer.FriendPlayer)]
    [InlineData(AcsActorPointer.Player1 | (AcsActorPointer.Player1 << 1))]
    [InlineData(AcsActorPointer.Player1 | AcsActorPointer.Null)]
    public void AmbiguousGroupFallsBackRatherThanSelectingFirstBit(int selector)
    {
        var sim = Room(); var player = sim.Players.Single(); var actor = sim.AddBot(80, 0);
        actor.SetTargetPointer(player); actor.MasterId = player.Id; actor.TracerId = player.Id; actor.FriendPlayer = 1;
        Assert.Same(actor, AcsActorPointer.Resolve(sim, actor, selector));
    }

    [Fact]
    public void AmbiguousGeneralGroupFallsThroughToValidStaticGroup()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0);
        Assert.Same(sim.Players.Single(), AcsActorPointer.Resolve(sim, actor,
            AcsActorPointer.Target | AcsActorPointer.Master | AcsActorPointer.Player1));
    }

    [Fact]
    public void ValidGeneralGroupHasPriorityOverStaticGroup()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); var target = sim.AddBot(100, 0);
        actor.SetTargetPointer(target);
        Assert.Same(target, AcsActorPointer.Resolve(sim, actor, AcsActorPointer.Target | AcsActorPointer.Player1));
    }

    [Fact]
    public void AmbiguousPlayerGroupFallsThroughToGeneralGroup()
    {
        var sim = Room(); var player = sim.Players.Single(); var target = sim.AddBot(80, 0); player.SetTargetPointer(target);
        Assert.Same(target, AcsActorPointer.Resolve(sim, player,
            AcsActorPointer.PlayerGetTarget | AcsActorPointer.PlayerGetConversation | AcsActorPointer.Target));
    }

    [Fact]
    public void NullOriginCanStillResolveValidStaticPlayer()
    {
        var sim = Room(); Assert.Same(sim.Players.Single(), AcsActorPointer.Resolve(sim, null, AcsActorPointer.Player1));
    }

    [Fact]
    public void AcsSetActivatorUsesNativeGroupFallback()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); actor.SetTargetPointer(sim.Players.Single());
        var binding = new AcsActivatorBinding { Value = actor };
        var stack = new List<int> { 0, AcsActorPointer.Target | AcsActorPointer.Master };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [], new(), AcsCallFunctions.SetActivator, 2, out var result));
        Assert.Equal(1, result); Assert.Same(actor, binding.Value); Assert.Empty(stack);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
