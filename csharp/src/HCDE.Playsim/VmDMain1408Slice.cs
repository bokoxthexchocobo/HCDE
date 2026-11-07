namespace HCDE.Playsim;

/// <summary>
/// Startup cvar callbacks, the HUD toggle, and the render wrapper from
/// d_main.cpp 422-649. Chunk 1408.
/// dmflags starts at line 657 and stays outside this band.
/// This tree compiles the software-renderer vid_rendermode callback.
/// </summary>
public static class VmDMain1408Slice
{
    public const int BandStart = 422;
    public const int BandEnd = 649;
    public const int DmFlagsStart = 657;
    public const float MinTimeScale = 0.05f;
    public const int WipeNone = 0;
    public const int WipeMelt = 1;
    public const int WipeBurn = 2;
    public const int WipeFade = 3;
    public const int WipeCount = 4;
    public const int HardwareMode = 4;
    public const int HiddenBlocks = 12;
    public const int MaxPlayers = 64;
    public const string NetScaleMessage = "Time scale cannot be changed in net games.\n";
    public const string MinScaleMessage = "Time scale must be at least 0.05!\n";
    public const string FragLimitKey = "TXT_FRAGLIMIT";

    public sealed class Level
    {
        public bool Lights;
        public int Probes;
        public bool HasDynamicLights;
        public int ParticlePasses;
        public int LinkPasses;
        public int Interpolations;
        public int Restores;
        public float LastFrac;
    }

    public sealed class World
    {
        public bool NetGame;
        public bool UserGame;
        public bool Deathmatch;
        public bool GlLights;
        public bool DynLights;
        public int RenderMode = HardwareMode;
        public float TimeScale = 1f;
        public int ScreenBlocks = 10;
        public bool DrawPlayerSprite = true;
        public bool ShowMessages = true;
        public bool HudToggled;
        public int SavedBlocks = 10;
        public bool SavedDraw = true;
        public bool SavedMessages = true;
        public string CursorPic = "";
        public int FriendlyTitle;
        public readonly bool[] InGame = new bool[MaxPlayers];
        public readonly int[] Frags = new int[MaxPlayers];
        public readonly List<string> Trace = new();
        public readonly List<Level> Levels = new();
    }

    public static float ApplyTimeScale(World world, float self)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (world.NetGame)
        {
            world.Trace.Add(NetScaleMessage);
            return 1f;
        }
        if (self >= MinTimeScale)
        {
            world.Trace.Add("freeze");
            world.TimeScale = self;
            world.Trace.Add("thaw");
            return self;
        }
        world.Trace.Add(MinScaleMessage);
        return self;
    }

    public static int ApplyRenderMode(World world, int self)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (self < 0 || self > HardwareMode)
            self = HardwareMode;
        else if (self == 2 || self == 3)
            self -= 2;
        world.RenderMode = self;
        if (world.UserGame)
            world.Trace.Add("pitch");
        world.Trace.Add("filter");
        return self;
    }

    public static int ApplyFragLimit(World world, int self, Func<string, string> text)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        if (world.Deathmatch && self > 0)
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (world.InGame[i] && self <= world.Frags[i])
                {
                    world.Trace.Add(text(FragLimitKey));
                    world.Trace.Add("exit 0 false");
                    break;
                }
            }
        }
        return self;
    }

    public static int ApplyWipeType(int self)
    {
        if (self < WipeNone || self >= WipeCount)
            return WipeMelt;
        return self;
    }

    public static void ApplyCursor(World world, string self, Func<string, bool> setCursor)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (self == null)
            throw new ArgumentNullException(nameof(self));
        if (setCursor == null)
            throw new ArgumentNullException(nameof(setCursor));
        bool accepted;
        if (string.Equals(self, "None", StringComparison.OrdinalIgnoreCase) && world.CursorPic.Length > 0)
            accepted = setCursor(world.CursorPic);
        else
            accepted = setCursor(self);
        if (!accepted)
            setCursor("cursor");
    }

    public static void ApplyFullbrightOverrides(World world, bool enabled)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        world.Trace.Add(enabled ? "fullbright on" : "fullbright off");
    }

    public static void ApplyFriendlyTitle(World world, int mode)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        world.FriendlyTitle = mode;
        world.Trace.Add("title");
    }

    public static void ToggleHud(World world)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        world.HudToggled = !world.HudToggled;
        if (world.HudToggled)
        {
            world.SavedBlocks = world.ScreenBlocks;
            world.SavedDraw = world.DrawPlayerSprite;
            world.SavedMessages = world.ShowMessages;
            world.ScreenBlocks = HiddenBlocks;
            world.DrawPlayerSprite = false;
            world.ShowMessages = false;
            world.Trace.Add("hide");
            world.Trace.Add("clearmenu");
        }
        else
        {
            world.ScreenBlocks = world.SavedBlocks;
            world.DrawPlayerSprite = world.SavedDraw;
            world.ShowMessages = world.SavedMessages;
        }
    }

    public static void ToggleHudCommand(World world)
    {
        ToggleHud(world);
    }

    public static void Render(World world, bool interpolate, float timeFrac, Action action)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        foreach (Level level in world.Levels)
        {
            bool lightsOn = (world.GlLights && world.RenderMode == HardwareMode)
                || (world.DynLights && world.RenderMode != HardwareMode)
                || level.Probes > 0;
            level.HasDynamicLights = lightsOn && (level.Lights || level.Probes > 0);
            if (interpolate)
            {
                level.Interpolations++;
                level.LastFrac = timeFrac;
            }
            level.ParticlePasses++;
            level.LinkPasses++;
        }
        action();
        if (!interpolate)
            return;
        foreach (Level level in world.Levels)
            level.Restores++;
    }
}
