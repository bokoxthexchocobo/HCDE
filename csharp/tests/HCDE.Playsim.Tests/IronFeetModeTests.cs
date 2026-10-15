using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IronFeetModeTests
{
    [Theory]
    [InlineData(IronFeetMode.Normal, 0, 100, true)]
    [InlineData(IronFeetMode.Normal, 256, 95, true)]
    [InlineData(IronFeetMode.Full, 0, 100, false)]
    [InlineData(IronFeetMode.Full, 256, 100, false)]
    [InlineData(IronFeetMode.None, 0, 95, false)]
    [InlineData(IronFeetMode.None, 256, 95, false)]
    public void SectorProtectionAndRandomDrawFollowMode(IronFeetMode mode, int leak, int health, bool draws)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(); player.PowerIronFeetMode = mode;
        sim.Level.Sectors[0].Leakiness = (short)leak; SectorDamage.Tick(sim, player);
        Assert.Equal(health, player.Health); Assert.Equal(draws, sim.SectorDamageRandomState.HasValue);
    }

    [Theory]
    [InlineData(IronFeetMode.Full)]
    [InlineData(IronFeetMode.None)]
    public void ModeDoesNotChangeTerrainOrDrowningAbsorption(IronFeetMode mode)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(); player.PowerIronFeetMode = mode;
        TerrainDamage.Tick(sim, player); ActorDamage.Apply(player, 5, damageType: "Drowning");
        Assert.Equal(100, player.Health);
    }

    [Fact]
    public void FullProtectionStillEndsOrdinaryGodMode()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet();
        player.PowerIronFeetMode = IronFeetMode.Full; player.GodMode = true;
        sim.Level.Sectors[0].DamageEndsGodMode = true; SectorDamage.Tick(sim, player);
        Assert.False(player.GodMode); Assert.Equal(100, player.Health); Assert.Null(sim.SectorDamageRandomState);
    }

    [Fact]
    public void FullModeWithoutActiveSuitDoesNotProtect()
    {
        var sim = Room(); var player = sim.Players.Single(); player.PowerIronFeetMode = IronFeetMode.Full;
        SectorDamage.Tick(sim, player); Assert.Equal(95, player.Health);
    }

    [Theory]
    [InlineData(IronFeetMode.Full)]
    [InlineData(IronFeetMode.None)]
    public void ModeRoundTripsWithChecksumAndOldSaveResetsToNormal(IronFeetMode mode)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet();
        var old = SimSavegame.Write(sim, true); sim.RestoreState(sim.CaptureState()); var normalChecksum = sim.Checksum;
        player.PowerIronFeetMode = mode; sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(normalChecksum, sim.Checksum); var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        Assert.Equal(298, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        SimSavegame.Apply(sim, old); Assert.Equal(IronFeetMode.Normal, player.PowerIronFeetMode);
        SimSavegame.Apply(sim, saved); Assert.Equal(mode, player.PowerIronFeetMode); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void InvalidModeRejectsStructuredAndBinaryArchives(int mode)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].PowerIronFeetMode = mode;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        sim.Players.Single().PowerIronFeetMode = IronFeetMode.Full; var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), mode);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-ironfeetmode-state", error);
    }

    [Fact]
    public void NonDefaultModeRequiresGeometryBeforeMutation()
    {
        var sim = Room(); var state = sim.CaptureState(); state.GeometryHealth = null; state.Actors[0].PowerIronFeetMode = 1;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(IronFeetMode.Normal, sim.Players.Single().PowerIronFeetMode);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new LevelTerrainDefinition("Lava", DamageAmount: 5, DamageTimeMask: 0, AllowProtection: true)],
        DefaultTerrain = "Lava", Sectors = [new LevelSector { CeilingHeight = 128, DamageAmount = 5,
            DamageInterval = 1, Leakiness = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
