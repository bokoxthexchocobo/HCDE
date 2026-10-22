namespace HCDE.Playsim.Tests;

public class VmPInteractionDamageDispatchTests
{
    [Fact]
    public void DoDamageMobj_SkipsMirrorActors()
    {
        var ctx = new VmPInteractionDamageDispatch.DamageDispatchContext { MirrorTarget = true };
        Assert.Equal(0, VmPInteractionDamageDispatch.DoDamageMobj(ctx, new object(), null, null, 10, 0, 0, 0));
    }

    [Fact]
    public void PDamageMobj_UsesVirtualWhenPresent()
    {
        var ctx = new VmPInteractionDamageDispatch.DamageDispatchContext
        {
            HasVirtualDamageMobj = true,
            VmDamageMobj = _ => 5,
        };
        Assert.Equal(5, VmPInteractionDamageDispatch.PDamageMobj(ctx, new object(), null, null, 10, 0, 0, 0));
    }

    [Fact]
    public void OkayToSwitchTarget_RejectsNull()
    {
        var self = new VmPInteractionDamageDispatch.SwitchActor();
        var ctx = new VmPInteractionDamageDispatch.DamageDispatchContext();
        Assert.False(VmPInteractionDamageDispatch.OkayToSwitchTarget(self, null, ctx));
    }

    [Fact]
    public void OkayToSwitchTarget_RejectsSelf()
    {
        var self = new VmPInteractionDamageDispatch.SwitchActor { Flags = VmPInteractionDamageDispatch.MfShootable };
        var ctx = new VmPInteractionDamageDispatch.DamageDispatchContext();
        Assert.False(VmPInteractionDamageDispatch.OkayToSwitchTarget(self, self, ctx));
    }

    [Fact]
    public void CallOkayToSwitchTarget_FallsBackToNative()
    {
        var self = new VmPInteractionDamageDispatch.SwitchActor
        {
            Flags4 = VmPInteractionDamageDispatch.Mf4Nohateplayers,
        };
        var other = new VmPInteractionDamageDispatch.SwitchActor
        {
            Flags = VmPInteractionDamageDispatch.MfShootable,
            OtherHasPlayer = true,
        };
        var ctx = new VmPInteractionDamageDispatch.DamageDispatchContext();
        Assert.False(VmPInteractionDamageDispatch.CallOkayToSwitchTarget(self, other, ctx));
    }
}
