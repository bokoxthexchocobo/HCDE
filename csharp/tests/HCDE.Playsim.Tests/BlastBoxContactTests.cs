using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlastBoxContactTests
{
    [Theory]
    [InlineData(38, 38, 4, 0, true)]
    [InlineData(30, 0, -4, 0, true)]
    [InlineData(30, 0, 0, 4, true)]
    [InlineData(30, 40, 4, 0, false)]
    public void EligibleBlastUsesStrictDestinationBox(int x, int y, int vx, int vy, bool hits)
    {
        var (sim, mover, target) = Room(x, y);
        mover.Invulnerable = target.Invulnerable = true;
        mover.VelocityX = Fixed.FromInt(vx); mover.VelocityY = Fixed.FromInt(vy);
        Assert.Equal(!hits, ActorPhysics.TryMove(sim, mover, vx, vy, out _));
        Assert.Equal(hits ? vx : 0, target.VelocityX.ToDouble());
        Assert.Equal(hits ? vy : 0, target.VelocityY.ToDouble());
        Assert.Equal(hits ? 0 : vx, mover.X.ToDouble()); Assert.Equal(hits ? 0 : vy, mover.Y.ToDouble());
        Assert.Equal(1000, mover.Health); Assert.Equal(1000, target.Health);
    }

    [Theory]
    [InlineData("boss")]
    [InlineData("dontblast")]
    [InlineData("notmonster")]
    [InlineData("thruactors")]
    [InlineData("noblockmap")]
    public void CornerContactRetainsBlastEligibilityExclusions(string exclusion)
    {
        var (sim, mover, target) = Room(38, 38);
        if (exclusion == "boss") target.Boss = true;
        if (exclusion == "dontblast") target.DontBlast = true;
        if (exclusion == "notmonster") target.IsMonster = false;
        if (exclusion == "thruactors") mover.ThruActors = true;
        if (exclusion == "noblockmap") target.NoBlockmap = true;
        Assert.True(ActorPhysics.TryMove(sim, mover, 4, 0, out _));
        Assert.Equal(0, target.VelocityX.Raw); Assert.Equal(1000, target.Health); Assert.Equal(1000, mover.Health);
    }

    [Fact]
    public void CornerBlastDamageAndMomentumReplayAfterSaveRestore()
    {
        var (sim, mover, target) = Room(38, 38);
        var saved = SimSavegame.Write(sim, true);
        var (restored, loaded, hit) = Room(38, 38); SimSavegame.Apply(restored, saved);
        Assert.False(ActorPhysics.TryMove(sim, mover, 4, 0, out _));
        Assert.False(ActorPhysics.TryMove(restored, loaded, 4, 0, out _));
        Assert.Equal(997, hit.Health); Assert.Equal(999, loaded.Health);
        Assert.Equal(target.VelocityX, hit.VelocityX); Assert.Equal(target.VelocityY, hit.VelocityY);
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState());
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room(int x, int y)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var mover = sim.AddBot(0, 0); var target = sim.AddBot(x, y);
        mover.Brain = target.Brain = null; mover.Health = target.Health = 1000;
        mover.DoHarmSpecies = target.DoHarmSpecies = true; mover.PainChance = target.PainChance = 0;
        mover.Mass = 200; target.Mass = 300; mover.Blasted = true; mover.VelocityX = Fixed.FromInt(4);
        mover.Solid = target.Solid = false;
        return (sim, mover, target);
    }
}
