namespace HCDE.Playsim.Tests;

public class PuffFrameLifecycleTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void FirstFrameDurationIsFollowedByThreeFourTicFrames(int firstFrameTics)
    {
        var puff = new PuffActor(firstFrameTics);
        Assert.Equal(5, puff.Mass); Assert.True(puff.FullBright);
        Assert.Equal(firstFrameTics + 12, puff.RemainingTics);
        for (var tic = 1; tic < firstFrameTics + 12; tic++)
        {
            puff.Tick();
            Assert.False(puff.Destroyed);
            Assert.Equal(firstFrameTics + 12 - tic, puff.RemainingTics);
            Assert.Equal(tic < firstFrameTics ? 0 : 1 + (tic - firstFrameTics) / 4, puff.States.Current);
            Assert.Equal(tic < firstFrameTics, puff.FullBright);
        }
        puff.Tick(); Assert.True(puff.Destroyed); Assert.Equal(0, puff.RemainingTics);
    }

    [Fact]
    public void RestoredFrameProgressControlsExpiryWithoutSeparateTimer()
    {
        var puff = new PuffActor();
        puff.Tick(); puff.Tick(); puff.Tick(); puff.Tick();
        var frame = puff.States.Current; var tics = puff.States.RemainingTics;
        puff.Tick(); puff.Tick(); puff.States.Restore(frame, tics);
        Assert.Equal(12, puff.RemainingTics);
        for (var tic = 0; tic < 11; tic++) { puff.Tick(); Assert.False(puff.Destroyed); }
        puff.Tick(); Assert.True(puff.Destroyed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void InvalidFirstFrameDurationIsRejected(int duration)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PuffActor(duration));
    }
}
