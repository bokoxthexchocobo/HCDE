namespace HCDE.Gamedata;

/// <summary>Default map actor dimensions and health from wadsrc/static/zscript/actors/doom.</summary>
public sealed record DoomActorDefinition(int EditorNumber, int Health, int Radius, int Height, int Speed, int PainChance, bool Floating = false);

public static class DoomActorCatalog
{
    private static readonly Dictionary<int, DoomActorDefinition> Definitions = new DoomActorDefinition[]
    {
        new(3004,20,20,56,8,200), new(9,30,20,56,8,170), new(65,70,20,56,8,170),
        new(84,50,20,56,8,170), new(3001,60,20,56,8,200),
        new(3002,150,30,56,10,180), new(58,150,30,56,10,180),
        new(3005,400,31,56,8,128,true), new(3003,1000,24,64,8,50), new(69,500,24,64,8,50),
        new(64,700,20,56,15,10), new(66,300,20,56,10,100), new(67,600,48,64,8,80),
        new(68,500,64,64,12,128), new(71,400,31,56,8,128,true), new(3006,100,16,56,8,256,true),
        new(16,4000,40,110,16,20), new(7,3000,128,100,12,40),
    }.ToDictionary(definition => definition.EditorNumber);

    public static DoomActorDefinition? Find(int editorNumber) => Definitions.GetValueOrDefault(editorNumber);
}
