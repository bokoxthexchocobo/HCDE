namespace HCDE.Client;

public delegate IVmMusicOutputStream? VmMusicOutputFactory(VmMusicStreamFormat format, VmMusicFloatFill fill);

public sealed class VmMusicStreamPlayback
{
    private readonly VmMusicStreamFill samples = new();
    private readonly VmMusicOutputStream output = new();
    public float MusicVolume { get; set; } = 1;
    public IVmMusicOutputStream? Stream => output.Stream;

    public void Create(bool hasSong, Func<VmMusicStreamFormat> getFormat, VmMusicOutputFactory createOutput,
        VmMusicFloatFill fillFloat, VmMusicInt16Fill fillInt16)
    {
        ArgumentNullException.ThrowIfNull(getFormat);
        ArgumentNullException.ThrowIfNull(createOutput);
        ArgumentNullException.ThrowIfNull(fillFloat);
        ArgumentNullException.ThrowIfNull(fillInt16);
        output.Create(hasSong, getFormat, format =>
        {
            var inputFloat = output.IsFloat;
            VmMusicFloatFill fill = inputFloat
                ? destination => samples.FillFloat(destination, MusicVolume, fillFloat)
                : destination => samples.FillInt16(destination, MusicVolume, fillInt16);
            return createOutput(format, fill);
        });
    }

    public void Pause(bool paused) => output.Pause(paused);
    public void Stop() => output.Stop();
}
