using HCDE.Net.Core;

namespace HCDE.Net.Pregame;

public static class CrossLanguageSoakSuite
{
    public static IReadOnlyList<CrossLanguageSoakResult> RunAll(string? repositoryRoot = null)
    {
        return
        [
            PregameCrossLanguageSoak.RunPregameGuestSmoke(repositoryRoot),
            NetcodeCrossLanguageSoak.RunStep12InvasionSmoke(repositoryRoot),
        ];
    }
}
