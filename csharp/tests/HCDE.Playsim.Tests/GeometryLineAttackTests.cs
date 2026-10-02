using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GeometryLineAttackTests
{
    private static AuthoritySimulation Room(int floor = 0, int ceiling = 128, bool twoSided = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1, FloorHeight = (short)floor,
            CeilingHeight = (short)ceiling, HealthFloor = 100, HealthCeiling = 100 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }],
        Lines = [new LevelLine { X1 = 64, Y1 = 64, X2 = 64, Y2 = -64, SideFront = 0,
            SideBack = twoSided ? 1 : -1, Health = 100, HealthGroup = 4 },
            new LevelLine { Health = 100, HealthGroup = 4 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(30, 70)]
    [InlineData(200, 0)]
    [InlineData(0, 100)]
    [InlineData(-30, 100)]
    public void AcsAttackDamagesBlockingLineAndLinkedGroup(int damage, int expected)
    {
        var sim = Room(); var source = sim.Players.Single();
        Assert.Equal(0, AcsLineAttack.Attack(sim, source, 0, 0, damage, "None", 128));
        Assert.Equal(expected, sim.Level.Lines[0].Health);
        Assert.Equal(expected, sim.Level.Lines[1].Health);
        Assert.Equal(expected, sim.HealthGroups[4]);
    }

    [Theory]
    [InlineData(64, 128, 70, 100)]
    [InlineData(0, 20, 100, 70)]
    public void LowerAndUpperWallHitsDamageOppositeSectorPart(int floor, int ceiling, int floorHealth, int ceilingHealth)
    {
        var sim = Room(floor, ceiling, true);
        AcsLineAttack.Attack(sim, sim.Players.Single(), 0, 0, 30, "None", 128);
        Assert.Equal(floorHealth, sim.Level.Sectors[1].HealthFloor);
        Assert.Equal(ceilingHealth, sim.Level.Sectors[1].HealthCeiling);
        Assert.Equal(70, sim.Level.Lines[0].Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HorizonAndMissingImpactSideAreInvulnerable(bool missingSide)
    {
        var sim = Room();
        if (missingSide)
        {
            var hit = new CombatTrace.LineAttackHit(true, null, 64, 0, 28) { Wall = sim.Level.Lines[0], Side = 1 };
            GeometryLineAttack.Apply(sim, hit, 30);
        }
        else
        {
            sim.Level.Lines[0].Special = 9;
            AcsLineAttack.Attack(sim, sim.Players.Single(), 0, 0, 30, "None", 128);
        }
        Assert.Equal(100, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void OpenTwoSidedLineAndOutOfRangeWallTakeNoDamage()
    {
        var sim = Room(twoSided: true);
        AcsLineAttack.Attack(sim, sim.Players.Single(), 0, 0, 30, "None", 128);
        Assert.Equal(100, sim.Level.Lines[0].Health);
        sim = Room();
        AcsLineAttack.Attack(sim, sim.Players.Single(), 0, 0, 30, "None", 32);
        Assert.Equal(100, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void TraceReportsWallSideAndRemainsReadOnly()
    {
        var sim = Room(); var source = sim.Players.Single();
        var hit = CombatTrace.TraceLineAttack(sim, source, source.Angle, new BamAngle(0), 128);
        Assert.Same(sim.Level.Lines[0], hit.Wall);
        Assert.Equal(0, hit.Side);
        Assert.Equal(100, sim.Level.Lines[0].Health);
        source.X = Fixed.FromInt(128);
        hit = CombatTrace.TraceLineAttack(sim, source, new BamAngle(0x80000000), new BamAngle(0), 128);
        Assert.Same(sim.Level.Lines[0], hit.Wall);
        Assert.Equal(0, hit.Side);
    }

    [Fact]
    public void ActorBeforeWallReceivesDamageWithoutDamagingGeometry()
    {
        var sim = Room(); var target = sim.AddBot(32, 0); var before = target.Health;
        Assert.Equal(1, AcsLineAttack.Attack(sim, sim.Players.Single(), 0, 0, 30, "None", 128));
        Assert.Equal(before - 30, target.Health);
        Assert.Equal(100, sim.Level.Lines[0].Health);
    }
}
