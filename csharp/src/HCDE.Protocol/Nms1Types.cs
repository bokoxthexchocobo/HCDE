namespace HCDE.Protocol;

public sealed class Nms1ChallengeToken
{
    public uint IssuedUnix { get; set; }
    public byte[] Token { get; } = new byte[MasterProtocol.Nms1ChallengeTokenSize];
}

public sealed class Nms1EntryToken
{
    public byte[] Token { get; } = new byte[MasterProtocol.Nms1EntryTokenSize];
}

public sealed class Nms1RegisterRequest
{
    public Nms1ChallengeToken Challenge { get; } = new();
    public string ProtocolFamily { get; set; } = MasterProtocol.Nms1DefaultProtocolFamily;
    public ushort GamePort { get; set; }
    public ushort QueryPort { get; set; }
    public ushort CurrentPlayers { get; set; }
    public ushort MaxPlayers { get; set; }
    public uint ServerFlags { get; set; }
    public string BuildLabel { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string MapName { get; set; } = string.Empty;
}

public sealed class Nms1HeartbeatRequest
{
    public string ProtocolFamily { get; set; } = MasterProtocol.Nms1DefaultProtocolFamily;
    public ushort GamePort { get; set; }
    public Nms1EntryToken Entry { get; } = new();
    public ushort CurrentPlayers { get; set; }
    public ushort MaxPlayers { get; set; }
    public uint ServerFlags { get; set; }
}

public sealed class Nms1UnregisterRequest
{
    public string ProtocolFamily { get; set; } = MasterProtocol.Nms1DefaultProtocolFamily;
    public ushort GamePort { get; set; }
    public Nms1EntryToken Entry { get; } = new();
}

public enum Nms1ParseResult
{
    Ok,
    NotForRequest,
    ErrorResponse,
    Malformed,
}

public sealed class Nms1ErrorResponse
{
    public ushort Code { get; set; }
    public string Text { get; set; } = string.Empty;
}
