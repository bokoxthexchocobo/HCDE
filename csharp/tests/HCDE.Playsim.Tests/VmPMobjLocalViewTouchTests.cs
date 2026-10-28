namespace HCDE.Playsim.Tests;

public class VmPMobjLocalViewTouchTests
{
    [Fact]
    public void CheckLocalView_CameraMatch()
    {
        var self = new object();
        var q = new VmPMobjLocalViewTouch.LocalViewQuery { ConsoleCamera = self };
        Assert.True(VmPMobjLocalViewTouch.CheckLocalView(q, self));
    }

    [Fact]
    public void ShouldRenderLocally_InvertsFlag()
    {
        Assert.False(VmPMobjLocalViewTouch.ShouldRenderLocally(true));
        Assert.True(VmPMobjLocalViewTouch.ShouldRenderLocally(false));
    }

    [Fact]
    public void IsInsideVisibleAngles_SkipsWhenUnmasked()
    {
        var q = new VmPMobjLocalViewTouch.VisibleAnglesQuery { RenderFlags = 0 };
        Assert.True(VmPMobjLocalViewTouch.IsInsideVisibleAngles(q));
    }

    [Fact]
    public void IsVisibleToPlayer_TeamGate()
    {
        var q = new VmPMobjLocalViewTouch.VisibilityQuery
        {
            VisibleToTeam = 2,
            Teamplay = true,
            ConsoleTeam = 0,
        };
        Assert.False(VmPMobjLocalViewTouch.IsVisibleToPlayer(q));
    }

    [Fact]
    public void ConversationAnimation_SetsStateWhenFound()
    {
        object? set = null;
        var ctx = new VmPMobjLocalViewTouch.ConversationContext
        {
            FindState = n => n == 1 ? new object() : null,
            SetState = s => set = s,
        };
        VmPMobjLocalViewTouch.ConversationAnimation(ctx, 1);
        Assert.NotNull(set);
    }

    [Fact]
    public void CallTouch_InvokesVirtual()
    {
        var called = false;
        VmPMobjLocalViewTouch.CallTouch(true, () => called = true);
        Assert.True(called);
    }

    [Fact]
    public void Massacre_ReturnsTrueWhenKilled()
    {
        var state = new VmPMobjLocalViewTouch.MassacreState
        {
            Health = 50,
            ApplyMassacreDamage = () => 0,
        };
        Assert.True(VmPMobjLocalViewTouch.Massacre(state));
        Assert.Equal(0, state.Health);
    }

    [Fact]
    public void Massacre_RestoresFlagsWhenSurvives()
    {
        var state = new VmPMobjLocalViewTouch.MassacreState
        {
            Health = 50,
            Flags = 2,
            Flags2 = 8,
            ApplyMassacreDamage = () => 50,
        };
        Assert.False(VmPMobjLocalViewTouch.Massacre(state));
        Assert.Equal(2, state.Flags);
        Assert.Equal(8, state.Flags2);
    }
}
