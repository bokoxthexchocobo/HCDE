namespace HCDE.Playsim;

/// <summary>Native <c>DLevelScript::DoSetMaster</c> for ACS master assignment.</summary>
public static class AcsSetMasterService
{
    public static int TrySet(AuthoritySimulation sim, Actor self, int masterTid)
    {
        if (self.Destroyed || !self.IsMonster)
            return 0;
        var master = masterTid == 0 ? null : AcsActorTid.SingleFromTid(sim, masterTid);
        return AcsActorProperties.SetMaster(self, master);
    }
}
