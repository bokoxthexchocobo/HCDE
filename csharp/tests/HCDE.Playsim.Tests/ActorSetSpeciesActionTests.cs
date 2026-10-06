using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSetSpeciesActionTests
{
    [Fact]
    public void CustomSpeciesOverridesAncestryAndUsesNativeNameCase()
    {
        var sim = Room(); var first = sim.AddBot(0, 0, 58); var second = sim.AddBot(100, 0, 3001);
        ActorPropertyActions.SetSpecies(first, "Squad"); ActorPropertyActions.SetSpecies(second, "squad");
        Assert.True(first.IsSameSpecies(second)); Assert.True(first.ProjectileImmune(second));
        Assert.Equal(2, ActorJumpActions.CheckSpecies(first, 2, "SQUAD"));
        Assert.Null(ActorJumpActions.CheckSpecies(first, 2, "Demon"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("NONE")]
    public void ClearRestoresClassDerivedSpecies(string? value)
    {
        var actor = Room().AddBot(0, 0, 58);
        ActorPropertyActions.SetSpecies(actor, "Squad"); ActorPropertyActions.SetSpecies(actor, value);
        Assert.Null(actor.Species); Assert.Equal(2, ActorJumpActions.CheckSpecies(actor, 2, "Demon"));
    }

    [Fact]
    public void FrameActionAssignsTargetAndNullPointerPreservesIt()
    {
        var sim = Room(); var caller = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7);
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self =>
            ActorPropertyActions.SetSpecies(self, "Squad", AcsActorPointer.Target))], 0);
        caller.States.Enter(caller, 1); Assert.Equal("Squad", target.Species); Assert.Null(caller.Species);
        ActorPropertyActions.SetSpecies(target, "Other", AcsActorPointer.Null); Assert.Equal("Squad", target.Species);
    }

    [Fact]
    public void ChecksumTracksSpeciesWithCaseInsensitiveIdentity()
    {
        var left = Room(); var right = Room(); var a = left.AddBot(0, 0); var b = right.AddBot(0, 0);
        a.Brain = b.Brain = null;
        ActorPropertyActions.SetSpecies(a, "Squad"); left.Tick(); right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        ActorPropertyActions.SetSpecies(b, "squad"); left.Tick(); right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
