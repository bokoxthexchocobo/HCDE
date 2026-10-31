namespace HCDE.Playsim;

/// <summary>
/// Activator camera-restore tail and <c>LS_SetPlayerProperty</c> from
/// <c>p_lnspec.cpp</c> (2869-3129). Chunk 1353. Inventory lookup, class
/// lookup, and the past-viewer clear stay injected. The camera-actor arm
/// of <c>LS_ChangeCamera</c> stays in chunk 931.
/// </summary>
public static class VmPlayerProperty1353Slice
{
    public const int MaxPlayers = 64;

    public const int PropFrozen = 0;
    public const int PropNoTarget = 1;
    public const int PropInstantWeaponSwitch = 2;
    public const int PropFly = 3;
    public const int PropTotallyFrozen = 4;
    public const int PropInvulnerability = 5;
    public const int PropStrength = 6;
    public const int PropInvisibility = 7;
    public const int PropRadiationSuit = 8;
    public const int PropAllMap = 9;
    public const int PropInfrared = 10;
    public const int PropWeaponLevel2 = 11;
    public const int PropFlight = 12;
    public const int PropUnused1 = 13;
    public const int PropUnused2 = 14;
    public const int PropSpeed = 15;
    public const int PropBuddha = 16;
    public const int PropBuddha2 = 17;
    public const int PropFrightening = 18;
    public const int PropNoClip = 19;
    public const int PropNoClip2 = 20;
    public const int PropGodMode = 21;
    public const int PropGodMode2 = 22;

    public const int CfNoClip = 1 << 0;
    public const int CfGodMode = 1 << 1;
    public const int CfNoTarget = 1 << 3;
    public const int CfFly = 1 << 4;
    public const int CfFrozen = 1 << 6;
    public const int CfRevertPlease = 1 << 7;
    public const int CfFrightening = 1 << 10;
    public const int CfInstantWeapSwitch = 1 << 11;
    public const int CfTotallyFrozen = 1 << 12;
    public const int CfBuddha2 = 1 << 24;
    public const int CfGodMode2 = 1 << 25;
    public const int CfBuddha = 1 << 27;
    public const int CfNoClip2 = 1 << 30;

    public const int MfNoGravity = 0x00000200;
    public const int Mf2Fly = 0x00000010;
    public const int Mf7FlyCheat = 0x00020000;
    public const int Level2AllMap = 0x00000002;
    public const int BCompatLinkFrozen = 1 << 6;
    public const int InverseColormap = 0;
    public const uint SpecialColormapMask = 0x00b60000;

    public const string NameNone = "None";

    public static readonly string[] Powers =
    [
        "PowerInvulnerable",
        "PowerStrength",
        "PowerInvisibility",
        "PowerIronFeet",
        "None",
        "PowerLightAmp",
        "PowerWeaponLevel2",
        "PowerFlight",
        "None",
        "None",
        "PowerSpeed",
        "PowerInfiniteAmmo",
        "PowerDoubleFiringSpeed",
        "PowerBuddha",
    ];

    /// <summary>
    /// Null-camera activator tail. Assigns the activator as the camera,
    /// clears <c>CF_REVERTPLEASE</c>, and clears the past viewer only when
    /// the camera pointer changed. Returns true.
    /// </summary>
    public static bool RestoreActivatorCamera(
        Player1353? player,
        Actor1353? activator,
        Actor1353? oldCamera,
        Action<Actor1353?>? clearPastViewer)
    {
        if (player == null)
            throw new NullReferenceException("player");
        player.Camera = activator;
        player.Cheats &= ~CfRevertPlease;
        if (!ReferenceEquals(oldCamera, player.Camera))
        {
            if (clearPastViewer == null)
                throw new InvalidOperationException("R_ClearPastViewer");
            clearPastViewer(player.Camera);
        }
        return true;
    }

    public static bool SetPlayerProperty(Actor1353? it, int arg0, int arg1, int arg2, Level1353 level)
    {
        int mask = 0;
        if ((it == null || it.Player == null) && arg0 == 0)
            return false;

        if (arg2 >= PropInvulnerability && arg2 <= PropSpeed)
        {
            int power = arg2 - PropInvulnerability;
            if (power > 4 && Powers[power] == NameNone)
                return false;

            if (arg0 == 0)
                ApplyActivatorPower(it!, power, arg1, level);
            else
                ApplyEveryPlayerPower(power, arg1, level);
            return true;
        }

        switch (arg2)
        {
            case PropBuddha:
                mask = CfBuddha;
                break;
            case PropBuddha2:
                mask = CfBuddha2;
                break;
            case PropFrozen:
                mask = CfFrozen;
                break;
            case PropNoTarget:
                mask = CfNoTarget;
                break;
            case PropInstantWeaponSwitch:
                mask = CfInstantWeapSwitch;
                break;
            case PropFly:
                break;
            case PropTotallyFrozen:
                mask = CfTotallyFrozen;
                break;
            case PropFrightening:
                mask = CfFrightening;
                break;
            case PropNoClip:
                mask = CfNoClip;
                break;
            case PropNoClip2:
                mask = CfNoClip | CfNoClip2;
                break;
            case PropGodMode:
                mask = CfGodMode;
                break;
            case PropGodMode2:
                mask = CfGodMode2;
                break;
        }

        if (arg0 == 0)
            ApplyActivatorFlag(it!, arg1, arg2, mask);
        else
        {
            if ((level.IbCompatFlags & BCompatLinkFrozen) != 0
                && (mask & (CfFrozen | CfTotallyFrozen)) != 0)
                mask = CfFrozen | CfTotallyFrozen;
            ApplyEveryPlayerFlag(arg1, arg2, mask, level);
        }

        return mask != 0;
    }

