using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ForcedFallingDamageTests
{
    public static IEnumerable<object[]> Policies()
    {
        for (var map = 0; map < 4; map++)
            for (var forced = 0; forced < 4; forced++)
                yield return [(FallingDamageStyle)map, (FallingDamageStyle)forced, (FallingDamageStyle)(map | forced)];
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void RuntimeAndMapPoliciesUseNativeBitwiseCombination(FallingDamageStyle map,
        FallingDamageStyle forced, FallingDamageStyle expected)
    {
        var sim = Room(map); sim.ForcedFallingDamage = forced;
        Assert.Equal(expected, sim.EffectiveFallingDamage);
        var player = sim.Players.Single(); player.VelocityZ = Fixed.FromInt(-24); FallingDamage.Apply(player);
        var health = expected switch
        { FallingDamageStyle.None => 100, FallingDamageStyle.ZDoom => 91, FallingDamageStyle.Hexen => 97, _ => 38 };
        Assert.Equal(health, player.Health);
    }

    [Fact]
    public void RuntimeStyleChecksumSaveReplayAndOlderResetWork()
    {
        var sim = Room(); sim.RestoreState(sim.CaptureState()); var baseline = sim.Checksum;
        var older = SimSavegame.Write(sim, true); sim.ForcedFallingDamage = FallingDamageStyle.Hexen;
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
        var player = sim.Players.Single(); player.Z = Fixed.FromInt(1); player.VelocityZ = Fixed.FromInt(-24); player.OnGround = false;
        var saved = SimSavegame.Write(sim, true); Assert.Equal(319, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(FallingDamageStyle.Hexen, restored.ForcedFallingDamage);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        sim.Tick(); restored.Tick(); Assert.Equal(97, player.Health);
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Equal(sim.PendingSoundRequests, restored.PendingSoundRequests);
        SimSavegame.Apply(restored, older); Assert.Equal(FallingDamageStyle.None, restored.ForcedFallingDamage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(int.MaxValue)]
    public void InvalidSnapshotAndBinaryStylesRejectBeforeMutation(int value)
    {
        var sim = Room(); var snapshot = sim.CaptureState(); snapshot.ForcedFallingDamage = (FallingDamageStyle)value;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(snapshot));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(snapshot));
        Assert.Equal(FallingDamageStyle.None, sim.ForcedFallingDamage);
        sim.ForcedFallingDamage = FallingDamageStyle.Hexen; var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-forcedfalling-header", error);
    }

    [Theory]
    [InlineData(FallingDamageStyle.None, FallingDamageStyle.None, "*death")]
    [InlineData(FallingDamageStyle.Hexen, FallingDamageStyle.None, "*splat")]
    [InlineData(FallingDamageStyle.None, FallingDamageStyle.Strife, "*splat")]
    public void DeathSplatSelectionUsesCombinedPolicy(FallingDamageStyle map, FallingDamageStyle forced, string sound)
    {
        var sim = Room(map); sim.ForcedFallingDamage = forced; var player = sim.Players.Single();
        player.VelocityZ = Fixed.FromInt(-39); player.DeathDamageAmount = 100;
        ActorSoundActions.PlayerScream(player);
        Assert.Contains(sim.PendingSoundRequests, request => request.Sound == sound && !request.Stop);
    }

    [Fact]
    public void ForcedPlayerStyleDoesNotEnableMonsterDamage()
    {
        var sim = Room(); sim.ForcedFallingDamage = FallingDamageStyle.Strife;
        var actor = sim.AddBot(100, 0); actor.Brain = null;
        actor.Z = Fixed.FromInt(1); actor.VelocityZ = Fixed.FromInt(-40); actor.OnGround = false;
        ActorPhysics.Step(sim, actor); Assert.Equal(20, actor.Health);
    }

    private static AuthoritySimulation Room(FallingDamageStyle style = FallingDamageStyle.None) => AuthoritySimulation.Start(new PlayLevel
    {
        FallingDamage = style,
        SoundDefinitions = new Dictionary<string, string> { ["*death"] = "DSDEATH", ["*splat"] = "DSSPLAT" },
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }],
    });
}
