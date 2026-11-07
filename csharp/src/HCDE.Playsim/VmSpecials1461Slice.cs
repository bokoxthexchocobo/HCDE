namespace HCDE.Playsim;

/// <summary>
/// InitSectorSpecial tail from specials.cpp 459-614. Lines 456-458 are already credited.
/// </summary>
public static class VmSpecials1461Slice
{
    public const int BandStart = 459;
    public const int BandEnd = 614;
    public const int DamageMask = 0x0300;
    public const int DeathMask = 0x8000;
    public const int Telefrag = 1000000;
    public const int FloorScroll = 1;
    public const int DoorWaitRaise = 3;
    public const int DoorWaitClose = 5;

    static readonly int[] ScrollDx = { 0, 0, 0, -1, -2, -4, 0, 0, 0, 1, 2, 4, 1, 2, 4, -1, -2, -4, -1, -2, -4, 1, 2, 4 };
    static readonly int[] ScrollDy = { 1, 2, 4, 0, 0, 0, -1, -2, -4, 0, 0, 0, 1, 2, 4, 1, 2, 4, -1, -2, -4, -1, -2, -4 };

    public sealed class Sector
    {
        public int Special;
        public int Flags;
        public int MoreFlags;
        public double Friction;
        public double MoveFactor;
        public int Sky;
    }

    public sealed class Damage
    {
        public int Amount, Interval, Leak, Flags;
        public string Type = "";
    }

    public sealed class Scroll
    {
        public int Kind;
        public double X, Y;
    }

    public sealed class Door
    {
        public int Type, Speed, Delay, Retrace, Wait;
    }

    public sealed class World
    {
        public bool Mbf21;
        public bool Lights;
        public List<Damage> Damages = new();
        public List<Scroll> Scrolls = new();
        public List<Door> Doors = new();
    }

    public static void InitTail(World w, Sector sector)
    {
        int special = sector.Special;
        if ((special & DeathMask) == 0 || !w.Mbf21)
        {
            int bits = special & DamageMask;
            if (bits == 0x100) Hurt(w, sector, 5, 32, 0, "Fire", 0);
            else if (bits == 0x200) Hurt(w, sector, 10, 32, 0, "Slime", 0);
            else if (bits == 0x300) Hurt(w, sector, 20, 32, 5, "Slime", 0);
        }
        else
        {
            int bits = special & DamageMask;
            if (bits == 0x100) Hurt(w, sector, Telefrag, 1, 256, "InstantDeath", 0);
            else if (bits == 0x200) sector.Flags |= 4096;
            else if (bits == 0x300) sector.Flags |= 8192;
            else Hurt(w, sector, Telefrag - 1, 0, 0, "InstantDeath", 0);
        }
        sector.Special &= 0xff;
        w.Lights = true;
        bool keep = false;
        switch (sector.Special)
        {
            case 68: Hurt(w, sector, 20, 32, 5, "Slime", 0); break;
            case 69: Hurt(w, sector, 10, 32, 0, "Slime", 0); break;
            case 71: Hurt(w, sector, 5, 32, 0, "Slime", 0); break;
            case 74: w.Doors.Add(new Door { Type = DoorWaitClose, Speed = 2, Wait = 30 * 35 }); break;
            case 75: Hurt(w, sector, 20, 32, 256, "None", 256 | 512); break;
            case 78: w.Doors.Add(new Door { Type = DoorWaitRaise, Speed = 2, Delay = 35 * 30 / 7, Wait = 5 * 60 * 35 }); break;
            case 79:
                sector.Friction = 0xf900 / 65536.0;
                sector.MoveFactor = 0x269 / 65536.0;
                sector.Flags |= 16;
                break;
            case 80: Hurt(w, sector, 20, 32, 5, "Slime", 0); break;
            case 82: Hurt(w, sector, 5, 16, 256, "Fire", 128); break;
            case 83: Hurt(w, sector, 8, 16, 256, "Fire", 128); break;
            case 84:
                Hurt(w, sector, 5, 16, 256, "Fire", 128);
                w.Scrolls.Add(new Scroll { Kind = FloorScroll, X = -4, Y = 0 });
                keep = true;
                break;
            case 85: Hurt(w, sector, 4, 32, 0, "Slime", 0); break;
            case 104: Hurt(w, sector, 5, 32, 0, "Slime", 0); break;
            case 105: Hurt(w, sector, 2, 32, 0, "Slime", 1024); break;
            case 115: Hurt(w, sector, Telefrag, 1, 256, "InstantDeath", 0); break;
            case 116: Hurt(w, sector, 4, 32, 0, "Slime", 1024); break;
            case 195: sector.MoreFlags |= 256; break;
            case 196: Hurt(w, sector, -1, 32, 0, "None", 0); break;
            case 200: sector.Sky = 0x40000000; break;
            default:
                if (sector.Special >= 201 && sector.Special <= 224)
                {
                    int i = sector.Special - 201;
                    w.Scrolls.Add(new Scroll { Kind = FloorScroll, X = ScrollDx[i] / 2.0, Y = ScrollDy[i] / 2.0 });
                }
                else if (sector.Special >= 225 && sector.Special <= 229)
                    w.Scrolls.Add(new Scroll { Kind = FloorScroll, X = -0.5 * (1 << ((sector.Special & 0xff) - 225)), Y = 0 });
                keep = true;
                break;
        }
        if (!keep) sector.Special = 0;
    }

    static void Hurt(World w, Sector sector, int amount, int interval, int leak, string type, int flags)
    {
        w.Damages.Add(new Damage { Amount = amount, Interval = interval, Leak = leak, Type = type, Flags = flags });
    }
}