    private static void ApplyActivatorPower(Actor1353 it, int power, int arg1, Level1353 level)
    {
        if (arg1 != 0)
        {
            if (power != 4)
            {
                Inventory1353? item = it.GiveInventoryType(level.ResolveActor(Powers[power]));
                if (item != null && power == 0 && arg1 == 1)
                    item.BlendColor = (uint)InverseColormap | SpecialColormapMask;
            }
            else if (it.Player != null && ReferenceEquals(it.Player, level.ConsolePlayer))
                level.Flags2 |= Level2AllMap;
        }
        else if (power != 4)
        {
            Inventory1353? item = it.FindInventory(Powers[power], true);
            if (item != null)
                item.Destroy();
        }
        else if (it.Player != null && ReferenceEquals(it.Player, level.ConsolePlayer))
            level.Flags2 &= ~Level2AllMap;
    }

    private static void ApplyEveryPlayerPower(int power, int arg1, Level1353 level)
    {
        for (int i = 0; i < MaxPlayers; i++)
        {
            Player1353? p = level.Players[i];
            if (!level.PlayerInGame(i))
                continue;
            if (p == null)
                throw new NullReferenceException("player");
            if (p.Mo == null)
                continue;

            if (arg1 != 0)
            {
                if (power != 4)
                {
                    Inventory1353? item = p.Mo.GiveInventoryType(level.ResolveActor(Powers[power]));
                    if (item != null && power == 0 && arg1 == 1)
                        item.BlendColor = (uint)InverseColormap | SpecialColormapMask;
                }
                else if (ReferenceEquals(p, level.ConsolePlayer))
                    level.Flags2 |= Level2AllMap;
            }
            else if (power != 4)
            {
                Inventory1353? item = p.Mo.FindInventory(level.ResolveActor(Powers[power]));
                if (item != null)
                    item.Destroy();
            }
            else if (ReferenceEquals(p, level.ConsolePlayer))
                level.Flags2 &= ~Level2AllMap;
        }
    }

    private static void ApplyActivatorFlag(Actor1353 it, int arg1, int arg2, int mask)
    {
        Player1353 player = it.Player ?? throw new NullReferenceException("player");
        if (arg1 != 0)
        {
            player.Cheats |= mask;
            if (arg2 == PropFly)
            {
                it.Flags7 |= Mf7FlyCheat;
                it.Flags2 |= Mf2Fly;
                it.Flags |= MfNoGravity;
            }
        }
        else
        {
            player.Cheats &= ~mask;
            if (arg2 == PropFly)
            {
                it.Flags7 &= ~Mf7FlyCheat;
                it.Flags2 &= ~Mf2Fly;
                it.Flags &= ~MfNoGravity;
            }
        }
    }

    private static void ApplyEveryPlayerFlag(int arg1, int arg2, int mask, Level1353 level)
    {
        for (int i = 0; i < MaxPlayers; i++)
        {
            if (!level.PlayerInGame(i))
                continue;
            Player1353 p = level.Players[i] ?? throw new NullReferenceException("player");
            if (arg1 != 0)
            {
                p.Cheats |= mask;
                if (arg2 == PropFly)
                {
                    Actor1353 mo = p.Mo ?? throw new NullReferenceException("mo");
                    mo.Flags2 |= Mf2Fly;
                    mo.Flags |= MfNoGravity;
                }
            }
            else
            {
                p.Cheats &= ~mask;
                if (arg2 == PropFly)
                {
                    Actor1353 mo = p.Mo ?? throw new NullReferenceException("mo");
                    mo.Flags2 &= ~Mf2Fly;
                    mo.Flags &= ~MfNoGravity;
                }
            }
        }
    }
}

public sealed class ActorClass1353
{
    public ActorClass1353(string name) => Name = name;
    public string Name { get; }
}

public sealed class Inventory1353
{
    public uint BlendColor;
    public bool Destroyed;
    public void Destroy() => Destroyed = true;
}

public sealed class Actor1353
{
    public Player1353? Player;
    public int Flags;
    public int Flags2;
    public int Flags7;
    public Func<ActorClass1353?, Inventory1353?>? Give;
    public Func<string, bool, Inventory1353?>? FindByName;
    public Func<ActorClass1353?, Inventory1353?>? FindByClass;

    public Inventory1353? GiveInventoryType(ActorClass1353? type)
    {
        if (Give == null)
            throw new InvalidOperationException("GiveInventoryType");
        return Give(type);
    }

    public Inventory1353? FindInventory(string name, bool subclass)
    {
        if (FindByName == null)
            throw new InvalidOperationException("FindInventory");
        return FindByName(name, subclass);
    }

    public Inventory1353? FindInventory(ActorClass1353? type)
    {
        if (FindByClass == null)
            throw new InvalidOperationException("FindInventory");
        return FindByClass(type);
    }
}

public sealed class Player1353
{
    public Actor1353? Mo;
    public Actor1353? Camera;
    public int Cheats;
}

public sealed class Level1353
{
    public Player1353?[] Players = new Player1353?[VmPlayerProperty1353Slice.MaxPlayers];
    public bool[] InGame = new bool[VmPlayerProperty1353Slice.MaxPlayers];
    public Player1353? ConsolePlayer;
    public int Flags2;
    public int IbCompatFlags;
    public Func<string, ActorClass1353?>? FindActor;

    public bool PlayerInGame(int index) => InGame[index];

    public ActorClass1353? ResolveActor(string name)
    {
        if (FindActor == null)
            throw new InvalidOperationException("FindActor");
        return FindActor(name);
    }
}
