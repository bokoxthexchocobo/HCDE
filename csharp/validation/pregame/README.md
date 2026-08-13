# Pregame validation

Cross-language pregame guest smoke tests for the C# `HCDE.Net.Pregame` guest pump against the shipping C++ `hcdeserv`.

## C# guest CLI

```bash
dotnet run --project csharp/src/HCDE.PregameGuest.Cli -- \
  --server 127.0.0.1:5029 \
  --engine-version 1.0.0 \
  --wad-crc <iwad-crc>
```

## Python harness

Requires a built `hcdeserv` and IWAD on disk. Skips gracefully when binaries are missing.

```bash
python3 csharp/validation/pregame/pregame_guest_smoke.py \
  --server build/hcdeserv \
  --iwad /path/to/doom2.wad \
  --wad-crc <iwad-crc>
```

## Environment variables (optional xUnit gate)

| Variable | Purpose |
| --- | --- |
| `HCDE_HCDESERV_PATH` | Path to `hcdeserv` for future automated cross-language tests |
| `HCDE_IWAD_PATH` | Path to IWAD for cross-language tests |
