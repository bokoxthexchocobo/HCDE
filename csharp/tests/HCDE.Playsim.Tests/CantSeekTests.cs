using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CantSeekTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void FlagSurvivesSaveAndGatesDirectAndPeriodicSteering(bool cantSeek, bool periodic)
    {
        var (sim, missile, target) = Room();
        Assert.True(AcsActorFlags.TrySet(target, "cantseek", cantSeek));
        Assert.True(AcsActorFlags.TryGet(target, "CANTSEEK", out var flag));
        Assert.Equal(cantSeek, flag);
        Assert.Equal(!cantSeek, missile.CanSeek(target));
        var bytes = SimSavegame.Write(sim);
        var state = sim.CaptureState();
        target.CantSeek = !cantSeek;
        sim.RestoreState(state);
        Assert.Equal(cantSeek, target.CantSeek);
        target.CantSeek = !cantSeek;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(cantSeek, target.CantSeek);
        var angle = missile.Angle; var vertical = missile.VelocityZ;
        if (periodic) sim.Tick(); else missile.Trace(16.875);
        Assert.Equal(cantSeek ? angle : BamAngle.FromDegrees(16.875), missile.Angle);
        if (cantSeek) Assert.Equal(vertical, missile.VelocityZ);
        else Assert.NotEqual(vertical, missile.VelocityZ);
        Assert.Equal(target.Id, missile.TracerTargetId);
    }

    [Fact]
    public void LegacySaveResetsFlagAndFlagChangesChecksum()
    {
        var (sim, _, target) = Room();
        var bytes = SimSavegame.Write(sim);
        sim.RestoreState(sim.CaptureState());
        var checksum = sim.Checksum;
        target.CantSeek = true;
        sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, bytes);
        Assert.False(target.CantSeek);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void InvalidArchiveFlagIsRejectedWithoutApplyingState()
    {
        var (sim, _, target) = Room(); target.CantSeek = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(183, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-cantseek-value", error);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.True(target.CantSeek);
    }

    private static (AuthoritySimulation, ProjectileActor, Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(0, 0); var target = sim.AddBot(800, 0);
        owner.Brain = target.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        target.Y = Fixed.FromInt(800); missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
