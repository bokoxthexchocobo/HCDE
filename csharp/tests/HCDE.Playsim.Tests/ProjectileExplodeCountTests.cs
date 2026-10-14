using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileExplodeCountTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ImpactsWaitForThresholdAndReplayFromSave(int threshold)
    {
        var (sim, missile) = Setup();
        Assert.True(AcsActorFlags.TrySet(missile, "Actor.EXPLOCOUNT", true));
        Assert.True(AcsActorFlags.TryGet(missile, "explocount", out var flag)); Assert.True(flag);
        ActorPropertyActions.SetChaseThreshold(missile, threshold, setDefault: true);
        var initial = SimSavegame.Write(sim);
        Assert.Equal(196, BinaryPrimitives.ReadUInt16LittleEndian(initial.AsSpan(4)));
        for (var hit = 1; hit <= threshold; hit++)
        {
            missile.Tick(); Assert.Equal(hit, missile.ChaseThreshold);
            Assert.Equal(hit == threshold, missile.Destroyed);
            if (hit < threshold)
            {
                Assert.Equal(-10, missile.VelocityZ.ToDouble());
                Assert.Empty(sim.DrainSoundRequests());
                var checkpoint = SimSavegame.Write(sim);
                SimSavegame.Apply(sim, checkpoint); Assert.Equal(checkpoint, SimSavegame.Write(sim));
                missile = sim.Actors.OfType<ProjectileActor>().Single();
            }
        }
        Assert.Single(sim.DrainSoundRequests());
        SimSavegame.Apply(sim, initial); missile = sim.Actors.OfType<ProjectileActor>().Single();
        Assert.False(missile.Destroyed); Assert.Equal(0, missile.ChaseThreshold);
        Assert.True(missile.ExplodeCount);
        missile.Tick(); Assert.Equal(1, missile.ChaseThreshold); Assert.Equal(threshold == 1, missile.Destroyed);
    }

    [Fact]
    public void DirectDamageOccursBeforeExplosionThreshold()
    {
        var (sim, missile) = Setup(); missile.ExplodeCount = true;
        ActorPropertyActions.SetChaseThreshold(missile, 2, setDefault: true);
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        missile.Z = Fixed.FromInt(20); missile.VelocityX = Fixed.FromInt(2); missile.VelocityZ = default;
        missile.DamageExpression = _ => 7;
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Equal(993, target.Health);
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(986, target.Health);
    }

    [Fact]
    public void LegacyArchiveClearsFlagAndMalformedValueIsRejected()
    {
        var (sim, missile) = Setup(); var legacy = SimSavegame.Write(sim);
        missile.ExplodeCount = true;
        var bytes = SimSavegame.Write(sim);
        SimSavegame.Apply(sim, bytes); var changed = sim.Checksum;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-explode-count-value", error);
        SimSavegame.Apply(sim, legacy); Assert.False(missile.ExplodeCount); Assert.NotEqual(changed, sim.Checksum);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    private static (AuthoritySimulation, ProjectileActor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); sim.DrainSoundRequests();
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(1);
        missile.VelocityX = Fixed.FromInt(1); missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(-10);
        return (sim, missile);
    }
}
