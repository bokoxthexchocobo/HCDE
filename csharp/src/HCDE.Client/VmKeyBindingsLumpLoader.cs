using HCDE.Scripting;

namespace HCDE.Client;

/// <summary><c>ReadBindings</c> subset from <c>c_bind.cpp</c>.</summary>
public static class VmKeyBindingsLumpLoader
{
    public static void Load(
        VmKeyBindConsole console,
        string text,
        bool overrideBindings,
        IReadOnlyList<int>? filter = null)
    {
        var sc = new VmFScanner();
        sc.OpenString("binds", text);
        while (sc.GetString())
        {
            VmKeyBindings dest = console.Bindings;
            if (sc.Compare("unbind"))
            {
                sc.MustGetString();
                if (!overrideBindings) continue;
                var key = VmKeyNames.GetKeyFromName(sc.String);
                if (filter is not null && !filter.Contains(key)) continue;
                dest.UnbindKey(sc.String);
                continue;
            }

            if (sc.Compare("bind"))
                sc.MustGetString();
            else if (sc.Compare("doublebind"))
            {
                dest = console.DoubleBindings;
                sc.MustGetString();
            }
            else if (sc.Compare("mapbind"))
            {
                dest = console.AutomapBindings;
                sc.MustGetString();
            }

            var keyNum = VmKeyNames.GetConfigKeyFromName(sc.String);
            if (filter is not null && !filter.Contains(keyNum)) continue;
            sc.MustGetString();
            dest.SetBind(keyNum, sc.String, overrideBindings);
        }
    }
}
