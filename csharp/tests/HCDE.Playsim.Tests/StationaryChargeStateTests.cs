using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StationaryChargeStateTests
{
    [Theory]
    [InlineData(false, true, 1)]
    [InlineData(false, false, 3)]
    [InlineData(true, true, 3)]
    [InlineData(true, false, 3)]
    public void StationaryChargeSelectsSeeOrIdleWithoutSlamAndSurvivesSave(bool dormant, bool hasSee, int expected)
    {
        var sim = Room(); var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        var entries = new int[4];
        soul.States.Configure(soul, Enumerable.Range(0, 4).Select(i => new ActorFrame(9, i, _ => entries[i]++)), 2);
        Array.Clear(entries);
        soul.SpawnState = 0; soul.SeeState = hasSee ? 1 : -1; soul.IdleState = 3;
        soul.SlamState = 2; soul.PainState = -1; soul.RetargetAfterSlam = true;
        soul.Brain!.StartCharge(soul, sim.Players.Single()); soul.Dormant = dormant;
        soul.VelocityX = soul.VelocityY = default; soul.VelocityZ = Fixed.FromInt(20);
        var before = SimSavegame.Write(sim); soul.Tick();
        Assert.False(soul.Brain.Charging); Assert.Equal(expected, soul.States.Current);
        Assert.Equal(dormant ? -1 : 9, soul.States.RemainingTics);
        Assert.Equal(1, entries[expected]); Assert.Equal(1, entries.Sum());
        Assert.Equal(default, soul.VelocityZ); Assert.Equal(100, sim.Players.Single().Health);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
        SimSavegame.Apply(sim, before); Array.Clear(entries); soul.Tick();
        Assert.Equal(expected, soul.States.Current); Assert.Equal(1, entries[expected]);
    }

    [Fact]
    public void NoAutoOffSkullFlySkipsStationaryStateSelection()
    {
        var sim = Room(); var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        soul.States.Configure(soul, [new ActorFrame(-1, 0), new ActorFrame(-1, 1)], 0);
        soul.SeeState = 1; soul.PainState = -1; soul.NoAutoOffSkullFly = true;
        soul.Brain!.StartCharge(soul, sim.Players.Single()); soul.VelocityX = soul.VelocityY = soul.VelocityZ = default;
        soul.Tick(); Assert.True(soul.Brain.Charging); Assert.Equal(0, soul.States.Current);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 800 }, new LevelThing { Type = 3006, Z = 100 }] });
}
