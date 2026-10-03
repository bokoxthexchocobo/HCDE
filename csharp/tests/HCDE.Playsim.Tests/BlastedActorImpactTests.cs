using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastedActorImpactTests
{
    [Theory]
    [InlineData(3, 0)]
    [InlineData(4, 3)]
    public void TransfersMomentumAndDamagesAboveStrictThreshold(int speed, int damage)
    {
        var (sim, source, target) = Setup(); source.VelocityX = Fixed.FromInt(speed);
        Assert.False(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(1000 - damage, target.Health); Assert.Equal(damage == 0 ? 1000 : 999, source.Health);
        if (damage == 0) Assert.Equal(speed, target.VelocityX.ToDouble());
    }

    [Theory]
    [InlineData("boss")]
    [InlineData("dontblast")]
    [InlineData("notmonster")]
    [InlineData("notshootable")]
    public void IneligibleTargetsReceiveNoBlastTransfer(string exclusion)
    {
        var (sim, source, target) = Setup(); source.VelocityX = Fixed.FromInt(4);
        if (exclusion == "boss") target.Boss = true;
        if (exclusion == "dontblast") target.DontBlast = true;
        if (exclusion == "notmonster") target.IsMonster = false;
        if (exclusion == "notshootable") target.Shootable = false;
        Assert.False(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(1000, target.Health); Assert.Equal(1000, source.Health); Assert.Equal(0, target.VelocityX.Raw);
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(16, true)]
    [InlineData(3003, false)]
    public void SupportedDoomBossDefaultsMatchMapAndDynamicSpawns(int type, bool boss)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = type }] });
        Assert.Equal(boss, Assert.Single(sim.Actors).Boss); Assert.Equal(boss, sim.AddBot(500, 0, type).Boss);
    }

    [Theory]
    [InlineData("boss")]
    [InlineData("dontblast")]
    public void AcsEligibilityFlagsCanBeSetAndCleared(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, flag, true));
        Assert.True(AcsActorFlags.TryGet(actor, flag.ToUpperInvariant(), out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, flag, false));
        Assert.True(AcsActorFlags.TryGet(actor, flag, out value)); Assert.False(value);
    }

    [Fact]
    public void SpeciesImmunitySuppressesDamageButStillTransfersMomentum()
    {
        var (sim, source, target) = Setup();
        source.DoHarmSpecies = target.DoHarmSpecies = false;
        source.VelocityX = Fixed.FromInt(4);
        Assert.False(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(1000, target.Health); Assert.Equal(1000, source.Health);
        Assert.Equal(4, target.VelocityX.ToDouble());
    }
    private static (AuthoritySimulation, Actor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var source = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        source.Brain = target.Brain = null; source.Health = target.Health = 1000;
        source.DoHarmSpecies = target.DoHarmSpecies = true;
        source.PainChance = target.PainChance = 0; source.Mass = 200; target.Mass = 300; source.Blasted = true;
        return (sim, source, target);
    }
}
