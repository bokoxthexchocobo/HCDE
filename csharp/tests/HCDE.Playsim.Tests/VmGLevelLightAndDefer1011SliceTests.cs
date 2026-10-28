namespace HCDE.Playsim.Tests;

public class VmGLevelLightAndDefer1011SliceTests
{
    [Fact]
    public void RealLightMode()
    {
        Assert.Equal(
            VmGLevelLightAndDefer1011Slice.LightModeDoomSoftware,
            VmGLevelLightAndDefer1011Slice.ResolveRealLightMode(
                levelInfoLightMode: VmGLevelLightAndDefer1011Slice.LightModeNotSet,
                glMapLightMode: VmGLevelLightAndDefer1011Slice.GlMapLightModeInactive,
                glLightMode: VmGLevelLightAndDefer1011Slice.GlLightModeVanillaAccurate,
                for3d: true));
        Assert.Equal(
            VmGLevelLightAndDefer1011Slice.LightModeDoom,
            VmGLevelLightAndDefer1011Slice.ResolveRealLightMode(
                levelInfoLightMode: VmGLevelLightAndDefer1011Slice.LightModeNotSet,
                glMapLightMode: VmGLevelLightAndDefer1011Slice.GlMapLightModeInactive,
                glLightMode: 0,
                for3d: false));
        Assert.Equal(3, VmGLevelLightAndDefer1011Slice.ResolveRealLightMode(
            levelInfoLightMode: VmGLevelLightAndDefer1011Slice.LightModeNotSet,
            glMapLightMode: 3,
            glLightMode: 0,
            for3d: true));
    }

    [Fact]
    public void CutsceneCreation()
    {
        Assert.Equal(
            VmGLevelLightAndDefer1011Slice.CutsceneContentKind.None,
            VmGLevelLightAndDefer1011Slice.ClassifyCutsceneDefinition("none", video: null));
        Assert.Equal(
            VmGLevelLightAndDefer1011Slice.CutsceneContentKind.Video,
            VmGLevelLightAndDefer1011Slice.ClassifyCutsceneDefinition(function: null, video: "intro.vp9"));
        Assert.True(VmGLevelLightAndDefer1011Slice.ValidateCutsceneFunctionArity(
            argumentCount: 2,
            runnerOnlyArity: 1,
            runnerAndMapArity: 2));
    }

    [Fact]
    public void DeferredNewGame()
    {
        Assert.True(VmGLevelLightAndDefer1011Slice.ShouldAssignStartupPlayerClass(
            hasPlayerClass: true,
            startupClass: "Fighter"));
        Assert.True(VmGLevelLightAndDefer1011Slice.ShouldRunEpisodeIntro(
            introDefined: true,
            cutsceneCreated: true));
        Assert.True(VmGLevelLightAndDefer1011Slice.ShouldAbortEpisodeIntroOnCutsceneFailure(
            introDefined: true,
            cutsceneCreated: false));
        Assert.True(VmGLevelLightAndDefer1011Slice.ShouldClearCutsceneCompletionAfterInvalidScreenJob(
            screenJobValid: false,
            hadCompletionCallback: true));
        Assert.Equal(2, VmGLevelLightAndDefer1011Slice.ResolveFinishStateForNewGameStartup());
        Assert.Equal(3, VmGLevelLightAndDefer1011Slice.ResolveDeferredNewGameTraceAction());
    }
}
