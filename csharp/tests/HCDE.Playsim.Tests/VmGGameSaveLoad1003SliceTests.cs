namespace HCDE.Playsim.Tests;

public class VmGGameSaveLoad1003SliceTests
{
    [Fact]
    public void LoadGlobalsTail()
    {
        Assert.True(VmGGameSaveLoad1003Slice.ShouldLoadImportantCvarsFromBlob("x=1"));
        Assert.Equal(35, VmGGameSaveLoad1003Slice.ScaleLevelTimeFromSaveTicks(
            levelTime: 70,
            engineTicRate: 35,
            saveTicRate: 70));
    }

    [Fact]
    public void SaveGameGates()
    {
        Assert.Equal(
            VmGGameSaveLoad1003Slice.SaveGameRejectReason.NotUserGame,
            VmGGameSaveLoad1003Slice.ResolveSaveGameRejectReason(
                sendSave: false,
                gameAction: VmGGameSaveLoad1003Slice.GaNothing,
                userGame: false,
                gameState: VmGGameSaveLoad1003Slice.GsLevel,
                health: 100,
                multiplayer: false,
                netGame: false,
                netLimitSaves: false,
                localCanControlSettings: true));

        var quick = VmGGameSaveLoad1003Slice.ResolveSaveGameCommandEffect(
            quickSave: true,
            currentGameAction: VmGGameSaveLoad1003Slice.GaNothing);
        Assert.False(quick.SendSave);
        Assert.Equal(VmGGameSaveLoad1003Slice.GaQuicksave, quick.TraceGameAction);
    }

    [Fact]
    public void AutosaveRotation()
    {
        Assert.Equal(
            VmGGameSaveLoad1003Slice.AutosaveSkipReason.NetGame,
            VmGGameSaveLoad1003Slice.ResolveAutosaveSkipReason(netGame: true));
        Assert.Equal(4, VmGGameSaveLoad1003Slice.ResolveAutosaveSlotCount(autosaveCount: 4));
        Assert.Equal(2, VmGGameSaveLoad1003Slice.ComputeInitialNextAutosaveIndex(
            autosaveNum: 1,
            slotCount: 4));
        Assert.True(VmGGameSaveLoad1003Slice.ShouldRotateAutosaveHint(levelFlags2: 0));
        Assert.Equal(0u, VmGGameSaveLoad1003Slice.ClearLevelNoAutosaveHint(
            VmGGameSaveLoad1003Slice.Level2NoAutosaveHint));
    }
}
