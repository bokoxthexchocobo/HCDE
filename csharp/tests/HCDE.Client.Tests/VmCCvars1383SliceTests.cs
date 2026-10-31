using static HCDE.Client.VmCCvars1383Slice;

namespace HCDE.Client.Tests;

public class VmCCvars1383SliceTests
{
    [Fact]
    public void InitBuildsScalarsThenFlagAndMask()
    {
        var anchorSlice = new VmCCvars1383Slice();
        var anchorDecl = new Decl { Name = "dmflags", Type = Kind.Int, IntDefault = 1 };
        anchorSlice.Init(0, new[] { anchorDecl });
        var anchor = anchorDecl.Ref!;

        var plainLong = new string('a', 64);
        var flagLong = new string('f', 64);
        Action<CVar, string?> grav = (_, _) => { };
        var slice = new VmCCvars1383Slice();
        var decls = new[]
        {
            new Decl { Name = "notarget", Type = Kind.Flag, Flags = 1 << 5, Pointer = anchor, Description = "no target" },
            new Decl { Name = "nope", Type = Kind.Dummy, Flags = ServerInfo },
            new Decl { Name = null, Type = Kind.Int, Flags = Archive, IntDefault = 1, Description = "bare" },
            new Decl { Name = "secret", Type = Kind.Int, Flags = Hidden, IntDefault = 2 },
            new Decl { Name = "sv_cheats", Type = Kind.Bool, BoolDefault = false },
            new Decl { Name = "movebob", Type = Kind.Float, FloatDefault = 2.5f },
            new Decl { Name = "playername", Type = Kind.String, StringDefault = "doom" },
            new Decl { Name = "ui_color", Type = Kind.Color, IntDefault = 0x112233 },
            new Decl { Name = "sv_gravity", Type = Kind.Int, IntDefault = -4, Callback = grav, Description = "grav" },
            new Decl { Name = plainLong, Type = Kind.Int },
            new Decl { Name = "", Type = Kind.Int, IntDefault = 8 },
            new Decl { Name = flagLong, Type = Kind.Flag, Flags = ServerInfo, Pointer = anchor },
            new Decl { Name = "compatflags", Type = Kind.Mask, Flags = 1 << 3, Pointer = anchor },
        };

        slice.Init(41, decls);

        Assert.Equal(41, slice.LastWhich);
        Assert.Null(decls[1].Ref);
        var bare = decls[2].Ref!;
        Assert.Equal("", bare.VarName);
        Assert.Equal("bare", bare.Description);
        Assert.Equal(Archive | IsDefault, bare.Flags);
        Assert.Equal("1", bare.ValueText);
        Assert.DoesNotContain(bare, slice.Map.Values);
        Assert.Equal(11, slice.Map.Count);
        Assert.Null(slice.Find(null));
        Assert.Null(slice.Find(""));
        Assert.True(slice.Map.ContainsKey(""));
        Assert.Null(slice.Find("nope"));

        var secret = decls[3].Ref!;
        Assert.Equal(Hidden | IsDefault, secret.Flags);
        Assert.DoesNotContain("secret", slice.TabAdds);
        var cheats = decls[4].Ref!;
        Assert.Same(cheats, slice.Find("SV_CHEATS"));
        Assert.Equal("false", cheats.ValueText);
        Assert.False(cheats.DefaultBool);
        var bob = decls[5].Ref!;
        Assert.Equal("2.5", bob.ValueText);
        Assert.Equal(2.5f, bob.DefaultFloat);
        Assert.Equal("doom", decls[6].Ref!.ValueText);
        Assert.Equal("1122867", decls[7].Ref!.HumanString(4));
        var gravity = decls[8].Ref!;
        Assert.Same(grav, gravity.Callback);
        Assert.Equal("grav", gravity.Description);
        Assert.Equal("-4", gravity.HumanString(1));
        Assert.Equal("-4", gravity.HumanStringDefault(9));
        Assert.Equal(0, gravity.DoSetCount);
        Assert.Equal(IsDefault, gravity.Flags);
        Assert.Same(decls[9].Ref, slice.Find(plainLong));
        Assert.Equal(64, decls[9].Ref!.VarName.Length);

        var notarget = decls[0].Ref!;
        Assert.Equal(Kind.Flag, notarget.Kind);
        Assert.Equal(0, notarget.Flags);
        Assert.Equal(1 << 5, notarget.BitValue);
        Assert.Same(anchor, notarget.Referenced);
        Assert.Null(notarget.Callback);
        Assert.Equal("no target", notarget.Description);
        var longFlag = decls[11].Ref!;
        Assert.Equal(64, longFlag.VarName.Length);
        Assert.Equal(0, longFlag.Flags);
        Assert.Equal(ServerInfo, longFlag.BitValue);
        Assert.Equal(Kind.Mask, decls[12].Ref!.Kind);
        Assert.Equal(1 << 3, decls[12].Ref!.BitValue);
        Assert.Equal(0, decls[12].Ref!.Flags);

        Assert.Equal(new[]
        {
            "sv_cheats",
            "movebob",
            "playername",
            "ui_color",
            "sv_gravity",
            plainLong,
            "",
            "notarget",
            flagLong,
            "compatflags",
        }, slice.TabAdds);
        Assert.Empty(slice.CallbackNotes);
        Assert.Empty(slice.UserInfoNotes);

        var ok = new VmCCvars1383Slice();
        var serverName = new string('s', 63);
        var server = new Decl { Name = serverName, Type = Kind.String, Flags = ServerInfo | UserInfo, StringDefault = "ok" };
        ok.Init(1, new[] { server });
        Assert.Equal(serverName, server.Ref!.VarName);
        Assert.Equal(ServerInfo | UserInfo | IsDefault, server.Ref.Flags);

        var fatal = new VmCCvars1383Slice();
        var tooLong = new string('s', 64);
        var ex = Assert.Throws<ServerNameTooLongException>(() =>
            fatal.Init(5, new[]
            {
                new Decl { Name = tooLong, Type = Kind.Int, Flags = ServerInfo, IntDefault = 1 },
            }));
        Assert.Equal(
            $"Name of the server console variable \"{tooLong}\" is too long.\n" +
            "Its length should not exceed 63 characters.\n",
            ex.Message);
        Assert.Equal(5, fatal.LastWhich);
        Assert.Empty(fatal.Map);
        Assert.Empty(fatal.TabAdds);
    }

