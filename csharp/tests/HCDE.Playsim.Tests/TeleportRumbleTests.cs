using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportRumbleTests
{
    [Theory]
    [InlineData(70, 0, 18)]
    [InlineData(154, 3, 0)]
    public void SuccessfulMoveRumblesBeforeAngleMomentumFreezeAndPostHook(int special, int speed, int reaction)
    {
        var sim = Room(); var player = sim.Players.Single(); var calls = new List<string>();
        player.VelocityX = Fixed.FromInt(3);
        player.PreTeleportFunction = (_, _, _, _) => { calls.Add("pre"); return true; };
        sim.RumbleRequested += request =>
        {
            Assert.Equal(new SimulationRumbleRequest(player.Id, "misc/teleport", true), request);
            Assert.Equal(200, player.X.ToDouble()); Assert.Equal(0, player.Angle.ToDegrees());
            Assert.Equal(3, player.VelocityX.ToDouble()); Assert.Equal(0, player.ReactionTime); calls.Add("rumble");
        };
        player.PostTeleportFunction = (_, _, _, _) =>
        {
            Assert.Equal(90, player.Angle.ToDegrees()); Assert.Equal(speed, player.VelocityX.ToDouble());
            Assert.Equal(reaction, player.ReactionTime); calls.Add("post");
        };
        Assert.True(LineSpecials.ExecuteTeleportSpecial(sim, special, 0, 7, player, false));
        Assert.Equal(new[] { "pre", "rumble", "post" }, calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CancelBlockedOrDisabledWorldPreferenceSuppressTeleportPattern(int mode)
    {
        var sim = Room(); var player = sim.Players.Single(); var requests = new List<SimulationRumbleRequest>();
        sim.RumbleRequested += requests.Add;
        if (mode == 0) player.PreTeleportFunction = (_, _, _, _) => false;
        if (mode == 1) sim.AddBot(200, 0).NoTelefrag = true;
        if (mode == 2) sim.WorldHapticsEnabled = false;
        Assert.Equal(mode == 2, LineSpecials.ExecuteTeleportSpecial(sim, 70, 0, 7, player, false));
        Assert.Empty(requests);
    }

    [Fact]
    public void NonPlayerTeleportDoesNotRequestPlayerHaptics()
    {
        var sim = Room(); var actor = sim.AddBot(80, 0); var calls = 0; sim.RumbleRequested += _ => calls++;
        Assert.True(LineSpecials.ExecuteTeleportSpecial(sim, 70, 0, 7, actor, false)); Assert.Equal(0, calls);
    }

    [Fact]
    public void SaveRestoreDoesNotRepeatTeleportPresentation()
    {
        var sim = Room(); var player = sim.Players.Single(); var calls = 0; sim.RumbleRequested += _ => calls++;
        Assert.True(LineSpecials.ExecuteTeleportSpecial(sim, 70, 0, 7, player, false)); Assert.Equal(1, calls);
        var saved = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved); Assert.Equal(1, calls);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 14, X = 200, Angle = 90 }],
    });
}
