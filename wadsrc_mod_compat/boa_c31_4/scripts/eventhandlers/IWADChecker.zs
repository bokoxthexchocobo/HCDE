/*
 * HCDE Blade of Agony startup notice compatibility.
 *
 * BOA's upstream IWADChecker intentionally opens a black full-screen notice
 * and freezes the level when BOA is loaded as a PWAD over DOOM2.WAD. HCDE
 * launchers commonly keep DOOM2.WAD as the selected IWAD and pass BOA through
 * -file, so that guard looks like a permanent black screen. The real script
 * parse fixes for HCDE already live in this compat PK3; this override keeps
 * the startup checker inert so BOA can continue into its menu/title flow.
 */
class IWADChecker : EventHandler
{
	void CheckIWAD()
	{
	}

	void CheckRenderer()
	{
	}

	void DisplayDisclaimer()
	{
	}

	override void WorldTick()
	{
		CVar firstrun = CVar.FindCVar("boa_firstrun");
		if (level.time == 5)
		{
			if (firstrun)
			{
				firstrun.SetBool(false);
			}
			Destroy();
		}
	}
}
