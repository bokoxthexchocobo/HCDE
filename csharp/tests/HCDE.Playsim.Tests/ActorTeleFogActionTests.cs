using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTeleFogActionTests
{
    [Theory]
    [InlineData("TeleportFog", "ItemFog")]
    [InlineData(null, "TeleportFog")]
    [InlineData("TeleportFog", null)]
    [InlineData(null, null)]
    [InlineData("TeleportFog", "TeleportFog")]
    public void SetAndDoubleSwapRetainOverrides(string? source, string? destination)
    {
        var actor = new Actor(); ActorPropertyActions.SetTeleFog(actor, source, destination);
        Assert.Equal(source, actor.TeleFogSource); Assert.Equal(destination, actor.TeleFogDest);
        ActorPropertyActions.SwapTeleFog(actor);
        Assert.Equal(destination, actor.TeleFogSource); Assert.Equal(source, actor.TeleFogDest);
        ActorPropertyActions.SwapTeleFog(actor);
        Assert.Equal(source, actor.TeleFogSource); Assert.Equal(destination, actor.TeleFogDest);
    }

    [Fact]
    public void FrameActionReplacesAndClearsPreviousOverrides()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetTeleFog(actor, "TeleportFog", "ItemFog");
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: self =>
        { ActorPropertyActions.SetTeleFog(self, null, "TeleportFog"); ActorPropertyActions.SwapTeleFog(self); })], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal("TeleportFog", actor.TeleFogSource); Assert.Null(actor.TeleFogDest);
    }

    [Fact]
    public void AcsTidSelectionAndActionSwapUseSameFields()
    {
        var sim = Room(); var first = sim.AddBot(0, 0, thingId: 7); var second = sim.AddBot(100, 0, thingId: 7);
        var other = sim.AddBot(200, 0, thingId: 8);
        Assert.Equal(2, AcsActorTeleFog.Set(sim, 7, other, "TeleportFog", null));
        ActorPropertyActions.SwapTeleFog(first);
        Assert.Equal(2, AcsActorTeleFog.Swap(sim, 7, other));
        Assert.Equal("TeleportFog", first.TeleFogSource); Assert.Null(first.TeleFogDest);
        Assert.Null(second.TeleFogSource); Assert.Equal("TeleportFog", second.TeleFogDest);
        Assert.Null(other.TeleFogSource); Assert.Null(other.TeleFogDest);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
