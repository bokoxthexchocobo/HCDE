using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GunShotRandomTests
{
    [Fact]
    public void NamedSeedMatchesNativeDamageAndLowByteSpreadVectors()
    {
        var sim = Room(0);
        foreach (var (damage, spread) in new[] { (15, 42), (10, -108), (5, 156) })
        {
            Assert.Equal(damage, sim.NextGunShotDamage());
            Assert.Equal(spread * (5.625 / 256), sim.NextGunShotSpread(5.625));
        }
    }

    [Theory]
    [InlineData(WeaponKind.Pistol)]
    [InlineData(WeaponKind.Shotgun)]
    [InlineData(WeaponKind.Chaingun)]
    public void CombatAndBfgDrawsDoNotPerturbShotgunPellets(WeaponKind weapon)
    {
        var sim = Room(); var control = Room();
        for (var i = 0; i < 31; i++) { sim.NextCombatRandom(); sim.NextBfgSprayDamageDie(); sim.NextFireSg2Damage(); }
        foreach (var world in new[] { sim, control })
        { var player = world.Players.Single(); player.Inventory.Weapons |= weapon; player.Inventory.Selected = weapon; }
        var combat = sim.CombatRandomState; var bfg = sim.CaptureState().BfgSprayRandomState;
        Assert.Equal(Fire(control), Fire(sim));
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Equal(bfg, sim.CaptureState().BfgSprayRandomState);
        Assert.Equal(control.CaptureState().GunShotRandomState, sim.CaptureState().GunShotRandomState);
    }

    [Fact]
    public void FreshSaveContinuesPelletsAndLegacySaveResetsStream()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim, true); var first = Fire(sim);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(361, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true)); Assert.Equal(Fire(sim), Fire(fresh));
        SimSavegame.Apply(fresh, legacy); Assert.Null(fresh.CaptureState().GunShotRandomState);
        Assert.Equal(first, Fire(fresh));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DryFireAndCooldownDoNotConsumeStream(bool cooldown)
    {
        var sim = Room(); var player = sim.Players.Single();
        if (cooldown) player.WeaponCooldown = 1; else player.Inventory.Shells = 0;
        Assert.False(HitscanCombat.Fire(sim, player)); Assert.Null(sim.CaptureState().GunShotRandomState);
    }

    [Fact]
    public void InvalidArchivePredecessorIsRejected()
    {
        var sim = Room(); Fire(sim); var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 16), 361);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-gunshotrandom-header", error);
    }

    [Fact]
    public void MissingGeometryRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.GunShotRandomState = 42; state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void StreamParticipatesInChecksumWithoutFiring()
    {
        var sim = Room(); var control = Room(); sim.NextGunShotDamage();
        sim.Tick(); control.Tick(); Assert.NotEqual(control.Checksum, sim.Checksum);
    }

    private static int Fire(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.WeaponCooldown = 0;
        var target = sim.Actors.Single(a => a.ThingId == 7); var before = target.Health;
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.True(target.Health < before);
        return before - target.Health;
    }

    private static AuthoritySimulation Room(int seed = 1)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: seed);
        var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun; player.Inventory.Shells = 40;
        var target = sim.AddBot(200, 0, 3001, thingId: 7); target.Brain = null;
        target.Health = 10000; target.Height = Fixed.FromInt(128); target.Radius = Fixed.FromInt(64);
        return sim;
    }
}
