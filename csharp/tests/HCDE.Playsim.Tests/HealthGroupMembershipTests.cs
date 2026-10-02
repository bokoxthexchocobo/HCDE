using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthGroupMembershipTests
{
    private static AuthoritySimulation Room(bool floorMember, bool lineMember = true) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Lines = lineMember ? [new LevelLine { Tag = 7, Health = 100, HealthGroup = 4 }] : [],
        Sectors = [new LevelSector { Tag = 9, CeilingHeight = 128, HealthFloor = 80,
            HealthFloorGroup = floorMember ? 4 : 0, Health3D = 200, Health3DGroup = 4 }],
    });

    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(false, true, 100)]
    [InlineData(true, false, 80)]
    [InlineData(true, true, 100)]
    public void ThreeDHealthDoesNotCreateGroupOrRaiseInitialMaximum(bool floorMember, bool lineMember, int expected)
    {
        var sim = Room(floorMember, lineMember);
        Assert.Equal(expected != 0, sim.HealthGroups.ContainsKey(4));
        if (expected != 0) Assert.Equal(expected, sim.HealthGroups[4]);
        Assert.Equal(expected == 0 ? 200 : expected, AcsDestructibleHealth.GetSectorHealth(sim, 9, 2));
        // Native startup computes pooled maximum without rewriting local health.
        Assert.Equal(80, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(200, sim.Level.Sectors[0].Health3D);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LineSetterUpdatesThreeDOnlyOnSectorRegisteredInGroup(bool floorMember)
    {
        var sim = Room(floorMember);
        Assert.True(GeometryHealthActions.Execute(sim, 150, 7, 30, 0));
        Assert.Equal(floorMember ? 30 : 200, sim.Level.Sectors[0].Health3D);
        Assert.Equal(floorMember ? 30 : 80, sim.Level.Sectors[0].HealthFloor);
        Assert.Equal(30, AcsDestructibleHealth.GetSectorHealth(sim, 9, 2));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ThreeDSetterUpdatesExistingGroupEvenWithoutSectorMembership(bool floorMember)
    {
        var sim = Room(floorMember);
        Assert.True(GeometryHealthActions.Execute(sim, 151, 9, 2, 30));
        Assert.Equal(30, sim.Level.Sectors[0].Health3D);
        Assert.Equal(30, sim.Level.Lines[0].Health);
        Assert.Equal(floorMember ? 30 : 80, sim.Level.Sectors[0].HealthFloor);
    }

    [Fact]
    public void ThreeDOnlyGroupRemainsLocalThroughSetterAndDamage()
    {
        var sim = Room(false, false);
        GeometryHealthActions.Execute(sim, 151, 9, 2, 30);
        LevelDestructibleDamage.DamageSectorPart(sim, sim.Level.Sectors[0], 2, 10);
        Assert.Empty(sim.HealthGroups);
        Assert.Equal(20, AcsDestructibleHealth.GetSectorHealth(sim, 9, 2));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MembershipBehaviorSurvivesSaveAndDamageContinuation(bool floorMember)
    {
        var original = Room(floorMember);
        GeometryHealthActions.Execute(original, 150, 7, 30, 0);
        original.Tick();
        var restored = Room(floorMember);
        SimSavegame.Apply(restored, SimSavegame.Write(original));
        Assert.Equal(original.Checksum, restored.Checksum);
        foreach (var sim in new[] { original, restored })
        {
            LevelDestructibleDamage.DamageLine(sim, sim.Level.Lines[0], 10);
            sim.Tick();
        }
        Assert.Equal(floorMember ? 20 : 200, restored.Level.Sectors[0].Health3D);
        Assert.Equal(original.Checksum, restored.Checksum);
    }
}
