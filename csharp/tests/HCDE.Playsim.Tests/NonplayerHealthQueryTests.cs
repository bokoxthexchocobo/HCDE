namespace HCDE.Playsim.Tests;

public class NonplayerHealthQueryTests
{
    private sealed class CustomMaximumActor : Actor
    {
        public bool? RequestedUpgrades { get; private set; }
        public override int GetMaxHealth(bool withUpgrades)
        {
            RequestedUpgrades = withUpgrades;
            return 175;
        }
    }

    [Fact]
    public void MaximumQueryDispatchesVirtualMethodWithoutUpgrades()
    {
        var actor = new CustomMaximumActor { Health = 30, ResurrectionHealth = 60 };
        Assert.Equal(175, AcsPlayerInventory.Count(actor, ["health"], 0, true));
        Assert.Equal(false, actor.RequestedUpgrades);
        Assert.Equal(30, AcsPlayerInventory.Count(actor, "Health", false));
    }

    [Fact]
    public void PlayerMaximumQueryExcludesHealthUpgradesAndPickupOverride()
    {
        var player = new PlayerPawn { MaxHealth = 150, Stamina = 10, BonusHealth = 20, MaxPickupHealth = 200 };
        Assert.Equal(150, AcsPlayerInventory.Count(player, "Health", true));
    }
    [Theory]
    [InlineData(20, 60)]
    [InlineData(0, 300)]
    [InlineData(-10, 1000)]
    [InlineData(20, 0)]
    [InlineData(20, -7)]
    public void ActorHealthQueryReportsCurrentAndVirtualMaximum(int current, int spawn)
    {
        var actor = new Actor { Health = current, ResurrectionHealth = spawn };
        Assert.Equal(current, AcsPlayerInventory.Count(actor, "health", false));
        Assert.Equal(100, AcsPlayerInventory.Count(actor, "Health", true));
    }

    [Fact]
    public void StringTableQueryUsesNonplayerHealthPath()
    {
        var actor = new Actor { Health = 30, ResurrectionHealth = 60 };
        Assert.Equal(30, AcsPlayerInventory.Count(actor, ["Health"], 0, false));
        Assert.Equal(100, AcsPlayerInventory.Count(actor, ["Health"], 0, true));
        Assert.Equal(0, AcsPlayerInventory.Count(actor, ["Health"], -1, false));
        Assert.Equal(0, AcsPlayerInventory.Count(actor, "Clip", false));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-7)]
    [InlineData(200)]
    public void SpawnHealthWritesDoNotChangeNonplayerHealth(int value)
    {
        var sim = AuthoritySimulation.Start(new HCDE.MapLoader.PlayLevel
        {
            MapName = "MAP01",
            Sectors = [new HCDE.MapLoader.LevelSector { Index = 0, CeilingHeight = 128 }],
            Things = [new HCDE.MapLoader.LevelThing { Type = 1 }],
        });
        var actor = new Actor { Health = 30, ResurrectionHealth = 60 };
        AcsActorProperties.Set(sim, actor, 0, AcsActorProperties.SpawnHealth, value);
        Assert.Equal(30, actor.Health);
        Assert.Equal(60, actor.ResurrectionHealth);
        Assert.Equal(100, AcsActorProperties.Get(sim, actor, 0, AcsActorProperties.SpawnHealth));
        Assert.Equal(100, AcsPlayerInventory.Count(actor, "Health", true));
    }
}
