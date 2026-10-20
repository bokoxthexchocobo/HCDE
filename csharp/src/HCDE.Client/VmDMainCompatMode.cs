namespace HCDE.Client;

/// <summary><c>CUSTOM_CVAR(compatmode)</c> presets from <c>d_main.cpp</c> (841–893).</summary>
public static class VmDMainCompatMode
{
    public readonly struct CompatPreset
    {
        public CompatPreset(int compatFlags, int compatFlags2)
        {
            CompatFlags = compatFlags;
            CompatFlags2 = compatFlags2;
        }

        public int CompatFlags { get; }
        public int CompatFlags2 { get; }
    }

    public static CompatPreset ResolvePreset(int compatMode)
    {
        return compatMode switch
        {
            1 => new CompatPreset(
                CompatF.ShortTex | CompatF.StairIndex | CompatF.UseBlocking | CompatF.NoDoorLight | CompatF.SpriteSort
                | CompatF.Trace | CompatF.MissileClip | CompatF.SoundTarget | CompatF.DehHealth | CompatF.CrossDropOff
                | CompatF.Light | CompatF.MaskedMidTex,
                CompatF2.FloorMove | CompatF2.Explode1 | CompatF2.NoMbf21),
            2 => new CompatPreset(
                CompatF.ShortTex | CompatF.StairIndex | CompatF.UseBlocking | CompatF.NoDoorLight | CompatF.SpriteSort
                | CompatF.Trace | CompatF.MissileClip | CompatF.SoundTarget | CompatF.NoPassMob | CompatF.LimitPain
                | CompatF.DehHealth | CompatF.Invisibility | CompatF.CrossDropOff | CompatF.VileGhosts | CompatF.Hitscan
                | CompatF.WallRun | CompatF.NoTossDrops | CompatF.Light | CompatF.MaskedMidTex,
                CompatF2.BadAngles | CompatF2.FloorMove | CompatF2.PointOnLine | CompatF2.Explode2 | CompatF2.NoMbf21
                | CompatF2.VoodooZombies | CompatF2.EmulateMikoPortals),
            3 => new CompatPreset(
                CompatF.Trace | CompatF.SoundTarget | CompatF.BoomScroll | CompatF.MissileClip | CompatF.MaskedMidTex,
                CompatF2.Explode1 | CompatF2.NoMbf21),
            4 => new CompatPreset(CompatF.SoundTarget | CompatF.Light,
                CompatF2.MultiExit | CompatF2.Teleport | CompatF2.PushWindow | CompatF2.CheckSwitchRange | CompatF2.NoMbf21),
            5 => new CompatPreset(
                CompatF.Trace | CompatF.SoundTarget | CompatF.BoomScroll | CompatF.MissileClip | CompatF.Mushroom
                | CompatF.MbfMonsterMove | CompatF.NoBlockFriends | CompatF.MaskedMidTex,
                CompatF2.Explode1 | CompatF2.AvoidHazards | CompatF2.StayOnLift | CompatF2.NoMbf21),
            6 => new CompatPreset(
                CompatF.Trace | CompatF.SoundTarget | CompatF.BoomScroll | CompatF.MissileClip | CompatF.NoPassMob
                | CompatF.Invisibility | CompatF.Hitscan | CompatF.WallRun | CompatF.NoTossDrops | CompatF.MaskedMidTex,
                CompatF2.PointOnLine | CompatF2.Explode2 | CompatF2.NoMbf21 | CompatF2.EmulateMikoPortals),
            7 => new CompatPreset(
                CompatF.NoBlockFriends | CompatF.MbfMonsterMove | CompatF.Invisibility | CompatF.NoTossDrops | CompatF.Mushroom
                | CompatF.NoPassMob | CompatF.BoomScroll | CompatF.WallRun | CompatF.Trace | CompatF.Hitscan
                | CompatF.MissileClip | CompatF.MaskedMidTex | CompatF.SoundTarget,
                CompatF2.PointOnLine | CompatF2.Explode1 | CompatF2.Explode2 | CompatF2.AvoidHazards | CompatF2.StayOnLift
                | CompatF2.NoMbf21),
            _ => new CompatPreset(0, 0),
        };
    }
}

internal static class CompatF
{
    public const int ShortTex = 1 << 0;
    public const int StairIndex = 1 << 1;
    public const int UseBlocking = 1 << 2;
    public const int NoDoorLight = 1 << 3;
    public const int SpriteSort = 1 << 4;
    public const int Trace = 1 << 5;
    public const int MissileClip = 1 << 6;
    public const int SoundTarget = 1 << 7;
    public const int DehHealth = 1 << 8;
    public const int CrossDropOff = 1 << 9;
    public const int Light = 1 << 10;
    public const int MaskedMidTex = 1 << 11;
    public const int NoPassMob = 1 << 12;
    public const int LimitPain = 1 << 13;
    public const int Invisibility = 1 << 14;
    public const int VileGhosts = 1 << 15;
    public const int Hitscan = 1 << 16;
    public const int WallRun = 1 << 17;
    public const int NoTossDrops = 1 << 18;
    public const int BoomScroll = 1 << 19;
    public const int Mushroom = 1 << 20;
    public const int MbfMonsterMove = 1 << 21;
    public const int NoBlockFriends = 1 << 22;
}

internal static class CompatF2
{
    public const int FloorMove = 1 << 0;
    public const int Explode1 = 1 << 1;
    public const int NoMbf21 = 1 << 2;
    public const int BadAngles = 1 << 3;
    public const int PointOnLine = 1 << 4;
    public const int Explode2 = 1 << 5;
    public const int VoodooZombies = 1 << 6;
    public const int EmulateMikoPortals = 1 << 7;
    public const int MultiExit = 1 << 8;
    public const int Teleport = 1 << 9;
    public const int PushWindow = 1 << 10;
    public const int CheckSwitchRange = 1 << 11;
    public const int AvoidHazards = 1 << 12;
    public const int StayOnLift = 1 << 13;
}
