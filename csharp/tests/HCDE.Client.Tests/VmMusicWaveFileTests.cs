using System.Buffers.Binary;

namespace HCDE.Client.Tests;

public class VmMusicWaveFileTests
{
    [Theory]
    [InlineData(0u)]
    [InlineData(16u)]
    public void ExtensibleHeaderMatchesNativeLayoutAndFinalSizes(uint dataBytes)
    {
        var bytes = new byte[72];
        Array.Fill(bytes, (byte)99);
        VmMusicWaveFile.WriteHeader(bytes, 44100, dataBytes);
        var expected = Convert.FromHexString(
            "524946460000000057415645666D742028000000FEFF020044AC0000206205000800200016002000030000000300000000001000800000AA00389B716461746100000000");
        BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(4), dataBytes + 60);
        BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(64), dataBytes);
        Assert.Equal(expected, bytes[..68]);
        Assert.Equal(new byte[] { 99, 99, 99, 99 }, bytes[68..]);
    }

    [Fact]
    public void InvalidHeaderInputsDoNotMutateDestination()
    {
        var bytes = Enumerable.Repeat((byte)99, 68).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicWaveFile.WriteHeader(bytes, 0, 0));
        Assert.Throws<ArgumentException>(() => VmMusicWaveFile.WriteHeader(bytes, 44100, 1));
        Assert.Throws<OverflowException>(() => VmMusicWaveFile.WriteHeader(bytes, 44100, uint.MaxValue - 7));
        Assert.Throws<OverflowException>(() => VmMusicWaveFile.WriteHeader(bytes, int.MaxValue, 0));
        Assert.Throws<ArgumentException>(() => VmMusicWaveFile.WriteHeader(bytes.AsSpan(0, 67), 44100, 0));
        Assert.All(bytes, value => Assert.Equal((byte)99, value));
    }

    [Fact]
    public void SamplesPreserveUnclippedFloatBitsAndUseLittleEndian()
    {
        float[] samples = [1, -1, 2.5f, BitConverter.Int32BitsToSingle(unchecked((int)0x80000000))];
        var buffer = Enumerable.Repeat((byte)99, 20).ToArray();
        Assert.Equal(16, VmMusicWaveFile.WriteSamples(samples, buffer));
        Assert.Equal(Convert.FromHexString("0000803F000080BF0000204000000080"), buffer[..16]);
        Assert.Equal(new byte[] { 99, 99, 99, 99 }, buffer[16..]);
        Assert.Throws<ArgumentException>(() => VmMusicWaveFile.WriteSamples(new float[1], buffer));
        Assert.Throws<ArgumentException>(() => VmMusicWaveFile.WriteSamples(samples, buffer.AsSpan(0, 15)));
    }
}
