namespace HCDE.Playsim;

/// <summary>
/// dmflags, dmflags2, the empty dmflags3 callback, and the compatibility
/// mode masks from d_main.cpp 657-912. Chunk 1409.
/// GetCaps starts at line 974 and stays outside this band.
/// Assigning compatflags and compatflags2 runs those cvar callbacks.
/// </summary>
public static class VmDMain1409Slice
{
    public const int BandStart = 657;
    public const int BandEnd = 912;
    public const int GetCapsStart = 974;
    public const int MaxPlayers = 64;
    public const int ChaseCamCheat = 1 << 5;
    public const uint NoFreelook = 1u << 18;
    public const uint NoFov = 1u << 20;
    public const uint NoAutomap = 1u << 18;
    public const uint DisallowSpying = 1u << 20;
    public const uint ChaseCamFlag = 1u << 21;

    public const uint ShortTex = 1u << 0;
    public const uint StairIndex = 1u << 1;
    public const uint LimitPain = 1u << 2;
    public const uint NoPassMobj = 1u << 4;
    public const uint WallRun = 1u << 6;
    public const uint NoTossDrops = 1u << 7;
    public const uint UseBlocking = 1u << 8;
    public const uint NoDoorLight = 1u << 9;
    public const uint SoundTarget = 1u << 11;
    public const uint DehHealth = 1u << 12;
    public const uint CompatTrace = 1u << 13;
    public const uint BoomScroll = 1u << 15;
    public const uint Invisibility = 1u << 16;
    public const uint MissileClip = 1u << 19;
    public const uint CrossDropoff = 1u << 20;
    public const uint Mushroom = 1u << 23;
    public const uint MbfMonsterMove = 1u << 24;
    public const uint VileGhosts = 1u << 25;
    public const uint NoBlockFriends = 1u << 26;
    public const uint SpriteSort = 1u << 27;
    public const uint Hitscan = 1u << 28;
    public const uint Light = 1u << 29;
    public const uint MaskedMidtex = 1u << 31;

    public const uint BadAngles = 1u << 0;
    public const uint FloorMove = 1u << 1;
    public const uint PointOnLine = 1u << 3;
    public const uint MultiExit = 1u << 4;
    public const uint Teleport = 1u << 5;
    public const uint PushWindow = 1u << 6;
    public const uint CheckSwitchRange = 1u << 7;
    public const uint Explode1 = 1u << 8;
    public const uint Explode2 = 1u << 9;
    public const uint AvoidHazards = 1u << 12;
    public const uint StayOnLift = 1u << 13;
    public const uint NoMbf21 = 1u << 14;
    public const uint VoodooZombies = 1u << 15;
    public const uint EmulateMikoPortals = 1u << 19;
    public const uint ReservedLineFlag = 1u << 20;

    public sealed class Level
    {
        public int CompatPasses;
        public int Compat2Passes;
        public int SidePasses;
    }

    public sealed class World
    {
        public bool AutomapActive;
        public bool LocalAuthority;
        public bool DemoPlayback;
        public bool Multiplayer;
        public float DesiredFov;
        public int CameraId;
        public int MoId;
        public bool FovSent;
        public float SentFov;
        public uint DmFlags2;
        public uint DmFlags3;
        public uint CompatFlags;
        public uint CompatFlags2;
        public readonly bool[] InGame = new bool[MaxPlayers];
        public readonly int[] Cheats = new int[MaxPlayers];
        public readonly List<string> Trace = new();
        public readonly List<Level> Levels = new();
    }

