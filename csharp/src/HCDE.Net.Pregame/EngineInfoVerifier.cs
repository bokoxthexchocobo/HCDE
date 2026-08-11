namespace HCDE.Net.Pregame;

public enum EngineVerificationError
{
    None = 0,
    Engine = 1,
    FileMissing = 2,
    FileUnknown = 3,
    FileOrder = 4,
}

public sealed class EngineVerificationResult
{
    public EngineVerificationError Error { get; init; } = EngineVerificationError.None;
    public IReadOnlyList<string> UnknownFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> MissingFiles { get; init; } = Array.Empty<string>();

    public bool IsSuccess => Error == EngineVerificationError.None;
}

/// <summary>
/// Ports the CRC-list matching rules from <c>Net_VerifyEngine</c>.
/// </summary>
public static class EngineInfoVerifier
{
    public static EngineVerificationResult Verify(EngineInfoSnapshot guest, IReadOnlyList<string> hostRequiredCrcs)
    {
        if (hostRequiredCrcs.Count == 0)
            return new EngineVerificationResult();

        if (guest.WadCrcs.Count < hostRequiredCrcs.Count)
        {
            return new EngineVerificationResult
            {
                Error = EngineVerificationError.FileMissing,
                MissingFiles = hostRequiredCrcs.Skip(guest.WadCrcs.Count).ToArray(),
            };
        }

        var error = guest.WadCrcs.Count > hostRequiredCrcs.Count
            ? EngineVerificationError.FileUnknown
            : EngineVerificationError.None;
        var unverified = Enumerable.Range(0, hostRequiredCrcs.Count).ToList();
        var unknownFiles = new List<string>();

        foreach (var guestCrc in guest.WadCrcs)
        {
            var matchIndex = -1;
            for (var i = 0; i < unverified.Count; i++)
            {
                if (hostRequiredCrcs[unverified[i]] == guestCrc)
                {
                    matchIndex = i;
                    break;
                }
            }

            if (matchIndex < 0)
            {
                unknownFiles.Add(guestCrc);
                error = EngineVerificationError.FileUnknown;
                continue;
            }

            unverified.RemoveAt(matchIndex);
        }

        if (unverified.Count > 0)
        {
            return new EngineVerificationResult
            {
                Error = EngineVerificationError.FileMissing,
                MissingFiles = unverified.Select(i => hostRequiredCrcs[i]).ToArray(),
            };
        }

        if (error == EngineVerificationError.FileUnknown)
        {
            return new EngineVerificationResult
            {
                Error = EngineVerificationError.FileUnknown,
                UnknownFiles = unknownFiles,
            };
        }

        for (var i = 0; i < hostRequiredCrcs.Count; i++)
        {
            if (guest.WadCrcs[i] != hostRequiredCrcs[i])
                return new EngineVerificationResult { Error = EngineVerificationError.FileOrder };
        }

        return new EngineVerificationResult();
    }
}
