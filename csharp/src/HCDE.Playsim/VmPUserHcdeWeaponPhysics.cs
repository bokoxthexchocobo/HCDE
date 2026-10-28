namespace HCDE.Playsim;

/// <summary>HCDE weapon prediction gates and <c>FPhysicsLinkBackup</c> from <c>p_user.cpp</c> (112–386).</summary>
public static class VmPUserHcdeWeaponPhysics
{
    public const int CfPredicting = 1;
    public const int PstLive = 0;
    public const int Maxplayers = 64;

    public static bool ClientPredictsLocalWeapon(
        bool clPredictWeapon,
        bool netgame,
        bool demoplayback,
        bool usesDedicatedSlot,
        bool localAuthority,
        int consoleplayer,
        bool isConsolePlayerMo) =>
        clPredictWeapon
        && netgame
        && !demoplayback
        && usesDedicatedSlot
        && !localAuthority
        && consoleplayer >= 0
        && consoleplayer < Maxplayers
        && isConsolePlayerMo;

    public static bool ShouldTickWeaponPsprites(
        bool playerPresent,
        bool clientPredictsLocal,
        int cheats,
        bool hcdeWeaponPredictNewTic,
        int playerstate,
        int pstLive)
    {
        if (!playerPresent)
            return false;
        if (!clientPredictsLocal)
            return (cheats & CfPredicting) == 0;
        if ((cheats & CfPredicting) != 0)
            return hcdeWeaponPredictNewTic;
        return playerstate != pstLive;
    }

    public static bool NetShouldTickWeaponPsprites(bool playerPresent, bool shouldTick) =>
        playerPresent && shouldTick;

    public sealed class PhysicsLinkBackup
    {
        public Dictionary<object, bool> ValidSectors { get; } = new();
        public List<int> TouchingSectorPositions { get; } = new();
        public Dictionary<int, bool> ValidBlockNodes { get; } = new();
        public List<int> BlockmapPositions { get; } = new();
        public int ThingPosition { get; set; } = -1;
        public bool ThingInSector { get; set; }

        public static int IndexOnSectorThingList(VmPSecnodes.MsecNode? head, object thing)
        {
            var index = 0;
            for (var sNode = head; sNode != null; sNode = sNode.SectorNext)
            {
                if (ReferenceEquals(sNode.Thing, thing))
                    return index - 1;
                index++;
            }

            return -1;
        }

        public static PhysicsLinkBackup CaptureFromTouching(
            VmPSecnodes.MsecNode? touchingList,
            VmPSecnodes.SectorHeadMap sectorHeads,
            IReadOnlyList<(int BlockIndex, VmPSecnodes.MsecNode? Head, object Actor)> blockNodes,
            object? sector,
            IReadOnlyList<object?> sectorThingChain,
            object actor)
        {
            var bak = new PhysicsLinkBackup();
            for (var node = touchingList; node != null; node = node.ThingNext)
            {
                ref var head = ref sectorHeads.Head(node.Sector!);
                bak.TouchingSectorPositions.Add(IndexOnSectorThingList(head, actor));
                bak.ValidSectors[node.Sector!] = true;
            }

            if (sector != null)
            {
                for (var i = 0; i < sectorThingChain.Count; i++)
                {
                    if (ReferenceEquals(sectorThingChain[i], actor))
                    {
                        bak.ThingInSector = true;
                        bak.ThingPosition = i;
                        break;
                    }
                }
            }

            foreach (var (blockIndex, head, me) in blockNodes)
            {
                var index = 0;
                for (var bNode = head; bNode != null; bNode = bNode.ThingNext)
                {
                    if (ReferenceEquals(bNode.Thing, me))
                        break;
                    index++;
                }

                bak.BlockmapPositions.Add(index);
                bak.ValidBlockNodes[blockIndex] = true;
            }

            return bak;
        }

        public static void RelinkSectorThingNode(
            VmPSecnodes.MsecNode node,
            int savedIndex,
            VmPSecnodes.SectorHeadMap sectorHeads)
        {
            ref var head = ref sectorHeads.Head(node.Sector!);
            if (node.SectorPrev == null)
                head = node.SectorNext;
            else
                node.SectorPrev.SectorNext = node.SectorNext;
            if (node.SectorNext != null)
                node.SectorNext.SectorPrev = node.SectorPrev;

            var i = 0;
            VmPSecnodes.MsecNode? prev = null;
            VmPSecnodes.MsecNode? next;
            for (next = head; next != null; prev = next, next = next.SectorNext)
            {
                if (i > savedIndex)
                    break;
                i++;
            }

            node.SectorPrev = prev;
            node.SectorNext = next;
            if (prev != null)
                prev.SectorNext = node;
            if (next != null)
                next.SectorPrev = node;
            if (prev == null)
                head = node;
        }

