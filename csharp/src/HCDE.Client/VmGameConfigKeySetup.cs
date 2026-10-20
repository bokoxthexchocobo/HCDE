namespace HCDE.Client;

/// <summary><c>FGameConfigFile::DoKeySetup</c> using <see cref="VmKeyBindings"/>.</summary>
public static class VmGameConfigKeySetup
{
    private static readonly string[] BinderLabels = ["Bindings", "DoubleBindings", "AutomapBindings"];

    public static void DoKeySetup(
        VmConfigFile config,
        string gameName,
        VmKeyBindings bindings,
        VmKeyBindings doubleBindings,
        VmKeyBindings automapBindings,
        ref int resetBindFlags,
        Action? resetJoysticks = null,
        IReadOnlyList<int>? axisKeysToReset = null)
    {
        var binders = new[] { bindings, doubleBindings, automapBindings };
        foreach (var (label, binder) in BinderLabels.Zip(binders))
        {
            if (!config.SetSection(VmGameConfigGameSetup.GameSection(gameName, label))) continue;
            binder.UnbindAll();
            binder.LoadFromConfig(config);
        }

        if ((resetBindFlags & VmGameConfigResetBindFlags.V226GamePad) != 0)
        {
            resetBindFlags &= ~VmGameConfigResetBindFlags.V226GamePad;
            resetJoysticks?.Invoke();
            if (axisKeysToReset is not null)
            {
                bindings.UnbindAll(axisKeysToReset);
                doubleBindings.UnbindAll(axisKeysToReset);
                automapBindings.UnbindAll(axisKeysToReset);
            }
        }

        if ((resetBindFlags & VmGameConfigResetBindFlags.V230Gamma) != 0)
        {
            resetBindFlags &= ~VmGameConfigResetBindFlags.V230Gamma;
            bindings.UnbindACommand("bumpgamma");
            bindings.DefaultBind("F11", "bumplight");
        }

        config.OkayToWrite = true;
    }
}
