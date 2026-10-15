using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsActivatorFailureTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void SetActivatorMissingTidClearsOldActivator(int tid)
    {
        var sim = Room(); var binding = new AcsActivatorBinding { Value = sim.Players.Single() };
        Assert.Equal(0, Call(sim, binding, AcsCallFunctions.SetActivator, [tid])); Assert.Null(binding.Value);
    }

    [Theory]
    [InlineData(AcsActorPointer.Null)]
    [InlineData(AcsActorPointer.Target)]
    public void SetActivatorNullPointerClearsOldActivator(int pointer)
    {
        var sim = Room(); var binding = new AcsActivatorBinding { Value = sim.Players.Single() };
        Assert.Equal(0, Call(sim, binding, AcsCallFunctions.SetActivator, [0, pointer])); Assert.Null(binding.Value);
    }

    [Fact]
    public void SetActivatorExistingTidSelectsActor()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0, thingId: 9); var binding = new AcsActivatorBinding();
        Assert.Equal(1, Call(sim, binding, AcsCallFunctions.SetActivator, [9])); Assert.Same(actor, binding.Value);
    }

    [Fact]
    public void TargetSelectionUsesStoredTargetWithoutBrain()
    {
        var sim = Room(); var player = sim.Players.Single(); var actor = sim.AddBot(80, 0, thingId: 9);
        actor.Brain = null; actor.SetTargetPointer(player); var binding = new AcsActivatorBinding { Value = actor };
        Assert.Equal(1, Call(sim, binding, AcsCallFunctions.SetActivatorToTarget, [0])); Assert.Same(player, binding.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void TargetSelectionFailurePreservesOldActivator(int tid)
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); actor.Brain = null;
        var binding = new AcsActivatorBinding { Value = actor };
        Assert.Equal(0, Call(sim, binding, AcsCallFunctions.SetActivatorToTarget, [tid])); Assert.Same(actor, binding.Value);
    }

    private static int Call(AuthoritySimulation sim, AcsActivatorBinding binding, int function, int[] args)
    {
        var stack = new List<int> { 77 }; stack.AddRange(args);
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [], new(), function, args.Length, out var result));
        Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
