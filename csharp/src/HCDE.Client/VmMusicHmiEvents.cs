namespace HCDE.Client;

public static class VmMusicHmiEvents
{
    public static VmMusicSmfTrack Decode(ReadOnlySpan<byte> data) => VmMusicHmiSchedule.DecodeTrack(data);
}
