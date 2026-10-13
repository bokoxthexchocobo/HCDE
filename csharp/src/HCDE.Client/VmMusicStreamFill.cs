namespace HCDE.Client;

public delegate bool VmMusicFloatFill(Span<float> samples);
public delegate bool VmMusicInt16Fill(Span<short> samples);

public sealed class VmMusicStreamFill
{
    private short[] scratch = [];

    public bool FillFloat(Span<float> destination, float musicVolume, VmMusicFloatFill fill)
    {
        ArgumentNullException.ThrowIfNull(fill);
        return VmMusicStreamSamples.ApplyFloat(destination, musicVolume, fill(destination));
    }

    public bool FillInt16(Span<float> destination, float musicVolume, VmMusicInt16Fill fill)
    {
        ArgumentNullException.ThrowIfNull(fill);
        if (scratch.Length < destination.Length) scratch = new short[destination.Length];
        var input = scratch.AsSpan(0, destination.Length);
        return VmMusicStreamSamples.ConvertInt16(input, destination, musicVolume, fill(input));
    }
}
