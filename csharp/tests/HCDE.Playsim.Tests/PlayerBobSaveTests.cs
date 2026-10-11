using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerBobSaveTests
{
    [Fact]
    public void SavedBobRestoresExactlyAndContinuesLikeUninterruptedSimulation()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.BobTimer = 17; player.ViewBobOffset = -2.25; player.MovementBob = 5;
        player.WeaponBobX = 1.25; player.WeaponBobY = 0.75;
        var saved = SimSavegame.Write(sim); Assert.Equal(168, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var control = Room(); SimSavegame.Apply(control, saved);
        player.BobTimer = 0; player.ViewBobOffset = 0; player.MovementBob = 0; player.WeaponBobX = 0; player.WeaponBobY = 0;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(17, player.BobTimer); Assert.Equal(-2.25, player.ViewBobOffset); Assert.Equal(5, player.MovementBob);
        Assert.Equal(1.25, player.WeaponBobX); Assert.Equal(0.75, player.WeaponBobY);
        Assert.Equal(control.Checksum, sim.Checksum);
        sim.Tick(); control.Tick(); Assert.Equal(control.Checksum, sim.Checksum); Assert.Equal(18, player.BobTimer);
    }

    [Theory]
    [InlineData(36)]
    [InlineData(44)]
    [InlineData(52)]
    [InlineData(60)]
    public void NonfiniteBobValuesRejectBeforeMutation(int offset)
    {
        var sim = Room(); var player = sim.Players.Single(); player.BobTimer = 17;
        var saved = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt64LittleEndian(saved.AsSpan(saved.Length - size + offset), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved)); Assert.Equal(17, player.BobTimer);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
