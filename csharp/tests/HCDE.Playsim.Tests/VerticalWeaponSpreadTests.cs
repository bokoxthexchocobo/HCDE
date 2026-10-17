using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VerticalWeaponSpreadTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OptionalShotgunSpreadCanHitAboveLevelAim(bool enabled)
    {
        var sim = Room(); sim.VerticalWeaponSpread = enabled;
        Assert.True(HitscanCombat.Fire(sim, sim.Players.Single()));
        Assert.Equal(enabled, Target(sim).Health < 1000);
    }

    [Theory]
    [InlineData(WeaponKind.Pistol)]
    [InlineData(WeaponKind.SuperShotgun)]
    public void AccuratePistolAndSuperShotgunKeepTheirExistingSequences(WeaponKind weapon)
    {
        var sim = Room(); var control = Room(); sim.VerticalWeaponSpread = true;
        foreach (var world in new[] { sim, control })
        {
            var player = world.Players.Single(); player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon;
            Assert.True(HitscanCombat.Fire(world, player));
        }
        Assert.Equal(Target(control).Health, Target(sim).Health);
        Assert.Equal(control.CaptureState().GunShotRandomState, sim.CaptureState().GunShotRandomState);
        Assert.Equal(control.CaptureState().FireSg2RandomState, sim.CaptureState().FireSg2RandomState);
    }

    [Fact]
    public void FreshSavePreservesSettingAndFiringWhileLegacyResetsIt()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim, true); sim.VerticalWeaponSpread = true;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(362, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.True(fresh.VerticalWeaponSpread); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.True(HitscanCombat.Fire(sim, sim.Players.Single()));
        Assert.True(HitscanCombat.Fire(fresh, fresh.Players.Single()));
        Assert.Equal(Target(sim).Health, Target(fresh).Health);
        Assert.Equal(sim.CaptureState().GunShotRandomState, fresh.CaptureState().GunShotRandomState);
        SimSavegame.Apply(fresh, legacy); Assert.False(fresh.VerticalWeaponSpread);
    }

    [Fact]
    public void OptionParticipatesInChecksum()
    {
        var sim = Room(); var control = Room(); sim.VerticalWeaponSpread = true;
        sim.Tick(); control.Tick(); Assert.NotEqual(control.Checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedArchiveHeaderIsRejected(bool predecessor)
    {
        var sim = Room(); sim.VerticalWeaponSpread = true; var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - (predecessor ? 12 : 8)), predecessor ? 362 : 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-verticalspread-header", error);
    }

    [Fact]
    public void MissingGeometryRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.VerticalWeaponSpread = true; state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static Actor Target(AuthoritySimulation sim) => sim.Actors.Single(a => a.ThingId == 7);
    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 42);
        var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun; player.Inventory.Shells = 20;
        var target = sim.AddBot(500, 0, 3001, thingId: 7); target.Brain = null; target.Health = 1000;
        target.Radius = Fixed.FromInt(150); target.Z = Fixed.FromInt(37); target.Height = Fixed.FromInt(40);
        return sim;
    }
}
