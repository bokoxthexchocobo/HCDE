namespace HCDE.Playsim;

/// <summary>
/// VOXELDEF sprite lists, option blocks, and <c>R_InitVoxels</c> from
/// <c>voxeldef.cpp</c> (1-331). Chunk 1338. Lump lookup, sprite index,
/// and voxel load stay injected.
/// </summary>
public sealed class VmVoxelDef1338
{
    public const int MaxSpriteFrames = 29;
    public const int TkIdentifier = 257;
    public const int TkString = 258;
    public const int TkInt = 260;
    public const int TkFloat = 262;
    public const int NsVoxels = 11;

    public sealed class Voxel
    {
        public bool PaletteRemoved { get; private set; }

        public void RemovePalette()
        {
            PaletteRemoved = true;
        }
    }

    public sealed class VoxelDef
    {
        public Voxel? Voxel { get; init; }
        public double Scale { get; init; }
        public int DroppedSpin { get; init; }
        public int PlacedSpin { get; init; }
        public double AngleOffset { get; init; }
        public double XOffset { get; init; }
        public double YOffset { get; init; }
        public double ZOffset { get; init; }
        public bool PitchFromMomentum { get; init; }
        public bool UseActorPitch { get; init; }
        public bool UseActorRoll { get; init; }
    }

    public sealed class Options
    {
        public int DroppedSpin { get; set; }
        public int PlacedSpin { get; set; }
        public double Scale { get; set; } = 1;
        public double AngleOffset { get; set; } = 90;
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public double ZOffset { get; set; }
        public bool OverridePalette { get; set; }
        public bool PitchFromMomentum { get; set; }
        public bool UseActorPitch { get; set; }
        public bool UseActorRoll { get; set; }
    }

    public sealed class Token
    {
        public int Type { get; init; }
        public string Text { get; init; } = "";
        public double Float { get; init; }
        public int Number { get; init; }
    }

    public sealed class Scan
    {
        readonly List<Token> tokens = new();
        int index;
        Token? pending;

        public int TokenType { get; private set; }
        public string String { get; private set; } = "";
        public int StringLen => String.Length;
        public double Float { get; private set; }
        public int Number { get; private set; }
        public bool CMode { get; private set; }
        public List<bool> CModeHistory { get; } = new();
        public List<string> Messages { get; } = new();

        public Scan Add(int type, string text, int number = 0, double floatValue = 0)
        {
            tokens.Add(new Token { Type = type, Text = text, Number = number, Float = floatValue });
            return this;
        }

        public bool GetString()
        {
            var token = Pull();
            if (token == null)
                return false;
            Apply(token);
            return true;
        }

        public bool GetToken()
        {
            var token = Pull();
            if (token == null)
                return false;
            Apply(token);
            return true;
        }

        public void Unget()
        {
            pending = new Token { Type = TokenType, Text = String, Float = Float, Number = Number };
        }

        public void UseNumberAsFloat()
        {
            Float = Number;
        }

        public bool CheckToken(int token)
        {
            if (!GetToken())
                return false;
            if (TokenType == token)
                return true;
            Unget();
            return false;
        }

        public void MustGetAnyToken()
        {
            if (!GetToken())
                throw new InvalidOperationException("Missing token (unexpected end of file).");
        }

        public void TokenMustBe(int token)
        {
            if (TokenType != token)
                throw new InvalidOperationException("Expected token " + token + " but got " + TokenType);
        }

        public void MustGetToken(int token)
        {
            MustGetAnyToken();
            TokenMustBe(token);
        }

        public bool Compare(string text)
        {
            return AsciiEqual(text, String);
        }

        public void ScriptMessage(string message)
        {
            Messages.Add(message);
        }

        public void SetCMode(bool mode)
        {
            CMode = mode;
            CModeHistory.Add(mode);
        }

        public void UpperFrameChar()
        {
            var chars = String.ToCharArray();
            if (chars[4] is >= 'a' and <= 'z')
                chars[4] = (char)(chars[4] - 32);
            String = new string(chars);
        }

        Token? Pull()
        {
            if (pending != null)
            {
                var held = pending;
                pending = null;
                return held;
            }

            if (index >= tokens.Count)
                return null;
            return tokens[index++];
        }

        void Apply(Token token)
        {
            TokenType = token.Type;
            String = token.Text;
            Float = token.Float;
            Number = token.Number;
        }

        static bool AsciiEqual(string left, string right)
        {
            if (left.Length != right.Length)
                return false;
            for (var i = 0; i < left.Length; i++)
            {
                if (Fold(left[i]) != Fold(right[i]))
                    return false;
            }

            return true;
        }

        static char Fold(char c)
        {
            return c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
        }
    }

    public delegate int FindLumpDelegate(string name, ref int lastLump);

    public FindLumpDelegate? FindLump { get; set; }
    public Func<int, Scan>? Open { get; set; }
    public Func<string, int>? SpriteIndex { get; set; }
    public Func<string, bool, int, int>? FullName { get; set; }
    public Func<int, Voxel?>? GetVoxel { get; set; }
    public List<VoxelDef> VoxelDefs { get; } = new();
    public List<(int Sprite, int Frame, VoxelDef Def)> Added { get; } = new();

