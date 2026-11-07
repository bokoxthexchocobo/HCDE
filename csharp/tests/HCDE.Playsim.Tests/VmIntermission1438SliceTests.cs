using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmIntermission1438SliceTests
{
    [Fact]
    public void ControllerInputAndTicker()
    {
        Assert.Equal(987, VmIntermission1438Slice.BandStart);
        Assert.Equal(1205, VmIntermission1438Slice.BandEnd);
        var world = new VmIntermission1438Slice.World();
        var bare = new VmIntermission1438Slice.Ctrl();
        Assert.False(VmIntermission1438Slice.Respond(world, bare, null));
        var screen = new VmIntermission1438Slice.Screen { Age = 0 };
        var ctrl = new VmIntermission1438Slice.Ctrl { Screen = screen };
        Assert.Throws<NullReferenceException>(() => VmIntermission1438Slice.Respond(world, ctrl, null));
        world.Binds[1] = "ToggleConsole";
        world.Binds[2] = "PAUSE";
        world.Binds[3] = "screenshot";
        Assert.False(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = VmIntermission1438Slice.KeyDown, Key = 1 }));
        Assert.False(world.MenuOpen);
        Assert.True(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = VmIntermission1438Slice.KeyDown, Key = 2 }));
        Assert.True(world.MenuOpen);
        Assert.Equal("MainMenu", world.Menu);
        Assert.Equal(-1, world.MenuArg);
        Assert.Equal(0, screen.Calls);
        Assert.False(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = VmIntermission1438Slice.KeyDown, Key = 9 }));
        screen.Age = 2;
        screen.Answer = -1;
        Assert.True(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = 4, Key = 9 }));
        Assert.True(ctrl.Advance);
        Assert.Equal(1, screen.Calls);
        screen.Answer = 0;
        ctrl.Advance = false;
        Assert.False(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = 4 }));

        world.Net = true;
        world.Console = 3;
        screen.Calls = 0;
        Assert.True(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = VmIntermission1438Slice.KeyDown, Key = 9 }));
        Assert.Equal(new[] { 3 }, world.Ready);
        Assert.Equal(new[] { 77 }, world.Bytes);
        Assert.Equal(0, screen.Calls);
        world.Console = -1;
        world.Ready.Clear();
        Assert.True(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = VmIntermission1438Slice.KeyDown }));
        Assert.Empty(world.Ready);
        Assert.Equal(2, world.Bytes.Count);
        world.Console = 64;
        Assert.True(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = VmIntermission1438Slice.KeyDown }));
        Assert.Empty(world.Ready);
        Assert.False(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = 4 }));
        world.Demo = true;
        screen.Answer = 1;
        Assert.True(VmIntermission1438Slice.Respond(world, ctrl, new VmIntermission1438Slice.Ev { Type = 4 }));
        Assert.Equal(1, screen.Calls);

        var run = new VmIntermission1438Slice.Ctrl
        {
            Screen = new VmIntermission1438Slice.Screen(),
            Advance = true,
            Sent = true,
            Desc = new VmIntermission1438Slice.Desc(),
        };
        Assert.True(VmIntermission1438Slice.Tick(run));
        Assert.False(run.Sent);
        Assert.Equal(1, run.Screen.Ticked);
        Assert.Equal(1, run.NextCalls);
        Assert.False(run.Advance);
        run.Desc.Opens = false;
        run.Advance = true;
        Assert.False(VmIntermission1438Slice.Tick(run));
        run.Screen.TickResult = -1;
        run.Advance = false;
        run.Desc.Opens = true;
        Assert.True(VmIntermission1438Slice.Tick(run));
        Assert.Equal(3, run.NextCalls);
        run.Screen.TickResult = 0;
        Assert.True(VmIntermission1438Slice.Tick(run));
        Assert.Equal(3, run.NextCalls);
    }

    [Fact]
    public void ControllerDrawAndDestroy()
    {
        var world = new VmIntermission1438Slice.World();
        var ctrl = new VmIntermission1438Slice.Ctrl();
        VmIntermission1438Slice.Draw(world, ctrl);
        Assert.Equal(0, world.Clears);
        Assert.Equal(0, VmIntermission1438Slice.CallDraw(world, ctrl));
        var screen = new VmIntermission1438Slice.Screen();
        ctrl.Screen = screen;
        Assert.Equal(0, VmIntermission1438Slice.CallBegin(ctrl));
        Assert.Equal(1, screen.Started);
        VmIntermission1438Slice.Draw(world, ctrl);
        Assert.Equal(1, world.Clears);
        Assert.Equal(1, screen.Drawn);
        ctrl.Desc = new VmIntermission1438Slice.Desc { Id = 4 };
        ctrl.Delete = true;
        var held = ctrl.Screen;
        VmIntermission1438Slice.Destroy(world, ctrl);
        Assert.True(held.Dead);
        Assert.Null(ctrl.Screen);
        Assert.Null(ctrl.Desc);
        Assert.Equal(new[] { 4 }, world.Deleted);
        var kept = new VmIntermission1438Slice.Ctrl { Desc = new VmIntermission1438Slice.Desc { Id = 8 } };
        VmIntermission1438Slice.Destroy(world, kept);
        Assert.Null(kept.Desc);
        Assert.Equal(new[] { 4 }, world.Deleted);
    }

    [Fact]
    public void StartFinale()
    {
        var world = new VmIntermission1438Slice.World();
        var desc = new VmIntermission1438Slice.Desc { Id = 6, Opens = true };
        var ctrl = VmIntermission1438Slice.Start(world, desc, VmIntermission1438Slice.InLevelNoWipe, true, true);
        Assert.NotNull(ctrl);
        Assert.True(world.Stopped);
        Assert.Equal(0, world.Action);
        Assert.Equal(2, world.State);
        Assert.Equal(2, world.Wipe);
        Assert.True(ctrl!.Delete);
        Assert.True(ctrl.EndGame);
        Assert.Equal(1, ctrl.NextCalls);
        Assert.Same(desc, ctrl.Desc);

        world.Wipe = 0;
        var quiet = VmIntermission1438Slice.Start(world, new VmIntermission1438Slice.Desc { Id = 1 }, 1, false, false);
        Assert.NotNull(quiet);
        Assert.Equal(0, world.Wipe);
        Assert.False(quiet!.Delete);

        desc.Opens = false;
        Assert.Null(VmIntermission1438Slice.Start(world, desc, 1, true, false));
        Assert.Equal(new[] { 6 }, world.Deleted);
    }

    [Fact]
    public void NamedStartAndWrappers()
    {
        var world = new VmIntermission1438Slice.World { Action = 9, State = 9 };
        Assert.Null(VmIntermission1438Slice.StartNamed(world, "missing", 3));
        Assert.Equal(9, world.Action);
        world.Links["empty"] = new VmIntermission1438Slice.Desc();
        Assert.Null(VmIntermission1438Slice.StartNamed(world, "EMPTY", 3));
        world.Links["title"] = new VmIntermission1438Slice.Desc
        {
            Actions = { new() { Class = VmIntermission1438Slice.Title } },
        };
        Assert.Null(VmIntermission1438Slice.StartNamed(world, "title", 3));
        Assert.Equal(9, world.State);
        world.Links["cast"] = new VmIntermission1438Slice.Desc
        {
            Id = 2,
            Actions = { new() { Class = 1 } },
        };
        var ctrl = VmIntermission1438Slice.StartNamed(world, "Cast", VmIntermission1438Slice.InLevelNoWipe);
        Assert.NotNull(ctrl);
        Assert.False(ctrl!.Delete);
        Assert.False(ctrl.EndGame);
        Assert.Equal(2, world.Wipe);
        Assert.Equal(0, world.Action);
        var screen = new VmIntermission1438Slice.Screen { Answer = -1 };
        ctrl.Screen = screen;
        Assert.True(VmIntermission1438Slice.CallRespond(world, ctrl, new VmIntermission1438Slice.Ev()));
        Assert.True(ctrl.Advance);
        Assert.True(VmIntermission1438Slice.CallTick(ctrl));
        Assert.True(VmIntermission1438Slice.CallNext(ctrl));
        Assert.Equal(3, ctrl.NextCalls);
    }

    [Fact]
    public void MeasureWideRows()
    {
        Assert.Equal(37, VmIntermission1438Slice.Texts.Split(',').Length);
        Assert.Equal(14, VmIntermission1438Slice.Langs.Split(',').Length);
        var world = new VmIntermission1438Slice.World();
        uint english = VmIntermission1438Slice.DefaultTable;
        string wide = new string('A', 4);
        world.Lines["E1TEXT:" + english] = wide;
        world.Widths[wide] = 361;
        string exact = "ok";
        world.Lines["E2TEXT:" + english] = exact;
        world.Widths[exact] = 360;
        string tall = new string('\n', 20);
        uint czech = (uint)('c' | ('z' << 8));
        world.Lines["C1TEXT:" + czech] = tall;
        string edge = new string('\n', 19);
        uint eng = (uint)('e' | ('n' << 8) | ('g' << 16));
        world.Lines["NERVETEXT:" + eng] = edge;
        world.Widths[edge] = 360;
        VmIntermission1438Slice.Measure(world);
        Assert.Equal(new[] { "E1TEXT, : 361 x 1", "C1TEXT, cz: 0 x 21" }, world.Notes);
    }
}
