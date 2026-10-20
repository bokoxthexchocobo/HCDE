using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PZMovementSmoothStepViewTests
{
    [Fact]
    public void ApplySmoothStepUpView_LowersViewHeightWhenBelowFloor()
    {
        var player = new PlayerPawn { ViewHeight = PlayerPawn.StandingViewHeight };
        ActorPhysics.ApplySmoothStepUpView(player, -4, 0);
        Assert.Equal(PlayerPawn.StandingViewHeight - 4, player.ViewHeight, 3);
    }

}
