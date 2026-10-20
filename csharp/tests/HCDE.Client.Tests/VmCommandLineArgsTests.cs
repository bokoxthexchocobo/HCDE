namespace HCDE.Client.Tests;

public class VmCommandLineArgsTests
{
    [Fact]
    public void CheckParm_FindsCaseInsensitiveFlag()
    {
        var args = new VmCommandLineArgs(["prog", "-warp", "map01"]);
        Assert.Equal(1, args.CheckParm("-warp"));
    }

    [Fact]
    public void CheckValue_ReturnsFollowingToken()
    {
        var args = new VmCommandLineArgs(["prog", "-skill", "4"]);
        Assert.Equal("4", args.CheckValue("-skill"));
    }

    [Fact]
    public void CheckParmList_CollectsTrailingValues()
    {
        var args = new VmCommandLineArgs(["prog", "-file", "a.wad", "b.wad", "-warp", "1"]);
        var count = args.CheckParmList("-file", out var values);
        Assert.Equal(2, count);
        Assert.Equal(["a.wad", "b.wad"], values);
    }

    [Fact]
    public void TakeValue_RemovesFlagAndValue()
    {
        var args = new VmCommandLineArgs(["prog", "-map", "e1m1", "-skill", "2"]);
        Assert.Equal("e1m1", args.TakeValue("-map"));
        Assert.Equal(3, args.NumArgs);
        Assert.Null(args.CheckValue("-map"));
    }

    [Fact]
    public void RemoveArg_DropsIndex()
    {
        var args = new VmCommandLineArgs(["a", "b", "c"]);
        args.RemoveArg(1);
        Assert.Equal(["a", "c"], args.Arguments);
    }

    [Fact]
    public void AppendRawArgsString_InsertsFileArguments()
    {
        var args = new VmCommandLineArgs(["prog"]);
        args.AppendRawArgsString("pwad1.wad pwad2.wad");
        Assert.Contains("pwad1.wad", args.Arguments);
    }
}
