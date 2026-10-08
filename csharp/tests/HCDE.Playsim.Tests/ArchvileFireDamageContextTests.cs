using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileFireDamageContextTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(0.5, false)]
    [InlineData(0.5, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void FireFactorAffectsBlastButNotDefaultDirectHitAfterSave(double factor, bool fireExists)
    {
        var sim = Room(); var target = sim.Players.Single(); target.Health = 1000;
        target.SetDamageFactor("Fire", factor); target.NoPain = true;
        var archvile = sim.AddBot(0, 0, 64); archvile.Brain = null;
        archvile.DamageType = "Ice";
        SimSavegame.Apply(sim, SimSavegame.Write(sim));
        ArchvileActions.Attack(sim, archvile, target, fireExists);
        Assert.Equal(1000 - 20 - (fireExists ? (int)(62 * factor) : 0), target.Health);
        Assert.Equal(10, target.VelocityZ.ToDouble());
        Assert.Equal(archvile.Id, target.LastDamageSourceId);
    }

    [Fact]
    public void DefaultFireInflictorDoesNotInheritAttackerSpectralFlag()
    {
        var sim = Room(); var target = sim.Players.Single(); target.Health = 1000;
        target.Spectral = true; target.NoPain = true;
        var archvile = sim.AddBot(0, 0, 64); archvile.Brain = null; archvile.Spectral = true;
        ArchvileActions.Attack(sim, archvile, target, fireExists: true);
        Assert.Equal(980, target.Health);
        Assert.Equal(10, target.VelocityZ.ToDouble());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 1, X = 300 }],
        Sectors = [new LevelSector { CeilingHeight = 512 }],
    }, rngSeed: 42);
}
