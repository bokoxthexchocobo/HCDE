using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontSeekInvisibleTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void MissileFlagConditionallyBlocksInvisibleTargetAfterSave(bool dontSeek, bool invisible, bool periodic)
    {
        var (sim, missile, target) = Room();
        Assert.True(AcsActorFlags.TrySet(missile, "dontseekinvisible", dontSeek));
        Assert.True(AcsActorFlags.TryGet(missile, "DONTSEEKINVISIBLE", out var flag));
        Assert.Equal(dontSeek, flag);
        target.Invisible = invisible;
        var blocked = dontSeek && invisible;
        Assert.Equal(!blocked, missile.CanSeek(target));
        var state = sim.CaptureState(); missile.DontSeekInvisible = !dontSeek;
        sim.RestoreState(state); Assert.Equal(dontSeek, missile.DontSeekInvisible);
        var bytes = SimSavegame.Write(sim);
        missile.DontSeekInvisible = !dontSeek; target.Invisible = !invisible;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(dontSeek, missile.DontSeekInvisible);
        Assert.Equal(invisible, target.Invisible);
        if (periodic) sim.Tick(); else missile.Trace(16.875);
        Assert.Equal(BamAngle.FromDegrees(blocked ? 0 : 16.875), missile.Angle);
        Assert.Equal(blocked ? 0 : -0.125, missile.VelocityZ.ToDouble());
        Assert.Equal(target.Id, missile.TracerTargetId);
    }

    [Fact]
    public void LegacySaveResetsFlagAndChecksumTracksIt()
    {
        var (sim, missile, target) = Room();
        var bytes = SimSavegame.Write(sim);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        missile.DontSeekInvisible = true; sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(checksum, sim.Checksum);
        target.CantSeek = true;
        Assert.False(missile.CanSeek(target));
        SimSavegame.Apply(sim, bytes);
        Assert.False(missile.DontSeekInvisible);
        Assert.False(target.CantSeek);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void CorruptFlagIsRejectedWithoutApplyingState()
    {
        var (sim, missile, target) = Room();
        missile.DontSeekInvisible = true; target.CantSeek = true; target.Invisible = true;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(184, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        SimSavegame.Apply(sim, bytes);
        Assert.True(missile.DontSeekInvisible); Assert.True(target.CantSeek); Assert.True(target.Invisible);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-dontseekinvisible-value", error);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.True(missile.DontSeekInvisible);
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
