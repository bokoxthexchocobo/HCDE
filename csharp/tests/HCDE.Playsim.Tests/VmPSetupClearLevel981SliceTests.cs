namespace HCDE.Playsim.Tests;

public class VmPSetupClearLevel981SliceTests
{
    [Fact]
    public void UdmfKeysAndDialogueDrain()
    {
        Assert.True(VmPSetupClearLevel981Slice.UdmfKeySlotInRange(2));
        Assert.True(VmPSetupClearLevel981Slice.ShouldDrainStrifeDialogueQueue(
            nodePopped: true,
            queueMayHoldMore: false));

        var pops = 0;
        Assert.True(VmPSetupClearLevel981Slice.TryPopStrifeDialogue(
            tryPop: () => pops++ < 2,
            _ => { },
            out var freed));
        Assert.Equal(2, freed);
    }

    [Fact]
    public void PolyblockChainDeletion()
    {
        var chain = new object[] { new object(), new object() };
        var index = 0;
        var deleted = VmPSetupClearLevel981Slice.DeletePolyblockChain(
            chain[0],
            _ =>
            {
                index++;
                return index < chain.Length ? chain[index] : null;
            });
        Assert.Equal(2, deleted);
    }

    [Fact]
    public void ClearLevelDataTailPredicates()
    {
        Assert.True(VmPSetupClearLevel981Slice.ShouldDeleteSubsectorBsp(new object(), subsectorAllocated: true));
        Assert.True(VmPSetupClearLevel981Slice.ShouldDestroyAutomap(new object(), levelHadAutomap: true));
        Assert.True(VmPSetupClearLevel981Slice.ShouldDeleteSpatialAccelerators(new object(), null));
        Assert.True(VmPSetupClearLevel981Slice.ShouldClearNetworkSectorBaselines(replicationEnabled: true));
    }

    [Fact]
    public void FreeLevelDataAndSetupHead()
    {
        Assert.True(VmPSetupClearLevel981Slice.ShouldIterateFreeLevelData(
            levelsExist: true,
            requestFullGc: true));
        Assert.True(VmPSetupClearLevel981Slice.SetupLevelResetsScoreCounts(
            savegameRestore: false,
            levelLoaded: true));
        Assert.True(VmPSetupClearLevel981Slice.MaptypeAwaitingMapinfoClassification(
            VmPSetupClearLevel981Slice.MaptypeUnknown,
            VmPSetupClearLevel981Slice.MaptypeUnknown));
        Assert.Equal(VmPSetupClearLevel981Slice.ViewzUnset,
            VmPSetupClearLevel981Slice.SelectInitialViewHeight(hasConsolePlayer: true));
    }
}
