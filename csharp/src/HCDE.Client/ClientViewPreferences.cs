using System.Text.Json;
using System.Text.Json.Serialization;

namespace HCDE.Client;

/// <summary>Local presentation settings, archived separately from simulation saves.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record ClientViewPreferences
{
    public double BloodFadeScalar { get; init; } = 1;
    public bool SmoothPain { get; init; }
    public bool PoisonPaletteFlash { get; init; }
    public float PainSmoothStrength { get; init; } = 0.6f;

    public string Write()
    {
        Validate();
        return JsonSerializer.Serialize(this);
    }

    public static ClientViewPreferences Read(string json)
    {
        var preferences = JsonSerializer.Deserialize<ClientViewPreferences>(json)
            ?? throw new JsonException("View preferences must be an object.");
        preferences.Validate();
        return preferences with { PainSmoothStrength = Math.Clamp(preferences.PainSmoothStrength, 0, 1) };
    }

    public static ClientViewPreferences LoadFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string json;
        try { json = File.ReadAllText(path); }
        catch (FileNotFoundException) { return new(); }
        return Read(json);
    }

    public void SaveFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var json = Write();
        var destination = Path.GetFullPath(path);
        var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, json);
            File.Move(temporary, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    internal void Validate()
    {
        if (!double.IsFinite(BloodFadeScalar) || BloodFadeScalar < 0)
            throw new ArgumentOutOfRangeException(nameof(BloodFadeScalar));
        if (!float.IsFinite(PainSmoothStrength))
            throw new ArgumentOutOfRangeException(nameof(PainSmoothStrength));
    }
}
