namespace HCDE.Playsim.Tests;

public class PlayerBeginPlayTests
{
    [Theory]
    [InlineData(40)]
    [InlineData(72)]
    public void BeginPlayCapturesCurrentHeight(int height)
    {
        var player = new PlayerPawn { Height = Fixed.FromInt(height) };
        player.BeginPlay();
        Assert.Equal(height, player.FullHeight);
    }

    [Fact]
    public void BeginPlayRunsBaseBeforeCapturingHeight()
    {
        var player = new PlayerPawn { IsMonster = true, Dormant = true, InactiveState = 1 };
        player.States.Configure(player, [new ActorFrame(-1, 0),
            new ActorFrame(-1, 1, actor => actor.Height = Fixed.FromInt(72))], 0);
        player.BeginPlay();
        Assert.True(player.Dormant); Assert.Equal(72, player.FullHeight);
    }
}
