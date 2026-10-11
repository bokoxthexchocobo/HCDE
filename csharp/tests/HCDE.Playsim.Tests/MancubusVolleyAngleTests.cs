using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MancubusVolleyAngleTests
{
    [Theory]
    [InlineData(20, 11.25, 0, 11.25)]
    [InlineData(40, 348.75, 0, 348.75)]
    [InlineData(60, 0, 354.375, 5.625)]
    public void VolleyChangesActorAngleAndPreservesNativeProjectileSpread(int shotTic,
        double actorAngle, double firstAngle, double secondAngle)
    {
        var (sim, monster) = Room();
        for (var tic = 0; tic < shotTic; tic++) Tick(sim, monster);
        var count = sim.Actors.OfType<ProjectileActor>().Count();
        Tick(sim, monster);
        Assert.Equal(actorAngle, monster.Angle.ToDegrees(), 5);
        var shots = sim.Actors.OfType<ProjectileActor>().Skip(count).ToArray();
        Assert.Equal(2, shots.Length);
        Assert.Equal(firstAngle, shots[0].Angle.ToDegrees(), 5);
        Assert.Equal(secondAngle, shots[1].Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(20)]
    [InlineData(40)]
    [InlineData(60)]
    public void SaveBeforeVolleyReproducesActorAngleAndProjectileVelocities(int shotTic)
    {
        var (sim, monster) = Room();
        for (var tic = 0; tic < shotTic; tic++) Tick(sim, monster);
        var count = sim.Actors.OfType<ProjectileActor>().Count();
        var bytes = SimSavegame.Write(sim);
        Tick(sim, monster);
        var angle = monster.Angle;
        var expected = sim.Actors.OfType<ProjectileActor>().Skip(count)
            .Select(p => (p.Angle, p.VelocityX, p.VelocityY, p.VelocityZ)).ToArray();
        SimSavegame.Apply(sim, bytes);
        monster = sim.Actors.Single(a => a.DoomEdNum == 67);
        Tick(sim, monster);
        Assert.Equal(angle, monster.Angle);
        Assert.Equal(expected, sim.Actors.OfType<ProjectileActor>().Skip(count)
            .Select(p => (p.Angle, p.VelocityX, p.VelocityY, p.VelocityZ)).ToArray());
    }

    private static void Tick(AuthoritySimulation sim, Actor monster) => monster.Brain!.Tick(sim, monster,
        MonsterChaseFlags.NoDirectionTurn | MonsterChaseFlags.NoPlayActive);

    [Theory]
    [InlineData(20)]
    [InlineData(40)]
    [InlineData(60)]
    public void DeviatedMissilesUseNativeHorizontalSpeedAndRetainVerticalVelocity(int shotTic)
    {
        var (sim, monster) = Room();
        sim.Players.Single().Z = Fixed.FromInt(100);
        for (var tic = 0; tic < shotTic; tic++) Tick(sim, monster);
        var count = sim.Actors.OfType<ProjectileActor>().Count();
        Tick(sim, monster);
        var shots = sim.Actors.OfType<ProjectileActor>().Skip(count).ToArray();
        Assert.Equal(2, shots.Length);
        Assert.True(shots[0].VelocityZ.Raw > 0);
        Assert.Equal(shots[0].VelocityZ, shots[1].VelocityZ);
        Assert.Equal(shots[1].Speed, shots[1].VelXYToSpeed(), 4);
        if (shotTic == 60) Assert.Equal(shots[0].Speed, shots[0].VelXYToSpeed(), 4);
        else Assert.True(shots[0].VelXYToSpeed() < shots[0].Speed);
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 67 }],
        });
        var monster = sim.Actors.Single(a => a.DoomEdNum == 67);
        monster.ReactionTime = 0;
        return (sim, monster);
    }
}
