namespace HCDE.Playsim;

/// <summary>Legacy inventory <c>FParser::SF_*</c> from <c>fragglescript/t_func.cpp</c> (2394–2684).</summary>
public static partial class VmFScriptParser
{
    static readonly string[] FraggleDoomKeys =
    [
        "BlueCard", "YellowCard", "RedCard", "BlueSkull", "YellowSkull", "RedSkull",
    ];

    static readonly string[] FraggleWeaponNames =
    [
        "Fist", "Pistol", "Shotgun", "Chaingun", "RocketLauncher",
        "PlasmaRifle", "BFG9000", "Chainsaw", "SuperShotgun",
    ];

    static PlayerPawn? PlayerFromArg(ParserContext ctx, ScriptValue arg)
    {
        var num = GetPlayerNum(ctx, arg);
        if (num < 0)
            return null;
        return ctx.PlayerMo(num) as PlayerPawn;
    }

    public static void SfPlayerKeys(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var player = PlayerFromArg(ctx, ctx.TArgv[0]);
        if (player == null)
            return;
        var keynum = ctx.IntValue(ctx.TArgv[1]);
        if (keynum < 0 || keynum >= FraggleDoomKeys.Length)
        {
            ctx.ScriptError($"key number out of range: {keynum}\n");
            return;
        }
        var keyname = FraggleDoomKeys[keynum];
        if (ctx.TArgc == 2)
        {
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = AcsPlayerInventory.Count(player, keyname, max: false);
            return;
        }
        var givetake = ctx.IntValue(ctx.TArgv[2]);
        if (givetake != 0)
            AcsPlayerInventory.Give(player, keyname, 1);
        else
            AcsPlayerInventory.Take(player, keyname, 1);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
    }

    public static void SfPlayerAmmo(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var player = GetPlayerActor(ctx, ctx.TArgv[0]) as PlayerPawn;
        if (player == null)
        {
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = 0;
            return;
        }
        var typeName = AmmoNameFromArg(ctx, ctx.TArgv[1]);
        if (typeName == null)
        {
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = 0;
            return;
        }
        if (ctx.TArgc >= 3)
        {
            var target = ctx.IntValue(ctx.TArgv[2]);
            var current = AcsPlayerInventory.Count(player, typeName, max: false);
            if (target > current)
                AcsPlayerInventory.Give(player, typeName, target - current);
            else if (target < current)
                AcsPlayerInventory.Take(player, typeName, current - target);
        }
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = AcsPlayerInventory.Count(player, typeName, max: false);
    }

    public static void SfMaxPlayerAmmo(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var player = GetPlayerActor(ctx, ctx.TArgv[0]) as PlayerPawn;
        var typeName = AmmoNameFromArg(ctx, ctx.TArgv[1]);
        if (player == null || typeName == null)
        {
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = 0;
            return;
        }
        if (ctx.TArgc >= 4)
            AcsPlayerInventory.SetAmmoCapacity(player, typeName, ctx.IntValue(ctx.TArgv[3]));
        else if (ctx.TArgc >= 3)
            AcsPlayerInventory.SetAmmoCapacity(player, typeName, ctx.IntValue(ctx.TArgv[2]));
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = AcsPlayerInventory.AmmoCapacity(player, typeName);
    }

    public static void SfPlayerWeapon(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var player = PlayerFromArg(ctx, ctx.TArgv[0]);
        if (player == null)
            return;
        var weaponnum = ctx.IntValue(ctx.TArgv[1]);
        if (weaponnum < 0 || weaponnum > 8)
        {
            ctx.ScriptError($"weaponnum out of range! {weaponnum}\n");
            return;
        }
        var weaponName = FraggleWeaponNames[weaponnum];
        if (!AcsPlayerInventory.TryWeaponKind(weaponName, out var weaponKind))
        {
            ctx.ScriptError($"incompatibility in playerweapon {weaponnum}\n");
            return;
        }
        if (ctx.TArgc == 2)
        {
            ctx.TReturn.Type = SvtInt;
            ctx.TReturn.IntValue = player.Inventory.Owns(weaponKind) ? 1 : 0;
            return;
        }
        var newweapon = ctx.IntValue(ctx.TArgv[2]) != 0;
        if (!newweapon)
        {
            if (player.Inventory.Owns(weaponKind))
            {
                AcsPlayerInventory.Take(player, weaponName, 1);
                if (player.Inventory.Selected == weaponKind)
                {
                    player.Inventory.Pending = null;
                    if (!player.Inventory.Owns(WeaponKind.Pistol))
                        player.Inventory.Selected = WeaponKind.Fist;
                }
            }
        }
        else if (!player.Inventory.Owns(weaponKind))
        {
            var pending = player.Inventory.Pending;
            AcsPlayerInventory.Give(player, weaponName, 1);
            player.Inventory.Pending = pending;
        }
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = newweapon ? 1 : 0;
    }

