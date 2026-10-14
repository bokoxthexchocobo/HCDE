using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChainsawRandomLightTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(63, 0)]
    [InlineData(64, 1)]
    [InlineData(159, 1)]
    [InlineData(160, 2)]
    [InlineData(255, 2)]
    public void HitLightUsesNativeThresholds(int random, int expected)
    {
        var (_, player) = Room(); player.ExtraLight = 10;
        ChainsawActions.ApplyRandomLight(player, random, true); Assert.Equal(expected, player.ExtraLight);
    }

    [Theory]
    [InlineData(64, 2, 2)]
    [InlineData(65, 2, 0)]
    [InlineData(255, 0, 1)]
    [InlineData(0, 0, 0)]
    public void MissLightTogglesOnlyAboveSixtyFour(int random, int initial, int expected)
    {
        var (_, player) = Room(); player.ExtraLight = initial;
        ChainsawActions.ApplyRandomLight(player, random, false); Assert.Equal(expected, player.ExtraLight);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AttackSelectsHitOrMissLightAndReplays(bool hit)
    {
        var (sim, player) = Room(); if (!hit) sim.Actors[1].X = Fixed.FromInt(500);
        var saved = SimSavegame.Write(sim, true);
        sim.NextSawSpread(); sim.NextSawSpread(); var random = sim.NextSawByte();
        var expectedLight = hit ? random < 64 ? 0 : random < 160 ? 1 : 2 : random > 64 ? 1 : 0;
        SimSavegame.Apply(sim, saved);
        void Attack() => ChainsawActions.Attack(player, 10, spread: 0,
            flags: ChainsawAttackFlags.NoRandom | ChainsawAttackFlags.RandomLightBoth);
        Attack(); Assert.Equal(expectedLight, player.ExtraLight); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Attack(); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ExtraLightRoundTripsAndLegacyRestoreClearsIt()
    {
        var (sim, player) = Room(); var legacy = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, legacy); var checksum = sim.Checksum;
        player.ExtraLight = 2; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
        var saved = SimSavegame.Write(sim, true); Assert.Equal(248, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        player.ExtraLight = 0; SimSavegame.Apply(sim, saved); Assert.Equal(2, player.ExtraLight);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.ExtraLight); Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void MalformedLightTrailerIsRejected()
    {
        var (sim, player) = Room(); player.ExtraLight = 1; var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-player-extralight-size", error);
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }]
        });
        sim.Actors[1].Brain!.Enabled = false; return (sim, sim.Players.Single());
    }
}
