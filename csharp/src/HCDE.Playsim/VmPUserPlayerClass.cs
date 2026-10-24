namespace HCDE.Playsim;

/// <summary>
/// <c>FPlayerClass</c> registry, validation/setup, and <c>player_t</c> copy/GC hooks from <c>p_user.cpp</c> (632–838).
/// </summary>
public static class VmPUserPlayerClass
{
    public const int PcfNomenu = 1;
    public const int Mf6Nomenu = 0x00400000;
    public const int MaxPlayers = 64;

    public static readonly object WpNoChange = VmPPSpriteWeaponBob.WpNoChange;

    public sealed class PlayerClassModel
    {
        public object? Type { get; set; }
        public uint Flags { get; set; }
        public List<int> Skins { get; } = new();
    }

    public sealed class ActorClassInfo
    {
        public string DisplayName { get; init; } = "";
        public bool IsPlayerPawnDescendant { get; init; }
        public int DefaultFlags6 { get; init; }
    }

    public sealed class AngleOffsetModel
    {
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double Roll { get; set; }

        public void CopyFrom(AngleOffsetModel other)
        {
            Pitch = other.Pitch;
            Yaw = other.Yaw;
            Roll = other.Roll;
        }
    }

    public sealed class SafePositionModel
    {
        public object? Sector { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double Height { get; set; }
        public double MaxStepHeight { get; set; }
        public bool ValidPos { get; set; }

        public void CopyFrom(SafePositionModel other)
        {
            Sector = other.Sector;
            PosX = other.PosX;
            PosY = other.PosY;
            PosZ = other.PosZ;
            Height = other.Height;
            MaxStepHeight = other.MaxStepHeight;
            ValidPos = other.ValidPos;
        }
    }

    public sealed class PlayerStateModel
    {
        public object? Mo { get; set; }
        public byte PlayerState { get; set; }
        public int Cmd { get; set; }
        public int OriginalCmd { get; set; }
        public uint OriginalOldButtons { get; set; }
        public object? UserInfoMarker { get; set; }
        public object? Cls { get; set; }
        public float DesiredFov { get; set; }
        public float Fov { get; set; }
        public double ViewZ { get; set; }
        public double ViewHeight { get; set; }
        public double DeltaViewHeight { get; set; }
        public double Bob { get; set; }
        public int BobTimer { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public bool Centering { get; set; }
        public byte TurnTicks { get; set; }
        public bool AttackDown { get; set; }
        public bool UseDown { get; set; }
        public uint OldButtons { get; set; }
        public int Health { get; set; }
        public int InventoryTics { get; set; }
        public int CurrentPlayerClass { get; set; }
        public int[] Frags { get; } = new int[MaxPlayers];
        public int FragCount { get; set; }
        public int LastKillTime { get; set; }
        public byte MultiCount { get; set; }
        public byte SpreeCount { get; set; }
        public ushort WeaponState { get; set; }
        public object? ReadyWeapon { get; set; }
        public object? PendingWeapon { get; set; }
        public object? Psprites { get; set; }
        public int Cheats { get; set; }
        public int TimeFreezer { get; set; }
        public short Refire { get; set; }
        public short Inconsistant { get; set; }
        public bool Waiting { get; set; }
        public int KillCount { get; set; }
        public int ItemCount { get; set; }
        public int SecretCount { get; set; }
        public uint DamageCount { get; set; }
        public uint BonusCount { get; set; }
        public int HazardCount { get; set; }
        public string HazardType { get; set; } = "";
        public int HazardInterval { get; set; }
        public int PoisonCount { get; set; }
        public string PoisonType { get; set; } = "";
        public string PoisonPainType { get; set; } = "";
        public object? Poisoner { get; set; }
        public object? Attacker { get; set; }
        public int ExtraLight { get; set; }
        public short FixedColormap { get; set; }
        public short FixedLightLevel { get; set; }
        public int FullbrightMode { get; set; }
        public bool ForceFullbright { get; set; }
        public int MorphTics { get; set; }
        public object? MorphedPlayerClass { get; set; }
        public int MorphStyle { get; set; }
        public object? MorphExitFlash { get; set; }
        public object? PremorphWeapon { get; set; }
        public int ChickenPeck { get; set; }
        public int JumpTics { get; set; }
        public bool OnGround { get; set; }
        public int RespawnTime { get; set; }
        public object? Camera { get; set; }
        public int AirFinished { get; set; }
        public string LastDamageType { get; set; } = "";
        public object? Bot { get; set; }
        public bool SettingsController { get; set; }
        public float BlendR { get; set; }
        public float BlendG { get; set; }
        public float BlendB { get; set; }
        public float BlendA { get; set; }
        public string LogText { get; set; } = "";
        public double MinPitch { get; set; }
        public double MaxPitch { get; set; }
        public sbyte Crouching { get; set; }
        public sbyte CrouchDir { get; set; }
        public double CrouchFactor { get; set; }
        public double CrouchOffset { get; set; }
        public double CrouchViewDelta { get; set; }
        public object? Weapons { get; set; }
        public object? ConversationNpc { get; set; }
        public object? ConversationPc { get; set; }
        public double ConversationNpcAngle { get; set; }
        public bool ConversationFaceTalker { get; set; }
        public object? MusInfoActor { get; set; }
        public sbyte MusInfoTics { get; set; }
        public string SoundClass { get; set; } = "";
        public SafePositionModel LastSafePos { get; } = new();
        public AngleOffsetModel AngleOffsetTargets { get; } = new();
    }

