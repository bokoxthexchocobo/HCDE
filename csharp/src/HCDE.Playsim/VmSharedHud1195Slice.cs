namespace HCDE.Playsim;

/// <summary>
/// Alternate HUD cvars, class creation, and draw size from <c>shared_hud.cpp</c> (<b>1–186</b>).
/// Chunk <b>1195</b>. Class lookup and <c>GetUIScale</c> are host results.
/// </summary>
public static class VmSharedHud1195Slice
{
    public const int Archive = 1;
    public const int NoInitCall = 1 << 9;
    public const int GlobalConfig = 1 << 10;
    public const int TypeBool = 0;
    public const int TypeInt = 1;
    public const int CrBrick = 0;
    public const int CrGreen = 3;
    public const int CrGold = 5;
    public const int CrRed = 6;
    public const int CrOrange = 8;
    public const int CrYellow = 10;
    public const string TextColorRed = "\u001cG";
    public const string NameNone = "None";
    public const string NameAltHud = "AltHud";

    public static readonly List<Decl> Declarations = new();
    public static readonly Dictionary<string, Class> Classes = new();
    public static readonly List<string> Messages = new();
    public static readonly List<string> Calls = new();
    public static readonly List<Player> Players = new();

    public static int AltHudScale;
    public static bool OldScale;
    public static bool AspectScale;
    public static double ScaleFactor = 1;
    public static bool ForceInternal;
    public static string GameAltHudClass = NameAltHud;
    public static int ConsolePlayer;
    public static Bar? StatusBar;
    public static Func<Drawer, int, int> GetUIScale = (_, scale) => scale > 0 ? scale : 1;

    public sealed class FatalError : Exception
    {
        public FatalError(string message)
            : base(message)
        {
            Detail = message;
        }

        public string Detail;
    }

    public sealed class Decl
    {
        public string Name = "";
        public int Type;
        public int Flags;
        public bool Bool;
        public int Int;
    }

    public sealed class Class
    {
        public string Name = "";
        public Class? Parent;
        public bool Init;
        public bool Draw;
    }

    public sealed class Hud
    {
        public Class Class = new();
        public bool Destroyed;

        public void Destroy()
        {
            Destroyed = true;
        }
    }

    public sealed class Player
    {
        public int InventoryTics;
    }

    public sealed class Drawer
    {
        public int Width;
        public int Height;
    }

    public sealed class Bar
    {
        public Hud? AltHud;
        public Player? CPlayer;
        public Drawer Screen = new();

        public void CreateAltHUD()
        {
            if (AltHud != null)
            {
                AltHud.Destroy();
                AltHud = null;
            }

            if (!ForceInternal)
                AltHud = DoCreate(GameAltHudClass);
            if (AltHud == null)
                AltHud = DoCreate(NameAltHud);
            if (AltHud == null)
                throw new FatalError("AltHud");
        }

        public void DrawAltHUD()
        {
            if (StatusBar == null)
                throw new FatalError("StatusBar");
            var player = StatusBar.CPlayer;
            Players[ConsolePlayer].InventoryTics = 0;
            int width;
            int height;
            if (OldScale)
            {
                var scale = GetUIScale(Screen, AltHudScale);
                width = Screen.Width / scale;
                height = Screen.Height / scale;
            }
            else
            {
                width = (int)(640d / ScaleFactor);
                height = width * Screen.Height / Screen.Width;
            }

            if (AspectScale)
                height = height * 5 / 6;
            if (AltHud != null && HasVirtual(AltHud.Class, "Draw"))
                Calls.Add("Draw " + AltHud.Class.Name + " " + width + " " + height + " " + (ReferenceEquals(player, CPlayer) ? "local" : "global"));
        }
    }

