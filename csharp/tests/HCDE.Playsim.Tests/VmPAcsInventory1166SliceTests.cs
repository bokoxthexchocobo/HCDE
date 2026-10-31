namespace HCDE.Playsim.Tests;

public class VmPAcsInventory1166SliceTests
{
    [Fact]
    public void UseInv_ClearsTotallyFrozenOnlyWhileTheItemIsUsed()
    {
        var stim = new VmPAcsInventory1166Slice.ActorClass { Name = "Stimpack", Inventory = true };
        var actor = new VmPAcsInventory1166Slice.ActorState();
        Assert.False(VmPAcsInventory1166Slice.DoUseInv(actor, stim));
        Assert.False(actor.Used);

        actor.Items.Add(new VmPAcsInventory1166Slice.Item { ClassName = "Stimpack", Amount = 1 });
        var seen = 0;
        actor.OnUse = _ =>
        {
            seen = -1;
            return false;
        };
        Assert.False(VmPAcsInventory1166Slice.DoUseInv(actor, stim));
        Assert.Equal(-1, seen);

        var player = new VmPAcsInventory1166Slice.PlayerState
        {
            Cheats = VmPAcsInventory1166Slice.CfTotallyFrozen | 2,
            Mo = actor,
        };
        actor.Player = player;
        actor.OnUse = _ =>
        {
            seen = player.Cheats;
            player.Cheats = 1;
            return true;
        };
        Assert.True(VmPAcsInventory1166Slice.DoUseInv(actor, stim));
        Assert.Equal(2, seen);
        Assert.Equal(1 | VmPAcsInventory1166Slice.CfTotallyFrozen, player.Cheats);

        var lookups = 0;
        Assert.Equal(0, VmPAcsInventory1166Slice.UseInventory(Array.Empty<VmPAcsInventory1166Slice.PlayerState?>(), actor, null, _ =>
        {
            lookups++;
            return stim;
        }));
        Assert.Equal(0, lookups);
        Assert.Equal(0, VmPAcsInventory1166Slice.UseInventory(Array.Empty<VmPAcsInventory1166Slice.PlayerState?>(), actor, "Nope", _ => null));

        var second = new VmPAcsInventory1166Slice.ActorState
        {
            Items = { new VmPAcsInventory1166Slice.Item { ClassName = "Stimpack", Amount = 1 } },
        };
        var idle = new VmPAcsInventory1166Slice.ActorState();
        var players = new VmPAcsInventory1166Slice.PlayerState?[VmPAcsInventory1166Slice.MaxPlayers];
        players[0] = new VmPAcsInventory1166Slice.PlayerState { Mo = actor };
        players[2] = new VmPAcsInventory1166Slice.PlayerState { Mo = second };
        players[3] = new VmPAcsInventory1166Slice.PlayerState { Mo = idle };
        actor.Player = null;
        Assert.Equal(2, VmPAcsInventory1166Slice.UseInventory(players, null, "Stimpack", _ => stim));
        Assert.Equal(1, VmPAcsInventory1166Slice.UseInventory(players, actor, "Stimpack", _ => stim));
    }

