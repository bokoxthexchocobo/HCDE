using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class Phase5SimulationTests
{
    [Fact]
    public void Stimpack_HealsToOneHundredAndStaysWhenAlreadyFull()
    {
        var healing = Touch(PickupCatalog.Stimpack, health: 90);
        healing.Tick();
        Assert.Equal(100, healing.Players.Single().Health);
        Assert.Single(healing.Actors);

        var full = Touch(PickupCatalog.Stimpack, health: 100);
        full.Tick();
        Assert.Equal(100, full.Players.Single().Health);
        Assert.Equal(2, full.Actors.Count);
    }

    [Fact]
    public void SoulsphereAndMedikit_UseTheirOwnCaps()
    {
        var soul = Touch(PickupCatalog.Soulsphere);
        soul.Tick();
        Assert.Equal(200, soul.Players.Single().Health);

        var kit = Touch(PickupCatalog.Medikit, health: 90);
        kit.Tick();
        Assert.Equal(100, kit.Players.Single().Health);

        var bonus = Touch(PickupCatalog.HealthBonus);
        bonus.Tick();
        Assert.Equal(101, bonus.Players.Single().Health);
    }

    [Fact]
    public void Armor_GreenThenMegaAndABonusOnEmpty()
    {
        var green = Touch(PickupCatalog.GreenArmor);
        green.Tick();
        var inventory = green.Players.Single().Inventory;
        Assert.Equal(100, inventory.Armor);
        Assert.Equal(PlayerInventory.GreenSavePercent, inventory.ArmorSavePercent);

        var rejected = Touch(PickupCatalog.GreenArmor);
        rejected.Players.Single().Inventory.Armor = 100;
        rejected.Tick();
        Assert.Equal(2, rejected.Actors.Count);

        var mega = Touch(PickupCatalog.MegaArmor);
        mega.Players.Single().Inventory.Armor = 100;
        mega.Tick();
        Assert.Equal(200, mega.Players.Single().Inventory.Armor);
        Assert.Equal(PlayerInventory.MegaSavePercent, mega.Players.Single().Inventory.ArmorSavePercent);

        var bonus = Touch(PickupCatalog.ArmorBonus);
        bonus.Tick();
        Assert.Equal(1, bonus.Players.Single().Inventory.Armor);
        Assert.Equal(PlayerInventory.GreenSavePercent, bonus.Players.Single().Inventory.ArmorSavePercent);
    }

    [Fact]
    public void Clip_AddsTenAndStaysAtTheCap()
    {
        var clip = Touch(PickupCatalog.Clip);
        clip.Tick();
        Assert.Equal(60, clip.Players.Single().Inventory.Bullets);

        var full = Touch(PickupCatalog.Clip);
        full.Players.Single().Inventory.Bullets = full.Players.Single().Inventory.MaxBullets;
        full.Tick();
        Assert.Equal(2, full.Actors.Count);
    }

    [Fact]
    public void BlueCard_SetsTheColorAndSinglePlayerConsumesBothKeys()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = PickupCatalog.BlueCard, X = 32, Y = 64 },
                new LevelThing { Type = PickupCatalog.BlueSkull, X = 32, Y = 64 },
            },
        });
        sim.Tick();
        Assert.True(sim.Players.Single().Inventory.BlueKey);
        Assert.Single(sim.Actors);
    }

    [Fact]
    public void Weapons_GrantTheOwnedFlagAndTheirAmmo()
    {
        var gun = Touch(PickupCatalog.Shotgun);
        gun.Tick();
        var inventory = gun.Players.Single().Inventory;
        Assert.True(inventory.Owns(WeaponKind.Shotgun));
        Assert.Equal(8, inventory.Shells);

        var saw = Touch(PickupCatalog.Chainsaw);
        saw.Tick();
        Assert.True(saw.Players.Single().Inventory.Owns(WeaponKind.Chainsaw));
        Assert.Equal(0, saw.Players.Single().Inventory.Shells);

        var owned = Touch(PickupCatalog.Shotgun);
        owned.Players.Single().Inventory.Weapons |= WeaponKind.Shotgun;
        owned.Players.Single().Inventory.Shells = owned.Players.Single().Inventory.MaxShells;
        owned.Tick();
        Assert.Equal(2, owned.Actors.Count);
    }

    [Fact]
    public void IncorrectBlockmapCannotLetActorsPassThroughWalls()
    {
        var open = AuthoritySimulation.Start(WallLevel(blockWithLine: 1));
        open.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        open.Tick();
        Assert.Equal(32, open.Players.Single().X.ToDouble());

        var closed = AuthoritySimulation.Start(WallLevel(blockWithLine: 0));
        closed.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        closed.Tick();
        Assert.Equal(32, closed.Players.Single().X.ToDouble());
    }

    [Fact]
    public void SolidActor_StopsThePlayerAndAPickupDoesNot()
    {
        var blocked = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, X = 50, Y = 64 },
            },
        });
        blocked.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        blocked.Tick();
        Assert.Equal(32, blocked.Players.Single().X.ToDouble());

        var pickup = Touch(PickupCatalog.Stimpack, health: 90);
        pickup.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        pickup.Tick();
        Assert.Equal(33, pickup.Players.Single().X.ToDouble());
        Assert.Equal(100, pickup.Players.Single().Health);
    }

    [Fact]
    public void FloorRaiseLowerAndLift_MoveEightUnits()
    {
        var raised = AuthoritySimulation.Start(FloorLevel(LineSpecials.FloorRaise));
        raised.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        raised.Tick();
        Assert.Equal(8, raised.FloorOf(0));
        Assert.Equal(0, raised.Level.Lines[0].Special);

        var lowered = AuthoritySimulation.Start(FloorLevel(LineSpecials.FloorLower));
        lowered.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        lowered.Tick();
        Assert.Equal(-8, lowered.FloorOf(0));

        var lift = AuthoritySimulation.Start(FloorLevel(LineSpecials.Lift));
        lift.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 });
        lift.Tick();
        Assert.Equal(-8, lift.FloorOf(0));
        for (var i = 0; i < 6; i++)
            lift.Tick();
        Assert.Equal(0, lift.FloorOf(0));
    }

    private static AuthoritySimulation Touch(int type, int health = 100)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = (short)type, X = 32, Y = 64 },
            },
        });
        sim.Players.Single().Health = health;
        return sim;
    }

    private static PlayLevel FloorLevel(int special) => new()
    {
        MapName = "MAP01",
        Sectors = new[] { new LevelSector { FloorHeight = 0, CeilingHeight = 128, Tag = 1 } },
        Lines = new[]
        {
            new LevelLine
            {
                X1 = 32.5,
                Y1 = 0,
                X2 = 32.5,
                Y2 = 128,
                SideFront = 0,
                SideBack = 0,
                Special = special,
                Tag = 1,
            },
        },
        Things = new[] { new LevelThing { Type = 1, X = 32, Y = 64 } },
    };

    private static PlayLevel WallLevel(int blockWithLine)
    {
        var bytes = new byte[22];
        void Write(int index, short value) => BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(index * 2), value);
        Write(2, 2);
        Write(3, 1);
        if (blockWithLine == 0)
        {
            Write(4, 6);
            Write(5, 9);
            Write(6, 0);
            Write(7, 0);
            Write(8, -1);
            Write(9, 0);
            Write(10, -1);
        }
        else
        {
            Write(4, 6);
            Write(5, 8);
            Write(6, 0);
            Write(7, -1);
            Write(8, 0);
            Write(9, 0);
            Write(10, -1);
        }

        Assert.True(MapBlockmapCodec.TryRead(bytes, out var blockmap, out var error), error);
        return new PlayLevel
        {
            MapName = "MAP01",
            Lines = new[]
            {
                new LevelLine { X1 = 40, Y1 = 0, X2 = 40, Y2 = 128, SideBack = LevelLine.NoSide },
            },
            Things = new[] { new LevelThing { Type = 1, X = 32, Y = 64 } },
            Blockmap = blockmap,
        };
    }
}
