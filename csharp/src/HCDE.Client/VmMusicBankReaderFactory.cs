namespace HCDE.Client;

public static class VmMusicBankReaderFactory
{
    public static IVmMusicBankReader? Open(string? name, VmMusicBankFormat allowed,
        IReadOnlyList<VmMusicBank> banks, Func<string, int>? findResource = null,
        Func<int, byte[]?>? reopenResource = null)
    {
        ArgumentNullException.ThrowIfNull(banks);
        if (name is null) return null;
        if ((findResource is null) != (reopenResource is null))
            throw new ArgumentException("Indexed resource lookup and reopen callbacks must be supplied together");
        var configuration = name.Length > 4 && name.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase);
        if (configuration && findResource is not null)
        {
            var index = findResource(name);
            if (index >= 0) return new VmGusResourceMusicReader(name, index, findResource, reopenResource!);
        }
        if (File.Exists(name))
        {
            using var file = File.OpenRead(name);
            var header = new byte[16];
            var count = file.ReadAtLeast(header, 16, throwOnEndOfStream: false);
            if ((allowed & VmMusicBankFormat.Sf2) != 0 && VmMusicBankCatalog.Identify(header.AsSpan(0, count)) == VmMusicBankFormat.Sf2)
                return new VmSf2MusicReader(name);
            if ((allowed & VmMusicBankFormat.Gus) != 0)
            {
                if (count >= 2 && header[0] == 'P' && header[1] == 'K') return new VmGusZipMusicReader(name);
                if (configuration) return new VmGusFileMusicReader(name);
            }
        }
        var bank = VmMusicBankCatalog.Find(banks, name, allowed);
        return bank?.Format switch
        {
            null => null,
            VmMusicBankFormat.Sf2 => new VmSf2MusicReader(bank.Path),
            VmMusicBankFormat.Gus => new VmGusZipMusicReader(bank.Path),
            _ => throw new NotSupportedException("FM-bank reader integration is not implemented"),
        };
    }
}
