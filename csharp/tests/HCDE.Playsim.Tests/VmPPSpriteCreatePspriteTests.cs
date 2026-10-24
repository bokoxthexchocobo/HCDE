namespace HCDE.Playsim.Tests;

public class VmPPSpriteCreatePspriteTests
{
    [Fact]
    public void Construct_InsertsSortedAndReplacesDuplicateId()
    {
        var owner = new VmPPSpriteCreatePsprite.PlayerOwner();
        var existing = new VmPPSpriteLayers.PspriteModel { Id = 2 };
        var dup = new VmPPSpriteLayers.PspriteModel { Id = 3 };
        existing.Next = dup;
        owner.Psprites = existing;
        var destroyed = false;

        var pspr = VmPPSpriteCreatePsprite.Construct(
            owner,
            "caller",
            3,
            (_, _, id) => new VmPPSpriteLayers.PspriteModel { Id = id },
            _ => true,
            _ => destroyed = true);

        Assert.Equal(3, pspr.Id);
        Assert.Same(existing, owner.Psprites);
        Assert.Same(pspr, existing.Next);
        Assert.True(destroyed);
    }

    [Fact]
    public void CreatePsprite_AppliesWeaponScaleWhenCallerIsWeapon()
    {
        var owner = new VmPPSpriteCreatePsprite.PlayerOwner();
        var result = VmPPSpriteCreatePsprite.CreatePsprite(
            owner,
            "weapon",
            1,
            (_, _, _) => new VmPPSpriteLayers.PspriteModel(),
            w => w as string == "weapon",
            (_, name) => name == "WeaponScaleX" ? 2.0 : 0.5,
            _ => true,
            null);
        Assert.NotNull(result);
        Assert.Equal(2.0, result!.BaseScaleX);
        Assert.Equal(0.5, result.BaseScaleY);
        Assert.Same(result, owner.Psprites);
    }
}
