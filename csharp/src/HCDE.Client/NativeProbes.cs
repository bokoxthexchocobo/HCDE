using System.Runtime.InteropServices;

namespace HCDE.Client;

public readonly record struct NativeProbe(bool LibraryFound, bool Bound, string Detail);

/// <summary>
/// Looks for the native libraries Phase 4 does not bind. <see cref="NativeProbe.Bound"/> stays false.
/// </summary>
public static class NativeProbes
{
    public static NativeProbe ZScriptJit() => Probe("asmjit");

    public static NativeProbe Vulkan() => Probe(OperatingSystem.IsWindows() ? "vulkan-1" : "vulkan");

    public static NativeProbe ZMusic() => Probe("zmusic");

    public static NativeProbe Probe(string libraryName)
    {
        try
        {
            if (!NativeLibrary.TryLoad(libraryName, out var handle))
                return new NativeProbe(false, false, "absent");
            NativeLibrary.Free(handle);
            return new NativeProbe(true, false, "present-unbound");
        }
        catch (Exception ex)
        {
            return new NativeProbe(false, false, ex.GetType().Name);
        }
    }
}
