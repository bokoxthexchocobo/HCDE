using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FireSg2RandomTests
{
    [Fact]
    public void NamedSeedUsesNativeDamageYawPitchDrawOrder()
    {
        var sim = Room();
        foreach (var (damage, yaw, pitch) in new[] { (15, 168, -15), (5, 163, -23), (5, -164, 163) })
        {
            Assert.Equal(damage, sim.NextFireSg2Damage());
            Assert.Equal(yaw * (11.25 / 256), sim.NextFireSg2Spread(11.25));
            Assert.Equal(pitch * (7.097 / 256), sim.NextFireSg2Spread(7.097));
        }
    }

    [Fact]
    public void CombatAndBfgDrawsDoNotPerturbSuperShotgunPellets()
    {
        var sim = Room(); var control = Room();
        for (var i = 0; i < 31; i++) { sim.NextCombatRandom(); sim.NextBfgSprayDamageDie(); }
        var combat = sim.CombatRandomState; var bfg = sim.CaptureState().BfgSprayRandomState;
        Assert.Equal(Fire(control), Fire(sim));
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Equal(bfg, sim.CaptureState().BfgSprayRandomState);
        Assert.Equal(control.CaptureState().FireSg2RandomState, sim.CaptureState().FireSg2RandomState);
    }

    [Fact]
    public void FreshSaveContinuesPelletsAndLegacySaveResetsStream()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim, true); var first = Fire(sim);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(360, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true)); Assert.Equal(Fire(sim), Fire(fresh));
        SimSavegame.Apply(fresh, legacy); Assert.Null(fresh.CaptureState().FireSg2RandomState);
        Assert.Equal(first, Fire(fresh));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DryFireAndCooldownDoNotConsumeStream(bool cooldown)
    {
        var sim = Room(); var player = sim.Players.Single();
        if (cooldown) player.WeaponCooldown = 1; else player.Inventory.Shells = 0;
        Assert.False(HitscanCombat.Fire(sim, player)); Assert.Null(sim.CaptureState().FireSg2RandomState);
    }

    [Fact]
    public void InvalidArchivePredecessorIsRejected()
    {
        var sim = Room(); Fire(sim); var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 16), 360);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-firesg2random-header", error);
    }

    [Fact]
    public void MissingGeometryRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.FireSg2RandomState = 42; state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void StreamParticipatesInChecksumWithoutFiring()
    {
        var sim = Room(); var control = Room(); sim.NextFireSg2Damage();
        sim.Tick(); control.Tick(); Assert.NotEqual(control.Checksum, sim.Checksum);
    }

    private static int Fire(AuthoritySimulation sim)
    {
        var player = sim.Players.Single(); player.WeaponCooldown = 0;
        var target = sim.Actors.Single(a => a.ThingId == 7); var before = target.Health;
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.True(target.Health < before);
        return before - target.Health;
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 1);
        var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.SuperShotgun;
        player.Inventory.Selected = WeaponKind.SuperShotgun; player.Inventory.Shells = 40;
        var target = sim.AddBot(200, 0, 3001, thingId: 7); target.Brain = null;
        target.Health = 10000; target.Height = Fixed.FromInt(128); target.Radius = Fixed.FromInt(64);
        return sim;
    }
}
