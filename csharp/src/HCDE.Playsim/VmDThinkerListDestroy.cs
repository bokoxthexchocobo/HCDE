namespace HCDE.Playsim;

public static class VmDThinkerListDestroy
{
    public sealed class ThinkerNode
    {
        public int ObjectFlags { get; set; }
        public ThinkerNode? Next { get; set; }
        public ThinkerNode? Prev { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class ListModel
    {
        public ThinkerNode? Sentinel { get; set; }
    }

    public const int OfSentinel = 1 << 20;
    public const int OfTravelling = 1 << 21;
    public const int OfEuthanizeMe = 1 << 22;

    public static void DestroyThinkers(ListModel list, Action<ListModel>? onError = null)
    {
        if (DoDestroyThinkers(list))
            onError?.Invoke(list);
    }

    public static bool DoDestroyThinkers(ListModel list)
    {
        var error = false;
        var sentinel = list.Sentinel;
        if (sentinel == null)
            return false;

        var toDelete = new List<ThinkerNode>();
        var node = sentinel.Next;
        while (node != null && node != sentinel)
        {
            var next = node.Next;
            node.Next = node.Prev = null;
            toDelete.Add(node);
            node = next;
        }
        sentinel.Next = sentinel.Prev = null;
        sentinel.Destroyed = true;
        list.Sentinel = null;

        foreach (var item in toDelete)
            item.Destroyed = true;
        return error;
    }

    public static void SaveList(ListModel list, Action<ThinkerNode>? serializeNode)
    {
        var node = GetHead(list);
        while (node != null && (node.ObjectFlags & OfSentinel) == 0)
        {
            serializeNode?.Invoke(node);
            node = node.Next;
        }
    }

    public static void RemoveTravellers(ListModel list, bool saveGame)
    {
        var node = GetHead(list);
        while (node != null && node != list.Sentinel)
        {
            var next = node.Next;
            if ((node.ObjectFlags & OfTravelling) != 0 && (node.ObjectFlags & OfEuthanizeMe) == 0)
            {
                if (saveGame)
                    node.ObjectFlags &= ~OfTravelling;
                else
                    node.Destroyed = true;
            }
            node = next;
        }
    }

    public static void OnLoad(ListModel list, Action<ThinkerNode>? onLoadNode)
    {
        var node = GetHead(list);
        while (node != null && node != list.Sentinel)
        {
            var next = node.Next;
            if ((node.ObjectFlags & OfEuthanizeMe) == 0)
                onLoadNode?.Invoke(node);
            node = next;
        }
    }

    static ThinkerNode? GetHead(ListModel list)
    {
        var sentinel = list.Sentinel;
        if (sentinel?.Next == null || sentinel.Next == sentinel)
            return null;
        return sentinel.Next;
    }
}
