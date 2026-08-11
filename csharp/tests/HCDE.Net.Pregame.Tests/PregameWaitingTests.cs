using HCDE.Net.Pregame;

namespace HCDE.Net.Pregame.Tests;

public class EngineInfoVerifierTests
{
    [Fact]
    public void AcceptsMatchingCrcList()
    {
        var guest = new EngineInfoSnapshot { WadCrcs = ["aaa", "bbb"] };
        var result = EngineInfoVerifier.Verify(guest, ["aaa", "bbb"]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RejectsMissingFiles()
    {
        var guest = new EngineInfoSnapshot { WadCrcs = ["aaa"] };
        var result = EngineInfoVerifier.Verify(guest, ["aaa", "bbb"]);
        Assert.Equal(EngineVerificationError.FileMissing, result.Error);
    }

    [Fact]
    public void RejectsUnknownFiles()
    {
        var guest = new EngineInfoSnapshot { WadCrcs = ["aaa", "extra"] };
        var result = EngineInfoVerifier.Verify(guest, ["aaa"]);
        Assert.Equal(EngineVerificationError.FileUnknown, result.Error);
    }

    [Fact]
    public void RejectsWrongOrder()
    {
        var guest = new EngineInfoSnapshot { WadCrcs = ["bbb", "aaa"] };
        var result = EngineInfoVerifier.Verify(guest, ["aaa", "bbb"]);
        Assert.Equal(EngineVerificationError.FileOrder, result.Error);
    }
}

public class PregameServicePayloadTests
{
    [Fact]
    public void MapLoadRoundTrip()
    {
        var original = new MapLoadInfo { MapName = "START", RngSeed = 99 };
        var buffer = new byte[128];
        var length = PregameServicePayloads.WriteMapLoadInfo(buffer, original);
        Assert.True(PregameServicePayloads.TryReadMapLoadInfo(buffer.AsSpan(0, length), out var parsed));
        Assert.Equal(original.MapName, parsed.MapName);
        Assert.Equal(original.RngSeed, parsed.RngSeed);
    }

    [Fact]
    public void GameInfoRoundTrip()
    {
        var original = new GameInfoPayload
        {
            TicDup = 2,
            GameId = [9, 8, 7, 6, 5, 4, 3, 2],
            ServerInfo = [0xAA, 0xBB],
        };
        var buffer = new byte[64];
        var length = PregameServicePayloads.WriteGameInfo(buffer, original);
        Assert.True(PregameServicePayloads.TryReadGameInfo(buffer.AsSpan(0, length), out var parsed));
        Assert.Equal(original.TicDup, parsed.TicDup);
        Assert.Equal(original.GameId, parsed.GameId);
        Assert.Equal(original.ServerInfo, parsed.ServerInfo);
    }

    [Fact]
    public void RosterRoundTrip()
    {
        var entries = new List<RosterEntry>
        {
            new() { ClientSlot = 0, UserInfo = "name\\host" },
            new() { ClientSlot = 1, Address = new byte[PregameServicePayloads.SockAddrInSize], UserInfo = "name\\guest" },
        };
        var buffer = new byte[256];
        var length = PregameServicePayloads.WriteRoster(buffer, entries);
        Assert.True(PregameServicePayloads.TryReadRoster(buffer.AsSpan(0, length), out var parsed));
        Assert.Equal(2, parsed.Count);
        Assert.Equal("name\\guest", parsed[1].UserInfo);
    }
}
