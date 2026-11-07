using S = HCDE.Playsim.VmDMain1409Slice;

namespace HCDE.Playsim.Tests;

public class VmDMain1409SliceTests
{
    [Fact]
    public void DmFlagsFreelookFovSpyAndChase()
    {
        var world = new S.World();
        Assert.Equal(657, S.BandStart);
        Assert.Equal(912, S.BandEnd);
        Assert.Equal(974, S.GetCapsStart);
        Assert.Equal(64, S.MaxPlayers);

        Assert.Equal(0, S.ApplyDmFlags(world, 0));
        Assert.Equal("sky", Assert.Single(world.Trace));
        Assert.False(world.FovSent);
        world.Trace.Clear();
        S.ApplyDmFlags(world, unchecked((int)S.NoFreelook));
        Assert.Equal(new[] { "sky", "center" }, world.Trace);
        world.Trace.Clear();
        world.LocalAuthority = false;
        world.DesiredFov = 0f;
        S.ApplyDmFlags(world, unchecked((int)S.NoFov));
        Assert.Equal("sky", Assert.Single(world.Trace));
        Assert.False(world.FovSent);
        world.LocalAuthority = true;
        world.Trace.Clear();
        S.ApplyDmFlags(world, unchecked((int)(S.NoFreelook | S.NoFov)));
        Assert.Equal(new[] { "sky", "center", "fov" }, world.Trace);
        Assert.True(world.FovSent);
        Assert.Equal(90f, world.SentFov);
        world.DesiredFov = -1f;
        world.FovSent = false;
        world.Trace.Clear();
        S.ApplyDmFlags(world, unchecked((int)S.NoFov));
        Assert.Equal(-1f, world.SentFov);
        Assert.Equal(new[] { "sky", "fov" }, world.Trace);

        world.Trace.Clear();
        world.AutomapActive = false;
        world.CameraId = 4;
        world.MoId = 4;
        int checks = 0;
        var chased = new List<int>();
        S.ApplyDmFlags2(world, unchecked((int)(S.NoAutomap | S.DisallowSpying)), _ =>
        {
            checks++;
            return false;
        }, chased.Add);
        Assert.Empty(world.Trace);
        Assert.Equal(0, checks);
        Assert.Equal(4, world.CameraId);

        world.AutomapActive = true;
        world.CameraId = 2;
        world.MoId = 1;
        world.DemoPlayback = false;
        world.Multiplayer = false;
        world.InGame[1] = true;
        world.InGame[3] = true;
        world.Cheats[1] = S.ChaseCamCheat;
        world.Cheats[3] = 0;
        var seen = new List<bool>();
        S.ApplyDmFlags2(world, unchecked((int)(S.NoAutomap | S.DisallowSpying)), arg =>
        {
            seen.Add(arg);
            return true;
        }, chased.Add);
        Assert.Equal(new[] { "stopmap", "sounds", "attach" }, world.Trace);
        Assert.Equal(1, world.CameraId);
        Assert.Equal(new[] { false, false }, seen);
        Assert.Equal(1, Assert.Single(chased));

        world.Trace.Clear();
        chased.Clear();
        world.CameraId = 9;
        world.MoId = 8;
        world.DemoPlayback = true;
        world.Multiplayer = false;
        S.ApplyDmFlags2(world, unchecked((int)S.DisallowSpying), _ => true, chased.Add);
        Assert.Equal(new[] { "sounds", "attach", "name" }, world.Trace);
        Assert.Equal(1, Assert.Single(chased));
        world.Trace.Clear();
        world.CameraId = 3;
        world.MoId = 3;
        world.Cheats[1] = 0;
        world.DemoPlayback = false;
        world.Multiplayer = true;
        S.ApplyDmFlags2(world, unchecked((int)S.DisallowSpying), _ => true, _ => throw new InvalidOperationException("chase"));
        Assert.Empty(world.Trace);

        world.InGame[0] = true;
        world.Cheats[0] = S.ChaseCamCheat;
        chased.Clear();
        int blocked = 0;
        S.ApplyDmFlags2(world, unchecked((int)S.ChaseCamFlag), _ =>
        {
            blocked++;
            return true;
        }, chased.Add);
        Assert.Equal(0, blocked);
        Assert.Empty(chased);
        Assert.Equal(unchecked((uint)S.ChaseCamFlag), world.DmFlags2);

        Assert.Equal(7, S.ApplyDmFlags3(world, 7));
        Assert.Equal(7u, world.DmFlags3);
        Assert.Empty(world.Trace);
        Assert.Throws<ArgumentNullException>(() => S.ApplyDmFlags(null!, 0));
        Assert.Throws<ArgumentNullException>(() => S.ApplyDmFlags2(world, 0, null!, chased.Add));
    }

    [Fact]
    public void CompatModeWritesBothMasksThroughTheirCallbacks()
    {
        var expected = new Dictionary<int, (uint V, uint W)>
        {
            [1] = (2820160259u, 16642u),
            [2] = (3122215895u, 573963u),
            [3] = (2148050944u, 16640u),
            [4] = (536872960u, 16624u),
            [5] = (2240325632u, 28928u),
            [6] = (2416552144u, 541192u),
            [7] = (2508826832u, 29448u),
            [8] = (2241374208u, 1061120u),
            [9] = (2509875408u, 1585928u),
        };
        var world = new S.World();
        var first = new S.Level();
        var second = new S.Level();
        world.Levels.Add(first);
        world.Levels.Add(second);
        foreach (var pair in expected)
        {
            Assert.Equal(pair.Key, S.ApplyCompatMode(world, pair.Key));
            Assert.Equal(pair.Value.V, world.CompatFlags);
            Assert.Equal(pair.Value.W, world.CompatFlags2);
        }
        Assert.Equal(9, first.CompatPasses);
        Assert.Equal(9, first.Compat2Passes);
        Assert.Equal(9, first.SidePasses);
        Assert.Equal(9, second.CompatPasses);
        Assert.Equal((S.Explode1 | S.AvoidHazards | S.StayOnLift | S.NoMbf21), expected[5].W);
        Assert.Equal((S.Explode1 | S.AvoidHazards | S.StayOnLift | S.ReservedLineFlag), expected[8].W);
        Assert.NotEqual(expected[5].W, expected[8].W);

        S.ApplyCompatMode(world, 0);
        Assert.Equal(0u, world.CompatFlags);
        Assert.Equal(0u, world.CompatFlags2);
        S.ApplyCompatMode(world, -1);
        S.ApplyCompatMode(world, 10);
        Assert.Equal(0u, world.CompatFlags);
        Assert.Equal(12, first.CompatPasses);
        Assert.Equal(12, first.SidePasses);

        world.Levels.Clear();
        S.ApplyCompatFlags(world, 3);
        S.ApplyCompatFlags2(world, 4);
        Assert.Equal(3u, world.CompatFlags);
        Assert.Equal(4u, world.CompatFlags2);
        Assert.Throws<ArgumentNullException>(() => S.ApplyCompatMode(null!, 1));
    }
}
