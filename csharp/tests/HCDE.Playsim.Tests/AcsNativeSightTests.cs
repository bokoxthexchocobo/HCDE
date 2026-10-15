using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsNativeSightTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 0)]
    [InlineData(6, 1)]
    public void AcsFlagsTranslateToNativeSightFlags(int flags, int expected)
    {
        var (sim, source, target) = Room(blockEverything: true); target.ThingId = 77;
        Assert.Equal(expected, Check(sim, source, 0, 77, flags));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(0, 77)]
    [InlineData(88, 77)]
    [InlineData(88, 0)]
    public void AllActorSelectionBranchesIgnoreVisibilityWithoutRandomDraws(int sourceTid, int destTid)
    {
        var (sim, source, target) = Room(); source.ThingId = 88; target.ThingId = 77;
        target.Invisible = target.MVisBlocked = true;
        var activator = destTid == 0 ? target : source;
        Assert.Equal(1, Check(sim, activator, sourceTid, destTid, 0));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void AcsSightUsesPartialTargetAperture()
    {
        var (sim, source, target) = Room(); target.ThingId = 77;
        sim.Floors[1] = 20; sim.Ceilings[1] = 25;
        Assert.False(CombatTrace.HasLineOfSight(sim, source, target));
        Assert.Equal(1, Check(sim, source, 0, 77, 0));
    }

    [Fact]
    public void ZeroSourceAndDestinationReturnOneEvenWithoutActivator()
    {
        var (sim, _, _) = Room(); Assert.Equal(1, Check(sim, null, 0, 0, 0));
        Assert.Equal(0, Check(sim, null, 0, 77, 0));
    }

    [Fact]
    public void DuplicateTidSearchContinuesAfterBlockedTarget()
    {
        var (sim, source, target) = Room(blockEverything: true); target.ThingId = 77;
        var second = sim.AddBot(-64, 100); second.ThingId = 77;
        Assert.Equal(1, Check(sim, source, 0, 77, 0));
    }

    private static int Check(AuthoritySimulation sim, Actor? activator, int sourceTid, int destTid, int flags)
    {
        var stack = new List<int> { 12345, sourceTid, destTid, flags };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = activator }, [], new AcsGlobalStrings(),
            AcsCallFunctions.CheckSight, 3, out var result));
        Assert.Equal(new[] { 12345 }, stack);
        return result;
    }

    private static (AuthoritySimulation, Actor, Actor) Room(bool blockEverything = false)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level; var lines = geometry.Lines.ToArray();
        if (blockEverything) lines[6] = new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128,
            SideFront = 0, SideBack = 1, Flags = LevelLine.TwoSidedFlag | LevelLine.BlockEverythingFlag };
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var source = sim.AddBot(-64, 0);
        return (sim, source, sim.Players.Single());
    }
}
