namespace HCDE.Playsim.Tests;

public class VmPMobjActivateDestroy1116SliceTests
{
    [Fact]
    public void IsFast_NeverFastOverridesSkill()
    {
        Assert.True(VmPMobjActivateDestroy1116Slice.IsFast(
            VmPMobjActivateDestroy1116Slice.Mf5NeverFast | VmPMobjActivateDestroy1116Slice.Mf5AlwaysFast,
            skillFastMonsters: false));
        Assert.False(VmPMobjActivateDestroy1116Slice.IsFast(
            VmPMobjActivateDestroy1116Slice.Mf5NeverFast,
            skillFastMonsters: true));
        Assert.True(VmPMobjActivateDestroy1116Slice.IsFast(0, true));
    }

    [Fact]
    public void RunActivate_WakesDormantMonster()
    {
        var ctx = new VmPMobjActivateDestroy1116Slice.ActivationContext
        {
            Flags3 = VmPMobjActivateDestroy1116Slice.Mf3IsMonster,
            Flags2 = VmPMobjActivateDestroy1116Slice.Mf2Dormant,
            Health = 100,
            HasActiveState = false,
            Tics = 10
        };
        VmPMobjActivateDestroy1116Slice.RunActivate(ctx);
        Assert.Equal(0, ctx.Flags2 & VmPMobjActivateDestroy1116Slice.Mf2Dormant);
        Assert.Equal(1, ctx.Tics);
    }

    [Fact]
    public void RunDeactivate_SetsDormantWithoutInactiveState()
    {
        var ctx = new VmPMobjActivateDestroy1116Slice.ActivationContext
        {
            Flags3 = VmPMobjActivateDestroy1116Slice.Mf3IsMonster,
            Health = 1
        };
        VmPMobjActivateDestroy1116Slice.RunDeactivate(ctx);
        Assert.True((ctx.Flags2 & VmPMobjActivateDestroy1116Slice.Mf2Dormant) != 0);
        Assert.Equal(-1, ctx.Tics);
    }

    [Fact]
    public void RunOnDestroyPremorph_ClearsLivingAlternative()
    {
        var ctx = new VmPMobjActivateDestroy1116Slice.OnDestroyContext
        {
            HasAlternative = true,
            HasLevel = true
        };
        VmPMobjActivateDestroy1116Slice.RunOnDestroyPremorph(ctx);
        Assert.True(ctx.DestroyAlternative);
        Assert.True(ctx.WorldDestroyed);
        Assert.True(ctx.ClearBehaviors);
    }
}
