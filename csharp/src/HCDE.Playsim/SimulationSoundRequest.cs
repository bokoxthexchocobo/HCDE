namespace HCDE.Playsim;

[Flags]
public enum SimulationSoundFlags
{
    ListenerZ = 8,
    MaybeLocal = 16,
    Ui = 32,
    NoPause = 64,
    Force = 65536,
    Loop = 256,
    NoStop = 4096,
    Overlap = 8192,
    Local = 16384,
    Singular = 131072,
}

public sealed record SimulationSoundRequest(uint ActorId, string Sound, int Channel, int Flags,
    float Volume, bool Loop, float Attenuation, bool Local, bool Stop = false, bool ChangeVolume = false);
