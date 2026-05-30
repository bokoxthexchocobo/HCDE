class HCDEID24Validation : StaticEventHandler
{
    override void WorldLoaded(WorldEvent e)
    {
        CVar myCVar = CVar.FindCVar("hcde_id24_loaded");
        if (myCVar) {
            myCVar.SetInt(1);
        }
    }
}