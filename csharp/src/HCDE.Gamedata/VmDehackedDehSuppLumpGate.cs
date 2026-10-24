namespace HCDE.Gamedata;

/// <summary><c>LoadDehSupp</c> lump gate from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedDehSuppLumpGate
{
    public static bool TryOpenDehsupp(int lumpIndex, int fileContainer, out string? warning)
    {
        warning = null;
        if (lumpIndex < 0)
            return false;
        if (fileContainer > 0)
        {
            warning = "DEHSUPP no longer supported from user WADs";
            return false;
        }

        return true;
    }

    public static string FormatUnknownSectionError(string section) =>
        $"Unknown section '{section}'";

    public static bool TryHandleRecoverableLoadError(string message, out string printed)
    {
        printed = message;
        return false;
    }
}
