namespace HCDE.Gamedata.Tests;

public class DehackedPatchTests
{
    [Fact]
    public void Apply_PatchesThingWidthHeightSpeedAndSounds()
    {
        const string text = """
            Thing 5 (Imp)
            Hit points = 80
            Width = 1048576
            Height = 3670016
            Speed = 8
            Alert sound = 34
            Death sound = 32
            Initial frame = 174
            ID # = 3001
            """;

        var result = DehackedPatch.Apply(text);
        var imp = result.Actors.Single(actor => actor.Index == 5);
        Assert.Equal(80, imp.Health);
        Assert.Equal(16.0, imp.Radius);
        Assert.Equal(56.0, imp.Height);
        Assert.Equal(8, imp.Speed);
        Assert.Equal(34, imp.SeeSound);
        Assert.Equal(32, imp.DeathSound);
        Assert.Equal(174, imp.SpawnState);
        Assert.Equal(3001, imp.DoomEdNum);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Apply_PlayerIgnoresAttackFrames()
    {
        const string text = """
            Thing 1
            Close attack frame = 10
            Far attack frame = 11
            Initial frame = 1
            """;

        var result = DehackedPatch.Apply(text);
        var player = result.Actors.Single(actor => actor.Index == 1);
        Assert.Equal(0, player.MeleeState);
        Assert.Equal(0, player.MissileState);
        Assert.Equal(1, player.SpawnState);
    }

    [Fact]
    public void Apply_FrameKeepsSuperShotgunFlashTicsAndFullBright()
    {
        const string text = """
            Frame 47
            Next frame = 48
            Sprite subnumber = 32769

            Frame 99
            Sprite subnumber = 64
            Duration = 4
            """;

        var result = DehackedPatch.Apply(text);
        var flash = result.States.Single(state => state.Index == 47);
        Assert.Equal(5, flash.Tics);
        Assert.Equal(48, flash.NextState);
        Assert.Equal(1, flash.Frame);
        Assert.True(flash.FullBright);
        Assert.True(flash.Patched);
        Assert.Contains(result.Errors, error => error.Contains("Frame 99", StringComparison.Ordinal));
        Assert.Equal(0, result.States.Single(state => state.Index == 99).Frame);
    }

    [Fact]
    public void Apply_SoundBlockIsIgnoredAndBexSoundNamesApply()
    {
        const string text = """
            Sound 1
            Offset = 8

            [SOUNDS]
            1 = DSPISTOL
            """;

        var result = DehackedPatch.Apply(text);
        Assert.Equal("DSPISTOL", result.Sounds.Single(sound => sound.Index == 1).Name);
    }

    [Fact]
    public void Apply_ReportsThingOutOfRange()
    {
        var result = DehackedPatch.Apply("Thing 99\nHit points = 1\n");
        Assert.Contains(result.Errors, error => error.Contains("Thing 99", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Actors, actor => actor.Index == 99);
    }
}
