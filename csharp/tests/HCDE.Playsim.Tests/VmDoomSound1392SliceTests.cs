using S = HCDE.Playsim.VmDoomSound1392Slice;

namespace HCDE.Playsim.Tests;

public class VmDoomSound1392SliceTests
{
    [Fact]
    public void AddSoundLumpZerosUserDataAndLimitDropsNonActors()
    {
        Assert.Equal("SoundEngine", S.BaseClass);
        Assert.Equal(2, S.SourceActor);
        Assert.Equal(1, S.SourceNone);
        Assert.Equal(-1, S.SourceAny);
        Assert.Equal(0, S.SourceUnattached);
        Assert.Equal(3, S.SourceSector);
        Assert.Equal(4, S.SourcePolyobj);
        Assert.Equal("haptics_do_action", S.HapticsDoAction);
        Assert.Equal("void S_AddLocalSndInfo(int lump)", S.AddLocalSndInfo);
        Assert.Equal("", S.LastLocalSndInfo);
        Assert.Equal("", S.LastLocalSndSeq);
        S.LastLocalSndInfo = "SNDINFO";
        S.LastLocalSndSeq = "SNDSEQ";
        Assert.Equal("SNDINFO", S.LastLocalSndInfo);
        Assert.Equal("SNDSEQ", S.LastLocalSndSeq);
        S.LastLocalSndInfo = "";
        S.LastLocalSndSeq = "";

        Assert.Equal("CalcPosVel", S.Method("CalcPosVel").Name);
        Assert.Contains("override", S.Method("CalcPosVel").Signature);
        Assert.Equal("private", S.Method("CalcPosVel").Access);
        Assert.Equal("private", S.Method("AddSoundLump").Access);
        Assert.Contains("resid = -1", S.Method("AddSoundLump").Signature);
        Assert.Contains("nearlimit = 2", S.Method("AddSoundLump").Signature);
        Assert.Equal("public", S.Method("DoomSoundEngine").Access);
        Assert.Equal("DoomSoundEngine() = default", S.Method("DoomSoundEngine").Signature);
        Assert.Equal("void NoiseDebug(void)", S.Method("NoiseDebug").Signature);
        Assert.Equal("void PrintSoundList()", S.Method("PrintSoundList").Signature);
        Assert.Equal(12, S.Methods.Length);

        S.LumpForward? seen = null;
        var sfx = new List<S.SfxEntry> { new(), new(), new() };
        sfx[1].UserData.Add(4);
        sfx[1].UserData.Add(5);
        int index = S.AddSoundLump("DSPISTOL", 9, 3, call =>
        {
            seen = call;
            return 1;
        }, sfx);
        Assert.Equal(1, index);
        Assert.Equal("DSPISTOL", seen!.Value.LogicalName);
        Assert.Equal(9, seen.Value.Lump);
        Assert.Equal(3, seen.Value.CurrentPitchMask);
        Assert.Equal(-1, seen.Value.ResId);
        Assert.Equal(2, seen.Value.NearLimit);
        Assert.Equal(0, Assert.Single(sfx[1].UserData));
        Assert.Empty(sfx[0].UserData);
        Assert.Empty(sfx[2].UserData);

        index = S.AddSoundLump("empty", 1, 0, _ => 0, sfx, resId: 40, nearLimit: 7);
        Assert.Equal(0, index);
        Assert.Equal(0, Assert.Single(sfx[0].UserData));

        Assert.Throws<ArgumentOutOfRangeException>(() => S.AddSoundLump("bad", 1, 0, _ => 9, sfx));
        Assert.Throws<ArgumentNullException>(() => S.AddSoundLump("x", 1, 0, null!, sfx));

        var actor = new object();
        S.LimitForward? limited = null;
        bool kept = S.CheckSoundLimit("sfx", "pos", 2, 4.5f, S.SourceActor, actor, 4, 1f, call =>
        {
            limited = call;
            return true;
        });
        Assert.True(kept);
        Assert.Same(actor, limited!.Value.Actor);
        Assert.Equal(S.SourceActor, limited.Value.SourceType);
        Assert.Equal(2, limited.Value.NearLimit);
        Assert.Equal(4.5f, limited.Value.LimitRange);
        Assert.Equal(4, limited.Value.Channel);
        Assert.Equal(1f, limited.Value.Attenuation);
        Assert.Equal("sfx", limited.Value.Sfx);
        Assert.Equal("pos", limited.Value.Pos);

        bool dropped = S.CheckSoundLimit("sfx", "pos", 1, 0f, S.SourceSector, actor, 0, 0f, call =>
        {
            limited = call;
            return false;
        });
        Assert.False(dropped);
        Assert.Null(limited!.Value.Actor);
        Assert.Equal(S.SourceSector, limited.Value.SourceType);
        Assert.Same(actor, actor);

        S.CheckSoundLimit(null, null, 0, 0f, S.SourceNone, null, 0, 0f, call =>
        {
            limited = call;
            return true;
        });
        Assert.Null(limited!.Value.Actor);
        Assert.Equal(S.SourceNone, limited.Value.SourceType);
    }

