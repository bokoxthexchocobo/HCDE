namespace HCDE.Playsim.Tests;

public class VmPMobjBehaviorTransferTests
{
    [Fact]
    public void MoveBehaviorsRejected_ClientMismatch() =>
        Assert.True(VmPMobjBehaviorTransfer.MoveBehaviorsRejected(false, true));

    [Fact]
    public void TransferBehaviorOwner_UpdatesOwner()
    {
        object? owner = null;
        Assert.True(VmPMobjBehaviorTransfer.TransferBehaviorOwner(new object(), new object(), (_, o) => owner = o));
        Assert.NotNull(owner);
    }

    [Fact]
    public void InStateSequence_FindsAdjacent()
    {
        var a = new object();
        var b = new object();
        Assert.True(VmPMobjBehaviorTransfer.InStateSequence(b, a, s => s == a ? b : null, (x, y) => y == b));
    }

    [Fact]
    public void IsMapActorInventory_OwnerNull() =>
        Assert.True(VmPMobjBehaviorTransfer.IsMapActorInventory(true, null));

    [Fact]
    public void ClearBehaviorsMatching_AddsKey()
    {
        var list = new List<string>();
        VmPMobjBehaviorTransfer.ClearBehaviorsMatching(true, false, false, list, "a");
        Assert.Contains("a", list);
    }

    [Fact]
    public void UnlinkBehaviorNull_QueuesMissing()
    {
        var list = new List<string>();
        Assert.True(VmPMobjBehaviorTransfer.UnlinkBehaviorNull(null, list, "b", null));
        Assert.Contains("b", list);
    }

    [Fact]
    public void RunMoveBehaviors_TransfersOwner()
    {
        object? owner = null;
        var behavior = new object();
        Assert.True(VmPMobjBehaviorTransfer.RunMoveBehaviors(new VmPMobjBehaviorTransfer.MoveBehaviorsContext
        {
            Entries = [new VmPMobjPostSerializeBehaviors.BehaviorMapEntry("k", behavior)],
            FromActor = new object(),
            SetOwner = (b, o) => owner = o,
            IsValid = _ => true,
        }));
        Assert.NotNull(owner);
    }
}
