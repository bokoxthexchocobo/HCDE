using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LostSoulTests
{
    [Fact]
    public void ChargeStartsAfterWindupAndHitsOnceWithoutGroundFriction()
    {
        var (sim, soul, player) = Setup();
        Windup(sim, soul);
        for (var i = 0; i < 9; i++) sim.Tick();
        Assert.False(soul.Brain!.Charging);
        sim.Tick();
        Assert.True(soul.Brain.Charging);
        Assert.Equal(20, soul.VelocityX.ToDouble());
        for (var i = 0; i < 30 && soul.Brain.Charging; i++) sim.Tick();
        Assert.False(soul.Brain.Charging);
        Assert.InRange(100 - player.Health, 3, 24);
        Assert.Equal(0, (100 - player.Health) % 3);
        Assert.Equal(default, soul.VelocityX);
        var health = player.Health;
        sim.Tick();
        Assert.Equal(health, player.Health);
        Assert.Empty(sim.Actors.OfType<ProjectileActor>());
    }

    [Fact]
    public void WallInsertedDuringWindupStopsChargeWithoutHurtingTarget()
    {
        var (sim, soul, player) = Setup();
        Windup(sim, soul);
        ((List<LevelLine>)sim.Level.Lines).Add(new LevelLine { X1 = 100, X2 = 100, Y1 = -200, Y2 = 200, SideBack = -1 });
        for (var i = 0; i < 16; i++) sim.Tick();
        Assert.False(soul.Brain!.Charging);
        Assert.InRange(soul.X.ToDouble(), 0, 84);
        Assert.Equal(100, player.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PainAndDeathCancelChargeAndVerticalVelocity(bool kill)
    {
        var (sim, soul, _) = Setup();
        Windup(sim, soul);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.True(soul.Brain!.Charging);
        ActorDamage.Apply(soul, kill ? 1000 : 1);
        sim.Tick();
        Assert.False(soul.Brain.Charging);
        Assert.Equal(default, soul.VelocityX);
        Assert.Equal(default, soul.VelocityZ);
        Assert.Equal(kill ? MonsterMode.Dead : MonsterMode.Pain, soul.Brain.Mode);
    }

    [Fact]
    public void VerticalChargeHitsOverlappingHorizontalTarget()
    {
        var (sim, soul, player) = Setup();
        player.X = default; player.Z = Fixed.FromInt(180); player.NoGravity = true;
        Windup(sim, soul);
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.False(soul.Brain!.Charging);
        Assert.InRange(100 - player.Health, 3, 24);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlaneContactPreservesChargeAndUsesNativeVerticalResponse(bool ceiling)
    {
        var (sim, soul, _) = Setup();
        Windup(sim, soul);
        for (var i = 0; i < 10; i++) sim.Tick();
        soul.Z = Fixed.FromInt(ceiling ? 455 : 1);
        soul.VelocityZ = Fixed.FromInt(ceiling ? 20 : -20);
        sim.Tick();
        Assert.True(soul.Brain!.Charging);
        Assert.Equal(ceiling ? -20 : 0, soul.VelocityZ.ToDouble());
        Assert.InRange(soul.Z.ToDouble(), 0, 456);
        Assert.Equal(20, soul.VelocityX.ToDouble());
    }

    private static void Windup(AuthoritySimulation sim, Actor soul)
    {
        for (var i = 0; i < 20 && soul.Brain!.Mode != MonsterMode.Windup; i++) sim.Tick();
        Assert.Equal(MonsterMode.Windup, soul.Brain!.Mode);
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Lines = new List<LevelLine>(),
            Things = [new LevelThing { Type = 3006 }, new LevelThing { Type = 1, X = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Sides = [new LevelSide { Sector = 0 }],
        }, rngSeed: 42);
        return (sim, sim.Actors.Single(actor => actor.DoomEdNum == 3006), sim.Players.Single());
    }
}
