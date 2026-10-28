namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawInventoryBar1088SliceTests
{
    [Fact]
    public void ComputeLiveInventoryBarCounterSpacing_UsesWidthWhenHorizontal()
    {
        var spacing = VmSbarinfoCommandDrawInventoryBar1088Slice.ComputeLiveInventoryBarCounterSpacing(
            boxDisplayWidth: 30,
            boxDisplayHeight: 40,
            vertical: false,
            VmSbarinfoCommandDrawInventoryBar1087Slice.InventoryBarStyle.Doom);
        Assert.Equal(31, spacing);
    }

    [Fact]
    public void ResolveInventoryBarCounterSlotCoordinates_AdvancesVerticalSlots()
    {
        var (startX, y) = VmSbarinfoCommandDrawInventoryBar1088Slice.ResolveInventoryBarCounterSlotCoordinates(
            counterX: 5,
            counterY: 10,
            slotIndex: 2,
            spacing: 32,
            vertical: true);
        Assert.Equal(5, startX);
        Assert.Equal(74, y);
    }

    [Fact]
    public void ResolveInventoryBarCounterWhenNotZeroFlag_InvertsAlwaysShowCounter()
    {
        Assert.True(VmSbarinfoCommandDrawInventoryBar1088Slice.ResolveInventoryBarCounterWhenNotZeroFlag(
            alwaysShowCounter: false));
    }

    [Fact]
    public void CreateDrawKeyBarCtorDefaults_MatchesNativeInit()
    {
        var d = VmSbarinfoCommandDrawKeyBar1088Slice.CreateDrawKeyBarCtorDefaults();
        Assert.Equal(3, d.Number);
        Assert.Equal(-1, d.IconSize);
    }

    [Fact]
    public void IsKeyBarInventoryItemDrawable_RequiresValidKeyIcon()
    {
        Assert.True(VmSbarinfoCommandDrawKeyBar1088Slice.IsKeyBarInventoryItemDrawable(
            iconValid: true,
            isKeyKind: true));
        Assert.False(VmSbarinfoCommandDrawKeyBar1088Slice.IsKeyBarInventoryItemDrawable(
            iconValid: true,
            isKeyKind: false));
    }

    [Fact]
    public void AdvanceKeyBarSlotOffset_ReversesStrideWhenRequested()
    {
        Assert.Equal(-10, VmSbarinfoCommandDrawKeyBar1088Slice.AdvanceKeyBarSlotOffset(
            priorOffset: 0,
            reverse: true,
            stride: 10));
    }

    [Fact]
    public void ShouldWrapKeyBarRow_DetectsRowBoundary()
    {
        Assert.True(VmSbarinfoCommandDrawKeyBar1088Slice.ShouldWrapKeyBarRow(slotIndex: 5, rowSize: 6));
    }

    [Fact]
    public void AdvanceKeyBarRowOffset_SubtractsWhenReverseRows()
    {
        Assert.Equal(2, VmSbarinfoCommandDrawKeyBar1088Slice.AdvanceKeyBarRowOffset(
            priorRowOffset: 10,
            rowWidth: 8,
            reverseRows: true));
    }

    [Fact]
    public void ResolveKeyBarAutoSlotStride_AddsTwoPixelPadding()
    {
        Assert.Equal(34, VmSbarinfoCommandDrawKeyBar1088Slice.ResolveKeyBarAutoSlotStride(
            textureWidth: 32,
            textureHeight: 16,
            vertical: false));
    }

    [Fact]
    public void ClassifyKeyBarOrientationKeyword_MapsVertical()
    {
        var (ok, vertical, unknown) = VmSbarinfoCommandDrawKeyBar1088Slice.ClassifyKeyBarOrientationKeyword("vertical");
        Assert.True(ok);
        Assert.True(vertical);
        Assert.False(unknown);
    }
}
