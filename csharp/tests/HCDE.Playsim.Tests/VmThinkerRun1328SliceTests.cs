namespace HCDE.Playsim.Tests;

public class VmThinkerRun1328SliceTests
{
    const int Span = VmThinkerRun1328.MaxStatNum - VmThinkerRun1328.StatFirstThinking + 1;

    [Fact]
    public void Link_TicksFreshListsAndReportsDestroy()
    {
        var vm = new VmThinkerRun1328();
        int tails = 0;
        vm.OnAddTail = (_, _, _) =>
        {
            tails++;
            return true;
        };

        var spawned = new VmThinkerRun1328.Thinker { ObjectFlags = VmThinkerRun1328.OfJustSpawned | 1 };
        vm.Link(spawned, 32);
        Assert.Equal(VmThinkerRun1328.OfJustSpawned | 1, spawned.ObjectFlags);
        Assert.Equal("fresh:32:" + (VmThinkerRun1328.OfJustSpawned | 1), vm.Steps[0]);

        var below = new VmThinkerRun1328.Thinker { ObjectFlags = VmThinkerRun1328.OfJustSpawned | 1 };
        vm.Link(below, 31);
        Assert.Equal(1, below.ObjectFlags);
        Assert.Equal("live:31:1", vm.Steps[1]);

        var travelling = new VmThinkerRun1328.Thinker { ObjectFlags = VmThinkerRun1328.OfJustSpawned | 1 };
        vm.Link(travelling, VmThinkerRun1328.StatTravelling);
        Assert.Equal(VmThinkerRun1328.OfJustSpawned | 1, travelling.ObjectFlags);

        var stat = new VmThinkerRun1328.Thinker { ObjectFlags = VmThinkerRun1328.OfJustSpawned };
        vm.Link(stat, VmThinkerRun1328.StatStatic);
        Assert.Equal(0, stat.ObjectFlags);
        Assert.Equal("live:5:0", vm.Steps[3]);

        var plain = new VmThinkerRun1328.Thinker { ObjectFlags = 1 };
        vm.Link(plain, 100);
        Assert.Equal(1, plain.ObjectFlags);
        Assert.Equal("live:100:1", vm.Steps[4]);

        var high = new VmThinkerRun1328.Thinker { ObjectFlags = VmThinkerRun1328.OfJustSpawned | 1 };
        vm.Link(high, 100);
        Assert.Equal(VmThinkerRun1328.OfJustSpawned | 1, high.ObjectFlags);
        Assert.Equal(6, tails);

        var level = new VmThinkerRun1328.Level();
        var lamp = new VmThinkerRun1328.DynLight();
        level.Lights = lamp;
        vm.ThinkCount = 9;
        vm.BotWtg = 4;
        int freshSeen = 0;
        vm.OnTick = (fresh, stat, dest) =>
        {
            if (!fresh)
            {
                Assert.False(dest);
                return 0;
            }
            Assert.True(dest);
            int pass = freshSeen / Span;
            freshSeen++;
            if (pass == 0 && stat == 40)
                return 1;
            return 0;
        };
        vm.RunThinkers(level);
        Assert.Equal(0, vm.ThinkCount);
        Assert.Equal(0, vm.BotWtg);
        Assert.Equal(0, lamp.Ticks);
        Assert.Equal(Span, vm.Ticks.Count(call => !call.Profile && !call.Fresh));
        Assert.Equal(Span * 2, freshSeen);
        Assert.Equal(Span + Span * 2, vm.Ticks.Count);
        Assert.Equal(new VmThinkerRun1328.TickCall(false, false, 32, false), vm.Ticks[0]);
        Assert.Equal(new VmThinkerRun1328.TickCall(false, false, 127, false), vm.Ticks[Span - 1]);
        Assert.Equal(new VmThinkerRun1328.TickCall(false, true, 32, true), vm.Ticks[Span]);
        Assert.Equal(new VmThinkerRun1328.TickCall(false, true, 32, true), vm.Ticks[Span * 2]);
        Assert.Equal(
            new[] { "think.reset", "bot.reset", "action.reset", "think.clock", "think.unclock" },
            vm.Cycles);
        Assert.Empty(vm.Prints);

        var destroy = new VmThinkerRun1328();
        destroy.OnDestroy = (fresh, stat) => fresh && stat == 10;
        destroy.DestroyAllThinkers();
        Assert.Equal(253, destroy.Steps.Count(step => step.Contains("destroy")));
        Assert.DoesNotContain(destroy.Steps, step => step.StartsWith("live-destroy:4:") || step.StartsWith("fresh-destroy:4:"));
        Assert.DoesNotContain(destroy.Steps, step => step.StartsWith("live-destroy:5:") || step.StartsWith("fresh-destroy:5:"));
        Assert.Contains("fresh-destroy:10:1", destroy.Steps);
        Assert.Contains("live-destroy:128:0", destroy.Steps);
        Assert.DoesNotContain(destroy.Steps, step => step.StartsWith("fresh-destroy:128:"));
        Assert.Equal("fullgc", destroy.Steps[^3]);
        Assert.Equal("clear", destroy.Steps[^2]);
        Assert.Equal("error", destroy.Steps[^1]);
        Assert.Equal(VmThinkerRun1328.DestroyFailed, destroy.ErrorText);
        Assert.Null(destroy.FatalText);
        Assert.Equal(1, destroy.FullCollections);
        Assert.Equal(1, destroy.StackClears);

        var fatal = new VmThinkerRun1328();
        fatal.OnDestroy = (fresh, stat) => fresh && stat == 10;
        fatal.DestroyAllThinkers(false);
        Assert.Equal("clear", fatal.Steps[^2]);
        Assert.Equal("fatal", fatal.Steps[^1]);
        Assert.DoesNotContain("fullgc", fatal.Steps);
        Assert.Equal(VmThinkerRun1328.DestroyFailed, fatal.FatalText);
        Assert.Null(fatal.ErrorText);
        Assert.Equal(0, fatal.FullCollections);

        var quiet = new VmThinkerRun1328();
        quiet.DestroyAllThinkers();
        Assert.Equal("fullgc", quiet.Steps[^1]);
        Assert.Equal(0, quiet.StackClears);
        Assert.Null(quiet.ErrorText);

        var idle = new VmThinkerRun1328();
        idle.DestroyAllThinkers(false);
        Assert.Equal(253, idle.Steps.Count);
        Assert.Equal(0, idle.FullCollections);
        Assert.Equal(0, idle.StackClears);

        var cleanup = new VmThinkerRun1328();
        int removes = 0;
        bool save = true;
        cleanup.OnRemoveTravellers = (_, _, flag) =>
        {
            removes++;
            save &= flag;
        };
        int lists = 0;
        cleanup.OnDestroyList = stat =>
        {
            lists++;
            Assert.Equal(VmThinkerRun1328.StatTravelling, stat);
        };
        cleanup.CleanUpTravellers(true);
        Assert.Equal(1, lists);
        Assert.Equal(256, removes);
        Assert.True(save);
        Assert.Equal("list:4", cleanup.Steps[0]);
        Assert.Equal("fresh-remove:0:1", cleanup.Steps[1]);
        Assert.Equal("live-remove:127:1", cleanup.Steps[^1]);
        Assert.DoesNotContain(cleanup.Steps, step => step.Contains("remove:128"));

        var load = new VmThinkerRun1328();
        int loads = 0;
        load.OnLoadList = (_, _) => loads++;
        load.OnLoad();
        Assert.Equal(VmThinkerRun1328.FreshListCount + VmThinkerRun1328.ThinkerListCount, loads);
        Assert.Equal("fresh:0", load.Steps[0]);
        Assert.Equal("fresh:127", load.Steps[127]);
        Assert.Equal("live:0", load.Steps[128]);
        Assert.Equal("live:128", load.Steps[^1]);
    }