        public sealed class PhysicsRestoreContext
        {
            public VmPSecnodes.SectorHeadMap SectorHeads { get; init; } = new();
            public Func<VmPSecnodes.MsecNode?> GetTouchingSectorList { get; init; } = () => null;
            public Action<VmPSecnodes.MsecNode?> SetTouchingSectorList { get; init; } = _ => { };
            public VmPMapUtlActorWorldLink.ActorLinkState? ActorInSector { get; init; }
            public VmPMapUtlActorWorldLink.SectorThingListHead? SectorThingList { get; init; }
            public Func<VmPMapUtlActorWorldLink.BlockNode?> GetBlockNodeHead { get; init; } = () => null;
            public Func<VmPMapUtlActorWorldLink.BlockNode, int> BlockIndex { get; init; } = _ => 0;
            public Func<int, VmPMapUtlActorWorldLink.BlockNode?> GetBlockLinkHead { get; init; } = _ => null;
            public Action<int, VmPMapUtlActorWorldLink.BlockNode?> SetBlockLinkHead { get; init; } = (_, _) => { };
            public Action<VmPMapUtlActorWorldLink.BlockNode>? ReleaseBlockNode { get; init; }
        }

        public void Restore(PhysicsRestoreContext ctx)
        {
            var count = 0;
            for (var node = ctx.GetTouchingSectorList(); node != null;)
            {
                if (!ValidSectors.ContainsKey(node.Sector!))
                {
                    var next = node.ThingNext;
                    if (ReferenceEquals(node, ctx.GetTouchingSectorList()))
                        ctx.SetTouchingSectorList(next);
                    ref var sectorHead = ref ctx.SectorHeads.Head(node.Sector!);
                    VmPSecnodes.DelSecnode(node, ref sectorHead);
                    node = next;
                    continue;
                }

                if (count >= TouchingSectorPositions.Count)
                {
                    node = node.ThingNext;
                    continue;
                }

                RelinkSectorThingNode(node, TouchingSectorPositions[count], ctx.SectorHeads);
                count++;
                node = node.ThingNext;
            }

            if (ThingInSector && ctx.ActorInSector is not null && ctx.SectorThingList is not null)
                RelinkActorSectorThingList(ctx.ActorInSector, ctx.SectorThingList, ThingPosition);

            count = 0;
            for (var block = ctx.GetBlockNodeHead(); block != null;)
            {
                var blockIndex = ctx.BlockIndex(block);
                if (!ValidBlockNodes.ContainsKey(blockIndex))
                {
                    var next = block.NextBlock;
                    if (block.NextActor is not null)
                        block.NextActor.PrevActorPtr = block.PrevActorPtr;
                    if (block.PrevActorPtr is not null)
                        block.PrevActorPtr.NextActor = block.NextActor;
                    if (next is not null && block.PrevBlockPtr is not null)
                        next.PrevBlockPtr = block.PrevBlockPtr;
                    if (block.PrevBlockPtr is not null)
                        block.PrevBlockPtr.NextBlock = next;
                    ctx.ReleaseBlockNode?.Invoke(block);
                    block = next;
                    continue;
                }

                if (count >= BlockmapPositions.Count)
                {
                    block = block.NextBlock;
                    continue;
                }

                RelinkBlockmapNode(block, blockIndex, BlockmapPositions[count], ctx);
                count++;
                block = block.NextBlock;
            }
        }

        public static void RelinkActorSectorThingList(
            VmPMapUtlActorWorldLink.ActorLinkState actor,
            VmPMapUtlActorWorldLink.SectorThingListHead sectorList,
            int savedIndex)
        {
            if (actor.PrevHead is not null)
            {
                var next = actor.NextInSector;
                actor.PrevHead.Head = next;
                if (next is not null)
                    next.PrevHead = actor.PrevHead;
            }

            var i = 0;
            VmPMapUtlActorWorldLink.ActorLinkState? prev = null;
            VmPMapUtlActorWorldLink.ActorLinkState? nextLink;
            for (nextLink = sectorList.Head; nextLink is not null; prev = nextLink, nextLink = nextLink.NextInSector)
            {
                if (i > savedIndex)
                    break;
                i++;
            }

            actor.NextInSector = nextLink;
            actor.PrevHead = sectorList;
            if (nextLink is not null)
                nextLink.PrevHead = sectorList;
            if (prev is not null)
                prev.NextInSector = actor;
            else
                sectorList.Head = actor;
        }

        public static void RelinkBlockmapNode(
            VmPMapUtlActorWorldLink.BlockNode node,
            int blockIndex,
            int savedIndex,
            PhysicsRestoreContext ctx)
        {
            if (node.PrevActorPtr is not null)
                node.PrevActorPtr.NextActor = node.NextActor;
            if (node.NextActor is not null)
                node.NextActor.PrevActorPtr = node.PrevActorPtr;

            var i = 0;
            VmPMapUtlActorWorldLink.BlockNode? next = ctx.GetBlockLinkHead(blockIndex);
            VmPMapUtlActorWorldLink.BlockNode? prevPtr = null;
            for (; next is not null; prevPtr = next, next = next.NextActor)
            {
                if (i > savedIndex)
                    break;
                i++;
            }

            node.NextActor = next;
            if (next is not null)
                next.PrevActorPtr = node;
            node.PrevActorPtr = prevPtr;
            if (prevPtr is not null)
                prevPtr.NextActor = node;
            else
                ctx.SetBlockLinkHead(blockIndex, node);
        }
    }
}
