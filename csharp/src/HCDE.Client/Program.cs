namespace HCDE.Client;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h")
        {
            Console.WriteLine("Usage: hcde --iwad <path> --map <name> [--skill 1-5] [--width n] [--height n] [--renderer software|hardware] [--view-preferences <path>] [--script-file <path>] [--core-script-file <path>] [--game-type doom|heretic|hexen|strife|chex] [--iwad-filter <name>] [--iwad-info <catalog-path>] [--required-iwad <path>] [--iwad-search-dir <directory>] [--iwad-recursive-dir <directory>] [--no-support-wad] [--search-config <path>] [--discover-search-config] [--music-banks] [--no-music] [--search-path <directory>] [--search-recursive-path <directory>] [--search-path-list <paths>] [--file-search-dir <directory>] [--file-recursive-dir <directory>] [--game-filters] [--game-filter <name>] [--script-block <pattern>] [--script-embed <archive-entry>]");
            Console.WriteLine("       hcde --self-test");
            return args.Length == 0 ? 2 : 0;
        }

        if (args[0] == "--self-test")
        {
            using var host = ClientHost.DemoRoom();
            host.Tick(default);
            host.Frame.Get(host.Frame.Width / 2, host.Frame.Height / 2, out var r, out var g, out var b);
            Console.WriteLine("center={0},{1},{2} tic={3}", r, g, b, host.Simulation.Thinkers.Clock.Tic);
            return r == SoftwareView.WallR && g == SoftwareView.WallG && b == SoftwareView.WallB ? 0 : 1;
        }

        if (!LauncherPlan.TryParse(args, out var selection, out var error))
        {
            Console.Error.WriteLine(error);
            return 2;
        }

        foreach (var arg in LauncherPlan.BuildArgs(selection))
            Console.Write(arg + " ");
        Console.WriteLine();
        return 0;
    }
}
