using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WeaponRefireAccuracyTests
{
    [Theory]
    [InlineData(WeaponKind.Pistol, false)]
    [InlineData(WeaponKind.Pistol, true)]
    [InlineData(WeaponKind.Chaingun, false)]
    [InlineData(WeaponKind.Chaingun, true)]
    public void FirstShotIsAccurateAndHeldRefireConsumesSpread(WeaponKind weapon, bool vertical)
    {
        var sim = Room(weapon); var reference = Room(weapon); sim.VerticalWeaponSpread = vertical;
        Step(sim, true); reference.NextGunShotDamage();
        Assert.Equal(reference.CaptureState().GunShotRandomState, sim.CaptureState().GunShotRandomState);
        Assert.True(sim.Players.Single().WeaponRefiring);
        for (var i = 0; i < WeaponCatalog.Find(weapon)!.RefireTics; i++) Step(sim, true);
        reference.NextGunShotDamage(); reference.NextGunShotSpread(5.625);
        if (vertical) reference.NextGunShotSpread(3.549);
        Assert.Equal(reference.CaptureState().GunShotRandomState, sim.CaptureState().GunShotRandomState);
        Assert.Equal(48, sim.Players.Single().Inventory.Bullets);
    }

    [Theory]
    [InlineData(WeaponKind.Pistol)]
    [InlineData(WeaponKind.Chaingun)]
    public void ReleasedFireCycleResetsAccuracy(WeaponKind weapon)
    {
        var sim = Room(weapon); var reference = Room(weapon); Step(sim, true); reference.NextGunShotDamage();
        for (var i = 0; i < WeaponCatalog.Find(weapon)!.RefireTics; i++) Step(sim, false);
        Assert.False(sim.Players.Single().WeaponRefiring);
        Step(sim, true); reference.NextGunShotDamage();
        Assert.Equal(reference.CaptureState().GunShotRandomState, sim.CaptureState().GunShotRandomState);
    }

    [Fact]
    public void FreshSaveContinuesHeldRefireAndLegacySaveResetsTracking()
    {
        var sim = Room(WeaponKind.Pistol); var legacy = SimSavegame.Write(sim, true); Step(sim, true);
        var saved = SimSavegame.Write(sim, true); Assert.Equal(365, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var fresh = Room(WeaponKind.Pistol); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true)); Assert.True(fresh.Players.Single().WeaponRefiring);
        for (var i = 0; i < 19; i++) { Step(sim, true); Step(fresh, true); }
        Assert.Equal(sim.CaptureState().GunShotRandomState, fresh.CaptureState().GunShotRandomState);
        SimSavegame.Apply(fresh, legacy); Assert.False(fresh.Players.Single().WeaponRefiring);
    }

    [Fact]
    public void BringingUpAnotherWeaponResetsAccuracy()
    {
        var sim = Room(WeaponKind.Pistol); Step(sim, true); var player = sim.Players.Single();
        player.BringUpWeapon(WeaponKind.Pistol); Assert.False(player.WeaponRefiring);
    }

    [Fact]
    public void InvalidTrackingFlagRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(WeaponKind.Pistol); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.Actors[0].WeaponRefiringFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static void Step(AuthoritySimulation sim, bool attack)
    { sim.QueueCommand(0, new PlayerCommand { Attack = attack }); sim.Tick(); }
    private static AuthoritySimulation Room(WeaponKind weapon)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 1);
        var player = sim.Players.Single(); player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
        return sim;
    }
}
