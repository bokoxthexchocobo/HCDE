namespace HCDE.Playsim.Tests;

public class VmGGameDemoPlayback1007SliceTests
{
    [Fact]
    public void DeferredPlayDemoAction()
    {
        Assert.Equal(
            VmGGameDemoPlayback1007Slice.GaLoadGamePlayDemo,
            VmGGameDemoPlayback1007Slice.ResolveDeferredPlayDemoGameAction(
                VmGGameDemoPlayback1007Slice.GaLoadGame));
        Assert.Equal(
            VmGGameDemoPlayback1007Slice.GaPlayDemo,
            VmGGameDemoPlayback1007Slice.ResolveDeferredPlayDemoGameAction(
                VmGGameDemoPlayback1007Slice.GaPlayDemo));
        Assert.Equal(
            VmGGameDemoPlayback1007Slice.PlayDemoRejectReason.NetGame,
            VmGGameDemoPlayback1007Slice.ResolvePlayDemoRejectReason(
                netGame: true,
                demoRecording: false));
        Assert.True(VmGGameDemoPlayback1007Slice.HasDemoFilenameArgument(argc: 2));
    }

    [Fact]
    public void ProcessIffDemoHead()
    {
        Assert.Equal(
            0x2000L + 100 + (100 & 1),
            VmGGameDemoPlayback1007Slice.ComputeZdemFormEndOffset(
                streamDataOffset: 0x2000,
                formLength: 100));
        Assert.Equal(
            VmGGameDemoPlayback1007Slice.DemoIffParseFailure.NotZdemFile,
            VmGGameDemoPlayback1007Slice.ResolveZdemMagicFailure(
                chunkId: 0,
                expectedZdemId: 0x4D45445A));
        Assert.True(VmGGameDemoPlayback1007Slice.IsDemoChunkLayoutMangled(
            nextChunkOffset: 0x3000,
            formEndOffset: 0x2500));
        Assert.True(VmGGameDemoPlayback1007Slice.RequiresNewerEngineForDemo(
            minimumVersionField: VmGGameDemoPlayback1007Slice.DemoGameVersion + 1,
            engineDemoVersion: VmGGameDemoPlayback1007Slice.DemoGameVersion));
        Assert.True(VmGGameDemoPlayback1007Slice.UsesStringMapNameFromDemoHeader(
            VmGGameDemoPlayback1007Slice.LegacyMapNameDemoVersion));
        Assert.True(VmGGameDemoPlayback1007Slice.ShouldClearRngForDemoMapStart("MAP01"));
    }

    [Fact]
    public void DemoHeaderChunks()
    {
        var players = 0;
        Assert.True(VmGGameDemoPlayback1007Slice.ShouldRegisterDemoPlayer(
            ref players,
            alreadyInGame: false));
        Assert.Equal(1, players);
        Assert.True(VmGGameDemoPlayback1007Slice.ActivatesMultiplayerFromNetdemoChunk(
            chunkId: 0x4454454E,
            netdemoChunkId: 0x4454454E));
        Assert.True(VmGGameDemoPlayback1007Slice.IsDemoBodyChunk(
            chunkId: 0x59444F42,
            bodyChunkId: 0x59444F42));
        Assert.Equal(
            VmGGameDemoPlayback1007Slice.DemoIffParseFailure.MissingHeader,
            VmGGameDemoPlayback1007Slice.ResolveMissingDemoHeaderFailure(headerSeen: false));
    }
}
