namespace HCDE.Playsim;

/// <summary>
/// Testable seam mirroring <c>FSerializer</c> field/object/archive calls used by model serialize helpers.
/// Shared for future <c>DActorModelData</c> / playsim archive ports; not tied to a single chunk credit row.
/// </summary>
public interface IModelArchiveFieldSink
{
    bool Writing { get; }
    bool HasKey(string key);
    bool BeginObject(string key);
    void EndObject();
    int ReadInt(string key, int current);
    void WriteInt(string key, int value);
    double ReadDouble(string key, double current);
    void WriteDouble(string key, double value);
    int[] ReadIntArray(string key, int[] current);
    void WriteIntArray(string key, int[] value);
    string ReadString(string key, string current);
    void WriteString(string key, string value);
    void ArcVector3(string key, ref double x, ref double y, ref double z);
}
