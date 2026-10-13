namespace HCDE.Client;

public static class VmSearchConfigurationDiscovery
{
    public static string? FindWindows(string programDirectory, string documentsDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(programDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentsDirectory);
        var portable = Path.Combine(programDirectory, "hcde_portable.ini");
        if (File.Exists(portable)) return portable;
        var user = Path.Combine(documentsDirectory, "My Games", "HCDE", "hcde.ini");
        if (File.Exists(user)) return user;
        var template = Path.Combine(programDirectory, "hcde.ini");
        return File.Exists(template) ? template : null;
    }
}
