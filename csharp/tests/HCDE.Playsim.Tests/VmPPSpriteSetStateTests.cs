namespace HCDE.Playsim.Tests;

public class VmPPSpriteSetStateTests
{
    [Fact]
    public void SetState_ClearsWeaponReadyOnWeaponLayer()
    {
        var ready = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufWeapon, Tics = 1 };
        var ctx = new VmPPSpriteSetState.SetStateContext
        {
            Id = VmPPSpriteSetState.PspWeapon,
            WeaponState = VmPPSpriteSetState.WfWeaponready | VmPPSpriteSetState.WfWeaponswitchok,
        };
        VmPPSpriteSetState.SetState(ctx, ready, false);
        Assert.Equal(0, ctx.WeaponState);
        Assert.Equal(1, ctx.Tics);
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
        var ctx = new VmPPSpriteSetState.SetStateContext
        {
            Id = VmPPSpriteSetState.PspWeapon,
            Flags = VmPPSpriteSetState.PspfCvarfast,
            SvFastWeapons = 1,
            OwnerMo = new object(),
        };
        VmPPSpriteSetState.SetState(ctx, st, false);
        Assert.Equal(1, ctx.Tics);
    }

    [Fact]
    public void SetState_ActionJumpContinuesWithZeroTics()
    {
        var jump = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufWeapon, Tics = 3 };
        var first = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufWeapon, Tics = 0, ActionFunc = new object() };
        var ctx = new VmPPSpriteSetState.SetStateContext
        {
            Id = VmPPSpriteSetState.PspWeapon,
            OwnerMo = new object(),
            CallAction = (_, _, _, _) => jump,
        };
        VmPPSpriteSetState.SetState(ctx, first, false);
        Assert.Equal(jump, ctx.State);
        Assert.Equal(3, ctx.Tics);
    }

    [Fact]
    public void SetState_RejectsOverlayOnlyStateOnWeaponCaller()
    {
        var st = new VmPPSpriteSetState.StateModel { UseFlags = VmPPSpriteSetState.SufOverlay, Tics = 1 };
        var destroyed = false;
        var ctx = new VmPPSpriteSetState.SetStateContext
        {
            Id = VmPPSpriteSetState.PspWeapon,
            CallerIsWeapon = true,
            Destroy = () => destroyed = true,
        };
        VmPPSpriteSetState.SetState(ctx, st, false);
        Assert.True(destroyed);
        Assert.Null(ctx.State);
    }
}
