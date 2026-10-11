using System.Text.Json;
using HCDE.Playsim;

namespace HCDE.Client.Tests;

public class ClientViewPreferencesTests
{
    [Fact]
    public void OlderPreferenceArchiveKeepsContinuousPoisonDefault()
    {
        var settings = ClientViewPreferences.Read("{\"BloodFadeScalar\":0.5,\"SmoothPain\":true,\"PainSmoothStrength\":0.8}");
        Assert.False(settings.PoisonPaletteFlash); Assert.Equal(0.5, settings.BloodFadeScalar);
        Assert.True(settings.SmoothPain); Assert.Equal(0.8f, settings.PainSmoothStrength);
    }
    [Fact]
    public void ArchiveRoundTripRestoresSettingsWithoutChangingGameplaySave()
    {
        using var host = ClientHost.DemoRoom();
        host.BloodFadeScalar = 0.25; host.SmoothPain = true; host.PainSmoothStrength = 0.8f;
        var archive = host.CaptureViewPreferences().Write();
        var saved = SimSavegame.Write(host.Simulation);
        host.BloodFadeScalar = 1; host.SmoothPain = false; host.PainSmoothStrength = 0;
        host.ApplyViewPreferences(ClientViewPreferences.Read(archive));
        Assert.Equal(0.25, host.BloodFadeScalar); Assert.True(host.SmoothPain);
        Assert.Equal(0.8f, host.PainSmoothStrength);
        Assert.Equal(archive, host.CaptureViewPreferences().Write());
        Assert.Equal(saved, SimSavegame.Write(host.Simulation));
    }

    [Fact]
    public void MissingSettingsUseNativeDefaults()
    {
        Assert.Equal(new ClientViewPreferences(), ClientViewPreferences.Read("{}"));
    }

    [Fact]
    public void GameplayLoadRestoresReceivedTypeWithoutReplacingViewPreferences()
    {
        using var host = ClientHost.DemoRoom();
        var player = host.Simulation.Players.Single(); player.NoPain = true;
        ActorDamage.Apply(player, 10, damageType: "Fire");
        var saved = SimSavegame.Write(host.Simulation);
        ActorDamage.Apply(player, 5, damageType: "Ice");
        host.ApplyViewPreferences(new() { SmoothPain = true, BloodFadeScalar = 0.5 });
        var preferences = host.CaptureViewPreferences();
        SimSavegame.Apply(host.Simulation, saved);
        Assert.Equal("Fire", player.LastDamageType); Assert.Equal(10, player.DamageCount);
        Assert.Equal(preferences, host.CaptureViewPreferences());
    }

    [Theory]
    [InlineData(-2f, 0f)]
    [InlineData(2f, 1f)]
    public void ImportedStrengthClampsLikeNativeSetting(float input, float expected)
    {
        var json = new ClientViewPreferences { PainSmoothStrength = input }.Write();
        Assert.Equal(expected, ClientViewPreferences.Read(json).PainSmoothStrength);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"SmoothPain\":1}")]
    [InlineData("{\"Unknown\":true}")]
    public void InvalidArchiveIsRejected(string json)
    {
        Assert.Throws<JsonException>(() => ClientViewPreferences.Read(json));
    }

    [Fact]
    public void InvalidSettingsDoNotPartiallyChangeHost()
    {
        using var host = ClientHost.DemoRoom();
        var before = host.CaptureViewPreferences();
        Assert.Throws<ArgumentOutOfRangeException>(() => host.ApplyViewPreferences(new()
        { BloodFadeScalar = 0.5, SmoothPain = true, PainSmoothStrength = float.NaN }));
        Assert.Equal(before, host.CaptureViewPreferences());
        Assert.Throws<ArgumentOutOfRangeException>(() => ClientViewPreferences.Read("{\"BloodFadeScalar\":-1}"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClientViewPreferences { BloodFadeScalar = double.PositiveInfinity }.Write());
    }
}
