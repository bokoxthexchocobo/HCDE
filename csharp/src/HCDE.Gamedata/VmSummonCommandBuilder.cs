namespace HCDE.Gamedata;

/// <summary><c>SummonActor</c> net message layout from <c>info.cpp</c>.</summary>
public static class VmSummonCommandBuilder
{
    public static byte[]? BuildSummonMessage(string actorType, int? angle = null, int? tid = null)
    {
        if (string.IsNullOrWhiteSpace(actorType)) return null;
        if (angle is null)
            return [0x01, .. System.Text.Encoding.UTF8.GetBytes(actorType)];
        var payload = new List<byte> { 0x02 };
        payload.AddRange(System.Text.Encoding.UTF8.GetBytes(actorType));
        payload.Add((byte)(angle & 0xFF));
        payload.Add((byte)((tid ?? 0) & 0xFF));
        return payload.ToArray();
    }
}
