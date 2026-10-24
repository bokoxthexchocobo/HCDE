using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VmFScriptParserSfInventoryTests
{
    static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 1 }],
    });

    static VmFScriptParser.ParserContext Ctx(AuthoritySimulation sim, int argc, Action<VmFScriptParser.ScriptValue[]> fill)
    {
        var argv = new VmFScriptParser.ScriptValue[Math.Max(argc, 3)];
        for (var i = 0; i < argv.Length; i++)
            argv[i] = new VmFScriptParser.ScriptValue();
        fill(argv);
        return new VmFScriptParser.ParserContext
        {
            TArgc = argc,
            TArgv = argv,
            IntValue = v => v.IntValue,
            StringValue = v => v.String,
            ActorValue = v => v.Mobj,
            PlayerInGame = n => n == 0,
            PlayerMo = _ => sim.Players.First(),
            PlayerNum = o => o is PlayerPawn p ? p.PlayerNum : -1,
            Simulation = sim,
        };
    }

    [Fact]
    public void SfPlayerKeys_ReadBlueCard()
    {
        var sim = Room();
        var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "BlueCard", 1);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 0;
        });
        VmFScriptParser.SfPlayerKeys(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfPlayerAmmo_ReadBullets()
    {
        var sim = Room();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 0;
        });
        VmFScriptParser.SfPlayerAmmo(ctx);
        Assert.True(ctx.TReturn.IntValue >= 0);
    }

    [Fact]
    public void SfMaxPlayerAmmo_ReturnsCap()
    {
        var sim = Room();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 0;
        });
        VmFScriptParser.SfMaxPlayerAmmo(ctx);
        Assert.Equal(200, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfPlayerWeapon_TakeRemovesOwnedWeapon()
    {
        var sim = Room();
        var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        player.Inventory.Selected = WeaponKind.Shotgun;
        var ctx = Ctx(sim, 3, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 2;
            argv[2].Type = VmFScriptParser.SvtInt;
            argv[2].IntValue = 0;
        });
        VmFScriptParser.SfPlayerWeapon(ctx);
        Assert.False(player.Inventory.Owns(WeaponKind.Shotgun));
    }

    [Fact]
    public void SfPlayerWeapon_OwnsShotgun()
    {
        var sim = Room();
        AcsPlayerInventory.Give(sim.Players.Single(), "Shotgun", 1);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtInt;
            argv[1].IntValue = 2;
        });
        VmFScriptParser.SfPlayerWeapon(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfPlayerSelectedWeapon_ReturnsSelectedIndex()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Selected = WeaponKind.Pistol;
        var ctx = Ctx(sim, 1, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
        });
        VmFScriptParser.SfPlayerSelectedWeapon(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfGiveInventory_GivesKey()
    {
        var sim = Room();
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "BlueCard";
        });
        VmFScriptParser.SfGiveInventory(ctx);
        Assert.True(sim.Players.Single().Inventory.BlueKey);
    }

    [Fact]
    public void SfCheckInventory_FindsArmor()
    {
        var sim = Room();
        var player = sim.Players.Single();
        PickupCatalog.GiveArmor(player.Inventory, 100, 50, "GreenArmor");
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "BasicArmor";
        });
        VmFScriptParser.SfCheckInventory(ctx);
        Assert.Equal(100, ctx.TReturn.IntValue);
    }

    [Fact]
    public void SfSetWeapon_SelectsOwned()
    {
        var sim = Room();
        AcsPlayerInventory.Give(sim.Players.Single(), "Shotgun", 1);
        var ctx = Ctx(sim, 2, argv =>
        {
            argv[0].Type = VmFScriptParser.SvtInt;
            argv[0].IntValue = 0;
            argv[1].Type = VmFScriptParser.SvtString;
            argv[1].String = "Shotgun";
        });
        VmFScriptParser.SfSetWeapon(ctx);
        Assert.Equal(1, ctx.TReturn.IntValue);
    }
}
