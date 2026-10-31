namespace HCDE.Playsim.Tests;

public class VmSharedHud1195SliceTests
{
    [Fact]
    public void Reset_RegistersTheArchiveCvarsWithoutCallingTheCallback()
    {
        VmSharedHud1195Slice.Reset();
        var bar = new VmSharedHud1195Slice.Bar();
        VmSharedHud1195Slice.StatusBar = bar;
        VmSharedHud1195Slice.Reset();
        Assert.Null(bar.AltHud);
        Assert.Empty(VmSharedHud1195Slice.Calls);
        Assert.Equal(36, VmSharedHud1195Slice.Declarations.Count);
        Assert.Equal(1, VmSharedHud1195Slice.Archive);
        Assert.Equal(512, VmSharedHud1195Slice.NoInitCall);
        Assert.Equal(1024, VmSharedHud1195Slice.GlobalConfig);
        Assert.Equal(5, VmSharedHud1195Slice.CrGold);
        Assert.Equal(10, VmSharedHud1195Slice.CrYellow);
        Assert.Equal(6, VmSharedHud1195Slice.CrRed);
        Assert.Equal(8, VmSharedHud1195Slice.CrOrange);
        Assert.Equal(3, VmSharedHud1195Slice.CrGreen);
        Assert.All(VmSharedHud1195Slice.Declarations, decl => Assert.True((decl.Flags & VmSharedHud1195Slice.Archive) != 0));

        var scale = Find("hud_althudscale");
        Assert.Equal(VmSharedHud1195Slice.TypeInt, scale.Type);
        Assert.Equal(0, scale.Int);
        Assert.Equal(1, scale.Flags);
        Assert.False(Find("hud_althud").Bool);
        Assert.True(Find("hud_showsecrets").Bool);
        Assert.False(Find("hud_showitems").Bool);
        Assert.Equal(2, Find("hud_showammo").Int);
        Assert.Equal(VmSharedHud1195Slice.CrGold, Find("hud_timecolor").Int);
        Assert.Equal(25, Find("hud_ammo_red").Int);
        Assert.Equal(50, Find("hud_health_yellow").Int);
        Assert.Equal(100, Find("hud_armor_green").Int);
        Assert.True(Find("hud_berserk_health").Bool);
        Assert.Equal(VmSharedHud1195Slice.CrYellow, Find("hudcolor_titl").Int);
        Assert.Equal(VmSharedHud1195Slice.CrGreen, Find("hudcolor_stats").Int);
        var map = Find("map_point_coordinates");
        Assert.True(map.Bool);
        Assert.Equal(VmSharedHud1195Slice.Archive | VmSharedHud1195Slice.GlobalConfig, map.Flags);
        var force = Find("hud_althud_forceinternal");
        Assert.False(force.Bool);
        Assert.Equal(VmSharedHud1195Slice.Archive | VmSharedHud1195Slice.NoInitCall, force.Flags);
        Assert.Equal("hud_althud_forceinternal", VmSharedHud1195Slice.Declarations[^1].Name);
    }

    [Fact]
    public void CreateAltHUD_FallsBackAndTheCallbackRebuilds()
    {
        VmSharedHud1195Slice.Reset();
        var bar = new VmSharedHud1195Slice.Bar();
        VmSharedHud1195Slice.StatusBar = bar;
        VmSharedHud1195Slice.GameAltHudClass = "Missing";
        bar.CreateAltHUD();
        Assert.Equal("\u001cGUnknown alternative HUD class \"Missing\"\n", VmSharedHud1195Slice.Messages[0]);
        Assert.Equal("AltHud", bar.AltHud!.Class.Name);
        Assert.Equal(new[] { "Init AltHud" }, VmSharedHud1195Slice.Calls);
        Assert.False(bar.AltHud.Destroyed);

        var other = new VmSharedHud1195Slice.Class { Name = "Widget" };
        VmSharedHud1195Slice.Classes["Widget"] = other;
        VmSharedHud1195Slice.GameAltHudClass = "Widget";
        var previous = bar.AltHud;
        bar.CreateAltHUD();
        Assert.True(previous.Destroyed);
        Assert.Contains("\u001cGAlternative HUD class \"Widget\" is not derived from AltHud\n", VmSharedHud1195Slice.Messages);
        Assert.Equal("AltHud", bar.AltHud.Class.Name);

        var custom = new VmSharedHud1195Slice.Class
        {
            Name = "MyHud",
            Parent = VmSharedHud1195Slice.Classes["AltHud"],
            Init = true,
            Draw = true
        };
        VmSharedHud1195Slice.Classes["MyHud"] = custom;
        VmSharedHud1195Slice.GameAltHudClass = "MyHud";
        VmSharedHud1195Slice.Calls.Clear();
        bar.CreateAltHUD();
        Assert.Equal("MyHud", bar.AltHud.Class.Name);
        Assert.Equal(new[] { "Init MyHud" }, VmSharedHud1195Slice.Calls);

        VmSharedHud1195Slice.GameAltHudClass = "None";
        bar.CreateAltHUD();
        Assert.Equal("AltHud", bar.AltHud.Class.Name);

        VmSharedHud1195Slice.ForceInternal = true;
        VmSharedHud1195Slice.GameAltHudClass = "MyHud";
        VmSharedHud1195Slice.Calls.Clear();
        bar.CreateAltHUD();
        Assert.Equal("AltHud", bar.AltHud.Class.Name);
        Assert.Equal(new[] { "Init AltHud" }, VmSharedHud1195Slice.Calls);

        VmSharedHud1195Slice.Classes.Remove("AltHud");
        var error = Assert.Throws<VmSharedHud1195Slice.FatalError>(() => bar.CreateAltHUD());
        Assert.Equal("AltHud", error.Detail);
        VmSharedHud1195Slice.Classes["AltHud"] = new VmSharedHud1195Slice.Class { Name = "AltHud", Init = true, Draw = true };
        VmSharedHud1195Slice.SetForceInternal(false);
        Assert.Equal("MyHud", bar.AltHud.Class.Name);
        VmSharedHud1195Slice.StatusBar = null;
        VmSharedHud1195Slice.SetForceInternal(true);
        Assert.True(VmSharedHud1195Slice.ForceInternal);
    }

