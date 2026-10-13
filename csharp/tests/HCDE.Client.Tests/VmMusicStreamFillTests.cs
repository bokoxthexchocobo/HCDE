namespace HCDE.Client.Tests;

public class VmMusicStreamFillTests
{
    [Fact]
    public void FloatBackendWritesDirectlyBeforeGain()
    {
        var stream = new VmMusicStreamFill();
        float[] output = [9, 9];
        Assert.True(stream.FillFloat(output, 2, samples =>
        {
            Assert.Equal(2, samples.Length);
            samples[0] = .25f;
            samples[1] = -.5f;
            return true;
        }));
        Assert.Equal(new float[] { .5f, -1 }, output);
    }

    [Fact]
    public void Int16BackendReceivesExactSampleCountAcrossBufferSizeChanges()
    {
        var stream = new VmMusicStreamFill();
        foreach (var length in new[] { 4, 2, 8, 0 })
        {
            var output = new float[length];
            Assert.True(stream.FillInt16(output, .5f, samples =>
            {
                Assert.Equal(length, samples.Length);
                samples.Fill(short.MinValue);
                return true;
            }));
            Assert.All(output, value => Assert.Equal(-.5f, value));
        }
    }

    [Fact]
    public void FailedBackendFillClearsPartialWritesAndPreviousOutput()
    {
        var stream = new VmMusicStreamFill();
        float[] output = [9, 9];
        Assert.False(stream.FillFloat(output, 1, samples => { samples[0] = 1; return false; }));
        Assert.Equal(new float[2], output);
        output.AsSpan().Fill(9);
        Assert.False(stream.FillInt16(output, 1, samples => { samples[0] = 123; return false; }));
        Assert.Equal(new float[2], output);
    }

    [Fact]
    public void BackendExceptionsPropagate()
    {
        var stream = new VmMusicStreamFill();
        Assert.Throws<InvalidOperationException>(() => stream.FillFloat(new float[1], 1,
            _ => throw new InvalidOperationException("backend failed")));
        Assert.Throws<InvalidOperationException>(() => stream.FillInt16(new float[1], 1,
            _ => throw new InvalidOperationException("backend failed")));
    }
}
