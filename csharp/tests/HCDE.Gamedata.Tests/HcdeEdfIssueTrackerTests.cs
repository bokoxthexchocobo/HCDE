namespace HCDE.Gamedata.Tests;

public class HcdeEdfIssueTrackerTests
{
    [Fact]
    public void TryRecordIssue_CapsAtTwenty()
    {
        var list = new List<HcdeEdfIssueSample>();
        for (var i = 0; i < 25; i++)
            HcdeEdfIssueTracker.TryRecordIssue(list, "ctx", "d", i);
        Assert.Equal(20, list.Count);
    }
}