    [Fact]
    public void DrawAltHUD_SizesFromTheScalePathAndClearsInventoryTics()
    {
        VmSharedHud1195Slice.Reset();
        var local = new VmSharedHud1195Slice.Player { InventoryTics = 9 };
        var global = new VmSharedHud1195Slice.Player();
        VmSharedHud1195Slice.Players[0] = global;
        var bar = new VmSharedHud1195Slice.Bar
        {
            CPlayer = local,
            Screen = new VmSharedHud1195Slice.Drawer { Width = 1280, Height = 720 }
        };
        bar.CreateAltHUD();
        VmSharedHud1195Slice.StatusBar = bar;
        var seen = new List<int>();
        VmSharedHud1195Slice.GetUIScale = (drawer, scale) =>
        {
            seen.Add(drawer.Width);
            seen.Add(scale);
            return 4;
        };
        VmSharedHud1195Slice.OldScale = true;
        VmSharedHud1195Slice.AltHudScale = 3;
        VmSharedHud1195Slice.AspectScale = true;
        global.InventoryTics = 12;
        VmSharedHud1195Slice.Calls.Clear();
        bar.DrawAltHUD();
        Assert.Equal(new[] { 1280, 3 }, seen);
        Assert.Equal(0, global.InventoryTics);
        Assert.Equal(9, local.InventoryTics);
        Assert.Equal("Draw AltHud 320 150 local", VmSharedHud1195Slice.Calls[^1]);

        VmSharedHud1195Slice.OldScale = false;
        VmSharedHud1195Slice.ScaleFactor = 1.6;
        VmSharedHud1195Slice.AspectScale = false;
        bar.DrawAltHUD();
        Assert.Equal("Draw AltHud 400 225 local", VmSharedHud1195Slice.Calls[^1]);
        VmSharedHud1195Slice.AspectScale = true;
        bar.DrawAltHUD();
        Assert.Equal("Draw AltHud 400 187 local", VmSharedHud1195Slice.Calls[^1]);

        var other = new VmSharedHud1195Slice.Bar
        {
            CPlayer = new VmSharedHud1195Slice.Player(),
            Screen = bar.Screen,
            AltHud = bar.AltHud
        };
        VmSharedHud1195Slice.Calls.Clear();
        other.DrawAltHUD();
        Assert.Equal("Draw AltHud 400 187 global", VmSharedHud1195Slice.Calls[0]);
        Assert.Equal(2, seen.Count);

        bar.AltHud!.Class.Draw = false;
        if (bar.AltHud.Class.Parent != null)
            bar.AltHud.Class.Parent.Draw = false;
        VmSharedHud1195Slice.Calls.Clear();
        bar.DrawAltHUD();
        Assert.Empty(VmSharedHud1195Slice.Calls);
        Assert.Equal(0, global.InventoryTics);
    }

    static VmSharedHud1195Slice.Decl Find(string name)
    {
        foreach (var decl in VmSharedHud1195Slice.Declarations)
        {
            if (decl.Name == name)
                return decl;
        }

        throw new InvalidOperationException(name);
    }
}
