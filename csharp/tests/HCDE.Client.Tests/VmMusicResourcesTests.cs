namespace HCDE.Client.Tests;

public class VmMusicResourcesTests
{
    [Fact]
    public void ExternalFileWinsEvenWhenEmpty()
    {
        var path = Path.GetTempFileName();
        try
        {
            var resources = new VmMusicResources(_ => throw new InvalidOperationException(),
                _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
                _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
            Assert.True(resources.Exists(path));
            using var reader = resources.Open(path);
            Assert.NotNull(reader);
            Assert.Equal(0, reader.Read(new byte[1]));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void GameLookupFallsBackToMusicNamespaceAndReaderSnapshotsBytes()
    {
        byte[] bytes = [1, 2];
        var resources = new VmMusicResources(_ => -1, _ => 3, index => { Assert.Equal(3, index); return 2; },
            index => { Assert.Equal(3, index); return bytes; }, _ => throw new InvalidOperationException());
        Assert.True(resources.Exists("track"));
        using var reader = resources.Open("track");
        bytes[0] = 9;
        var output = new byte[2];
        Assert.Equal(2, reader!.Read(output));
        Assert.Equal(new byte[] { 1, 2 }, output);
    }

    [Fact]
    public void EmptyFoundResourceDoesNotFallbackOrReportMissing()
    {
        var resources = new VmMusicResources(_ => 2, _ => throw new InvalidOperationException(), _ => 0,
            _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
        Assert.False(resources.Exists("empty"));
        Assert.Null(resources.Open("empty"));
    }

    [Fact]
    public void MissingOpenReportsButExistenceQueryDoesNot()
    {
        var messages = new List<string>();
        var resources = new VmMusicResources(_ => -1, _ => -1, _ => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), messages.Add);
        Assert.False(resources.Exists(null));
        Assert.False(resources.Exists("missing"));
        Assert.Empty(messages);
        Assert.Null(resources.Open("missing"));
        Assert.Equal("Music \"missing\" not found\n", Assert.Single(messages));
    }
}
