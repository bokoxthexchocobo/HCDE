namespace HCDE.Client;

public sealed class VmReplayGainAnalyzer
{
    private VmReplayGainFilter leftYule;
    private VmReplayGainFilter rightYule;
    private VmReplayGainFilter leftButter;
    private VmReplayGainFilter rightButter;
    private readonly VmReplayGainHistogram histogram = new();
    private int window;
    private float[] leftBuffer;
    private float[] rightBuffer;
    private int count;
    private double leftSum;
    private double rightSum;

    public VmReplayGainAnalyzer(int sampleRate)
    {
        window = VmReplayGainKernels.WindowSamples(sampleRate);
        leftBuffer = new float[window];
        rightBuffer = new float[window];
        leftYule = new(VmReplayGainKernels.YuleFor(sampleRate), true);
        rightYule = new(VmReplayGainKernels.YuleFor(sampleRate), true);
        leftButter = new(VmReplayGainKernels.ButterFor(sampleRate), false);
        rightButter = new(VmReplayGainKernels.ButterFor(sampleRate), false);
    }

    public void Analyze(ReadOnlySpan<float> left, ReadOnlySpan<float> right, int channels)
    {
        if (left.IsEmpty) return;
        if (channels is not (1 or 2)) throw new ArgumentOutOfRangeException(nameof(channels));
        if (channels == 2 && right.Length < left.Length)
            throw new ArgumentException("Right channel must contain every left-channel frame.", nameof(right));
        var position = 0;
        while (position < left.Length)
        {
            var length = Math.Min(left.Length - position, window - count);
            if (position < 10) length = Math.Min(length, 10 - position);
            var l = leftBuffer.AsSpan(0, length);
            var r = rightBuffer.AsSpan(0, length);
            leftYule.Process(left.Slice(position, length), l);
            rightYule.Process((channels == 1 ? left : right).Slice(position, length), r);
            leftButter.Process(l, l);
            rightButter.Process(r, r);
            Accumulate(l, ref leftSum);
            Accumulate(r, ref rightSum);
            position += length;
            count += length;
            if (count != window) continue;
            histogram.AddWindow(leftSum, rightSum, count);
            count = 0;
            leftSum = rightSum = 0;
        }
    }

    public void ResetSampleFrequency(int sampleRate)
    {
        var nextWindow = VmReplayGainKernels.WindowSamples(sampleRate);
        var nextLeftYule = new VmReplayGainFilter(VmReplayGainKernels.YuleFor(sampleRate), true);
        var nextRightYule = new VmReplayGainFilter(VmReplayGainKernels.YuleFor(sampleRate), true);
        var nextLeftButter = new VmReplayGainFilter(VmReplayGainKernels.ButterFor(sampleRate), false);
        var nextRightButter = new VmReplayGainFilter(VmReplayGainKernels.ButterFor(sampleRate), false);
        var nextLeftBuffer = new float[nextWindow];
        var nextRightBuffer = new float[nextWindow];
        window = nextWindow;
        leftYule = nextLeftYule;
        rightYule = nextRightYule;
        leftButter = nextLeftButter;
        rightButter = nextRightButter;
        leftBuffer = nextLeftBuffer;
        rightBuffer = nextRightBuffer;
        count = 0;
        leftSum = rightSum = 0;
        histogram.ClearTitle();
    }

    private static void Accumulate(ReadOnlySpan<float> samples, ref double sum)
    {
        var position = 0;
        var remainder = samples.Length % 16;
        for (; position < remainder; position++) sum += (double)samples[position] * samples[position];
        for (; position < samples.Length; position += 16)
        {
            var group = (double)samples[position] * samples[position];
            for (var i = 1; i < 16; i++) group += (double)samples[position + i] * samples[position + i];
            sum += group;
        }
    }

    public float GetTitleGain()
    {
        var result = histogram.GetTitleGain();
        leftYule.Reset();
        rightYule.Reset();
        leftButter.Reset();
        rightButter.Reset();
        count = 0;
        leftSum = rightSum = 0;
        return result;
    }

    public float GetAlbumGain() => histogram.GetAlbumGain();
}
