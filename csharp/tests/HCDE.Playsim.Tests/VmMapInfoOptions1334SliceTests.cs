namespace HCDE.Playsim.Tests;

public class VmMapInfoOptions1334SliceTests
{
    [Fact]
    public void StringsFogLightAndShadow()
    {
        var w = New("=", "SEQ", "=", "SND", "=", "SOUND", "=", "XLAT", "=", "Death", "=", "Show");
        w.Apply("sndseq");
        w.Apply("SNDINFO");
        w.Apply("soundinfo");
        w.Apply("translator");
        w.Apply("deathsequence");
        w.Apply("slideshow");
        Assert.Equal("SEQ", w.SndSeq);
        Assert.Equal("SOUND", w.SoundInfo);
        Assert.Equal("XLAT", w.Translator);
        Assert.Equal("Death", w.DeathSequence);
        Assert.Equal("Show", w.Slideshow);

        w.Push("=", "BORDER", "=", "F1PIC", "=", "BACK");
        w.Apply("bordertexture");
        w.Apply("f1");
        w.Apply("mapbackground");
        Assert.Equal("BORDER", w.BorderTexture);
        Assert.Equal("F1PIC", w.F1Pic);
        Assert.Equal("BACK", w.MapBackground);

        w.Push("=", "1.25", "=", "1e2", "=", "0x1.8p1");
        w.Apply("teamdamage");
        Assert.Equal(1.25, w.TeamDamage);
        w.Apply("teamdamage");
        Assert.Equal(100d, w.TeamDamage);
        w.Apply("teamdamage");
        Assert.Equal(3d, w.TeamDamage);

        w.Push("=", "513", "=", "511", "=", "-4", "=", "2");
        w.Apply("fogdensity");
        Assert.Equal(256, w.FogDensity);
        w.Apply("fogdensity");
        Assert.Equal(255, w.FogDensity);
        w.Apply("outsidefogdensity");
        Assert.Equal(0, w.OutsideFogDensity);
        w.Apply("outsidefogdensity");
        Assert.Equal(1, w.OutsideFogDensity);

        w.ThickFogMultiplier = 9f;
        w.Push("=", "-7", "=", "4.5", "=", "-1", "=", "0", "=", "1e-50", "=", "2.5", "=", "1.5");
        w.Apply("skyfog");
        w.Apply("thickfogdistance");
        w.Apply("thickfogmultiplier");
        w.Apply("thickfogmultiplier");
        w.Apply("thickfogmultiplier");
        Assert.Equal(-7, w.SkyFog);
        Assert.Equal(4.5f, w.ThickFogDistance);
        Assert.Equal(9f, w.ThickFogMultiplier);
        w.Apply("thickfogmultiplier");
        Assert.Equal(2.5f, w.ThickFogMultiplier);
        w.Apply("pixelratio");
        Assert.Equal(1.5f, w.PixelStretch);

        w.Push("=", "200", "=", "128", "=", "255", "=", "256", "=", "-1");
        w.Apply("brightfog");
        Assert.Equal((sbyte)-56, w.BrightFog);
        w.Apply("brightfog");
        Assert.Equal((sbyte)-128, w.BrightFog);
        w.Apply("brightfog");
        Assert.Equal((sbyte)-1, w.BrightFog);
        w.Apply("brightfog");
        Assert.Equal((sbyte)0, w.BrightFog);
        w.Apply("brightfog");
        Assert.Equal((sbyte)-1, w.BrightFog);

        w.LightMode = 3;
        w.Push("=", "8", "=", "4", "=", "5", "=", "16", "=", "-1");
        w.Apply("lightmode");
        Assert.Equal(VmMapInfoOptions1334.LightNotSet, w.LightMode);
        w.Apply("lightmode");
        Assert.Equal(4, w.LightMode);
        w.Apply("lightmode");
        Assert.Equal(4, w.LightMode);
        Assert.Contains("Invalid light mode 5", w.Messages);
        Assert.True(w.ParseError);
        w.Apply("lightmode");
        Assert.Equal(VmMapInfoOptions1334.LightNotSet, w.LightMode);
        w.Apply("lightmode");
        Assert.Equal(VmMapInfoOptions1334.LightNotSet, w.LightMode);
        Assert.Contains("Invalid light mode -1", w.Messages);

        w.Push("=", "8", "=", "-2", "=", "7", "=", "-1");
        w.Apply("hcde_shadowprofile");
        Assert.Equal(7, w.HcdeShadowProfile);
        w.Apply("hcde_shadowprofile");
        Assert.Equal(-1, w.HcdeShadowProfile);
        w.Apply("hcde_shadowprofile");
        Assert.Equal(7, w.HcdeShadowProfile);
        w.Apply("hcde_shadowprofile");
        Assert.Equal(-1, w.HcdeShadowProfile);

        w.Push("=", "0", "=", "255", "=", "256", "=", "8192", "=", "8193", "=", "-1");
        w.Apply("hcde_shadowqualitycap");
        Assert.Equal(128, w.HcdeShadowQualityCap);
        w.Apply("hcde_shadowqualitycap");
        Assert.Equal(128, w.HcdeShadowQualityCap);
        w.Apply("hcde_shadowqualitycap");
        Assert.Equal(256, w.HcdeShadowQualityCap);
        w.Apply("hcde_shadowqualitycap");
        Assert.Equal(8192, w.HcdeShadowQualityCap);
        w.Apply("hcde_shadowqualitycap");
        Assert.Equal(8192, w.HcdeShadowQualityCap);
        w.Apply("hcde_shadowqualitycap");
        Assert.Equal(-1, w.HcdeShadowQualityCap);
        Assert.Equal(128, VmMapInfoOptions1334.NormalizeCap(127));
        Assert.Equal(4096, VmMapInfoOptions1334.NormalizeCap(4096));

        w.Push("=", "1024", "=", "1025", "=", "-4", "=", "0");
        w.Apply("hcde_shadowmaxlightscap");
        Assert.Equal(1024, w.HcdeShadowMaxLights);
        w.Apply("hcde_shadowmaxlightscap");
        Assert.Equal(1024, w.HcdeShadowMaxLights);
        w.Apply("hcde_shadowmaxlightscap");
        Assert.Equal(-1, w.HcdeShadowMaxLights);
        w.Apply("hcde_shadowmaxlightscap");
        Assert.Equal(0, w.HcdeShadowMaxLights);

        w.X11Text = "! comment\n255 0 0 red\n0 128 255 dodger blue\n";
        w.Push("=", "#f80", "=", "#ff8000", "=", "ff8000", "=", "f 8 0", "=", "Red", "=", "dodger blue", "=", "#gg0000", "=", "#12");
        w.Apply("hazardcolor");
        Assert.Equal(0xFF8800, w.HazardColor);
        w.Apply("hazardcolor");
        Assert.Equal(0xFF8000, w.HazardColor);
        w.Apply("hazardcolor");
        Assert.Equal(0xFF8000, w.HazardColor);
        w.Apply("hazardflash");
        Assert.Equal(0xFF8800, w.HazardFlash);
        w.Apply("hazardcolor");
        Assert.Equal(0xFF0000, w.HazardColor);
        w.Apply("hazardcolor");
        Assert.Equal(0x0080FF, w.HazardColor);
        w.Apply("hazardcolor");
        Assert.Equal(0, w.HazardColor);
        Assert.Contains("Bad hex number: gg", w.Warnings);
        w.Apply("hazardcolor");
        Assert.Equal(0, w.HazardColor);
        Assert.DoesNotContain("X11R6RGB lump not found", w.Warnings);

        var missing = New("=", "#00ff00");
        missing.X11Mode = VmMapInfoOptions1334.X11Missing;
        missing.Apply("hazardcolor");
        Assert.Equal(0x00FF00, missing.HazardColor);
        Assert.Contains("X11R6RGB lump not found", missing.Warnings);
        Assert.False(missing.ParseError);

        var corrupt = New("=", "red");
        corrupt.X11Text = "notanumber\n";
        corrupt.Apply("hazardcolor");
        Assert.Contains("X11R6RGB lump is corrupt", corrupt.Warnings);
        Assert.Equal(0, corrupt.HazardColor);

        var env = New("=", "1", ",", "2", "=", "0x10", "=", "Cave", "=", "Nope", "=", "010", "=", "MAXINT");
        env.Environments["Cave"] = 22;
        env.Apply("defaultenvironment");
        Assert.Equal(258, env.DefaultEnvironment);
        env.Apply("defaultenvironment");
        Assert.Equal(4096, env.DefaultEnvironment);
        env.Apply("defaultenvironment");
        Assert.Equal(22, env.DefaultEnvironment);
        env.Apply("defaultenvironment");
        Assert.Equal(0, env.DefaultEnvironment);
        Assert.Contains("Unknown sound environment 'Nope'\n", env.Messages);
        Assert.True(env.ParseError);
        env.Apply("defaultenvironment");
        Assert.Equal(2048, env.DefaultEnvironment);
        env.Apply("defaultenvironment");
        Assert.Equal(-256, env.DefaultEnvironment);

        var octal = New("=", "010");
        octal.NoOctals = true;
        octal.Apply("defaultenvironment");
        Assert.Equal(2560, octal.DefaultEnvironment);

        var oldEnv = New("3", "4");
        oldEnv.Format = VmMapInfoOptions1334.FmtOld;
        oldEnv.Apply("defaultenvironment");
        Assert.Equal(772, oldEnv.DefaultEnvironment);

        var oldDeath = New("Die");
        oldDeath.Format = VmMapInfoOptions1334.FmtOld;
        oldDeath.Apply("deathsequence");
        Assert.Equal("Die", oldDeath.DeathSequence);

        Assert.Equal(31, VmMapInfoOptions1334.Catalog.Count(o => o.MacroInBand));
        Assert.Contains(VmMapInfoOptions1334.Catalog, o => o.Name == "notexturefill" && !o.Old && o.MacroInBand);
        Assert.Contains(VmMapInfoOptions1334.Catalog, o => o.Name == "redirect" && o.Old && !o.MacroInBand);
        Assert.Contains(VmMapInfoOptions1334.Catalog, o => o.Name == "teamdamage" && o.Old);
        Assert.Contains(VmMapInfoOptions1334.Catalog, o => o.Name == "deathsequence" && !o.Old);

        var cut = New("1");
        var ex = Assert.Throws<InvalidOperationException>(() => cut.Apply("notexturefill"));
        Assert.Equal("notexturefill body starts at line 1827", ex.Message);
        Assert.True(cut.HasPending);
        Assert.Equal("1", cut.Pending);
        Assert.Throws<InvalidOperationException>(() => cut.Apply("skyrotate"));

        var badEq = New("SEQ");
        var miss = Assert.Throws<InvalidOperationException>(() => badEq.Apply("sndseq"));
        Assert.Equal("Expected '=', got 'SEQ'.", miss.Message);

        var badFloat = New("=", "1.5x");
        var bad = Assert.Throws<InvalidOperationException>(() => badFloat.Apply("teamdamage"));
        Assert.Equal("SC_GetFloat: Bad numeric constant \"1.5x\".", bad.Message);
    }