    public static void SfPlayerSelectedWeapon(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1))
            return;
        var player = PlayerFromArg(ctx, ctx.TArgv[0]);
        if (player == null)
            return;
        if (ctx.TArgc == 2)
        {
            var weaponnum = ctx.IntValue(ctx.TArgv[1]);
            if (weaponnum < 0 || weaponnum >= 9)
            {
                ctx.ScriptError($"weaponnum out of range! {weaponnum}\n");
                return;
            }
            var weaponName = FraggleWeaponNames[weaponnum];
            if (!AcsPlayerInventory.TryWeaponKind(weaponName, out var weaponKind))
            {
                ctx.ScriptError($"incompatibility in playerweapon {weaponnum}\n");
                return;
            }
            if (player.Inventory.Owns(weaponKind))
                player.Inventory.Pending = weaponKind;
        }
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
        for (var i = 0; i < FraggleWeaponNames.Length; i++)
        {
            if (AcsPlayerInventory.TryWeaponKind(FraggleWeaponNames[i], out var kind)
                && player.Inventory.Selected == kind)
            {
                ctx.TReturn.IntValue = i;
                break;
            }
        }
    }

    public static void SfGiveInventory(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var player = PlayerFromArg(ctx, ctx.TArgv[0]);
        if (player == null)
            return;
        var count = ctx.TArgc == 2 ? 1 : ctx.IntValue(ctx.TArgv[2]);
        AcsPlayerInventory.Give(player, ctx.StringValue(ctx.TArgv[1]), count);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
    }

    public static void SfTakeInventory(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var player = PlayerFromArg(ctx, ctx.TArgv[0]);
        if (player == null)
            return;
        var count = ctx.TArgc == 2 ? 32767 : ctx.IntValue(ctx.TArgv[2]);
        AcsPlayerInventory.Take(player, ctx.StringValue(ctx.TArgv[1]), count);
        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = 0;
    }

    public static void SfCheckInventory(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var player = PlayerFromArg(ctx, ctx.TArgv[0]);
        ctx.TReturn.Type = SvtInt;
        if (player == null)
        {
            ctx.TReturn.IntValue = 0;
            return;
        }
        ctx.TReturn.IntValue = AcsPlayerInventory.Count(player, ctx.StringValue(ctx.TArgv[1]), max: false);
    }

    public static void SfSetWeapon(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        ctx.TReturn.Type = SvtInt;
        var player = GetPlayerActor(ctx, ctx.TArgv[0]);
        if (ctx.TArgv[1].Type != SvtString)
        {
            ctx.TReturn.IntValue = 0;
            return;
        }
        ctx.TReturn.IntValue = AcsPlayerInventory.SetWeapon(player as Actor, ctx.StringValue(ctx.TArgv[1]));
    }

    static string? AmmoNameFromArg(ParserContext ctx, ScriptValue arg)
    {
        if (arg.Type == SvtString)
            return ctx.StringValue(arg);
        if (arg.Type == SvtInt)
        {
            return ctx.IntValue(arg) switch
            {
                0 => "Clip",
                1 => "Shell",
                2 => "Cell",
                _ => "RocketAmmo",
            };
        }
        var ammoClass = GetAmmo(ctx, arg);
        if (ammoClass == null)
            return null;
        if (ammoClass is string name)
            return name;
        return null;
    }
}
