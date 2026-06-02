# HCDE RCON Design

Roadmap board item: **#24** ("rcon utility").

## Scope

A narrow, authority-only command channel for an operator (or external
launcher) to drive a dedicated `hcdeserv` instance: kick clients, restart
the map, dump diagnostics, toggle invasion state, etc. RCON is *not* a
gameplay extension; it must not borrow the gameplay packet stream and it
must not influence the authoritative simulation except through the same
console commands a local operator would run.

## Current state (2026-05-31)

The local TCP listener, nonce/password handshake, diagnostic command surface,
and standalone client have landed. See `src/d_net_rcon.{cpp,h}` and
`tools/hcdercon/`:

- `sv_rcon_enable` (Bool, default `0`, `CVAR_ARCHIVE | CVAR_SERVERINFO`)
- `sv_rcon_password` (String, default empty, `CVAR_ARCHIVE | CVAR_SERVERINFO`)
- `sv_rcon_port` (Int, default `0`; listener disabled until non-zero)
- `rcon_status` console command for visibility
- `HCDERconShouldAcceptCommands()` predicate for listener gating
- `HCDERconStartListener()`, `HCDERconStopListener()`, and
  `HCDERconPollListener()` transport lifecycle
- `hcdercon --port <port> --password <password> [--host 127.0.0.1] <command>`

The listener binds only to loopback (`127.0.0.1`) and only when RCON is enabled,
a password is configured, a non-zero port is set, and the process is the local
HCDE service authority. Frames are length-prefixed with a 4096-byte ceiling.

The command dispatcher is intentionally narrow for now: `ping`, `status`, and
`rcon_status` are accepted after authentication. Wider admin commands still
belong behind an explicit allowlist.

## Planned phases

### Phase 1 -- Transport scaffold (landed)

- Dedicated TCP listener on a configurable port (`sv_rcon_port`, default
  off / 0 = disabled).
- Bind only on the authority. Hard-refuse on listen hosts so a player who
  flips the CVAR cannot accidentally expose RCON on their LAN box.
- Length-prefixed frames; reject anything larger than a small ceiling
  (e.g. 4 KiB) to keep the parser trivial.
- All transport state lives in `d_net_rcon.cpp`; the gameplay packet pump
  in `i_net.cpp` is untouched.

### Phase 2 -- Authentication (minimal version landed)

- Server emits a fresh nonce per connection.
- Client sends a nonce/password verifier as the auth response. The current
  implementation uses the same lightweight verifier on both server and
  `hcdercon`; HMAC/rate limiting remain hardening work.
- Plaintext passwords never travel on the wire; the CVAR is only used to
  derive the local HMAC key.
- A future hardening pass can swap the CVAR for a key-derivation source
  (e.g. libsodium argon2 verifier loaded from a separate file) without
  changing the wire protocol.

### Phase 3 -- Command dispatch (diagnostics only today)

- Strict allowlist of commands. First-class candidates:
  - `say`, `kick`, `kickban`, `addipban`, `removeipban`
  - `map`, `restart`, `changemap`
  - `net_profile`, `net_actorstats`, `net_rewind_dump`, `net_lagcomp_summary`
  - `set sv_*` (with a denylist for known-dangerous keys)
- Hard-deny everything else, including `exec`, `playdemo`, `timedemo`,
  filesystem-touching CCMDs, and any UNSAFE_CCMD entry. The dispatcher
  whitelists by name; new commands are denied by default.
- All RCON-issued commands are tagged in the diag log
  (`DebugTrace::Markf("rcon", ...)` with the source address, command, and
  result) so abuse is auditable.

### Phase 4 -- Tooling

- `hcdercon` lives in `tools/hcdercon/` and is included in Windows runtime
  packages.
- `gh` / CI integration for automated soak runs that need to flip server
  state mid-test.

## Non-goals

- **No game packet co-mingling.** RCON does not ride on `NCMD_*` /
  `HCDE-native` lanes; it has its own listener, framing, and parser.
- **No replacement for the authority's local console.** Listen hosts are
  expected to use the in-game console, not RCON.
- **No "send a chat message via RCON" shortcut that bypasses player
  policy.** Operator messages go through `say` like any other command and
  obey existing chat policy.
- **No file transfer / mod download.** RCON is text-only.

## Threat model

- **Off-host attacker on the open internet.** Mitigated by mandatory
  password (no anonymous mode), rate-limited auth, and HMAC-only on the
  wire.
- **On-host attacker with read access to the config file.** Treated as
  game over: the password is stored as a CVAR in the same config that
  contains every other server secret. Users who need stronger isolation
  should keep the config out of shared paths and rely on file ACLs.
- **Replay attacks.** Mitigated by the per-connection nonce.
- **Command-injection through args.** The dispatcher tokenises with the
  same rules the local console uses; the allowlist prevents commands
  that interpret arbitrary text as a script.

## Source map

| Concern | File |
| --- | --- |
| CVAR + status today | `src/d_net_rcon.{cpp,h}` |
| Transport | `src/d_net_rcon.cpp` |
| Future dispatch / allowlist | TBD; new file `src/d_net_rcon_dispatch.{cpp,h}` |
| Tooling | `tools/hcdercon/` |
