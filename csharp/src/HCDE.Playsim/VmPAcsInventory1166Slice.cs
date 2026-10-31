namespace HCDE.Playsim;

/// <summary>
/// Inventory use, plane watchers, and ACS module loading from <c>p_acs.cpp</c>
/// (<b>1711–2101</b>). Chunk <b>1166</b>.
/// </summary>
public static class VmPAcsInventory1166Slice
{
    public const int CfTotallyFrozen = 1 << 12;
    public const int MaxPlayers = 64;

    public sealed class Item
    {
        public string ClassName = "";
        public int Amount;
        public int MaxAmount;
    }

    public sealed class ActorClass
    {
        public string Name = "";
        public bool Inventory;
        public int DefaultMaxAmount;
    }

    public sealed class PlayerState
    {
        public int Cheats;
        public ActorState Mo = null!;
    }

    public sealed class ActorState
    {
        public List<Item> Items { get; } = new();
        public PlayerState? Player;
        public int Health;
        public int MaxHealth;
        public bool Used;
        public Func<Item, bool>? OnUse;

        public bool Use(Item item)
        {
            Used = true;
            if (OnUse != null)
            {
                return OnUse(item);
            }

            return true;
        }
    }

    public sealed class Plane
    {
        public double D;
        public double NormalZ;
    }

    public sealed class WatchSector
    {
        public Plane Floor { get; } = new();
        public Plane Ceiling { get; } = new();
    }

    public sealed class Watcher
    {
        public int Special;
        public object? Activator;
        public object? Line;
        public bool LineSide;
        public bool Ceiling;
        public int[] Args { get; } = new int[5];
        public WatchSector? Sector;
        public double LastD;
        public double WatchD;
        public bool Destroyed;
        public bool Executed;
    }

    public sealed class Module
    {
        public int LumpNum;
        public bool Good = true;
        public bool Destroyed;
    }

    public sealed class Script
    {
        public Script? Next;
    }

    public sealed class Thinker
    {
        public Script? Scripts;
    }

    public sealed class Container
    {
        public List<Module> Modules { get; } = new();
        public Thinker? AcsThinker;
        public Thinker? ClientThinker;
    }

    public static bool DoUseInv(ActorState actor, ActorClass info)
    {
        Item? item = null;
        for (var i = 0; i < actor.Items.Count; i++)
        {
            if (string.Equals(actor.Items[i].ClassName, info.Name, StringComparison.Ordinal))
            {
                item = actor.Items[i];
                break;
            }
        }

        if (item == null)
        {
            return false;
        }

        if (actor.Player == null)
        {
            return actor.Use(item);
        }

        var cheats = actor.Player.Cheats;
        actor.Player.Cheats &= ~CfTotallyFrozen;
        var used = actor.Use(item);
        actor.Player.Cheats |= cheats & CfTotallyFrozen;
        return used;
    }

    public static int UseInventory(
        PlayerState?[] players,
        ActorState? activator,
        string? type,
        Func<string, ActorClass?> findActor)
    {
        if (type == null)
        {
            return 0;
        }

        var info = findActor(type);
        if (info == null)
        {
            return 0;
        }

        if (activator == null)
        {
            var ret = 0;
            for (var i = 0; i < MaxPlayers; i++)
            {
                if (i < players.Length && players[i] != null)
                {
                    if (DoUseInv(players[i]!.Mo, info))
                    {
                        ret += 1;
                    }
                }
            }

            return ret;
        }

        if (DoUseInv(activator, info))
        {
            return 1;
        }

        return 0;
    }

    public static int ToInternalPlayer(int playernum, Func<int, int> toInternalSlot)
    {
        if (playernum < 0)
        {
            return playernum;
        }

        return toInternalSlot(playernum);
    }

    public static int ToAcsPlayer(int playernum, Func<int, int> toVisibleSlot)
    {
        if (playernum < 0)
        {
            return playernum;
        }

        return toVisibleSlot(playernum);
    }

    public static bool ResolvePlayer(
        int playernum,
        Func<int, int> toInternalSlot,
        Func<int, bool> reservedSlot,
        Func<int, bool> playerInGame,
        out int internalPlayer)
    {
        var resolved = ToInternalPlayer(playernum, toInternalSlot);
        internalPlayer = resolved;
        if (resolved < 0)
        {
            return false;
        }

        if (resolved >= MaxPlayers)
        {
            return false;
        }

        if (reservedSlot(resolved))
        {
            return false;
        }

        return playerInGame(resolved);
    }

    public static int CheckInventory(
        ActorState? activator,
        string? type,
        bool max,
        Func<string, ActorClass?> findActor,
        List<string> messages)
    {
        if (activator == null || type == null)
        {
            return 0;
        }

        if (string.Equals(type, "Armor", StringComparison.OrdinalIgnoreCase))
        {
            type = "BasicArmor";
        }
        else if (string.Equals(type, "Health", StringComparison.OrdinalIgnoreCase))
        {
            if (max)
            {
                return activator.MaxHealth;
            }

            return activator.Health;
        }

        var info = findActor(type);
        if (info == null)
        {
            messages.Add("ACS: '" + type + "': Unknown actor class.");
            return 0;
        }

        if (!info.Inventory)
        {
            messages.Add("ACS: '" + type + "' is not an inventory item.");
            return 0;
        }

        Item? item = null;
        for (var i = 0; i < activator.Items.Count; i++)
        {
            if (string.Equals(activator.Items[i].ClassName, info.Name, StringComparison.Ordinal))
            {
                item = activator.Items[i];
                break;
            }
        }

        if (max)
        {
            if (item != null)
            {
                return item.MaxAmount;
            }

            if (info.Inventory)
            {
                return info.DefaultMaxAmount;
            }
        }

        if (item != null)
        {
            return item.Amount;
        }

        return 0;
    }

