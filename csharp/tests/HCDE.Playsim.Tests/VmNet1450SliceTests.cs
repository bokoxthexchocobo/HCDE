using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1450SliceTests
{
    [Fact]
    public void StaleRoomRecoveryAndTail()
    {
        var world = new VmNet1450Slice.World
        {
            Room = 4,
            AuthoritySlot = 1,
            GameState = VmNet1450Slice.Cutscene,
            HasRunner = true,
            GameTic = 9,
            ClientTic = 11,
        };
        Assert.False(VmNet1450Slice.RecoverStaleRoom(world, 1, 5, 1000));
        Assert.Equal(1, world.EndJobs);
        Assert.Equal(1, world.StalledTicks);
        Assert.Equal(1000UL, world.LastForce);
        Assert.Contains("authority (client=1) is one room ahead (room=5 local=4)", Assert.Single(world.Logs));
        Assert.False(VmNet1450Slice.RecoverStaleRoom(world, 1, 5, 1299));
        Assert.Equal(1, world.EndJobs);
        Assert.False(VmNet1450Slice.RecoverStaleRoom(world, 1, 5, 1300));
        Assert.Equal(2, world.EndJobs);

        world.EndJobs = 0;
        Assert.False(VmNet1450Slice.RecoverStaleRoom(world, 2, 5, 5000));
        Assert.Equal(0, world.EndJobs);
        world.LocalAuthority = true;
        Assert.False(VmNet1450Slice.RecoverStaleRoom(world, 1, 5, 5000));
        Assert.Equal(0, world.EndJobs);
        world.LocalAuthority = false;
        world.HasRunner = false;
        Assert.False(VmNet1450Slice.RecoverStaleRoom(world, 1, 5, 5000));
        Assert.Equal(0, world.EndJobs);

        var wrapped = new VmNet1450Slice.World
        {
            Room = 255,
            AuthoritySlot = 0,
            GameState = VmNet1450Slice.Intro,
            HasRunner = true,
        };
        Assert.False(VmNet1450Slice.RecoverStaleRoom(wrapped, 0, 0, 10));
        Assert.Equal(1, wrapped.EndJobs);
        wrapped.LastForce = 50;
        Assert.False(VmNet1450Slice.RecoverStaleRoom(wrapped, 0, 0, 10));
        Assert.Equal(2, wrapped.EndJobs);

        Assert.Equal((true, 8, 27), VmNet1450Slice.CompleteEnvelope(20));
        Assert.Equal((true, 0, 27), VmNet1450Slice.CompleteEnvelope(12));
    }

    [Fact]
    public void AppendAndReadRoundTrip()
    {
        var logs = new List<string>();
        var empty = new byte[0];
        Assert.Equal((false, 0), VmNet1450Slice.AppendByte(empty, 0, 7, logs));
        Assert.Contains("append byte failed: cursor=0", Assert.Single(logs));

        var buf = new byte[16];
        logs.Clear();
        var step = VmNet1450Slice.AppendByte(buf, 0, 9, logs);
        Assert.True(step.Ok);
        Assert.Equal((true, (byte)9, 1), VmNet1450Slice.ReadByte(buf, 0, logs));
        step = VmNet1450Slice.AppendBE16(buf, 1, 0x0102, logs);
        Assert.Equal((true, (ushort)0x0102, 3), VmNet1450Slice.ReadBE16(buf, 1, logs));
        Assert.Equal((false, (ushort)0, 15), VmNet1450Slice.ReadBE16(buf, 15, logs));
        Assert.Contains(logs, line => line.Contains("read BE16 failed: cursor=15") && line.Contains("insufficient_data"));

        step = VmNet1450Slice.AppendBE32(buf, 3, 0x00000102, logs);
        var read32 = VmNet1450Slice.ReadBE32(buf, 3, logs);
        Assert.Equal((true, 0x00000102u, 7), read32);
        var wide = VmNet1450Slice.AppendDouble(buf, 0, 1.5, logs);
        Assert.True(wide.Ok);
        var back = VmNet1450Slice.ReadDouble(buf, 0, logs);
        Assert.Equal((true, 1.5, 8), back);
        logs.Clear();
        Assert.Equal((false, 4), VmNet1450Slice.AppendDouble(buf, 4, double.NaN, logs));
        Assert.Contains("append double failed: cursor=4 reason=not_finite_value", Assert.Single(logs));

        logs.Clear();
        ulong nanBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(double.NaN));
        var planted = VmNet1450Slice.AppendBE64(buf, 0, nanBits, logs);
        var nan = VmNet1450Slice.ReadDouble(buf, 0, logs);
        Assert.False(nan.Ok);
        Assert.Equal(8, nan.Cursor);
        Assert.Contains(logs, line => line.Contains("read double failed: cursor=8 reason=not_finite_value"));

        logs.Clear();
        var flt = VmNet1450Slice.AppendFloat(buf, 0, 1.5, logs);
        var fltBack = VmNet1450Slice.ReadFloat(buf, 0, logs);
        Assert.True(flt.Ok);
        Assert.Equal((true, 1.5, flt.Cursor), fltBack);
        Assert.Equal((false, 2), VmNet1450Slice.AppendFloat(buf, 2, double.PositiveInfinity, logs));
        Assert.Contains(logs, line => line.Contains("append float failed: cursor=2"));

        logs.Clear();
        var copied = VmNet1450Slice.AppendBytes(buf, 8, new byte[] { 4, 5 }, logs);
        Assert.Equal((true, 10), copied);
        Assert.Equal(4, buf[8]);
        Assert.Equal((true, 8), VmNet1450Slice.AppendBytes(buf, 8, System.Array.Empty<byte>(), logs));
        Assert.Equal((false, 16), VmNet1450Slice.AppendBytes(buf, 16, System.Array.Empty<byte>(), logs));
        Assert.Contains(logs, line => line.Contains("append bytes failed: cursor=16") && line.Contains("cursor_ge_capacity"));
    }
}
