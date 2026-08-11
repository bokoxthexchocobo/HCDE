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
    InvUse = 19,
    Pause = 20,
    MyFov = 28,
    RunScript = 32,
    InvDrop = 34,
    CenterView = 36,
    Spray = 38,
    Crouch = 39,
    RunScript2 = 40,
    ConvReply = 59,
    ConvClose = 60,
    ConvNull = 61,
    RunSpecial = 62,
    SetPitchLimit = 63,
    RunNamedScript = 65,
    RevertCamera = 66,
    NetEvent = 70,
    EndScreenJob = 73,
    ZscCmd = 74,
    Readied = 76,
    WeapSelect = 77,
    UseFlechette = 78,
}

public static class DemoCommandPolicy
{
    public static bool IsAllowedTicEvent(byte type) => type switch
    {
        (byte)DemoCommand.MusicChange or (byte)DemoCommand.Print or (byte)DemoCommand.CenterPrint
            or (byte)DemoCommand.UinfChanged or (byte)DemoCommand.SinfChanged or (byte)DemoCommand.GenericCheat
            or (byte)DemoCommand.GiveCheat or (byte)DemoCommand.Say or (byte)DemoCommand.Taunt
            or (byte)DemoCommand.ChangeMap or (byte)DemoCommand.Suicide or (byte)DemoCommand.InvUse
            or (byte)DemoCommand.Pause or (byte)DemoCommand.MyFov or (byte)DemoCommand.RunScript
            or (byte)DemoCommand.InvDrop or (byte)DemoCommand.CenterView or (byte)DemoCommand.Spray
            or (byte)DemoCommand.Crouch or (byte)DemoCommand.RunScript2 or (byte)DemoCommand.ConvReply
            or (byte)DemoCommand.ConvClose or (byte)DemoCommand.ConvNull or (byte)DemoCommand.RunSpecial
            or (byte)DemoCommand.SetPitchLimit or (byte)DemoCommand.RunNamedScript
            or (byte)DemoCommand.RevertCamera or (byte)DemoCommand.NetEvent or (byte)DemoCommand.EndScreenJob
            or (byte)DemoCommand.ZscCmd or (byte)DemoCommand.Readied or (byte)DemoCommand.WeapSelect
            or (byte)DemoCommand.UseFlechette => true,
        _ => false,
    };

    public static bool IsAllowedClientInput(byte type) => type switch
    {
        (byte)DemoCommand.UinfChanged or (byte)DemoCommand.Say or (byte)DemoCommand.Taunt
            or (byte)DemoCommand.Suicide or (byte)DemoCommand.InvUse or (byte)DemoCommand.Pause
            or (byte)DemoCommand.MyFov or (byte)DemoCommand.RunScript or (byte)DemoCommand.InvDrop
            or (byte)DemoCommand.CenterView or (byte)DemoCommand.Spray or (byte)DemoCommand.Crouch
            or (byte)DemoCommand.RunScript2 or (byte)DemoCommand.ConvReply or (byte)DemoCommand.ConvClose
            or (byte)DemoCommand.ConvNull or (byte)DemoCommand.RunSpecial or (byte)DemoCommand.SetPitchLimit
            or (byte)DemoCommand.RunNamedScript or (byte)DemoCommand.RevertCamera or (byte)DemoCommand.NetEvent
            or (byte)DemoCommand.ZscCmd or (byte)DemoCommand.Readied or (byte)DemoCommand.WeapSelect
            or (byte)DemoCommand.UseFlechette or (byte)DemoCommand.GenericCheat => true,
        _ => false,
    };
}