    public static void ConstructWatcher(
        Watcher watcher,
        object? activator,
        object? line,
        int lineSide,
        bool ceiling,
        WatchSector sector,
        int height,
        int special,
        int arg0,
        int arg1,
        int arg2,
        int arg3,
        int arg4)
    {
        watcher.Special = special;
        watcher.Activator = activator;
        watcher.Line = line;
        watcher.LineSide = lineSide != 0;
        watcher.Ceiling = ceiling;
        watcher.Args[0] = arg0;
        watcher.Args[1] = arg1;
        watcher.Args[2] = arg2;
        watcher.Args[3] = arg3;
        watcher.Args[4] = arg4;
        watcher.Sector = sector;
        var plane = ceiling ? sector.Ceiling : sector.Floor;
        watcher.LastD = plane.D;
        watcher.WatchD = plane.D - (height * plane.NormalZ);
    }

    public static void SerializeWatcher(Watcher watcher, Dictionary<string, object?> arc)
    {
        arc["special"] = watcher.Special;
        arc["sector"] = watcher.Sector;
        arc["ceiling"] = watcher.Ceiling;
        arc["watchd"] = watcher.WatchD;
        arc["lastd"] = watcher.LastD;
        arc["activator"] = watcher.Activator;
        arc["line"] = watcher.Line;
        arc["lineside"] = watcher.LineSide;
        arc["args"] = watcher.Args;
    }

    public static void TickWatcher(Watcher watcher)
    {
        if (watcher.Sector == null)
        {
            watcher.Destroyed = true;
            return;
        }

        var plane = watcher.Ceiling ? watcher.Sector.Ceiling : watcher.Sector.Floor;
        var newD = plane.D;
        if ((watcher.LastD < watcher.WatchD && newD >= watcher.WatchD)
            || (watcher.LastD > watcher.WatchD && newD <= watcher.WatchD))
        {
            watcher.Executed = true;
            watcher.Destroyed = true;
        }
    }

    public static void LoadDefaultModules(
        Container container,
        IReadOnlyList<IReadOnlyList<string>> lumps,
        Func<string, int> checkNumForName,
        Func<Module, int, int, bool> init,
        List<string> messages,
        Func<int, string> lumpName)
    {
        for (var lump = 0; lump < lumps.Count; lump++)
        {
            var names = lumps[lump];
            for (var i = 0; i < names.Count; i++)
            {
                var library = names[i];
                var acsLump = checkNumForName(library);
                if (acsLump >= 0)
                {
                    LoadModule(container, acsLump, false, 0, -1, init, messages, lumpName(acsLump));
                }
                else
                {
                    messages.Add("Could not find autoloaded ACS library " + library);
                }
            }
        }
    }

    public static Module? LoadModule(
        Container container,
        int lumpnum,
        bool readerPresent,
        int length,
        int realLump,
        Func<Module, int, int, bool> init,
        List<string> messages,
        string lumpName)
    {
        if (lumpnum == -1 && !readerPresent)
        {
            return null;
        }

        for (var i = 0; i < container.Modules.Count; i++)
        {
            if (container.Modules[i].LumpNum == lumpnum)
            {
                return container.Modules[i];
            }
        }

        var behavior = new Module { LumpNum = lumpnum };
        if (init(behavior, length, realLump))
        {
            return behavior;
        }

        container.Modules.Remove(behavior);
        behavior.Destroyed = true;
        messages.Add(lumpName + ": invalid ACS module");
        return null;
    }

    public static bool CheckAllGood(Container container)
    {
        for (var i = 0; i < container.Modules.Count; i++)
        {
            if (!container.Modules[i].Good)
            {
                return false;
            }
        }

        return true;
    }

    public static void UnloadModules(Container container)
    {
        for (var i = container.Modules.Count; i-- > 0;)
        {
            container.Modules[i].Destroyed = true;
        }

        container.Modules.Clear();
    }

    public static Module? GetModule(Container container, int lib)
    {
        if ((uint)lib >= (uint)container.Modules.Count)
        {
            return null;
        }

        return container.Modules[lib];
    }

    public static void MarkLevelVarStrings(
        Container container,
        Action<Module> markMap,
        Action<Script> markLocal)
    {
        for (var modnum = 0; modnum < container.Modules.Count; modnum++)
        {
            markMap(container.Modules[modnum]);
        }

        WalkScripts(container.AcsThinker, markLocal);
        WalkScripts(container.ClientThinker, markLocal);
    }

    public static void LockLevelVarStrings(
        Container container,
        int levelnum,
        Action<Module, int> lockMap,
        Action<Script, int> lockLocal)
    {
        for (var modnum = 0; modnum < container.Modules.Count; modnum++)
        {
            lockMap(container.Modules[modnum], levelnum);
        }

        WalkScripts(container.AcsThinker, script => lockLocal(script, levelnum));
        WalkScripts(container.ClientThinker, script => lockLocal(script, levelnum));
    }

    private static void WalkScripts(Thinker? thinker, Action<Script> visit)
    {
        if (thinker == null)
        {
            return;
        }

        for (var script = thinker.Scripts; script != null; script = script.Next)
        {
            visit(script);
        }
    }
}