    public static int ApplyDmFlags(World world, int self)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        uint bits = unchecked((uint)self);
        world.Trace.Add("sky");
        if ((bits & NoFreelook) != 0)
            world.Trace.Add("center");
        if ((bits & NoFov) != 0 && world.LocalAuthority)
        {
            world.Trace.Add("fov");
            float fov = world.DesiredFov == 0f ? 90f : world.DesiredFov;
            world.FovSent = true;
            world.SentFov = fov;
        }
        return self;
    }

    public static int ApplyDmFlags2(World world, int self, Func<bool, bool> checkCheat, Action<int> chase)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (checkCheat == null)
            throw new ArgumentNullException(nameof(checkCheat));
        if (chase == null)
            throw new ArgumentNullException(nameof(chase));
        uint bits = unchecked((uint)self);
        world.DmFlags2 = bits;
        if ((bits & NoAutomap) != 0 && world.AutomapActive)
            world.Trace.Add("stopmap");
        if ((bits & DisallowSpying) != 0 && world.CameraId != world.MoId)
        {
            world.CameraId = world.MoId;
            world.Trace.Add("sounds");
            world.Trace.Add("attach");
            if (world.DemoPlayback || world.Multiplayer)
                world.Trace.Add("name");
        }
        for (int i = 0; i < MaxPlayers; i++)
        {
            if (!world.InGame[i])
                continue;
            if ((world.DmFlags2 & ChaseCamFlag) == 0 && checkCheat(false))
            {
                if ((world.Cheats[i] & ChaseCamCheat) != 0)
                    chase(i);
            }
        }
        return self;
    }

    public static int ApplyDmFlags3(World world, int self)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        world.DmFlags3 = unchecked((uint)self);
        return self;
    }

    public static int ApplyCompatFlags(World world, int self)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        world.CompatFlags = unchecked((uint)self);
        foreach (Level level in world.Levels)
            level.CompatPasses++;
        return self;
    }

    public static int ApplyCompatFlags2(World world, int self)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        world.CompatFlags2 = unchecked((uint)self);
        foreach (Level level in world.Levels)
        {
            level.Compat2Passes++;
            level.SidePasses++;
        }
        return self;
    }

    public static int ApplyCompatMode(World world, int self)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        uint v;
        uint w = 0;
        switch (self)
        {
            default:
            case 0:
                v = 0;
                break;
            case 1:
                v = ShortTex | StairIndex | UseBlocking | NoDoorLight | SpriteSort | CompatTrace
                    | MissileClip | SoundTarget | DehHealth | CrossDropoff | Light | MaskedMidtex;
                w = FloorMove | Explode1 | NoMbf21;
                break;
            case 2:
                v = ShortTex | StairIndex | UseBlocking | NoDoorLight | SpriteSort | CompatTrace
                    | MissileClip | SoundTarget | NoPassMobj | LimitPain | DehHealth | Invisibility
                    | CrossDropoff | VileGhosts | Hitscan | WallRun | NoTossDrops | Light | MaskedMidtex;
                w = BadAngles | FloorMove | PointOnLine | Explode2 | NoMbf21 | VoodooZombies | EmulateMikoPortals;
                break;
            case 3:
                v = CompatTrace | SoundTarget | BoomScroll | MissileClip | MaskedMidtex;
                w = Explode1 | NoMbf21;
                break;
            case 4:
                v = SoundTarget | Light;
                w = MultiExit | Teleport | PushWindow | CheckSwitchRange | NoMbf21;
                break;
            case 5:
                v = CompatTrace | SoundTarget | BoomScroll | MissileClip | Mushroom | MbfMonsterMove
                    | NoBlockFriends | MaskedMidtex;
                w = Explode1 | AvoidHazards | StayOnLift | NoMbf21;
                break;
            case 6:
                v = CompatTrace | SoundTarget | BoomScroll | MissileClip | NoPassMobj | Invisibility
                    | Hitscan | WallRun | NoTossDrops | MaskedMidtex;
                w = PointOnLine | Explode2 | NoMbf21 | EmulateMikoPortals;
                break;
            case 7:
                v = NoBlockFriends | MbfMonsterMove | Invisibility | NoTossDrops | Mushroom | NoPassMobj
                    | BoomScroll | WallRun | CompatTrace | Hitscan | MissileClip | MaskedMidtex | SoundTarget;
                w = PointOnLine | Explode1 | Explode2 | AvoidHazards | StayOnLift | NoMbf21;
                break;
            case 8:
                v = CompatTrace | SoundTarget | BoomScroll | MissileClip | CrossDropoff | Mushroom
                    | MbfMonsterMove | NoBlockFriends | MaskedMidtex;
                w = Explode1 | AvoidHazards | StayOnLift | ReservedLineFlag;
                break;
            case 9:
                v = NoBlockFriends | MbfMonsterMove | Invisibility | NoTossDrops | Mushroom | NoPassMobj
                    | BoomScroll | WallRun | CompatTrace | Hitscan | MissileClip | CrossDropoff
                    | MaskedMidtex | SoundTarget;
                w = PointOnLine | Explode1 | Explode2 | AvoidHazards | StayOnLift | EmulateMikoPortals
                    | ReservedLineFlag;
                break;
        }
        ApplyCompatFlags(world, unchecked((int)v));
        ApplyCompatFlags2(world, unchecked((int)w));
        return self;
    }
}
