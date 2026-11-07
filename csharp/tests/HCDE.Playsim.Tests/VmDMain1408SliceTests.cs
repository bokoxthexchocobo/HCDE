using S = HCDE.Playsim.VmDMain1408Slice;

namespace HCDE.Playsim.Tests;

public class VmDMain1408SliceTests
{
    [Fact]
    public void TimeScaleCursorAndRenderModeClamp()
    {
        var world = new S.World();
        Assert.Equal(422, S.BandStart);
        Assert.Equal(649, S.BandEnd);
        Assert.Equal(657, S.DmFlagsStart);
        Assert.Equal(0.05f, S.MinTimeScale);
        Assert.Equal(1, S.WipeMelt);
        Assert.Equal(4, S.WipeCount);

        Assert.Equal(2f, S.ApplyTimeScale(world, 2f));
        Assert.Equal(2f, world.TimeScale);
        Assert.Equal(new[] { "freeze", "thaw" }, world.Trace);
        world.Trace.Clear();
        Assert.Equal(0.04f, S.ApplyTimeScale(world, 0.04f));
        Assert.Equal(2f, world.TimeScale);
        Assert.Equal(S.MinScaleMessage, Assert.Single(world.Trace));
        world.Trace.Clear();
        world.NetGame = true;
        Assert.Equal(1f, S.ApplyTimeScale(world, 3f));
        Assert.Equal(2f, world.TimeScale);
        Assert.Equal(S.NetScaleMessage, Assert.Single(world.Trace));

        world.Trace.Clear();
        world.UserGame = true;
        Assert.Equal(0, S.ApplyRenderMode(world, 2));
        Assert.Equal(0, world.RenderMode);
        Assert.Equal(new[] { "pitch", "filter" }, world.Trace);
        world.Trace.Clear();
        world.UserGame = false;
        Assert.Equal(1, S.ApplyRenderMode(world, 3));
        Assert.Equal(4, S.ApplyRenderMode(world, 5));
        Assert.Equal(4, S.ApplyRenderMode(world, -1));
        Assert.Equal(4, world.RenderMode);
        Assert.Equal(new[] { "filter", "filter", "filter" }, world.Trace);

        Assert.Equal(0, S.ApplyWipeType(0));
        Assert.Equal(2, S.ApplyWipeType(2));
        Assert.Equal(3, S.ApplyWipeType(3));
        Assert.Equal(1, S.ApplyWipeType(-1));
        Assert.Equal(1, S.ApplyWipeType(4));

        var cursors = new List<string>();
        world.CursorPic = "foo";
        S.ApplyCursor(world, "nONE", name =>
        {
            cursors.Add(name);
            return true;
        });
        Assert.Equal("foo", Assert.Single(cursors));
        cursors.Clear();
        S.ApplyCursor(world, "None", name =>
        {
            cursors.Add(name);
            return false;
        });
        Assert.Equal(new[] { "foo", "cursor" }, cursors);
        cursors.Clear();
        world.CursorPic = "";
        S.ApplyCursor(world, "None", name =>
        {
            cursors.Add(name);
            return name == "cursor";
        });
        Assert.Equal(new[] { "None", "cursor" }, cursors);

        world.Trace.Clear();
        S.ApplyFullbrightOverrides(world, true);
        S.ApplyFullbrightOverrides(world, false);
        Assert.Equal(new[] { "fullbright on", "fullbright off" }, world.Trace);
        S.ApplyFriendlyTitle(world, 0);
        Assert.Equal(0, world.FriendlyTitle);
        Assert.Equal("title", world.Trace[^1]);
        S.ApplyFriendlyTitle(world, 2);
        Assert.Equal(2, world.FriendlyTitle);
    }

    [Fact]
    public void HudToggleFragLimitAndRenderPass()
    {
        var world = new S.World();
        world.ScreenBlocks = 9;
        world.DrawPlayerSprite = true;
        world.ShowMessages = true;
        S.ToggleHudCommand(world);
        Assert.True(world.HudToggled);
        Assert.Equal(9, world.SavedBlocks);
        Assert.True(world.SavedDraw);
        Assert.True(world.SavedMessages);
        Assert.Equal(S.HiddenBlocks, world.ScreenBlocks);
        Assert.False(world.DrawPlayerSprite);
        Assert.False(world.ShowMessages);
        Assert.Equal(new[] { "hide", "clearmenu" }, world.Trace);
        world.SavedBlocks = 8;
        world.Trace.Clear();
        S.ToggleHud(world);
        Assert.False(world.HudToggled);
        Assert.Equal(8, world.ScreenBlocks);
        Assert.True(world.DrawPlayerSprite);
        Assert.True(world.ShowMessages);
        Assert.Empty(world.Trace);

        world.Deathmatch = true;
        world.InGame[0] = false;
        world.Frags[0] = 10;
        world.InGame[1] = true;
        world.Frags[1] = 3;
        world.InGame[2] = true;
        world.Frags[2] = 9;
        Assert.Equal(3, S.ApplyFragLimit(world, 3, key =>
        {
            Assert.Equal(S.FragLimitKey, key);
            return "limit";
        }));
        Assert.Equal(new[] { "limit", "exit 0 false" }, world.Trace);
        world.Trace.Clear();
        Assert.Equal(0, S.ApplyFragLimit(world, 0, _ => throw new InvalidOperationException("text")));
        world.Deathmatch = false;
        Assert.Equal(1, S.ApplyFragLimit(world, 1, _ => throw new InvalidOperationException("text")));
        Assert.Empty(world.Trace);

        world.RenderMode = S.HardwareMode;
        world.GlLights = true;
        world.DynLights = true;
        var hardware = new S.Level { Lights = true };
        var software = new S.Level { Probes = 2 };
        var dark = new S.Level();
        world.Levels.Add(hardware);
        world.Levels.Add(software);
        world.Levels.Add(dark);
        int during = -1;
        S.Render(world, true, 0.25f, () => during = hardware.Restores);
        Assert.Equal(0, during);
        Assert.True(hardware.HasDynamicLights);
        Assert.True(software.HasDynamicLights);
        Assert.False(dark.HasDynamicLights);
        Assert.Equal(1, hardware.Interpolations);
        Assert.Equal(0.25f, hardware.LastFrac);
        Assert.Equal(1, hardware.ParticlePasses);
        Assert.Equal(1, hardware.LinkPasses);
        Assert.Equal(1, hardware.Restores);
        Assert.Equal(1, software.Restores);
        Assert.Equal(1, dark.Restores);

        world.RenderMode = 0;
        world.GlLights = true;
        world.DynLights = false;
        dark.Lights = true;
        dark.Probes = 0;
        S.Render(world, false, 9f, () => { });
        Assert.False(dark.HasDynamicLights);
        Assert.Equal(1, dark.Interpolations);
        Assert.Equal(1, dark.Restores);
        Assert.Equal(2, dark.ParticlePasses);

        world.DynLights = true;
        dark.Lights = false;
        S.Render(world, false, 9f, () => { });
        Assert.False(dark.HasDynamicLights);
        software.Lights = false;
        world.RenderMode = S.HardwareMode;
        world.GlLights = false;
        S.Render(world, false, 1f, () => { });
        Assert.True(software.HasDynamicLights);
        Assert.Throws<ArgumentNullException>(() => S.Render(world, false, 0f, null!));
    }
}
