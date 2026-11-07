using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDMain1421SliceTests
{
    [Fact]
    public void FatalDebugScreenAndRng()
    {
        Assert.Equal(163, VmDMain1421Slice.BandEnd - VmDMain1421Slice.BandStart + 1);
        Assert.Equal(4499, VmDMain1421Slice.FunctionEnd);
        Assert.Equal(4020, VmDMain1421Slice.NextPhase);
        var missing = new VmDMain1421Slice.State { HasIwad = false };
        var fatal = Assert.Throws<VmDMain1421Slice.Fatal>(() => VmDMain1421Slice.Boot(missing));
        Assert.Equal(VmDMain1421Slice.MissingIwad, fatal.Message);
        Assert.Empty(missing.Guards);
        var debug = new VmDMain1421Slice.State { DebugValue = "", AllWads = { "doom.wad" } };
        VmDMain1421Slice.Boot(debug);
        Assert.True(debug.DapDebugging);
        Assert.False(debug.Jit);
        Assert.False(debug.JitAot);
        var live = new VmDMain1421Slice.State { VmDebug = true, Jit = true, AllWads = { "doom.wad" } };
        VmDMain1421Slice.Boot(live);
        Assert.False(live.JitAot);
        var dedicated = new VmDMain1421Slice.State { Dedicated = true, AllWads = { "hexen.wad" }, GameType = 4, LumpFilter = "hexen.wad" };
        VmDMain1421Slice.Boot(dedicated);
        Assert.True(dedicated.ScreenNull);
        Assert.Equal(0, dedicated.ScreenInits);
        Assert.Contains("S_Init: Setting up server sound tables.\n", dedicated.Lines);
        var restart = new VmDMain1421Slice.State { Restart = true, Dedicated = true, ScreenNull = false, AllWads = { "doom.wad" } };
        VmDMain1421Slice.Boot(restart);
        Assert.False(restart.ScreenNull);
        Assert.Equal(0, restart.ScreenSizeInits);
        Assert.True(restart.NoStartScreen);
        var client = new VmDMain1421Slice.State { AllWads = { "doom.wad" } };
        VmDMain1421Slice.Boot(client);
        Assert.Contains("startup dummy framebuffer", client.Guards);
        Assert.Equal(1, client.ScreenSizeInits);
        Assert.Equal(1, client.ScreenInits);
        Assert.False(client.ScreenNull);
        var seeded = new VmDMain1421Slice.State { RngValue = "  -12abc", AllWads = { "doom.wad" } };
        VmDMain1421Slice.Boot(seeded);
        Assert.Equal(-12, seeded.RngSeed);
        Assert.Equal(-12, seeded.StaticRngSeed);
        Assert.Equal(-12, seeded.SrandSeed);
        Assert.True(seeded.UseStaticRng);
        Assert.Contains("D_DoomInit: Static RNGseed -12 set.\n", seeded.Lines);
        var made = new VmDMain1421Slice.State { MadeSeed = 99, Batch = true, AllWads = { "doom.wad" } };
        VmDMain1421Slice.Boot(made);
        Assert.Equal(99, made.RngSeed);
        Assert.False(made.UseStaticRng);
        Assert.Equal(99, made.SrandSeed);
        Assert.Equal(1, made.Clears);
        Assert.DoesNotContain(made.Lines, line => line.Contains("RNGseed", StringComparison.Ordinal));
        Assert.DoesNotContain(made.Lines, line => line.StartsWith("W_Init", StringComparison.Ordinal));
        Assert.DoesNotContain(made.Lines, line => line.StartsWith("S_Init", StringComparison.Ordinal));
        Assert.Equal(1, made.SoundTrace);
    }

    [Fact]
    public void FilesFiltersAndProgress()
    {
        var doom = new VmDMain1421Slice.State
        {
            Autoname = "doom.id",
            ConfigName = "Doom",
            NoKeyboardCheats = true,
            Flags = 1,
            AllWads = { "doom.wad", "doom.wad" },
            Pwads = { "mod.wad", "mod.wad" },
            PullinWads = { "extra.pk3" },
            Autoexec = { "auto.cfg" },
            GatheredExec = { "user.cfg" },
            Plus = { "+map" },
            DeleteLumps = "DEMO1",
            TextureGuess = 2000,
            LumpFilter = "doom.id.doom.freedoom",
        };
        VmDMain1421Slice.Boot(doom);
        Assert.Equal("doom.id", doom.SaveFolder);
        Assert.Equal(1, doom.CopiedType);
        Assert.Equal(1, doom.CopiedFlags);
        Assert.True(doom.CopiedCheats);
        Assert.Equal("Doom", doom.SetupConfig);
        Assert.Equal("doom.id", doom.AutoloadName);
        Assert.Equal(1, doom.CallbackDisables);
        Assert.NotNull(doom.Exec);
        Assert.Equal(new[] { "auto.cfg", "user.cfg" }, doom.Exec!.Files);
        Assert.Equal(new[] { "+map" }, doom.Exec.Plus);
        Assert.Equal(1, doom.Pullins);
        Assert.Equal(new[] { "", "" }, doom.Pwads);
        Assert.True(doom.ClearedWads);
        Assert.Empty(doom.AllWads);
        Assert.True(doom.Shrunk);
        Assert.Equal(1, doom.Postprocess);
        Assert.Equal("DEMO1", doom.SpriteLumps);
        Assert.Equal(new[]
        {
            "game-DoomStrifeChex",
            "game-DoomChex",
            "game-Doom",
            "doom.id.doom.freedoom",
            "doom.doom.freedoom",
        }, doom.Filters);
        Assert.Equal(1, doom.Reserved);
        Assert.Equal(new[]
        {
            "Scanning and mounting WAD/PK3 archives...",
            "Parsing loaded game resources...",
        }, doom.Status);
        Assert.Equal(8000, doom.MaxProgress);
        Assert.Equal(0, doom.PerShaderProgress);
        Assert.False(doom.VideoInitialized);
        Assert.False(doom.FirstShaderCompiled);
        Assert.False(doom.NoStartScreen);
        Assert.Equal(1, doom.Mapxx);
        Assert.Equal(1, doom.Eternity);
        Assert.Equal(1, doom.Edf);
        Assert.Equal(1, doom.DefCvars);
        Assert.Equal(1, doom.Palette);
        Assert.Equal(1, doom.SoundInit);
        Assert.Contains("S_Init: Setting up sound.\n", doom.Lines);
        var dup = new VmDMain1421Slice.State
        {
            AllowDuplicates = false,
            EntryOverride = 0,
            AllWads = { "a.wad", "b.wad", "a.wad" },
            Pwads = { "b.wad" },
            LumpFilter = "strife",
            GameType = 8,
        };
        var none = Assert.Throws<VmDMain1421Slice.Fatal>(() => VmDMain1421Slice.Boot(dup));
        Assert.Equal(VmDMain1421Slice.NoFiles, none.Message);
        Assert.Equal(new[] { "a.wad", "b.wad" }, dup.AllWads);
        Assert.Equal(0, dup.Postprocess);
        Assert.False(dup.ClearedWads);
        var kept = new VmDMain1421Slice.State
        {
            AllowDuplicates = true,
            AllWads = { "a.wad", "a.wad" },
            GameType = 2,
            LumpFilter = "heretic",
            TextureGuess = 1,
            Join = true,
        };
        VmDMain1421Slice.Boot(kept);
        Assert.Empty(kept.AllWads);
        Assert.Equal(4096, kept.MaxProgress);
        Assert.True(kept.NoStartScreen);
        Assert.Equal(new[] { "game-Raven", "game-Heretic", "heretic" }, kept.Filters);
        var chex = new VmDMain1421Slice.State { GameType = 16, LumpFilter = "chex", AllWads = { "chex.wad" }, Host = true, NoAutoexec = true };
        VmDMain1421Slice.Boot(chex);
        Assert.Equal(new[] { "game-DoomStrifeChex", "game-DoomChex", "game-Chex", "chex" }, chex.Filters);
        Assert.Null(chex.Exec);
        Assert.Equal(0, chex.Pullins);
        Assert.True(chex.NoStartScreen);
        var exact = new VmDMain1421Slice.State { LumpFilter = "doom.id.doom", AllWads = { "doom.wad" }, TextureGuess = 1024 };
        VmDMain1421Slice.Boot(exact);
        Assert.Contains("doom.doom", exact.Filters);
        Assert.Equal(4096, exact.MaxProgress);
        Assert.Throws<NullReferenceException>(() => VmDMain1421Slice.Boot(new VmDMain1421Slice.State { GameType = 3, AllWads = { "x" } }));
        Assert.Throws<NullReferenceException>(() => VmDMain1421Slice.Boot(new VmDMain1421Slice.State { Autoname = null, AllWads = { "x" } }));
    }
}
