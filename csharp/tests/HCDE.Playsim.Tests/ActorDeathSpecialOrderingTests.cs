using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDeathSpecialOrderingTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void DeathSpecialRunsBeforeDeathFrameAction(bool hexenHack, bool nullSource)
    {
        var sim = Room(hexenHack); var player = sim.Players.Single(); var victim = sim.Actors[1];
        player.ThingId = 7; player.VelocityX = Fixed.FromInt(8);
        victim.SpecialArgs[0] = 7;
        int? observedSpecial = null; int? observedVelocity = null;
        victim.DeathState = 1; victim.GibHealth = int.MinValue;
        victim.States.Configure(victim, [new(-1, 0), new(-1, 1, actor =>
        {
            observedSpecial = actor.Special;
            observedVelocity = player.VelocityX.Raw;
        })], 0);
        ActorDamage.Apply(victim, 1000, nullSource ? null : player);
        Assert.Equal(hexenHack ? 19 : 0, observedSpecial);
        Assert.Equal(0, observedVelocity);
        Assert.Equal(1, victim.DeathCount);
    }

    [Fact]
    public void DirectHealthAssignmentStillDoesNotRunDamageDeathSpecial()
    {
        var sim = Room(false); var player = sim.Players.Single();
        player.VelocityX = Fixed.FromInt(8);
        sim.Actors[1].Health = 0;
        Assert.Equal(19, sim.Actors[1].Special);
        Assert.Equal(Fixed.FromInt(8), player.VelocityX);
    }

    private static AuthoritySimulation Room(bool hexenHack) => AuthoritySimulation.Start(new PlayLevel
    {
        HexenHack = hexenHack, Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20, Special = 19 }],
    });
}
