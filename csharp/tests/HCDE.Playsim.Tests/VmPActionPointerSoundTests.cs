namespace HCDE.Playsim.Tests;

public class VmPActionPointerSoundTests
{
    [Fact]
    public void RearrangePointers_TargetFromMaster()
    {
        var master = new object();
        var self = new VmPActionPointerSound.PointerActor { Master = master, Target = new() };
        VmPActionPointerSound.RearrangePointers(new(), self,
            VmPActionPointerSound.AaptrMaster, 0, 0, 0);
        Assert.Same(master, self.Target);
    }

    [Fact]
    public void RearrangePointers_NullClearsTarget()
    {
        var self = new VmPActionPointerSound.PointerActor { Target = new() };
        VmPActionPointerSound.RearrangePointers(new(), self,
            VmPActionPointerSound.AaptrNull, 0, 0, 0);
        Assert.Null(self.Target);
    }

    [Fact]
    public void TransferPointer_SelfNullWhenRecipientMatches()
    {
        var self = new VmPActionPointerSound.PointerActor();
        self.Target = self;
        var ctx = new VmPActionPointerSound.PointerContext
        {
            CopyAaptr = (origin, sel) => origin switch
            {
                VmPActionPointerSound.PointerActor a when sel == VmPActionPointerSound.AaptrTarget => a.Target,
                _ => origin,
            },
        };
        VmPActionPointerSound.TransferPointer(ctx, self, 0, 0,
            VmPActionPointerSound.AaptrTarget, VmPActionPointerSound.AaptrTarget, 0);
        Assert.Null(self.Target);
    }

    [Fact]
    public void TransferPointer_NullRecipient_NoOp()
    {
        var self = new VmPActionPointerSound.PointerActor { Target = new() };
        var ctx = new VmPActionPointerSound.PointerContext { CopyAaptr = (_, _) => null };
        VmPActionPointerSound.TransferPointer(ctx, self, 0, 0, 0, 0, 0);
        Assert.NotNull(self.Target);
    }

    [Fact]
    public void CopyFriendlinessFromPointer_SkipsPlayers()
    {
        var copied = false;
        var ctx = new VmPActionPointerSound.PointerContext
        {
            CopyAaptr = (_, _) => new object(),
            CopyFriendliness = (_, _, _, _) => copied = true,
        };
        VmPActionPointerSound.CopyFriendlinessFromPointer(ctx,
            new VmPActionPointerSound.PointerActor { HasPlayer = true }, 0);
        Assert.False(copied);
    }

    [Fact]
    public void StopSoundEx_ValidChannelStops()
    {
        var stopped = false;
        var ctx = new VmPActionPointerSound.SoundContext { StopSound = (_, _) => stopped = true };
        VmPActionPointerSound.StopSoundEx(ctx, new(), VmPActionPointerSound.NameAuto + 1);
        Assert.True(stopped);
    }

    [Fact]
    public void PlaySoundEx_AttenuationMapping()
    {
        Assert.Equal(3f, VmPActionPointerSound.AttenuationFromRaw(-1));
        Assert.Equal(0f, VmPActionPointerSound.AttenuationFromRaw(2));
        Assert.Equal(1f, VmPActionPointerSound.AttenuationFromRaw(0));
    }

    [Fact]
    public void PlaySoundEx_LoopingSkipsWhenAlreadyPlaying()
    {
        var plays = 0;
        var ctx = new VmPActionPointerSound.SoundContext
        {
            IsPlaying = (_, _, _) => true,
            PlaySound = (_, _, _, _, _, _) => plays++,
        };
        VmPActionPointerSound.PlaySoundEx(ctx, new(), "x", VmPActionPointerSound.NameAuto, looping: true, 0);
        Assert.Equal(0, plays);
    }
}
