namespace HCDE.Client;

public interface IVmMusicBankReader
{
    VmMusicPatchPaths Paths { get; }
    (byte[]? Data, string Name) Open(string? name);
}
