using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ArchvileAttackSightTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void AttackFacesBeforeSightGateAndSuppressesDamageAndLaunchWhenBlocked(bool blocked, bool fireExists)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1, Y = 300 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Lines = blocked ? [new LevelLine { X1 = -100, X2 = 100, Y1 = 150, Y2 = 150, SideBack = -1 }] : [],
        }, rngSeed: 42);
        var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        target.VelocityZ = Fixed.FromInt(-3);
        var archvile = sim.AddBot(0, 0, 64); archvile.Brain = null;
        archvile.Angle = BamAngle.FromDegrees(180); archvile.PitchDegrees = 25; archvile.Ambush = true;
        ArchvileActions.Attack(sim, archvile, target, fireExists);
        Assert.Equal(BamAngle.FromDegrees(90), archvile.Angle);
        Assert.False(archvile.Ambush); Assert.Equal(25, archvile.PitchDegrees);
        Assert.Equal(blocked ? 1000 : fireExists ? 918 : 980, target.Health);
        Assert.Equal(blocked ? -3 : 10, target.VelocityZ.ToDouble());
        Assert.Equal(blocked ? (uint?)null : archvile.Id, target.LastDamageSourceId);
    }
}