    [Fact]
    public void CheckInventory_RenamesArmorAndHealthBeforeClassLookup()
    {
        var actor = new VmPAcsInventory1166Slice.ActorState { Health = 40, MaxHealth = 100 };
        var messages = new List<string>();
        Assert.Equal(0, VmPAcsInventory1166Slice.CheckInventory(null, "Health", false, _ => null, messages));
        Assert.Equal(0, VmPAcsInventory1166Slice.CheckInventory(actor, null, true, _ => null, messages));
        Assert.Equal(100, VmPAcsInventory1166Slice.CheckInventory(actor, "HeAlTh", true, _ => throw new InvalidOperationException(), messages));
        Assert.Equal(40, VmPAcsInventory1166Slice.CheckInventory(actor, "health", false, _ => throw new InvalidOperationException(), messages));

        string? lookedUp = null;
        var armor = new VmPAcsInventory1166Slice.ActorClass
        {
            Name = "BasicArmor",
            Inventory = true,
            DefaultMaxAmount = 200,
        };
        Assert.Equal(200, VmPAcsInventory1166Slice.CheckInventory(actor, "aRmOr", true, name =>
        {
            lookedUp = name;
            return armor;
        }, messages));
        Assert.Equal("BasicArmor", lookedUp);

        actor.Items.Add(new VmPAcsInventory1166Slice.Item { ClassName = "BasicArmor", Amount = 15, MaxAmount = 80 });
        Assert.Equal(80, VmPAcsInventory1166Slice.CheckInventory(actor, "Armor", true, _ => armor, messages));
        Assert.Equal(15, VmPAcsInventory1166Slice.CheckInventory(actor, "Armor", false, _ => armor, messages));

        Assert.Equal(0, VmPAcsInventory1166Slice.CheckInventory(actor, "Ghost", false, _ => null, messages));
        Assert.Contains("ACS: 'Ghost': Unknown actor class.", messages);
        var decoration = new VmPAcsInventory1166Slice.ActorClass { Name = "Tree", Inventory = false };
        Assert.Equal(0, VmPAcsInventory1166Slice.CheckInventory(actor, "Tree", true, _ => decoration, messages));
        Assert.Contains("ACS: 'Tree' is not an inventory item.", messages);
    }

    [Fact]
    public void Players_KeepNegativeNumbersAndRejectReservedSlots()
    {
        var hostCalls = 0;
        Assert.Equal(-4, VmPAcsInventory1166Slice.ToInternalPlayer(-4, _ =>
        {
            hostCalls++;
            return 0;
        }));
        Assert.Equal(-2, VmPAcsInventory1166Slice.ToAcsPlayer(-2, _ =>
        {
            hostCalls++;
            return 9;
        }));
        Assert.Equal(0, hostCalls);
        Assert.Equal(7, VmPAcsInventory1166Slice.ToInternalPlayer(3, slot => slot + 4));
        Assert.Equal(1, VmPAcsInventory1166Slice.ToAcsPlayer(8, slot => slot - 7));

        Assert.False(VmPAcsInventory1166Slice.ResolvePlayer(-1, slot => slot, _ => true, _ => true, out var negative));
        Assert.Equal(-1, negative);

        var reservedChecks = 0;
        Assert.False(VmPAcsInventory1166Slice.ResolvePlayer(3, slot => slot + 70, slot =>
        {
            reservedChecks++;
            return false;
        }, _ => true, out var tooHigh));
        Assert.Equal(73, tooHigh);
        Assert.Equal(0, reservedChecks);

        Assert.False(VmPAcsInventory1166Slice.ResolvePlayer(1, slot => slot, slot => slot == 1, _ => true, out var reserved));
        Assert.Equal(1, reserved);
        Assert.True(VmPAcsInventory1166Slice.ResolvePlayer(4, slot => slot, _ => false, slot => slot == 4, out var ready));
        Assert.Equal(4, ready);
    }

    [Fact]
    public void WatcherAndModules_CrossThePlaneAndReuseALoadedLump()
    {
        var sector = new VmPAcsInventory1166Slice.WatchSector();
        sector.Floor.D = 10;
        sector.Floor.NormalZ = 1;
        sector.Ceiling.D = 5;
        sector.Ceiling.NormalZ = 2;
        var watcher = new VmPAcsInventory1166Slice.Watcher();
        var line = new object();
        VmPAcsInventory1166Slice.ConstructWatcher(watcher, "act", line, -3, true, sector, 1, 80, 1, 2, 3, 4, 5);
        Assert.True(watcher.LineSide);
        Assert.Equal(5, watcher.LastD);
        Assert.Equal(3, watcher.WatchD);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, watcher.Args);

        sector.Ceiling.D = 4;
        VmPAcsInventory1166Slice.TickWatcher(watcher);
        Assert.False(watcher.Executed);
        Assert.Equal(5, watcher.LastD);

        sector.Ceiling.D = 3;
        VmPAcsInventory1166Slice.TickWatcher(watcher);
        Assert.True(watcher.Executed);
        Assert.True(watcher.Destroyed);
        Assert.Equal(5, watcher.LastD);

