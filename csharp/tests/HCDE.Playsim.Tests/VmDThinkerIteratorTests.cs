namespace HCDE.Playsim.Tests;

public class VmDThinkerIteratorTests
{
    private static VmDThinkerListOps.CollectionModel NewCollection() =>
        new()
        {
            Thinkers = Enumerable.Range(0, VmDThinkerListOps.MaxStatNum + 2)
                .Select(_ => new VmDThinkerListOps.ThinkerListModel
                {
                    CreateSentinel = () => new VmDThinkerListOps.ThinkerNodeModel()
                }).ToArray(),
            FreshThinkers = Enumerable.Range(0, VmDThinkerListOps.MaxStatNum + 2)
                .Select(_ => new VmDThinkerListOps.ThinkerListModel
                {
                    CreateSentinel = () => new VmDThinkerListOps.ThinkerNodeModel()
                }).ToArray(),
        };

    [Fact]
    public void Next_ReturnsMatchingThinkersAcrossFreshList()
    {
        var collection = NewCollection();
        var actor = new VmDThinkerListOps.ThinkerNodeModel { TypeName = () => "Actor" };
        var freshActor = new VmDThinkerListOps.ThinkerNodeModel { TypeName = () => "Actor" };
        VmDThinkerListOps.AddTail(collection.Thinkers[ThinkerStat.Default], actor);
        VmDThinkerListOps.AddTail(collection.FreshThinkers[ThinkerStat.Default], freshActor);

        var it = new VmDThinkerIterator(collection, "Actor", ThinkerStat.Default, clientSide: false);
        Assert.Same(actor, it.Next());
        Assert.Same(freshActor, it.Next());
        Assert.Null(it.Next());
    }
}
