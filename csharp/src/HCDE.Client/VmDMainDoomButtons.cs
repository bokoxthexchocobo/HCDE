namespace HCDE.Client;

/// <summary><c>DoomButtons</c> default bind table from <c>d_main.cpp</c> (3283–3316).</summary>
public static class VmDMainDoomButtons
{
    public static readonly string[] DefaultButtons =
    {
        "am_panleft", "user2", "jump", "right", "zoom", "back", "am_zoomin", "reload",
        "lookdown", "am_zoomout", "user4", "attack", "user1", "klook", "forward", "movedown",
        "altattack", "moveleft", "moveright", "am_panright", "am_panup", "mlook", "crouch",
        "left", "lookup", "user3", "strafe", "am_pandown", "showscores", "speed", "use", "moveup"
    };

    public static int ButtonCount => DefaultButtons.Length;

    public static string ResolveButton(int index) =>
        index >= 0 && index < DefaultButtons.Length ? DefaultButtons[index] : string.Empty;
}
