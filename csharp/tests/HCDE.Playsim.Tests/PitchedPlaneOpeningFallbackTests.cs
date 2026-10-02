using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PitchedPlaneOpeningFallbackTests
{
    [Theory]
    [InlineData(false, false, false, 0)]
    [InlineData(false, false, true, 0)]
    [InlineData(false, true, false, 0)]
    [InlineData(false, true, true, 0)]
    [InlineData(true, false, false, 0)]
    [InlineData(true, false, true, 0)]
    [InlineData(true, true, false, 0)]
    [InlineData(true, true, true, 0)]
    [InlineData(false, false, false, LevelLine.BlockHitscanFlag)]
    [InlineData(false, false, true, LevelLine.BlockHitscanFlag)]
    [InlineData(false, true, false, LevelLine.BlockHitscanFlag)]
    [InlineData(false, true, true, LevelLine.BlockHitscanFlag)]
    [InlineData(true, false, false, LevelLine.BlockHitscanFlag)]
    [InlineData(true, false, true, LevelLine.BlockHitscanFlag)]
    [InlineData(true, true, false, LevelLine.BlockHitscanFlag)]
    [InlineData(true, true, true, LevelLine.BlockHitscanFlag)]
    public void PitchedOriginOnNearPlaneUsesFarOpening(bool reverse, bool ceiling, bool closed, int flags)
    {
        var near = new LevelSector { FloorHeight = ceiling ? -256 : 28, CeilingHeight = ceiling ? 28 : 256 };
        var far = new LevelSector { FloorHeight = !ceiling && closed ? 0 : -256,
            CeilingHeight = ceiling && closed ? 64 : 256 };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = reverse ? [far, near] : [near, far],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = [new LevelLine { X1 = 64, X2 = 64, Y1 = 128, Y2 = -128,
                SideFront = 0, SideBack = 1, Flags = flags, Health = 100 }],
            Things = [new LevelThing { Type = 1, X = reverse ? 128 : 0, Angle = reverse ? 180 : 0 }],
        });
        var source = sim.Players.Single(); source.Z = Fixed.FromInt(0);
        var target = sim.AddBot(reverse ? 0 : 128, 0);
        target.Z = Fixed.FromInt(ceiling ? 120 : -112); target.Height = Fixed.FromInt(56);
        target.Radius = Fixed.FromInt(16);
        var pitch = BamAngle.FromDegrees(ceiling ? -45 : 45);
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, pitch, 256);
        Assert.Equal(closed ? null : target, hit.Victim);
        Assert.Equal(closed ? sim.Level.Lines[0] : null, hit.Wall);
        Assert.Equal(-1, hit.PlaneSector);
        Assert.Equal(closed ? null : target, CombatTrace.PickActor(sim, source, source.Angle, pitch, 256));
        var health = target.Health;
        AcsLineAttack.Attack(sim, source, unchecked((int)source.Angle.Raw), unchecked((int)pitch.Raw),
            7, "None", 256, absoluteAngles: true);
        Assert.Equal(closed ? health : health - 7, target.Health);
        Assert.Equal(closed ? 93 : 100, sim.Level.Lines[0].Health);
    }
}
