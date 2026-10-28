namespace HCDE.Client;

/// <summary>Portable <c>FGameConfigFile::DoGlobalSetup</c> version gates (no <c>FindCVar</c>).</summary>
public static class VmGameConfigVersionMigrations
{
    public static readonly string[] ScaleCvarsToResetAt214 =
    [
        "hud_scale", "st_scale", "hud_althudscale", "con_scale", "con_scaletext", "uiscale"
    ];

    public static void Apply(double lastRunVersion, IDictionary<string, string> cvars)
    {
        if (lastRunVersion >= 231) return;
        if (lastRunVersion < 213 && cvars.TryGetValue("snd_channels", out var ch)
            && int.TryParse(ch, out var channels) && channels < 64)
        {
            cvars.Remove("snd_channels");
        }

        if (lastRunVersion < 214)
        {
            foreach (var name in ScaleCvarsToResetAt214) cvars.Remove(name);
        }

        if (lastRunVersion < 215) cvars.Remove("snd_hrtf");

        if (lastRunVersion < 216 && cvars.TryGetValue("gl_texture_hqresize", out var hq)
            && int.TryParse(hq, out var legacy))
        {
            var (mode, mult) = MapLegacyHqResize(legacy);
            cvars["gl_texture_hqresizemode"] = mode.ToString();
            cvars["gl_texture_hqresizemult"] = mult.ToString();
            cvars.Remove("gl_texture_hqresize");
        }

        if (lastRunVersion < 222 && cvars.TryGetValue("mod_dumb_mastervolume", out var vol)
            && float.TryParse(vol, out var master))
        {
            master /= 4f;
            if (master < 1f) master = 1f;
            cvars["mod_dumb_mastervolume"] = master.ToString("G");
        }

        if (lastRunVersion < 223)
        {
            var soundBg = cvars.TryGetValue("i_soundinbackground", out var s)
                && bool.TryParse(s, out var sb) && sb;
            cvars["i_pauseinbackground"] = (!soundBg).ToString();
        }

        if (lastRunVersion < 224 && cvars.TryGetValue("m_sensitivity_x", out var sens)
            && float.TryParse(sens, out var sx))
        {
            cvars["m_sensitivity_x"] = (sx * 0.5f).ToString("G");
        }

        if (lastRunVersion < 225 && cvars.TryGetValue("gl_lightmode", out var lm)
            && int.TryParse(lm, out var light))
        {
            cvars["gl_lightmode"] = MapGlLightmode(light).ToString();
        }

        if (lastRunVersion < 221)
        {
            ApplyMouseSensitivity221(cvars);
            cvars["adl_volume_model"] = "0";
            cvars["adl_chan_alloc"] = "-1";
            cvars["adl_auto_arpeggio"] = "false";
            cvars["opn_volume_model"] = "0";
            cvars["opn_chan_alloc"] = "-1";
            cvars["opn_auto_arpeggio"] = "false";
            if (cvars.TryGetValue("gl_texture_hqresize_targets", out var tgt)
                && int.TryParse(tgt, out var targets))
            {
                cvars["gl_texture_hqresize_targets"] = ExpandHqResizeTargets(targets).ToString();
            }
        }

        if (lastRunVersion < 231) cvars["language"] = "auto";
    }

    public static (int Mode, int Mult) MapLegacyHqResize(int legacy) => legacy switch
    {
        1 => (1, 2),
        2 => (1, 3),
        3 => (1, 4),
        4 => (2, 2),
        5 => (2, 3),
        6 => (2, 4),
        7 => (3, 2),
        8 => (3, 3),
        9 => (3, 4),
        10 => (4, 2),
        11 => (4, 3),
        12 => (4, 4),
        18 => (4, 5),
        19 => (4, 6),
        13 => (5, 2),
        14 => (5, 3),
        15 => (5, 4),
        16 => (5, 5),
        17 => (5, 6),
        20 => (6, 2),
        21 => (6, 3),
        22 => (6, 4),
        23 => (6, 5),
        24 => (6, 6),
        _ => (0, 1)
    };

    public static int MapGlLightmode(int value) => value switch
    {
        16 => 2,
        8 => 1,
        _ => 0
    };

    public static int ExpandHqResizeTargets(int oldTargets) =>
        oldTargets | ((oldTargets & 1) != 0 ? 8 : 0);

    private static void ApplyMouseSensitivity221(IDictionary<string, string> cvars)
    {
        double xfact = 3, yfact = 2;
        if (cvars.TryGetValue("m_noprescale", out var np) && bool.TryParse(np, out var noPre) && noPre)
            xfact = yfact = 1;
        if (cvars.TryGetValue("mouse_sensitivity", out var ms) && double.TryParse(ms, out var sens))
        {
            xfact *= sens;
            yfact *= sens;
        }

        cvars["m_sensitivity_x"] = xfact.ToString("G");
        cvars["m_sensitivity_y"] = yfact.ToString("G");
    }
}
