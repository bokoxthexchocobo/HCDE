using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client.Tests;

public class VmMusicXmiDocumentTests
{
    private static byte[] Chunk(string id, params byte[][] parts)
    {
        var payload = parts.SelectMany(x => x).ToArray();
        var result = new byte[8 + payload.Length + (payload.Length & 1)];
        Encoding.ASCII.GetBytes(id).CopyTo(result, 0);
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(4), (uint)payload.Length);
        payload.CopyTo(result, 8);
        return result;
    }

    private static byte[] Song(params byte[][] chunks) => Chunk("FORM", ["XMID"u8.ToArray(), .. chunks]);

    [Fact]
    public void OwnsEventsAndLastTimbresBeforeEvents()
    {
        var bytes = Song(Chunk("TIMB", [1]), Chunk("JUNK", [9]), Chunk("TIMB", [2, 3]), Chunk("EVNT", [4]));
        var document = VmMusicXmiDocument.Parse(bytes);
        Array.Clear(bytes);
        var song = Assert.Single(document.Songs);
        Assert.Equal(new byte[] { 4 }, song.Events.ToArray());
        Assert.Equal(new byte[] { 2, 3 }, song.Timbres.ToArray());
        Assert.Equal(60, document.Division);
        Assert.Equal(500000u, document.InitialTempo);
    }

    [Fact]
    public void DiscoversNestedCatalogsInOrderAndIgnoresDirectoryForms()
    {
        var bytes = Chunk("FORM", "XDIR"u8.ToArray(), Song(Chunk("EVNT", [99])))
            .Concat(Chunk("CAT ", "XMID"u8.ToArray(), Song(Chunk("EVNT", [1])),
                Chunk("CAT ", "OTHR"u8.ToArray(), Song(Chunk("EVNT", [2]))), Song(Chunk("EVNT", [3])))).ToArray();
        Assert.Equal(new byte[] { 1, 2, 3 }, VmMusicXmiDocument.Parse(bytes).Songs.Select(x => x.Events.Span[0]));
    }

    [Fact]
    public void FirstEventsEndsSongEvenWithMalformedTrailingSongData()
    {
        var song = Assert.Single(VmMusicXmiDocument.Parse(Song(Chunk("EVNT"), [1, 2])).Songs);
        Assert.Empty(song.Events.ToArray());
        Assert.Empty(song.Timbres.ToArray());
    }

    [Fact]
    public void MissingEventsOrSongsIsRejected()
    {
        Assert.Throws<InvalidDataException>(() => VmMusicXmiDocument.Parse(Song(Chunk("TIMB", [1]))));
        Assert.Throws<InvalidDataException>(() => VmMusicXmiDocument.Parse(Chunk("FORM", "XDIR"u8.ToArray())));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(11)]
    [InlineData(20)]
    public void TruncatedContainersAreRejected(int length)
    {
        var bytes = Song(Chunk("EVNT", [1]));
        Assert.Throws<InvalidDataException>(() => VmMusicXmiDocument.Parse(bytes.AsSpan(0, length)));
    }

    [Fact]
    public void OversizedLengthAndMissingContainerTypeAreRejected()
    {
        var bytes = Song(Chunk("EVNT"));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4), uint.MaxValue);
        Assert.Throws<InvalidDataException>(() => VmMusicXmiDocument.Parse(bytes));
        Assert.Throws<InvalidDataException>(() => VmMusicXmiDocument.Parse(Chunk("CAT ")));
    }

    [Fact]
    public void ChildCannotReadOutsideItsContainerAndOddPayloadNeedsPadding()
    {
        var bytes = Song(Chunk("EVNT", [1]));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(16), 10);
        Assert.Throws<InvalidDataException>(() => VmMusicXmiDocument.Parse(bytes));
        var unpadded = Chunk("EVNT", [1])[..^1];
        Assert.Throws<InvalidDataException>(() => VmMusicXmiDocument.Parse(Song(unpadded)));
    }

    [Fact]
    public void DeepCatalogsUseAnExplicitStack()
    {
        var bytes = Song(Chunk("EVNT", [1]));
        for (var i = 0; i < 2000; i++) bytes = Chunk("CAT ", "XMID"u8.ToArray(), bytes);
        Assert.Single(VmMusicXmiDocument.Parse(bytes).Songs);
    }
}
