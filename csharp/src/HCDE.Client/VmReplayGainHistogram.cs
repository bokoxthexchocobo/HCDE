namespace HCDE.Client;

public sealed class VmReplayGainHistogram
{
    public const float NotEnoughSamples = -24601;
    private readonly uint[] title = new uint[12000];
    private readonly uint[] album = new uint[12000];

    public void AddWindow(double leftSum, double rightSum, int samples)
    {
        if (samples <= 0) throw new ArgumentOutOfRangeException(nameof(samples));
        var value = 100 * 10 * Math.Log10((leftSum + rightSum) / samples * .5 + 1e-37);
        if (double.IsNaN(value)) throw new ArgumentOutOfRangeException(nameof(leftSum));
        var bin = value <= 0 ? 0 : value >= title.Length ? title.Length - 1 : (int)value;
        title[bin] = unchecked(title[bin] + 1);
    }

    public float GetTitleGain()
    {
        var result = Analyze(title);
        for (var i = 0; i < title.Length; i++)
        {
            album[i] = unchecked(album[i] + title[i]);
            title[i] = 0;
        }
        return result;
    }

    public float GetAlbumGain() => Analyze(album);

    public void ClearTitle() => Array.Clear(title);

    private static float Analyze(uint[] histogram)
    {
        uint count = 0;
        foreach (var value in histogram) count = unchecked(count + value);
        if (count == 0) return NotEnoughSamples;
        var upper = (long)Math.Ceiling(count * (1.0 - .95));
        for (var i = histogram.Length - 1; i >= 0; i--)
        {
            upper -= histogram[i];
            if (upper <= 0) return 64.82f - i / 100f;
        }
        throw new InvalidOperationException("Histogram count overflow prevents percentile selection.");
    }
}
