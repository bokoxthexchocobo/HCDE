namespace HCDE.Gamedata;

/// <summary><c>FDecalLib::ReadAllDecals</c> lump ingest from <c>decallib.cpp</c>.</summary>
public static class VmDecalDocumentLoader
{
    public static void LoadAllDecalLumps(IEnumerable<string> lumps, VmDecalLibCore lib, VmDecalDatabase db, List<string> errors)
    {
        lib.Clear();
        db.Decals.Clear();
        db.Groups.Clear();
        foreach (var lump in lumps)
        {
            if (!VmDecalLibDocumentParser.TryParseDocument(lump, db, errors))
                return;
        }
    }
}
