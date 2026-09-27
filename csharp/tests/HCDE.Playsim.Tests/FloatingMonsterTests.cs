using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloatingMonsterTests
{
    [Theory]
    [InlineData(3005)]
    [InlineData(71)]
    [InlineData(3006)]
    public void NativeFloatersRiseFourUnitsTowardNearbyElevatedTarget(int type)
    {
        var (sim, monster, player) = Setup(type);
        Acquire(sim, monster);
        player.Z = Fixed.FromInt(200);
        var before = monster.Z.ToDouble(); sim.Tick();
        Assert.True(monster.Floating);
        Assert.Equal(before + 4, monster.Z.ToDouble());
        Assert.Equal(default, monster.VelocityZ); // Float is a position adjustment, not acceleration.
    }

    [Fact]
    public void FloatDistanceGateIsStrictAndUsesTargetCenter()
    {
        var (sim, monster, player) = Setup(3005);
        Acquire(sim, monster);
        player.X = Fixed.FromInt(84); monster.Z = default;
        sim.Tick(); Assert.Equal(0, monster.Z.ToDouble()); // Distance equals 3 * (28 - 0).
        player.X = Fixed.FromInt(83);
        sim.Tick(); Assert.Equal(4, monster.Z.ToDouble());
    }

    [Fact]
    public void FloatDescendsAndRespectsPlanes()
    {
        var (sim, monster, player) = Setup(3005);
        Acquire(sim, monster);
        monster.Z = Fixed.FromInt(200);
        sim.Tick(); Assert.Equal(196, monster.Z.ToDouble());
        monster.Z = Fixed.FromInt(455); player.Z = Fixed.FromInt(490);
        player.X = Fixed.FromInt(60);
        sim.Tick(); Assert.Equal(456, monster.Z.ToDouble());
    }

    [Fact]
    public void NoGravityAloneDoesNotEnableMonsterFloating()
    {
        var (sim, monster, player) = Setup(3001);
        monster.NoGravity = true;
        Acquire(sim, monster); player.Z = Fixed.FromInt(200);
        sim.Tick(); Assert.Equal(0, monster.Z.ToDouble());
        Assert.False(monster.Floating);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeadOrDisabledMonsterDoesNotFloat(bool dead)
    {
        var (sim, monster, player) = Setup(3005);
        Acquire(sim, monster); player.Z = Fixed.FromInt(200);
        if (dead) monster.Health = 0; else monster.Brain!.Enabled = false;
        sim.Tick(); Assert.Equal(0, monster.Z.ToDouble());
    }

    private static void Acquire(AuthoritySimulation sim, Actor monster)
    {
        for (var i = 0; i < 11; i++) sim.Tick();
        Assert.NotNull(monster.Brain!.TargetId);
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Setup(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = type }, new LevelThing { Type = 1, X = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
        });
        var player = sim.Players.Single(); player.NoGravity = true; player.Invulnerable = true;
        return (sim, sim.Actors.Single(actor => actor.DoomEdNum == type), player);
    }
}
