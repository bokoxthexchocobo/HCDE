namespace HCDE.Playsim.Tests;

public class VmPPSpriteClearBulletSetupTests
{
    [Fact]
    public void ClearOverlays_SafetySkipsWeaponLayers()
    {
        var flash = new VmPPSpriteLayers.PspriteModel { Id = VmPPSpriteLayers.PspFlash };
        var weapon = new VmPPSpriteLayers.PspriteModel { Id = VmPPSpriteLayers.PspWeapon, Next = flash };
        var custom = new VmPPSpriteLayers.PspriteModel { Id = 50, Next = weapon };
        var cleared = new List<int>();
        var player = new VmPPSpriteClearBulletSetup.ClearPlayerModel
        {
            Psprites = custom,
            SetStateNull = p => cleared.Add(p.Id)
        };

        var count = VmPPSpriteClearBulletSetup.ClearOverlays(player, 0, 0, safety: true);
        Assert.Equal(1, count);
        Assert.DoesNotContain(VmPPSpriteLayers.PspWeapon, cleared);
    }

    [Fact]
    public void BulletSlope_TriesAngledShotsUntilTarget()
    {
        var calls = 0;
        var target = new VmPPSpriteClearBulletSetup.LineTargetScratch();
        var pitch = VmPPSpriteClearBulletSetup.BulletSlope(
            new VmPPSpriteClearBulletSetup.BulletSlopeContext
            {
                AimLineAttackDegrees = (_, _, scratch, _) =>
                {
                    calls++;
                    if (calls == 3)
                        scratch.Linetarget = new object();
                    return calls;
                }
            },
            new object(),
            target,
            0);
        Assert.Equal(3, pitch);
        Assert.Equal(3, calls);
    }

    [Fact]
    public void SetupPsprites_DestroyAndBringUp()
    {
        var destroyed = false;
        var broughtUp = false;
        var player = new VmPPSpriteWeaponBob.PlayerModel
        {
            PlayerIndex = 2,
            ReadyWeapon = new object(),
        };
        VmPPSpriteClearBulletSetup.SetupPsprites(
            new VmPPSpriteClearBulletSetup.SetupContext
            {
                Player = player,
                DestroyPsprites = () => destroyed = true
            },
            startweaponup: false,
            ctx =>
            {
                broughtUp = true;
                Assert.Same(player, ctx.Player);
            });
        Assert.True(destroyed);
        Assert.True(broughtUp);
        Assert.Same(player.ReadyWeapon, player.PendingWeapon);
    }
}
