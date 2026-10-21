namespace HCDE.Net.Core;

/// <summary>Win32 WSA error name helper (<c>i_net.cpp</c> 4626–4679).</summary>
public static class VmINetWinNetError
{
    public static string NetErrorNative(Func<int>? wsaGetLastError = null)
    {
        var code = wsaGetLastError?.Invoke() ?? 0;
        return code switch
        {
            10013 => "EACCES",
            10048 => "EADDRINUSE",
            10049 => "EADDRNOTAVAIL",
            10047 => "EAFNOSUPPORT",
            10037 => "EALREADY",
            10053 => "ECONNABORTED",
            10061 => "ECONNREFUSED",
            10054 => "ECONNRESET",
            10039 => "EDESTADDRREQ",
            10014 => "EFAULT",
            10064 => "EHOSTDOWN",
            10065 => "EHOSTUNREACH",
            10036 => "EINPROGRESS",
            10004 => "EINTR",
            10022 => "EINVAL",
            10056 => "EISCONN",
            10024 => "EMFILE",
            10040 => "EMSGSIZE",
            10050 => "ENETDOWN",
            10052 => "ENETRESET",
            10051 => "ENETUNREACH",
            10055 => "ENOBUFS",
            10042 => "ENOPROTOOPT",
            10057 => "ENOTCONN",
            10038 => "ENOTSOCK",
            10045 => "EOPNOTSUPP",
            10046 => "EPFNOSUPPORT",
            10067 => "EPROCLIM",
            10043 => "EPROTONOSUPPORT",
            10041 => "EPROTOTYPE",
            10058 => "ESHUTDOWN",
            10044 => "ESOCKTNOSUPPORT",
            10060 => "ETIMEDOUT",
            10035 => "EWOULDBLOCK",
            11001 => "HOST_NOT_FOUND",
            10093 => "NOTINITIALISED",
            11004 => "NO_DATA",
            11003 => "NO_RECOVERY",
            10091 => "SYSNOTREADY",
            11002 => "TRY_AGAIN",
            10092 => "VERNOTSUPPORTED",
            10101 => "EDISCON",
            _ => code.ToString(),
        };
    }
}