    [Fact]
    public void LookupMusicResolvesStarCdDehackedAndAliases()
    {
        S.CdAudioWarned = false;
        var world = new S.MusicWorld
        {
            GameState = S.GsLevel,
            PrimaryLevel = new S.Level { Music = "D_E1M1", MusicOrder = 3 },
        };
        world.Strings["e1m1"] = "E1M1";
        world.Strings["bar"] = "BAR";
        world.Strings["none"] = "SHOULD_NOT_RESOLVE";
        world.Aliases["d_e1m1"] = new S.Alias(4, "d_runnin");
        world.Aliases["d_runnin"] = new S.Alias(8, "d_runnin");
        world.Aliases["silence"] = new S.Alias(S.NameNone, "None");

        var prints = new List<string>();
        var traces = new List<S.TraceMark>();
        int order = 11;
        Assert.Equal("", S.LookupMusic(null, ref order, world, prints, traces));
        Assert.Equal(11, order);
        Assert.Empty(traces);
        Assert.Equal("", S.LookupMusic("", ref order, world, prints, traces));
        Assert.Empty(traces);

        string star = "*";
        Assert.Equal("d_runnin", S.LookupMusic(star, ref order, world, prints, traces));
        Assert.Equal("*", star);
        Assert.Equal(3, order);
        Assert.Equal("music", traces[0].Channel);
        Assert.Equal("LookupMusic in='*' out='d_runnin' order=3", traces[0].Text);
        traces.Clear();

        world.GameState = S.GsTitleLevel;
        world.PrimaryLevel.Music = "D_TITLE";
        world.PrimaryLevel.MusicOrder = 0;
        order = 5;
        Assert.Equal("D_TITLE", S.LookupMusic("*", ref order, world, prints, traces));
        Assert.Equal(0, order);
        Assert.Equal("LookupMusic in='*' out='D_TITLE' order=0", traces[0].Text);
        traces.Clear();

        world.GameState = S.GsIntermission;
        order = 6;
        Assert.Equal("", S.LookupMusic("*", ref order, world, prints, traces));
        Assert.Equal(6, order);
        Assert.Empty(traces);
        Assert.Empty(prints);

        world.GameState = S.GsLevel;
        world.PrimaryLevel.Music = "";
        world.PrimaryLevel.MusicOrder = 9;
        order = 2;
        Assert.Equal("", S.LookupMusic("*", ref order, world, prints, traces));
        Assert.Equal(9, order);
        Assert.Empty(traces);

        world.PrimaryLevel = null;
        Assert.Throws<NullReferenceException>(() => S.LookupMusic("*", ref order, world, prints, traces));
        world.PrimaryLevel = new S.Level { Music = "D_E1M1", MusicOrder = 1 };

        order = 4;
        Assert.Equal("", S.LookupMusic(",cd,track", ref order, world, prints, traces));
        Assert.Equal(4, order);
        Assert.Equal(S.CdWarning, prints[0]);
        Assert.True(S.CdAudioWarned);
        Assert.Empty(traces);
        Assert.Equal("", S.LookupMusic(",CD,again", ref order, world, prints, traces));
        Assert.Equal(S.CdWarning, Assert.Single(prints));
        prints.Clear();

        order = 1;
        Assert.Equal(",CD,x", S.LookupMusic("/,CD,x", ref order, world, prints, traces));
        Assert.Empty(prints);
        Assert.Equal("LookupMusic in='/,CD,x' out=',CD,x' order=1", traces[0].Text);
        traces.Clear();

        Assert.Equal("d_runnin", S.LookupMusic("/$e1m1", ref order, world, prints, traces));
        Assert.Equal("LookupMusic in='/$e1m1' out='d_runnin' order=1", traces[0].Text);
        traces.Clear();
        Assert.Equal("D_BAR", S.LookupMusic("$bar", ref order, world, prints, traces));
        Assert.Equal("LookupMusic in='$bar' out='D_BAR' order=1", traces[0].Text);
        traces.Clear();
        Assert.Equal("$missing", S.LookupMusic("$missing", ref order, world, prints, traces));
        Assert.Equal("$None", S.LookupMusic("$None", ref order, world, prints, traces));
        Assert.Equal("$", S.LookupMusic("$", ref order, world, prints, traces));
        traces.Clear();

        order = 8;
        Assert.Equal("", S.LookupMusic("Silence", ref order, world, prints, traces));
        Assert.Equal(-1, order);
        Assert.Equal("LookupMusic 'Silence' -> '' (alias suppressed)", traces[0].Text);
        traces.Clear();

        order = 2;
        Assert.Equal("d_runnin", S.LookupMusic("D_E1M1", ref order, world, prints, traces));
        Assert.Equal(2, order);
        Assert.Equal("LookupMusic in='D_E1M1' out='d_runnin' order=2", traces[0].Text);
        traces.Clear();
        Assert.Equal("", S.LookupMusic("/", ref order, world, prints, traces));
        Assert.Equal("LookupMusic in='/' out='' order=2", traces[0].Text);

        Assert.Throws<ArgumentNullException>(() => S.LookupMusic("D_E1M1", ref order, null!, prints, traces));
    }
}
