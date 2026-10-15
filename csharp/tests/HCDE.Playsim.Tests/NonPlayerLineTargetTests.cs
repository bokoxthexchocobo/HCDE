using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class NonPlayerLineTargetTests
{
    [Fact]
    public void NonplayerLineTargetFindsActorAlongFacingDirection()
    {
        var (sim, source, target) = Room();
        Assert.Same(target, AcsActorPointer.Resolve(sim, source, AcsActorPointer.LineTarget));
        source.Angle = BamAngle.FromDegrees(180);
        Assert.Null(AcsActorPointer.Resolve(sim, source, AcsActorPointer.LineTarget));
    }

    [Fact]
    public void NonShootableActorDoesNotBlockLineTarget()
    {
        var (sim, source, target) = Room(); var intermediate = sim.AddBot(0, 0); intermediate.Shootable = false;
        Assert.Same(target, AcsActorPointer.Resolve(sim, source, AcsActorPointer.LineTarget));
    }

    [Fact]
    public void FirstShootableActorIsSelected()
    {
        var (sim, source, _) = Room(); var intermediate = sim.AddBot(0, 0);
        Assert.Same(intermediate, AcsActorPointer.Resolve(sim, source, AcsActorPointer.LineTarget));
    }

    [Fact]
    public void ClosedSectorOpeningBlocksTarget()
    {
        var (sim, source, _) = Room(); sim.Ceilings[1] = 0;
        Assert.Null(AcsActorPointer.Resolve(sim, source, AcsActorPointer.LineTarget));
    }

    [Fact]
    public void AcsSetActivatorCanUseNonplayerLineTarget()
    {
        var (sim, source, target) = Room(); var binding = new AcsActivatorBinding { Value = source };
        var stack = new List<int> { 0, AcsActorPointer.LineTarget };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, binding, [], new(), AcsCallFunctions.SetActivator, 2, out var result));
        Assert.Equal(1, result); Assert.Same(target, binding.Value);
    }

    [Fact]
    public void SavedGeometryAndActorStateReproduceSelection()
    {
        var (sim, source, target) = Room(); var saved = SimSavegame.Write(sim, true);
        source.Angle = BamAngle.FromDegrees(180); SimSavegame.Apply(sim, saved);
        Assert.Same(target, AcsActorPointer.Resolve(sim, source, AcsActorPointer.LineTarget));
    }

    private static (AuthoritySimulation, Actor, Actor) Room()
    {
        var level = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel { Lines = level.Lines, Sides = level.Sides,
            Sectors = level.Sectors, Things = [] });
        var source = sim.AddBot(-80, 0); var target = sim.AddBot(80, 0); source.Angle = BamAngle.FromDegrees(0);
        return (sim, source, target);
    }
}