    public static bool CheckSkin(PlayerClassModel self, int skin)
    {
        for (var i = 0; i < self.Skins.Count; i++)
        {
            if (self.Skins[i] == skin)
                return true;
        }

        return false;
    }

    public static bool CheckSkinAction(PlayerClassModel self, int skin) => CheckSkin(self, skin);

    public static string GetPrintableDisplayName(object? cls, Func<object?, string>? getDisplayName) =>
        getDisplayName?.Invoke(cls) ?? "";

    public static bool ValidatePlayerClass(
        ActorClassInfo? info,
        string name,
        Action<string>? printf = null)
    {
        if (info is null)
        {
            printf?.Invoke($"Unknown player class '{name}'\n");
            return false;
        }

        if (!info.IsPlayerPawnDescendant)
        {
            printf?.Invoke($"Invalid player class '{name}'\n");
            return false;
        }

        if (string.IsNullOrEmpty(info.DisplayName))
        {
            printf?.Invoke($"Missing displayname for player class '{name}'\n");
            return false;
        }

        return true;
    }

    public static void SetupPlayerClasses(
        List<PlayerClassModel> playerClasses,
        IReadOnlyList<string> configuredNames,
        Func<string, ActorClassInfo?> resolveClass,
        Action<string>? printf = null)
    {
        playerClasses.Clear();
        foreach (var name in configuredNames)
        {
            var info = resolveClass(name);
            if (!ValidatePlayerClass(info, name, printf))
                continue;

            var entry = new PlayerClassModel
            {
                Type = info,
                Flags = 0,
            };
            if ((info!.DefaultFlags6 & Mf6Nomenu) != 0)
                entry.Flags |= (uint)PcfNomenu;

            playerClasses.Add(entry);
        }
    }

    public static void DestroyPlayer(PlayerStateModel self, Action? destroyPsprites) => destroyPsprites?.Invoke();

