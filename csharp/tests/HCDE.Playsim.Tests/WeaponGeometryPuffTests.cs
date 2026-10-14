using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponGeometryPuffTests
{
    [Theory]
    [InlineData(WeaponKind.Pistol, 1)]
    [InlineData(WeaponKind.Chaingun, 1)]
    [InlineData(WeaponKind.Shotgun, 7)]
    [InlineData(WeaponKind.SuperShotgun, 20)]
    [InlineData(WeaponKind.Fist, 1)]
    [InlineData(WeaponKind.Chainsaw, 1)]
    public void WallImpactSpawnsPuffPerPelletWithNativeState(WeaponKind weapon, int count)
    {
        var sim = Room(wallX: 48); var player = Equip(sim, weapon);
        Assert.True(HitscanCombat.Fire(sim, player));
        var puffs = sim.Actors.OfType<PuffActor>().ToArray(); Assert.Equal(count, puffs.Length);
        foreach (var puff in puffs)
        {
            Assert.Same(player, puff.DamageSource); Assert.Equal(1, puff.Radius.Raw);
            Assert.InRange(puff.X.ToDouble(), 43.9, 44.2);
            Assert.Equal(weapon == WeaponKind.Fist ? 2 : 0, puff.States.Current);
            Assert.Equal(weapon != WeaponKind.Fist, puff.FullBright);
            Assert.InRange(puff.States.RemainingTics, 1, 4);
        }
        Assert.Equal(WeaponCatalog.Find(weapon)!.RefireTics, player.WeaponCooldown);
        Assert.False(HitscanCombat.Fire(sim, player)); Assert.Equal(count, sim.Actors.OfType<PuffActor>().Count());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PistolPlaneImpactUsesAimDirectionAndNativeJitter(bool ceiling)
    {
        var sim = Room(wallX: 1000);
        var player = Equip(sim, WeaponKind.Pistol); player.PitchDegrees = ceiling ? -45 : 45;
        var pitch = BamAngle.FromDegrees(player.PitchDegrees);
        var hit = CombatTrace.TraceLineAttack(sim, player, player.Angle, pitch, HitscanCombat.HitscanRange);
        Assert.True(hit.Hit); Assert.Null(hit.Wall);
        Assert.True(HitscanCombat.Fire(sim, player));
        var puff = sim.Actors.OfType<PuffActor>().Single();
        Assert.Equal(hit.X - 4 / Math.Sqrt(2), puff.X.ToDouble(), 4);
        Assert.Equal(hit.Z + (ceiling ? -1 : 1) * 4 / Math.Sqrt(2) + 15 / 64.0, puff.Z.ToDouble(), 4);
        Assert.Equal(BamAngle.FromDegrees(180), puff.Angle);
    }

    [Theory]
    [InlineData(WeaponKind.Pistol)]
    [InlineData(WeaponKind.Shotgun)]
    [InlineData(WeaponKind.Fist)]
    public void HorizonDoesNotSpawnPuffsOrAdvanceTheirStream(WeaponKind weapon)
    {
        var sim = Room(wallX: 48); sim.Level.Lines[0].Special = 9;
        Assert.True(HitscanCombat.Fire(sim, Equip(sim, weapon)));
        Assert.Empty(sim.Actors.OfType<PuffActor>()); Assert.Null(sim.CaptureState().PuffRandomState);
    }

    [Theory]
    [InlineData(WeaponKind.Pistol)]
    [InlineData(WeaponKind.Fist)]
    public void OpenSpaceDoesNotSpawnPuffs(WeaponKind weapon)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
        });
        Assert.True(HitscanCombat.Fire(sim, Equip(sim, weapon)));
        Assert.Empty(sim.Actors.OfType<PuffActor>()); Assert.Null(sim.CaptureState().PuffRandomState);
    }

    [Theory]
    [InlineData(WeaponKind.Shotgun)]
    [InlineData(WeaponKind.Fist)]
    public void WeaponPuffsAndExpirationReplayAfterRewind(WeaponKind weapon)
    {
        var sim = Room(wallX: 48); var player = Equip(sim, weapon); var before = SimSavegame.Write(sim, true);
        Assert.True(HitscanCombat.Fire(sim, player));
        Assert.NotEmpty(sim.Actors.OfType<PuffActor>());
        var fired = SimSavegame.Write(sim, true); var expected = Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, before); Assert.True(HitscanCombat.Fire(sim, player)); Assert.Equal(fired, SimSavegame.Write(sim, true));
        Assert.Equal(expected, Enumerable.Range(0, 20).Select(_ => Step(sim)).ToArray());
        Assert.Empty(sim.Actors.OfType<PuffActor>());
    }

    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static PlayerPawn Equip(AuthoritySimulation sim, WeaponKind weapon)
    {
        var player = sim.Players.Single(); player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
        player.Inventory.Shells = 100; player.Inventory.Bullets = 100;
        return player;
    }
    private static AuthoritySimulation Room(int wallX = 64) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = wallX, X2 = wallX, Y1 = 1000, Y2 = -1000, SideFront = 0, SideBack = -1 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
