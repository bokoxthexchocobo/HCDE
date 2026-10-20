namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnMapThingComposeTests
{
    [Fact]
    public void Gate_RequiresVirtualTrue() =>
        Assert.False(VmPMobjSpawnMapThingCompose.ShouldSpawnVirtual(false));

    [Fact]
    public void Z_CeilingWinsOverFloat() =>
        Assert.Equal(VmPMobjSpawnMapThingCompose.Onceilingz,
            VmPMobjSpawnMapThingCompose.ResolveMapthingSpawnZ(
                VmPMobjSpawnMapThingCompose.MfSpawnceiling, VmPMobjSpawnMapThingCompose.Mf2Spawnfloat));

    [Fact]
    public void Conversation_NeedsValidRoot() =>
        Assert.True(VmPMobjSpawnMapThingCompose.ApplyConversation(3, 1));

    [Fact]
    public void Udmf_AlphaWhenNonNegative() =>
        Assert.True(VmPMobjSpawnMapThingCompose.ApplyAlpha(0.5));

    [Fact]
    public void Light_OverrideClearsFlag() =>
        Assert.Equal(0, VmPMobjSpawnMapThingCompose.ApplyLightFlagOverride(
            true, false, VmPMobjSpawnMapThingCompose.LfNoshadowmap, VmPMobjSpawnMapThingCompose.LfNoshadowmap));

    [Fact]
    public void Health_PositiveScales() =>
        Assert.Equal(200, VmPMobjSpawnMapThingCompose.PositiveScaledHealth(100, 2));

    [Fact]
    public void Health_DieFromSpawnWhenZero() =>
        Assert.True(VmPMobjSpawnMapThingCompose.ScheduleDieFromSpawn(0, true));

    [Fact]
    public void Spawn_FloatbobCompatRecordsSpecial() =>
        Assert.True(VmPMobjSpawnMapThingCompose.RecordFloatbobSpecial(
            VmPMobjSpawnMapThingCompose.Onfloorz, true, true));
}
