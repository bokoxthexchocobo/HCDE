using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnSoundSourceFlagTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AutomaticLaunchRoutesFlaggedMissileToOwner(bool enabled)
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket, spawnSoundSource: enabled);
        var request = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal(enabled, missile.SpawnSoundSource);
        Assert.Equal(enabled ? owner.Id : missile.Id, request.ActorId);
        Assert.Equal(enabled ? 1 : 2, request.Channel);
    }

    [Fact]
    public void ScriptFlagPersistsThroughProjectileReconstructionAndControlsDefaultAction()
    {
        var sim = Room(); var owner = sim.AddBot(-200, 0);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket); sim.DrainSoundRequests();
        Assert.True(AcsActorFlags.TrySet(missile, "Actor.SPAWNSOUNDSOURCE", true));
        Assert.True(AcsActorFlags.TryGet(missile, "spawnsoundsource", out var enabled)); Assert.True(enabled);
        sim.RestoreState(sim.CaptureState()); var bytes = SimSavegame.Write(sim); var checksum = sim.Checksum;
        Assert.Equal(188, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        missile.Destroy(); SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.OfType<ProjectileActor>().Single(a => a.Id == missile.Id);
        Assert.True(restored.SpawnSoundSource); Assert.Empty(sim.DrainSoundRequests());
        Assert.Equal(bytes, SimSavegame.Write(sim)); Assert.Equal(checksum, sim.Checksum);
        ActorSoundActions.PlaySpawnSound(restored, owner);
        var sound = Assert.Single(sim.DrainSoundRequests()); Assert.Equal(owner.Id, sound.ActorId); Assert.Equal(1, sound.Channel);
        ActorSoundActions.PlaySpawnSound(restored);
        Assert.NotNull(Assert.Single(sim.DrainSoundRequests()).DetachedOrigin);
    }

    [Fact]
    public void LegacySaveRestoresFalseAndFlagChangesChecksum()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); sim.RestoreState(sim.CaptureState()); var bytes = SimSavegame.Write(sim); var checksum = sim.Checksum;
        actor.SpawnSoundSource = true; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, bytes); Assert.False(actor.SpawnSoundSource);
        Assert.Equal(checksum, sim.Checksum); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MalformedTrailerIsRejected(int mutation)
    {
        var sim = Room(); sim.AddBot(0, 0).SpawnSoundSource = true;
        var bytes = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        if (mutation == 0) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8), 2);
        if (mutation == 1) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), 188);
        if (mutation == 2) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), int.MaxValue);
        if (mutation == 3) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.StartsWith("save-spawn-sound-source-", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