    [Fact]
    public void RedirectExitBlendAndEndgame()
    {
        var numbered = New("=", "Imp", ",", "7");
        numbered.Apply("redirect");
        Assert.Equal("Imp", numbered.RedirectType);
        Assert.Equal("MAP07", numbered.RedirectMapName);

        var hexen = New("=", "Key", ",", "4");
        hexen.HexenHack = true;
        hexen.Apply("cvar_redirect");
        Assert.Equal("Key", hexen.RedirectCVar);
        Assert.Equal("&wt@04", hexen.RedirectCVarMap);

        var wide = New("=", "Key", ",", "100");
        wide.Apply("redirect");
        Assert.Equal("MAP100", wide.RedirectMapName);
        var neg = New("=", "Key", ",", "-1");
        neg.Apply("redirect");
        Assert.Equal("MAP-1", neg.RedirectMapName);

        var plain = New("=", "DoomImp", ",", "E1M1");
        plain.Apply("redirect");
        Assert.Equal("E1M1", plain.RedirectMapName);
        Assert.False(plain.HasPending);

        var old = New("DoomImp", "3");
        old.Format = VmMapInfoOptions1334.FmtOld;
        old.Apply("redirect");
        Assert.Equal("DoomImp", old.RedirectType);
        Assert.Equal("MAP03", old.RedirectMapName);

        var comma = New("=", "DoomImp", "MAP01");
        var expectedComma = Assert.Throws<InvalidOperationException>(() => comma.Apply("redirect"));
        Assert.Equal("Expected ',', got 'MAP01'.", expectedComma.Message);

        var bunny = New("=", "T", ",", "endbunny", "=", "T", ",", "endcast");
        bunny.Apply("redirect");
        Assert.Equal("enDSeQ0001", bunny.RedirectMapName);
        Assert.Equal("Inter_Bunny", bunny.NameOf(1));
        bunny.Apply("redirect");
        Assert.Equal("enDSeQ0002", bunny.RedirectMapName);
        Assert.Equal("Inter_Cast", bunny.NameOf(2));
        Assert.Equal("Inter_Bunny", bunny.NameOf(1));

        var skill = New("=", "T", ",", "EndGame1");
        skill.Apply("redirect");
        Assert.Equal("Inter_Pic1", skill.NameOf(1));
        Assert.Equal("enDSeQ0001", skill.RedirectMapName);
        var lower = New("=", "T", ",", "EndGamec");
        lower.Apply("redirect");
        Assert.Equal("Inter_Pic3", lower.NameOf(1));
        var castBit = New("=", "T", ",", "EndGameC");
        castBit.Apply("redirect");
        Assert.Equal("Inter_Cast", castBit.NameOf(1));
        var water = New("=", "T", ",", "EndGameW");
        water.Apply("redirect");
        Assert.Equal("Inter_Underwater", water.NameOf(1));

        var bare = New("=", "T", ",", "endgame", "E1M1");
        bare.Apply("redirect");
        Assert.Equal("enDSeQ0001", bare.RedirectMapName);
        Assert.Equal("Inter_Pic3", bare.NameOf(1));
        Assert.True(bare.HasPending);
        Assert.Equal("E1M1", bare.Pending);

        var pic = New("=", "T", ",", "endpic", ",", "FOO");
        pic.Apply("redirect");
        Assert.Equal("FOO", pic.LastPic);
        Assert.Equal("@EndPic_FOO", pic.NameOf(1));
        Assert.Equal("enDSeQ0001", pic.RedirectMapName);

        var seq = New("=", "T", ",", "endsequence", ",", "MySeq");
        seq.Apply("redirect");
        Assert.Equal("MySeq", seq.NameOf(1));
        Assert.Equal("enDSeQ0001", seq.RedirectMapName);

        var block = New(
            "=", "Imp", ",", "endgame", "{",
            "pic", "=", "A",
            "weird", "=", "1", ",", "2",
            "music", "=", "D_X", ",", "0",
            "}");
        block.Apply("redirect");
        Assert.Equal("Imp", block.RedirectType);
        Assert.Equal("enDSeQ0001", block.RedirectMapName);
        Assert.Equal("@EndSequence_0_", block.LastFinale!.Name);
        Assert.Equal(VmMapInfoOptions1334.EndPic, block.LastFinale.EndType);
        Assert.Equal("A", block.LastFinale.Pic);
        Assert.Equal("D_X", block.LastFinale.Music);
        Assert.False(block.LastFinale.MusicLooping);
        Assert.Contains("Unknown property 'weird' found in endgame definition\n", block.Messages);
        Assert.True(block.ParseError);

        var scroll = New("=", "T", ",", "endgame", "{", "hscroll", "=", "A", ",", "B", ",", "1", "}");
        scroll.Apply("redirect");
        Assert.Equal(VmMapInfoOptions1334.EndBunny, scroll.LastFinale!.EndType);
        Assert.Equal("A", scroll.LastFinale.Pic);
        Assert.Equal("B", scroll.LastFinale.Pic2);
        Assert.True(scroll.LastFinale.PlayTheEnd);
        Assert.Equal("TheEnd", scroll.LastFinale.Link);
        Assert.Equal(VmMapInfoOptions1334.ScrollLeft, scroll.LastFinale.ScrollDir);
        Assert.Equal(230, scroll.LastFinale.ScrollDelay);
        Assert.Equal(640, scroll.LastFinale.ScrollTime);
        Assert.Equal(1130, scroll.LastFinale.Duration);
        Assert.Equal("@EndSequence_0_", scroll.LastFinale.Name);

        var demon = New("=", "T", ",", "endgame", "{", "vscroll", "=", "D1", ",", "D2", "}");
        demon.Apply("redirect");
        Assert.Equal(VmMapInfoOptions1334.EndDemon, demon.LastFinale!.EndType);
        Assert.Equal(VmMapInfoOptions1334.ScrollUp, demon.LastFinale.ScrollDir);
        Assert.Equal(70, demon.LastFinale.ScrollDelay);
        Assert.Equal(600, demon.LastFinale.ScrollTime);
        Assert.Equal("", demon.LastFinale.Link);
        Assert.Equal("D1", demon.LastFinale.Pic);
        Assert.Equal("D2", demon.LastFinale.Pic2);

        var cast = New("=", "T", ",", "endgame", "{", "cast", "}");
        cast.Apply("redirect");
        Assert.Equal(VmMapInfoOptions1334.EndCast, cast.LastFinale!.EndType);
        Assert.Equal("$bgcastcall", cast.LastFinale.Pic);
        Assert.Equal(1, cast.LastFinale.Duration);
        Assert.Equal("Doom2Cast", cast.LastFinale.Link);

        var empty = New("=", "T", ",", "endgame", "{", "}");
        var emptyEx = Assert.Throws<InvalidOperationException>(() => empty.Apply("redirect"));
        Assert.Equal("Endgame type was not defined", emptyEx.Message);
        Assert.Equal("endgame", empty.RedirectMapName);

        var oldBad = New("T", "endgame", "{", "foo", "}");
        oldBad.Format = VmMapInfoOptions1334.FmtOld;
        var oldEx = Assert.Throws<InvalidOperationException>(() => oldBad.Apply("redirect"));
        Assert.Equal("Unknown property 'foo' found in endgame definition\n", oldEx.Message);
        Assert.Equal("endgame", oldBad.RedirectMapName);
        Assert.Equal("T", oldBad.RedirectType);

        var exits = New(
            "=", "E1M1", ",", "Hello", ",", "World",
            "=", "e1m1", ",", "lookup", ",", "HI",
            "=", "E2M1", ",", "$SECRET",
            "=", "E1M1", ",", "FLOOR",
            "=", "E3M1", ",", "WALL",
            "=", "E4M1", ",", "D_DM2:08",
            "=", "E5M1", ",", "D_READ", ",", "4");
        exits.Apply("exittext");
        VmMapInfoOptions1334.ExitDef hello = exits.FindExit("E1M1")!;
        Assert.Equal("Hello\nWorld", hello.Text);
        Assert.Equal(VmMapInfoOptions1334.DefText, hello.Defined);
        exits.Apply("exittext");
        Assert.Equal("HI", hello.Text);
        Assert.Equal(VmMapInfoOptions1334.DefText | VmMapInfoOptions1334.DefLookup, hello.Defined);
        Assert.Equal("E1M1", hello.Map);
        exits.Apply("exittext");
        VmMapInfoOptions1334.ExitDef secret = exits.FindExit("E2M1")!;
        Assert.Equal("SECRET", secret.Text);
        Assert.Equal(VmMapInfoOptions1334.DefText | VmMapInfoOptions1334.DefLookup, secret.Defined);
        exits.Apply("textflat");
        Assert.Equal("FLOOR", hello.Backdrop);
        Assert.Equal(
            VmMapInfoOptions1334.DefText | VmMapInfoOptions1334.DefLookup | VmMapInfoOptions1334.DefBackdrop,
            hello.Defined);
        exits.Apply("textpic");
        VmMapInfoOptions1334.ExitDef wall = exits.FindExit("E3M1")!;
        Assert.Equal("WALL", wall.Backdrop);
        Assert.Equal(
            VmMapInfoOptions1334.DefBackdrop | VmMapInfoOptions1334.DefMusic | VmMapInfoOptions1334.DefPic,
            wall.Defined);
        exits.Apply("textmusic");
        VmMapInfoOptions1334.ExitDef colon = exits.FindExit("E4M1")!;
        Assert.Equal("D_DM2", colon.Music);
        Assert.Equal(8, colon.Order);
        Assert.Equal(VmMapInfoOptions1334.DefMusic, colon.Defined);
        exits.Apply("textmusic");
        VmMapInfoOptions1334.ExitDef ordered = exits.FindExit("E5M1")!;
        Assert.Equal("D_READ", ordered.Music);
        Assert.Equal(4, ordered.Order);

        var flat = New("=", "E7M1", ",", "FLAT");
        flat.Apply("textflat");
        VmMapInfoOptions1334.ExitDef fresh = flat.FindExit("E7M1")!;
        Assert.Equal(VmMapInfoOptions1334.DefBackdrop | VmMapInfoOptions1334.DefMusic, fresh.Defined);

        var oldMusic = New("E8M1", "D_X", "6");
        oldMusic.Format = VmMapInfoOptions1334.FmtOld;
        oldMusic.Apply("textmusic");
        Assert.Equal(6, oldMusic.FindExit("E8M1")!.Order);
        Assert.Equal("D_X", oldMusic.FindExit("E8M1")!.Music);

        var compat = New("=", "E6M1", ",", "Hello", ",", "}");
        compat.TrailingTextCommaCompat = true;
        compat.Apply("exittext");
        Assert.Equal("Hello", compat.FindExit("E6M1")!.Text);
        Assert.Contains(
            "HCDE: tolerated trailing comma in MAPINFO text list for known mod compatibility.\n",
            compat.Prints);
        Assert.True(compat.HasPending);
        Assert.Equal("}", compat.Pending);

        var raw = New("=", "E6M1", ",", "Hello", ",", "}");
        raw.Apply("exittext");
        Assert.Equal("Hello\n}", raw.FindExit("E6M1")!.Text);

        var blend = New("=", "Unclamped", ",", "Reinhard");
        blend.Tonemap = VmMapInfoOptions1334.ToneLinear;
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.BlendNoClamp, blend.LightBlendMode);
        Assert.Equal(VmMapInfoOptions1334.ToneReinhard, blend.Tonemap);
        blend.Push("=", "coloredclamp");
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.BlendColor, blend.LightBlendMode);
        Assert.Equal(VmMapInfoOptions1334.ToneReinhard, blend.Tonemap);
        blend.Push("=", "Default");
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.BlendClamp, blend.LightBlendMode);
        blend.Push("=", "clamp");
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.BlendClamp, blend.LightBlendMode);
        blend.Push("=", "nope");
        blend.LightBlendMode = VmMapInfoOptions1334.BlendColor;
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.BlendColor, blend.LightBlendMode);
        Assert.Contains("Invalid light blend mode nope", blend.Messages);
        blend.Push("=", "unclamped", ",", "Bogus");
        blend.Tonemap = VmMapInfoOptions1334.ToneLinear;
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.BlendNoClamp, blend.LightBlendMode);
        Assert.Equal(VmMapInfoOptions1334.ToneLinear, blend.Tonemap);
        Assert.Contains("Invalid tonemap Bogus", blend.Messages);
        blend.Push("=", "Unclamped", ",", "hejldawson");
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.ToneHejlDawson, blend.Tonemap);
        blend.Push("=", "Unclamped", ",", "None");
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.ToneNone, blend.Tonemap);
        blend.Push("=", "Unclamped");
        blend.Tonemap = VmMapInfoOptions1334.ToneLinear;
        blend.Apply("lightblendmode");
        Assert.Equal(VmMapInfoOptions1334.BlendNoClamp, blend.LightBlendMode);
        Assert.Equal(VmMapInfoOptions1334.ToneLinear, blend.Tonemap);
    }

    static VmMapInfoOptions1334 New(params string[] tokens)
    {
        var world = new VmMapInfoOptions1334();
        world.Push(tokens);
        return world;
    }
}
