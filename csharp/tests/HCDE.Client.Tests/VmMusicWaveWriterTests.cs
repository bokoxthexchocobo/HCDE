using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicWaveWriterTests
{
    [Fact]
    public void RenderLoopWrites4096SamplesPerSuccessfulRequestAndDropsFinalFalseRequest()
    {
        using var stream = new MemoryStream();
        using var writer = new VmMusicWaveWriter(stream, 44100);
        var calls = 0;
        writer.RenderUntilEnd(samples =>
        {
            Assert.Equal(4096, samples.Length);
            samples.Fill(++calls);
            return calls < 3;
        });
        Assert.Equal(3, calls);
        Assert.Equal(32768u, writer.DataBytes);
        var bytes = stream.ToArray();
        Assert.Equal(32836, bytes.Length);
        Assert.Equal(32768u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(64)));
        Assert.Equal(0x3f800000, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(68)));
        Assert.Equal(0x40000000, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(68 + 16384)));
    }

    [Fact]
    public void RenderFailurePreservesExceptionAndDoesNotFinalizePartialFile()
    {
        using var stream = new MemoryStream();
        var writer = new VmMusicWaveWriter(stream, 44100);
        var failure = new InvalidOperationException("renderer failed");
        var calls = 0;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => writer.RenderUntilEnd(samples =>
        {
            if (++calls == 2) throw failure;
            samples.Fill(1);
            return true;
        })));
        writer.Dispose();
        Assert.Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(stream.ToArray().AsSpan(4)));
        Assert.Equal(16452, stream.Length);
    }

    [Fact]
    public void CancelledRenderDoesNotInvokeRendererOrFinalizeOutput()
    {
        using var stream = new MemoryStream();
        using var writer = new VmMusicWaveWriter(stream, 44100);
        var called = false;
        Assert.Throws<OperationCanceledException>(() => writer.RenderUntilEnd(_ => { called = true; return true; }, new CancellationToken(true)));
        Assert.False(called);
        Assert.Equal(0u, writer.DataBytes);
        Assert.Throws<InvalidOperationException>(() => writer.Complete());
    }

    [Fact]
    public void FinalizationPatchesSizesAndPreservesEncodedSamples()
    {
        using var stream = new MemoryStream();
        var writer = new VmMusicWaveWriter(stream, 44100);
        Assert.Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(stream.ToArray().AsSpan(4)));
        writer.WriteSamples(new float[] { 1, -1 });
        writer.WriteSamples(new float[] { 2.5f, -2.5f });
        writer.Complete();
        writer.Complete();
        var bytes = stream.ToArray();
        Assert.Equal(16u, writer.DataBytes);
        Assert.Equal(76u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4)));
        Assert.Equal(16u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(64)));
        Assert.Equal(Convert.FromHexString("0000803F000080BF00002040000020C0"), bytes[68..]);
        Assert.Equal(84, stream.Position);
        Assert.Throws<InvalidOperationException>(() => writer.WriteSamples(new float[2]));
        writer.Dispose();
        Assert.True(stream.CanWrite);
    }

    [Fact]
    public void LargeRequestsCrossEncodingChunksAndDisposeFinalizesOwnedStream()
    {
        var stream = new MemoryStream();
        var writer = new VmMusicWaveWriter(stream, 48000, false);
        writer.WriteSamples(Enumerable.Repeat(1f, 5000).ToArray());
        writer.Dispose();
        writer.Dispose();
        Assert.False(stream.CanWrite);
        var bytes = stream.ToArray();
        Assert.Equal(20068, bytes.Length);
        Assert.Equal(20000u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(64)));
        Assert.Equal(0x3f800000, BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4)));
    }

    [Fact]
    public void FailedWritePropagatesAndCannotBeFinalizedAsSuccess()
    {
        var stream = new FailingStream();
        var writer = new VmMusicWaveWriter(stream, 44100, false);
        stream.Fail = true;
        Assert.Same(stream.Failure, Assert.Throws<IOException>(() => writer.WriteSamples(new float[2])));
        Assert.Throws<InvalidOperationException>(() => writer.Complete());
        writer.Dispose();
        Assert.False(stream.CanWrite);
    }

    [Fact]
    public void InvalidFramesDoNotPoisonWriterAndNonemptyOutputIsRejected()
    {
        using var stream = new MemoryStream();
        using var writer = new VmMusicWaveWriter(stream, 44100);
        Assert.Throws<ArgumentException>(() => writer.WriteSamples(new float[1]));
        writer.WriteSamples(new float[2]);
        Assert.Throws<ArgumentException>(() => new VmMusicWaveWriter(stream, 44100));
    }

    private sealed class FailingStream : MemoryStream
    {
        public bool Fail { get; set; }
        public IOException Failure { get; } = new("write failed");
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (Fail) throw Failure;
            base.Write(buffer);
        }
    }
}
