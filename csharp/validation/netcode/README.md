# C# Netcode Wire Validation

Lightweight validation harness for the Phase 2c C# wire codecs. This does not
replace `tests/netcode_step12/` (which exercises full `hcdeserv` + `hcde`
processes); it provides a fast, CI-friendly gate on the managed codec layer.

## Quick check

```bash
cd csharp
dotnet test
```

## What is covered here

- HLIV/HGPL live envelope round-trips
- HCIN/HCSN headers and HCIR/HCSR bodies
- Snapshot tail walker (HCDW, HCDA, HCDS, HCAV, HCIV, ECHO, HCKS)
- Authority live session loopback (single and multi-client pump)

## Cross-language soak (optional)

When a local C++ `hcdeserv` build and IWAD are available:

**Pregame** — `PregameCrossLanguageSoak.RunPregameGuestSmoke()` or:

```bash
python3 csharp/validation/pregame/pregame_guest_smoke.py \
  --server /path/to/hcdeserv \
  --iwad /path/to/DOOM2.WAD
```

**Netcode (Step 12)** — `NetcodeCrossLanguageSoak.RunStep12InvasionSmoke()` or:

```bash
python3 tests/netcode_step12/netcode_step12_stress.py \
  --server /path/to/hcdeserv \
  --iwad /path/to/DOOM2.WAD \
  --cases invasion \
  --duration 20 \
  --wave-pulses 2
```

Set `HCDE_HCDE_CLIENT_PATH` to add one joining client during the soak.

Full native live gameplay stress remains under `tests/netcode_step12/`. The managed
gate includes `NetcodeCrossLanguageTests`, `NetcodeCrossLanguageSoakTests`, and
`PregameCrossLanguageSoakTests`, which skip unless `HCDE_HCDESERV_PATH` and
`HCDE_IWAD_PATH` are set.
