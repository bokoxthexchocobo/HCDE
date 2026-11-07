using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1451SliceTests
{
    [Fact]
    public void QuantizeBasisViewAndCommand()
    {
        Assert.Equal(0, VmNet1451Slice.QuantizePos(double.NaN));
        Assert.Equal(1, VmNet1451Slice.QuantizePos(0.03125));
        Assert.Equal(-1, VmNet1451Slice.QuantizePos(-0.03125));
        Assert.Equal(int.MaxValue, VmNet1451Slice.QuantizePos(300000000d));
        Assert.Equal((short)1, VmNet1451Slice.QuantizeVel(0.015625));
        Assert.Equal(short.MaxValue, VmNet1451Slice.QuantizeVel(2000d));
        Assert.Equal(1d, VmNet1451Slice.DequantizePos(16));
        Assert.Equal(0.5, VmNet1451Slice.DequantizeVel(16));
        Assert.Equal((ushort)2, VmNet1451Slice.CompactAngle(0x00020000));
        Assert.Equal(0x00020000u, VmNet1451Slice.ExpandAngle(2));

        var logs = new List<string>();
        var buf = new byte[32];
        var wrote = VmNet1451Slice.AppendQuantizedPos(buf, 0, -0.03125, logs);
        var read = VmNet1451Slice.ReadQuantizedPos(buf, 0, logs);
        Assert.Equal((true, -1d / 16d, wrote.Cursor), read);
        wrote = VmNet1451Slice.AppendQuantizedVel(buf, 4, 0.015625, logs);
        Assert.Equal((true, 1d / 32d, wrote.Cursor), VmNet1451Slice.ReadQuantizedVel(buf, 4, logs));

        var field = VmNet1451Slice.AppendField(buf, 8, new byte[] { 9, 8, 7 }, 1, 2, logs);
        Assert.Equal((true, 10, 3), field);
        Assert.Equal(8, buf[8]);
        Assert.Equal((false, 0, 3), VmNet1451Slice.AppendField(buf, 0, new byte[] { 1 }, 3, 1, logs));
        Assert.Contains(logs, line => line.Contains("reason=input_ge_size"));

        var world = new VmNet1451Slice.World { Console = 2, TicDup = 2 };
        world.Local[4].Buttons = 11;
        world.Remote[3, 2].Yaw = 6;
        Assert.Null(VmNet1451Slice.BuildBasis(world, 2, 0));
        Assert.Equal(11u, VmNet1451Slice.BuildBasis(world, 2, 3)!.Value.Buttons);
        Assert.Equal((short)6, VmNet1451Slice.BuildBasis(world, 3, 3)!.Value.Yaw);
        Assert.Null(VmNet1451Slice.BuildBasis(world, 70, 3));
        Assert.Null(VmNet1451Slice.ReceiveBasis(world, -1, 3));
        Assert.Equal((short)6, VmNet1451Slice.ReceiveBasis(world, 3, 3)!.Value.Yaw);

        var cmd = new VmNet1451Slice.Cmd { Buttons = 0x01020304, Pitch = 5, Yaw = -2, Roll = 7, Forward = 8, Side = 9, Up = 10 };
        var packed = VmNet1451Slice.AppendUserCmd(buf, 0, cmd, logs);
        Assert.Equal(16, packed.Cursor);
        var opened = VmNet1451Slice.ReadUserCmdPrefix(buf, 0, logs);
        Assert.True(opened.Ok);
        Assert.Equal(10, opened.Cursor);
        Assert.Equal(0x01020304u, opened.Command.Buttons);
        Assert.Equal((short)-2, opened.Command.Yaw);
        Assert.Equal((short)0, opened.Command.Forward);
        Assert.False(VmNet1451Slice.ReadUserCmdPrefix(buf, 20, logs).Ok);
        Assert.Contains(logs, line => line.Contains("read usercmd failed: cursor=20") && line.Contains("insufficient_data"));

        world.Smooth = true;
        world.Active = true;
        world.HasMo = true;
        world.Console = 2;
        world.ClientTic = 3;
        world.X = 4;
        world.Z = 4;
        world.VelLengthSq = 1;
        world.DecayFactor = 0.5;
        VmNet1451Slice.DecayView(world);
        Assert.Equal(0d, world.X);
        Assert.Equal(0d, world.Y);
        Assert.Equal(2d, world.Z);
        Assert.True(world.Active);
        world.VelLengthSq = 0;
        world.Z = 3;
        world.DecayFactor = 1;
        VmNet1451Slice.DecayView(world);
        Assert.False(world.Active);
        Assert.Equal(0d, world.Z);
        world.Smooth = false;
        world.Active = true;
        world.X = 5;
        VmNet1451Slice.DecayView(world);
        Assert.Equal(5d, world.X);
        Assert.False(VmNet1451Slice.ViewOffset(world, out _, out _, out _, out _, out _));
        world.Smooth = true;
        Assert.True(VmNet1451Slice.ViewOffset(world, out var x, out _, out _, out _, out _));
        Assert.Equal(5d, x);
    }

    [Fact]
    public void InvasionHeaderAndLaneBudget()
    {
        var idle = new VmNet1451Slice.World();
        Assert.Equal((false, 0), VmNet1451Slice.AppendInvasion(idle, -1, new byte[8], 0));
        Assert.Equal((true, 0), VmNet1451Slice.AppendInvasion(idle, 1, new byte[8], 0));
        idle.Invasion = true;
        idle.Caps = 1UL << 4;
        var partial = VmNet1451Slice.AppendInvasion(idle, 1, new byte[6], 0);
        Assert.False(partial.Ok);
        Assert.Equal(6, partial.Cursor);
        idle.Caps = 0;

        Assert.Equal((true, 0), VmNet1451Slice.AppendInvasion(idle, 1, new byte[8], 0));

        var world = new VmNet1451Slice.World
        {
            Invasion = true,
            Caps = (1UL << 4) | (1UL << 18) | (1UL << 19) | (1UL << 17) | (1UL << 16),
            Boss = true,
            Fallback = true,
            State = 99,
            StateTics = -4,
            Wave = 3,
            MaxWaves = 8,
            Budget = 1,
            Spawned = 2,
            Cleared = 3,
            Monsters = 4,
            Spots = 5,
            ActiveSpots = 6,
            PlanBudget = 7,
            Tag = 8,
            FallbackSource = 9,
        };
        var buf = new byte[4000];
        var built = VmNet1451Slice.AppendInvasion(world, 2, buf, 0);
        Assert.True(built.Ok);
        Assert.Equal(52, built.Cursor);
        Assert.Equal(new byte[] { (byte)'H', (byte)'C', (byte)'I', (byte)'V', 2, 1, 7, 0 }, buf.Take(8).ToArray());
        Assert.Equal(0, buf[8]);
        Assert.Equal(3, buf[15]);
        Assert.Equal(1, world.Packets);
        Assert.Equal(52, world.Bytes);
        Assert.Contains("tx 2 2 52", world.Calls);
        Assert.Contains("clamp 2", world.Calls);
        Assert.Contains("events 436", world.Calls);
        Assert.Contains("deltas 952", world.Calls);

        world.EventsOk = false;
        world.Packets = 0;
        world.Calls.Clear();
        var stopped = VmNet1451Slice.AppendInvasion(world, 2, buf, 0);
        Assert.False(stopped.Ok);
        Assert.Equal(0, world.Packets);
        Assert.Contains(world.Calls, line => line.StartsWith("tx "));
        Assert.Contains(world.Calls, line => line.StartsWith("events "));
        Assert.DoesNotContain(world.Calls, line => line.StartsWith("deltas "));
    }
}
