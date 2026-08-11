namespace HCDE.Net.Core.Tests;

public class LiveSequenceTrackerTests
{
    [Fact]
    public void SequenceTracker_TracksPerMessageTypeIndependently()
    {
        var tracker = new LiveSequenceTracker();
        Assert.True(tracker.IsFresh(LiveMessageType.Control, 1));
        tracker.Accept(LiveMessageType.Control, 1);
        Assert.False(tracker.IsFresh(LiveMessageType.Control, 1));
        Assert.Equal(1u, tracker.DuplicateCount);

        Assert.True(tracker.IsFresh(LiveMessageType.ServerSnapshot, 1));
        tracker.Accept(LiveMessageType.ServerSnapshot, 1);
        Assert.Equal(1u, tracker.RxSequence);
    }

    [Fact]
    public void SequenceTracker_RejectsZeroSequence()
    {
        var tracker = new LiveSequenceTracker();
        Assert.False(tracker.IsFresh(LiveMessageType.ClientCommands, 0));
    }

    [Fact]
    public void SequenceTracker_RxSequenceAdvancesToHighestAccepted()
    {
        var tracker = new LiveSequenceTracker();
        tracker.Accept(LiveMessageType.Control, 5);
        tracker.Accept(LiveMessageType.ClientCommands, 10);
        Assert.Equal(10u, tracker.RxSequence);
    }
}
