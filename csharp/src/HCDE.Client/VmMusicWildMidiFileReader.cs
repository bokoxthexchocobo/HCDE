namespace HCDE.Client;

public static class VmMusicWildMidiFileReader
{
    public static byte[] Read(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length > 0x1fffffff)
            throw new InvalidDataException("The WildMIDI resource exceeds the native file size limit.");
        var data = new byte[(int)stream.Length];
        stream.ReadExactly(data);
        return data;
    }
}
