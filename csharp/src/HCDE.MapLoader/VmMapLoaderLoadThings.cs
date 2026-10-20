namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadThings</c>, <c>LoadThings2</c>, and <c>MakeSkill</c> from <c>maploader.cpp</c> (1277–1428).</summary>
public static class VmMapLoaderLoadThings
{
    public const int MtfEasy = 1;
    public const int MtfMedium = 2;
    public const int MtfHard = 4;
    public const int MtfAmbush = 8;
    public const int MtfSkillMask = 0xf;

    public const int BtfNotDeathmatch = 1 << 4;
    public const int BtfNotCooperative = 1 << 5;
    public const int BtfFriendly = 1 << 6;
    public const int BtfBadEditorCheck = 1 << 7;
    public const int BtfNotSingle = 1 << 8;

    public const int StfShadow = 1;
    public const int StfAltShadow = 2;
    public const int StfStandstill = 4;
    public const int StfAmbush = 8;
    public const int StfFriendly = 16;

    public const int MtfDeathmatch = 1 << 4;
    public const int MtfCooperative = 1 << 5;
    public const int MtfShadow = 1 << 6;
    public const int MtfAltShadow = 1 << 7;
    public const int MtfStandstill = 1 << 8;
    public const int MtfAmbushFlag = 1 << 9;
    public const int MtfFriendly = 1 << 10;

    public const int MtfClassMask = 0x3c00;
    public const int MtfClassShift = 10;

    public const int Level2HexenHack = 1 << 0;

    public static int MakeSkill(int flags)
    {
        var res = 0;
        if ((flags & 1) != 0)
            res |= 1 + 2;
        if ((flags & 2) != 0)
            res |= 4;
        if ((flags & 4) != 0)
            res |= 8 + 16;
        return res;
    }

    public static int CountDoomThings(int lumpBytes, int recordSize) =>
        recordSize > 0 ? lumpBytes / recordSize : 0;

    public static void ApplyDoomThingDefaults(VmMapThingDraft draft)
    {
        draft.Gravity = 1;
        draft.Conversation = 0;
        draft.SkillFilter = 0;
        draft.ClassFilter = 0xffff;
        draft.RenderStyle = -1;
        draft.Alpha = -1;
        draft.Health = 1;
        draft.FloatbobPhase = -1;
        draft.LightNoShadowMapDefined = false;
        draft.LightDontLightActorsDefined = false;
        draft.LightDontLightMapDefined = false;
        draft.LightSoftShadowRadius = -1;
        draft.LightLinearity = -1;
        draft.LightShadowMinQuality = -1;
    }

    public static ushort TranslateDoomThingFlags(ushort options, bool strifeGame)
    {
        var flags = options;
        flags = (ushort)(flags & ~MtfSkillMask);
        var mtiFlags = (ushort)((flags & 0xf) | 0x7e0);
        if (strifeGame)
        {
            mtiFlags = (ushort)(mtiFlags & ~MtfAmbushFlag);
            if ((options & StfShadow) != 0)
                mtiFlags |= (ushort)MtfShadow;
            if ((options & StfAltShadow) != 0)
                mtiFlags |= (ushort)MtfAltShadow;
            if ((options & StfStandstill) != 0)
                mtiFlags |= (ushort)MtfStandstill;
            if ((options & StfAmbush) != 0)
                mtiFlags |= (ushort)MtfAmbushFlag;
            if ((options & StfFriendly) != 0)
                mtiFlags |= (ushort)MtfFriendly;
        }
        else
        {
            if ((options & BtfBadEditorCheck) != 0)
                flags &= 0x1f;
            if ((options & BtfNotDeathmatch) != 0)
                mtiFlags = (ushort)(mtiFlags & ~MtfDeathmatch);
            if ((options & BtfNotCooperative) != 0)
                mtiFlags = (ushort)(mtiFlags & ~MtfCooperative);
            if ((options & BtfFriendly) != 0)
                mtiFlags |= (ushort)MtfFriendly;
        }

        return mtiFlags;
    }

    public static void ApplyHexenThingDefaults(VmMapThingDraft draft)
    {
        ApplyDoomThingDefaults(draft);
        draft.FriendlySeeBlocks = -1;
    }

    public static ushort MaskHexenClassAndSkill(ushort flags) =>
        (ushort)(flags & ~(MtfSkillMask | MtfClassMask));

    public static int ExtractClassFilter(ushort flags) => (flags & MtfClassMask) >> MtfClassShift;

    public static ushort ApplyHexenHackMask(ushort flags, bool hexenHack) =>
        hexenHack ? (ushort)(flags & 0x7ff) : flags;

    public static int CountHexenThings(int lumpBytes, int recordSize) => CountDoomThings(lumpBytes, recordSize);

    public sealed class VmMapThingDraft
    {
        public int ThingId { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int PosZ { get; set; }
        public int Angle { get; set; }
        public int EdNum { get; set; }
        public ushort Flags { get; set; }
        public int Special { get; set; }
        public int[] Args { get; } = new int[5];
        public int SkillFilter { get; set; }
        public int ClassFilter { get; set; }
        public double Gravity { get; set; }
        public int Conversation { get; set; }
        public int RenderStyle { get; set; }
        public double Alpha { get; set; }
        public int Health { get; set; }
        public int FloatbobPhase { get; set; }
        public int FriendlySeeBlocks { get; set; }
        public bool LightNoShadowMapDefined { get; set; }
        public bool LightDontLightActorsDefined { get; set; }
        public bool LightDontLightMapDefined { get; set; }
        public double LightSoftShadowRadius { get; set; }
        public double LightLinearity { get; set; }
        public int LightShadowMinQuality { get; set; }
    }
}
