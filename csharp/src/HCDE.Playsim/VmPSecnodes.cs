using System.Runtime.InteropServices;

namespace HCDE.Playsim;

/// <summary>Boom secnode freelist and `msecnode_t` list helpers from <c>p_secnodes.cpp</c> (42–283).</summary>
public static class VmPSecnodes
{
    public sealed class MsecNode
    {
        public object? Sector { get; set; }
        public object? Thing { get; set; }
        public MsecNode? ThingPrev { get; set; }
        public MsecNode? ThingNext { get; set; }
        public MsecNode? SectorPrev { get; set; }
        public MsecNode? SectorNext { get; set; }
        public int Visited { get; set; }
    }

    static readonly Stack<MsecNode> FreeList = new();

    public static MsecNode GetSecnode()
    {
        if (FreeList.Count > 0)
            return FreeList.Pop();
        return new MsecNode();
    }

    public static void PutSecnode(MsecNode node)
    {
        node.Sector = null;
        node.Thing = null;
        node.ThingPrev = null;
        node.ThingNext = null;
        node.SectorPrev = null;
        node.SectorNext = null;
        node.Visited = 0;
        FreeList.Push(node);
    }

    public static void ClearThingMarks(MsecNode? sectorList)
    {
        for (var node = sectorList; node != null; node = node.ThingNext)
            node.Thing = null;
    }

    public static MsecNode? PruneUnmarkedNodes(MsecNode? sectorList, SectorHeadMap heads)
    {
        var node = sectorList;
        while (node != null)
        {
            if (node.Thing == null)
            {
                if (ReferenceEquals(node, sectorList))
                    sectorList = node.ThingNext;
                ref var head = ref heads.Head(node.Sector!);
                node = DelSecnode(node, ref head);
            }
            else
                node = node.ThingNext;
        }
        return sectorList;
    }

    public static MsecNode? AddSecnode(
        object sector,
        object thing,
        MsecNode? nextOnThing,
        ref MsecNode? sectorThingHead)
    {
        if (sector == null!)
            throw new InvalidOperationException("AddSecnode of null sector");
        var node = nextOnThing;
        while (node != null)
        {
            if (ReferenceEquals(node.Sector, sector))
            {
                node.Thing = thing;
                return nextOnThing;
            }
            node = node.ThingNext;
        }
        var newNode = GetSecnode();
        newNode.Visited = 0;
        newNode.Sector = sector;
        newNode.Thing = thing;
        newNode.ThingPrev = null;
        newNode.ThingNext = nextOnThing;
        if (nextOnThing != null)
            nextOnThing.ThingPrev = newNode;
        newNode.SectorPrev = null;
        newNode.SectorNext = sectorThingHead;
        if (sectorThingHead != null)
            sectorThingHead.SectorPrev = newNode;
        sectorThingHead = newNode;
        return newNode;
    }

    public static MsecNode? DelSecnode(MsecNode? node, ref MsecNode? sectorThingHead)
    {
        if (node == null)
            return null;
        var tp = node.ThingPrev;
        var tn = node.ThingNext;
        if (tp != null)
            tp.ThingNext = tn;
        if (tn != null)
            tn.ThingPrev = tp;
        var sp = node.SectorPrev;
        var sn = node.SectorNext;
        if (sp != null)
            sp.SectorNext = sn;
        else
            sectorThingHead = sn;
        if (sn != null)
            sn.SectorPrev = sp;
        PutSecnode(node);
        return tn;
    }

    public static void DelSeclist(MsecNode? node, ref MsecNode? sectorThingHead)
    {
        while (node != null)
            node = DelSecnode(node, ref sectorThingHead);
    }

    public sealed class SectorHeadMap
    {
        readonly Dictionary<object, MsecNode?> _heads = new();

        public ref MsecNode? Head(object sector)
        {
            ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(_heads, sector, out var exists);
            if (!exists)
                slot = null;
            return ref slot;
        }
    }

    public sealed class CreateListContext
    {
        public object Thing { get; init; } = null!;
        public object HomeSector { get; init; } = null!;
        public IEnumerable<LineCrossing>? LineCrossings { get; init; }
        public Func<LineCrossing, bool>? IncludeLineCrossing { get; init; }
        public required SectorHeadMap SectorHeads { get; init; }
    }

    public readonly record struct LineCrossing(object FrontSector, object? BackSector, object? Line = null);

    public static void AppendLineCrossingSectors(
        ref MsecNode? sectorList,
        CreateListContext ctx,
        LineCrossing cross)
    {
        ref var frontHead = ref ctx.SectorHeads.Head(cross.FrontSector);
        sectorList = AddSecnode(cross.FrontSector, ctx.Thing, sectorList, ref frontHead);
        if (cross.BackSector != null)
        {
            ref var backHead = ref ctx.SectorHeads.Head(cross.BackSector);
            sectorList = AddSecnode(cross.BackSector, ctx.Thing, sectorList, ref backHead);
        }
    }

    public static MsecNode? CreateSecNodeList(MsecNode? sectorList, CreateListContext ctx)
    {
        ClearThingMarks(sectorList);
        if (ctx.LineCrossings != null)
        {
            foreach (var cross in ctx.LineCrossings)
            {
                if (ctx.IncludeLineCrossing != null && !ctx.IncludeLineCrossing(cross))
                    continue;
                AppendLineCrossingSectors(ref sectorList, ctx, cross);
            }
        }
        ref var homeHead = ref ctx.SectorHeads.Head(ctx.HomeSector);
        sectorList = AddSecnode(ctx.HomeSector, ctx.Thing, sectorList, ref homeHead);
        return PruneUnmarkedNodes(sectorList, ctx.SectorHeads);
    }
}
