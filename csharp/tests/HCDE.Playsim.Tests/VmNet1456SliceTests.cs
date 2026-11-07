using Xunit;

namespace HCDE.Playsim.Tests;

public class VmNet1456SliceTests
{
    [Fact]
    public void AnnouncementsAndOrdinals()
    {
        Assert.Equal("1st", VmNet1456Slice.Ordinal(1));
        Assert.Equal("22nd", VmNet1456Slice.Ordinal(22));
        Assert.Equal("3rd", VmNet1456Slice.Ordinal(3));
        Assert.Equal("11th", VmNet1456Slice.Ordinal(11));
        Assert.Equal("113th", VmNet1456Slice.Ordinal(113));
        Assert.True(VmNet1456Slice.RoundActive(3));
        Assert.False(VmNet1456Slice.RoundActive(6));

        var world = new VmNet1456Slice.World { Demo = true, GameType = 4, AnnouncedWave = 4, LastSecond = 2 };
        VmNet1456Slice.TickAnnouncements(world);
        Assert.Equal(0, world.AnnouncedWave);
        Assert.Equal(-1, world.LastSecond);

        world.Demo = false;
        world.State = 3;
        world.Wave = 2;
        world.AnnouncedState = 2;
        VmNet1456Slice.TickAnnouncements(world);
        Assert.Equal(VmNet1456Slice.Red + "Begin!", Assert.Single(world.Mid));
        Assert.Equal(3, world.AnnouncedState);

        world.Mid.Clear();
        world.State = 5;
        world.StateTics = 70;
        VmNet1456Slice.TickAnnouncements(world);
        Assert.Equal(VmNet1456Slice.Red + "Wave 2 Complete!", world.Mid[0]);
        Assert.Contains("3rd wave starting in: 2", world.Mid[1]);
        int count = world.Mid.Count;
        VmNet1456Slice.TickAnnouncements(world);
        Assert.Equal(count, world.Mid.Count);

        world.Dedicated = true;
        world.Mid.Clear();
        world.State = 2;
        world.StateTics = 35;
        world.Cutscene = 105;
        world.LocalService = true;
        world.PendingWave = 1;
        VmNet1456Slice.TickAnnouncements(world);
        Assert.Empty(world.Mid);
        Assert.Contains("1st wave starting in: 3", Assert.Single(world.Console));
        VmNet1456Slice.Show(world, "");
        Assert.Single(world.Console);
    }

    [Fact]
    public void LevelStartHudAndState()
    {
        var world = new VmNet1456Slice.World { Netgame = true, GameTic = 9, ClientTic = 12, Room = 3, Map = "MAP02" };
        VmNet1456Slice.SetStartStatus(world, 0, null);
        Assert.Empty(world.Logs);
        VmNet1456Slice.SetStartStatus(world, 2, null);
        Assert.Contains("ready -> waiting reason=? gametic=9 clienttic=12 room=3 map=MAP02", Assert.Single(world.Logs));
        Assert.True(world.BaselineReset);
        VmNet1456Slice.SetStartStatus(world, 0, "loaded");
        Assert.True(world.JitterReset);
        Assert.True(world.DiagReady);

        world.EnterTic = 4;
        world.LastUpdate = 9;
        world.LagState = 3;
        world.State = 3;
        world.MirrorMicros = 2500;
        world.MirrorPasses = 2;
        world.UsesDedicatedSlot = true;
        VmNet1456Slice.RefreshHud(world);
        Assert.Equal(0, world.Hud.Stale);
        Assert.Equal("skipping", world.Hud.Lag);
        Assert.Equal("spawning", world.Hud.Invasion);
        Assert.Equal(1.25, world.Hud.AvgMirror);
        Assert.True(world.Hud.DedicatedClient);
        Assert.Equal(3, world.Hud.Backlog);

        world.Debug = true;
        world.WipeGrace = 8;
        world.Wave = 1;
        world.Participants = 2;
        world.Alive = 1;
        VmNet1456Slice.SetInvasionState(world, 6, -5, null);
        Assert.Equal(0, world.StateTics);
        Assert.Equal(0, world.WipeGrace);
        Assert.Contains("state spawning -> victory tics=0 reason=none", world.Logs[^1]);
        Assert.Contains("participants=2 alive=1", Assert.Single(world.Console));
        world.Console.Clear();
        VmNet1456Slice.SetInvasionState(world, 6, 0, "again");
        Assert.Empty(world.Console);
    }
}
