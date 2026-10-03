using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastedNonSolidImpactTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void BlastTransferDoesNotRequireSolidMoverOrTarget(bool sourceSolid, bool targetSolid)
    {
        var (sim, source, target) = Setup(); source.Solid = sourceSolid; target.Solid = targetSolid;
        Assert.False(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(4, target.VelocityX.ToDouble());
        Assert.Equal(997, target.Health); Assert.Equal(999, source.Health);
        Assert.Equal(0, source.X.Raw);
    }

    [Theory]
    [InlineData("boss")]
    [InlineData("dontblast")]
    [InlineData("notmonster")]
    [InlineData("notshootable")]
    [InlineData("noblockmap")]
    [InlineData("dead")]
    [InlineData("destroyed")]
    public void ExcludedNonSolidTargetDoesNotBlockOrReceiveMomentum(string exclusion)
    {
        var (sim, source, target) = Setup(); target.Solid = false;
        switch (exclusion)
        {
            case "boss": target.Boss = true; break;
            case "dontblast": target.DontBlast = true; break;
            case "notmonster": target.IsMonster = false; break;
            case "notshootable": target.Shootable = false; break;
            case "noblockmap": target.NoBlockmap = true; break;
            case "dead": target.Health = 0; break;
            case "destroyed": target.Destroy(); break;
        }
        var health = target.Health;
        Assert.True(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(0, target.VelocityX.Raw); Assert.Equal(health, target.Health);
        Assert.Equal(1000, source.Health);
    }

    [Fact]
    public void NonBlastedSolidActorPassesNonSolidMonster()
    {
        var (sim, source, target) = Setup(); source.Blasted = false; target.Solid = false;
        Assert.True(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(0, target.VelocityX.Raw); Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void NonSolidBlastedMoverPassesExcludedSolidTarget()
    {
        var (sim, source, target) = Setup(); source.Solid = false; target.Boss = true;
        Assert.True(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(0, target.VelocityX.Raw);
    }

    [Fact]
    public void VerticallySeparatedNonSolidTargetIsUnaffected()
    {
        var (sim, source, target) = Setup(); target.Solid = false;
        target.Z = source.Height;
        Assert.True(ActorPhysics.TryMove(sim, source, 20, 0, out _));
        Assert.Equal(0, target.VelocityX.Raw); Assert.Equal(1000, target.Health);
    }

    private static (AuthoritySimulation, Actor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var source = sim.AddBot(0, 0); var target = sim.AddBot(50, 0);
        source.Brain = target.Brain = null; source.Health = target.Health = 1000;
        source.DoHarmSpecies = target.DoHarmSpecies = true;
        source.PainChance = target.PainChance = 0; source.Mass = 200; target.Mass = 300;
        source.Blasted = true; source.VelocityX = Fixed.FromInt(4);
        return (sim, source, target);
    }
}
