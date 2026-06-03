/*
** invasion_hud.zs
**
** Middle-top invasion status banner (red text).
** Draws a single persistent stats line while invasion mode is active:
**   "Wave X/Y  |  Monsters: N  |  Archviles: N"
** (the "/Y" and the Archviles segment are only shown when those counts apply).
*/

class InvasionHUD : EventHandler
{
	const BANNER_TOP_FRAC = 0.12; // ~12% from top of screen

	override void RenderOverlay(RenderEvent e)
	{
		if (gamestate != GS_LEVEL)
			return;

		if (InvasionGetState() <= 0)
			return;

		let fnt = BigFont;
		if (!fnt)
			return;

		int screenW = Screen.GetWidth();
		int screenH = Screen.GetHeight();

		int wave = InvasionGetWave();
		int maxWaves = InvasionGetMaxWaves();
		int monsters = InvasionGetActiveMonsterCount();
		int archviles = InvasionGetArchvileCount();

		// Build the single stats line. No Array<string> here: RenderOverlay runs
		// every rendered frame, so a per-frame dynamic allocation would create
		// avoidable GC churn for what is only ever one line of text.
		string stats;
		if (maxWaves > 0)
			stats = String.Format("Wave %d/%d  |  Monsters: %d", wave, maxWaves, monsters);
		else
			stats = String.Format("Wave %d  |  Monsters: %d", wave, monsters);

		if (archviles > 0)
			stats = stats .. String.Format("  |  Archviles: %d", archviles);

		// Center horizontally; BANNER_TOP_FRAC puts the baseline near the top.
		int fontHeight = fnt.GetHeight();
		int y = int(screenH * BANNER_TOP_FRAC) - fontHeight / 2;
		int x = (screenW - fnt.StringWidth(stats)) / 2;

		Screen.DrawText(fnt, Font.CR_RED, x, y, stats, DTA_VirtualWidth, screenW, DTA_VirtualHeight, screenH);
	}
}