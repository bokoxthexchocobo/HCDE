using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorViewHeightServiceTests
{
    [Fact]
    public void Get_PlayerUsesDefaultViewHeight()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var expected = (int)Math.Floor(PlayerPawn.StandingViewHeight * 65536.0);
        Assert.Equal(expected, AcsActorViewHeightService.Get(sim, 0, player));
    }

    [Fact]
    public void Get_NonPlayerUsesCameraHeight()
    {
        var sim = Room();
        var bot = sim.AddBot(80, 0);
        bot.CameraHeight = 22;
        Assert.Equal((int)Math.Floor(22 * 65536.0), AcsActorViewHeightService.Get(sim, 0, bot));
    }

    private static AuthoritySimulation Room()
        => AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
}
