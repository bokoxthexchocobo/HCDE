# Chunk 133 — `gameconfigfile.cpp` `DoGlobalSetup` version migrations

`VmGameConfigVersionMigrations` ports dictionary-level equivalents of native `FindCVar` migration gates (`last` &lt; 213–225, 221, 231): `snd_channels`, scale/HRTF/hqresize splits, pause flip, sensitivity halving, `gl_lightmode`, mouse sensitivity merge, ADL/OPN defaults, hqresize target bits, `language=auto`.

**Not credited:** `FindCVar`/`ResetToDefault` wiring, `vid_scalemode`/`vid_preferbackend` block (`last` &lt; 217–219), bind-reset flags (`bResetBindFlags`), `last` &lt; 207–209.

Extends chunk 128; chunk 127 `ReadCVars` remains dictionary-only (not `FStringCVar` creation).

Tests: `VmGameConfigVersionMigrationsTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
