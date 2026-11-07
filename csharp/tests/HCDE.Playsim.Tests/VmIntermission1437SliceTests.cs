using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmIntermission1437SliceTests
{
    static VmIntermission1437Slice.World Stage()
    {
        var world = new VmIntermission1437Slice.World();
        world.Textures[1] = new VmIntermission1437Slice.Tex { W = 320, H = 200 };
        world.Textures[2] = new VmIntermission1437Slice.Tex { W = 320, H = 200 };
        return world;
    }

    [Fact]
    public void ScrollAdvanceAndHorizontal()
    {
        Assert.Equal(778, VmIntermission1437Slice.BandStart);
        Assert.Equal(985, VmIntermission1437Slice.BandEnd);
        var world = Stage();
        var page = new VmIntermission1437Slice.Page();
        VmIntermission1437Slice.Init(world, page, 40, 7, 1, 2, 3, 10, VmIntermission1437Slice.ScrollLeft, true);
        Assert.Equal(0, page.Ticker);
        Assert.Equal(40, page.Duration);
        Assert.True(page.OpenedFirst);
        Assert.Equal(1, page.First);
        Assert.Equal("2:32:7:1:0", Assert.Single(world.Audio));
        Assert.Equal(0, VmIntermission1437Slice.Key(world, page, 4));
        Assert.Equal(-1, VmIntermission1437Slice.Key(world, page, VmIntermission1437Slice.KeyDown));
        Assert.Equal(2, page.Background);
        Assert.Equal(13, page.Ticker);
        page.Background = 1;
        page.Ticker = 5;
        world.AbortScroll = true;
        Assert.Equal(0, VmIntermission1437Slice.Key(world, page, VmIntermission1437Slice.KeyDown));
        Assert.Equal(1, page.Background);
        Assert.Equal(5, page.Ticker);

        world.AbortScroll = false;
        page.Ticker = 5;
        page.Delay = 0;
        VmIntermission1437Slice.Draw(world, page);
        Assert.Equal(2, page.Background);
        Assert.Equal(1, world.ClipClears);
        Assert.Equal(2, world.Pics.Count);
        Assert.Equal(1, world.Pics[0].Id);
        Assert.Equal(320d, world.Pics[0].X);
        Assert.Equal(0d, world.Pics[0].Y);
        Assert.Equal(640d, world.Pics[0].W);
        Assert.Equal(480d, world.Pics[0].H);
        Assert.Equal(-320d, world.Pics[1].X);
        Assert.Equal(0, world.Pics[0].ClipL);
        Assert.Equal(640, world.Pics[0].ClipW);
        Assert.Equal(480, world.Pics[0].ClipH);

        world.Pics.Clear();
        page.Dir = VmIntermission1437Slice.ScrollRight;
        VmIntermission1437Slice.Draw(world, page);
        Assert.Equal(-320d, world.Pics[0].X);
        Assert.Equal(320d, world.Pics[1].X);

        world.Pics.Clear();
        page.Dir = VmIntermission1437Slice.ScrollLeft;
        page.Ticker = 2;
        page.Delay = 5;
        VmIntermission1437Slice.Draw(world, page);
        Assert.Equal(0d, world.Pics[0].X);
        Assert.Equal(-640d, world.Pics[1].X);

        page.Ticker = 100;
        page.Delay = 0;
        page.Time = 0;
        world.Pics.Clear();
        VmIntermission1437Slice.Draw(world, page);
        Assert.True(double.IsNaN(world.Pics[0].X));
        Assert.Throws<NullReferenceException>(() => VmIntermission1437Slice.Draw(world, new VmIntermission1437Slice.Page { First = 9, Second = 2, Time = 1 }));
    }

    [Fact]
    public void ScrollVerticalAndAspect()
    {
        var world = Stage();
        var page = new VmIntermission1437Slice.Page { First = 1, Second = 2, Time = 10, Ticker = 5, Dir = VmIntermission1437Slice.ScrollUp };
        VmIntermission1437Slice.Draw(world, page);
        Assert.Equal(0d, world.Pics[0].X);
        Assert.Equal(240d, world.Pics[0].Y);
        Assert.Equal(-240d, world.Pics[1].Y);
        Assert.Equal(640d, world.Pics[0].W);

        world.Pics.Clear();
        page.Dir = VmIntermission1437Slice.ScrollDown;
        page.Background = 1;
        VmIntermission1437Slice.Draw(world, page);
        Assert.Equal(-240d, world.Pics[0].Y);
        Assert.Equal(240d, world.Pics[1].Y);
        Assert.Equal(2, page.Background);

        world.Pics.Clear();
        page.Dir = 9;
        VmIntermission1437Slice.Draw(world, page);
        Assert.Equal(240d, world.Pics[0].Y);

        var crop = VmIntermission1437Slice.Fit(new VmIntermission1437Slice.World { Width = 400, Height = 320 }, 384, 240);
        Assert.Equal(-40d, crop.Left);
        Assert.Equal(10d, crop.Top);
        Assert.Equal(480d, crop.Width);
        Assert.Equal(300d, crop.Height);
        var fill = VmIntermission1437Slice.Fit(new VmIntermission1437Slice.World { Width = 640, Height = 480 }, 384, 240);
        Assert.Equal(-64d, fill.Left);
        Assert.Equal(0d, fill.Top);
        Assert.Equal(768d, fill.Width);
        Assert.Equal(480d, fill.Height);
    }

    static VmIntermission1437Slice.Act Page(int cls) => new() { Class = cls };

    [Fact]
    public void NextPageCreatesAndLinks()
    {
        var world = new VmIntermission1437Slice.World { Net = true };
        var desc = new VmIntermission1437Slice.Desc
        {
            Id = 1,
            Actions = { new() { Class = VmIntermission1437Slice.Wiper, Wipe = 4 }, Page(8), Page(9) },
        };
        var ctrl = VmIntermission1437Slice.Ctor(desc, true, false);
        Assert.True(ctrl.First);
        Assert.False(ctrl.Advance);
        Assert.Null(ctrl.Screen);
        Assert.True(VmIntermission1437Slice.Next(world, ctrl));
        Assert.Equal(4, ctrl.Wipe);
        Assert.Equal(8, ctrl.Screen!.Class);
        Assert.Equal(0, ctrl.Screen.Background);
        Assert.True(ctrl.Screen.Inited);
        Assert.True(ctrl.Screen.FirstArg);
        Assert.False(ctrl.First);
        Assert.Equal(1, world.Cutscenes);
        ctrl.Screen.Background = 12;
        ctrl.Screen.Flat = true;
        var first = ctrl.Screen;
        Assert.True(VmIntermission1437Slice.Next(world, ctrl));
        Assert.True(first.Dead);
        Assert.Equal(9, ctrl.Screen!.Class);
        Assert.Equal(12, ctrl.Screen.Background);
        Assert.True(ctrl.Screen.Flat);
        Assert.False(ctrl.Screen.FirstArg);
        Assert.Equal(2, world.Cutscenes);

        world.Demo = true;
        var linked = new VmIntermission1437Slice.Desc { Id = 2, Actions = { Page(3) } };
        world.Links["Next"] = linked;
        var jump = new VmIntermission1437Slice.Desc { Id = 5, Link = "next" };
        var owned = VmIntermission1437Slice.Ctor(jump, true, true);
        Assert.True(VmIntermission1437Slice.Next(world, owned));
        Assert.Equal(new[] { 5 }, world.Deleted);
        Assert.False(owned.Delete);
        Assert.Same(linked, owned.Desc);
        Assert.Equal(3, owned.Screen!.Class);
        Assert.Equal(2, world.Cutscenes);
    }

    [Fact]
    public void NextPageStops()
    {
        var world = new VmIntermission1437Slice.World { Net = true };
        var done = new VmIntermission1437Slice.Desc { Id = 4 };
        var ctrl = VmIntermission1437Slice.Ctor(done, true, true);
        ctrl.Screen = new VmIntermission1437Slice.Screen { Class = 1, Background = 6 };
        Assert.True(VmIntermission1437Slice.Next(world, ctrl));
        Assert.False(ctrl.Screen.Dead);
        Assert.Empty(world.Deleted);
        ctrl.EndGame = false;
        Assert.False(VmIntermission1437Slice.Next(world, ctrl));
        Assert.False(ctrl.Screen.Dead);

        var titled = new VmIntermission1437Slice.Desc
        {
            Actions = { new() { Class = VmIntermission1437Slice.Wiper, Wipe = 2 }, new() { Class = VmIntermission1437Slice.Title } },
        };
        var show = VmIntermission1437Slice.Ctor(titled, false, false);
        show.Screen = new VmIntermission1437Slice.Screen { Class = 1 };
        Assert.False(VmIntermission1437Slice.Next(world, show));
        Assert.Equal(2, show.Wipe);
        Assert.Null(show.Screen);
        Assert.Equal(0, world.Cutscenes);

        var missing = new VmIntermission1437Slice.Desc { Id = 8, Link = "gone" };
        var lost = VmIntermission1437Slice.Ctor(missing, true, false);
        Assert.False(VmIntermission1437Slice.Next(world, lost));
        Assert.True(lost.Delete);
        Assert.Empty(world.Deleted);
        Assert.Throws<NullReferenceException>(() => VmIntermission1437Slice.Next(world, new VmIntermission1437Slice.Ctrl()));
    }
}
