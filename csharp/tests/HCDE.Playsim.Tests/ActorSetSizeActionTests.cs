using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSetSizeActionTests
{
    [Theory]
    [InlineData(-1, -2, 20, 56)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(12.5, 30.5, 12.5, 30.5)]
    public void NegativeDimensionsPreserveAndZeroDimensionsAreAllowed(double radius, double height, double expectedRadius, double expectedHeight)
    {
        var actor = new Actor(); Assert.True(ActorPropertyActions.SetSize(actor, radius, height));
        Assert.Equal(expectedRadius, actor.Radius.ToDouble()); Assert.Equal(expectedHeight, actor.Height.ToDouble());
    }

    [Fact]
    public void CollisionFailureRestoresBothDimensions()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.AddBot(50, 0);
        var radius = actor.Radius; var height = actor.Height;
        Assert.False(ActorPropertyActions.SetSize(actor, 40, 60, true));
        Assert.Equal(radius, actor.Radius); Assert.Equal(height, actor.Height);
        Assert.True(ActorPropertyActions.SetSize(actor, 10, 30, true));
    }

    [Fact]
    public void CeilingTestIsOptional()
    {
        var actor = Room().AddBot(0, 0); var height = actor.Height;
        Assert.False(ActorPropertyActions.SetSize(actor, height: 129, testPosition: true));
        Assert.Equal(height, actor.Height);
        Assert.True(ActorPropertyActions.SetSize(actor, height: 129)); Assert.Equal(129, actor.Height.ToDouble());
    }

    [Fact]
    public void PlayerStandingHeightOnlyChangesAfterSuccessfulTest()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); var standing = player.FullHeight;
        Assert.False(ActorPropertyActions.SetSize(player, height: 129, testPosition: true));
        Assert.Equal(standing, player.FullHeight);
        Assert.True(ActorPropertyActions.SetSize(player, height: 40, testPosition: true));
        Assert.Equal(40, player.FullHeight); Assert.Equal(40, player.Height.ToDouble());
    }

    [Fact]
    public void FrameActionResizesAndInvalidInputsFailBeforeMutation()
    {
        var actor = Room().AddBot(0, 0);
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: self =>
            ActorPropertyActions.SetSize(self, 12, 30, true))], 0);
        actor.States.Enter(actor, 1); Assert.Equal(12, actor.Radius.ToDouble()); Assert.Equal(30, actor.Height.ToDouble());
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.SetSize(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.SetSize(actor, height: double.PositiveInfinity));
        Assert.Equal(12, actor.Radius.ToDouble()); Assert.Equal(30, actor.Height.ToDouble());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
