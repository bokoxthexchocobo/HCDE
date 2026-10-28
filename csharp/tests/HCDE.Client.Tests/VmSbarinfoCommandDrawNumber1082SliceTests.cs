namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawNumber1082SliceTests
{
    [Fact]
    public void ResolveParameterizedAmmoCapacityMax_UsesDefaultWhenMissing()
    {
        Assert.Equal(
            200,
            VmSbarinfoCommandDrawNumber1082Slice.ResolveParameterizedAmmoCapacityMax(
                itemFound: false,
                itemMaxAmount: 50,
                defaultTypeMaxAmount: 200));
    }

    [Fact]
    public void ResolveDrawNumberPrimaryLevelStat_MapsKillsAndSecrets()
    {
        Assert.Equal(
            12,
            VmSbarinfoCommandDrawNumber1082Slice.ResolveDrawNumberPrimaryLevelStat(
                VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Kills,
                fragCount: 0,
                killedMonsters: 12,
                totalMonsters: 0,
                foundItems: 0,
                totalItems: 0,
                foundSecrets: 0,
                totalSecrets: 0,
                score: 0));
        Assert.Equal(
            3,
            VmSbarinfoCommandDrawNumber1082Slice.ResolveDrawNumberPrimaryLevelStat(
                VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.Secrets,
                0,
                0,
                0,
                0,
                0,
                foundSecrets: 3,
                totalSecrets: 9,
                score: 0));
    }

    [Fact]
    public void ComputeDrawNumberArmorClassOrSavePercent_DividesOnlyArmorClass()
    {
        var armorClass = VmSbarinfoCommandDrawNumber1082Slice.ComputeDrawNumberArmorClassOrSavePercent(
            hexenSlotSum: 25,
            basicArmorActive: false,
            basicSavePercent: 0,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.ArmorClass);
        var savePercent = VmSbarinfoCommandDrawNumber1082Slice.ComputeDrawNumberArmorClassOrSavePercent(
            hexenSlotSum: 25,
            basicArmorActive: false,
            basicSavePercent: 0,
            VmSbarinfoCommandDrawNumber1080Slice.DrawNumberValueKind.SavePercent);
        Assert.Equal(5, armorClass);
        Assert.Equal(25, savePercent);
    }

    [Fact]
    public void ConvertPowerupEffectTicsToDisplaySeconds_MatchesNativeFormula()
    {
        Assert.Equal(0, VmSbarinfoCommandDrawNumber1082Slice.ConvertPowerupEffectTicsToDisplaySeconds(-1, 35));
        Assert.Equal(2, VmSbarinfoCommandDrawNumber1082Slice.ConvertPowerupEffectTicsToDisplaySeconds(35, 35));
    }

    [Fact]
    public void ResolveDrawNumberAirTimeTickNum_SwitchesOnWaterLevel()
    {
        Assert.Equal(
            10,
            VmSbarinfoCommandDrawNumber1082Slice.ResolveDrawNumberAirTimeTickNum(
                waterLevel: 0,
                airSupply: 350,
                mapTime: 0,
                airFinished: 0,
                ticRate: 35));
        Assert.Equal(
            2,
            VmSbarinfoCommandDrawNumber1082Slice.ResolveDrawNumberAirTimeTickNum(
                waterLevel: 3,
                airSupply: 0,
                mapTime: 100,
                airFinished: 170,
                ticRate: 35));
    }

    [Fact]
    public void ResolveSelectedInventoryTickNum_LeavesPriorWhenMissing()
    {
        var (updated, num) = VmSbarinfoCommandDrawNumber1082Slice.ResolveSelectedInventoryTickNum(
            hasSelectedInventory: false,
            selectedAmount: 99,
            priorNum: 7);
        Assert.False(updated);
        Assert.Equal(7, num);
    }

    [Fact]
    public void FoldDrawNumberKeyCount_IncrementsForKeys()
    {
        Assert.Equal(2, VmSbarinfoCommandDrawNumber1082Slice.FoldDrawNumberKeyCount(1, inventoryItemIsKey: true));
        Assert.Equal(1, VmSbarinfoCommandDrawNumber1082Slice.FoldDrawNumberKeyCount(1, inventoryItemIsKey: false));
    }

    [Fact]
    public void ResolveDrawNumberIntCVarTickNum_FallsBackToZero()
    {
        Assert.Equal(
            0,
            VmSbarinfoCommandDrawNumber1082Slice.ResolveDrawNumberIntCVarTickNum(
                cvarExists: true,
                VmSbarinfoCommandDrawNumber1080Slice.IntCVarAcceptedType.Rejected,
                intRepresentation: 42));
    }

    [Fact]
    public void StepDrawNumberInterpolationTowardTarget_ClampsStep()
    {
        Assert.Equal(
            96,
            VmSbarinfoCommandDrawNumber1082Slice.StepDrawNumberInterpolationTowardTarget(
                drawValue: 100,
                targetNum: 50,
                interpolationSpeed: 8));
    }

    [Fact]
    public void ResolveDrawNumberDisplayValueAfterTick_SnapsWhenInterpolationDisabled()
    {
        Assert.Equal(
            50,
            VmSbarinfoCommandDrawNumber1082Slice.ResolveDrawNumberDisplayValueAfterTick(
                drawValue: 100,
                targetNum: 50,
                interpolationSpeed: 0,
                hudChanged: true,
                levelTime: 2));
    }
}
