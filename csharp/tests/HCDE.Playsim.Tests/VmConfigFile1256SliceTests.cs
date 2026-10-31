namespace HCDE.Playsim.Tests;

public class VmConfigFile1256SliceTests
{
    const string LinkGolden =
"""
ensure none
section new Game
entry Bind alloc=44 value=Yes
ensure add Bind=Yes
ensure hit Bind=Yes
entry bind alloc=44 value=No
ensure add bind=No
section keep Game
rename miss missing
rename GAME -> Play

""";

    const string LoadGolden =
"""
load closed
load existed=0
load existed=1
section new Auto
entry  alloc=40 value=blank

""";

    [Fact]
    public void Ensure_KeepsAnExistingValueAndReusesTheSection()
    {
        var vm = new VmConfigFile1256();
        vm.Ensure("Bind", "Yes");
        vm.Current = vm.NewSection("Game");
        vm.Ensure("Bind", "Yes");
        vm.Ensure("bind", "YES");
        vm.Ensure("bind", "No");
        var again = vm.NewSection("game");
        Assert.Same(again, vm.Current);
        vm.Rename("missing", "Other");
        vm.Rename("GAME", "Play");
        Assert.Null(vm.FindSection("Game"));
        Assert.Equal("Play", vm.FindSection("Play")!.Name);
        var first = vm.FindEntry(vm.Current, "BIND");
        Assert.Equal("Bind", first!.Key);
        Assert.Equal("Yes", first.Value);
        Assert.Equal(44, first.AllocBytes);
        Assert.Equal("bind", first.Next!.Key);
        Assert.Equal("No", first.Next.Value);
        Assert.Null(first.Next.Next);
        Assert.Equal(LinkGolden, string.Join('\n', vm.Log) + "\n");
        first.SetValue("Later");
        Assert.Equal("Later", first.Value);
    }

    [Fact]
    public void Load_SetsExistedOnlyAfterASuccessfulRead()
    {
        var vm = new VmConfigFile1256 { Path = "missing.cfg" };
        int reads = 0;
        vm.Open = _ => false;
        vm.Read = () => { reads++; return true; };
        vm.Load();
        Assert.False(vm.FileExisted);
        Assert.Equal(0, reads);
        vm.Path = "empty.cfg";
        vm.Open = _ => true;
        vm.Read = () => { reads++; return false; };
        vm.Load();
        Assert.False(vm.FileExisted);
        vm.Path = "ok.cfg";
        vm.Read = () => { reads++; return true; };
        vm.Load();
        Assert.True(vm.FileExisted);
        Assert.Equal(2, reads);
        var sec = vm.NewSection("Auto");
        var blank = vm.NewEntry(sec, "", "blank");
        Assert.Equal(40, blank.AllocBytes);
        Assert.Equal("", blank.Key);
        Assert.Equal(LoadGolden, string.Join('\n', vm.Log) + "\n");
    }
}
