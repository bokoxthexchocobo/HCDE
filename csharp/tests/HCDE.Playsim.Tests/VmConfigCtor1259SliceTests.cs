namespace HCDE.Playsim.Tests;

public class VmConfigCtor1259SliceTests
{
    const string OpenGolden =
"""
path missing.cfg
section Boot
load body
load

""";

    const string AssignGolden =
"""
clear value Gone
clear entry Gone 44
drop Old
tail head
section Game
entry Bind alloc=44 value=Yes
entry  alloc=40 value=blank
entry Extra alloc=45 value=1
section Audio
entry Vol alloc=43 value=3
assign

""";

    const string DestroyGolden =
"""
free value Bind
free entry Bind 44
free value 
free entry  40
free value Extra
free entry Extra 45
free section Game
free value Vol
free entry Vol 43
free section Audio

""";

    [Fact]
    public void Open_ForcesExistedAfterTheLoad()
    {
        var empty = VmConfigCtor1259.Create();
        Assert.True(empty.OkayToWrite);
        Assert.True(empty.FileExisted);
        Assert.Equal("", empty.Path);
        Assert.Null(empty.Sections);
        Assert.Equal("ctor\n", string.Join('\n', empty.Log) + "\n");

        var opened = VmConfigCtor1259.Open("missing.cfg", self =>
        {
            self.FileExisted = false;
            self.OkayToWrite = false;
            self.NewSection("Boot");
            self.Log.Add("load body");
        });
        Assert.True(opened.FileExisted);
        Assert.True(opened.OkayToWrite);
        Assert.Equal("missing.cfg", opened.Path);
        Assert.Equal("Boot", opened.Sections!.Name);
        Assert.Null(opened.Sections.Next);
        Assert.Equal(OpenGolden, string.Join('\n', opened.Log) + "\n");
    }

    [Fact]
    public void Assign_KeepsDestinationFlagsAndMergesSections()
    {
        var src = VmConfigCtor1259.Create();
        src.Path = "src.cfg";
        src.OkayToWrite = false;
        src.FileExisted = false;
        var game = src.NewSection("Game");
        src.NewEntry(game, "Bind", "Yes");
        src.NewEntry(game, "", "blank");
        var again = src.NewSection("GAME");
        Assert.Same(game, again);
        src.NewEntry(again, "Extra", "1");
        var audio = src.NewSection("Audio");
        src.NewEntry(audio, "Vol", "3");
        Assert.Equal("Game", src.Sections!.Name);
        Assert.Equal("Audio", src.Sections.Next!.Name);
        Assert.Null(src.Sections.Next.Next);
        Assert.Equal("section keep Game", src.Log[^4]);

        var dest = VmConfigCtor1259.Create();
        dest.Path = "keep.cfg";
        dest.OkayToWrite = true;
        dest.FileExisted = true;
        var old = dest.NewSection("Old");
        dest.NewEntry(old, "Gone", "1");
        dest.Log.Clear();
        dest.Assign(src);
        Assert.Equal("keep.cfg", dest.Path);
        Assert.True(dest.OkayToWrite);
        Assert.True(dest.FileExisted);
        Assert.Equal("Game", dest.Sections!.Name);
        Assert.Equal("Audio", dest.Sections.Next!.Name);
        Assert.Equal("Bind", dest.Sections.Root!.Key);
        Assert.Equal("Yes", dest.Sections.Root.Value);
        Assert.Equal("", dest.Sections.Root.Next!.Key);
        Assert.Equal(40, dest.Sections.Root.Next.AllocBytes);
        Assert.Equal("Extra", dest.Sections.Root.Next.Next!.Key);
        Assert.Null(dest.Sections.Root.Next.Next.Next);
        Assert.Equal(AssignGolden, string.Join('\n', dest.Log) + "\n");

        var copy = VmConfigCtor1259.CopyOf(src);
        Assert.Equal("src.cfg", copy.Path);
        Assert.False(copy.OkayToWrite);
        Assert.False(copy.FileExisted);
        Assert.Equal("Game", copy.Sections!.Name);
        Assert.Equal("3", copy.Sections.Next!.Root!.Value);
        copy.Log.Clear();
        copy.Destroy();
        Assert.Null(copy.Sections);
        Assert.Null(copy.Current);
        Assert.Equal(DestroyGolden, string.Join('\n', copy.Log) + "\n");
    }
}
