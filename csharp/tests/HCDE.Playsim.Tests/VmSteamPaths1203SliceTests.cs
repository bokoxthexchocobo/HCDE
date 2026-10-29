namespace HCDE.Playsim.Tests;

public class VmSteamPaths1203SliceTests
{
    const string LibraryFolders = """
        "libraryfolders"
        {
          // note
          /* block */
          "0"
          {
            "label" "Main"
            "apps" { "2280" "1" }
            "Path" "C:\\Steam"
          }
          "1"
          {
            "path" "D:/Games"
            "apps" { "2300" "1" }
          }
        }
        """;

    static bool Exists(string path) =>
        path is "C:/Steam" or "C://Steam/steamapps/common/Doom 2/base" or "C:/Steam/steamapps/common/Strife";

    [Fact]
    public void ParseRegistry_ReadsLibraryPathsAndInstalledGames()
    {
        var libs = VmSteamPaths1203Slice.ParseRegistry(LibraryFolders);
        Assert.Equal(new[] { "C:\\\\Steam/steamapps/common", "D:/Games/steamapps/common" }, libs);
        var games = VmSteamPaths1203Slice.GetGamePaths("C:/Steam", LibraryFolders, Exists);
        Assert.Equal(
            new[] { "C://Steam/steamapps/common/Doom 2/base", "C:/Steam/steamapps/common/Strife" },
            games);
        Assert.Equal(19, VmSteamPaths1203Slice.Apps.Length);
        Assert.Equal(208200, VmSteamPaths1203Slice.Apps[9].AppId);
        Assert.Equal("DOOM 3 BFG Edition/base/wads", VmSteamPaths1203Slice.Apps[9].Path);
        var quoted = VmSteamPaths1203Slice.ParseRegistry("\"root\" {\n\"0\" {\n\"path\" \"say \\\"hi\\\"\"\n}\n}\n");
        Assert.Equal(new[] { "say \"hi\"/steamapps/common" }, quoted);
    }

    [Fact]
    public void MissingRoot_AndParseErrors_SkipTheCanonPath()
    {
        Assert.Empty(VmSteamPaths1203Slice.GetGamePaths("", LibraryFolders, Exists));
        Assert.Empty(VmSteamPaths1203Slice.GetGamePaths("C:/missing", LibraryFolders, Exists));
        Assert.Equal(
            new[] { "C:/Steam/steamapps/common/Strife" },
            VmSteamPaths1203Slice.GetGamePaths("C:/Steam", null, Exists));
        Assert.Empty(VmSteamPaths1203Slice.ParseRegistry("\"libraryfolders\" {"));
        Assert.Equal(
            new[] { "C:/Steam/steamapps/common/Strife" },
            VmSteamPaths1203Slice.GetGamePaths("C:/Steam", "\"libraryfolders\" {", Exists));
        var missingBrace = Assert.Throws<VmSteamPaths1203Slice.ParseError>(
            () => VmSteamPaths1203Slice.ParseRegistry("\"libraryfolders\""));
        Assert.Equal("Expected '{'.", missingBrace.Detail);
        Assert.Empty(VmSteamPaths1203Slice.GetGamePaths("C:/Steam", "\"libraryfolders\" { \"0\" { \"path\"", Exists));
    }
}
