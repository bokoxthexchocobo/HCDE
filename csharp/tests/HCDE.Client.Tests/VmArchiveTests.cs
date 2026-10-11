using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using HCDE.MapLoader;
using HCDE.Playsim;
using HCDE.Scripting;

namespace HCDE.Client.Tests;

public class VmArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RequiredPackagesLoadBeforeSelectedGame(bool embedded)
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-required-package-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var game = Path.Combine(root, embedded ? "expansion.ipk3" : "expansion.pk3");
            var dependency = Path.Combine(root, embedded ? "base.ipk3" : "base.pk3");
            var catalog = Path.Combine(root, "iwadinfo.txt");
            const string expansionInfo = "IWAD { Name = Expansion MustContain = EXPAND Required = Base Autoname = custom.game }";
            File.WriteAllText(catalog, expansionInfo + " IWAD { Name = Base MustContain = BASE } Names { broken.pk3 base.pk3 }");
            File.WriteAllBytes(Path.Combine(root, "broken.pk3"), [80, 65, 67, 75, 255, 255, 255, 127, 64, 0, 0, 0]);
            File.WriteAllBytes(game, ScriptZip(("expand.dat", ""), ("iwadinfo.txt", expansionInfo),
                ("zscript", "version 4.13\n#include \"child.txt\""), ("child.txt", "expansion")));
            File.WriteAllBytes(dependency, ScriptZip(("base.dat", ""), ("iwadinfo.txt", "IWAD { Name = Base }"),
                ("zscript", "version 4.12"), ("child.txt", "base")));
            string[] args = embedded
                ? ["--iwad", game]
                : ["--iwad", game, "--iwad-info", catalog];
            Assert.True(LauncherPlan.TryParse(args, out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0) }, host.VmScriptSources.Select(set => set.Version));
            Assert.Equal("expansion", host.VmScriptSources[1].Sources[1].Text);
            var definitions = VmIwadInfoParser.Parse(File.ReadAllText(catalog));
            Assert.Equal(dependency, VmIwadRequiredResource.Resolve(definitions.Entries[0], definitions, game, dependency));
            Assert.Throws<InvalidDataException>(() => VmIwadRequiredResource.Resolve(definitions.Entries[0], definitions, game,
                Path.Combine(root, "broken.pk3")));
            File.WriteAllBytes(dependency, ScriptZip(("iwadinfo.txt", "IWAD { Name = Wrong }")));
            if (embedded) Assert.Throws<InvalidDataException>(() => LauncherPlan.CreateHost(copy, demo.Simulation));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ArchiveIdentificationUsesNativeNamespacesAndMapPathsWithoutReadingPayloads()
    {
        var definition = new VmIwadDefinition("Package", VmStartupGameType.Doom, "custom.game", ["MAP01", "SPRITE", "MARKER"]);
        var bytes = BinaryScriptZip(("maps/map01.wad", new byte[] { 255 }),
            ("sprites/sprite.png", new byte[] { 255 }), ("marker.dat", new byte[] { 255 }),
            ("hidden/secret.dat", new byte[] { 255 }), ("zscript", Encoding.UTF8.GetBytes("#include \"missing\"")));
        var archive = new VmScriptArchive("game.pk3", bytes, DeletedGlobalLumps: ["MARKER"]);
        Assert.Same(definition, VmIwadIdentifier.IdentifyArchive([definition], archive));
        var unknown = new VmIwadDefinition("Hidden", VmStartupGameType.Doom, "", ["SECRET"]);
        Assert.Null(VmIwadIdentifier.IdentifyArchive([unknown], archive));
        var names = VmArchiveScriptLoader.ReadResourceNames(archive);
        Assert.Contains(names, name => name.ShortName == "SPRITE" && name.FullName == "sprites/sprite.png");
        Assert.Contains(names, name => name.ShortName == "" && name.FullName == "hidden/secret.dat");
    }

    [Fact]
    public void ArchiveIdentificationIncludesNonGlobalWadNames()
    {
        var definition = new VmIwadDefinition("Sprite Game", VmStartupGameType.Doom, "", ["SPECIAL"]);
        var archive = new VmScriptArchive("game.wad", ScriptArchive(("S_START", ""), ("SPECIAL", "sprite data"), ("S_END", "")));
        Assert.Same(definition, VmIwadIdentifier.IdentifyArchive([definition], archive));
    }

    [Fact]
    public void ExternalCatalogIdentifiesPackageAtStartup()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-catalog-package-" + Guid.NewGuid().ToString("N") + ".pk3");
        try
        {
            File.WriteAllText(path + ".txt", "IWAD { Name = Package Game = Doom Autoname = custom.game MustContain = MAP01, MARKER }");
            File.WriteAllBytes(path, ScriptZip(("maps/map01.wad", ""), ("marker.dat", ""),
                ("zscript", "#include \"child.zs\""), ("child.zs", "base"), ("filter/custom.game/child.zs", "identified")));
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(new LauncherSelection { IwadPath = path, IwadInfoPath = path + ".txt" }, demo.Simulation);
            Assert.Equal("identified", Assert.Single(host.VmScriptSources).Sources[1].Text);
        }
        finally { File.Delete(path); File.Delete(path + ".txt"); }
    }

    [Theory]
    [InlineData(".ipk3")]
    [InlineData(".IPK7")]
    public void SpecialPackageStartupReadsEmbeddedIwadInfo(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-info-package-" + Guid.NewGuid().ToString("N") + extension);
        try
        {
            (string Name, string Text)[] files = [
                ("project/iwadinfo.txt", "IWAD { Name = Package Game = Doom Autoname = custom.game DeleteLumps = ZSCRIPT }"),
                ("project/zscript.txt", "version 4.13")];
            byte[] bytes;
            if (extension.Equals(".ipk3", StringComparison.OrdinalIgnoreCase)) bytes = ScriptZip(files);
            else
            {
                using var output = new MemoryStream();
                using (var writer = new SharpCompress.Writers.SevenZip.SevenZipWriter(output,
                    new SharpCompress.Writers.SevenZip.SevenZipWriterOptions(SharpCompress.Common.CompressionType.LZMA2)))
                    foreach (var file in files)
                    {
                        using var input = new MemoryStream(Encoding.UTF8.GetBytes(file.Text));
                        writer.Write(file.Name, input, null);
                    }
                bytes = output.ToArray();
            }
            Assert.Equal("Package", VmIwadInfoParser.ReadEmbeddedArchive(new(path, bytes)).Definition.Name);
            File.WriteAllBytes(path, bytes);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(new LauncherSelection { IwadPath = path }, demo.Simulation);
            Assert.Empty(host.VmScriptSources);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void EmbeddedMetadataLookupUsesLastGlobalNameAndDoesNotParseZscript()
    {
        var archive = new VmScriptArchive("game.ipk3", ScriptZip(
            ("iwadinfo.a", "IWAD { Name = First }"), ("iwadinfo.z", "IWAD { Name = Last }"),
            ("filter/doom/iwadinfo.txt", "IWAD { Name = Filtered }"),
            ("zscript", "#include \"missing-script\"")));
        Assert.Equal("Last", VmIwadInfoParser.ReadEmbeddedArchive(archive).Definition.Name);
        Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], gameTypeFilters: []));
        Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.ReadEmbeddedArchive(new("empty.ipk3", ScriptZip())));
        Assert.Throws<ArgumentException>(() => VmArchiveScriptLoader.ReadGlobalText(archive, "scripts/iwadinfo"));
        Assert.Throws<ArgumentException>(() => VmArchiveScriptLoader.ReadGlobalText(archive, "TOOLONG99"));
        Assert.Throws<DecoderFallbackException>(() => VmIwadInfoParser.ReadEmbeddedArchive(new("invalid.ipk3",
            BinaryScriptZip(("iwadinfo.txt", new byte[] { 255 })))));
    }

    [Fact]
    public void EmbeddedIwadInfoUsesLastGlobalRecordAndIdentifiesSpecialIwadStartup()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-embedded-info-" + Guid.NewGuid().ToString("N") + ".IWAD");
        try
        {
            var wad = ScriptArchive(("IWADINFO", "IWAD { Name = Old Game = Heretic }"),
                ("IWADINFO", "IWAD { Name = New Game = Doom Autoname = doom.id.doom2 MustContain = MISSING DeleteLumps = ZSCRIPT }"),
                ("S_START", ""), ("IWADINFO", "hidden invalid record"), ("S_END", ""), ("ZSCRIPT", "version 4.13"));
            Assert.Equal("New", VmIwadInfoParser.ReadEmbeddedWad(wad).Definition.Name);
            File.WriteAllBytes(path, wad);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(new LauncherSelection { IwadPath = path }, demo.Simulation);
            Assert.Empty(host.VmScriptSources);
            Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.ReadEmbeddedWad(ScriptArchive()));
            Assert.Throws<InvalidDataException>(() => VmIwadInfoParser.ReadEmbeddedWad(new byte[12]));
            File.WriteAllBytes(path, ScriptArchive(("ZSCRIPT", "")));
            Assert.Throws<InvalidDataException>(() => LauncherPlan.CreateHost(new LauncherSelection { IwadPath = path }, demo.Simulation));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(VmScriptArchiveFormat.Pk3)]
    [InlineData(VmScriptArchiveFormat.Grp)]
    [InlineData(VmScriptArchiveFormat.Hog)]
    [InlineData(VmScriptArchiveFormat.Mvl)]
    public void ArchiveDeletionHidesShortLookupAndPreservesFullPath(VmScriptArchiveFormat format)
    {
        (string Name, string Text)[] files = [("zscript.zs", "#include \"CHILD\"\n#include \"child.txt\""), ("child.txt", "target")];
        var data = format switch
        {
            VmScriptArchiveFormat.Pk3 => ScriptZip(files),
            VmScriptArchiveFormat.Grp => ScriptGrp(files),
            VmScriptArchiveFormat.Hog => ScriptHog(files),
            _ => ScriptMvl(files),
        };
        var sets = VmArchiveScriptLoader.LoadAll([
            new("engine.wad", ScriptArchive(("CHILD", "engine"))),
            new("game.archive", data, format, ["CHILD"])]);
        var set = Assert.Single(sets);
        Assert.Equal(new[] { "engine", "target" }, set.Sources.Skip(1).Select(source => source.Text));
        Assert.Empty(VmArchiveScriptLoader.LoadAll([new("game.archive", data, format, ["ZSCRIPT"])]));
    }

    [Fact]
    public void DeletedArchiveRootRemainsAvailableAsFullNameInclude()
    {
        var sets = VmArchiveScriptLoader.LoadAll([
            new("engine.wad", ScriptArchive()),
            new("game.pk3", ScriptZip(("zscript.txt", "target")), DeletedGlobalLumps: ["ZSCRIPT"]),
            new("mod.wad", ScriptArchive(("ZSCRIPT", "#include \"zscript.txt\"")))]);
        Assert.Equal("target", Assert.Single(sets).Sources[1].Text);
    }

    [Fact]
    public void DirectoryDeletionKeepsFullPathIncludes()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-delete-dir-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "zscript.zs"), "#include \"CHILD\"\n#include \"child.txt\"");
            File.WriteAllText(Path.Combine(root, "child.txt"), "directory");
            var sets = VmArchiveScriptLoader.LoadAll([
                new("engine.wad", ScriptArchive(("CHILD", "engine"))),
                new(root, [], VmScriptArchiveFormat.Directory, ["CHILD"])]);
            Assert.Equal(new[] { "engine", "directory" }, Assert.Single(sets).Sources.Skip(1).Select(source => source.Text));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void DeleteLumpsRemovesOnlyTargetWadGlobalShortNames()
    {
        var bytes = ScriptArchive(("ZSCRIPT", "version 4.13"));
        Assert.Empty(VmArchiveScriptLoader.LoadAll([new("game.wad", bytes, DeletedGlobalLumps: ["ZSCRIPT"])]));
        Assert.Single(VmArchiveScriptLoader.LoadAll([new("game.wad", bytes, DeletedGlobalLumps: ["zscript"])]));
        Assert.Equal(bytes, ScriptArchive(("ZSCRIPT", "version 4.13")));
        Assert.Single(VmArchiveScriptLoader.LoadAll([
            new("game.wad", bytes, DeletedGlobalLumps: ["ZSCRIPT"]), new("mod.wad", bytes)]));
        Assert.Empty(VmArchiveScriptLoader.LoadAll([
            new("game.pk3", ScriptZip(("zscript", "")), DeletedGlobalLumps: ["ZSCRIPT"])]));
    }

    [Fact]
    public void DeletedWadIncludeFallsBackToEarlierArchive()
    {
        var sets = VmArchiveScriptLoader.LoadAll([
            new("engine.wad", ScriptArchive(("CHILD", "engine"))),
            new("game.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD\""), ("CHILD", "game")), DeletedGlobalLumps: ["CHILD"])]);
        Assert.Equal("engine", Assert.Single(sets).Sources[1].Text);
        Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([
            new("engine.wad", ScriptArchive()),
            new("game.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD\""), ("CHILD", "game")), DeletedGlobalLumps: ["CHILD"])]));
    }

    [Fact]
    public void CatalogDeleteLumpsTargetsRequiredAndSelectedIwadButKeepsSupportAndMods()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-delete-start-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var game = Path.Combine(root, "game.wad");
            var catalog = Path.Combine(root, "iwadinfo.txt");
            var mod = Path.Combine(root, "mod.wad");
            File.WriteAllText(catalog, """
                IWAD { Name = Expansion MustContain = EXPAND Required = Base SupportWAD = support.wad DeleteLumps = ZSCRIPT }
                IWAD { Name = Base MustContain = BASE }
                Names { base.wad }
                """);
            File.WriteAllBytes(game, ScriptArchive(("EXPAND", ""), ("ZSCRIPT", "version 4.13")));
            File.WriteAllBytes(Path.Combine(root, "base.wad"), ScriptArchive(("BASE", ""), ("ZSCRIPT", "version 4.12")));
            File.WriteAllBytes(Path.Combine(root, "support.wad"), ScriptArchive(("ZSCRIPT", "version 4.11")));
            File.WriteAllBytes(mod, ScriptArchive(("ZSCRIPT", "version 4.10")));
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(new LauncherSelection
                { IwadPath = game, IwadInfoPath = catalog, ScriptPaths = [mod] }, demo.Simulation);
            Assert.Equal(new[] { new Version(4, 11, 0), new Version(4, 10, 0) }, host.VmScriptSources.Select(set => set.Version));
            Assert.Equal(new[] { 1, 4 }, host.VmScriptSources.Select(set => set.Sources[0].ContainerIndex));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void OptionalSupportWadLoadsBeforeIwadAndCanBeDisabled()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-support-start-" + Guid.NewGuid().ToString("N"));
        var searchVariable = "HCDE_SUPPORT_" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(root);
        try
        {
            var iwad = Path.Combine(root, "game.wad");
            var catalog = Path.Combine(root, "iwadinfo.txt");
            var library = Directory.CreateDirectory(Path.Combine(root, "library", "nested")).FullName;
            var support = Path.Combine(library, "support.wad");
            File.WriteAllText(catalog, "IWAD { Name = Test MustContain = MAP01 SupportWAD = support.wad }");
            File.WriteAllBytes(iwad, ScriptArchive(("MAP01", ""), ("ZSCRIPT", "version 4.13"), ("CHILD", "game")));
            using var demo = ClientHost.DemoRoom();
            Environment.SetEnvironmentVariable(searchVariable, Path.GetDirectoryName(library)!);
            var args = new[] { "--iwad", iwad, "--iwad-info", catalog, "--iwad-recursive-dir", "$" + searchVariable };
            Assert.True(LauncherPlan.TryParse(args, out var selection, out _));
            using var missing = LauncherPlan.CreateHost(selection, demo.Simulation);
            Assert.Single(missing.VmScriptSources);
            File.WriteAllBytes(support, ScriptArchive(("ZSCRIPT", "version 4.12\n#include \"CHILD\""), ("CHILD", "support")));
            using var present = LauncherPlan.CreateHost(selection, demo.Simulation);
            Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0) }, present.VmScriptSources.Select(set => set.Version));
            Assert.Equal(new[] { 1, 2 }, present.VmScriptSources.Select(set => set.Sources[0].ContainerIndex));
            Assert.Equal("game", present.VmScriptSources[0].Sources[1].Text);
            Assert.True(LauncherPlan.TryParse([.. args, "--no-support-wad"], out var disabled, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(disabled), out var copy, out _));
            Assert.False(copy.LoadSupportWad);
            using var disabledHost = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Single(disabledHost.VmScriptSources);
            using var explicitHost = LauncherPlan.CreateHost(new LauncherSelection
                { IwadPath = iwad, IwadInfoPath = catalog, LoadSupportWad = false, ScriptPaths = [support] }, demo.Simulation);
            Assert.Equal(new[] { new Version(4, 13, 0), new Version(4, 12, 0) }, explicitHost.VmScriptSources.Select(set => set.Version));
        }
        finally
        {
            Environment.SetEnvironmentVariable(searchVariable, null);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void RequiredIwadIsIdentifiedAndLoadedBeforeDependentGame()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-required-start-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var iwad = Path.Combine(root, "expansion.wad");
            var baseWad = Path.Combine(root, "base.wad");
            var catalogPath = Path.Combine(root, "iwadinfo.txt");
            File.WriteAllText(catalogPath, """
                IWAD { Name = Expansion MustContain = EXPAND Required = "Base Game" }
                IWAD { Name = "Base Game" MustContain = BASE }
                Names { missing.wad invalid.wad base.wad }
                """);
            File.WriteAllBytes(Path.Combine(root, "invalid.wad"), [0]);
            File.WriteAllBytes(baseWad, ScriptArchive(("BASE", ""), ("ZSCRIPT", "version 4.12"), ("CHILD", "base")));
            File.WriteAllBytes(iwad, ScriptArchive(("EXPAND", ""), ("ZSCRIPT", "version 4.13\n#include \"CHILD\""), ("CHILD", "expansion")));
            Assert.True(LauncherPlan.TryParse(["--iwad", iwad, "--iwad-info", catalogPath], out var selection, out _));
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(selection, demo.Simulation);
            Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0) }, host.VmScriptSources.Select(set => set.Version));
            Assert.Equal(new[] { 1, 2 }, host.VmScriptSources.Select(set => set.Sources[0].ContainerIndex));
            Assert.Equal("expansion", host.VmScriptSources[1].Sources[1].Text);
            var moved = Path.Combine(root, "renamed.data");
            File.Move(baseWad, moved);
            Assert.Throws<InvalidDataException>(() => LauncherPlan.CreateHost(selection, demo.Simulation));
            Assert.True(LauncherPlan.TryParse(["--iwad", iwad, "--iwad-info", catalogPath, "--required-iwad", moved], out var explicitSelection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(explicitSelection), out var copy, out _));
            Assert.Equal(moved, copy.RequiredIwadPath);
            using var explicitHost = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Equal(2, explicitHost.VmScriptSources.Count);
            var catalog = VmIwadInfoParser.Parse(File.ReadAllText(catalogPath));
            Assert.Throws<InvalidDataException>(() => VmIwadRequiredResource.ResolveWad(catalog.Entries[0], catalog, iwad, iwad));
            Assert.Throws<InvalidDataException>(() => VmIwadRequiredResource.ResolveWad(catalog.Entries[1], catalog, moved, iwad));
            Assert.Throws<FileNotFoundException>(() => VmIwadRequiredResource.ResolveWad(catalog.Entries[0], catalog, iwad, baseWad));
            var wrongCase = VmIwadInfoParser.Parse(File.ReadAllText(catalogPath).Replace("Required = \"Base Game\"", "Required = \"base game\""));
            Assert.Throws<InvalidDataException>(() => VmIwadRequiredResource.ResolveWad(wrongCase.Entries[0], wrongCase, iwad, moved));
            Assert.False(LauncherPlan.TryParse(["--iwad", iwad, "--required-iwad", moved], out _, out var error));
            Assert.Equal("required-iwad-path", error);
            Assert.False(LauncherPlan.TryParse(["--iwad", iwad, "--required-iwad"], out _, out error));
            Assert.Equal("required-iwad-path", error);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void CatalogCompanionsLoadAfterIwadBeforeModsAndOnlyOnce()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-companion-start-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var iwad = Path.Combine(root, "game.wad");
            var catalog = Path.Combine(root, "iwadinfo.txt");
            var library = Directory.CreateDirectory(Path.Combine(root, "library", "nested")).FullName;
            var companion = Path.Combine(library, "companion.pk3");
            var mod = Path.Combine(root, "mod.pk3");
            File.WriteAllText(catalog, "IWAD { Name = Test MustContain = MAP01 Load = \":companion.pk3\", \":companion.pk3\" }");
            File.WriteAllBytes(iwad, ScriptArchive(("MAP01", ""), ("ZSCRIPT", "version 4.11")));
            File.WriteAllBytes(companion, ScriptZip(("zscript", "version 4.12\n#include \"child.zs\""), ("child.zs", "companion")));
            File.WriteAllBytes(mod, ScriptZip(("zscript", "version 4.13"), ("child.zs", "mod")));
            var searchConfig = Path.Combine(root, "paths.ini");
            File.WriteAllText(searchConfig, "[FileSearch.Directories]\nRecursivePath=" + Path.GetDirectoryName(library)!);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(new LauncherSelection
                { IwadPath = iwad, IwadInfoPath = catalog,
                    SearchPathConfigPath = searchConfig,
                    ScriptPaths = [companion, mod] }, demo.Simulation);
            Assert.Equal(new[] { new Version(4, 11, 0), new Version(4, 12, 0), new Version(4, 13, 0) }, host.VmScriptSources.Select(set => set.Version));
            Assert.Equal(new[] { 1, 2, 3 }, host.VmScriptSources.Select(set => set.Sources[0].ContainerIndex));
            Assert.Equal("mod", host.VmScriptSources[1].Sources[1].Text);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void StartupIdentifiesIwadFromCatalogAndUsesItsFilters()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-iwadinfo-start-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllBytes(path, ScriptArchive(("MAP01", "")));
            File.WriteAllText(path + ".txt", "IWAD { Name = Test Game = Doom Autoname = doom.id.doom2 MustContain = MAP01 }");
            File.WriteAllBytes(path + ".pk3", ScriptZip(("zscript", "#include \"child.zs\""),
                ("child.zs", "base"), ("filter/doom.doom2/child.zs", "identified"),
                ("filter/custom/child.zs", "explicit")));
            Assert.True(LauncherPlan.TryParse(["--iwad", path, "--iwad-info", path + ".txt",
                "--script-file", path + ".pk3"], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            Assert.Equal(path + ".txt", copy.IwadInfoPath);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Equal("identified", Assert.Single(host.VmScriptSources).Sources[1].Text);
            using var overridden = LauncherPlan.CreateHost(new LauncherSelection
            {
                IwadPath = path, IwadInfoPath = path + ".txt", IwadFilter = "custom", ScriptPaths = [path + ".pk3"],
            }, demo.Simulation);
            Assert.Equal("explicit", Assert.Single(overridden.VmScriptSources).Sources[1].Text);
            File.WriteAllBytes(path, ScriptArchive(("E1M1", "")));
            Assert.Throws<InvalidDataException>(() => LauncherPlan.CreateHost(copy, demo.Simulation));
            Assert.Equal("identified", Assert.Single(host.VmScriptSources).Sources[1].Text);
            Assert.False(LauncherPlan.TryParse(["--iwad", path, "--iwad-info"], out _, out var error));
            Assert.Equal("iwad-info-path", error);
        }
        finally { File.Delete(path); File.Delete(path + ".txt"); File.Delete(path + ".pk3"); }
    }

    [Fact]
    public void IwadIdentificationReadsWadNamesIncludingNonGlobalNamespaces()
    {
        var definition = new VmIwadDefinition("test game", VmStartupGameType.Doom, "doom.id.doom2", ["MAP01", "SPECIAL"]);
        var wad = ScriptArchive(("MAP01", ""), ("S_START", ""), ("SPECIAL", "sprite data"), ("S_END", ""));
        Assert.Same(definition, VmIwadIdentifier.IdentifyWad([definition], wad));
        var filters = VmStartupGameFilters.Build(definition.Game, definition.AutoName);
        Assert.Equal("doom.doom2", filters[^1]);
        Assert.Throws<InvalidDataException>(() => VmIwadIdentifier.IdentifyWad([definition], new byte[12]));
        Assert.Throws<InvalidDataException>(() => VmIwadIdentifier.IdentifyWad([definition], wad[..^1]));
    }

    [Fact]
    public void StartupGeneratedGameFiltersApplyFamiliesAndLegacyIwadOverrides()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-game-start-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllBytes(path, ScriptArchive());
            File.WriteAllBytes(path + ".pk3", ScriptZip(("zscript", "#include \"child.zs\""),
                ("child.zs", "base"), ("filter/game-DoomStrifeChex/child.zs", "family"),
                ("filter/game-Doom/child.zs", "game"), ("filter/doom.id.doom2/child.zs", "iwad"),
                ("filter/doom.doom2/child.zs", "legacy"), ("filter/custom/child.zs", "custom")));
            Assert.True(LauncherPlan.TryParse(["--iwad", path, "--script-file", path + ".pk3",
                "--game-type", "DOOM", "--iwad-filter", "doom.id.doom2"], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            Assert.Equal(VmStartupGameType.Doom, copy.GameType);
            Assert.Equal("doom.id.doom2", copy.IwadFilter);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Equal("legacy", Assert.Single(host.VmScriptSources).Sources[1].Text);
            using var custom = LauncherPlan.CreateHost(new LauncherSelection
            {
                IwadPath = path, ScriptPaths = [path + ".pk3"], GameType = VmStartupGameType.Doom,
                IwadFilter = "doom.id.doom2", GameTypeFilters = ["custom"],
            }, demo.Simulation);
            Assert.Equal("custom", Assert.Single(custom.VmScriptSources).Sources[1].Text);
            using var family = LauncherPlan.CreateHost(new LauncherSelection
                { IwadPath = path, ScriptPaths = [path + ".pk3"], GameType = VmStartupGameType.Chex }, demo.Simulation);
            Assert.Equal("family", Assert.Single(family.VmScriptSources).Sources[1].Text);
        }
        finally { File.Delete(path); File.Delete(path + ".pk3"); }
    }

    [Fact]
    public void StartupResourceRulesSelectEmbeddedArchivesAndBlockRoots()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-resource-start-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllBytes(path, ScriptArchive());
            File.WriteAllBytes(path + ".pk3", BinaryScriptZip(
                ("zscript", Encoding.UTF8.GetBytes("version 4.12")),
                ("nested/scripts.pack", ScriptZip(("zscript", "version 4.13")))));
            Assert.True(LauncherPlan.TryParse(["--iwad", path, "--script-file", path + ".pk3",
                "--script-block", "zscript", "--script-embed", "NESTED/SCRIPTS.PACK"], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var parsed, out _));
            Assert.Equal(new[] { "zscript" }, parsed.BlockedScriptPatterns);
            Assert.Equal(new[] { "NESTED/SCRIPTS.PACK" }, parsed.EmbeddedScriptArchives);
            using var demo = ClientHost.DemoRoom();
            // Blocking applies to embedded containers too, before root discovery.
            using var blocked = LauncherPlan.CreateHost(parsed, demo.Simulation);
            Assert.Empty(blocked.VmScriptSources);
            using var embedded = LauncherPlan.CreateHost(new LauncherSelection
            {
                IwadPath = path, ScriptPaths = [path + ".pk3"],
                EmbeddedScriptArchives = parsed.EmbeddedScriptArchives,
            }, demo.Simulation);
            Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0) },
                embedded.VmScriptSources.Select(set => set.Version));
            using var ordinary = LauncherPlan.CreateHost(new LauncherSelection
                { IwadPath = path, ScriptPaths = [path + ".pk3"] }, demo.Simulation);
            Assert.Single(ordinary.VmScriptSources);
        }
        finally { File.Delete(path); File.Delete(path + ".pk3"); }
    }

    [Theory]
    [InlineData("--script-block", "script-block")]
    [InlineData("--script-embed", "script-embed")]
    public void StartupResourceRulesRejectMissingOrBlankValues(string option, string expectedError)
    {
        foreach (var suffix in new[] { Array.Empty<string>(), new[] { " " }, new[] { "--game-filters" } })
        {
            Assert.False(LauncherPlan.TryParse(["--iwad", "game.wad", option, .. suffix], out _, out var error));
            Assert.Equal(expectedError, error);
        }
        var invalid = option == "--script-block"
            ? new LauncherSelection { IwadPath = "game.wad", BlockedScriptPatterns = [""] }
            : new LauncherSelection { IwadPath = "game.wad", EmbeddedScriptArchives = [""] };
        Assert.False(LauncherPlan.TryValidate(invalid, out var validationError));
        Assert.Equal(expectedError, validationError);
    }

    [Fact]
    public void StartupFiltersRoundTripAndSelectArchiveOverrides()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-filter-start-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllBytes(path, ScriptArchive());
            File.WriteAllBytes(path + ".pk3", ScriptZip(("zscript", "#include \"child.zs\""),
                ("child.zs", "base"), ("filter/doom/child.zs", "doom"), ("filter/doom.id/child.zs", "specific")));
            Assert.True(LauncherPlan.TryParse(["--iwad", path, "--script-file", path + ".pk3", "--game-filter", "doom.id"], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var parsed, out _));
            Assert.Equal(new[] { "doom.id" }, parsed.GameTypeFilters);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(parsed, demo.Simulation);
            Assert.Equal("specific", Assert.Single(host.VmScriptSources).Sources[1].Text);
            Assert.True(LauncherPlan.TryParse(["--iwad", path, "--game-filters"], out var empty, out _));
            Assert.NotNull(empty.GameTypeFilters);
            Assert.Empty(empty.GameTypeFilters!);
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(empty), out var emptyCopy, out _));
            Assert.NotNull(emptyCopy.GameTypeFilters);
            Assert.True(LauncherPlan.TryParse(["--iwad", path], out var unfiltered, out _));
            Assert.Null(unfiltered.GameTypeFilters);
            Assert.False(LauncherPlan.TryParse(["--iwad", path, "--game-filter"], out _, out var error));
            Assert.Equal("game-filter", error);
        }
        finally { File.Delete(path); File.Delete(path + ".pk3"); }
    }

    [Fact]
    public void LauncherCoreArchiveOccupiesZeroAndRejectsOverriddenCoreIncludes()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-core-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var core = Path.Combine(root, "engine.pk3");
            var iwad = Path.Combine(root, "game.wad");
            var mod = Path.Combine(root, "mod.pk3");
            File.WriteAllBytes(core, ScriptZip(("zscript", "version 4.13\n#include \"CORE\""), ("core", "engine")));
            File.WriteAllBytes(iwad, ScriptArchive(("ZSCRIPT", "version 4.12")));
            File.WriteAllBytes(mod, ScriptZip(("core", "override")));
            Assert.True(LauncherPlan.TryParse(["--iwad", iwad, "--core-script-file", core], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            Assert.Equal(core, copy.CoreScriptPath);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Equal(2, host.VmScriptSources.Count);
            Assert.Equal(0, host.VmScriptSources[0].Sources[0].ContainerIndex);
            Assert.Equal("engine", host.VmScriptSources[0].Sources[1].Text);
            Assert.Equal(1, host.VmScriptSources[1].Sources[0].ContainerIndex);
            Assert.Throws<InvalidOperationException>(() => LauncherPlan.CreateHost(new LauncherSelection
                { IwadPath = iwad, CoreScriptPath = core, ScriptPaths = [mod] }, demo.Simulation));
            Assert.Equal(2, host.VmScriptSources.Count);
            Assert.False(LauncherPlan.TryParse(["--iwad", iwad, "--core-script-file"], out _, out var error));
            Assert.Equal("core-script-path", error);
            Assert.Throws<FileNotFoundException>(() => LauncherPlan.CreateHost(new LauncherSelection
                { IwadPath = iwad, CoreScriptPath = Path.Combine(root, "missing.pk3") }, demo.Simulation));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void LauncherLoadsIwadBeforeModsWithoutApplyingEngineOverrideGuard()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-iwad-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllBytes(path, ScriptArchive(("ZSCRIPT", "version 4.13\n#include \"CHILD\""), ("CHILD", "base")));
            File.WriteAllBytes(path + ".mod", ScriptArchive(("CHILD", "override")));
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(new LauncherSelection { IwadPath = path, ScriptPaths = [path + ".mod"] }, demo.Simulation);
            var set = Assert.Single(host.VmScriptSources);
            Assert.Equal(1, set.Sources[0].ContainerIndex);
            Assert.Equal("override", set.Sources[1].Text);
            Assert.Equal(2, set.Sources[1].ContainerIndex);
        }
        finally { File.Delete(path); File.Delete(path + ".mod"); }
    }

    [Fact]
    public void LauncherScriptInputsRoundTripAndLoadDuringHostCreation()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-start-" + Guid.NewGuid().ToString("N") + ".wad");
        var iwad = path + ".game.wad";
        try
        {
            File.WriteAllBytes(iwad, ScriptArchive());
            File.WriteAllBytes(path, ScriptArchive(("ZSCRIPT", "version 4.13")));
            Assert.True(LauncherPlan.TryParse(["--iwad", iwad, "--script-file", path], out var selection, out _));
            Assert.True(LauncherPlan.TryParse(LauncherPlan.BuildArgs(selection), out var copy, out _));
            Assert.Equal(selection.ScriptPaths, copy.ScriptPaths);
            using var demo = ClientHost.DemoRoom();
            using var host = LauncherPlan.CreateHost(copy, demo.Simulation);
            Assert.Equal(new Version(4, 13, 0), Assert.Single(host.VmScriptSources).Version);
            File.WriteAllBytes(path, ScriptArchive(("ZSCRIPT", "#include \"missing\"")));
            Assert.Throws<VmScriptIncludeException>(() => LauncherPlan.CreateHost(copy, demo.Simulation));
            demo.Tick(default);
            Assert.Equal(new Version(4, 13, 0), Assert.Single(host.VmScriptSources).Version);
            Assert.False(LauncherPlan.TryParse(["--iwad", "core.wad", "--script-file"], out _, out var error));
            Assert.Equal("script-path", error);
        }
        finally { File.Delete(path); File.Delete(iwad); }
    }

    [Fact]
    public void ScriptPathsDispatchMixedFilesAndDirectoriesInCallerOrder()
    {
        var root = Path.Combine(Path.GetTempPath(), "hcde-paths-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "folder"));
        try
        {
            var wad = Path.Combine(root, "core.wad");
            File.WriteAllBytes(wad, ScriptArchive(("ZSCRIPT", "version 4.13")));
            var zip = Path.Combine(root, "mod.pk3");
            File.WriteAllBytes(zip, ScriptZip(("zscript", "version 4.12")));
            File.WriteAllText(Path.Combine(root, "folder", "zscript"), "version 4.11");
            Assert.Equal(new[] { new Version(4, 13, 0), new Version(4, 12, 0), new Version(4, 11, 0) },
                VmArchiveScriptLoader.LoadPaths([wad, zip, Path.Combine(root, "folder")]).Select(set => set.Version));
            Assert.Throws<FileNotFoundException>(() => VmArchiveScriptLoader.LoadPaths([Path.Combine(root, "missing.wad")]));
            Assert.Throws<ArgumentException>(() => VmArchiveScriptLoader.LoadPaths([""]));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void RealLzmaZipScriptsAndZipCryptoExclusionMatchNative()
    {
        byte[] Fixture(string name)
        {
            using var input = typeof(VmArchiveTests).Assembly.GetManifestResourceStream("HCDE.Client.Tests.Fixtures." + name)!;
            using var output = new MemoryStream();
            input.CopyTo(output);
            return output.ToArray();
        }
        var compressed = Fixture("scripts-lzma.zip");
        Assert.Equal("lzma child", Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.pk3", compressed)])).Sources[1].Text);
        var warnings = new List<string>();
        Assert.Empty(VmArchiveScriptLoader.LoadAll([new("encrypted.pk3", Fixture("scripts-encrypted.zip"))], archiveWarning: warnings.Add));
        Assert.Equal(2, warnings.Count);
        Assert.All(warnings, warning => Assert.Contains("encrypted", warning));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZipUnsupportedOrEncryptedEntriesWarnAndAreExcluded(bool encrypted)
    {
        var bytes = ScriptZip(("zscript", "version 4.13"));
        var central = bytes.AsSpan().IndexOf(new byte[] {80, 75, 1, 2});
        Assert.True(central > 0);
        if (encrypted)
        {
            bytes[6] |= 1;
            bytes[central + 8] |= 1;
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8), 9);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(central + 10), 9);
        }
        var warnings = new List<string>();
        Assert.Empty(VmPk3ScriptLoader.LoadAll([new("core.pk3", bytes)], archiveWarning: warnings.Add));
        Assert.Contains(encrypted ? "encrypted" : "unsupported", Assert.Single(warnings));
        Assert.Throws<InvalidOperationException>(() => VmArchiveScriptLoader.LoadAll([new("core.pk3", bytes)],
            archiveWarning: _ => throw new InvalidOperationException("warning")));
    }

    [Fact]
    public void Pk3Bzip2ScriptsResolveNestedIncludes()
    {
        using var fixture = typeof(VmArchiveTests).Assembly.GetManifestResourceStream("HCDE.Client.Tests.Fixtures.scripts-bzip2.zip")!;
        using var output = new MemoryStream();
        fixture.CopyTo(output);
        var bytes = output.ToArray();
        var set = Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.pk3", bytes)]));
        Assert.Equal("bzip2 child", set.Sources[1].Text);
        Assert.Empty(VmArchiveScriptLoader.LoadAll([new("core.pk3", bytes)], blockedPatterns: ["zscript"]));
    }

    [Fact]
    public void SevenZipEncryptedHeadersFailWithoutPublishingSources()
    {
        using var fixture = typeof(VmArchiveTests).Assembly.GetManifestResourceStream("HCDE.Client.Tests.Fixtures.scripts-encrypted.7z")!;
        using var bytes = new MemoryStream();
        fixture.CopyTo(bytes);
        Assert.ThrowsAny<Exception>(() => VmArchiveScriptLoader.LoadAll([new("encrypted.7z", bytes.ToArray())]));
    }

    [Fact]
    public void SevenZipRejectsValidlyDecodedPayloadWithWrongChecksum()
    {
        using var output = new MemoryStream();
        var source = Encoding.ASCII.GetBytes("version 4.13");
        using (var writer = new SharpCompress.Writers.SevenZip.SevenZipWriter(output,
            new SharpCompress.Writers.SevenZip.SevenZipWriterOptions(SharpCompress.Common.CompressionType.LZMA2)))
        {
            using var input = new MemoryStream(source);
            writer.Write("zscript", input, null);
        }
        var bytes = output.ToArray();
        var position = bytes.AsSpan().IndexOf(source);
        Assert.True(position >= 32);
        bytes[position + source.Length - 1] = (byte)'2';
        Assert.ThrowsAny<Exception>(() => VmArchiveScriptLoader.LoadAll([new("bad.7z", bytes)]));
        Assert.ThrowsAny<Exception>(() => VmArchiveScriptLoader.LoadAll([new("bad.7z", bytes)], blockedPatterns: ["zscript"]));
    }

    [Fact]
    public void SolidSevenZipResolvesBackwardIncludesAcrossMultipleRoots()
    {
        using var fixture = typeof(VmArchiveTests).Assembly.GetManifestResourceStream("HCDE.Client.Tests.Fixtures.scripts-solid.7z")!;
        using var output = new MemoryStream();
        fixture.CopyTo(output);
        var bytes = output.ToArray();
        var sets = VmArchiveScriptLoader.LoadAll([new("solid.7z", bytes)]);
        Assert.Equal(new[] { new Version(4, 13, 0), new Version(4, 12, 0) }, sets.Select(set => set.Version));
        Assert.Equal(new[] { "c", "a", "b" }, sets[0].Sources.Skip(1).Select(source => source.Text));
        Assert.Equal("a", sets[1].Sources[1].Text);
        Assert.Equal(2, VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.7z", bytes)))], embeddings: ["inner.7z"]).Count);
    }

    [Fact]
    public void AutomaticArchiveDispatchLoadsScriptsAndPreservesShortSignatureFallback()
    {
        Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.wad", ScriptArchive(("ZSCRIPT", "version 4.13")))]));
        Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.pk3", ScriptZip(("zscript", "version 4.13")))]));
        Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.grp", ScriptGrp(("ZSCRIPT", "version 4.13")))]));
        Assert.Equal("PACK", Assert.Single(VmArchiveScriptLoader.LoadAll([new("zscript", "PACK"u8.ToArray())])).Sources[0].Text);
        Assert.Empty(VmArchiveScriptLoader.LoadAll([new("empty.pk3", ScriptZip())]));
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("core.pak", "PACK"u8.ToArray(), VmScriptArchiveFormat.Pak)]));
    }

    [Fact]
    public void SevenZipDoesNotDecodeUnusedDamagedPayloads()
    {
        using var output = new MemoryStream();
        var marker = Encoding.ASCII.GetBytes("unused_unique_payload");
        using (var writer = new SharpCompress.Writers.SevenZip.SevenZipWriter(output,
            new SharpCompress.Writers.SevenZip.SevenZipWriterOptions(SharpCompress.Common.CompressionType.LZMA2)))
        {
            using var root = new MemoryStream(Encoding.UTF8.GetBytes("version 4.13"));
            writer.Write("zscript", root, null);
            using var unused = new MemoryStream(marker);
            writer.Write("unused.bin", unused, null);
        }
        var bytes = output.ToArray();
        var position = bytes.AsSpan().IndexOf(marker);
        Assert.True(position >= 35);
        bytes[position - 3] = 255;
        var archive = new VmScriptArchive("core.7z", bytes, VmScriptArchiveFormat.SevenZip);
        Assert.Single(VmArchiveScriptLoader.LoadAll([archive]));
        Assert.Empty(VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: ["zscript"]));
        Assert.ThrowsAny<Exception>(() => VmArchiveScriptLoader.LoadAll([archive], embeddings: ["unused.bin"]));
    }

    [Fact]
    public void SevenZipScriptsDecodeCompressedEntriesAndApplyFilters()
    {
        using var output = new MemoryStream();
        using (var writer = new SharpCompress.Writers.SevenZip.SevenZipWriter(output,
            new SharpCompress.Writers.SevenZip.SevenZipWriterOptions(SharpCompress.Common.CompressionType.LZMA2)))
        {
            writer.WriteDirectory("scripts", null);
            foreach (var file in new[] { ("zscript", "version 4.13\n#include \"scripts/child.zs\""),
                ("scripts/child.zs", "base"), ("filter/doom/scripts/child.zs", "selected") })
            {
                using var input = new MemoryStream(Encoding.UTF8.GetBytes(file.Item2));
                writer.Write(file.Item1, input, null);
            }
        }
        var bytes = output.ToArray();
        var archive = new VmScriptArchive("core.7z", bytes, VmScriptArchiveFormat.SevenZip);
        Assert.Equal("base", Assert.Single(VmArchiveScriptLoader.LoadAll([archive])).Sources[1].Text);
        Assert.Equal("selected", Assert.Single(VmArchiveScriptLoader.LoadAll([archive], gameTypeFilters: ["doom"])).Sources[1].Text);
        Assert.Equal("base", Assert.Single(VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.7z", bytes)))], embeddings: ["inner.7z"])).Sources[1].Text);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([archive with { Data = bytes[..20] }]));
    }

    [Fact]
    public void DirectoryFilenameChecksReceiveLowercaseLeafAndPhysicalPathBeforeReads()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(path, "scripts"));
        try
        {
            File.WriteAllText(Path.Combine(path, "ZSCRIPT"), "#include \"scripts/child.zs\"");
            var child = Path.Combine(path, "scripts", "CHILD.ZS");
            File.WriteAllText(child, "found");
            File.WriteAllText(Path.Combine(path, "ZSCRIPT.BAK"), "unused");
            var calls = new List<(string Name, string Path)>();
            var archive = new VmScriptArchive(path, [], VmScriptArchiveFormat.Directory);
            var sets = VmArchiveScriptLoader.LoadAll([archive], directoryFilenameCheck: (name, file) =>
            {
                calls.Add((name, file));
                return true;
            });
            Assert.Equal("found", Assert.Single(sets).Sources[1].Text);
            Assert.Contains(("child.zs", child), calls);
            Assert.DoesNotContain(calls, call => call.Name.Contains(".bak"));
            Assert.Empty(VmArchiveScriptLoader.LoadAll([archive], directoryFilenameCheck: (_, _) => false));
            Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], directoryFilenameCheck: (name, _) => name != "child.zs"));
            Assert.Throws<InvalidOperationException>(() => VmArchiveScriptLoader.LoadAll([archive], directoryFilenameCheck: (_, _) => throw new InvalidOperationException("callback")));
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void WhresIncludesUseNumberedPathsAndAlignedPayloads()
    {
        var bytes = new byte[12288];
        "first"u8.CopyTo(bytes);
        "second"u8.CopyTo(bytes.AsSpan(4096));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8196), 5);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8204), 1);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8208), 6);
        var sources = VmArchiveScriptLoader.LoadAll([
            new("core.pk3", ScriptZip(), VmScriptArchiveFormat.Pk3),
            new("mod.pk3", ScriptZip(("zscript", "#include \"sounds/0000\"\n#include \"sounds/0001\"")), VmScriptArchiveFormat.Pk3),
            new("folder/SOUNDS.RES", bytes, VmScriptArchiveFormat.Whres)]);
        Assert.Equal(new[] { "first", "second" }, Assert.Single(sources).Sources.Skip(1).Select(source => source.Text));
        Assert.Empty(VmArchiveScriptLoader.LoadAll([new("sounds.res", bytes, VmScriptArchiveFormat.Whres)]));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8204), 2);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("sounds.res", bytes, VmScriptArchiveFormat.Whres)]));
    }

    [Fact]
    public void WhresRejectsPayloadsThatNativeDetectionWouldAcceptOutsideDataArea()
    {
        var bytes = new byte[16384];
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12292), 8192);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12300), 2);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12304), 6000);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("sounds.res", bytes, VmScriptArchiveFormat.Whres)]));
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("sounds.res", new byte[8191], VmScriptArchiveFormat.Whres)]));
    }

    [Fact]
    public void DirectoryLoaderOpensOnlyRequestedSources()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-lazy-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        try
        {
            File.WriteAllText(Path.Combine(path, "zscript"), "version 4.13");
            var unused = Path.Combine(path, "unused.bin");
            File.WriteAllBytes(unused, [255]);
            using (var locked = new FileStream(unused, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var archive = new VmScriptArchive(path, [], VmScriptArchiveFormat.Directory);
                Assert.Single(VmArchiveScriptLoader.LoadAll([archive]));
                File.WriteAllText(Path.Combine(path, "zscript"), "#include \"unused.bin\"");
                Assert.Throws<IOException>(() => VmArchiveScriptLoader.LoadAll([archive]));
                Assert.Empty(VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: ["zscript"]));
            }
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void DirectoryScriptsResolvePathsFiltersAndExcludeBackupFiles()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-script-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(path, "scripts"));
        Directory.CreateDirectory(Path.Combine(path, "filter", "doom"));
        try
        {
            File.WriteAllText(Path.Combine(path, "zscript"), "version 4.13\n#include \"scripts/child.zs\"");
            File.WriteAllText(Path.Combine(path, "scripts", "child.zs"), "base");
            File.WriteAllText(Path.Combine(path, "filter", "doom", "child.zs"), "filtered");
            File.WriteAllText(Path.Combine(path, "zscript.bak"), "invalid root");
            File.WriteAllText(Path.Combine(path, "zscript.cache.txt"), "invalid root");
            File.WriteAllText(Path.Combine(path, "zscript.orig"), "invalid root");
            var archive = new VmScriptArchive(path, [], VmScriptArchiveFormat.Directory);
            var set = Assert.Single(VmArchiveScriptLoader.LoadAll([archive]));
            Assert.Equal("base", set.Sources[1].Text);
            File.WriteAllText(Path.Combine(path, "zscript"), "#include \"child.zs\"");
            var filtered = Assert.Single(VmArchiveScriptLoader.LoadAll([archive], gameTypeFilters: ["doom"]));
            Assert.Equal("filtered", filtered.Sources[1].Text);
            Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], gameTypeFilters: []));
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
        Assert.Throws<DirectoryNotFoundException>(() => VmArchiveScriptLoader.LoadAll([new(path, [], VmScriptArchiveFormat.Directory)]));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void SsiScriptsExposeExtensionAliasesAndEmbeddedGrp(int version)
    {
        var ssi = ScriptSsi(version, ("ZSCRIPT.ZSC", Encoding.UTF8.GetBytes("version 4.13\n#include \"CHILD.ZSC\"")),
            ("CHILD.CSZ", Encoding.UTF8.GetBytes("found")));
        var sets = VmArchiveScriptLoader.LoadAll([new("core.ssi", ssi, VmScriptArchiveFormat.Ssi)]);
        Assert.Equal(2, sets.Count);
        Assert.All(sets, set => Assert.Equal("found", set.Sources[1].Text));
        var embedded = ScriptSsi(version, ("INNER.PRG", ScriptGrp(("ZSCRIPT", "version 4.13"))));
        Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.ssi", embedded, VmScriptArchiveFormat.Ssi)]));
        Assert.Equal(2, VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.ssi", ssi)))], embeddings: ["inner.ssi"]).Count);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.ssi", ssi[..^1], VmScriptArchiveFormat.Ssi)]));
        ssi[9] = 1;
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.ssi", ssi, VmScriptArchiveFormat.Ssi)]));
    }

    private static byte[] ScriptSsi(int version, params (string Name, byte[] Data)[] files)
    {
        var headerSize = version == 2 ? 267 : 254;
        var position = headerSize + files.Length * 121;
        var bytes = new byte[position + files.Sum(file => file.Data.Length)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, version);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), files.Length);
        for (var index = 0; index < files.Length; index++)
        {
            var entry = bytes.AsSpan(headerSize + index * 121, 121);
            entry[0] = (byte)files[index].Name.Length;
            Encoding.ASCII.GetBytes(files[index].Name).CopyTo(entry[1..]);
            BinaryPrimitives.WriteInt32LittleEndian(entry[13..], files[index].Data.Length);
            files[index].Data.CopyTo(bytes, position);
            position += files[index].Data.Length;
        }
        return bytes;
    }

    [Fact]
    public void MvlScriptsPreserveDirectoryOrderAndResolveIncludes()
    {
        var mvl = ScriptMvl(("zscript.zs", "version 4.13\n#include \"CHILD\""),
            ("child.txt", "found"), ("zscript.txt", "version 4.12"));
        var original = mvl.ToArray();
        var sets = VmArchiveScriptLoader.LoadAll([new("core.mvl", mvl, VmScriptArchiveFormat.Mvl)]);
        Assert.Equal(new[] { new Version(4, 13, 0), new Version(4, 12, 0) }, sets.Select(set => set.Version));
        Assert.Equal("found", sets[0].Sources[1].Text);
        Assert.Equal(original, mvl);
        Assert.Equal(2, VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.mvl", mvl)))], embeddings: ["inner.mvl"]).Count);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.mvl", mvl[..^1], VmScriptArchiveFormat.Mvl)]));
        BinaryPrimitives.WriteUInt32LittleEndian(mvl.AsSpan(4), uint.MaxValue);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.mvl", mvl, VmScriptArchiveFormat.Mvl)]));
    }

    [Fact]
    public void MvlUsesTwelveNameBytesAndRejectsOverflowingPayloadSizes()
    {
        var mvl = ScriptMvl(("zscript.txtXY", "version 4.13"));
        var set = Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.mvl", mvl, VmScriptArchiveFormat.Mvl)]));
        Assert.Equal("core.mvl:zscript.txtx", set.Sources[0].Name);
        BinaryPrimitives.WriteUInt32LittleEndian(mvl.AsSpan(21), uint.MaxValue);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.mvl", mvl, VmScriptArchiveFormat.Mvl)]));
    }

    private static byte[] ScriptMvl(params (string Name, string Text)[] files)
    {
        var payloads = files.Select(file => Encoding.UTF8.GetBytes(file.Text)).ToArray();
        var position = 8 + files.Length * 17;
        var bytes = new byte[position + payloads.Sum(payload => payload.Length)];
        "DMVL"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), files.Length);
        for (var index = 0; index < files.Length; index++)
        {
            var entry = bytes.AsSpan(8 + index * 17, 17);
            Encoding.ASCII.GetBytes(files[index].Name).CopyTo(entry);
            BinaryPrimitives.WriteInt32LittleEndian(entry[13..], payloads[index].Length);
            payloads[index].CopyTo(bytes, position);
            position += payloads[index].Length;
        }
        return bytes;
    }

    [Fact]
    public void HogScriptsPreserveRecordOrderAndResolveIncludes()
    {
        var hog = ScriptHog(("zscript.zs", "version 4.13\n#include \"CHILD\""),
            ("child.txt", "found"), ("zscript.txt", "version 4.12"));
        var sets = VmArchiveScriptLoader.LoadAll([new("core.hog", hog, VmScriptArchiveFormat.Hog)]);
        Assert.Equal(new[] { new Version(4, 13, 0), new Version(4, 12, 0) }, sets.Select(set => set.Version));
        Assert.Equal("found", sets[0].Sources[1].Text);
        Assert.Equal(2, VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.hog", hog)))], embeddings: ["inner.hog"]).Count);
        Assert.Equal(2, VmArchiveScriptLoader.LoadAll([new("core.hog", hog.Concat(new byte[12]).ToArray(), VmScriptArchiveFormat.Hog)]).Count);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.hog", hog[..^1], VmScriptArchiveFormat.Hog)]));
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.hog", hog.Concat(new byte[13]).ToArray(), VmScriptArchiveFormat.Hog)]));
        BinaryPrimitives.WriteUInt32LittleEndian(hog.AsSpan(16), uint.MaxValue);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.hog", hog, VmScriptArchiveFormat.Hog)]));
    }

    private static byte[] ScriptHog(params (string Name, string Text)[] files)
    {
        using var output = new MemoryStream();
        output.Write("DHF"u8);
        foreach (var file in files)
        {
            var record = new byte[17];
            Encoding.ASCII.GetBytes(file.Name).CopyTo(record, 0);
            var payload = Encoding.UTF8.GetBytes(file.Text);
            BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(13), payload.Length);
            output.Write(record);
            output.Write(payload);
        }
        return output.ToArray();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RffScriptsDecryptDirectoryAndOnlyFirst256PayloadBytes(bool encrypted)
    {
        var source = "version 4.13\n" + new string(' ', 300);
        var payload = Encoding.UTF8.GetBytes(source);
        var directoryOffset = 16 + payload.Length;
        var bytes = new byte[directoryOffset + 48];
        "RFF\u001a"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8), directoryOffset);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12), 1);
        payload.CopyTo(bytes, 16);
        var entry = bytes.AsSpan(directoryOffset);
        BinaryPrimitives.WriteInt32LittleEndian(entry[16..], 16);
        BinaryPrimitives.WriteInt32LittleEndian(entry[20..], payload.Length);
        entry[32] = encrypted ? (byte)0x10 : (byte)0;
        "TXT"u8.CopyTo(entry[33..]);
        "ZSCRIPT"u8.CopyTo(entry[36..]);
        if (encrypted)
            for (var index = 0; index < Math.Min(payload.Length, 256); index++) bytes[16 + index] ^= (byte)(index >> 1);
        for (var index = 0; index < entry.Length; index++) entry[index] ^= unchecked((byte)(directoryOffset + (index >> 1)));
        var original = bytes.ToArray();
        var set = Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.rff", bytes, VmScriptArchiveFormat.Rff)]));
        Assert.Equal(source, Assert.Single(set.Sources).Text);
        Assert.Equal("core.rff:zscript.txt", set.Sources[0].Name);
        Assert.Equal(original, bytes);
        Assert.Single(VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.rff", bytes)))], embeddings: ["inner.rff"]));
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.rff", bytes[..^1], VmScriptArchiveFormat.Rff)]));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(12), uint.MaxValue);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.rff", bytes, VmScriptArchiveFormat.Rff)]));
    }

    [Fact]
    public void PakScriptsLoadDirectlyAndFromEmbeddedArchivesAndRejectInvalidBounds()
    {
        var payload = Encoding.UTF8.GetBytes("version 4.13");
        var pak = new byte[12 + payload.Length + 64];
        "PACK"u8.CopyTo(pak);
        BinaryPrimitives.WriteInt32LittleEndian(pak.AsSpan(4), 12 + payload.Length);
        BinaryPrimitives.WriteInt32LittleEndian(pak.AsSpan(8), 64);
        payload.CopyTo(pak, 12);
        var entry = pak.AsSpan(12 + payload.Length);
        "ZSCRIPT"u8.CopyTo(entry);
        BinaryPrimitives.WriteInt32LittleEndian(entry[56..], 12);
        BinaryPrimitives.WriteInt32LittleEndian(entry[60..], payload.Length);
        Assert.Equal(new Version(4, 13, 0), Assert.Single(VmArchiveScriptLoader.LoadAll([new("core.pak", pak, VmScriptArchiveFormat.Pak)])).Version);
        Assert.Single(VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.pak", pak)))], embeddings: ["inner.pak"]));
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.pak", pak[..^1], VmScriptArchiveFormat.Pak)]));
        BinaryPrimitives.WriteUInt32LittleEndian(entry[60..], uint.MaxValue);
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.pak", pak, VmScriptArchiveFormat.Pak)]));
    }

    [Fact]
    public void GrpScriptsPreserveDirectoryOrderAndResolveFullAndShortNames()
    {
        var grp = ScriptGrp(("ZSCRIPT.TXT", "version 4.12\n#include \"CHILD\""), ("CHILD.ZS", "found"), ("ZSCRIPT.ZS", "version 4.13"));
        var sets = VmArchiveScriptLoader.LoadAll([new("core.grp", grp, VmScriptArchiveFormat.Grp)]);
        Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0) }, sets.Select(set => set.Version));
        Assert.Equal("core.grp:child.zs", sets[0].Sources[1].Name);
        Assert.Equal("found", sets[0].Sources[1].Text);
        Assert.Equal(2, VmPk3ScriptLoader.LoadAll([new("outer.pk3", BinaryScriptZip(("inner.grp", grp)))], embeddings: ["inner.grp"]).Count);
    }

    [Fact]
    public void GrpReaderRejectsTruncatedDirectoriesAndPayloads()
    {
        var grp = ScriptGrp(("ZSCRIPT", "version 4.13"));
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.grp", grp[..20], VmScriptArchiveFormat.Grp)]));
        Assert.Throws<InvalidDataException>(() => VmArchiveScriptLoader.LoadAll([new("bad.grp", grp[..^1], VmScriptArchiveFormat.Grp)]));
    }

    private static byte[] ScriptGrp(params (string Name, string Text)[] files)
    {
        var payloads = files.Select(file => Encoding.UTF8.GetBytes(file.Text)).ToArray();
        var bytes = new byte[16 + files.Length * 16 + payloads.Sum(data => data.Length)];
        "KenSilverman"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12), files.Length);
        var position = 16 + files.Length * 16;
        for (var index = 0; index < files.Length; index++)
        {
            Encoding.ASCII.GetBytes(files[index].Name).CopyTo(bytes, 16 + index * 16);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(28 + index * 16), payloads[index].Length);
            payloads[index].CopyTo(bytes, position);
            position += payloads[index].Length;
        }
        return bytes;
    }

    [Fact]
    public void FullPathQueriesFoldAsciiWithoutUnicodeCaseMatching()
    {
        var good = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"SCRIPTS/\u00e9.ZS\""),
            ("scripts/\u00e9.zs", "found")));
        Assert.Equal("found", Assert.Single(VmPk3ScriptLoader.LoadAll([good])).Sources[1].Text);
        var wrongCase = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"scripts/\u00c9.zs\""),
            ("scripts/\u00e9.zs", "")));
        Assert.Equal("scripts/\u00c9.zs", Assert.Single(Assert.Throws<VmScriptIncludeException>(() => VmPk3ScriptLoader.LoadAll([wrongCase])).Errors).Path);
    }

    [Fact]
    public void FullPathQueriesDoNotComposeUnicodeInput()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"scripts/e\u0301.zs\""),
            ("scripts/\u00e9.zs", "")));
        Assert.Throws<VmScriptIncludeException>(() => VmPk3ScriptLoader.LoadAll([archive]));
    }

    [Fact]
    public void ExplicitEmbeddedTextFallsBackToRawLumpScriptRoot()
    {
        var archive = new VmScriptPk3("outer.pk3", BinaryScriptZip(("scripts/zscript.txt", Encoding.UTF8.GetBytes("version 4.13"))));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive], embeddings: ["scripts/zscript.txt"]));
        Assert.Equal(1, set.Sources[0].ContainerIndex);
        Assert.Equal("outer.pk3:scripts/zscript.txt:zscript.txt", set.Sources[0].Name);
        Assert.Equal(new Version(4, 13, 0), set.Version);
    }

    [Fact]
    public void EmbeddedRawFallbackParticipatesInSharedIncludeLookup()
    {
        var core = new VmScriptArchive("core.wad", ScriptArchive(), VmScriptArchiveFormat.Wad);
        var archive = new VmScriptArchive("outer.pk3", BinaryScriptZip(("zscript", Encoding.UTF8.GetBytes("#include \"child.txt\"")),
            ("scripts/child.txt", Encoding.UTF8.GetBytes("found"))), VmScriptArchiveFormat.Pk3);
        var set = Assert.Single(VmArchiveScriptLoader.LoadAll([core, archive], embeddings: ["scripts/child.txt"]));
        Assert.Equal("found", set.Sources[1].Text);
        Assert.Equal(2, set.Sources[1].ContainerIndex);
    }

    [Fact]
    public void RawLumpArchivesExposeFullNamesAndGlobalShortNames()
    {
        var core = new VmScriptArchive("core.wad", ScriptArchive(), VmScriptArchiveFormat.Wad);
        var root = new VmScriptArchive("C:\\scripts\\ZSCRIPT.txt", Encoding.UTF8.GetBytes("version 4.13\n#include \"child\""), VmScriptArchiveFormat.RawLump);
        var child = new VmScriptArchive("mods/child.zs", Encoding.UTF8.GetBytes("#include \"leaf.zs\""), VmScriptArchiveFormat.RawLump);
        var leaf = new VmScriptArchive("leaf.zs", Encoding.UTF8.GetBytes(""), VmScriptArchiveFormat.RawLump);
        var set = Assert.Single(VmArchiveScriptLoader.LoadAll([core, root, child, leaf]));
        Assert.Equal(new[] { 1, 2, 3 }, set.Sources.Select(source => source.ContainerIndex));
        Assert.Equal("C:\\scripts\\ZSCRIPT.txt:zscript.txt", set.Sources[0].Name);
        Assert.Equal("mods/child.zs:child.zs", set.Sources[1].Name);
        Assert.Equal(new Version(4, 13, 0), set.Version);
    }

    [Fact]
    public void RawLumpOverridesPreserveFullNamePrecedenceAndCoreProtection()
    {
        var root = new VmScriptArchive("zscript", Encoding.UTF8.GetBytes("#include \"child.zs\""), VmScriptArchiveFormat.RawLump);
        var child = new VmScriptArchive("child.zs", Encoding.UTF8.GetBytes(""), VmScriptArchiveFormat.RawLump);
        Assert.Throws<InvalidOperationException>(() => VmArchiveScriptLoader.LoadAll([root, child]));
        var empty = new VmScriptArchive("core.wad", ScriptArchive(), VmScriptArchiveFormat.Wad);
        var replacement = new VmScriptArchive("mods/child.zs", Encoding.UTF8.GetBytes("new"), VmScriptArchiveFormat.RawLump);
        Assert.Equal("new", Assert.Single(VmArchiveScriptLoader.LoadAll([empty, root, child, replacement])).Sources[1].Text);
        Assert.Empty(VmArchiveScriptLoader.LoadAll([new("binary.bin", new byte[] { 255 }, VmScriptArchiveFormat.RawLump)]));
    }

    [Fact]
    public void ExplicitEmbeddingsLoadNestedPk3AndWadInContainerOrder()
    {
        var inner = BinaryScriptZip(("zscript", Encoding.UTF8.GetBytes("version 4.13")),
            ("leaf.wad", ScriptArchive(("ZSCRIPT", "version 4.13.1"))));
        var outer = new VmScriptArchive("outer.pk3", BinaryScriptZip(("bundles/content.bin", inner)), VmScriptArchiveFormat.Pk3);
        Assert.Empty(VmArchiveScriptLoader.LoadAll([outer]));
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        host.LoadVmScriptArchives([outer], embeddings: ["BUNDLES/CONTENT.BIN"]);
        Assert.Equal(new[] { 1, 2 }, host.VmScriptSources.Select(set => set.Sources[0].ContainerIndex));
        Assert.Equal("outer.pk3:bundles/content.bin:leaf.wad:ZSCRIPT", host.VmScriptSources[1].Sources[0].Name);
    }

    [Fact]
    public void ExplicitEmbeddingNamesApplyAfterFiltersAndBlockedEntriesStayHidden()
    {
        var outer = new VmScriptPk3("outer.pk3", BinaryScriptZip(("filter/doom/nested.bin", ScriptArchive(("ZSCRIPT", "")))));
        Assert.Single(VmPk3ScriptLoader.LoadAll([outer], gameTypeFilters: ["doom"], embeddings: ["nested.bin"]));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([outer], gameTypeFilters: ["doom"], blockedPatterns: ["*/nested.bin"], embeddings: ["nested.bin"]));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([new("bad.pk3", BinaryScriptZip(("nested.bin", new byte[] { 1, 2, 3 })))], embeddings: ["nested.bin"]));
    }

    [Theory]
    [InlineData("project.pk3", "project/child.wad")]
    [InlineData("C:\\mods\\PROJECT.pk3", "project/child.wad")]
    [InlineData("mods/project.zip", "PROJECT/child.WAD")]
    public void ProjectFolderEmbeddedWadsLoadWithNativeNames(string archiveName, string entryName)
    {
        var archive = new VmScriptPk3(archiveName, BinaryScriptZip((entryName, ScriptArchive(("ZSCRIPT", "version 4.13")))));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive]));
        Assert.Equal(1, set.Sources[0].ContainerIndex);
        Assert.Equal(archiveName + ":" + entryName.ToLowerInvariant() + ":ZSCRIPT", set.Sources[0].Name);
    }

    [Theory]
    [InlineData("other/child.wad")]
    [InlineData("project/nested/child.wad")]
    [InlineData("child.wad.backup.wad")]
    public void UnrecognizedEmbeddedPathsDoNotExposeWadScriptRoots(string entryName)
    {
        var archive = new VmScriptPk3("project.pk3", BinaryScriptZip((entryName, ScriptArchive(("ZSCRIPT", "version 4.13")))));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([archive]));
    }

    [Fact]
    public void SelectedGameFilterCanExposeProjectFolderEmbeddedWad()
    {
        var archive = new VmScriptPk3("project.pk3", BinaryScriptZip(("filter/doom/project/child.wad", ScriptArchive(("ZSCRIPT", "version 4.13")))));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([archive]));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive], gameTypeFilters: ["doom"]));
        Assert.Equal("project.pk3:project/child.wad:ZSCRIPT", set.Sources[0].Name);
    }

    [Fact]
    public void EmbeddedWadRootsLoadAfterParentAndBeforeFollowingArchives()
    {
        var data = BinaryScriptZip(("zscript", Encoding.UTF8.GetBytes("version 4.12")),
            ("child.wad", ScriptArchive(("ZSCRIPT", "version 4.13"))));
        var pk3 = new VmScriptArchive("core.pk3", data, VmScriptArchiveFormat.Pk3);
        var later = new VmScriptArchive("last.wad", ScriptArchive(("ZSCRIPT", "version 4.13.1")), VmScriptArchiveFormat.Wad);
        var sets = VmArchiveScriptLoader.LoadAll([pk3, later]);
        Assert.Equal(new[] { 0, 1, 2 }, sets.Select(set => set.Sources[0].ContainerIndex));
        Assert.Equal("core.pk3:child.wad:ZSCRIPT", sets[1].Sources[0].Name);
        Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0), new Version(4, 13, 1) }, sets.Select(set => set.Version));
    }

    [Fact]
    public void EmbeddedWadsRetainNativeCoreContainerProtection()
    {
        var pk3 = new VmScriptPk3("core.pk3", BinaryScriptZip(("zscript", Encoding.UTF8.GetBytes("#include \"CHILD\"")),
            ("child.wad", ScriptArchive(("CHILD", "")))));
        Assert.Contains("overriding core lump", Assert.Throws<InvalidOperationException>(() => VmPk3ScriptLoader.LoadAll([pk3])).Message);
        Assert.Empty(VmPk3ScriptLoader.LoadAll([new("core.pk3", BinaryScriptZip(("bad.wad", new byte[] { 1, 2, 3 })))]));
    }

    private static byte[] BinaryScriptZip(params (string Name, byte[] Data)[] files)
    {
        using var bytes = new MemoryStream();
        using (var zip = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var file in files)
            {
                using var stream = zip.CreateEntry(file.Name).Open();
                stream.Write(file.Data);
            }
        return bytes.ToArray();
    }

    [Theory]
    [InlineData("child.(zs|txt)")]
    [InlineData("child.(txt|zs)")]
    [InlineData("child.((txt|zs)|bin)")]
    [InlineData("child.(|z)s")]
    public void ArchiveAlternativeBranchesMatchInNativeOrder(string pattern)
    {
        var archive = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "#include \"child.zs\""), ("child.zs", "")), VmScriptArchiveFormat.Pk3);
        Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: [pattern]));
    }

    [Theory]
    [InlineData("child.zs", "child.zs\\", true)]
    [InlineData("child.zsx", "child.zs\\", false)]
    [InlineData("child[", "child[", true)]
    [InlineData("child]", "child[]]", true)]
    [InlineData("child[", "child\\[", true)]
    [InlineData("\u00e9.zs", "?.zs", false)]
    [InlineData("\u00e9.zs", "??.zs", true)]
    [InlineData("child(", "child(", true)]
    [InlineData("child)", "child)", true)]
    [InlineData("child()", "child(\\)", true)]
    [InlineData("child()", "child([)]", true)]
    [InlineData("scripts/child.zs", "*(child|leaf).zs", true)]
    [InlineData("scripts/child.zs", "*?(child|leaf).zs", true)]
    [InlineData("abc", "(a*|x)c", false)]
    [InlineData("abc", "(a*|ab)c", true)]
    public void ArchivePatternsPreserveNativeEscapeAndByteEdgeCases(string name, string pattern, bool blocked)
    {
        var archive = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "#include \"" + name + "\""), (name, "")), VmScriptArchiveFormat.Pk3);
        if (blocked) Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: [pattern]));
        else Assert.Equal(2, Assert.Single(VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: [pattern])).Sources.Count);
    }

    [Theory]
    [InlineData("child.[zt]s", true)]
    [InlineData("child.[!ab]s", true)]
    [InlineData("child.[!zt]s", false)]
    [InlineData("child.[a-z]s", true)]
    [InlineData("child.[a-y]s", false)]
    public void ArchiveWildcardSetsUseNativeLiteralByteMembership(string pattern, bool blocked)
    {
        var archive = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "#include \"child.zs\""), ("child.zs", "")), VmScriptArchiveFormat.Pk3);
        if (blocked) Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: [pattern]));
        else Assert.Equal(2, Assert.Single(VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: [pattern])).Sources.Count);
    }

    [Theory]
    [InlineData("scripts/*")]
    [InlineData("scripts/child.?s")]
    [InlineData("scripts/child.zs")]
    public void CustomArchivePatternsHideMatchingIncludes(string pattern)
    {
        var archive = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "#include \"scripts/child.zs\""),
            ("scripts/child.zs", "")), VmScriptArchiveFormat.Pk3);
        Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: [pattern]));
        Assert.Equal(2, Assert.Single(VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: ["SCRIPTS/*"])).Sources.Count);
        Assert.Equal(2, Assert.Single(VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: ["*(abc|def)"])).Sources.Count);
        Assert.Equal(2, Assert.Single(VmArchiveScriptLoader.LoadAll([archive], blockedPatterns: ["bad\\"])).Sources.Count);
    }

    [Theory]
    [InlineData("zandronum.pk3")]
    [InlineData("C:\\mods\\ZANDRONUM.PK3")]
    [InlineData("mods/zandronum.pk3")]
    [InlineData("outer.pk3:zandronum.pk3")]
    public void CompatibilityPk3HidesNativeCoreScriptOverridesWithoutGameFilters(string name)
    {
        var data = ScriptZip(("ZSCRIPT", "version 99.0"), ("zscript.txt", "version 99.0"), ("zscript/child.zs", ""));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([new(name, data)]));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([new(name, data)], gameTypeFilters: []));
    }

    [Fact]
    public void CompatibilityExclusionsDoNotAffectOtherArchivesOrUnlistedRoots()
    {
        var data = ScriptZip(("zscript", "version 4.13"));
        Assert.Single(VmPk3ScriptLoader.LoadAll([new("other.pk3", data)]));
        Assert.Single(VmPk3ScriptLoader.LoadAll([new("zandronum.pk3.bak", data)]));
        Assert.Single(VmPk3ScriptLoader.LoadAll([new("zandronum.pk3", ScriptZip(("zscript.zs", "version 4.13")))]));
    }

    [Fact]
    public void CompatibilityExclusionsHideFullPathIncludeCandidates()
    {
        var archive = new VmScriptPk3("zandronum.pk3", ScriptZip(("zscript.zs", "#include \"mapinfo/core.zs\"\n#include \"menudef.za\""),
            ("mapinfo/core.zs", ""), ("menudef.za", "")));
        var error = Assert.Throws<VmScriptIncludeException>(() => VmPk3ScriptLoader.LoadAll([archive]));
        Assert.Equal(new[] { "mapinfo/core.zs", "menudef.za" }, error.Errors.Select(item => item.Path));
    }

    [Theory]
    [InlineData("installer.exe")]
    [InlineData("install.BAT")]
    [InlineData("backup~")]
    [InlineData("__MACOSX/metadata")]
    [InlineData("other/__macosx/metadata")]
    public void BlockedEntriesDoNotPreventCommonFolderStripping(string blocked)
    {
        var archive = new VmScriptPk3("wrapped.pk3", ScriptZip((blocked, ""), ("wrapper/zscript", "version 4.13")));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive], gameTypeFilters: []));
        Assert.Equal("wrapped.pk3:zscript", set.Sources[0].Name);
    }

    [Fact]
    public void BlockedExecutableRootsAreHiddenOnlyInFilteredMode()
    {
        var archive = new VmScriptPk3("mod.pk3", ScriptZip(("zscript.exe", "version 4.13")));
        Assert.Single(VmPk3ScriptLoader.LoadAll([archive]));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([archive], gameTypeFilters: []));
    }

    [Fact]
    public void BlockedIncludesRemainUnavailableAfterGameFilterSelection()
    {
        var archive = new VmScriptPk3("mod.pk3", ScriptZip(("zscript", "#include \"child.exe\""),
            ("child.exe", ""), ("filter/doom/child.exe", "")));
        Assert.Equal(2, Assert.Single(VmPk3ScriptLoader.LoadAll([archive])).Sources.Count);
        Assert.Equal("child.exe", Assert.Single(Assert.Throws<VmScriptIncludeException>(() =>
            VmPk3ScriptLoader.LoadAll([archive], gameTypeFilters: ["doom"])).Errors).Path);
    }

    [Fact]
    public void FilteredPk3StripsCommonWrapperBeforeGameSelection()
    {
        var archive = new VmScriptPk3("wrapped.pk3", ScriptZip(("wrapper/zscript", "#include \"scripts/child.zs\""),
            ("wrapper/scripts/child.zs", "base"), ("wrapper/filter/doom/scripts/child.zs", "selected")));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([archive]));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive], gameTypeFilters: ["doom"]));
        Assert.Equal("wrapped.pk3:zscript", set.Sources[0].Name);
        Assert.Equal("selected", set.Sources[1].Text);
    }

    [Fact]
    public void CommonFolderFallsBackFromDeepPrefixToTopFolder()
    {
        var archive = new VmScriptPk3("wrapped.pk3", ScriptZip(("wrapper/a/file", ""), ("wrapper/zscript", "")));
        Assert.Equal("wrapped.pk3:zscript", Assert.Single(VmPk3ScriptLoader.LoadAll([archive], gameTypeFilters: [])).Sources[0].Name);
    }

    [Theory]
    [InlineData("sounds/", "sounds/zscript")]
    [InlineData("filter/doom/file", "wrapper/zscript")]
    [InlineData("other/file", "wrapper/zscript")]
    public void NativeReservedOrUnsharedFoldersPreventWrapperStripping(string first, string root)
    {
        var archive = new VmScriptPk3("wrapped.pk3", ScriptZip((first, ""), (root, "")));
        Assert.Empty(VmPk3ScriptLoader.LoadAll([archive], gameTypeFilters: []));
    }

    [Fact]
    public void Pk3GameFiltersLoadGeneralThenSpecificRootsAndOverrides()
    {
        var archive = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "version 4.12\n#include \"CHILD\""),
            ("child", "base"), ("filter/doom/zscript", "version 4.13"), ("filter/doom/child", "general"),
            ("filter/doom.id/zscript", "version 4.13.1"), ("filter/doom.id/child", "specific"),
            ("filter/other/zscript", "version 99.0")), VmScriptArchiveFormat.Pk3);
        var sets = VmArchiveScriptLoader.LoadAll([archive], gameTypeFilters: ["doom.id"]);
        Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0), new Version(4, 13, 1) }, sets.Select(set => set.Version));
        Assert.Equal("specific", sets[0].Sources[1].Text);
        Assert.All(sets, set => Assert.Equal("core.pk3:zscript", set.Sources[0].Name));
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        host.LoadVmScriptArchives([archive], gameTypeFilters: ["doom.id"]);
        Assert.Equal(3, host.VmScriptSources.Count);
        Assert.Equal(3, VmPk3ScriptLoader.LoadAll([new("core.pk3", archive.Data)], gameTypeFilters: ["doom.id"]).Count);
    }

    [Fact]
    public void EmptyGameFiltersHideUnmatchedResourcesWhileNullKeepsUnfilteredPaths()
    {
        var archive = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "#include \"filter/other/child\""),
            ("filter/other/child", "found")), VmScriptArchiveFormat.Pk3);
        Assert.Equal("found", Assert.Single(VmArchiveScriptLoader.LoadAll([archive])).Sources[1].Text);
        Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([archive], gameTypeFilters: []));
    }

    [Theory]
    [InlineData("flats")]
    [InlineData("textures")]
    [InlineData("hires")]
    [InlineData("sprites")]
    [InlineData("voxels")]
    [InlineData("colormaps")]
    [InlineData("acs")]
    [InlineData("voices")]
    [InlineData("patches")]
    [InlineData("graphics")]
    [InlineData("sounds")]
    [InlineData("music")]
    public void Pk3NamespaceFullPathsApplyNativeEncodedNameRules(string folder)
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"" + folder + "/child.\""),
            (folder + "/child.{12}", "found"), (folder + "/zscript", "version 99.0")));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive]));
        Assert.Equal("core.pk3:" + folder + "/child.", set.Sources[1].Name);
        Assert.Equal("found", set.Sources[1].Text);
    }

    [Fact]
    public void Pk3NamespaceResourcesRemainExcludedFromGlobalFallback()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"CHILD\""), ("sounds/child.txt", "")));
        Assert.Throws<VmScriptIncludeException>(() => VmPk3ScriptLoader.LoadAll([archive]));
    }

    [Fact]
    public void Pk3EncodedRootNamesPreserveNativeIdRemovalOffsets()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript.{12}", "#include \"child.\""), ("child.{12}", "found")));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive]));
        Assert.Equal("core.pk3:zscript.", set.Sources[0].Name);
        Assert.Equal("core.pk3:child.", set.Sources[1].Name);
        Assert.Equal("found", set.Sources[1].Text);
    }

    [Fact]
    public void Pk3EncodedNamesWithExtensionsAndHiddenPathsKeepNativeNames()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"child.{12}.zs\"\n#include \"scripts/leaf.{12}\""),
            ("child.{12}.zs", ""), ("scripts/leaf.{12}", "")));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([archive]));
        Assert.Equal("core.pk3:child.{12}.zs", set.Sources[1].Name);
        Assert.Equal("core.pk3:scripts/leaf.{12}", set.Sources[2].Name);
    }

    [Fact]
    public void Pk3ShortNameFallbackUsesEightUtf8Bytes()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"\u00e9\u00e9\u00e9\u00e9\""),
            ("\u00e9\u00e9\u00e9\u00e9tail.txt", "found")));
        Assert.Equal("found", Assert.Single(VmPk3ScriptLoader.LoadAll([archive])).Sources[1].Text);
        var tooLong = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"\u00e9\u00e9\u00e9\u00e9x\""),
            ("\u00e9\u00e9\u00e9\u00e9x.txt", "")));
        Assert.Throws<VmScriptIncludeException>(() => VmPk3ScriptLoader.LoadAll([tooLong]));
    }

    [Fact]
    public void Pk3FullNamesRemainAvailableBeyondShortNameByteLimit()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"\u00e9\u00e9\u00e9\u00e9x\""),
            ("\u00e9\u00e9\u00e9\u00e9x", "found")));
        Assert.Equal("found", Assert.Single(VmPk3ScriptLoader.LoadAll([archive])).Sources[1].Text);
    }

    [Fact]
    public void Pk3RootOrderingUsesUtf8BytesRatherThanUtf16CodeUnits()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript.\U00010000", "version 4.13"),
            ("zscript.\uE000", "version 4.12")));
        Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0) }, VmPk3ScriptLoader.LoadAll([archive]).Select(set => set.Version));
    }

    [Fact]
    public void Pk3EntryNamesComposeUnicodeAndNormalizePathSeparators()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"scripts/\u00e9.zs\""),
            ("SCRIPTS\\E\u0301.zs", "")));
        var source = Assert.Single(VmPk3ScriptLoader.LoadAll([archive])).Sources[1];
        Assert.Equal("core.pk3:scripts/\u00e9.zs", source.Name);
    }

    [Fact]
    public void HostLoadsMixedScriptSourcesAndReplacesThemAtomically()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var core = new VmScriptArchive("core.wad", ScriptArchive(), VmScriptArchiveFormat.Wad);
        var mod = new VmScriptArchive("mod.pk3", ScriptZip(("zscript", "version 4.13\n#include \"scripts/child.zs\""),
            ("scripts/child.zs", "")), VmScriptArchiveFormat.Pk3);
        host.LoadVmScriptArchives([core, mod], new Version(4, 13));
        var sources = host.VmScriptSources;
        Assert.Equal(2, Assert.Single(sources).Sources.Count);
        Assert.Throws<NotSupportedException>(() => ((IList<VmScriptSourceSet>)sources).Clear());
        mod.Data.AsSpan().Clear();
        Assert.Equal("mod.pk3:scripts/child.zs", sources[0].Sources[1].Name);
        var missing = new VmScriptArchive("bad.wad", ScriptArchive(("ZSCRIPT", "#include \"MISSING\"")), VmScriptArchiveFormat.Wad);
        Assert.Throws<VmScriptIncludeException>(() => host.LoadVmScriptArchives([core, missing]));
        Assert.Same(sources, host.VmScriptSources);
        host.LoadVmScriptArchives([]);
        Assert.Empty(host.VmScriptSources);
        Assert.Equal(2, sources[0].Sources.Count);
    }

    [Fact]
    public void HostScriptLoadingPreservesPriorSourcesOnVersionFailureAndRejectsDisposal()
    {
        var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var root = new VmScriptArchive("core.wad", ScriptArchive(("ZSCRIPT", "version 4.13")), VmScriptArchiveFormat.Wad);
        host.LoadVmScriptArchives([root]);
        var sources = host.VmScriptSources;
        Assert.Throws<NotSupportedException>(() => host.LoadVmScriptArchives([root], new Version(4, 12)));
        Assert.Same(sources, host.VmScriptSources);
        var snapshot = host.Simulation.CaptureState();
        host.Simulation.RestoreState(snapshot);
        Assert.Same(sources, host.VmScriptSources);
        host.Dispose();
        Assert.Empty(host.VmScriptSources);
        Assert.Throws<ObjectDisposedException>(() => host.LoadVmScriptArchives([root]));
        Assert.Equal(new Version(4, 13, 0), sources[0].Version);
    }

    [Theory]
    [InlineData("S", "version 99.0")]
    [InlineData("F", "")]
    [InlineData("C", "")]
    [InlineData("A", "")]
    [InlineData("TX", "")]
    [InlineData("V", "")]
    [InlineData("HI", "")]
    [InlineData("VX", "")]
    [InlineData("SS", "version 99.0")]
    public void WadNamespaceBlocksHideScriptRoots(string prefix, string text)
    {
        var archive = new VmScriptWad("core.wad", ScriptArchive((prefix + "_START", ""), ("ZSCRIPT", text), (prefix + "_END", "")));
        Assert.Empty(VmWadScriptLoader.LoadAll([archive]));
        Assert.Throws<InvalidOperationException>(() => VmWadScriptLoader.Load([archive]));
    }

    [Fact]
    public void WadNamespaceIncludesDoNotOverrideGlobalResources()
    {
        var core = new VmScriptWad("core.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD\""), ("CHILD", "core")));
        var mod = new VmScriptWad("mod.wad", ScriptArchive(("F_START", ""), ("CHILD", "replacement"), ("F_END", "")));
        Assert.Equal("core", VmWadScriptLoader.Load([core, mod]).Sources[1].Text);
        Assert.Equal("core", Assert.Single(VmWadScriptLoader.LoadAll([core, mod])).Sources[1].Text);
    }

    [Fact]
    public void WadMarkersPreserveNativeMalformedBlockAndSmallSpriteRules()
    {
        Assert.Empty(VmWadScriptLoader.LoadAll([new("core.wad", ScriptArchive(("F_START", ""), ("ZSCRIPT", "")))]));
        Assert.Single(VmWadScriptLoader.LoadAll([new("core.wad", ScriptArchive(("F_END", ""), ("ZSCRIPT", "")))]));
        Assert.Single(VmWadScriptLoader.LoadAll([new("core.wad", ScriptArchive(("S_START", ""), ("ZSCRIPT", ""), ("S_END", "")))]));
        Assert.Empty(VmWadScriptLoader.LoadAll([new("core.wad", ScriptArchive(("F_START", ""), ("F_START", ""),
            ("F_END", ""), ("ZSCRIPT", ""), ("F_END", "")))]));
    }

    [Fact]
    public void SkinWadsHideAllScriptResources()
    {
        var archive = new VmScriptWad("skin.wad", ScriptArchive(("ZSCRIPT", ""), ("S_SKIN01", "")));
        Assert.Empty(VmWadScriptLoader.LoadAll([archive]));
        Assert.Throws<InvalidOperationException>(() => VmWadScriptLoader.Load([archive]));
    }

    [Fact]
    public void MixedScriptArchivesResolveIncludesAcrossBothFormats()
    {
        var core = new VmScriptArchive("core.wad", ScriptArchive(), VmScriptArchiveFormat.Wad);
        var wad = new VmScriptArchive("mod.wad", ScriptArchive(("ZSCRIPT", "version 4.12\n#include \"scripts/child.zs\""),
            ("LEAF", "old")), VmScriptArchiveFormat.Wad);
        var pk3 = new VmScriptArchive("mod.pk3", ScriptZip(("zscript.txt", "version 4.13\n#include \"LEAF\""),
            ("scripts/child.zs", "#include \"LEAF\"")), VmScriptArchiveFormat.Pk3);
        var sets = VmArchiveScriptLoader.LoadAll([core, wad, pk3]);
        Assert.Equal(2, sets.Count);
        Assert.Equal(new[] { 1, 2, 1 }, sets[0].Sources.Select(source => source.ContainerIndex));
        Assert.Equal("mod.wad:LEAF", sets[1].Sources[1].Name);
        Assert.Equal(new Version(4, 13, 0), sets[1].Version);
    }

    [Fact]
    public void MixedLookupUsesWadShortNamesAndPk3FullNames()
    {
        var core = new VmScriptArchive("core.wad", ScriptArchive(), VmScriptArchiveFormat.Wad);
        var wad = new VmScriptArchive("mod.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD\""), ("CHILD", "exact")), VmScriptArchiveFormat.Wad);
        var pk3 = new VmScriptArchive("mod.pk3", ScriptZip(("child.txt", "fallback")), VmScriptArchiveFormat.Pk3);
        Assert.Equal("fallback", Assert.Single(VmArchiveScriptLoader.LoadAll([core, wad, pk3])).Sources[1].Text);
        var replacement = new VmScriptArchive("last.pk3", ScriptZip(("child", "replacement")), VmScriptArchiveFormat.Pk3);
        Assert.Equal("replacement", Assert.Single(VmArchiveScriptLoader.LoadAll([core, wad, pk3, replacement])).Sources[1].Text);
        Assert.Equal("replacement", Assert.Single(VmArchiveScriptLoader.LoadAll([core, replacement, wad])).Sources[1].Text);
    }

    [Fact]
    public void WadNamesContainingDotsDoNotResolveAsFullPaths()
    {
        var wad = new VmScriptArchive("core.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD.ZS\""), ("CHILD.ZS", "")), VmScriptArchiveFormat.Wad);
        Assert.Equal("CHILD.ZS", Assert.Single(Assert.Throws<VmScriptIncludeException>(() => VmArchiveScriptLoader.LoadAll([wad])).Errors).Path);
        Assert.Throws<VmScriptIncludeException>(() => VmWadScriptLoader.Load([new("core.wad", wad.Data)]));
    }

    [Fact]
    public void MixedArchivesRejectCoreOverridesInEitherDirection()
    {
        var wad = new VmScriptArchive("core.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD\""), ("CHILD", "")), VmScriptArchiveFormat.Wad);
        var pk3 = new VmScriptArchive("mod.pk3", ScriptZip(("child", "")), VmScriptArchiveFormat.Pk3);
        Assert.Throws<InvalidOperationException>(() => VmArchiveScriptLoader.LoadAll([wad, pk3]));
        var zipCore = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "#include \"CHILD\""), ("child.txt", "")), VmScriptArchiveFormat.Pk3);
        var wadMod = new VmScriptArchive("mod.wad", ScriptArchive(("CHILD", "")), VmScriptArchiveFormat.Wad);
        Assert.Throws<InvalidOperationException>(() => VmArchiveScriptLoader.LoadAll([zipCore, wadMod]));
        var exactCore = new VmScriptArchive("core.pk3", ScriptZip(("zscript", "#include \"CHILD\""), ("child", "core")), VmScriptArchiveFormat.Pk3);
        Assert.Equal("core", Assert.Single(VmArchiveScriptLoader.LoadAll([exactCore, wadMod])).Sources[1].Text);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmArchiveScriptLoader.LoadAll([new("bad", [], (VmScriptArchiveFormat)99)]));
    }

    [Fact]
    public void Pk3ScriptsResolveNestedRelativeIncludesAndLaterOverrides()
    {
        var core = new VmScriptPk3("core.pk3", ScriptZip(("scripts/child.zs", "old")));
        var mod = new VmScriptPk3("mod.pk3", ScriptZip(("ZSCRIPT.txt", "version 4.13\n#include \"/SCRIPTS/Parent.zs\""),
            ("scripts/parent.zs", "#include \"./child.zs\""), ("scripts/child.zs", "new")));
        var set = Assert.Single(VmPk3ScriptLoader.LoadAll([core, mod]));
        Assert.Equal(new Version(4, 13, 0), set.Version);
        Assert.Equal(new[] { "mod.pk3:zscript.txt", "mod.pk3:scripts/parent.zs", "mod.pk3:scripts/child.zs" }, set.Sources.Select(source => source.Name));
        Assert.Equal("new", set.Sources[2].Text);
        Assert.All(set.Sources, source => Assert.Equal(1, source.ContainerIndex));
    }

    [Fact]
    public void Pk3RootsUseAlphabeticalGlobalShortNamesAndIndependentSessions()
    {
        var archive = new VmScriptPk3("core.pk3", ScriptZip(("zscript.zs", "version 4.13\n#include \"CHILD\""),
            ("folder/zscript", "version 99.0"), ("zscript", "version 4.12\n#include \"CHILD\""), ("child.txt", "")));
        var sets = VmPk3ScriptLoader.LoadAll([archive]);
        Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0) }, sets.Select(set => set.Version));
        Assert.All(sets, set => Assert.Equal(2, set.Sources.Count));
        Assert.Throws<NotSupportedException>(() => ((IList<VmScriptSourceSet>)sets).Clear());
        Assert.Empty(VmPk3ScriptLoader.LoadAll([]));
    }

    [Fact]
    public void Pk3IncludesEnforceCoreOverridesAndPreserveMissingPaths()
    {
        var core = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"scripts/child.zs\""), ("scripts/child.zs", "")));
        var mod = new VmScriptPk3("mod.pk3", ScriptZip(("scripts/child.zs", "new")));
        Assert.Contains("overriding core lump scripts/child.zs", Assert.Throws<InvalidOperationException>(() => VmPk3ScriptLoader.LoadAll([core, mod])).Message);
        var missing = new VmScriptPk3("core.pk3", ScriptZip(("zscript", "#include \"//missing.zs\"")));
        Assert.Equal("//missing.zs", Assert.Single(Assert.Throws<VmScriptIncludeException>(() => VmPk3ScriptLoader.LoadAll([missing])).Errors).Path);
        Assert.Throws<InvalidDataException>(() => VmPk3ScriptLoader.LoadAll([new("bad.pk3", [1, 2, 3])]));
    }

    private static byte[] ScriptZip(params (string Name, string Text)[] files)
    {
        using var bytes = new MemoryStream();
        using (var zip = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var file in files)
            {
                using var stream = zip.CreateEntry(file.Name).Open();
                stream.Write(Encoding.UTF8.GetBytes(file.Text));
            }
        return bytes.ToArray();
    }

    [Fact]
    public void ScriptWadIncludesIgnoreExactlyOneLeadingSlash()
    {
        var archive = new VmScriptWad("core.wad", ScriptArchive(("ZSCRIPT", "#include \"/child\""), ("CHILD", "")));
        var result = VmWadScriptLoader.Load([archive]);
        Assert.Equal("core.wad:CHILD", result.Sources[1].Name);
        Assert.Equal(0, result.Sources[1].ContainerIndex);
        Assert.Equal(2, VmWadScriptLoader.Load([archive], "/ZSCRIPT").Sources.Count);

        var doubled = new VmScriptWad("core.wad", ScriptArchive(("ZSCRIPT", "#include \"//CHILD\""), ("CHILD", "")));
        var error = Assert.Single(Assert.Throws<VmScriptIncludeException>(() => VmWadScriptLoader.Load([doubled])).Errors);
        Assert.Equal("//CHILD", error.Path);
    }

    [Fact]
    public void LeadingSlashLookupPreservesCoreOverrideProtection()
    {
        var core = new VmScriptWad("core.wad", ScriptArchive(("ZSCRIPT", "#include \"/CHILD\""), ("CHILD", "")));
        var mod = new VmScriptWad("mod.wad", ScriptArchive(("CHILD", "replacement")));
        Assert.Contains("overriding core lump /CHILD", Assert.Throws<InvalidOperationException>(() => VmWadScriptLoader.LoadAll([core, mod])).Message);
    }

    [Fact]
    public void ScriptWadLoaderEnumeratesEveryRootInArchiveAndDirectoryOrder()
    {
        var core = new VmScriptWad("core.wad", ScriptArchive(("ZSCRIPT", "version 4.12")));
        var mod = new VmScriptWad("mod.wad", ScriptArchive(("ZSCRIPT", "version 4.13"), ("ZSCRIPT", "version 4.13.1")));
        var sets = VmWadScriptLoader.LoadAll([core, mod]);
        Assert.Equal(new[] { new Version(4, 12, 0), new Version(4, 13, 0), new Version(4, 13, 1) }, sets.Select(set => set.Version));
        Assert.Equal(new[] { 0, 1, 1 }, sets.Select(set => set.Sources[0].ContainerIndex));
        Assert.Throws<NotSupportedException>(() => ((IList<VmScriptSourceSet>)sets).Clear());
        Assert.Equal(new Version(4, 13, 1), VmWadScriptLoader.Load([core, mod]).Version);
    }

    [Fact]
    public void ScriptWadRootEnumerationKeepsIndependentIncludeSessions()
    {
        var emptyCore = new VmScriptWad("core.wad", ScriptArchive());
        var mod = new VmScriptWad("mod.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD\""),
            ("ZSCRIPT", "#include \"CHILD\""), ("CHILD", "")));
        var sets = VmWadScriptLoader.LoadAll([emptyCore, mod]);
        Assert.Equal(2, sets.Count);
        Assert.All(sets, set => Assert.Equal(2, set.Sources.Count));
        Assert.Empty(VmWadScriptLoader.LoadAll([emptyCore]));
        Assert.Empty(VmWadScriptLoader.LoadAll([]));
    }

    [Fact]
    public void ScriptWadLoaderReadsIncludesAndLaterDuplicateResources()
    {
        var core = new VmScriptWad("core.wad", ScriptArchive(("CHILD", "old")));
        var mod = new VmScriptWad("mod.wad", ScriptArchive(("ZSCRIPT", "version 4.13\n#include \"child\""), ("CHILD", "new")));
        var result = VmWadScriptLoader.Load([core, mod]);
        Assert.Equal(new Version(4, 13, 0), result.Version);
        Assert.Equal(new[] { "mod.wad:ZSCRIPT", "mod.wad:CHILD" }, result.Sources.Select(source => source.Name));
        Assert.Equal("new", result.Sources[1].Text);
        Assert.Equal(1, result.Sources[1].ContainerIndex);
    }

    [Fact]
    public void ScriptWadLoaderBlocksCoreOverridesAndReportsMissingIncludes()
    {
        var core = new VmScriptWad("core.wad", ScriptArchive(("ZSCRIPT", "#include \"CHILD\""), ("CHILD", "")));
        var mod = new VmScriptWad("mod.wad", ScriptArchive(("CHILD", "replacement")));
        Assert.Contains("overriding core lump CHILD", Assert.Throws<InvalidOperationException>(() => VmWadScriptLoader.Load([core, mod])).Message);
        var missing = new VmScriptWad("mod.wad", ScriptArchive(("ZSCRIPT", "#include \"MISSING\"")));
        Assert.Equal("MISSING", Assert.Single(Assert.Throws<VmScriptIncludeException>(() => VmWadScriptLoader.Load([missing])).Errors).Path);
        Assert.Throws<InvalidOperationException>(() => VmWadScriptLoader.Load([mod]));
        Assert.Throws<InvalidDataException>(() => VmWadScriptLoader.Load([new("broken.wad", [1, 2, 3])]));
    }

    private static byte[] ScriptArchive(params (string Name, string Text)[] lumps)
    {
        var data = lumps.Select(lump => Encoding.UTF8.GetBytes(lump.Text)).ToArray();
        var directory = 12 + data.Sum(item => item.Length);
        var wad = new byte[directory + lumps.Length * 16];
        "PWAD"u8.CopyTo(wad);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(4), lumps.Length);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(8), directory);
        var position = 12;
        for (var i = 0; i < lumps.Length; i++)
        {
            data[i].CopyTo(wad, position);
            var entry = wad.AsSpan(directory + i * 16, 16);
            BinaryPrimitives.WriteInt32LittleEndian(entry, position);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], data[i].Length);
            Encoding.ASCII.GetBytes(lumps[i].Name).CopyTo(entry[8..]);
            position += data[i].Length;
        }
        return wad;
    }

    [Fact]
    public void HostPassesScriptVersionToNamedArgumentBinding()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer], parameterNames: ["value"]);
        var arguments = new[] { new VmCallArgument(VmValue.FromInteger(42), "value") };
        Assert.Equal(VmFault.InvalidCall, host.RunVmNamedFunction(function, arguments, scriptVersion: new Version(4, 12)).Fault);
        Assert.Equal(42, host.RunVmNamedFunction(function, arguments, scriptVersion: new Version(4, 13)).Values[0].Value);
    }

    [Fact]
    public void HostBindsNamedScriptArgumentsAndRejectsDisposedEntry()
    {
        var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer], parameterNames: ["value"]);
        Assert.Equal(42, host.RunVmNamedFunction(function, [new(VmValue.FromInteger(42), "value")]).Values[0].Value);
        host.Dispose();
        Assert.Throws<ObjectDisposedException>(() => host.RunVmNamedFunction(function, []));
    }

    [Fact]
    public void HostPreservesNamedNullParameterAborts()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var callback = new VmFunction((args, count) => [VmValue.FromObject(VmNativeParameters.RequirePointer(args[0], "self"))], "CheckSelf");
        var result = host.RunVm([VmCode.Rrr(VmOp.Param, 3, 0, 0), VmCode.Rrr(VmOp.CallK, 0, 1, 0)], [], [], [], 0, objectConstants: [callback]);
        Assert.Equal("In function parameter self", result.AbortContext);
        Assert.Equal("Called from CheckSelf\n", result.AbortStackTrace);
        Assert.Throws<VmAbortException>(() => host.RunVmNativeFunction(callback, [VmValue.FromObject(null)]));
    }

    [Fact]
    public void HostFillsNativeDefaultsAndRejectsDisposedEntry()
    {
        var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var typed = new VmFunction((args, count) => [args[0]], defaultArguments: [VmValue.FromString("default")]);
        var integer = new VmIntegerFunction((args, count) => [args[0]], defaultArguments: [42]);
        Assert.Equal("default", host.RunVmNativeFunctionWithDefaults(typed, []).Values[0].StringValue);
        Assert.Equal(42, host.RunVmNativeFunctionWithDefaults(integer, []).Values[0].Value);
        host.Dispose();
        Assert.Throws<ObjectDisposedException>(() => host.RunVmNativeFunctionWithDefaults(typed, []));
        Assert.Throws<ObjectDisposedException>(() => host.RunVmNativeFunctionWithDefaults(integer, []));
    }

    [Fact]
    public void HostRunsDirectNativeFunctionsAndRejectsDisposedEntry()
    {
        var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var typed = new VmFunction((args, count) => [VmValue.FromString(args[0].String!)]);
        var integer = new VmIntegerFunction((args, count) => [args[0] + 1]);
        Assert.Equal("text", host.RunVmNativeFunction(typed, [VmValue.FromString("text")]).Values[0].StringValue);
        Assert.Equal(42, host.RunVmNativeFunction(integer, [41]).Values[0].Value);
        host.Dispose();
        Assert.Throws<ObjectDisposedException>(() => host.RunVmNativeFunction(typed, []));
        Assert.Throws<ObjectDisposedException>(() => host.RunVmNativeFunction(integer, []));
    }

    [Fact]
    public void HostPreservesSourceFramesForImplicitScriptAborts()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var function = new VmScriptFunction([VmCode.Rrr(VmOp.LoadWord, 0, 0, 0)], [0], [],
            lineInfo: [new(0, 17)], printableName: "ReadValue", sourceFileName: "read.zs");
        var direct = host.RunVmFunction(function, []);
        Assert.Equal(VmFault.ScriptAbort, direct.Fault);
        Assert.Equal((ushort)1, direct.AbortCode);
        Assert.Equal(new VmAbortFrame("ReadValue", "read.zs", 0, 17), Assert.Single(direct.AbortFrames!));
        var nested = host.RunVm([VmCode.Rrr(VmOp.CallK, 0, 0, 0)], [], [], [], 0, objectConstants: [function]);
        Assert.Equal(direct.AbortFrames, nested.AbortFrames);
        Assert.Empty(nested.Values);
    }

    [Fact]
    public void HostRunsScriptFunctionWithDefaults()
    {
        var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var function = new VmScriptFunction([VmCode.RetInt(0)], [], [VmValueType.Integer], trailingDefaults: [VmValue.FromInteger(42)]);
        Assert.Equal(42, host.RunVmFunction(function, []).Values[0].Value);
        host.Dispose();
        Assert.Throws<ObjectDisposedException>(() => host.RunVmFunction(function, []));
    }

    [Fact]
    public void HostPassesRuntimeClassMetadataToVm()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var memory = new byte[] { 42 };
        var metadata = new VmClassMetadata(new Dictionary<Type, object?> { [typeof(object)] = memory });
        var result = host.RunVm([VmCode.Rrr(VmOp.Lkp, 1, 0, 0), VmCode.Rrr(VmOp.Meta, 1, 1, 0),
            VmCode.Rrr(VmOp.LoadUnsignedByte, 0, 1, 0), VmCode.RetInt(0)], [0], [], [], objectConstants: [new object()], classMetadata: metadata);
        Assert.Equal(42, result.Values[0].Value);
    }

    [Fact]
    public void HostProvidesPerExecutionFrameExtraSpace()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        var code = new[] { VmCode.Rrr(VmOp.LoadFramePointer, 1, 0, 0), VmCode.Li(2, 42),
            VmCode.Rrr(VmOp.StoreWord, 1, 2, 0), VmCode.Rrr(VmOp.LoadWord, 2, 1, 0), VmCode.RetInt(2) };
        Assert.Equal(42, host.RunVm(code, [0], [], [], frameExtraSpace: 4).Values[0].Value);
        Assert.Equal(VmFault.InvalidFrame, host.RunVm(code, [0], [], []).Fault);
    }

    private static readonly int[] ColorCode = [VmCode.Rrr(VmOp.Lks, 0, 0, 0),
        VmCode.Rrr(VmOp.Cast, 0, 0, (int)VmCast.StringToColor), VmCode.RetInt(0)];

    [Fact]
    public void HostLoadsLatestColorResourceAndReplacesCatalogWithArchive()
    {
        using var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        Assert.True(host.TrySetSoundArchive(Archive("1 2 3 red", "4 5 6 red"), out var error), error);
        Assert.Equal(0x040506, host.RunVm(ColorCode, [], [], ["RED"]).Values[0].Value);
        var snapshot = host.Simulation.CaptureState();
        host.Simulation.RestoreState(snapshot);
        Assert.Equal(0x040506, host.RunVm(ColorCode, [], [], ["red"]).Values[0].Value);
        Assert.True(host.TrySetSoundArchive(Archive(), out error), error);
        Assert.Equal(VmFault.InvalidColor, host.RunVm(ColorCode, [], [], ["red"]).Fault);
    }

    [Fact]
    public void InvalidCatalogPreservesPreviousArchiveAndDisposedHostRejectsExecution()
    {
        var host = new ClientHost(AuthoritySimulation.Start(new PlayLevel()));
        Assert.True(host.TrySetSoundArchive(Archive("1 2 3 red"), out _));
        Assert.False(host.TrySetSoundArchive(Archive("broken"), out var error));
        Assert.Equal("vm-color-catalog-invalid", error);
        Assert.Equal(0x010203, host.RunVm(ColorCode, [], [], ["red"]).Values[0].Value);
        host.Dispose();
        Assert.Throws<ObjectDisposedException>(() => host.RunVm(ColorCode, [], [], ["red"]));
    }

    private static byte[] Archive(params string[] resources)
    {
        var data = resources.Select(Encoding.UTF8.GetBytes).ToArray();
        var directory = 12 + data.Sum(item => item.Length);
        var wad = new byte[directory + resources.Length * 16];
        Encoding.ASCII.GetBytes("PWAD").CopyTo(wad, 0);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(4), resources.Length);
        BinaryPrimitives.WriteInt32LittleEndian(wad.AsSpan(8), directory);
        var position = 12;
        for (var index = 0; index < data.Length; index++)
        {
            data[index].CopyTo(wad, position);
            var entry = wad.AsSpan(directory + index * 16, 16);
            BinaryPrimitives.WriteInt32LittleEndian(entry, position);
            BinaryPrimitives.WriteInt32LittleEndian(entry[4..], data[index].Length);
            Encoding.ASCII.GetBytes("X11R6RGB").CopyTo(entry[8..]);
            position += data[index].Length;
        }
        return wad;
    }
}
