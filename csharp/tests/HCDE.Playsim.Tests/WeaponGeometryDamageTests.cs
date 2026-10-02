using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponGeometryDamageTests
{
    private static AuthoritySimulation Room(int wallX = 48, int floor = 0, int ceiling = 128, bool twoSided = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1, FloorHeight = (short)floor,
            CeilingHeight = (short)ceiling, HealthFloor = 1000, HealthCeiling = 1000 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1, Index = 1 }],
        Lines = [new LevelLine { X1 = wallX, Y1 = 128, X2 = wallX, Y2 = -128,
            SideFront = 0, SideBack = twoSided ? 1 : -1, Health = 1000, HealthGroup = 4 },
            new LevelLine { Health = 1000, HealthGroup = 4 }],
        Things = [new LevelThing { Type = 1 }],
    });

    [Theory]
    [InlineData(WeaponKind.Pistol)]
    [InlineData(WeaponKind.Fist)]
    [InlineData(WeaponKind.Chainsaw)]
    [InlineData(WeaponKind.Shotgun)]
    [InlineData(WeaponKind.SuperShotgun)]
    [InlineData(WeaponKind.Chaingun)]
    public void EachHitscanWeaponDamagesWallsAndGroupsWithoutChangingRandomConsumption(WeaponKind weapon)
    {
        var wall = Room(); var open = Room(twoSided: true);
        foreach (var sim in new[] { wall, open })
        {
            var player = sim.Players.Single();
            player.Inventory.Weapons |= weapon;
            player.Inventory.Selected = weapon;
            player.Inventory.Shells = 20;
            Assert.True(HitscanCombat.Fire(sim, player));
        }
        Assert.InRange(wall.Level.Lines[0].Health, 1, 999);
        Assert.Equal(wall.Level.Lines[0].Health, wall.Level.Lines[1].Health);
        Assert.Equal(1000, open.Level.Lines[0].Health);
        Assert.Equal(wall.CombatRandomState, open.CombatRandomState);
    }

    [Theory]
    [InlineData(64, 128, true)]
    [InlineData(0, 20, false)]
    public void PistolHitsOppositeSectorWallPart(int floor, int ceiling, bool lower)
    {
        var sim = Room(floor: floor, ceiling: ceiling, twoSided: true);
        Assert.True(HitscanCombat.Fire(sim, sim.Players.Single()));
        var damage = 1000 - sim.Level.Lines[0].Health;
        Assert.InRange(damage, 5, 15);
        Assert.Equal(1000 - damage, lower ? sim.Level.Sectors[1].HealthFloor : sim.Level.Sectors[1].HealthCeiling);
        Assert.Equal(1000, lower ? sim.Level.Sectors[1].HealthCeiling : sim.Level.Sectors[1].HealthFloor);
    }

    [Fact]
    public void MeleeDoesNotReachDistantWall()
    {
        var sim = Room(wallX: 100); var player = sim.Players.Single();
        player.Inventory.Selected = WeaponKind.Fist;
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void ActorInFrontReceivesPistolDamageAndWallRemainsIntact()
    {
        var sim = Room(wallX: 100); var target = sim.AddBot(48, 0); var health = target.Health;
        Assert.True(HitscanCombat.Fire(sim, sim.Players.Single()));
        Assert.InRange(health - target.Health, 5, 15);
        Assert.Equal(1000, sim.Level.Lines[0].Health);
    }

    [Fact]
    public void CooldownPreventsRepeatedGeometryDamage()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(HitscanCombat.Fire(sim, player)); var health = sim.Level.Lines[0].Health;
        Assert.False(HitscanCombat.Fire(sim, player));
        Assert.Equal(health, sim.Level.Lines[0].Health);
        Assert.Equal(49, player.Inventory.Bullets);
    }
}