    public static void Reset()
    {
        Declarations.Clear();
        AddInt("hud_althudscale", 0);
        AddBool("hud_althud", false);
        AddBool("hud_showsecrets", true);
        AddBool("hud_showmonsters", true);
        AddBool("hud_showitems", false);
        AddBool("hud_showstats", false);
        AddBool("hud_showscore", false);
        AddBool("hud_showweapons", true);
        AddBool("am_showepisode", false);
        AddBool("am_showcluster", false);
        AddInt("hud_showammo", 2);
        AddInt("hud_showtime", 0);
        AddInt("hud_showtimestat", 0);
        AddInt("hud_timecolor", CrGold);
        AddInt("hud_showlag", 0);
        AddInt("hud_ammo_order", 0);
        AddInt("hud_ammo_red", 25);
        AddInt("hud_ammo_yellow", 50);
        AddBool("hud_swaphealtharmor", false);
        AddInt("hud_health_red", 25);
        AddInt("hud_health_yellow", 50);
        AddInt("hud_health_green", 100);
        AddInt("hud_armor_red", 25);
        AddInt("hud_armor_yellow", 50);
        AddInt("hud_armor_green", 100);
        AddBool("hud_berserk_health", true);
        AddBool("hud_showangles", false);
        AddInt("hudcolor_titl", CrYellow);
        AddInt("hudcolor_time", CrRed);
        AddInt("hudcolor_ltim", CrOrange);
        AddInt("hudcolor_ttim", CrGold);
        AddInt("hudcolor_xyco", CrGreen);
        AddInt("hudcolor_statnames", CrRed);
        AddInt("hudcolor_stats", CrGreen);
        AddBool("map_point_coordinates", true, Archive | GlobalConfig);
        AddBool("hud_althud_forceinternal", false, Archive | NoInitCall);
        AltHudScale = 0;
        OldScale = false;
        AspectScale = false;
        ScaleFactor = 1;
        ForceInternal = false;
        GameAltHudClass = NameAltHud;
        ConsolePlayer = 0;
        StatusBar = null;
        Classes.Clear();
        Messages.Clear();
        Calls.Clear();
        Players.Clear();
        Players.Add(new Player());
        GetUIScale = (_, scale) => scale > 0 ? scale : 1;
        var alt = new Class { Name = NameAltHud, Init = true, Draw = true };
        Classes[NameAltHud] = alt;
    }

    public static void SetForceInternal(bool value)
    {
        ForceInternal = value;
        if (StatusBar != null)
            StatusBar.CreateAltHUD();
    }

    static void AddBool(string name, bool value, int flags = Archive)
    {
        Declarations.Add(new Decl { Name = name, Type = TypeBool, Flags = flags, Bool = value });
    }

    static void AddInt(string name, int value, int flags = Archive)
    {
        Declarations.Add(new Decl { Name = name, Type = TypeInt, Flags = flags, Int = value });
    }

    static Hud? DoCreate(string className)
    {
        if (className == NameNone)
            return null;
        if (!Classes.TryGetValue(className, out var cls))
        {
            Messages.Add(TextColorRed + "Unknown alternative HUD class \"" + className + "\"\n");
            return null;
        }

        if (!IsDescendant(cls, NameAltHud))
        {
            Messages.Add(TextColorRed + "Alternative HUD class \"" + className + "\" is not derived from AltHud\n");
            return null;
        }

        var hud = new Hud { Class = cls };
        if (HasVirtual(cls, "Init"))
            Calls.Add("Init " + cls.Name);
        return hud;
    }

    static bool IsDescendant(Class cls, string name)
    {
        for (var cursor = cls; cursor != null; cursor = cursor.Parent)
        {
            if (cursor.Name == name)
                return true;
        }

        return false;
    }

    static bool HasVirtual(Class cls, string name)
    {
        for (var cursor = cls; cursor != null; cursor = cursor.Parent)
        {
            if (name == "Init" && cursor.Init)
                return true;
            if (name == "Draw" && cursor.Draw)
                return true;
        }

        return false;
    }
}
