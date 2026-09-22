namespace HCDE.Client;

/// <summary>
/// Headless launcher widget. It validates a start selection and builds the client argument list.
/// There is no Avalonia or ImGui window in this phase.
/// </summary>
public sealed class LauncherSelection
{
    public string IwadPath { get; init; } = "";
    public string MapName { get; init; } = "MAP01";
    public int Skill { get; init; } = 3;
    public int Width { get; init; } = 320;
    public int Height { get; init; } = 200;
    public bool SoftwareRenderer { get; init; } = true;
}

public static class LauncherPlan
{
    public static bool TryValidate(LauncherSelection selection, out string? error)
    {
        if (string.IsNullOrWhiteSpace(selection.IwadPath))
        {
            error = "iwad-required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(selection.MapName))
        {
            error = "map-required";
            return false;
        }

        if (selection.Skill is < 1 or > 5)
        {
            error = "skill-range";
            return false;
        }

        if (selection.Width < 16 || selection.Height < 16)
        {
            error = "view-size";
            return false;
        }

        error = null;
        return true;
    }

    public static string[] BuildArgs(LauncherSelection selection)
    {
        if (!TryValidate(selection, out var error))
            throw new InvalidOperationException(error);
        return
        [
            "--iwad", selection.IwadPath,
            "--map", selection.MapName,
            "--skill", selection.Skill.ToString(),
            "--width", selection.Width.ToString(),
            "--height", selection.Height.ToString(),
            "--renderer", selection.SoftwareRenderer ? "software" : "hardware",
        ];
    }

    public static bool TryParse(string[] args, out LauncherSelection selection, out string? error)
    {
        selection = new LauncherSelection();
        error = null;
        string? iwad = null;
        var map = "MAP01";
        var skill = 3;
        var width = 320;
        var height = 200;
        var software = true;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--iwad":
                    if (!Take(args, ref i, out iwad))
                    {
                        error = "iwad-required";
                        return false;
                    }
                    break;
                case "--map":
                    if (!Take(args, ref i, out var mapArg) || string.IsNullOrWhiteSpace(mapArg))
                    {
                        error = "map-required";
                        return false;
                    }
                    map = mapArg;
                    break;
                case "--skill":
                    if (!Take(args, ref i, out var skillText) || !int.TryParse(skillText, out skill))
                    {
                        error = "skill-range";
                        return false;
                    }
                    break;
                case "--width":
                    if (!Take(args, ref i, out var widthText) || !int.TryParse(widthText, out width))
                    {
                        error = "view-size";
                        return false;
                    }
                    break;
                case "--height":
                    if (!Take(args, ref i, out var heightText) || !int.TryParse(heightText, out height))
                    {
                        error = "view-size";
                        return false;
                    }
                    break;
                case "--renderer":
                    if (!Take(args, ref i, out var renderer))
                    {
                        error = "renderer";
                        return false;
                    }
                    software = renderer == "software";
                    if (renderer is not ("software" or "hardware"))
                    {
                        error = "renderer";
                        return false;
                    }
                    break;
                default:
                    error = "unknown-arg";
                    return false;
            }
        }

        selection = new LauncherSelection
        {
            IwadPath = iwad ?? "",
            MapName = map,
            Skill = skill,
            Width = width,
            Height = height,
            SoftwareRenderer = software,
        };
        return TryValidate(selection, out error);
    }

    private static bool Take(string[] args, ref int index, out string? value)
    {
        value = null;
        if (index + 1 >= args.Length)
            return false;
        value = args[++index];
        return true;
    }
}
