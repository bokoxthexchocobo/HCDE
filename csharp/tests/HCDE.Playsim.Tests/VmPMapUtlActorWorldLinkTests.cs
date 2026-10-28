namespace HCDE.Playsim.Tests;

public class VmPMapUtlActorWorldLinkTests
{
    [Fact]
    public void UnlinkFromWorld_DetachesSectorListAndBlockNodes()
    {
        var head = new VmPMapUtlActorWorldLink.SectorThingListHead();
        var actor = new VmPMapUtlActorWorldLink.ActorLinkState { PrevHead = head, SectorHead = head };
        head.Head = actor;
        var node = new VmPMapUtlActorWorldLink.BlockNode();
        actor.BlockNodeHead = node;
        var released = 0;
        VmPMapUtlActorWorldLink.UnlinkFromWorld(new VmPMapUtlActorWorldLink.UnlinkContext
        {
            Actor = actor,
            ReleaseBlockNode = _ => released++,
        });
        Assert.Null(head.Head);
        Assert.Null(actor.BlockNodeHead);
        Assert.Equal(1, released);
    }

    [Fact]
    public void FixMapthingPos_SkipsCoplanarTwoSidedLines()
    {
        var ctx = new VmPMapUtlActorWorldLink.FixMapthingPosContext
        {
            Lines =
            [
                new VmPMapUtlActorWorldLink.MapLineDef
                {
                    BackSector = new object(),
                    CoplanarFloorsCeilings = true,
                },
            ],
            InterceptDistanceToLine = (_, _) => 0,
        };
        Assert.False(VmPMapUtlActorWorldLink.FixMapthingPos(ctx));
    }

    [Fact]
    public void FixMapthingPos_PushesOffNearbyLine()
    {
        var ctx = new VmPMapUtlActorWorldLink.FixMapthingPosContext
        {
            X = 0,
            Y = 0,
            Radius = 16,
            Lines =
            [
                new VmPMapUtlActorWorldLink.MapLineDef
                {
                    Index = 1,
                    BboxLeft = -32,
                    BboxRight = 32,
                    BboxBottom = -32,
                    BboxTop = 32,
                    DeltaX = 1,
                    DeltaY = 0,
                },
            ],
            InterceptDistanceToLine = (_, _) => 4,
        };
        Assert.True(VmPMapUtlActorWorldLink.FixMapthingPos(ctx));
        Assert.True(ctx.Y < 0);
    }

    [Fact]
    public void UnlinkFromWorld_SavesTouchingListsIntoContext()
    {
        var saved = new VmPMapUtlActorWorldLink.LinkContext();
        var list = new object();
        var actor = new VmPMapUtlActorWorldLink.ActorLinkState { TouchingSectorList = list };
        VmPMapUtlActorWorldLink.UnlinkFromWorld(new VmPMapUtlActorWorldLink.UnlinkContext
        {
            Actor = actor,
            SaveCtx = saved,
        });
        Assert.Same(list, saved.SectorList);
        Assert.Null(actor.TouchingSectorList);
    }

    [Fact]
    public void LinkToWorld_InsertsIntoSectorList()
    {
        var head = new VmPMapUtlActorWorldLink.SectorThingListHead();
        var sector = new object();
        var actor = new VmPMapUtlActorWorldLink.ActorLinkState();
        VmPMapUtlActorWorldLink.LinkToWorld(new VmPMapUtlActorWorldLink.LinkToWorldContext
        {
            Actor = actor,
            SectorHint = sector,
            InsertSectorThingList = (a, _) =>
            {
                a.PrevHead = head;
                var next = head.Head;
                if (next is not null)
                    next.PrevHead = head;
                head.Head = a;
            },
            PointInRenderSubsector = _ => new object(),
            CollectPortalGroups = _ => Array.Empty<int>(),
        });
        Assert.Same(actor, head.Head);
        Assert.Same(sector, actor.Sector);
    }

    [Fact]
    public void LinkToWorld_RestoresSavedSectorNodesFromContext()
    {
        var savedList = new object();
        var actor = new VmPMapUtlActorWorldLink.ActorLinkState();
        object? captured = null;
        VmPMapUtlActorWorldLink.LinkToWorld(new VmPMapUtlActorWorldLink.LinkToWorldContext
        {
            Actor = actor,
            RestoreCtx = new VmPMapUtlActorWorldLink.LinkContext { SectorList = savedList },
            SectorHint = new object(),
            CreateSecNodeList = (_, restore) =>
            {
                captured = restore;
                return new object();
            },
            PointInRenderSubsector = _ => new object(),
            CollectPortalGroups = _ => Array.Empty<int>(),
        });
        Assert.Same(savedList, captured);
    }

    [Fact]
    public void SetOrigin_RelinksAfterCoordinateChange()
    {
        var actor = new VmPMapUtlActorWorldLink.ActorLinkState { X = 1, Y = 2, Z = 3 };
        var unlinked = false;
        var linked = false;
        VmPMapUtlActorWorldLink.SetOrigin(
            new VmPMapUtlActorWorldLink.LinkContext(),
            actor,
            10,
            20,
            30,
            moving: false,
            _ => unlinked = true,
            _ => linked = true,
            null,
            null);
        Assert.True(unlinked);
        Assert.True(linked);
        Assert.Equal(10, actor.X);
    }
}
