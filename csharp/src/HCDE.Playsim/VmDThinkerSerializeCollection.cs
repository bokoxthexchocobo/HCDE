namespace HCDE.Playsim;

public static class VmDThinkerSerializeCollection
{
    public const int StatStatic = VmDThinkerCollectionEarly.StatStatic;
    public const int OfJustSpawned = VmDThinkerListOps.OfJustSpawned;
    public const int OfEuthanizeMe = VmDThinkerListOps.OfEuthanizeMe;
    public const int MaxStatNum = VmDThinkerListOps.MaxStatNum;

    public static void SerializeThinkersWrite(
        VmDThinkerListOps.CollectionModel collection,
        Action<string?> beginArray,
        Action endArray,
        Action<VmDThinkerListOps.ThinkerNodeModel> serializeNode)
    {
        beginArray("thinkers");
        for (var i = 0; i <= MaxStatNum; i++)
        {
            beginArray(null);
            WalkList(collection.Thinkers[i], serializeNode);
            WalkList(collection.FreshThinkers[i], serializeNode);
            endArray();
        }

        endArray();
    }

    public static void SerializeThinkersRead(
        VmDThinkerListOps.CollectionModel collection,
        bool hubLoad,
        Func<bool> beginOuterArray,
        Func<bool> beginStatArray,
        Func<int> statArraySize,
        Func<int, VmDThinkerListOps.ThinkerNodeModel?> readThinker,
        Action endStatArray,
        Action endOuterArray)
    {
        if (!beginOuterArray())
            return;

        for (var i = 0; i <= MaxStatNum; i++)
        {
            if (!beginStatArray())
                continue;

            if (!hubLoad || i != StatStatic)
            {
                var size = statArraySize();
                for (var j = 0; j < size; j++)
                {
                    var thinker = readThinker(j);
                    if (thinker == null)
                        continue;

                    if (thinker.NextThinker != null)
                        VmDThinkerInstance.Remove(thinker, null);

                    if ((thinker.ObjectFlags & OfEuthanizeMe) != 0)
                        continue;

                    if ((thinker.ObjectFlags & OfJustSpawned) != 0)
                    {
                        VmDThinkerListOps.AddTail(collection.FreshThinkers[i], thinker);
                        thinker.PostSerialize?.Invoke();
                    }
                    else
                    {
                        VmDThinkerListOps.AddTail(collection.Thinkers[i], thinker);
                        thinker.PostSerialize?.Invoke();
                    }
                }
            }

            endStatArray();
        }

        endOuterArray();
    }

    private static void WalkList(
        VmDThinkerListOps.ThinkerListModel list,
        Action<VmDThinkerListOps.ThinkerNodeModel> serializeNode)
    {
        var node = VmDThinkerListOps.GetHead(list);
        var sentinel = list.Sentinel;
        while (node != null && node != sentinel)
        {
            serializeNode(node);
            node = node.NextThinker;
        }
    }
}
