namespace HCDE.Client.Tests;

public class VmReplayGainAnalyzerTests
{
    [Fact]
    public void CompleteWindowIsRequiredAndTitleResetDiscardsPartialWindow()
    {
        var analyzer = new VmReplayGainAnalyzer(8000);
        analyzer.Analyze(new float[399], [], 1);
        Assert.Equal(-24601, analyzer.GetTitleGain());
        analyzer.Analyze(new float[1], [], 1);
        Assert.Equal(-24601, analyzer.GetTitleGain());
        analyzer.Analyze(new float[400], [], 1);
        Assert.Equal(64.82f, analyzer.GetTitleGain());
        Assert.Equal(64.82f, analyzer.GetAlbumGain());
    }

    [Fact]
    public void ArbitraryChunksMatchWholeInputAndMonoMatchesDuplicatedStereo()
    {
        var input = Enumerable.Range(0, 5000).Select(i => MathF.Sin(i * .1f) * 1000).ToArray();
        var whole = new VmReplayGainAnalyzer(44100);
        var chunks = new VmReplayGainAnalyzer(44100);
        whole.Analyze(input, [], 1);
        for (var i = 0; i < input.Length; i += 137)
        {
            var block = input.AsSpan(i, Math.Min(137, input.Length - i));
            chunks.Analyze(block, block, 2);
        }
        Assert.Equal(whole.GetTitleGain(), chunks.GetTitleGain());
        Assert.Equal(whole.GetAlbumGain(), chunks.GetAlbumGain());
    }

    [Fact]
    public void DoublingAmplitudeLowersRecommendedGainByApproximatelySixDecibels()
    {
        var input = Enumerable.Range(0, 5000).Select(i => MathF.Sin(i * .1f) * 1000).ToArray();
        var first = new VmReplayGainAnalyzer(44100);
        var second = new VmReplayGainAnalyzer(44100);
        first.Analyze(input, [], 1);
        second.Analyze(input.Select(value => value * 2).ToArray(), [], 1);
        Assert.InRange(first.GetTitleGain() - second.GetTitleGain(), 6.00f, 6.04f);
    }

    [Fact]
    public void FrequencyResetDiscardsTitleAndHistoryButRetainsFinalizedAlbum()
    {
        var analyzer = new VmReplayGainAnalyzer(8000);
        analyzer.Analyze(new float[400], [], 1);
        var album = analyzer.GetTitleGain();
        analyzer.Analyze(Enumerable.Repeat(1000f, 450).ToArray(), [], 1);
        analyzer.ResetSampleFrequency(44100);
        Assert.Equal(-24601, analyzer.GetTitleGain());
        Assert.Equal(album, analyzer.GetAlbumGain());
        analyzer.Analyze(new float[2204], [], 1);
        Assert.Equal(-24601, analyzer.GetTitleGain());
        analyzer.Analyze(new float[2205], [], 1);
        Assert.Equal(64.82f, analyzer.GetTitleGain());
    }

    [Fact]
    public void UnsupportedFrequencyResetPreservesPriorAnalyzerState()
    {
        var analyzer = new VmReplayGainAnalyzer(8000);
        analyzer.Analyze(new float[399], [], 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => analyzer.ResetSampleFrequency(12345));
        analyzer.Analyze(new float[1], [], 1);
        Assert.Equal(64.82f, analyzer.GetTitleGain());
    }

    [Fact]
    public void InvalidChannelsAndShortStereoInputAreRejected()
    {
        var analyzer = new VmReplayGainAnalyzer(8000);
        analyzer.Analyze([], [], 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => analyzer.Analyze(new float[1], [], 3));
        Assert.Throws<ArgumentException>(() => analyzer.Analyze(new float[2], new float[1], 2));
    }
}
