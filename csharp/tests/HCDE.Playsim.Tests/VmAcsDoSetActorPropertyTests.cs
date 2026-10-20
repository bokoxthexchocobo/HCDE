namespace HCDE.Playsim.Tests;

public class VmAcsDoSetActorPropertyTests
{
    [Fact]
    public void ShouldSkipHealthOnDead_WhenCorpseOrPlayerDead()
    {
        Assert.True(VmAcsDoSetActorProperty.ShouldSkipHealthOnDead(0, false));
        Assert.True(VmAcsDoSetActorProperty.ShouldSkipHealthOnDead(10, true));
        Assert.False(VmAcsDoSetActorProperty.ShouldSkipHealthOnDead(10, false));
    }

    [Fact]
    public void TryMapLegacyRenderStyle_UsesNativeTable()
    {
        Assert.True(VmAcsDoSetActorProperty.TryMapLegacyRenderStyle(
            64, VmAcsDoSetActorProperty.LegacyRenderStyleIndices, out var idx));
        Assert.Equal(6, idx);
    }

    [Fact]
    public void FriendlyToggle_AdjustsMonsterTotalWhenKillCounted()
    {
        var before = VmAcsDoSetActorProperty.MonsterTotalBeforeFriendlyToggle(10, true);
        var after = VmAcsDoSetActorProperty.MonsterTotalAfterFriendlyToggle(before, true);
        Assert.Equal(9, before);
        Assert.Equal(10, after);
    }

    [Fact]
    public void ToggleFlag_AmbushMatchesNativeMask()
    {
        var flags = VmAcsDoSetActorProperty.ToggleFlag(0, VmAcsDoSetActorProperty.MfAmbush, true);
        Assert.Equal(VmAcsDoSetActorProperty.MfAmbush, flags);
    }

    [Fact]
    public void RequiresPlayerPawn_ForJumpZOnlyAmongScalars()
    {
        Assert.True(VmAcsDoSetActorProperty.RequiresPlayerPawn(VmAcsDoSetActorProperty.Aprop.JumpZ));
        Assert.False(VmAcsDoSetActorProperty.RequiresPlayerPawn(VmAcsDoSetActorProperty.Aprop.Mass));
    }

    [Fact]
    public void UsesBehaviorStringLookup_ForSpeciesNotMass()
    {
        Assert.True(VmAcsDoSetActorProperty.UsesBehaviorStringLookup(VmAcsDoSetActorProperty.Aprop.Species));
        Assert.False(VmAcsDoSetActorProperty.UsesBehaviorStringLookup(VmAcsDoSetActorProperty.Aprop.Mass));
    }

    [Fact]
    public void PlanDispatch_HealthMatchesNativeGuards()
    {
        var plan = VmAcsDoSetActorProperty.PlanDispatch(VmAcsDoSetActorProperty.Aprop.Health);
        Assert.True(plan.SkipWhenHealthDead);
        Assert.True(plan.DieOnNonPositiveHealth);
        Assert.True(plan.SyncPlayerHealth);
    }

    [Fact]
    public void PlanDispatch_FriendlyAdjustsKillCount()
    {
        var plan = VmAcsDoSetActorProperty.PlanDispatch(VmAcsDoSetActorProperty.Aprop.Friendly);
        Assert.True(plan.AdjustKillCountAroundFriendly);
    }

    [Fact]
    public void PlanDispatch_UnknownPropertyNoOp()
    {
        var plan = VmAcsDoSetActorProperty.PlanDispatch((VmAcsDoSetActorProperty.Aprop)999);
        Assert.True(plan.NoOp);
    }
}
