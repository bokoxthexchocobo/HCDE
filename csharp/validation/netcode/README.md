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

When a local C++ `hcdeserv` build and IWAD are available, run the existing
pregame cross-language harness:

```bash
python3 csharp/validation/pregame/pregame_guest_smoke.py \
  --server /path/to/hcdeserv \
  --iwad /path/to/DOOM2.WAD
```

Full native live gameplay stress remains under `tests/netcode_step12/`. The managed
gate also includes `NetcodeCrossLanguageTests`, which skips unless
`HCDE_HCDESERV_PATH` and `HCDE_IWAD_PATH` are set.
