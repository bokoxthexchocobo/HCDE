using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class Phase6CombatTests
{
    [Fact]
    public void Pistol_HitsTheActorAheadAndSpendsOneBullet()
    {
        var sim = Range(3001, 200);
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(20, Target(sim).Health);
        Assert.Equal(49, sim.Players.Single().Inventory.Bullets);
    }

    [Fact]
    public void Pistol_MissesBehindAndStillSpendsTheBullet()
    {
        var sim = Range(3001, 0);
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(30, Target(sim).Health);
        Assert.Equal(49, sim.Players.Single().Inventory.Bullets);
    }

    [Fact]
    public void Pistol_DoesNotFireWithoutBullets()
    {
        var sim = Range(3001, 200);
        sim.Players.Single().Inventory.Bullets = 0;
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(30, Target(sim).Health);
        Assert.Equal(0, sim.Players.Single().Inventory.Bullets);
    }

    [Fact]
    public void Fist_HitsInsideMeleeRangeAndDoesNotSpendAmmo()
    {
        var sim = Range(3001, 80);
        var player = sim.Players.Single();
        player.Inventory.Selected = WeaponKind.Fist;
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(20, Target(sim).Health);
        Assert.Equal(50, player.Inventory.Bullets);

        var far = Range(3001, 200);
        far.Players.Single().Inventory.Selected = WeaponKind.Fist;
        far.QueueCommand(0, new PlayerCommand { Attack = true });
        far.Tick();
        Assert.Equal(30, Target(far).Health);
    }

    [Fact]
    public void GreenArmor_AbsorbsAThirdOfPistolDamage()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 2, X = 200, Y = 64 },
            },
        });
        var target = sim.Players.Single(player => player.PlayerNum == 1);
        target.Inventory.Armor = 100;
        target.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        var before = target.Health;
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
        Assert.Equal(97, target.Inventory.Armor);
        Assert.Equal(before - 7, target.Health);
    }

    private static AuthoritySimulation Range(int type, int x) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = new[]
            {
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = (short)type, X = (short)x, Y = 64 },
            },
        });

    private static Actor Target(AuthoritySimulation sim) =>
        sim.Actors.Single(actor => actor is not PlayerPawn);
}
