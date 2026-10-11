using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MissileDamageRandomTests
{
    [Fact]
    public void NamedStreamMatchesNativeByteMasksAndRestores()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: 42);
        var legacy = sim.CaptureState(); var combat = sim.CombatRandomState;
        var actor = sim.AddBot(0, 0); actor.Damage = 5;
        var native = NativeStateRandom.Seed(42, 0x333cfa17u);
        var first = actor.GetMissileDamage(7, 1);
        Assert.Equal(((int)(NativeStateRandom.Next(ref native) & 255) & 7) * 5 + 5, first);
        Assert.Equal(native, sim.CaptureState().MissileDamageRandomState);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(144, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => actor.GetMissileDamage(15, 2)).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => actor.GetMissileDamage(15, 2)).ToArray());
        Assert.Equal(combat, sim.CombatRandomState);
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().MissileDamageRandomState);
        Assert.Equal(first, actor.GetMissileDamage(7, 1));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 144);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-missiledamagerandom-header", error);
    }

    [Fact]
    public void ConstantAndExpressionPathsDoNotAdvanceNamedStream()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(0, 0); owner.Damage = 5;
        Assert.Equal(10, owner.GetMissileDamage(0, 2));
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.DamageExpression = _ => 19;
        Assert.Equal(19, missile.GetMissileDamage(7, 1));
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }
}