    public static void CopyFrom(PlayerStateModel self, PlayerStateModel source)
    {
        self.Mo = source.Mo;
        self.PlayerState = source.PlayerState;
        self.Cmd = source.Cmd;
        self.OriginalCmd = source.OriginalCmd;
        self.OriginalOldButtons = source.OriginalOldButtons;
        self.Cls = source.Cls;
        self.DesiredFov = source.DesiredFov;
        self.Fov = source.Fov;
        self.ViewZ = source.ViewZ;
        self.ViewHeight = source.ViewHeight;
        self.DeltaViewHeight = source.DeltaViewHeight;
        self.Bob = source.Bob;
        self.BobTimer = source.BobTimer;
        self.VelX = source.VelX;
        self.VelY = source.VelY;
        self.Centering = source.Centering;
        self.TurnTicks = source.TurnTicks;
        self.AttackDown = source.AttackDown;
        self.UseDown = source.UseDown;
        self.OldButtons = source.OldButtons;
        self.Health = source.Health;
        self.InventoryTics = source.InventoryTics;
        self.CurrentPlayerClass = source.CurrentPlayerClass;
        Array.Copy(source.Frags, self.Frags, MaxPlayers);
        self.FragCount = source.FragCount;
        self.LastKillTime = source.LastKillTime;
        self.MultiCount = source.MultiCount;
        self.SpreeCount = source.SpreeCount;
        self.WeaponState = source.WeaponState;
        self.ReadyWeapon = source.ReadyWeapon;
        self.PendingWeapon = source.PendingWeapon;
        self.Cheats = source.Cheats;
        self.TimeFreezer = source.TimeFreezer;
        self.Refire = source.Refire;
        self.Inconsistant = source.Inconsistant;
        self.Waiting = source.Waiting;
        self.KillCount = source.KillCount;
        self.ItemCount = source.ItemCount;
        self.SecretCount = source.SecretCount;
        self.DamageCount = source.DamageCount;
        self.BonusCount = source.BonusCount;
        self.HazardCount = source.HazardCount;
        self.HazardType = source.HazardType;
        self.HazardInterval = source.HazardInterval;
        self.PoisonCount = source.PoisonCount;
        self.PoisonType = source.PoisonType;
        self.PoisonPainType = source.PoisonPainType;
        self.Poisoner = source.Poisoner;
        self.Attacker = source.Attacker;
        self.ExtraLight = source.ExtraLight;
        self.FixedColormap = source.FixedColormap;
        self.FixedLightLevel = source.FixedLightLevel;
        self.FullbrightMode = source.FullbrightMode;
        self.ForceFullbright = source.ForceFullbright;
        self.MorphTics = source.MorphTics;
        self.MorphedPlayerClass = source.MorphedPlayerClass;
        self.MorphStyle = source.MorphStyle;
        self.MorphExitFlash = source.MorphExitFlash;
        self.PremorphWeapon = source.PremorphWeapon;
        self.ChickenPeck = source.ChickenPeck;
        self.JumpTics = source.JumpTics;
        self.OnGround = source.OnGround;
        self.RespawnTime = source.RespawnTime;
        self.Camera = source.Camera;
        self.AirFinished = source.AirFinished;
        self.LastDamageType = source.LastDamageType;
        self.Bot = source.Bot;
        self.SettingsController = source.SettingsController;
        self.BlendR = source.BlendR;
        self.BlendG = source.BlendG;
        self.BlendB = source.BlendB;
        self.BlendA = source.BlendA;
        self.LogText = source.LogText;
        self.MinPitch = source.MinPitch;
        self.MaxPitch = source.MaxPitch;
        self.Crouching = source.Crouching;
        self.CrouchDir = source.CrouchDir;
        self.CrouchFactor = source.CrouchFactor;
        self.CrouchOffset = source.CrouchOffset;
        self.CrouchViewDelta = source.CrouchViewDelta;
        self.Weapons = source.Weapons;
        self.ConversationNpc = source.ConversationNpc;
        self.ConversationPc = source.ConversationPc;
        self.ConversationNpcAngle = source.ConversationNpcAngle;
        self.ConversationFaceTalker = source.ConversationFaceTalker;
        self.MusInfoActor = source.MusInfoActor;
        self.MusInfoTics = source.MusInfoTics;
        self.SoundClass = source.SoundClass;
        self.LastSafePos.CopyFrom(source.LastSafePos);
        self.AngleOffsetTargets.CopyFrom(source.AngleOffsetTargets);
        self.Psprites = source.Psprites;
        source.Psprites = null;
    }

    public static nuint PropagateMark(PlayerStateModel self, Action<object?> mark, nuint structSize)
    {
        static void MarkIfPresent(object? obj, Action<object?> mark)
        {
            if (obj is not null)
                mark(obj);
        }

        MarkIfPresent(self.Mo, mark);
        MarkIfPresent(self.Poisoner, mark);
        MarkIfPresent(self.Attacker, mark);
        MarkIfPresent(self.Camera, mark);
        MarkIfPresent(self.Bot, mark);
        MarkIfPresent(self.ReadyWeapon, mark);
        MarkIfPresent(self.ConversationNpc, mark);
        MarkIfPresent(self.ConversationPc, mark);
        MarkIfPresent(self.MusInfoActor, mark);
        MarkIfPresent(self.PremorphWeapon, mark);
        MarkIfPresent(self.Psprites, mark);
        if (!ReferenceEquals(self.PendingWeapon, WpNoChange))
            MarkIfPresent(self.PendingWeapon, mark);
        return structSize;
    }
}
