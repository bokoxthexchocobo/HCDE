namespace HCDE.Playsim.Tests;

public class VmGLevelInitCtor1014SliceTests
{
    [Fact]
    public void LevelLocalsCtorAndInitReset()
    {
        Assert.True(VmGLevelInitCtor1014Slice.ShouldWirePlayersArrayIndex(0));
        Assert.False(VmGLevelInitCtor1014Slice.ShouldWirePlayersArrayIndex(8));
        Assert.Equal(2, VmGLevelInitCtor1014Slice.CountNonNullDestructorTargets(new object(), new object()));
        Assert.Equal(1, VmGLevelInitCtor1014Slice.CountNonNullDestructorTargets(new object(), null));

        var reset = VmGLevelInitCtor1014Slice.CreateInitScalarReset(
            svGravity: 1f,
            svAirControl: 0.5f,
            globalTeamDamage: 0.25f);
        Assert.Equal(0u, reset.Flags);
        Assert.Equal(1f, reset.MusicVolume);
        Assert.False(reset.FromSnapshot);
        Assert.Equal(35, VmGLevelInitCtor1014Slice.TicRate);
    }

    [Fact]
    public void InitGravityFadeAndCluster()
    {
        Assert.True(VmGLevelInitCtor1014Slice.ShouldSetLevelHasFadeTable(
            fadeto: 0,
            fadeTableName: "FADE001"));
        Assert.Equal(
            VmGLevelInitCtor1014Slice.LevelHasFadeTable,
            VmGLevelInitCtor1014Slice.ApplyLevelHasFadeTableFlag(
                0,
                shouldSet: true));
        Assert.Equal(16, VmGLevelInitCtor1014Slice.ScaleWallLight(8));
        Assert.Equal(0, VmGLevelInitCtor1014Slice.ResolveInitGravity(1, float.MaxValue));
        Assert.Equal(0.75f, VmGLevelInitCtor1014Slice.ResolveInitAirControl(0.5f, 0.75f));
        Assert.Equal(2u, VmGLevelInitCtor1014Slice.ResolveClusterFlags(clusterFound: true, clusterFlags: 2));
        Assert.Equal(70, VmGLevelInitCtor1014Slice.ResolveAirSupplyTicks(2));
    }

    [Fact]
    public void InitGlPropsAndCalcMapName()
    {
        var gl = VmGLevelInitCtor1014Slice.ResolveInitGlProps(
            infoBrightFog: -1,
            infoLightAdditiveSurfaces: 1,
            infoNoTextureFill: 0,
            glBrightFog: true,
            glLightAdditiveSurfaces: false,
            glNoTextureFill: true);
        Assert.True(gl.BrightFog);
        Assert.True(gl.LightAdditiveSurfaces);
        Assert.False(gl.NoTextureFill);

        Assert.Equal("MAP05", VmGLevelInitCtor1014Slice.CalcMapName(
            VmGLevelInitCtor1014Slice.GameInfoMapxx,
            episode: 1,
            level: 5));
        Assert.Equal("E2M3", VmGLevelInitCtor1014Slice.CalcMapName(
            gameInfoFlags: 0,
            episode: 2,
            level: 3));
    }
}
