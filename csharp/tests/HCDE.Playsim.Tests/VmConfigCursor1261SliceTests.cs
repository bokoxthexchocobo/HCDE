namespace HCDE.Playsim.Tests;

public class VmConfigCursor1261SliceTests
{
    const string WalkGolden =
"""
first none
next none
section miss Nope
create Game
section Game -
section Game -
entry Bind Yes
entry Extra 1
section Game Bind
item Bind Yes
item Extra 1
item none
next none

""";

    const string EditGolden =
"""
delete none
create Game
section Game -
entry Bind Yes
entry bind no
entry Tail z
create Audio
section Audio -
entry Vol 3
create Video
section Video -
entry Mode 1
section Game Bind
key miss missing
key drop Bind
key drop Tail
entry More 9
section Game bind
item bind no
clear bind
clear More
clear root
section Audio Vol
clear Vol
clear root
drop Audio Video
clear Mode
clear root
drop Video none
first Game
clear root

""";

    [Fact]
    public void Walk_ResetsTheCursorAndLeavesItOnAFailedAdvance()
    {
        var vm = new VmConfigCursor1261();
        var stay = new VmConfigCursor1261.Entry { Key = "Stay", Value = "1" };
        vm.CurrentEntry = stay;
        Assert.False(vm.SetFirst());
        Assert.Null(vm.Current);
        Assert.Same(stay, vm.CurrentEntry);
        Assert.False(vm.SetNext());
        Assert.Same(stay, vm.CurrentEntry);
        Assert.Null(vm.CurrentName());
        Assert.True(vm.SectionIsEmpty());
        Assert.False(vm.SetSection("Nope", false));
        Assert.Null(vm.Current);
        Assert.Same(stay, vm.CurrentEntry);

        Assert.True(vm.SetSection("Game", true));
        Assert.Equal("Game", vm.CurrentName());
        Assert.Null(vm.CurrentEntry);
        Assert.Same(vm.Sections, vm.Tail);
        Assert.True(vm.SetSection("game", false));
        Assert.Same(vm.Sections, vm.Current);
        Assert.Null(vm.Sections!.Next);
        vm.Add("Bind", "Yes");
        vm.Add("Extra", "1");
        Assert.Null(vm.CurrentEntry);
        Assert.True(vm.SetSection("GAME", false));
        Assert.Equal("Bind", vm.CurrentEntry!.Key);
        Assert.Equal("1", vm.GetValue("EXTRA"));
        Assert.Null(vm.GetValue("missing"));
        Assert.Equal("Bind", vm.CurrentEntry.Key);
        Assert.True(vm.Next(out string key, out string? value));
        Assert.Equal("Bind", key);
        Assert.Equal("Yes", value);
        Assert.True(vm.Next(out key, out value));
        Assert.Equal("Extra", key);
        Assert.Equal("1", value);
        Assert.False(vm.Next(out key, out value));
        Assert.Equal("", key);
        Assert.Null(value);
        Assert.False(vm.SectionIsEmpty());
        Assert.False(vm.SetNext());
        Assert.Null(vm.Current);
        Assert.Null(vm.CurrentEntry);
        Assert.Equal("Game", vm.Sections.Name);
        Assert.Equal(WalkGolden, Dump(vm));
    }

    [Fact]
    public void Delete_UnlinksTheMiddleAndClearKeyDropsTheFirstMatch()
    {
        var fresh = new VmConfigCursor1261();
        Assert.Throws<NullReferenceException>(() => fresh.GetValue("x"));
        Assert.Throws<NullReferenceException>(() => fresh.ClearKey("x"));
        Assert.False(fresh.Delete());

        var vm = fresh;
        Assert.True(vm.SetSection("Game", true));
        vm.Add("Bind", "Yes");
        vm.Add("bind", "no");
        vm.Add("Tail", "z");
        Assert.True(vm.SetSection("Audio", true));
        vm.Add("Vol", "3");
        Assert.True(vm.SetSection("Video", true));
        vm.Add("Mode", "1");
        var game = vm.Sections!;
        var video = vm.Current!;
        Assert.Equal("Video", vm.Tail!.Name);

        Assert.True(vm.SetSection("Game", false));
        Assert.Equal("Yes", vm.GetValue("bind"));
        vm.ClearKey("missing");
        vm.ClearKey("BIND");
        Assert.Equal("no", vm.GetValue("bind"));
        Assert.Equal("bind", vm.Current!.Root!.Key);
        Assert.Equal("Tail", vm.Current.Tail!.Key);
        vm.ClearKey("TAIL");
        Assert.Equal("bind", vm.Current.Root.Key);
        Assert.Null(vm.Current.Root.Next);
        Assert.Same(vm.Current.Root, vm.Current.Tail);

        vm.Add("More", "9");
        Assert.True(vm.SetSection("Game", false));
        Assert.True(vm.Next(out _, out _));
        var detached = vm.CurrentEntry;
        Assert.Equal("More", detached!.Key);
        vm.Clear();
        Assert.True(vm.SectionIsEmpty());
        Assert.Null(vm.GetValue("bind"));
        Assert.Same(detached, vm.CurrentEntry);
        Assert.Same(game, vm.Current);

        Assert.True(vm.SetSection("Audio", false));
        var audio = vm.Current!;
        var vol = vm.CurrentEntry;
        Assert.True(vm.Delete());
        Assert.Equal("Video", vm.CurrentName());
        Assert.Same(vol, vm.CurrentEntry);
        Assert.Equal("Vol", vol!.Key);
        Assert.Null(audio.Root);
        Assert.Same(video, game.Next);
        Assert.Null(video.Next);
        Assert.Same(video, vm.Tail);
        Assert.Equal("Mode", video.Root!.Key);
        Assert.False(vm.SectionIsEmpty());
        Assert.Equal("1", vm.GetValue("mode"));

        Assert.False(vm.Delete());
        Assert.Null(vm.Current);
        Assert.Same(vol, vm.CurrentEntry);
        Assert.Null(video.Root);
        Assert.Null(game.Next);
        Assert.Same(game, vm.Tail);
        Assert.True(vm.SectionIsEmpty());

        Assert.True(vm.SetFirst());
        Assert.Same(game, vm.Current);
        Assert.Null(vm.CurrentEntry);
        Assert.Throws<NullReferenceException>(() => vm.Delete());
        Assert.Same(game, vm.Sections);
        Assert.Same(game, vm.Current);
        Assert.Same(game, vm.Tail);
        Assert.Null(game.Root);
        Assert.Null(game.Next);
        Assert.Equal(EditGolden, Dump(vm));
    }

    static string Dump(VmConfigCursor1261 vm) => string.Join('\n', vm.Log) + "\n";
}
