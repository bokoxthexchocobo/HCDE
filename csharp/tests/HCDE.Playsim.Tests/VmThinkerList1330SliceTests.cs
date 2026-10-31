namespace HCDE.Playsim.Tests;

public class VmThinkerList1330SliceTests
{
    [Fact]
    public void Link_BuildsTheRingAndDestroysIt()
    {
        var vm = new VmThinkerList1330();
        var list = vm.Thinkers[3];
        Assert.True(list.IsEmpty());
        Assert.Null(list.GetHead());
        Assert.Null(list.GetTail());

        var dying = Node(7, VmThinkerList1330.OfEuthanizeMe);
        Assert.Throws<InvalidOperationException>(() => list.AddTail(dying));
        Assert.Null(list.Sentinel);

        var linked = Node(8, 1);
        linked.Next = linked;
        Assert.Throws<InvalidOperationException>(() => list.AddTail(linked));
        Assert.Null(list.Sentinel);

        var first = Node(10, 1);
        list.AddTail(first);
        Assert.NotNull(list.Sentinel);
        int sentinelId = list.Sentinel!.Id;
        Assert.Equal(VmThinkerList1330.OfSentinel, list.Sentinel.Flags);
        Assert.False(list.IsEmpty());
        Assert.Same(first, list.GetHead());
        Assert.Same(first, list.GetTail());
        Assert.Same(list.Sentinel, first.Next);
        Assert.Same(list.Sentinel, first.Prev);
        Assert.Equal(
            new[]
            {
                "one:" + sentinelId,
                "10>" + sentinelId,
                "10>" + sentinelId,
                sentinelId + ">10",
                sentinelId + ">10",
            },
            vm.Barriers);

        var second = Node(11, 4);
        list.AddTail(second);
        Assert.Same(first, list.GetHead());
        Assert.Same(second, list.GetTail());
        Assert.Same(second, first.Next);
        Assert.Same(first, second.Prev);
        Assert.Same(list.Sentinel, second.Next);

        var freshOnly = Node(12, 1);
        vm.Fresh[4].AddTail(freshOnly);
        Assert.Same(first, vm.FirstThinker(3));
        Assert.Same(freshOnly, vm.FirstThinker(4));
        var top = Node(13, 1);
        vm.Thinkers[VmThinkerList1330.MaxStatNum].AddTail(top);
        Assert.Same(top, vm.FirstThinker(200));
        Assert.Same(top, vm.FirstThinker(-1));
        Assert.Null(vm.FirstThinker(0));

        vm.MarkRoots();
        Assert.Equal((VmThinkerList1330.MaxStatNum + 1) * 2 + 1, vm.Marks.Count);
        Assert.Equal("live:0:0", vm.Marks[0]);
        Assert.Equal("fresh:0:0", vm.Marks[1]);
        Assert.Equal("fresh:" + VmThinkerList1330.MaxStatNum + ":0", vm.Marks[^2]);
        Assert.Equal("live:" + (VmThinkerList1330.MaxStatNum + 1) + ":0", vm.Marks[^1]);
        Assert.Contains("live:3:" + sentinelId, vm.Marks);

        var ring = new VmThinkerList1330.Thinker
        {
            Id = 1,
            Flags = VmThinkerList1330.OfSentinel,
        };
        ring.Next = ring;
        ring.Prev = ring;
        var bare = vm.Thinkers[1];
        bare.Sentinel = ring;
        Assert.False(bare.IsEmpty());
        Assert.Null(bare.GetHead());
        Assert.Null(bare.GetTail());
        ring.Next = null;
        Assert.True(bare.IsEmpty());

        first.Prev = second;
        Assert.Throws<InvalidOperationException>(() => list.GetHead());
        first.Prev = list.Sentinel;

        var loose = Node(14, 0);
        loose.RunDestructor();
        loose.Next = first;
        Assert.Throws<InvalidOperationException>(() => loose.RunDestructor());

        var doomed = vm.Thinkers[6];
        var keep = Node(20, 1);
        var abort = Node(21, 2);
        var plain = Node(22, 3);
        var disk = Node(23, 4);
        abort.OnDestroy = () => throw new VmThinkerList1330.VmAbort(null, "trace");
        doomed.AddTail(keep);
        abort.OnDestroy = () => throw new VmThinkerList1330.VmAbort("boom", "at Foo");
        doomed.AddTail(abort);
        doomed.AddTail(plain);
        disk.OnDestroy = () => throw new VmThinkerList1330.RecoverableError("disk");
        doomed.AddTail(disk);
        var sentinel = doomed.Sentinel;
        var failed = Assert.Throws<InvalidOperationException>(() => doomed.DestroyThinkers());
        Assert.Equal(VmThinkerList1330.DestroyFailed, failed.Message);
        Assert.Null(doomed.Sentinel);
        Assert.NotNull(sentinel);
        Assert.True(sentinel!.Destroyed);
        Assert.True(keep.Destroyed);
        Assert.False(keep.Deleted);
        Assert.Equal(0, keep.Flags & VmThinkerList1330.OfYesReallyDelete);
        Assert.Null(keep.Next);
        Assert.Null(abort.Next);
        Assert.False(abort.Destroyed);
        Assert.True(abort.Deleted);
        Assert.Equal(VmThinkerList1330.OfYesReallyDelete | 2, abort.Flags);
        Assert.True(plain.Destroyed);
        Assert.False(disk.Destroyed);
        Assert.True(disk.Deleted);
        Assert.Equal(VmThinkerList1330.OfYesReallyDelete | 4, disk.Flags);
        Assert.Equal(new[] { sentinel.Id, keep.Id, abort.Id, plain.Id, disk.Id }, vm.DestroyOrder);
        Assert.Equal(
            new[]
            {
                new VmThinkerList1330.PrintLine(0, VmThinkerList1330.VmDestroyHeader),
                new VmThinkerList1330.PrintLine(
                    VmThinkerList1330.PrintBoldNoNotify,
                    VmThinkerList1330.TextRed + "boom\n"),
                new VmThinkerList1330.PrintLine(VmThinkerList1330.PrintBoldNoNotify, "at Foo"),
                new VmThinkerList1330.PrintLine(
                    VmThinkerList1330.PrintBoldNoNotify,
                    "Error in DestroyThinkers: disk\n"),
            },
            vm.Prints);

        var quiet = vm.Thinkers[8];
        var silent = Node(30, 1);
        silent.OnDestroy = () => throw new VmThinkerList1330.VmAbort(null, "only-trace");
        quiet.AddTail(silent);
        Assert.True(quiet.DoDestroyThinkers());
        Assert.Equal(
            new[]
            {
                new VmThinkerList1330.PrintLine(0, VmThinkerList1330.VmDestroyHeader),
                new VmThinkerList1330.PrintLine(VmThinkerList1330.PrintBoldNoNotify, "only-trace"),
            },
            vm.Prints.Skip(4));

        var blank = vm.Thinkers[9];
        var emptyMessage = Node(31, 1);
        emptyMessage.OnDestroy = () => throw new VmThinkerList1330.VmAbort("", "tail");
        blank.AddTail(emptyMessage);
        Assert.True(blank.DoDestroyThinkers());
        Assert.Equal(VmThinkerList1330.TextRed + "\n", vm.Prints[^2].Text);
        Assert.Equal(VmThinkerList1330.PrintBoldNoNotify, vm.Prints[^2].Flags);

        var boom = vm.Thinkers[2];
        var left = Node(40, 1);
        boom.AddTail(left);
        var created = boom.Sentinel!;
        created.OnDestroy = () => throw new InvalidOperationException("sentinel boom");
        var exploded = Assert.Throws<InvalidOperationException>(() => boom.DestroyThinkers());
        Assert.Equal("sentinel boom", exploded.Message);
        Assert.Same(created, boom.Sentinel);
        Assert.Null(created.Next);
        Assert.Null(left.Next);
        Assert.False(left.Destroyed);
        Assert.False(left.Deleted);
        Assert.False(created.Destroyed);

        Assert.False(vm.Thinkers[0].DoDestroyThinkers());
    }

