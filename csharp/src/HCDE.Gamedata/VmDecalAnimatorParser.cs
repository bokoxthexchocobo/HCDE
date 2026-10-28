namespace HCDE.Gamedata;

/// <summary>Generator and animator parsers from <c>decallib.cpp</c>.</summary>
public static class VmDecalAnimatorParser
{
    private const double TickRate = 35.0;

    public static bool TryParseGenerator(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected actor class."); return false; }
        var optional = tokens[i].Equals("optional", StringComparison.OrdinalIgnoreCase);
        if (optional) i++;
        if (i >= tokens.Count) { errors.Add("Expected actor class."); return false; }
        var actor = tokens[i++];
        if (i >= tokens.Count) { errors.Add("Expected decal name."); return false; }
        var decalName = tokens[i++];
        string? decal = null;
        if (!decalName.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            if (!db.TryGetDecal(decalName, out _) && !optional)
            {
                errors.Add($"{decalName} has not been defined.");
                return false;
            }

            decal = decalName;
        }

        db.Generators.Add(new VmDecalGeneratorBinding { ActorClass = actor, DecalName = decal, Optional = optional });
        return true;
    }

    public static bool TryParseFader(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected fader name."); return false; }
        var name = tokens[i++];
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{' after fader name."); return false; }
        i++;
        var fader = new VmDecalFaderAnimator { Name = name };
        while (i < tokens.Count && tokens[i] != "}")
        {
            var key = tokens[i++];
            if (key.Equals("DecayStart", StringComparison.OrdinalIgnoreCase))
                fader.DecayStartTics = ReadTics(tokens, ref i, errors);
            else if (key.Equals("DecayTime", StringComparison.OrdinalIgnoreCase))
                fader.DecayTimeTics = ReadTics(tokens, ref i, errors);
            else { errors.Add($"Unknown fader parameter '{key}'."); return false; }
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        db.Faders.Add(fader);
        db.AnimatorNames[name] = name;
        return errors.Count == 0;
    }

    public static bool TryParseStretcher(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected stretcher name."); return false; }
        var name = tokens[i++];
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{'."); return false; }
        i++;
        var stretch = new VmDecalStretchAnimator { Name = name };
        while (i < tokens.Count && tokens[i] != "}")
        {
            var key = tokens[i++];
            if (key.Equals("StretchStart", StringComparison.OrdinalIgnoreCase))
                stretch.StretchStartTics = ReadTics(tokens, ref i, errors);
            else if (key.Equals("StretchTime", StringComparison.OrdinalIgnoreCase))
                stretch.StretchTimeTics = ReadTics(tokens, ref i, errors);
            else if (key.Equals("GoalX", StringComparison.OrdinalIgnoreCase))
                stretch.GoalX = VmDecalLibParser.ReadScale(ReadDouble(tokens, ref i, errors));
            else if (key.Equals("GoalY", StringComparison.OrdinalIgnoreCase))
                stretch.GoalY = VmDecalLibParser.ReadScale(ReadDouble(tokens, ref i, errors));
            else { errors.Add($"Unknown stretcher parameter '{key}'."); return false; }
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        if (stretch.GoalX >= 0 || stretch.GoalY >= 0)
        {
            db.Stretchers.Add(stretch);
            db.AnimatorNames[name] = name;
        }

        return errors.Count == 0;
    }

    public static bool TryParseSlider(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected slider name."); return false; }
        var name = tokens[i++];
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{'."); return false; }
        i++;
        var slider = new VmDecalSliderAnimator { Name = name };
        while (i < tokens.Count && tokens[i] != "}")
        {
            var key = tokens[i++];
            if (key.Equals("SlideStart", StringComparison.OrdinalIgnoreCase))
                slider.SlideStartTics = ReadTics(tokens, ref i, errors);
            else if (key.Equals("SlideTime", StringComparison.OrdinalIgnoreCase))
                slider.SlideTimeTics = ReadTics(tokens, ref i, errors);
            else if (key.Equals("DistY", StringComparison.OrdinalIgnoreCase))
                slider.DistY = ReadDouble(tokens, ref i, errors);
            else if (key.Equals("DistX", StringComparison.OrdinalIgnoreCase))
                ReadDouble(tokens, ref i, errors);
            else { errors.Add($"Unknown slider parameter '{key}'."); return false; }
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        if (slider.DistY != 0)
        {
            db.Sliders.Add(slider);
            db.AnimatorNames[name] = name;
        }

        return errors.Count == 0;
    }

    public static bool TryParseColorChanger(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected colorchanger name."); return false; }
        var name = tokens[i++];
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{'."); return false; }
        i++;
        var color = new VmDecalColorAnimator { Name = name };
        while (i < tokens.Count && tokens[i] != "}")
        {
            var key = tokens[i++];
            if (key.Equals("FadeStart", StringComparison.OrdinalIgnoreCase))
                color.FadeStartTics = ReadTics(tokens, ref i, errors);
            else if (key.Equals("FadeTime", StringComparison.OrdinalIgnoreCase))
                color.FadeTimeTics = ReadTics(tokens, ref i, errors);
            else if (key.Equals("Color", StringComparison.OrdinalIgnoreCase))
                color.GoalColor = ReadString(tokens, ref i, errors);
            else { errors.Add($"Unknown color changer parameter '{key}'."); return false; }
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        db.ColorChangers.Add(color);
        db.AnimatorNames[name] = name;
        return errors.Count == 0;
    }

    public static bool TryParseCombiner(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected combiner name."); return false; }
        var name = tokens[i++];
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{'."); return false; }
        i++;
        var combiner = new VmDecalCombinerAnimator { Name = name };
        while (i < tokens.Count && tokens[i] != "}")
        {
            var anim = tokens[i++];
            if (!db.AnimatorNames.ContainsKey(anim))
            {
                errors.Add($"Undefined animator {anim}");
                return false;
            }

            combiner.AnimatorNames.Add(anim);
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        if (combiner.AnimatorNames.Count > 0)
        {
            db.Combiners.Add(combiner);
            db.AnimatorNames[name] = name;
        }

        return errors.Count == 0;
    }

    private static int ReadTics(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        var seconds = ReadDouble(tokens, ref i, errors);
        return (int)(seconds * TickRate);
    }

    private static string ReadString(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected string."); return ""; }
        return tokens[i++];
    }

    private static double ReadDouble(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !double.TryParse(tokens[i], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var f)) { errors.Add("Expected float."); return 0; }
        i++;
        return f;
    }
}