    public bool ReadSpriteNames(Scan scan, List<uint> sprites)
    {
        sprites.Clear();
        while (scan.GetString())
        {
            if (scan.String.Length > 0 && scan.String[0] == '=')
            {
                if (sprites.Count == 0)
                    scan.ScriptMessage("No sprites specified for voxel.\n");
                return true;
            }

            if (scan.StringLen != 4 && scan.StringLen != 5)
            {
                scan.ScriptMessage("Sprite name \"" + scan.String + "\" is wrong size.\n");
            }
            else if (scan.StringLen == 5 && (UpperAndInvalid(scan)))
            {
                scan.ScriptMessage("Sprite frame " + scan.String[4] + " is invalid.\n");
            }
            else
            {
                if (SpriteIndex == null)
                    throw new InvalidOperationException("GetSpriteIndex");
                var frame = scan.StringLen == 4 ? 255 : scan.String[4] - 'A';
                var index = SpriteIndex(scan.String);
                if (index != -1)
                    sprites.Add(((uint)frame << 24) | (uint)index);
            }
        }

        if (sprites.Count != 0)
            scan.ScriptMessage("Unexpected end of file\n");
        return false;
    }

    public void ReadOptions(Scan scan, Options opts)
    {
        while (scan.GetToken())
        {
            if (scan.TokenType == '}')
                return;
            scan.TokenMustBe(TkIdentifier);
            if (scan.Compare("scale"))
            {
                scan.MustGetToken('=');
                scan.MustGetToken(TkFloat);
                opts.Scale = scan.Float;
            }
            else if (scan.Compare("spin"))
            {
                var mul = Signed(scan);
                opts.DroppedSpin = opts.PlacedSpin = scan.Number * mul;
            }
            else if (scan.Compare("placedspin"))
            {
                var mul = Signed(scan);
                opts.PlacedSpin = scan.Number * mul;
            }
            else if (scan.Compare("droppedspin"))
            {
                var mul = Signed(scan);
                opts.DroppedSpin = scan.Number * mul;
            }
            else if (scan.Compare("angleoffset"))
            {
                var mul = SignedAny(scan);
                opts.AngleOffset = mul * scan.Float + 90;
            }
            else if (scan.Compare("xoffset"))
            {
                var mul = SignedAny(scan);
                opts.XOffset = scan.Float * mul;
            }
            else if (scan.Compare("yoffset"))
            {
                var mul = SignedAny(scan);
                opts.YOffset = scan.Float * mul;
            }
            else if (scan.Compare("zoffset"))
            {
                var mul = SignedAny(scan);
                opts.ZOffset = scan.Float * mul;
            }
            else if (scan.Compare("overridepalette"))
            {
                opts.OverridePalette = true;
            }
            else if (scan.Compare("pitchfrommomentum"))
            {
                opts.PitchFromMomentum = true;
            }
            else if (scan.Compare("useactorpitch"))
            {
                opts.UseActorPitch = true;
            }
            else if (scan.Compare("useactorroll"))
            {
                opts.UseActorRoll = true;
            }
            else
            {
                scan.ScriptMessage("Unknown voxel option '" + scan.String + "'\n");
                if (scan.CheckToken('='))
                    scan.MustGetAnyToken();
            }
        }

        scan.ScriptMessage("Unterminated voxel option block\n");
    }

    public void Init()
    {
        if (FindLump == null || Open == null || FullName == null || GetVoxel == null)
            throw new InvalidOperationException("R_InitVoxels");
        var lastLump = 0;
        int lump;
        while ((lump = FindLump("VOXELDEF", ref lastLump)) != -1)
        {
            var scan = Open(lump);
            var sprites = new List<uint>();
            while (ReadSpriteNames(scan, sprites))
            {
                Voxel? voxel = null;
                scan.SetCMode(true);
                scan.MustGetToken(TkString);
                var file = FullName(scan.String, true, NsVoxels);
                if (file < 0)
                {
                    scan.ScriptMessage("Voxel \"" + scan.String + "\" not found.\n");
                }
                else
                {
                    voxel = GetVoxel(file);
                    if (voxel == null)
                        scan.ScriptMessage("\"" + scan.String + "\" is not a valid voxel file.\n");
                }

                var opts = new Options();
                if (scan.CheckToken('{'))
                    ReadOptions(scan, opts);
                scan.SetCMode(false);
                if (voxel != null && sprites.Count != 0)
                {
                    if (opts.OverridePalette)
                        voxel.RemovePalette();
                    var def = new VoxelDef
                    {
                        Voxel = voxel,
                        Scale = opts.Scale,
                        DroppedSpin = opts.DroppedSpin,
                        PlacedSpin = opts.PlacedSpin,
                        AngleOffset = opts.AngleOffset,
                        XOffset = opts.XOffset,
                        YOffset = opts.YOffset,
                        ZOffset = opts.ZOffset,
                        PitchFromMomentum = opts.PitchFromMomentum,
                        UseActorPitch = opts.UseActorPitch,
                        UseActorRoll = opts.UseActorRoll,
                    };
                    VoxelDefs.Add(def);
                    foreach (var packed in sprites)
                    {
                        var sprnum = (int)(packed & 0xFFFFFF);
                        var frame = (int)(packed >> 24);
                        if (frame == 255)
                        {
                            for (var j = MaxSpriteFrames - 1; j >= 0; j--)
                                Added.Add((sprnum, j, def));
                        }
                        else
                        {
                            Added.Add((sprnum, frame, def));
                        }
                    }
                }
            }
        }
    }

    static bool UpperAndInvalid(Scan scan)
    {
        scan.UpperFrameChar();
        return scan.String[4] < 'A' || scan.String[4] >= 'A' + MaxSpriteFrames;
    }

    static int Signed(Scan scan)
    {
        var mul = 1;
        scan.MustGetToken('=');
        if (scan.CheckToken('-'))
            mul = -1;
        scan.MustGetToken(TkInt);
        return mul;
    }

    static int SignedAny(Scan scan)
    {
        var mul = 1;
        scan.MustGetToken('=');
        if (scan.CheckToken('-'))
            mul = -1;
        scan.MustGetAnyToken();
        if (scan.TokenType == TkInt)
            scan.UseNumberAsFloat();
        else
            scan.TokenMustBe(TkFloat);
        return mul;
    }
}
