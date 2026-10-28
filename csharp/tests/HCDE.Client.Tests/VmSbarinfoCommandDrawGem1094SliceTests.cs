namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawGem1094SliceTests
{
    [Fact]
    public void ValidateDrawGemChainSizeFromToken_AddsOne()
    {
        var (valid, size) = VmSbarinfoCommandDrawGem1094Slice.ValidateDrawGemChainSizeFromToken(4);
        Assert.True(valid);
        Assert.Equal(5, size);
    }

    [Fact]
    public void ResolveDrawGemInterpolationSpeedForScript_ArmorUsesScriptSpeed()
    {
        Assert.Equal(
            20,
            VmSbarinfoCommandDrawGem1094Slice.ResolveDrawGemInterpolationSpeedForScript(
                armorMode: true,
                scriptInterpolateHealth: false,
                scriptHealthInterpolationSpeed: 0,
                scriptInterpolateArmor: true,
                scriptArmorInterpolationSpeed: 20,
                parsedInterpolationSpeed: 3));
    }

    [Fact]
    public void ApplyDrawGemReverseGoalPercent_FlipsWhenReverse()
    {
        Assert.Equal(25, VmSbarinfoCommandDrawGem1094Slice.ApplyDrawGemReverseGoalPercent(
            reverse: true,
            goalPercent: 75));
    }

    [Fact]
    public void StepDrawGemDrawValueTowardGoal_ClampsStep()
    {
        Assert.Equal(
            92,
            VmSbarinfoCommandDrawGem1094Slice.StepDrawGemDrawValueTowardGoal(
                drawValue: 100,
                goalValue: 50,
                interpolationSpeed: 8));
    }

    [Fact]
    public void ResolveDrawGemChainWiggleBit_RequiresOddLevelTime()
    {
        Assert.Equal(1, VmSbarinfoCommandDrawGem1094Slice.ResolveDrawGemChainWiggleBit(
            wiggleEnabled: true,
            levelTime: 3,
            randomValue: 5));
    }

    [Fact]
    public void ShouldReplaceWeaponAmmoClass_WhenNotAmmo()
    {
        Assert.True(VmSbarinfoCommandWeaponAmmo1094Slice.ShouldReplaceWeaponAmmoClass(
            classFound: true,
            isAmmoKind: false));
    }

    [Fact]
    public void ResolveWeaponAmmoSecondAmmoSeparator_OrUsesDisjunction()
    {
        var (parseSecond, conditionAnd) = VmSbarinfoCommandWeaponAmmo1094Slice.ResolveWeaponAmmoSecondAmmoSeparator(
            orOrToken: true,
            andAndToken: false);
        Assert.True(parseSecond);
        Assert.False(conditionAnd);
    }

    [Fact]
    public void ResolveWeaponAmmoDualTypeFlowTruth_AndRequiresBoth()
    {
        Assert.True(VmSbarinfoCommandWeaponAmmo1094Slice.ResolveWeaponAmmoDualTypeFlowTruth(
            conditionAnd: true,
            primarySlotMatch: true,
            secondarySlotMatch: true));
    }

    [Fact]
    public void ResolveWeaponAmmoSingleConfiguredTypeTruth_MatchesSecondary()
    {
        Assert.True(VmSbarinfoCommandWeaponAmmo1094Slice.ResolveWeaponAmmoSingleConfiguredTypeTruth(
            usesPrimaryAmmo: false,
            primaryMatches: false,
            usesSecondaryAmmo: true,
            secondaryMatches: true));
    }

    [Fact]
    public void ComputeDrawGemGoalPercent_ClampsAt100()
    {
        Assert.Equal(100, VmSbarinfoCommandDrawGem1094Slice.ComputeDrawGemGoalPercent(
            goalStat: 250,
            maxStat: 100));
    }
}
