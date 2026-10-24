namespace HCDE.Gamedata.Tests;

public class VmActorInfoNativeExportRecognitionTests
{
    [Fact]
    public void DamageTypeZScriptExports_IncludesIgnoreArmor()
    {
        Assert.Contains("_DamageTypeDefinition.IgnoreArmor", VmActorInfoNativeExportRecognition.DamageTypeZScriptExports);
    }
}
