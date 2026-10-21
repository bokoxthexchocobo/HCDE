namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchMusicTests
{
    [Fact]
    public void TryParseMusicLine_BuildsMusicKey()
    {
        Assert.True(VmDehackedPatchMusic.TryParseMusicLine("d_e1m1", "  newtrack  ", out var key, out var name));
        Assert.Equal("MUSIC_d_e1m1", key);
        Assert.Equal("newtrack", name);
    }

    [Fact]
    public void RunPatchMusicNative_InsertsKey()
    {
        var lines = new Queue<(int, string, string)>(new[]
        {
            (1, "d_map01", "song"),
            (0, "", ""),
        });
        var inserted = new List<(string, string)>();
        VmDehackedPatchMusic.RunPatchMusicNative(
            () => lines.Dequeue(),
            (k, v) => inserted.Add((k, v)));
        Assert.Single(inserted);
        Assert.Equal(("MUSIC_d_map01", "song"), inserted[0]);
    }
}
