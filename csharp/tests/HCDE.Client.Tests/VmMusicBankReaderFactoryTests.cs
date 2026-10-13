namespace HCDE.Client.Tests;

public class VmMusicBankReaderFactoryTests
{
    [Fact]
    public void SelectedConfigurationIndexIsNotResolvedAgainDuringConstruction()
    {
        var lookups = 0;
        var reader = VmMusicBankReaderFactory.Open("bank.cfg", VmMusicBankFormat.None, [],
            _ => ++lookups == 1 ? 7 : -1, index => [(byte)index]);
        Assert.Equal(new byte[] { 7 }, reader!.Open(null).Data);
        Assert.Equal(new byte[] { 7 }, reader.Open(null).Data);
        Assert.Equal(1, lookups);
    }

    [Fact]
    public void DirectZipAndCatalogFallbackOpenBinaryConfigurationsAndReleaseFiles()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".zip");
        try
        {
            using (var file = File.Create(path))
            using (var zip = new System.IO.Compression.ZipArchive(file, System.IO.Compression.ZipArchiveMode.Create))
            {
                using var entry = zip.CreateEntry("timidity.cfg").Open();
                entry.Write([0, 255]);
            }
            var direct = Assert.IsType<VmGusZipMusicReader>(VmMusicBankReaderFactory.Open(path, VmMusicBankFormat.Gus, []));
            using (direct) Assert.Equal(new byte[] { 0, 255 }, direct.Open(null).Data);
            VmMusicBank[] banks = [new("bank", "bank.zip", path, VmMusicBankFormat.Gus)];
            var fallback = Assert.IsType<VmGusZipMusicReader>(VmMusicBankReaderFactory.Open("missing", VmMusicBankFormat.Gus, banks));
            using (fallback) Assert.Equal(new byte[] { 0, 255 }, fallback.Open(null).Data);
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ResourceConfigurationsTakePriorityEvenWithoutAllowedGusFormat()
    {
        var reader = VmMusicBankReaderFactory.Open("bank.cfg", VmMusicBankFormat.None, [], _ => 1, _ => [17]);
        Assert.IsType<VmGusResourceMusicReader>(reader);
        Assert.Equal(new byte[] { 17 }, reader!.Open(null).Data);
        Assert.Null(VmMusicBankReaderFactory.Open(null, VmMusicBankFormat.Sf2, []));
        Assert.Throws<ArgumentException>(() => VmMusicBankReaderFactory.Open("bank.cfg", VmMusicBankFormat.Gus, [], _ => 1));
    }

    [Fact]
    public void DirectSf2AndConfigurationSelectionRespectFormatsAndCatalogFallback()
    {
        var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var sf2 = Path.Combine(root, "bank.data");
            var cfg = Path.Combine(root, "bank.cfg");
            File.WriteAllText(sf2, "RIFF0000sfbkLIST");
            File.WriteAllBytes(cfg, [42]);
            Assert.IsType<VmSf2MusicReader>(VmMusicBankReaderFactory.Open(sf2, VmMusicBankFormat.Sf2, []));
            Assert.Null(VmMusicBankReaderFactory.Open(sf2, VmMusicBankFormat.Gus, []));
            Assert.IsType<VmGusFileMusicReader>(VmMusicBankReaderFactory.Open(cfg, VmMusicBankFormat.Gus, []));
            var banks = VmMusicBankCatalog.Collect([sf2]);
            Assert.IsType<VmSf2MusicReader>(VmMusicBankReaderFactory.Open("missing", VmMusicBankFormat.Sf2, banks));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
