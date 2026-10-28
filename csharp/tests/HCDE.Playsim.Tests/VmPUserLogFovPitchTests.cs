using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmPUserLogFovPitchTests
{
    [Fact]
    public void SetLogNumber_UsesStringTableLabel()
    {
        var text = "";
        var ctx = new VmPUserLogFovPitch.LogNumberContext
        {
            CheckString = key => key == "TXT_LOGTEXT5" ? "hello" : null,
            SetLogText = t => text = t,
        };
        VmPUserLogFovPitch.SetLogNumber(ctx, 5);
        Assert.Equal("$TXT_LOGTEXT5", text);
    }

    [Fact]
    public void SetSubtitle_SkipsShareware()
    {
        var player = new VmPUserLogFovPitch.PlayerLogState();
        VmPUserLogFovPitch.SetSubtitle(
            player,
            new VmPUserLogFovPitch.SubtitleContext { GameFlags = VmPUserLogFovPitch.GiShareware },
            1,
            VmPUserLogFovPitch.NoSound);
        Assert.Equal("", player.SubtitleText);
    }

    [Fact]
    public void PlanSetFov_ClampAndRespectDmflags()
    {
        var blocked = VmPUserLogFovPitch.PlanSetFov(true, VmPUserLogFovPitch.DfNoFov, false, 200);
        Assert.False(blocked.WriteNetwork);
        Assert.True(blocked.PrintDisabled);

        var allowed = VmPUserLogFovPitch.PlanSetFov(true, 0, false, 200);
        Assert.True(allowed.WriteNetwork);
        Assert.Equal(179f, allowed.ClampedFov);
    }

    [Fact]
    public void EnumColorSets_SortsAndSkipsDeleted()
    {
        var marine = new object();
        var entries = new List<VmPUserLogFovPitch.ColorSetEntry>
        {
            new(marine, 2, true),
            new(marine, 1, false),
            new(marine, 3, true),
        };
        VmPUserLogFovPitch.IsAncestorOfFunc = (a, b) => ReferenceEquals(a, b);
        var outList = new List<int>();
        VmPUserLogFovPitch.EnumColorSets(entries, marine, outList);
        Assert.Equal(new[] { 2, 3 }, outList);
    }

    [Fact]
    public void GetPainFlash_MatchesTypeAndClass()
    {
        var cls = new object();
        var flashes = new List<VmPUserLogFovPitch.PainFlashEntry>
        {
            new(cls, 7, 0xff0000),
        };
        VmPUserLogFovPitch.IsAncestorOfFunc = ReferenceEquals;
        Assert.Equal(0xff0000, VmPUserLogFovPitch.GetPainFlash(flashes, cls, 7));
    }

    [Fact]
    public void SendPitchLimits_AppliesLocalClamp()
    {
        var player = new VmPUserLogFovPitch.PlayerLogState();
        var plan = VmPUserLogFovPitch.PlanSendPitchLimits(true, false, 40, true);
        VmPUserLogFovPitch.ApplyPitchLimits(player, plan);
        Assert.Equal(-32, player.MinPitch);
        Assert.Equal(40, player.MaxPitch);
    }
}
