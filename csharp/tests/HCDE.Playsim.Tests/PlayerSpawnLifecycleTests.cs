namespace HCDE.Playsim.Tests;

public class PlayerSpawnLifecycleTests
{
    [Fact]
    public void PickClass_UsesSinglePlayerTable()
    {
        var result = PlayerSpawnClassPicker.Resolve(new PlayerSpawnClassPicker.PickContext
        {
            PlayerClassCount = 3,
            Deathmatch = false,
            Multiplayer = true,
            SinglePlayerClassIndex = 2,
        });
        Assert.True(result.Picked);
        Assert.Equal(2, result.ClassIndex);
    }

    [Fact]
    public void PickClass_RandomWhenUserInfoUnsetInDeathmatch()
    {
        var result = PlayerSpawnClassPicker.Resolve(new PlayerSpawnClassPicker.PickContext
        {
            PlayerClassCount = 4,
            Deathmatch = true,
            Multiplayer = true,
            RandomClassChoice = 7,
        });
        Assert.Equal(3, result.ClassIndex);
    }

    [Fact]
    public void Lifecycle_RebornCallsPlayerReborn()
    {
        var plan = PlayerSpawnLifecyclePolicy.Plan(new PlayerSpawnLifecyclePolicy.LifecycleContext
        {
            State = PlayerSpawnLifecyclePolicy.PlayerSpawnState.Reborn,
            RememberLastWeaponDmFlag = true,
            PlayerNumber = 1,
            PlayerHealth = 80,
        });
        Assert.True(plan.CallPlayerReborn);
        Assert.True(plan.RememberReadyWeapon);
        Assert.True(plan.ApplyPlayerTranslation);
        Assert.Equal(80, plan.SpawnHealth);
    }

    [Fact]
    public void Lifecycle_TransfersVoodooInventory()
    {
        var plan = PlayerSpawnLifecyclePolicy.Plan(new PlayerSpawnLifecyclePolicy.LifecycleContext
        {
            State = PlayerSpawnLifecyclePolicy.PlayerSpawnState.Live,
            OldActorOwnedByPlayer = true,
        });
        Assert.False(plan.CallPlayerReborn);
        Assert.True(plan.TransferVoodooInventory);
        Assert.True(plan.CancelOldActorScripts);
    }
}
