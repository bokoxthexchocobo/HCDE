namespace HCDE.Playsim.Tests;

public class VmPLnspecStrifeLineSpecialsTests
{
    [Fact]
    public void SetConversation_RejectsUnknownDialog()
    {
        Assert.True(VmPLnspecStrifeLineSpecials.SetConversationRejectUnknown(5, -1));
    }

    [Fact]
    public void StartConversation_RejectsDeadPlayer()
    {
        Assert.False(VmPLnspecStrifeLineSpecials.StartConversationNeedsLivingPlayer(new object(), new object(), new object(), 0));
    }
}

public class VmPLnspecGlassBreakTests
{
    [Fact]
    public void GlassBreak_SpawnsJunkWhenArgZero()
    {
        Assert.True(VmPLnspecGlassBreak.ShouldSpawnJunk(0));
    }

    [Fact]
    public void GlassBreak_CombinesSideSwitchResults()
    {
        Assert.True(VmPLnspecGlassBreak.CombineSwitchResults(false, true));
    }

    [Fact]
    public void GlassBreak_ClearsBlockingFlags()
    {
        Assert.Equal(0, VmPLnspecGlassBreak.GlassBreakClearFlags(3));
    }

    [Fact]
    public void ClearForceField_ReturnsTrueWhenSectorFound()
    {
        Assert.True(VmPLnspecGlassBreak.ClearForceFieldAnySector(true));
    }
}
