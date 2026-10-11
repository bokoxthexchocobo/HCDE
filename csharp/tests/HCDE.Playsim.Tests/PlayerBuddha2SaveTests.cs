using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerBuddha2SaveTests
{
    [Theory]
    [InlineData(200, DamageFlags.None)]
    [InlineData(200, DamageFlags.Forced)]
    [InlineData(ActorDamage.TelefragDamage, DamageFlags.Forced)]
    public void SavedUltimateBuddhaStillSurvivesLethalDamage(int damage, DamageFlags flags)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Buddha2 = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(155, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        player.Buddha2 = false; SimSavegame.Apply(sim, bytes);
        Assert.True(player.Buddha2); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.False(ActorDamage.Apply(player, damage, flags: flags).Killed);
        Assert.Equal(1, player.Health);
    }

    [Fact]
    public void SnapshotRestoresUltimateBuddhaAndLegacySaveResetsIt()
    {
        var sim = Room(); var player = sim.Players.Single();
        var legacy = SimSavegame.Write(sim);
        player.Buddha2 = true;
        var snapshot = sim.CaptureState();
        player.Buddha2 = false; sim.RestoreState(snapshot);
        Assert.True(player.Buddha2);
        SimSavegame.Apply(sim, legacy); Assert.False(player.Buddha2);
        Assert.True(ActorDamage.Apply(player, 200).Killed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArchiveRejectsMalformedFlagAndPriorVersion(bool invalidPrior)
    {
        var sim = Room(); sim.Players.Single().Buddha2 = true;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (invalidPrior) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 155);
        else bytes[start + 8] = 2;
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-player-buddha2-invalid", error);
        Assert.True(sim.Players.Single().Buddha2);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
