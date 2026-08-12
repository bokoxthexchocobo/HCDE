namespace HCDE.Net.Core;

/// <summary>
/// Subset of <c>EDemoCommand</c> from <c>d_protocol.h</c> used by live netcode.
/// </summary>
public enum DemoCommand : byte
{
    Bad = 0,
    UserCmd = 1,
    EmptyUserCmd = 2,
    MusicChange = 4,
    Print = 5,
    CenterPrint = 6,
    UinfChanged = 8,
    SinfChanged = 9,
    GenericCheat = 10,
    GiveCheat = 11,
    Say = 12,
    Taunt = 13,
    ChangeMap = 14,
    Suicide = 15,
    AddBot = 16,
    KillBots = 17,
    InvUseAll = 18,
    InvUse = 19,
    Pause = 20,
    SaveGame = 21,
    Summon = 26,
    Fov = 27,
    MyFov = 28,
    ChangeMap2 = 29,
    RunScript = 32,
    SinfChangedXor = 33,
    InvDrop = 34,
    WarpCheat = 35,
    CenterView = 36,
    SummonFriend = 37,
    Spray = 38,
    Crouch = 39,
    RunScript2 = 40,
    CheckAutosave = 41,
    DoAutosave = 42,
    MorphEx = 43,
    SummonFoe = 44,
    TakeCheat = 47,
    AddController = 48,
    DelController = 49,
    KillClassCheat = 50,
    Summon2 = 52,
    SummonFriend2 = 53,
    SummonFoe2 = 54,
    AddSlotDefault = 55,
    AddSlot = 56,
    SetSlot = 57,
    SummonMbf = 58,
    ConvReply = 59,
    ConvClose = 60,
    ConvNull = 61,
    RunSpecial = 62,
    SetPitchLimit = 63,
    RunNamedScript = 65,
    RevertCamera = 66,
    SetSlotPnum = 67,
    Remove = 68,
    FinishGame = 69,
    NetEvent = 70,
    Mdk = 71,
    SetInv = 72,
    EndScreenJob = 73,
    ZscCmd = 74,
    ChangeSkill = 75,
    Kick = 76,
    Readied = 77,
    WeapSelect = 78,
    UseFlechette = 79,
}

public static class DemoCommandPolicy
{
    public static bool IsAllowedTicEvent(byte type) => type switch
    {
        (byte)DemoCommand.MusicChange or (byte)DemoCommand.Print or (byte)DemoCommand.CenterPrint
            or (byte)DemoCommand.UinfChanged or (byte)DemoCommand.SinfChanged or (byte)DemoCommand.GenericCheat
            or (byte)DemoCommand.GiveCheat or (byte)DemoCommand.Say or (byte)DemoCommand.Taunt
            or (byte)DemoCommand.ChangeMap or (byte)DemoCommand.Suicide or (byte)DemoCommand.AddBot
            or (byte)DemoCommand.KillBots or (byte)DemoCommand.InvUseAll or (byte)DemoCommand.InvUse
            or (byte)DemoCommand.Pause or (byte)DemoCommand.SaveGame or (byte)DemoCommand.Summon
            or (byte)DemoCommand.Fov or (byte)DemoCommand.MyFov or (byte)DemoCommand.ChangeMap2
            or (byte)DemoCommand.RunScript or (byte)DemoCommand.SinfChangedXor or (byte)DemoCommand.InvDrop
            or (byte)DemoCommand.WarpCheat or (byte)DemoCommand.CenterView or (byte)DemoCommand.SummonFriend
            or (byte)DemoCommand.Spray or (byte)DemoCommand.Crouch or (byte)DemoCommand.RunScript2
            or (byte)DemoCommand.CheckAutosave or (byte)DemoCommand.DoAutosave or (byte)DemoCommand.MorphEx
            or (byte)DemoCommand.SummonFoe or (byte)DemoCommand.TakeCheat or (byte)DemoCommand.AddController
            or (byte)DemoCommand.DelController or (byte)DemoCommand.KillClassCheat or (byte)DemoCommand.Summon2
            or (byte)DemoCommand.SummonFriend2 or (byte)DemoCommand.SummonFoe2 or (byte)DemoCommand.AddSlotDefault
            or (byte)DemoCommand.AddSlot or (byte)DemoCommand.SetSlot or (byte)DemoCommand.SummonMbf
            or (byte)DemoCommand.ConvReply or (byte)DemoCommand.ConvClose or (byte)DemoCommand.ConvNull
            or (byte)DemoCommand.RunSpecial or (byte)DemoCommand.SetPitchLimit or (byte)DemoCommand.RunNamedScript
            or (byte)DemoCommand.RevertCamera or (byte)DemoCommand.SetSlotPnum or (byte)DemoCommand.Remove
            or (byte)DemoCommand.FinishGame or (byte)DemoCommand.NetEvent or (byte)DemoCommand.Mdk
            or (byte)DemoCommand.SetInv or (byte)DemoCommand.EndScreenJob or (byte)DemoCommand.ZscCmd
            or (byte)DemoCommand.ChangeSkill or (byte)DemoCommand.Kick or (byte)DemoCommand.Readied
            or (byte)DemoCommand.WeapSelect or (byte)DemoCommand.UseFlechette => true,
        _ => false,
    };

    public static bool IsAllowedClientInput(byte type) => type switch
    {
        (byte)DemoCommand.UinfChanged or (byte)DemoCommand.Say or (byte)DemoCommand.Taunt
            or (byte)DemoCommand.Suicide or (byte)DemoCommand.InvUseAll or (byte)DemoCommand.InvUse
            or (byte)DemoCommand.Pause or (byte)DemoCommand.MyFov or (byte)DemoCommand.RunScript
            or (byte)DemoCommand.InvDrop or (byte)DemoCommand.CenterView or (byte)DemoCommand.Spray
            or (byte)DemoCommand.Crouch or (byte)DemoCommand.RunScript2 or (byte)DemoCommand.ConvReply
            or (byte)DemoCommand.ConvClose or (byte)DemoCommand.ConvNull or (byte)DemoCommand.RunSpecial
            or (byte)DemoCommand.SetPitchLimit or (byte)DemoCommand.RunNamedScript or (byte)DemoCommand.RevertCamera
            or (byte)DemoCommand.SetSlot or (byte)DemoCommand.AddSlot or (byte)DemoCommand.AddSlotDefault
            or (byte)DemoCommand.NetEvent or (byte)DemoCommand.ZscCmd or (byte)DemoCommand.Readied
            or (byte)DemoCommand.WeapSelect or (byte)DemoCommand.UseFlechette or (byte)DemoCommand.GenericCheat
            or (byte)DemoCommand.GiveCheat or (byte)DemoCommand.TakeCheat or (byte)DemoCommand.SetInv
            or (byte)DemoCommand.WarpCheat or (byte)DemoCommand.Summon or (byte)DemoCommand.SummonFriend
            or (byte)DemoCommand.SummonFoe or (byte)DemoCommand.MorphEx => true,
        _ => false,
    };
}
