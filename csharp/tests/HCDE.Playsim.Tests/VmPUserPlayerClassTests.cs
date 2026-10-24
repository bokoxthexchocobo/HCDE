namespace HCDE.Playsim.Tests;

public class VmPUserPlayerClassTests
{
    private static VmPUserPlayerClass.ActorClassInfo ValidPawn(string display, int flags6 = 0) =>
        new()
        {
            DisplayName = display,
            IsPlayerPawnDescendant = true,
            DefaultFlags6 = flags6,
        };

    [Fact]
    public void CheckSkin_FindsRegisteredSkin()
    {
        var pc = new VmPUserPlayerClass.PlayerClassModel();
        pc.Skins.Add(2);
        pc.Skins.Add(5);
        Assert.True(VmPUserPlayerClass.CheckSkin(pc, 5));
        Assert.False(VmPUserPlayerClass.CheckSkin(pc, 9));
    }

    [Fact]
    public void CheckSkinAction_ReturnsBool()
    {
        var pc = new VmPUserPlayerClass.PlayerClassModel();
        pc.Skins.Add(1);
        Assert.True(VmPUserPlayerClass.CheckSkinAction(pc, 1));
    }

    [Fact]
    public void GetPrintableDisplayName_DelegatesToClass()
    {
        var cls = new object();
        Assert.Equal(
            "Marine",
            VmPUserPlayerClass.GetPrintableDisplayName(cls, _ => "Marine"));
    }

    [Fact]
    public void ValidatePlayerClass_RejectsNull()
    {
        var lines = new List<string>();
        Assert.False(VmPUserPlayerClass.ValidatePlayerClass(null, "Foo", s => lines.Add(s)));
        Assert.Contains("Unknown player class 'Foo'", lines[0]);
    }

    [Fact]
    public void ValidatePlayerClass_RejectsNonPawn()
    {
        var info = new VmPUserPlayerClass.ActorClassInfo
        {
            DisplayName = "X",
            IsPlayerPawnDescendant = false,
        };
        Assert.False(VmPUserPlayerClass.ValidatePlayerClass(info, "Bar"));
    }

    [Fact]
    public void ValidatePlayerClass_RejectsEmptyDisplayName()
    {
        var info = new VmPUserPlayerClass.ActorClassInfo
        {
            DisplayName = "",
            IsPlayerPawnDescendant = true,
        };
        Assert.False(VmPUserPlayerClass.ValidatePlayerClass(info, "Baz"));
    }

    [Fact]
    public void ValidatePlayerClass_AcceptsValid()
    {
        Assert.True(VmPUserPlayerClass.ValidatePlayerClass(ValidPawn("Doom Guy"), "DoomPlayer"));
    }

    [Fact]
    public void SetupPlayerClasses_AppliesNomenuFlag()
    {
        var list = new List<VmPUserPlayerClass.PlayerClassModel>();
        VmPUserPlayerClass.SetupPlayerClasses(
            list,
            ["Hidden", "Visible"],
            name => name == "Hidden"
                ? ValidPawn("Hidden", VmPUserPlayerClass.Mf6Nomenu)
                : ValidPawn("Visible"));
        Assert.Equal(2, list.Count);
        Assert.Equal((uint)VmPUserPlayerClass.PcfNomenu, list[0].Flags);
        Assert.Equal(0u, list[1].Flags);
    }

    [Fact]
    public void SetupPlayerClasses_SkipsInvalid()
    {
        var list = new List<VmPUserPlayerClass.PlayerClassModel>();
        VmPUserPlayerClass.SetupPlayerClasses(
            list,
            ["Bad", "Good"],
            name => name == "Good" ? ValidPawn("Good") : null);
        Assert.Single(list);
        Assert.Equal("Good", list[0].Type is VmPUserPlayerClass.ActorClassInfo i ? i.DisplayName : null);
    }

    [Fact]
    public void DestroyPlayer_CallsDestroyPsprites()
    {
        var called = false;
        VmPUserPlayerClass.DestroyPlayer(new VmPUserPlayerClass.PlayerStateModel(), () => called = true);
        Assert.True(called);
    }

    [Fact]
    public void CopyFrom_CopiesScalarsAndFrags()
    {
        var src = new VmPUserPlayerClass.PlayerStateModel
        {
            Health = 80,
            FragCount = 3,
            LogText = "note",
            SoundClass = "marine",
        };
        src.Frags[2] = 7;
        src.AngleOffsetTargets.Yaw = 15;
        src.LastSafePos.ValidPos = true;
        var dst = new VmPUserPlayerClass.PlayerStateModel { UserInfoMarker = "keep" };
        VmPUserPlayerClass.CopyFrom(dst, src);
        Assert.Equal(80, dst.Health);
        Assert.Equal(7, dst.Frags[2]);
        Assert.Equal("note", dst.LogText);
        Assert.Equal(15, dst.AngleOffsetTargets.Yaw);
        Assert.True(dst.LastSafePos.ValidPos);
        Assert.Equal("keep", dst.UserInfoMarker);
    }

    [Fact]
    public void CopyFrom_TransfersPsprites()
    {
        var ps = new object();
        var src = new VmPUserPlayerClass.PlayerStateModel { Psprites = ps };
        var dst = new VmPUserPlayerClass.PlayerStateModel();
        VmPUserPlayerClass.CopyFrom(dst, src);
        Assert.Same(ps, dst.Psprites);
        Assert.Null(src.Psprites);
    }

    [Fact]
    public void PropagateMark_MarksPendingWeapon()
    {
        var pending = new object();
        var player = new VmPUserPlayerClass.PlayerStateModel { PendingWeapon = pending };
        var marked = new List<object?>();
        VmPUserPlayerClass.PropagateMark(player, o => marked.Add(o), 1024);
        Assert.Contains(pending, marked);
    }

    [Fact]
    public void PropagateMark_SkipsWpNochange()
    {
        var player = new VmPUserPlayerClass.PlayerStateModel
        {
            PendingWeapon = VmPUserPlayerClass.WpNoChange,
            Mo = new object(),
        };
        var marked = new List<object?>();
        var size = VmPUserPlayerClass.PropagateMark(player, o => marked.Add(o), 512);
        Assert.Equal((nuint)512, size);
        Assert.DoesNotContain(VmPUserPlayerClass.WpNoChange, marked);
        Assert.Single(marked);
    }
}
