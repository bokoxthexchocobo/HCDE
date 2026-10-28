namespace HCDE.Playsim;

/// <summary><c>G_DoPlayerPop</c> from <c>g_game.cpp</c> (2453–2499).</summary>
public static class VmGGamePlayerPop
{
    public static bool ShouldRevertSpyCamera(bool inGame, object? camera, object? leavingMob) =>
        inGame && ReferenceEquals(camera, leavingMob);

    public static bool ShouldAttachStatusBar(int playerIndex, int consolePlayer) =>
        playerIndex == consolePlayer;

    public static bool ShouldDestroyMorphedReplacement(object? replacement, object? destroyedMob, bool euthanizeMe) =>
        replacement != null && !ReferenceEquals(replacement, destroyedMob) && !euthanizeMe;
}