    [Fact]
    public void Profile_SortsRowsAndRecreatesClientLights()
    {
        var vm = new VmThinkerRun1328();
        vm.Profiles["junk"] = new VmThinkerRun1328.ProfileInfo { Calls = 1, Time = 9 };
        vm.ProfileThinkers = 8;
        vm.ThinkCount = 9;
        vm.BotWtg = 3;
        vm.OnProfile = (fresh, stat, dest) =>
        {
            if (!fresh && stat == 32)
            {
                Assert.False(dest);
                vm.Profiles["Zebra"] = new VmThinkerRun1328.ProfileInfo { Calls = 2, Time = 4 };
                vm.Profiles["alpha"] = new VmThinkerRun1328.ProfileInfo { Calls = 5, Time = 5 };
                vm.Profiles["Mid"] = new VmThinkerRun1328.ProfileInfo { Calls = 3, Time = 9 };
            }
            return 0;
        };
        vm.OnTick = (_, _, _) => throw new InvalidOperationException("tick");
        vm.RunThinkers(new VmThinkerRun1328.Level());
        Assert.Equal(0, vm.ProfileThinkers);
        Assert.Equal(0, vm.ThinkCount);
        Assert.Equal(0, vm.BotWtg);
        Assert.False(vm.Profiles.ContainsKey("junk"));
        Assert.Equal(3, vm.Profiles.Count);
        Assert.Equal(Span * 2, vm.Ticks.Count);
        Assert.All(vm.Ticks, call => Assert.True(call.Profile));
        Assert.Equal(
            new[] { "think.reset", "bot.reset", "action.reset", "think.clock", "think.unclock" },
            vm.Cycles);
        Assert.Equal(VmThinkerRun1328.TextYellow + "Total, ms   Averg, ms   Calls   Actor class\n", vm.Prints[0]);
        Assert.Equal(VmThinkerRun1328.TextYellow + "----------  ----------  ------  --------------------\n", vm.Prints[1]);
        Assert.Equal(Row(8, 9, 3, "Mid"), vm.Prints[2]);
        Assert.Equal(Row(8, 5, 5, "alpha"), vm.Prints[3]);
        Assert.Equal(Row(8, 4, 2, "Zebra"), vm.Prints[4]);

        Assert.Equal(new[] { "alpha", "Mid", "Zebra" }, Names(1, 0));
        Assert.Equal(new[] { "Zebra", "Mid", "alpha" }, Names(2, 0));
        Assert.Equal(new[] { "Zebra", "Mid", "alpha" }, Names(3, 0));
        Assert.Equal(new[] { "alpha", "Mid", "Zebra" }, Names(4, 0));
        Assert.Equal(new[] { "alpha", "Zebra", "Mid" }, Names(5, 0));
        Assert.Equal(new[] { "Mid", "Zebra", "alpha" }, Names(6, 0));
        Assert.Equal(new[] { "Zebra", "alpha", "Mid" }, Names(7, 0));
        Assert.Equal(new[] { "Mid" }, Names(8, 1));
        Assert.Equal(Row(1, 5, 5, "alpha"), PrintOf(1, 0)[2]);
        Assert.Equal(Row(5, 5, 5, "alpha"), PrintOf(5, 0)[2]);
        Assert.Equal(Row(7, 4, 2, "Zebra"), PrintOf(7, 0)[2]);

        var client = new VmThinkerRun1328();
        client.ProfileThinkers = 5;
        client.ThinkCount = 9;
        var host = new VmThinkerRun1328.Level
        {
            Flags3 = VmThinkerRun1328.Level3LightCreated | 1,
            LocalWorldTimer = 4,
            LocalTimer = 8,
        };
        var server = new VmThinkerRun1328.Actor
        {
            Flags8 = VmThinkerRun1328.Mf8RecreateLights,
            Effects = 3,
        };
        var side = new VmThinkerRun1328.Actor
        {
            FountainColor = 4,
            RenderLocally = true,
        };
        host.Actors.Add(server);
        host.ClientActors.Add(side);
        var first = new VmThinkerRun1328.DynLight();
        var second = new VmThinkerRun1328.DynLight();
        var third = new VmThinkerRun1328.DynLight();
        first.Next = second;
        second.Next = third;
        first.OnTick = light => light.Next = null;
        host.Lights = first;
        bool sawPause = false;
        client.WorldPaused = lag =>
        {
            sawPause = lag == false;
            return 0;
        };
        client.GlLights = true;
        client.DynLights = false;
        client.RenderMode = 4;
        client.RunClientSideThinkers(host);
        Assert.True(sawPause);
        Assert.Equal(5, client.ProfileThinkers);
        Assert.Equal(9, client.ThinkCount);
        Assert.Empty(client.Cycles);
        Assert.Equal(5, host.LocalWorldTimer);
        Assert.Equal(9, host.LocalTimer);
        Assert.Equal(1, host.Flags3);
        Assert.Equal(1, host.FrozenChecks);
        Assert.Equal(Span * 2, client.Ticks.Count);
        Assert.All(client.Ticks, call => Assert.False(call.Profile));
        Assert.Equal(new[] { server, side }, client.Recreated);
        Assert.Equal(0, server.Flags8);
        Assert.Equal(1, server.DynamicLights);
        Assert.Equal(1, server.EffectCalls);
        Assert.Equal(3, server.LastEffect);
        Assert.Equal(1, side.EffectCalls);
        Assert.Equal(0, side.LastEffect);
        Assert.Equal(1, first.Ticks);
        Assert.Equal(1, second.Ticks);
        Assert.Equal(1, third.Ticks);

        var paused = new VmThinkerRun1328();
        paused.WorldPaused = _ => 7;
        paused.OnTick = (_, _, _) => throw new InvalidOperationException("paused");
        var held = new VmThinkerRun1328.Level { LocalWorldTimer = 4, LocalTimer = 8 };
        var actor = new VmThinkerRun1328.Actor { Flags8 = VmThinkerRun1328.Mf8RecreateLights, Effects = 1 };
        held.Actors.Add(actor);
        var glow = new VmThinkerRun1328.DynLight();
        held.Lights = glow;
        paused.RunClientSideThinkers(held);
        Assert.Equal(4, held.LocalWorldTimer);
        Assert.Equal(9, held.LocalTimer);
        Assert.Equal(0, actor.Flags8);
        Assert.Equal(1, actor.DynamicLights);
        Assert.Equal(1, actor.EffectCalls);
        Assert.Equal(0, glow.Ticks);
        Assert.Empty(paused.Ticks);

        Assert.False(Lights(gl: false, dyn: true, mode: 4));
        Assert.True(Lights(gl: false, dyn: true, mode: 1));
        Assert.False(Lights(gl: true, dyn: false, mode: 1));
        Assert.False(Lights(gl: false, dyn: false, mode: 4));
        Assert.True(Lights(gl: true, dyn: true, mode: 0));

        var cold = new VmThinkerRun1328();
        cold.WorldPaused = _ => 0;
        var frozen = new VmThinkerRun1328.Level { Frozen = true };
        var dark = new VmThinkerRun1328.Actor
        {
            Flags8 = VmThinkerRun1328.Mf8RecreateLights,
            Effects = 9,
            RenderLocally = false,
        };
        frozen.Actors.Add(dark);
        cold.GlLights = false;
        cold.DynLights = false;
        cold.RenderMode = 4;
        cold.RunClientSideThinkers(frozen);
        Assert.Equal(0, dark.Flags8);
        Assert.Equal(0, dark.DynamicLights);
        Assert.Equal(0, dark.EffectCalls);
        Assert.Equal(1, frozen.FrozenChecks);
    }

