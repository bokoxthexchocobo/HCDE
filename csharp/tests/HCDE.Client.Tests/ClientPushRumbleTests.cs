using HCDE.MapLoader;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class ClientPushRumbleTests
{
    [Theory]
    [InlineData(0, false, "misc/push")]
    [InlineData(1, false, "misc/pushed")]
    [InlineData(1, true, "misc/push")]
    public void PushRumbleUsesLocalBodyAndPusherWinsWhenBothEligible(int moverNum, bool viewMover, string expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = moverNum == 0 ? 0 : 40 },
                new LevelThing { Type = 2, X = moverNum == 1 ? 0 : 40 }],
        });
        var mover = sim.Players.Single(player => player.PlayerNum == moverNum);
        var target = sim.Players.Single(player => player.PlayerNum != moverNum);
        target.Pushable = true; target.Solid = false;
        using var host = new ClientHost(sim); var patterns = new List<string>();
        host.RumbleRequested += request => patterns.Add(request.Pattern);
        if (viewMover) Assert.True(host.TrySetAudioListener(mover.Id, out var error), error);
        mover.VelocityX = Fixed.FromInt(24); ActorPhysics.Step(sim, mover);
        Assert.Equal(expected, Assert.Single(patterns)); Assert.Equal(6, target.VelocityX.ToDouble());
    }
}
