namespace HCDE.Client.Tests;

public class VmSbarinfoCommandInInventory1095SliceTests
{
    [Fact]
    public void ShouldReplaceInInventoryItemClass_WhenNotInventoryKind()
    {
        Assert.True(VmSbarinfoCommandInInventory1095Slice.ShouldReplaceInInventoryItemClass(
            classFound: true,
            isInventoryKind: false));
    }

    [Fact]
    public void ResolveInInventorySecondItemListContinuation_AndRequiresBoth()
    {
        var (continueSecond, conditionAnd) =
            VmSbarinfoCommandInInventory1095Slice.ResolveInInventorySecondItemListContinuation(
                orOrToken: false,
                andAndToken: true);
        Assert.True(continueSecond);
        Assert.True(conditionAnd);
    }

    [Fact]
    public void ShouldNullifyInInventorySlotForInsufficientAmount()
    {
        Assert.True(VmSbarinfoCommandInInventory1095Slice.ShouldNullifyInInventorySlotForInsufficientAmount(
            slotPresent: true,
            requiredAmount: 10,
            heldAmount: 3));
    }

    [Fact]
    public void ResolveInInventoryDualConfiguredItemFlowTruth_OrEitherSlot()
    {
        Assert.True(VmSbarinfoCommandInInventory1095Slice.ResolveInInventoryDualConfiguredItemFlowTruth(
            conditionAnd: false,
            firstSlotCounts: false,
            secondSlotCounts: true));
    }

    [Fact]
    public void ResolveCommandAlphaMainBlockDrawInvocation_PassesBlockAlpha()
    {
        var (_, _, alpha) = VmSbarinfoCommandAlpha1095Slice.ResolveCommandAlphaMainBlockDrawInvocation(
            parentForceScaled: true,
            parentFullScreenOffsets: false,
            parentBlockAlpha: 192);
        Assert.Equal(192, alpha);
    }

    [Fact]
    public void ResolveCommandAlphaParseAlphaToken_DelegatesFlowControl()
    {
        var (alpha, delegateParse) = VmSbarinfoCommandAlpha1095Slice.ResolveCommandAlphaParseAlphaToken(0.35f);
        Assert.Equal(0.35f, alpha, precision: 3);
        Assert.True(delegateParse);
    }

    [Fact]
    public void ResolveIfHealthPlayerHealthForCompare_UsesPercentScale()
    {
        Assert.Equal(50, VmSbarinfoCommandIfHealth1095Slice.ResolveIfHealthPlayerHealthForCompare(
            percentage: true,
            health: 25,
            maxHealth: 50));
    }

    [Fact]
    public void ResolveIfHealthFlowTruth_ComparesThreshold()
    {
        Assert.True(VmSbarinfoCommandIfHealth1095Slice.ResolveIfHealthFlowTruth(
            playerHealthForCompare: 40,
            hpAmount: 40));
    }

    [Fact]
    public void ResolveIfInvulnerableFlowTruth_GodmodeCheat()
    {
        Assert.True(VmSbarinfoCommandIfInvulnerable1095Slice.ResolveIfInvulnerableFlowTruth(
            invulnerableActorFlag: false,
            godmodeCheat: true,
            godmode2Cheat: false));
    }

    [Fact]
    public void CreateCommandIfWaterLevelHullHeadDefaults_MarksNegatableFlow()
    {
        var (negatable, threshold) = VmSbarinfoCommandIfWaterLevel1095Slice.CreateCommandIfWaterLevelHullHeadDefaults();
        Assert.True(negatable);
        Assert.Equal(0, threshold);
    }

    [Fact]
    public void ResolveInInventoryAmountAfterOptionalComma_ParsesWhenComma()
    {
        Assert.Equal(5, VmSbarinfoCommandInInventory1095Slice.ResolveInInventoryAmountAfterOptionalComma(
            commaPresent: true,
            parsedAmount: 5,
            priorAmount: 0));
    }
}
