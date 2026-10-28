namespace HCDE.Playsim.Tests;

public class VmPPSpriteSetStateTests
{
    [Fact]
    public void SetState_ClearsWeaponReadyOnWeaponLayer()
    {
        var ready = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufWeapon, Tics = 1 };
        var psp = new VmPPSpriteLayers.PspriteModel
        {
            Id = VmPPSpriteSetState.PspWeapon,
            WeaponState = VmPPSpriteSetState.WfWeaponready | VmPPSpriteSetState.WfWeaponswitchok,
        };
        VmPPSpriteSetState.SetState(psp, ready, false);
        Assert.Equal(0, psp.WeaponState);
        Assert.Equal(1, psp.Tics);
    }

    [Fact]
    public void SetState_FastWeaponsCvarForcesOneTic()
    {
        var st = new VmPPSpriteSetState.StateModel
        {
            UseFlags = VmPPSpriteSetState.SufWeapon,
            Tics = 5,
            ActionFunc = new object(),
        };
        var psp = new VmPPSpriteLayers.PspriteModel
        {
            Id = VmPPSpriteSetState.PspWeapon,
            Flags = VmPPSpriteSetState.PspfCvarfast,
            SvFastWeapons = 1,
            OwnerMo = new object(),
        };
        VmPPSpriteSetState.SetState(psp, st, false);
        Assert.Equal(1, psp.Tics);
    }

    [Fact]
    public void SetState_ActionJumpContinuesWithZeroTics()
    {
        var jump = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufWeapon, Tics = 3 };
        var first = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufWeapon, Tics = 0, ActionFunc = new object() };
        var psp = new VmPPSpriteLayers.PspriteModel
        {
            Id = VmPPSpriteSetState.PspWeapon,
            OwnerMo = new object(),
        };
        VmPPSpriteSetState.SetState(
            psp,
            first,
            false,
            new VmPPSpriteSetState.SetStateSeams { CallAction = (_, _, _, _) => jump });
        Assert.Equal(jump, psp.State);
        Assert.Equal(3, psp.Tics);
    }

    [Fact]
    public void SetState_RejectsOverlayOnlyStateOnWeaponCaller()
    {
        var st = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufOverlay, Tics = 1 };
        var destroyed = false;
        var psp = new VmPPSpriteLayers.PspriteModel
        {
            Id = VmPPSpriteSetState.PspWeapon,
            CallerIsWeapon = true,
        };
        VmPPSpriteSetState.SetState(
            psp,
            st,
            false,
            new VmPPSpriteSetState.SetStateSeams { Destroy = () => destroyed = true });
        Assert.True(destroyed);
        Assert.Null(psp.State);
    }
}
