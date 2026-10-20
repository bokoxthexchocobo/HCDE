namespace HCDE.Gamedata;

/// <summary><c>SetSplashDefaults</c> / <c>ParseSplash</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainSplashParser
{
    public static void SetSplashDefaults(VmSplashDef splash)
    {
        splash.SmallSplashSound = "";
        splash.NormalSplashSound = "";
        splash.SmallSplashClass = "";
        splash.SplashBaseClass = "";
        splash.SplashChunkClass = "";
        splash.ChunkXVelShift = 8;
        splash.ChunkYVelShift = 8;
        splash.ChunkZVelShift = 8;
        splash.ChunkBaseZVel = 1;
        splash.SmallSplashClip = 12;
        splash.NoAlert = false;
    }

    public static bool TryParseSplash(string name, bool modify, string body, VmTerrainDatabase db, List<string> errors)
    {
        var index = db.FindSplash(name);
        var isNew = false;
        VmSplashDef splash;
        if (index < 0)
        {
            splash = new VmSplashDef { Name = name };
            SetSplashDefaults(splash);
            db.Splashes.Add(splash);
            isNew = true;
        }
        else
        {
            splash = db.Splashes[index];
        }

        if (modify && !isNew)
            SetSplashDefaults(splash);

        var tokens = VmLockDefTokenize.Tokenize(body);
        var i = 0;
        var ctx = new VmTerrainGenericContext
        {
            Database = db,
            Target = splash,
            OwnerType = "splash",
            OwnerName = name,
        };
        VmTerrainGenericParser.ParseBody(tokens, ref i, VmTerrainSplashDescriptors.Keywords,
            VmTerrainSplashDescriptors.Parser, ctx, errors);

        return errors.Count == 0;
    }
}
