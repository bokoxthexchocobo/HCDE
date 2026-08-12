using System.Text;

namespace HCDE.Net.Core.Tests;

public class PresentationEchoFullCodecTests
{
    [Fact]
    public void FullBlock_RoundTrip()
    {
        var block = PresentationEchoCodec.CreateExampleBlock();
        Span<byte> chunk = stackalloc byte[512];
        var written = PresentationEchoCodec.Write(chunk, block);
        Assert.True(written > LiveConstants.PresentationEchoMinHeaderSize);

        Assert.True(PresentationEchoCodec.TryRead(chunk[..written], out var parsed, out var consumed, out _));
        Assert.Equal(written, consumed);
        Assert.Equal((byte)0, parsed.InventoryPlayerSlot);
        Assert.Equal(2, parsed.InventoryItems.Length);
        Assert.True(parsed.InventoryItems[1].IsArmor);
        Assert.Equal(5, parsed.InventoryItems[1].HexenSlots.Length);
        Assert.Single(parsed.Players);
        Assert.Equal("Pistol", Encoding.UTF8.GetString(parsed.Players[0].ReadyWeaponName));
    }

    [Fact]
    public void FullEcho_InCoopTail_WalksSuccessfully()
    {
        var echo = PresentationEchoCodec.CreateExampleBlock();
        Span<byte> echoChunk = stackalloc byte[256];
        var echoWritten = PresentationEchoCodec.Write(echoChunk, echo);

        Span<byte> tail = stackalloc byte[512];
        var cursor = 0;
        cursor += WorldDeltaChunkCodec.WriteEmpty(tail[cursor..], gameTic: 3);
        cursor += ActorDeltasCodec.WriteEmpty(tail[cursor..]);
        echoChunk[..echoWritten].CopyTo(tail[cursor..]);
        cursor += echoWritten;

        Assert.True(ServerSnapshotTailWalker.TryWalk(tail[..cursor], out var sections, out var consumed, out _));
        Assert.Equal(cursor, consumed);
        Assert.Equal(1, sections.PresentationEcho.PlayerCount);
        Assert.Equal((byte)0, sections.PresentationEcho.InventoryPlayerSlot);
    }
}