    [Fact]
    public void ReplaceKeepsTheOldValueAndForceSetUpdatesUserInfo()
    {
        var seen = new List<string?>();
        var slice = new VmCCvars1383Slice();
        slice.UseCallback = true;
        slice.InstallHandlers(new Handlers { UserInfoChanged = _ => { } });
        var first = new Decl
        {
            Name = "Gravity",
            Type = Kind.Int,
            IntDefault = 9,
            Callback = (_, _) => seen.Add("first"),
        };
        var second = new Decl
        {
            Name = "gravity",
            Type = Kind.Int,
            IntDefault = 3,
            Flags = UserInfo | Archive,
            Description = "g",
            Callback = (_, previous) => seen.Add(previous),
        };
        slice.Init(0, new[] { first, second });

        var live = second.Ref!;
        Assert.True(first.Ref!.Destroyed);
        Assert.Same(live, slice.Find("GRAVITY"));
        Assert.Equal("9", live.ValueText);
        Assert.Equal(3, live.DefaultInt);
        Assert.Equal("9", live.HumanString(0));
        Assert.Equal("9", live.HumanString(6));
        Assert.Equal("3", live.HumanStringDefault(2));
        Assert.Equal(UserInfo | Archive, live.Flags);
        Assert.Null(live.SafeValue);
        Assert.Empty(slice.UserInfoNotes);
        Assert.Equal(new string?[] { null }, seen);
        Assert.Equal(new[] { "Gravity", "gravity" }, slice.TabAdds);
        Assert.Empty(slice.TabRemoves);
        Assert.Equal(1, live.DoSetCount);
        Assert.Equal(Kind.Int, live.LastSetType);

        slice.ForceSet(live, "1", Kind.Int, noUserInfoSend: false);
        Assert.Equal(new[] { live }, slice.UserInfoNotes);
        Assert.Equal("1", live.SafeValue);
        Assert.Equal(new string?[] { null, "9" }, seen);

        live.Flags |= Ignore;
        slice.ForceSet(live, "2", Kind.Int, noUserInfoSend: false);
        Assert.Single(slice.UserInfoNotes);
        Assert.Equal("2", live.SafeValue);

        live.Flags |= UnsafeContext;
        slice.ForceSet(live, "3", Kind.Int, noUserInfoSend: false);
        Assert.Equal("2", live.SafeValue);
        Assert.Equal(0, live.Flags & (IsDefault | UnsafeContext));
        Assert.NotEqual(0, live.Flags & Ignore);

        live.Flags &= ~Ignore;
        slice.ForceSet(live, "4", Kind.Int, noUserInfoSend: true);
        Assert.Single(slice.UserInfoNotes);
        Assert.Equal("4", live.SafeValue);

        slice.InstallHandlers(null);
        slice.ForceSet(live, "5", Kind.Int, noUserInfoSend: false);
        Assert.Single(slice.UserInfoNotes);
        Assert.Null(slice.Callbacks);

        slice.InstallHandlers(new Handlers());
        slice.ForceSet(live, "8", Kind.Int, noUserInfoSend: false);
        Assert.Single(slice.UserInfoNotes);

        live.ClearCallback();
        slice.ForceSet(live, "6", Kind.Int, noUserInfoSend: true);
        Assert.Equal(new string?[] { null, "9", "1", "2", "3", "4", "5" }, seen);

        var marker = new object();
        live.SetCallback((_, _) => seen.Add("again"));
        live.SetExtra(marker);
        live.SetExtra2("two");
        Assert.Same(marker, live.GetExtra());
        Assert.Equal("two", live.GetExtra2());
        slice.UseCallback = false;
        slice.ForceSet(live, "7", Kind.Int, noUserInfoSend: true);
        Assert.Equal(new string?[] { null, "9", "1", "2", "3", "4", "5" }, seen);
        Assert.Equal("7", live.ValueText);

        slice.Destroy(live);
        Assert.False(slice.Map.ContainsKey("gravity"));
        Assert.Equal(new[] { "gravity" }, slice.TabRemoves);

        var emptied = new VmCCvars1383Slice();
        var blank = new Decl { Name = "", Type = Kind.Int, IntDefault = 8 };
        emptied.Init(0, new[] { blank });
        emptied.Destroy(blank.Ref!);
        Assert.True(blank.Ref!.Destroyed);
        Assert.Same(blank.Ref, emptied.Map[""]);
        Assert.Empty(emptied.TabRemoves);
        Assert.Equal(new[] { "" }, emptied.TabAdds);

        var shutdown = new VmCCvars1383Slice();
        var alpha = new Decl { Name = "alpha", Type = Kind.Int, IntDefault = 1 };
        var beta = new Decl { Name = "beta", Type = Kind.Bool, BoolDefault = true };
        shutdown.Init(0, new[] { alpha, beta });
        shutdown.Uninit();
        Assert.Null(shutdown.Map["alpha"]);
        Assert.Null(shutdown.Map["beta"]);
        Assert.True(alpha.Ref!.Destroyed);
        Assert.True(beta.Ref!.Destroyed);
        Assert.Empty(shutdown.TabRemoves);
        Assert.Equal(new[] { "alpha", "beta" }, shutdown.TabAdds);

        var mirror = new VmCCvars1383Slice();
        mirror.UseCallback = true;
        CVar? self = null;
        var nested = 0;
        var mirrorDecl = new Decl
        {
            Name = "mirror",
            Type = Kind.Int,
            IntDefault = 1,
            Flags = Virtual | Archive,
            Callback = (cvar, previous) =>
            {
                nested++;
                Assert.Equal("1", previous);
                if (nested == 1)
                    mirror.ForceSet(cvar, "nested", Kind.Int, noUserInfoSend: false);
            },
        };
        mirror.Init(0, new[] { mirrorDecl });
        self = mirrorDecl.Ref;
        mirror.ForceSet(self!, "outer", Kind.Int, noUserInfoSend: false);
        Assert.Equal(1, nested);
        Assert.Equal("nested", self!.ValueText);
        Assert.Equal("nested", self.SafeValue);
        Assert.Equal("nested", self.HumanString(3));
        Assert.Equal(0, self.Flags & IsDefault);
        Assert.NotEqual(0, self.Flags & Virtual);
    }
}