        var flat = new VmPAcsInventory1166Slice.Watcher();
        VmPAcsInventory1166Slice.ConstructWatcher(flat, null, null, 0, false, sector, 0, 1, 0, 0, 0, 0, 0);
        Assert.False(flat.LineSide);
        sector.Floor.D = 0;
        VmPAcsInventory1166Slice.TickWatcher(flat);
        Assert.False(flat.Executed);
        flat.Sector = null;
        VmPAcsInventory1166Slice.TickWatcher(flat);
        Assert.True(flat.Destroyed);

        var arc = new Dictionary<string, object?>();
        VmPAcsInventory1166Slice.SerializeWatcher(watcher, arc);
        Assert.Equal(80, arc["special"]);
        Assert.Equal(3d, arc["watchd"]);
        Assert.Same(line, arc["line"]);

        var container = new VmPAcsInventory1166Slice.Container();
        var messages = new List<string>();
        Assert.Null(VmPAcsInventory1166Slice.LoadModule(container, -1, false, 0, -1, (_, _, _) => true, messages, "none"));
        Assert.Empty(messages);

        var inits = 0;
        VmPAcsInventory1166Slice.LoadDefaultModules(
            container,
            new[]
            {
                new[] { "MISSING", "LIBA" },
                new[] { "LIBA" },
            },
            name => name == "LIBA" ? 12 : -1,
            (module, length, realLump) =>
            {
                inits++;
                Assert.Equal(0, length);
                Assert.Equal(-1, realLump);
                module.LumpNum = 12;
                container.Modules.Add(module);
                return true;
            },
            messages,
            lump => "lump" + lump);
        Assert.Equal(1, inits);
        Assert.Contains("Could not find autoloaded ACS library MISSING", messages);
        Assert.Single(container.Modules);

        var failed = VmPAcsInventory1166Slice.LoadModule(container, 4, true, 8, 9, (module, length, realLump) =>
        {
            Assert.Equal(8, length);
            Assert.Equal(9, realLump);
            container.Modules.Add(module);
            return false;
        }, messages, "bad");
        Assert.Null(failed);
        Assert.Contains("bad: invalid ACS module", messages);
        Assert.Single(container.Modules);

        container.Modules[0].Good = false;
        Assert.False(VmPAcsInventory1166Slice.CheckAllGood(container));
        container.Modules[0].Good = true;
        container.Modules.Add(new VmPAcsInventory1166Slice.Module { LumpNum = 4, Good = true });
        Assert.True(VmPAcsInventory1166Slice.CheckAllGood(container));
        Assert.Null(VmPAcsInventory1166Slice.GetModule(container, -1));
        Assert.Equal(12, VmPAcsInventory1166Slice.GetModule(container, 0)!.LumpNum);
        Assert.Null(VmPAcsInventory1166Slice.GetModule(container, 2));

        var first = container.Modules[0];
        var second = container.Modules[1];
        VmPAcsInventory1166Slice.UnloadModules(container);
        Assert.True(second.Destroyed);
        Assert.True(first.Destroyed);
        Assert.Empty(container.Modules);

        var map = new VmPAcsInventory1166Slice.Module();
        var localA = new VmPAcsInventory1166Slice.Script();
        var localB = new VmPAcsInventory1166Slice.Script();
        localA.Next = localB;
        var client = new VmPAcsInventory1166Slice.Script();
        container.Modules.Add(map);
        container.AcsThinker = new VmPAcsInventory1166Slice.Thinker { Scripts = localA };
        container.ClientThinker = new VmPAcsInventory1166Slice.Thinker { Scripts = client };
        var marked = new List<object>();
        VmPAcsInventory1166Slice.MarkLevelVarStrings(container, marked.Add, marked.Add);
        Assert.Equal(new object[] { map, localA, localB, client }, marked);

        var locked = new List<(object Target, int Level)>();
        VmPAcsInventory1166Slice.LockLevelVarStrings(
            container,
            6,
            (module, level) => locked.Add((module, level)),
            (script, level) => locked.Add((script, level)));
        Assert.Equal(4, locked.Count);
        Assert.All(locked, pair => Assert.Equal(6, pair.Level));
        Assert.Same(map, locked[0].Target);
        Assert.Same(client, locked[3].Target);
    }
}
