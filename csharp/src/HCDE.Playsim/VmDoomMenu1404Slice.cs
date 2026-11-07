namespace HCDE.Playsim;

/// <summary>
/// Menu colors and the menu console commands from <c>doommenu.cpp</c> 1636-1786. Chunk 1404.
/// Font-color lookup and class creation stay callees. The file ends at line 1786.
/// </summary>
public static class VmDoomMenu1404Slice
{
    public const int BandStart = 1636;
    public const int BandEnd = 1786;
    public const int Level = 0;
    public const int DemoScreen = 3;
    public const int MenuScreen = DemoScreen;
    public const int FullConsole = 4;
    public const int HideConsole = 5;

    public sealed class MenuClass
    {
        public bool IsMenu;
        public bool IsDelegate;
        public object? Created = new();
    }

    public sealed class Colors
    {
        public int Title;
        public int Font;
        public int Value;
        public int More;
        public int Header;
        public int Highlight;
        public int Selection;
    }

    public sealed class GameInfo
    {
        public string Title = "";
        public string Font = "";
        public string Value = "";
        public string More = "";
        public string Header = "";
        public string Highlight = "";
        public string Selection = "";
        public string HelpMenuClass = "";
        public string MenuDelegateClass = "";
    }

    public readonly record struct Command(string Name, string Menu, bool LeaveConsole);

    public static readonly Command[] Commands =
    [
        new("menu_main", "MainMenu", true),
        new("menu_load", "LoadgameMenu", false),
        new("menu_save", "SavegameMenu", false),
        new("menu_help", "ReadthisMenu", false),
        new("menu_game", "PlayerclassMenu", false),
        new("menu_options", "OptionsMenu", false),
        new("menu_player", "PlayerMenu", false),
        new("menu_messages", "MessageOptions", false),
        new("menu_automap", "AutomapOptions", false),
        new("menu_scoreboard", "ScoreboardOptions", false),
        new("menu_mapcolors", "MapColorMenu", false),
        new("menu_keys", "CustomizeControls", false),
        new("menu_gameplay", "GameplayOptions", false),
        new("menu_compatibility", "CompatibilityOptions", false),
        new("menu_mouse", "MouseOptions", false),
        new("menu_joystick", "JoystickOptions", false),
        new("menu_sound", "SoundOptions", false),
        new("menu_advsound", "AdvSoundOptions", false),
        new("menu_modreplayer", "ModReplayerOptions", false),
        new("menu_display", "VideoOptions", false),
        new("menu_video", "VideoModeMenu", false),
    ];

    public static object? SetDefaultMenuColors(
        GameInfo info,
        Colors colors,
        Func<string, int> findColor,
        Func<string, MenuClass?> findClass)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));
        if (colors == null)
            throw new ArgumentNullException(nameof(colors));
        if (findColor == null)
            throw new ArgumentNullException(nameof(findColor));
        if (findClass == null)
            throw new ArgumentNullException(nameof(findClass));
        colors.Title = findColor(info.Title);
        colors.Font = findColor(info.Font);
        colors.Value = findColor(info.Value);
        colors.More = findColor(info.More);
        colors.Header = findColor(info.Header);
        colors.Highlight = findColor(info.Highlight);
        colors.Selection = findColor(info.Selection);
        MenuClass? help = findClass(info.HelpMenuClass);
        if (help == null)
            throw new InvalidOperationException($"{info.HelpMenuClass}: Undefined help menu class");
        if (!help.IsMenu)
            throw new InvalidOperationException($"'{info.HelpMenuClass}' does not inherit from Menu");
        MenuClass? deleg = findClass(info.MenuDelegateClass);
        if (deleg == null)
            throw new InvalidOperationException($"{info.MenuDelegateClass}: Undefined menu delegate class");
        if (!deleg.IsDelegate)
            throw new InvalidOperationException($"'{info.MenuDelegateClass}' does not inherit from MenuDelegateBase");
        return deleg.Created;
    }

    public static void Run(string name, ref int state, Action<bool> startPanel, Action<string, int> setMenu)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        if (startPanel == null)
            throw new ArgumentNullException(nameof(startPanel));
        if (setMenu == null)
            throw new ArgumentNullException(nameof(setMenu));
        foreach (Command command in Commands)
        {
            if (command.Name != name)
                continue;
            if (command.LeaveConsole && state == FullConsole)
                state = MenuScreen;
            startPanel(true);
            setMenu(command.Menu, -1);
            return;
        }
        throw new ArgumentException(name);
    }
}
