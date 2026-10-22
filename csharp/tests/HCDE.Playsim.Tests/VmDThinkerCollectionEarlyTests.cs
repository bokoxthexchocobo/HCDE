namespace HCDE.Playsim.Tests;

public class VmDThinkerCollectionEarlyTests
{
    private static VmDThinkerCollectionEarly.CollectionModel EmptyCollection(int statSlots = VmDThinkerCollectionEarly.MaxStatNum + 2)
    {
        var thinkers = new VmDThinkerCollectionEarly.ThinkerListOps[statSlots];
        var fresh = new VmDThinkerCollectionEarly.ThinkerListOps[statSlots];
        for (var i = 0; i < statSlots; i++)
        {
            thinkers[i] = new VmDThinkerCollectionEarly.ThinkerListOps();
            fresh[i] = new VmDThinkerCollectionEarly.ThinkerListOps();
        }

        return new VmDThinkerCollectionEarly.CollectionModel { Thinkers = thinkers, FreshThinkers = fresh };
    }

    [Fact]
    public void Link_RoutesJustSpawnedToFreshList()
    {
        var collection = EmptyCollection();
        var added = new List<string>();
        collection.FreshThinkers[ThinkerStat.Default].AddTail = _ => added.Add("fresh");
        collection.Thinkers[ThinkerStat.Default].AddTail = _ => added.Add("main");
        var thinker = new VmDThinkerCollectionEarly.ThinkerLinkModel
        {
            ObjectFlags = VmDThinkerCollectionEarly.OfJustSpawned,
        };
        VmDThinkerCollectionEarly.Link(collection, thinker, ThinkerStat.Default);
        Assert.Equal(["fresh"], added);
        Assert.Equal(ThinkerStat.Default, thinker.StatNum);
    }

    [Fact]
    public void Link_ClearsJustSpawnedWhenNotTravelling()
    {
        var collection = EmptyCollection();
        collection.Thinkers[ThinkerStat.Info].AddTail = _ => { };
        var thinker = new VmDThinkerCollectionEarly.ThinkerLinkModel
        {
            ObjectFlags = VmDThinkerCollectionEarly.OfJustSpawned,
        };
        VmDThinkerCollectionEarly.Link(collection, thinker, ThinkerStat.Info);
        Assert.Equal(0, thinker.ObjectFlags & VmDThinkerCollectionEarly.OfJustSpawned);
    }

    [Fact]
    public void RunThinkers_TicksFreshUntilStable()
    {
        var collection = EmptyCollection();
        var freshPasses = 0;
        for (var i = VmDThinkerCollectionEarly.StatFirstThinking; i <= VmDThinkerCollectionEarly.MaxStatNum; i++)
        {
            collection.Thinkers[i].TickThinkers = _ => 0;
            collection.FreshThinkers[i].TickThinkers = _ => 0;
        }

        collection.FreshThinkers[ThinkerStat.Default].TickThinkers = _ =>
        {
            freshPasses++;
            return freshPasses < 2 ? 1 : 0;
        };

        VmDThinkerCollectionEarly.RunThinkers(new VmDThinkerCollectionEarly.RunThinkersContext
        {
            Collection = collection,
        });
        Assert.Equal(2, freshPasses);
    }

    [Fact]
    public void DestroyAllThinkers_SkipsTravellingAndStatic()
    {
        var collection = EmptyCollection();
        var destroyed = new List<int>();
        for (var i = 0; i <= VmDThinkerCollectionEarly.MaxStatNum; i++)
        {
            var stat = i;
            collection.Thinkers[i].DoDestroyThinkers = () =>
            {
                destroyed.Add(stat);
                return false;
            };
            collection.FreshThinkers[i].DoDestroyThinkers = () => false;
        }

        collection.Thinkers[VmDThinkerCollectionEarly.MaxStatNum + 1].DoDestroyThinkers = () =>
        {
            destroyed.Add(VmDThinkerCollectionEarly.MaxStatNum + 1);
            return false;
        };

        VmDThinkerCollectionEarly.DestroyAllThinkers(new VmDThinkerCollectionEarly.DestroyAllContext
        {
            Collection = collection,
        });

        Assert.DoesNotContain(VmDThinkerCollectionEarly.StatTravelling, destroyed);
        Assert.DoesNotContain(VmDThinkerCollectionEarly.StatStatic, destroyed);
        Assert.Contains(VmDThinkerCollectionEarly.MaxStatNum + 1, destroyed);
    }

    [Fact]
    public void CleanUpTravellers_DestroyListAndStripFresh()
    {
        var collection = EmptyCollection();
        var removedFresh = 0;
        collection.FreshThinkers[ThinkerStat.Default].RemoveTravellers = _ => removedFresh++;
        collection.Thinkers[ThinkerStat.Default].RemoveTravellers = _ => { };
        var destroyedStat = -1;
        VmDThinkerCollectionEarly.CleanUpTravellers(new VmDThinkerCollectionEarly.CleanUpTravellersContext
        {
            Collection = collection,
            SaveGame = true,
            DestroyThinkersInList = stat => destroyedStat = stat,
        });
        Assert.Equal(VmDThinkerCollectionEarly.StatTravelling, destroyedStat);
        Assert.Equal(1, removedFresh);
    }

    [Fact]
    public void OnLoad_InvokesBothListKinds()
    {
        var collection = EmptyCollection(2);
        var loads = 0;
        collection.FreshThinkers[0].OnLoad = () => loads++;
        collection.Thinkers[1].OnLoad = () => loads++;
        VmDThinkerCollectionEarly.OnLoad(collection);
        Assert.Equal(2, loads);
    }

    [Fact]
    public void CompareProfileEntries_SortsByTotalTimeDescending()
    {
        var left = new VmDThinkerCollectionEarly.ProfileEntry { TimeMs = 1 };
        var right = new VmDThinkerCollectionEarly.ProfileEntry { TimeMs = 5 };
        Assert.True(VmDThinkerCollectionEarly.CompareProfileEntries(left, right, 0) > 0);
    }
}
