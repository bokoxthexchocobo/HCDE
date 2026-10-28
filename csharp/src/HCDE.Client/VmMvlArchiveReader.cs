namespace HCDE.Client;

internal static class VmMvlArchiveReader
{
    public static IReadOnlyList<(string Name, byte[] Data)> Read(byte[] data)
    {
        var directory = VmMvlResourceProbe.ReadDirectory(data);
        var entries = new List<(string, byte[])>(directory.Count);
        foreach (var entry in directory)
        {
            if (entry.Position + entry.Length > (ulong)data.Length)
                throw new InvalidDataException("MVL entry exceeds archive bounds.");
            entries.Add((entry.FileName, data.AsSpan((int)entry.Position, (int)entry.Length).ToArray()));
        }

        return entries.AsReadOnly();
    }
}
