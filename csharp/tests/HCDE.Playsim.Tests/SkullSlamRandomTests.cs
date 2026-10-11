using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullSlamRandomTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(8, true)]
    [InlineData(8, false)]
    public void SlamUsesNamedDamageDrawIncludingZeroAndUnshootableTargets(int damage, bool shootable)
    {
        var sim = Room(); var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true; target.Shootable = shootable;
        soul.Damage = damage; soul.Brain!.StartCharge(soul, target);
        var save = SimSavegame.Write(sim); var combat = sim.CombatRandomState;
        var native = NativeStateRandom.Seed(42, 0x333cfa17u);
        var expected = damage * (1 + (int)(NativeStateRandom.Next(ref native) & 7));
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.False(soul.Brain.Charging);
        Assert.Equal(shootable ? 1000 - expected : 1000, target.Health);
        Assert.Equal(native, sim.CaptureState().MissileDamageRandomState);
        Assert.Equal(combat, sim.CombatRandomState);
        SimSavegame.Apply(sim, save);
        for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        Assert.Equal(shootable ? 1000 - expected : 1000, target.Health);
        Assert.Equal(native, sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void NegativeDamageWithoutNativeFunctionIsExplicitlyUnsupported()
    {
        var sim = Room(); var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        soul.Damage = -5; soul.Brain!.StartCharge(soul, sim.Players.Single());
        Assert.Throws<NotSupportedException>(() =>
        {
            for (var i = 0; i < 10 && soul.Brain.Charging; i++) soul.Tick();
        });
        Assert.Equal(100, sim.Players.Single().Health);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3006 }] }, rngSeed: 42);
}
