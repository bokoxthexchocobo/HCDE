namespace HCDE.Playsim.Tests;

public class VmDThinkerSerializeCollectionTests
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
    public void SerializeThinkersWrite_WalksBothListsPerStat()
    {
        var collection = NewCollection();
        var main = new VmDThinkerListOps.ThinkerNodeModel();
        var fresh = new VmDThinkerListOps.ThinkerNodeModel();
        VmDThinkerListOps.AddTail(collection.Thinkers[ThinkerStat.Default], main);
        VmDThinkerListOps.AddTail(collection.FreshThinkers[ThinkerStat.Default], fresh);
        var serialized = new List<VmDThinkerListOps.ThinkerNodeModel>();
        VmDThinkerSerializeCollection.SerializeThinkersWrite(
            collection,
            _ => { },
            () => { },
            serialized.Add);
        Assert.Contains(main, serialized);
        Assert.Contains(fresh, serialized);
    }

    [Fact]
    public void SerializeThinkersRead_LinksFreshAndMain()
    {
        var collection = NewCollection();
        var fresh = new VmDThinkerListOps.ThinkerNodeModel { ObjectFlags = VmDThinkerListOps.OfJustSpawned };
        var main = new VmDThinkerListOps.ThinkerNodeModel();
        var post = 0;
        fresh.PostSerialize = () => post++;
        main.PostSerialize = () => post++;
        var queue = new Queue<VmDThinkerListOps.ThinkerNodeModel>([fresh, main]);
        var statPass = 0;
        VmDThinkerSerializeCollection.SerializeThinkersRead(
            collection,
            hubLoad: false,
            () => true,
            () => statPass++ == 0,
            () => queue.Count,
            _ => queue.Dequeue(),
            () => { },
            () => { });
        Assert.Same(fresh, VmDThinkerListOps.GetHead(collection.FreshThinkers[ThinkerStat.Default]));
        Assert.Same(main, VmDThinkerListOps.GetHead(collection.Thinkers[ThinkerStat.Default]));
        Assert.Equal(2, post);
    }
}
