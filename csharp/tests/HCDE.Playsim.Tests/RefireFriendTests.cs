using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RefireFriendTests
{
    [Theory]
    [InlineData(65, 18)]
    [InlineData(7, 28)]
    [InlineData(68, 28)]
    [InlineData(84, 34)]
    public void InterveningFriendStopsRepeatWithoutConsumingRefireRandom(int type, int calls)
    {
        var sim = Room(type); var shooter = sim.Actors.Single(a => a.DoomEdNum == type);
        var friend = sim.Actors.Single(a => a.DoomEdNum == 3001);
        shooter.Friendly = friend.Friendly = true; shooter.ReactionTime = 0;
        var target = sim.Players.Single(); target.Friendly = false; target.Invulnerable = true;
        for (var i = 0; i < calls; i++) ActorChaseActions.ExtChase(shooter, false, true, playActive: false);
        friend.Y = default;
        Assert.True(MonsterBrain.HitFriend(sim, shooter, target));
        shooter.Ambush = true;
        shooter.Brain!.Tick(sim, shooter, MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPlayActive);
        Assert.Equal(MonsterMode.Chase, shooter.Brain!.Mode);
        Assert.True(shooter.Ambush);
        Assert.Null(sim.CaptureState().CPosRefireRandomState);
    }

    [Theory]
    [InlineData(65, 18)]
    [InlineData(7, 28)]
    [InlineData(68, 28)]
    [InlineData(84, 34)]
    public void RefireFacesTargetAndClearsAmbushWithChaseTurningDisabled(int type, int calls)
    {
        var sim = Room(type);
        var shooter = sim.Actors.Single(a => a.DoomEdNum == type);
        shooter.ReactionTime = 0;
        sim.Players.Single().Invulnerable = true;
        for (var i = 0; i < calls; i++)
            shooter.Brain!.Tick(sim, shooter, MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPlayActive);
        shooter.Angle = BamAngle.FromDegrees(180);
        shooter.Ambush = true;
        shooter.PitchDegrees = 17;
        shooter.Brain!.Tick(sim, shooter, MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPlayActive);
        Assert.Equal(0, shooter.Angle.ToDegrees(), 5);
        Assert.False(shooter.Ambush);
        Assert.Equal(17, shooter.PitchDegrees);
        Assert.NotNull(sim.CaptureState().CPosRefireRandomState);
    }

    [Theory]
    [InlineData(false, true, 0, false)]
    [InlineData(true, false, 0, false)]
    [InlineData(true, true, 100, false)]
    [InlineData(true, true, 0, true)]
    public void FriendTraceRequiresFriendlyShooterAndFirstFriendlyVictim(bool friendlyShooter, bool friendlyBlocker, int y, bool expected)
    {
        var sim = Room(65); var shooter = sim.Actors.Single(a => a.DoomEdNum == 65);
        var blocker = sim.Actors.Single(a => a.DoomEdNum == 3001);
        shooter.Friendly = friendlyShooter; blocker.Friendly = friendlyBlocker;
        blocker.Y = Fixed.FromInt(y); sim.Players.Single().Friendly = false;
        Assert.Equal(expected, MonsterBrain.HitFriend(sim, shooter, sim.Players.Single()));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type },
            new LevelThing { Type = 3001, X = 250, Y = 100 }] });
}
