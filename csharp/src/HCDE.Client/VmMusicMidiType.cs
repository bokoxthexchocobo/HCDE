namespace HCDE.Client;

public enum VmMusicMidiType { None, Midi, Hmi, Xmi, Mus, Mids }

public static class VmMusicMidiDetector
{
    public static VmMusicMidiType Identify(ReadOnlySpan<byte> header)
    {
        if (header.IndexOf("MUS\u001a"u8) >= 0) return VmMusicMidiType.Mus;
        if (header.StartsWith("HMI-MIDISONG"u8) || header.StartsWith("HMIMIDIP"u8)) return VmMusicMidiType.Hmi;
        if (header.Length >= 12)
        {
            var form = header.StartsWith("FORM"u8);
            var type = header.Slice(8, 4);
            if (form && type.SequenceEqual("XDIR"u8)
                || (form || header.StartsWith("CAT "u8)) && type.SequenceEqual("XMID"u8)) return VmMusicMidiType.Xmi;
            if (header.StartsWith("RIFF"u8) && type.SequenceEqual("MIDS"u8)) return VmMusicMidiType.Mids;
        }
        return header.StartsWith("MThd"u8) ? VmMusicMidiType.Midi : VmMusicMidiType.None;
    }
}