    [Fact]
    public void Tick_SavesTravellersAndProfiles()
    {
        var vm = new VmThinkerList1330();
        var saved = vm.Thinkers[10];
        Assert.Empty(vm.Serialized);
        saved.SaveList();
        Assert.Empty(vm.Serialized);

        var alpha = Node(50, 1, "Alpha");
        var beta = Node(51, 1, "Beta");
        saved.AddTail(alpha);
        saved.AddTail(beta);
        beta.Flags |= VmThinkerList1330.OfEuthanizeMe;
        Assert.Throws<InvalidOperationException>(() => saved.SaveList());
        Assert.Empty(vm.Serialized);

        var clean = vm.Thinkers[11];
        var gamma = Node(52, 1);
        var delta = Node(53, 1);
        clean.AddTail(gamma);
        clean.AddTail(delta);
        clean.SaveList();
        Assert.Equal(new[] { gamma, delta }, vm.Serialized);

        var travel = vm.Thinkers[12];
        var moving = Node(60, VmThinkerList1330.OfTravelling | 8);
        var stay = Node(61, 8);
        var both = Node(62, VmThinkerList1330.OfTravelling | 8);
        var leaving = Node(63, VmThinkerList1330.OfTravelling);
        travel.AddTail(moving);
        travel.AddTail(stay);
        travel.AddTail(both);
        travel.AddTail(leaving);
        both.Flags |= VmThinkerList1330.OfEuthanizeMe;
        var marker = Node(64, 0);
        vm.NextToThink = marker;
        travel.RemoveTravellers(saveGame: false);
        Assert.True(moving.Destroyed);
        Assert.Null(moving.Next);
        Assert.False(stay.Destroyed);
        Assert.Same(stay, travel.GetHead());
        Assert.Equal(VmThinkerList1330.OfTravelling | VmThinkerList1330.OfEuthanizeMe | 8, both.Flags);
        Assert.Same(both, stay.Next);
        Assert.True(leaving.Destroyed);
        Assert.Same(travel.Sentinel, vm.NextToThink);

        var parked = vm.Thinkers[13];
        vm.NextToThink = marker;
        parked.RemoveTravellers(saveGame: true);
        Assert.Same(marker, vm.NextToThink);
        var kept = Node(65, VmThinkerList1330.OfTravelling | 8);
        parked.AddTail(kept);
        parked.RemoveTravellers(saveGame: true);
        Assert.Equal(8, kept.Flags);
        Assert.False(kept.Destroyed);
        Assert.Same(kept, parked.GetHead());

        var loading = vm.Thinkers[14];
        var loadA = Node(70, 1);
        var loadB = Node(71, 1);
        var loadC = Node(72, 1);
        loadA.HasOnLoad = true;
        loadC.HasOnLoad = true;
        loadB.HasOnLoad = true;
        loadB.OnLoad = () => { };
        loadA.OnLoad = () => loadB.Remove();
        loading.AddTail(loadA);
        loading.AddTail(loadB);
        loading.AddTail(loadC);
        loading.OnLoad();
        Assert.Equal(1, loadA.LoadCalls);
        Assert.Equal(0, loadB.LoadCalls);
        Assert.Null(loadB.Next);
        Assert.Equal(1, loadC.LoadCalls);
        Assert.Same(loading.Sentinel, vm.NextToThink);

        var skipped = vm.Thinkers[15];
        var quiet = Node(73, 1);
        var dead = Node(74, 1);
        quiet.OnLoad = () => quiet.LoadCalls = 9;
        dead.HasOnLoad = true;
        dead.OnLoad = () => dead.LoadCalls = 9;
        skipped.AddTail(quiet);
        skipped.AddTail(dead);
        dead.Flags |= VmThinkerList1330.OfEuthanizeMe;
        skipped.OnLoad();
        Assert.Equal(0, quiet.LoadCalls);
        Assert.Equal(0, dead.LoadCalls);

        var idle = vm.Thinkers[16];
        Assert.Equal(0, idle.TickThinkers(null));
        Assert.Equal(0, vm.ThinkCount);

        var live = vm.Thinkers[17];
        var spawned = Node(80, VmThinkerList1330.OfJustSpawned | 1);
        var older = Node(81, 1);
        live.AddTail(spawned);
        live.AddTail(older);
        Assert.Equal(2, live.TickThinkers(null));
        Assert.Equal(2, vm.ThinkCount);
        Assert.Equal(1, spawned.PostBegins);
        Assert.Equal(1, spawned.TickCalls);
        Assert.Equal(VmThinkerList1330.OfSpawned | 1, spawned.Flags);
        Assert.Equal(1, older.TickCalls);
        Assert.Equal(0, older.PostBegins);
        Assert.Same(spawned, live.GetHead());

        var fresh = vm.Fresh[18];
        var moved = Node(82, VmThinkerList1330.OfJustSpawned | 2);
        fresh.AddTail(moved);
        var dest = vm.Thinkers[18];
        Assert.Equal(1, fresh.TickThinkers(dest));
        Assert.Null(fresh.GetHead());
        Assert.False(fresh.IsEmpty());
        Assert.Same(moved, dest.GetHead());
        Assert.Equal(VmThinkerList1330.OfSpawned | 2, moved.Flags);
        Assert.Equal(3, vm.ThinkCount);

        var blocked = vm.Fresh[19];
        var newborn = Node(83, VmThinkerList1330.OfJustSpawned);
        blocked.AddTail(newborn);
        newborn.Flags |= VmThinkerList1330.OfEuthanizeMe;
        Assert.Throws<InvalidOperationException>(() => blocked.TickThinkers(dest));
        Assert.Null(newborn.Next);
        Assert.False(newborn.Destroyed);
        Assert.Equal(0, newborn.PostBegins);

        var stale = vm.Fresh[20];
        var aged = Node(84, 1);
        var waiting = Node(85, VmThinkerList1330.OfJustSpawned);
        stale.AddTail(aged);
        stale.AddTail(waiting);
        var error = Assert.Throws<InvalidOperationException>(() => stale.TickThinkers(dest));
        Assert.Equal(VmThinkerList1330.FreshAlreadyTicked, error.Message);
        Assert.Equal(0, aged.TickCalls);
        Assert.Equal(0, waiting.TickCalls);
        Assert.Same(waiting, vm.NextToThink);

        var profiled = vm.Thinkers[21];
        var impA = Node(90, 1, "Imp");
        var impB = Node(91, 1, "Imp");
        var baron = Node(92, 1, "Baron");
        impA.TickMillis = 1.5;
        impB.TickMillis = 9;
        baron.TickMillis = 2.5;
        profiled.AddTail(impA);
        profiled.AddTail(impB);
        profiled.AddTail(baron);
        impB.Flags |= VmThinkerList1330.OfEuthanizeMe;
        Assert.Equal(3, profiled.ProfileThinkers(null));
        Assert.Equal(5, vm.ThinkCount);
        Assert.Equal(1, vm.Profiles["Imp"].Calls);
        Assert.Equal(1, vm.Profiles["Imp"].Clocks);
        Assert.Equal(1, vm.Profiles["Imp"].Unclocks);
        Assert.Equal(1.5, vm.Profiles["Imp"].Time);
        Assert.Equal(1, vm.Profiles["Baron"].Calls);
        Assert.Equal(1, vm.Profiles["Baron"].Unclocks);
        Assert.Equal(2.5, vm.Profiles["Baron"].Time);
        Assert.Equal(0, impB.TickCalls);
        Assert.Same(impA, profiled.GetHead());
        Assert.Same(impB, impA.Next);
        Assert.Same(baron, impB.Next);

        var ferry = vm.Fresh[22];
        var mover = Node(93, VmThinkerList1330.OfJustSpawned | 1, "Mover");
        mover.TickMillis = 0.25;
        ferry.AddTail(mover);
        var other = vm.Thinkers[22];
        Assert.Equal(1, ferry.ProfileThinkers(other));
        Assert.Equal(6, vm.ThinkCount);
        Assert.Same(mover, other.GetTail());
        Assert.Equal(VmThinkerList1330.OfSpawned | 1, mover.Flags);
        Assert.Equal(0.25, vm.Profiles["Mover"].Time);
        Assert.Null(ferry.GetHead());
        Assert.False(ferry.IsEmpty());

        var late = vm.Thinkers[23];
        var baby = Node(94, VmThinkerList1330.OfJustSpawned | 1, "Baby");
        baby.OnTick = () => throw new InvalidOperationException("baby");
        late.AddTail(baby);
        var babyError = Assert.Throws<InvalidOperationException>(() => late.ProfileThinkers(null));
        Assert.Equal("baby", babyError.Message);
        Assert.Equal(1, vm.Profiles["Baby"].Calls);
        Assert.Equal(1, vm.Profiles["Baby"].Clocks);
        Assert.Equal(0, vm.Profiles["Baby"].Unclocks);
        Assert.Equal(0, vm.Profiles["Baby"].Time);
        Assert.Equal(VmThinkerList1330.OfJustSpawned | VmThinkerList1330.OfSpawned | 1, baby.Flags);
        Assert.Equal(1, baby.PostBegins);
        Assert.Equal(1, baby.TickCalls);
        Assert.Equal(7, vm.ThinkCount);
    }

    static VmThinkerList1330.Thinker Node(int id, int flags, string type = "Actor")
    {
        return new VmThinkerList1330.Thinker
        {
            Id = id,
            Flags = flags,
            TypeName = type,
        };
    }
}
