using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RelativeLineAttackPitchTests
{
    [Theory]
    [InlineData(90, 0, false, 0)]
    [InlineData(-90, 0, false, 1)]
    [InlineData(450, 0, false, 0)]
    [InlineData(-450, 0, false, 1)]
    [InlineData(180, 90, false, 1)]
    [InlineData(450, -90, false, -1)]
    [InlineData(90, -90, true, 1)]
    [InlineData(-90, 90, true, 0)]
    public void RelativePitchUsesOrdinaryActorWhileAbsolutePitchUsesArgument(
        int sourcePitch, int offsetPitch, bool absolute, int part)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, HealthFloor = 100, HealthCeiling = 100 }],
            Things = [new LevelThing { Type = 2035, Pitch = (short)sourcePitch }],
        });
        var source = Assert.Single(sim.Actors);
        var rng = sim.CombatRandomState;
        var result = AcsLineAttack.Attack(sim, source, 0, unchecked((int)BamAngle.FromDegrees(offsetPitch).Raw),
            5, "None", 256, 19, absolute);
        Assert.Equal(0, result);
        Assert.Equal(part == 0 ? 95 : 100, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(part == 1 ? 95 : 100, sim.Level.Sectors[0].HealthCeiling);
        Assert.Equal(part >= 0 ? 1 : 0, sim.Actors.Count(actor => actor.ThingId == 19));
        Assert.Equal(sourcePitch, source.PitchDegrees);
        Assert.Equal(rng, sim.CombatRandomState);
    }
}
