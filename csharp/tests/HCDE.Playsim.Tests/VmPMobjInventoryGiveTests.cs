namespace HCDE.Playsim.Tests;

public class VmPMobjInventoryGiveTests
{
    [Fact]
    public void DropInventory_VmPath_ReturnsResult()
    {
        var ctx = new VmPMobjInventoryGive.DropInventoryContext
        {
            VmDropInventory = (_, _, amt) => amt == 3 ? new object() : null,
        };
        Assert.NotNull(VmPMobjInventoryGive.DropInventory(ctx, new(), new(), 3));
        Assert.Null(VmPMobjInventoryGive.DropInventory(ctx, new(), new(), 1));
    }

    [Fact]
    public void CopyFriendliness_CopiesFlagsAndTarget()
    {
        var self = new VmPMobjInventoryGive.FriendActor { Health = 10, Flags = 0 };
        var other = new VmPMobjInventoryGive.FriendActor
        {
            Flags = VmPMobjInventoryGive.MfFriendly,
            Flags3 = VmPMobjInventoryGive.Mf3Huntplayers,
            Target = new(),
            FriendPlayer = 2,
        };
        var deltas = 0;
        var ctx = new VmPMobjInventoryGive.FriendlinessContext
        {
            CountsAsKill = _ => true,
            AdjustTotalMonsters = add => deltas += add ? 1 : -1,
        };
        VmPMobjInventoryGive.CopyFriendliness(ctx, self, other, changeTarget: true, resetHealth: false);
        Assert.Equal(VmPMobjInventoryGive.MfFriendly, self.Flags & VmPMobjInventoryGive.MfFriendly);
        Assert.Equal(2, self.FriendPlayer);
        Assert.Same(other.Target, self.Target);
        Assert.Equal(0, deltas);
    }

    [Fact]
    public void CopyFriendliness_ResetHealth_UsesSpawnHealth()
    {
        var self = new VmPMobjInventoryGive.FriendActor { Health = 5 };
        var other = new VmPMobjInventoryGive.FriendActor();
        var ctx = new VmPMobjInventoryGive.FriendlinessContext { SpawnHealth = _ => 100 };
        VmPMobjInventoryGive.CopyFriendliness(ctx, self, other, false, resetHealth: true);
        Assert.Equal(100, self.Health);
    }

    [Fact]
    public void GetRealMaxHealth_MaxPickupHealthCaps()
    {
        var q = new VmPMobjInventoryGive.MaxHealthQuery
        {
            ExplicitMax = 0,
            MaxPickupHealth = 120,
            DefaultMaxHealth = () => 200,
        };
        Assert.Equal(120, VmPMobjInventoryGive.GetRealMaxHealth(q));
    }

    [Fact]
    public void GetRealMaxHealth_ExplicitMaxAddsBonus()
    {
        var q = new VmPMobjInventoryGive.MaxHealthQuery
        {
            ExplicitMax = 80,
            BonusHealth = 20,
            HasAlternative = false,
        };
        Assert.Equal(100, VmPMobjInventoryGive.GetRealMaxHealth(q));
    }

    [Fact]
    public void GiveBody_DeadActor_ReturnsFalse()
    {
        var actor = new VmPMobjInventoryGive.GiveBodyActor { Health = 0 };
        Assert.False(VmPMobjInventoryGive.GiveBody(new(), actor, 10, 0));
    }

    [Fact]
    public void GiveBody_PlayerNegativePercent_RaisesHealth()
    {
        var actor = new VmPMobjInventoryGive.GiveBodyActor
        {
            Health = 10,
            HasPlayer = true,
            PlayerHealth = 10,
        };
        var ctx = new VmPMobjInventoryGive.GiveBodyContext
        {
            ResolveMaxHealth = (_, _) => 100,
        };
        Assert.True(VmPMobjInventoryGive.GiveBody(ctx, actor, -50, 0));
        Assert.Equal(50, actor.PlayerHealth);
        Assert.Equal(50, actor.Health);
    }

    [Fact]
    public void GiveBodyAction_Delegates()
    {
        var actor = new VmPMobjInventoryGive.GiveBodyActor { Health = 1, HasPlayer = false };
        var ctx = new VmPMobjInventoryGive.GiveBodyContext { MonsterSpawnHealth = _ => 100 };
        Assert.True(VmPMobjInventoryGive.GiveBodyAction(ctx, actor, 5, 0));
        Assert.Equal(6, actor.Health);
    }
}
