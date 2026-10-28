namespace HCDE.Net.Core.Tests;

public class VmINetPregameUiTests
{
    [Fact]
    public void IShouldStartNetGameNative_UsesDedicatedFlag()
    {
        Assert.True(VmINetPregameUi.IShouldStartNetGameNative(true, true, () => false));
        Assert.False(VmINetPregameUi.IShouldStartNetGameNative(true, false, () => true));
    }

    [Fact]
    public void IGetKickClientsNative_CollectsIds()
    {
        var queue = new Queue<int>([2, 5]);
        var list = new List<int>();
        VmINetPregameUi.IGetKickClientsNative(list, () => queue.Count > 0 ? queue.Dequeue() : -1);
        Assert.Equal([2, 5], list);
    }
}
