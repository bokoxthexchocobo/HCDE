namespace HCDE.Playsim.Tests;

public class VmSoundEngineLimitStopTests
{
    [Fact]
    public void CheckSoundLimit_HitsCeiling()
    {
        var ctx = new VmSoundEngineLimitStop.CheckSoundLimitContext
        {
            NearLimit = 2,
            LimitRange = 256,
            SfxId = 1,
            Attenuation = 1,
            Channels = new[]
            {
                new VmSoundEngineLimitStop.SoundLimitChannel(0, 1, 0, 0, null, 1),
                new VmSoundEngineLimitStop.SoundLimitChannel(0, 1, 0, 0, null, 1),
            },
            CalcPosVel = _ => (0, 0, 0),
        };
        Assert.True(VmSoundEngineLimitStop.CheckSoundLimit(ctx));
    }

    [Fact]
    public void IsRestartingSameSource_AllowsReplay()
    {
        var actor = new object();
        var ctx = new VmSoundEngineLimitStop.CheckSoundLimitContext
        {
            NearLimit = 2,
            SfxId = 1,
            Actor = actor,
            Channel = 0,
            Channels = new[]
            {
                new VmSoundEngineLimitStop.SoundLimitChannel(0, 1, 0, 1, actor, 1),
            },
        };
        Assert.False(VmSoundEngineLimitStop.CheckSoundLimit(ctx));
    }

    [Fact]
    public void ShouldStopUnpositioned_MatchesAutoChannel()
    {
        var stopped = new List<int>();
        VmSoundEngineLimitStop.StopSound(
            VmSoundEngineLimitStop.ChanAuto,
            VmSoundEngineLimitStop.InvalidSound,
            new[] { (0, 3, 0) },
            i => stopped.Add(i));
        Assert.Equal(new[] { 0 }, stopped);
    }

    [Fact]
    public void ShouldStopActorSound_ByOrgId()
    {
        var actor = new object();
        var stopped = new List<int>();
        VmSoundEngineLimitStop.StopActorSound(
            1, actor, 0, 9,
            new[] { (1, actor, 9, 0) },
            i => stopped.Add(i));
        Assert.Single(stopped);
    }

    [Fact]
    public void ShouldStopActorChannel_RangeOrAll()
    {
        var actor = new object();
        var stopped = new List<int>();
        VmSoundEngineLimitStop.StopActorSounds(1, actor, 2, 4, new[] { (1, actor, 3) }, i => stopped.Add(i));
        Assert.Single(stopped);
    }

    [Fact]
    public void ShouldRelinkSource_MatchesFromPointer()
    {
        var from = new object();
        var to = new object();
        var channels = new List<(int, object?, int, double, double, double)>
        {
            (1, from, 0, 0, 0, 0),
        };
        VmSoundEngineLimitStop.RelinkSound(1, from, to, false, 0, 0, 0, channels, _ => { });
        Assert.Equal(to, channels[0].Item2);
    }
}
