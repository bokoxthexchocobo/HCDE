namespace HCDE.Net.Pregame;

public sealed class PregameSessionSnapshot
{
    public MapLoadInfo MapLoad { get; init; } = new();
    public GameInfoPayload GameInfo { get; init; } = new();
    public string HostUserInfo { get; init; } = "name\\player";
    public IReadOnlyList<string> RequiredWadCrcs { get; init; } = Array.Empty<string>();
    public byte RoomId { get; init; }
    public uint AuthorityGameTic { get; init; } = 1;
    public uint AuthorityClientTic { get; init; }
    public uint Consistency { get; init; }
}
