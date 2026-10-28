namespace HCDE.Playsim.Tests;

public class VmDThinkerListOpsTests
{
    private static VmDThinkerListOps.ThinkerListModel NewList()
    {
        return new VmDThinkerListOps.ThinkerListModel
        {
            CreateSentinel = () => new VmDThinkerListOps.ThinkerNodeModel(),
        };
    }

    [Fact]
    public void AddTail_LinksCircularList()
    {
        var list = NewList();
        var node = new VmDThinkerListOps.ThinkerNodeModel();
        VmDThinkerListOps.AddTail(list, node);

        Assert.Same(node, VmDThinkerListOps.GetHead(list));
        Assert.Same(node, VmDThinkerListOps.GetTail(list));
        Assert.False(VmDThinkerListOps.IsEmpty(list));
    }

    [Fact]
    public void FirstThinker_PrefersMainListThenFresh()
    {
        var collection = new VmDThinkerListOps.CollectionModel
        {
            Thinkers = Enumerable.Range(0, VmDThinkerListOps.MaxStatNum + 2)
                .Select(_ => NewList()).ToArray(),
            FreshThinkers = Enumerable.Range(0, VmDThinkerListOps.MaxStatNum + 2)
                .Select(_ => NewList()).ToArray(),
        };

        var freshOnly = new VmDThinkerListOps.ThinkerNodeModel();
        VmDThinkerListOps.AddTail(collection.FreshThinkers[ThinkerStat.Default], freshOnly);
        Assert.Same(freshOnly, VmDThinkerListOps.FirstThinker(collection, ThinkerStat.Default));

        var main = new VmDThinkerListOps.ThinkerNodeModel();
        VmDThinkerListOps.AddTail(collection.Thinkers[ThinkerStat.Default], main);
        Assert.Same(main, VmDThinkerListOps.FirstThinker(collection, ThinkerStat.Default));
    }

    [Fact]
    public void TickThinkers_MovesJustSpawnedToDestAndTicks()
    {
        var src = NewList();
        var dest = NewList();
        var ticked = 0;
        var node = new VmDThinkerListOps.ThinkerNodeModel
        {
            ObjectFlags = VmDThinkerListOps.OfJustSpawned,
            CallPostBeginPlay = () => { },
            CallTick = () => ticked++,
        };
        VmDThinkerListOps.AddTail(src, node);

        var ctx = new VmDThinkerListOps.TickContext();
        var count = VmDThinkerListOps.TickThinkers(src, dest, ctx);

        Assert.Equal(1, count);
        Assert.Equal(1, ticked);
        Assert.Equal(1, ctx.ThinkCount);
        Assert.Same(node, VmDThinkerListOps.GetHead(dest));
        Assert.Null(VmDThinkerListOps.GetHead(src));
        Assert.Equal(0, node.ObjectFlags & VmDThinkerListOps.OfJustSpawned);
    }

    [Fact]
    public void ProfileThinkers_AccumulatesClassProfiles()
    {
        var list = NewList();
        var node = new VmDThinkerListOps.ThinkerNodeModel
        {
            TypeName = () => "LightThinker",
            CallTick = () => { },
        };
        VmDThinkerListOps.AddTail(list, node);
        var profiles = new Dictionary<string, VmDThinkerListOps.ProfileEntry>
        {
            ["LightThinker"] = new() { Clock = () => { }, Unclock = () => { } },
        };

        var ctx = new VmDThinkerListOps.TickContext { Profiles = profiles };
        VmDThinkerListOps.ProfileThinkers(list, null, ctx);

        Assert.Equal(1, profiles["LightThinker"].NumCalls);
        Assert.Equal(1, ctx.ThinkCount);
    }

    [Fact]
    public void MarkRoots_InvokesMarkForEachSentinel()
    {
        var collection = new VmDThinkerListOps.CollectionModel
        {
            Thinkers = Enumerable.Range(0, VmDThinkerListOps.MaxStatNum + 2)
                .Select(_ =>
                {
                    var list = NewList();
                    VmDThinkerListOps.AddTail(list, new VmDThinkerListOps.ThinkerNodeModel());
                    return list;
                }).ToArray(),
            FreshThinkers = Enumerable.Range(0, VmDThinkerListOps.MaxStatNum + 2)
                .Select(_ =>
                {
                    var list = NewList();
                    VmDThinkerListOps.AddTail(list, new VmDThinkerListOps.ThinkerNodeModel());
                    return list;
                }).ToArray(),
        };

        var marks = 0;
        VmDThinkerListOps.MarkRoots(collection, _ => marks++);
        Assert.Equal((VmDThinkerListOps.MaxStatNum + 1) * 2 + 1, marks);
    }
}
