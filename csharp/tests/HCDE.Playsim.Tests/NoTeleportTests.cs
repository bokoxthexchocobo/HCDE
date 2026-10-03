using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoTeleportTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoTeleportRejectsPlayerAndMonsterWithoutMutatingPose(bool player)
    {
        var sim = Room();
        Actor actor = player ? Assert.Single(sim.Players) : sim.AddBot(-64, 0);
        actor.NoTeleport = true; actor.ReactionTime = 7; actor.VelocityX = Fixed.FromInt(3);
        var x = actor.X;
        Assert.False(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0));
        Assert.Equal(x, actor.X); Assert.Equal(3, actor.VelocityX.ToDouble()); Assert.Equal(7, actor.ReactionTime);
    }

    [Fact]
    public void DoomMissilesDefaultToNoTeleport()
    {
        var sim = Room();
        foreach (var kind in Enum.GetValues<ProjectileKind>())
        {
            var missile = new ProjectileActor(new Actor(), kind);
            Assert.True(missile.NoTeleport);
            Assert.False(LineSpecials.Execute(sim, missile, LineSpecials.Teleport, 0));
        }
    }

    [Fact]
    public void NoTeleportParticipatesInSimulationChecksum()
    {
        var first = Room(); var second = Room(); Assert.Single(first.Players).NoTeleport = true;
        first.Tick(); second.Tick();
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}
