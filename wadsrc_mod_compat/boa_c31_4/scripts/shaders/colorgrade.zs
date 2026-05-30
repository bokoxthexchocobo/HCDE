/*
 * HCDE Blade of Agony color-grading compatibility shim.
 *
 * BOA's `boashaders.txt` declares the `ColorGrade` post-process shader with
 * the `Enabled` keyword, so the engine starts running it as soon as gldefs
 * parses, before any player exists or its uniforms are initialized. Under
 * HCDE that produces a black 3D scene with the 2D menu still drawing on top
 * (because post-process only affects the scene buffer) -- the symptom the
 * user reported as "the screen doesn't load anything other than ESC menu".
 *
 * BOA's upstream colorgrade.zs then re-asserts `Shader.SetEnabled(... true)`
 * on PlayerEntered and on every Tick when the `boa_colorgrading` server CVar
 * is true (the default), so just toggling the CVar off is not enough -- the
 * thinker would just turn it back on the next tic.
 *
 * This override keeps every public surface that BOA's `shadercontrol.zs`
 * still references (the `ColorGradeState` struct shape, the
 * `ColorGradeThinker.playerStates` array, the static `Get()`/`Set()`/
 * `TransitionTo()` helpers, and the `PlayerSet` / `PlayerTransitionTo`
 * instance methods). It just stops the thinker and the event handler from
 * ever flipping the post-process shader to enabled, and it actively disables
 * the shader at PlayerEntered to undo the gldefs-time `Enabled` flag and to
 * undo any earlier zscript that might have re-enabled it.
 *
 * If we later add real post-process LUT support that doesn't blank the scene
 * (or fix the engine's gldefs `Enabled` semantics), this whole file should
 * be deleted from the compat PK3 so BOA's upstream colorgrade.zs takes over.
 */

struct ColorGradeState
{
	int currentLut;
	double currentSpeed;
	int nextLut;
	double nextSpeed;
	double alpha;
}

class ColorGradeEventHandler : EventHandler
{
	override void PlayerEntered(PlayerEvent e)
	{
		// Force-off, regardless of CVar state or what the gldefs `Enabled`
		// flag set at parse time. This call iterates every PostProcessShader
		// with the matching name and clears its Enabled flag, so the renderer
		// stops running ColorGrade from this frame onward.
		Shader.SetEnabled(players[e.PlayerNumber], "ColorGrade", false);
	}
}

class ColorGradeThinker : Thinker
{
	const DEFAULT_SPEED_TICKS = 70;

	ColorGradeState playerStates[MAXPLAYERS];
	bool wasEnabled;

	ColorGradeThinker Init()
	{
		self.wasEnabled = false;
		return self;
	}

	static ColorGradeThinker Get()
	{
		ThinkerIterator it = ThinkerIterator.Create("ColorGradeThinker");
		let p = ColorGradeThinker(it.Next());
		if (p == null)
		{
			p = new("ColorGradeThinker").Init();
		}
		return p;
	}

	override void Tick()
	{
		// Defensive: in case some other code path re-enabled the shader
		// (BOA's powerups.zs flips ColorGradeShaderControl on/off based on
		// pickups), keep stamping it disabled until the underlying renderer
		// fix lands. This is cheap -- the inner loop in ShaderSetEnabled is
		// O(PostProcessShaders.Size()) and only runs on this thinker tick.
		for (int i = 0; i < MAXPLAYERS; i++)
		{
			if (!playeringame[i]) { continue; }
			Shader.SetEnabled(players[i], "ColorGrade", false);
		}
	}

	void InitPlayer(int playerNumber)
	{
		// Same as PlayerSet(playerNumber, 0), but we *don't* call
		// Shader.SetEnabled(true) the way BOA's upstream does.
		PlayerSet(playerNumber, 0);
	}

	void PlayerTransitionTo(int playerNumber, int newLut, int speed)
	{
		if (speed == 0) { speed = DEFAULT_SPEED_TICKS; }
		playerStates[playerNumber].nextLut = newLut;
		playerStates[playerNumber].nextSpeed = 1.0 / double(speed);
	}

	void PlayerSet(int playerNumber, int lut)
	{
		// Keep the bookkeeping that ColorGradeShaderControl reads back
		// from on save/load and pickup transitions, but skip the
		// Shader.SetUniform* calls -- those are cheap, but pointless while
		// the shader is forced off, and skipping them keeps the GL state
		// completely untouched by us.
		playerStates[playerNumber].currentLut = lut;
		playerStates[playerNumber].currentSpeed = 0.0;
		playerStates[playerNumber].nextLut = lut;
		playerStates[playerNumber].nextSpeed = 0.0;
		playerStates[playerNumber].alpha = 1.0;
	}

	static void TransitionTo(int playerNumber, int lutNew, int speed)
	{
		let thinker = ColorGradeThinker.Get();
		thinker.PlayerTransitionTo(playerNumber, lutNew, speed);
	}

	static void Set(int playerNumber, int lut)
	{
		let thinker = ColorGradeThinker.Get();
		thinker.PlayerSet(playerNumber, lut);
	}
}
