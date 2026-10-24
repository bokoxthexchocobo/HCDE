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
        bool clientPredictsLocal,
        int cheats,
        bool hcdeWeaponPredictNewTic,
        int playerstate,
        int pstLive) =>
        clientPredictsLocal
            ? (cheats & CfPredicting) != 0
                ? hcdeWeaponPredictNewTic
                : playerstate != pstLive
            : (cheats & CfPredicting) == 0;

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
    }
}
