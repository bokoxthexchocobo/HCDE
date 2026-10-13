namespace HCDE.Client.Tests;

public class VmMusicBankCatalogTests
{
    [Theory]
    [InlineData("timidity.cfg", true, true)]
    [InlineData("TIMIDITY.CFG", true, true)]
    [InlineData("timidity.cfg", false, false)]
    [InlineData("nested/timidity.cfg", true, false)]
    [InlineData("other.cfg", true, false)]
    public void PackedGusRequiresRootConfigurationAndMultipleResources(string configName, bool patch, bool accepted)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".data");
        try
        {
            using (var output = File.Create(path))
            using (var zip = new System.IO.Compression.ZipArchive(output, System.IO.Compression.ZipArchiveMode.Create))
            {
                using (var entry = zip.CreateEntry(configName).Open()) entry.Write([255]);
                if (patch)
                {
                    using var entry = zip.CreateEntry("instrument.pat").Open();
                    entry.Write([255]);
                }
            }
            var banks = VmMusicBankCatalog.Collect([path]);
            if (accepted)
            {
                Assert.Equal(VmMusicBankFormat.Gus, Assert.Single(banks).Format);
                Assert.Equal(path, VmMusicBankCatalog.Find(banks, null, VmMusicBankFormat.Gus)!.Path);
            }
            else Assert.Empty(banks);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DiscoveryUsesOrderedNormalPathsAndFallbackOnlyWhenNoBanksExist()
    {
        var root = Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var first = Directory.CreateDirectory(System.IO.Path.Combine(root, "first")).FullName;
            var second = Directory.CreateDirectory(System.IO.Path.Combine(root, "second")).FullName;
            var nested = Directory.CreateDirectory(System.IO.Path.Combine(first, "nested")).FullName;
            File.WriteAllText(System.IO.Path.Combine(nested, "ignored.sf2"), "RIFF0000sfbkLIST");
            var fallbackDirectory = Directory.CreateDirectory(System.IO.Path.Combine(root, "soundfonts")).FullName;
            var fallback = System.IO.Path.Combine(fallbackDirectory, "hcde.sf2");
            File.WriteAllText(fallback, "RIFF0000sfbkLIST");
            var config = VmSearchPathConfiguration.Parse("[SoundfontSearch.Directories]\nPath=$FIRST\nRecursivePath=" + nested + "\nPath=" + second);
            Assert.Equal(fallback, Assert.Single(VmMusicBankCatalog.Discover(config, root, _ => first)).Path);
            var early = System.IO.Path.Combine(first, "bank.data");
            File.WriteAllText(early, "WOPL3-BANK\0");
            File.WriteAllText(System.IO.Path.Combine(second, "BANK.sf2"), "RIFF0000sfbkLIST");
            Assert.Equal(early, Assert.Single(VmMusicBankCatalog.Discover(config, root, _ => first)).Path);
            var missing = VmSearchPathConfiguration.Parse("[SoundfontSearch.Directories]\nPath=" + System.IO.Path.Combine(root, "missing"));
            var warnings = new List<string>();
            Assert.Equal(fallback, Assert.Single(VmMusicBankCatalog.Discover(missing, root, warning: warnings.Add)).Path);
            Assert.Single(warnings);
            File.Delete(fallback);
            Assert.Empty(VmMusicBankCatalog.Discover(missing, root, warning: _ => { }));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [InlineData("WOPL3-BANK\0", VmMusicBankFormat.Wopl)]
    [InlineData("WOPN2-BANK\0", VmMusicBankFormat.Wopn)]
    [InlineData("WOPN2-B2NK\0", VmMusicBankFormat.Wopn)]
    [InlineData("RIFF0000sfbkLIST", VmMusicBankFormat.Sf2)]
    [InlineData("RIFF0000sfbk", VmMusicBankFormat.None)]
    [InlineData("riff0000sfbkLIST", VmMusicBankFormat.None)]
    public void HeadersIdentifyFormats(string header, VmMusicBankFormat expected) =>
        Assert.Equal(expected, VmMusicBankCatalog.Identify(System.Text.Encoding.ASCII.GetBytes(header)));

    [Fact]
    public void FirstRecognizedBaseNameWinsAndSelectionFallsBackByCompatibility()
    {
        var root = Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var first = System.IO.Path.Combine(root, "bank.data");
            var duplicate = System.IO.Path.Combine(root, "BANK.wopl");
            var other = System.IO.Path.Combine(root, "other.wopl");
            File.WriteAllText(first, "RIFF0000sfbkLIST");
            File.WriteAllText(duplicate, "WOPL3-BANK\0");
            File.WriteAllText(other, "WOPL3-BANK\0");
            var banks = VmMusicBankCatalog.Collect([first, duplicate, other]);
            Assert.Equal(2, banks.Count);
            Assert.Equal(first, VmMusicBankCatalog.Find(banks, "BANK.DATA", VmMusicBankFormat.Sf2)!.Path);
            Assert.Equal(other, VmMusicBankCatalog.Find(banks, "missing", VmMusicBankFormat.Wopl)!.Path);
            Assert.Null(VmMusicBankCatalog.Find(banks, null, VmMusicBankFormat.Wopn));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
