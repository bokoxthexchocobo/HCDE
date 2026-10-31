namespace HCDE.Net.Core.Tests;

public class VmServerMode1373SliceTests
{
    [Fact]
    public void InitFromArgsParsesStrictValuesAndFatals()
    {
        var cold = new VmServerMode1373Slice(dedicatedExecutable: true);
        Assert.False(cold.Initialized);
        Assert.False(cold.DedicatedExecutable);
        Assert.True(cold.AuthorityPlayerBacked);
        Assert.Equal(1, cold.VisibleSlots);
        Assert.Equal(1, cold.InternalSlots);
        cold.InitFromArgs();
        Assert.False(cold.Initialized);
        Assert.True(cold.DedicatedExecutable);
        Assert.Empty(cold.Traces);
        Assert.Empty(cold.Fatals);

        var parsed = With(
            false,
            "-server", "4",
            "-host",
            "-port", "5029",
            "+map", "MAP05",
            "-join", "10.0.0.2",
            "-iwad", "doom2.wad",
            "-netwaitsilent");
        parsed.InitFromArgs();
        Assert.True(parsed.Initialized);
        Assert.False(parsed.DedicatedExecutable);
        Assert.True(parsed.DedicatedServer);
        Assert.True(parsed.HostMode);
        Assert.True(parsed.JoinMode);
        Assert.False(parsed.DedicatedJoin);
        Assert.True(parsed.NetWaitSilent);
        Assert.Equal(4, parsed.RequestedServerSlots);
        Assert.Equal(5029, parsed.GamePort);
        Assert.Equal("doom2.wad", parsed.IwadArg);
        Assert.Equal("MAP05", parsed.MapArg);
        Assert.Equal("10.0.0.2", parsed.JoinAddress);
        Assert.Equal("startup", parsed.FlowState);
        var init = Assert.Single(parsed.Traces);
        Assert.Equal("Infof", init.Level);
        Assert.Equal("serv", init.Category);
        Assert.Equal(VmServerMode1373Slice.InitFormat, init.Format);
        Assert.Equal(
            "init dedicated-exe=0 dedicated-srv=1 host=1 join=1 slots=4 port=5029 map=MAP05",
            init.Text);
        parsed.Args = ["hcdeserv", "-server", "9"];
        parsed.InitFromArgs();
        Assert.Equal(4, parsed.RequestedServerSlots);
        Assert.Single(parsed.Traces);

        Assert.Equal(10, VmServerMode1373Slice.StrictBase10("010"));
        Assert.Equal(8, VmServerMode1373Slice.StrictBase10("08"));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("0x2A"));
        Assert.Equal(12, VmServerMode1373Slice.StrictBase10(" 12"));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("12 "));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("2147483648"));
        Assert.Equal(int.MaxValue, VmServerMode1373Slice.StrictBase10("2147483647"));
        Assert.Equal(int.MinValue, VmServerMode1373Slice.StrictBase10("-2147483648"));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("-2147483649"));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10(""));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("   "));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("nope"));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("1.5"));
        Assert.Equal(8, VmServerMode1373Slice.StrictBase10("+8"));
        Assert.Equal(0, VmServerMode1373Slice.StrictBase10("999999999999999999999"));
        Assert.Equal(-5, VmServerMode1373Slice.StrictBase10(" -5"));

        var malformed = With(false, "-SERVER", "2abc", "-port", "-5", "-port", "+5");
        malformed.InitFromArgs();
        Assert.True(malformed.DedicatedServer);
        Assert.Equal(0, malformed.RequestedServerSlots);
        Assert.Equal(0, malformed.GamePort);
        Assert.Equal("default", TextMap(malformed));

        var spaced = With(false, "-port", " -5");
        Assert.Equal(-5, spaced.ReadIntValue("-port"));
        var first = With(false, "-server", "2", "-server", "9");
        Assert.Equal(2, first.ReadIntValue("-server"));

        var warp = With(false, "-warp", "e1", "m1");
        Assert.Equal("e1 m1", warp.ReadWarpValue());
        var oneWarp = With(false, "-warp", "only", "-file");
        Assert.Equal("only", oneWarp.ReadWarpValue());
        var emptySecond = With(false, "-warp", "1", "");
        Assert.Equal("1 ", emptySecond.ReadWarpValue());
        var bareWarp = With(false, "-warp");
        Assert.Equal("", bareWarp.ReadWarpValue());

        var mapWins = With(false, "+MAP", "E1M1", "-warp", "1", "2");
        mapWins.InitFromArgs();
        Assert.Equal("E1M1", mapWins.MapArg);
        var dashMap = With(false, "-map", "MAP01", "-warp", "3", "4");
        dashMap.InitFromArgs();
        Assert.Equal("3 4", dashMap.MapArg);
        var mapThenWarp = With(false, "+map", "-warp", "9", "8");
        mapThenWarp.InitFromArgs();
        Assert.Equal("9 8", mapThenWarp.MapArg);

        var joinChain = With(false, "-join", "-notahost", "-joindedicated", "10.0.0.5", "-dedicatedjoin", "9.9.9.9");
        joinChain.InitFromArgs();
        Assert.True(joinChain.JoinMode);
        Assert.True(joinChain.DedicatedJoin);
        Assert.Equal("10.0.0.5", joinChain.JoinAddress);
        var dedicatedOnly = With(false, "-dedicatedjoin", "9.9.9.9");
        dedicatedOnly.InitFromArgs();
        Assert.False(dedicatedOnly.JoinMode);
        Assert.True(dedicatedOnly.DedicatedJoin);
        Assert.Equal("9.9.9.9", dedicatedOnly.JoinAddress);
        var flagOnly = With(false, "-joindedicated");
        flagOnly.InitFromArgs();
        Assert.True(flagOnly.DedicatedJoin);
        Assert.Equal("", flagOnly.JoinAddress);

        var emptyIwad = With(false, "-iwad", "");
        Assert.True(emptyIwad.IsValueArg(2));
        Assert.Null(emptyIwad.CheckValue("-iwad"));
        Assert.Equal("", emptyIwad.AssignArgValue("-iwad"));
        var nullSlot = new VmServerMode1373Slice { Args = ["hcdeserv", null, "-server"] };
        Assert.False(nullSlot.IsValueArg(1));
        Assert.Equal(2, nullSlot.CheckParm("-server"));

        Assert.Equal("yes", VmServerMode1373Slice.YesNo(true));
        Assert.Equal("no", VmServerMode1373Slice.YesNo(false));
        Assert.Equal("fallback", VmServerMode1373Slice.TextOr("", "fallback"));
        Assert.Equal("kept", VmServerMode1373Slice.TextOr("kept", "fallback"));

        var fatal = With(true, "-host");
        var ex = Assert.Throws<InvalidOperationException>(fatal.InitFromArgs);
        Assert.Equal(VmServerMode1373Slice.DedicatedFatal, ex.Message);
        Assert.Equal(VmServerMode1373Slice.DedicatedFatal, Assert.Single(fatal.Fatals));
        Assert.True(fatal.Initialized);
        Assert.True(fatal.DedicatedExecutable);
        Assert.False(fatal.DedicatedServer);
        Assert.True(fatal.HostMode);
        Assert.Equal("startup", fatal.FlowState);
        Assert.Equal(
            "init dedicated-exe=1 dedicated-srv=0 host=1 join=0 slots=0 port=0 map=default",
            Assert.Single(fatal.Traces).Text);
        fatal.InitFromArgs();
        Assert.Single(fatal.Traces);

        var served = With(true, "-server", "3");
        served.InitFromArgs();
        Assert.True(served.DedicatedServer);
        Assert.Equal(3, served.RequestedServerSlots);
        Assert.Empty(served.Fatals);

        var ignoredProgram = new VmServerMode1373Slice { Args = ["-server", "-host"] };
        ignoredProgram.InitFromArgs();
        Assert.False(ignoredProgram.DedicatedServer);
        Assert.True(ignoredProgram.HostMode);
        Assert.Equal(0, ignoredProgram.RequestedServerSlots);
        var missingFlagReadsArgv1 = With(false, "4");
        Assert.Equal(0, missingFlagReadsArgv1.CheckParm("-server"));
        Assert.Equal(4, missingFlagReadsArgv1.ReadIntValue("-server"));
    }

    [Fact]
    public void SettersRecordNetworkMasterAndAuthority()
    {
        var bare = new VmServerMode1373Slice(dedicatedExecutable: true);
        bare.SetSelectedIwad(null);
        Assert.False(bare.Initialized);
        Assert.True(bare.DedicatedExecutable);
        Assert.Equal("", bare.SelectedIwad);
        Assert.Empty(bare.Traces);
        bare.SetSelectedIwad("freedoom2.wad");
        Assert.Equal("freedoom2.wad", bare.SelectedIwad);

        var slice = With(false, "-server", "4", "-port", "1", "-host", "-netwaitsilent", "-dedicatedjoin", "10.1.1.1", "+map", "MAP02");
        slice.SetSelectedIwad("doom2.wad");
        Assert.True(slice.Initialized);
        Assert.True(slice.DedicatedServer);
        Assert.True(slice.HostMode);
        Assert.True(slice.NetWaitSilent);
        Assert.True(slice.DedicatedJoin);
        Assert.Equal("10.1.1.1", slice.JoinAddress);
        Assert.Equal("MAP02", slice.MapArg);
        Assert.Equal(4, slice.RequestedServerSlots);
        Assert.Equal("doom2.wad", slice.SelectedIwad);
        Assert.Equal(
            "init dedicated-exe=0 dedicated-srv=1 host=1 join=0 slots=4 port=1 map=MAP02",
            slice.Traces[0].Text);

        slice.Args = ["hcdeserv", "-server", "99"];
        slice.SetNetworkDetails(8, 9, 5029, true, null);
        Assert.Equal(4, slice.RequestedServerSlots);
        Assert.Equal(8, slice.VisibleSlots);
        Assert.Equal(9, slice.InternalSlots);
        Assert.Equal(5029, slice.GamePort);
        Assert.True(slice.ReservedServerSlot);
        Assert.Equal("", slice.FlowState);
        Assert.Equal(VmServerMode1373Slice.NetworkFormat, slice.Traces[1].Format);
        Assert.Equal("Infof", slice.Traces[1].Level);
        Assert.Equal("network visible=8 internal=9 port=5029 reserved=1 flow=?", slice.Traces[1].Text);

        slice.SetNetworkDetails(2, 2, 3, false, "lobby");
        Assert.Equal("network visible=2 internal=2 port=3 reserved=0 flow=lobby", slice.Traces[2].Text);

        slice.SetMasterState(false, 2);
        Assert.True(slice.MasterStateKnown);
        Assert.False(slice.MasterAdvertisingEnabled);
        Assert.Equal(2ul, slice.ConfiguredMasters);
        Assert.Equal(VmServerMode1373Slice.MasterFormat, slice.Traces[3].Format);
        Assert.Equal("master advertising=0 configured=2", slice.Traces[3].Text);

        slice.SetAuthorityState(3, false, true, true, null);
        Assert.True(slice.AuthorityStateKnown);
        Assert.Equal(3, slice.AuthoritySlot);
        Assert.False(slice.AuthorityPlayerBacked);
        Assert.True(slice.AuthoritySettingsController);
        Assert.True(slice.AuthorityWaiting);
        Assert.Equal("", slice.AuthorityName);
        Assert.Equal("Markf", slice.Traces[4].Level);
        Assert.Equal("serv", slice.Traces[4].Category);
        Assert.Equal(VmServerMode1373Slice.AuthorityFormat, slice.Traces[4].Format);
        Assert.Equal(
            "authority slot=3 playerBacked=0 settings=1 waiting=1 name=unknown",
            slice.Traces[4].Text);

        slice.SetAuthorityState(1, true, false, false, "Ada");
        Assert.Equal("Ada", slice.AuthorityName);
        Assert.Equal(
            "authority slot=1 playerBacked=1 settings=0 waiting=0 name=Ada",
            slice.Traces[5].Text);
        Assert.Equal(6, slice.Traces.Count);

        var aborted = With(true, "-host");
        var ex = Assert.Throws<InvalidOperationException>(() => aborted.SetAuthorityState(1, true, false, false, "Ada"));
        Assert.Equal(VmServerMode1373Slice.DedicatedFatal, ex.Message);
        Assert.False(aborted.AuthorityStateKnown);
        Assert.Equal(0, aborted.AuthoritySlot);
        Assert.True(aborted.AuthorityPlayerBacked);
        Assert.Equal("", aborted.AuthorityName);
    }

    static string TextMap(VmServerMode1373Slice slice) => slice.Traces[0].Text.Split("map=")[1];

    static VmServerMode1373Slice With(bool dedicated, params string?[] rest)
    {
        var argv = new string?[rest.Length + 1];
        argv[0] = "hcdeserv";
        rest.CopyTo(argv, 1);
        return new VmServerMode1373Slice(dedicated) { Args = argv };
    }
}