    static bool Lights(bool gl, bool dyn, int mode)
    {
        var vm = new VmThinkerRun1328
        {
            GlLights = gl,
            DynLights = dyn,
            RenderMode = mode,
        };
        vm.WorldPaused = _ => 0;
        var level = new VmThinkerRun1328.Level();
        var light = new VmThinkerRun1328.DynLight();
        level.Lights = light;
        vm.RunClientSideThinkers(level);
        return light.Ticks == 1;
    }

    static string[] Names(int mode, uint limit)
    {
        return PrintOf(mode, limit).Skip(2).Select(line => line.TrimEnd()).Select(line => line[(line.LastIndexOf('\u001c') + 2)..]).ToArray();
    }

    static List<string> PrintOf(int mode, uint limit)
    {
        var vm = new VmThinkerRun1328 { ProfileThinkers = mode, ProfileLimit = limit };
        vm.OnProfile = (fresh, stat, _) =>
        {
            if (!fresh && stat == 32)
            {
                vm.Profiles["Zebra"] = new VmThinkerRun1328.ProfileInfo { Calls = 2, Time = 4 };
                vm.Profiles["alpha"] = new VmThinkerRun1328.ProfileInfo { Calls = 5, Time = 5 };
                vm.Profiles["Mid"] = new VmThinkerRun1328.ProfileInfo { Calls = 3, Time = 9 };
            }
            return 0;
        };
        vm.RunThinkers(new VmThinkerRun1328.Level());
        Assert.Equal(0, vm.ProfileThinkers);
        return vm.Prints;
    }

    static string Row(int mode, double time, int calls, string name)
    {
        double average = time / calls;
        string timeTone = mode >= 7 ? VmThinkerRun1328.TextYellow : VmThinkerRun1328.TextWhite;
        string averageTone = mode == 5 || mode == 6 ? VmThinkerRun1328.TextYellow : VmThinkerRun1328.TextWhite;
        string callTone = mode == 3 || mode == 4 ? VmThinkerRun1328.TextYellow : VmThinkerRun1328.TextWhite;
        string nameTone = mode == 1 || mode == 2 ? VmThinkerRun1328.TextYellow : VmThinkerRun1328.TextWhite;
        return timeTone + time.ToString("0.000000").PadLeft(10) + "  "
            + averageTone + average.ToString("0.000000").PadLeft(10) + "  "
            + callTone + calls.ToString().PadLeft(6) + "  "
            + nameTone + name + "\n";
    }
}
