# DoomCenter HCDE Compatibility

HCDE-owned compatibility shim for DoomCenter (`doomcenter_v95.pk3` and later).
Built into `hcde_mod_compat_doomcenter.pk3` and auto-loaded by HCDE when a
DoomCenter archive is detected. Layered on top of DoomCenter; the third-party
DoomCenter archive itself is never modified.

## What it fixes

DoomCenter is a Skulltag/Zandronum-era hub mod. On HCDE its DECORATE fails to
parse for two reasons:

1. **Skulltag base monsters** (`Abaddon`, `Belphegor`, `BloodDemon`,
   `Cacolantern`, `DarkImp`, `Hectebus`, `SuperShotgunGuy`) that DoomCenter's
   holographic display actors inherit from. These live in
   `skulltag_content-4.0.pk3`, so HCDE auto-loads that file **before**
   DoomCenter when present. It is third-party content and is not shipped here;
   the user supplies it (the same file HCDE already uses for Armageddon2).

2. **`FloatyIcon`** - a Skulltag/Zandronum *engine* built-in actor that exists
   in neither `skulltag_content` nor `skulltag_actors`. DoomCenter does
   `ACTOR DCFloatyIcon : FloatyIcon replaces FloatyIcon`, so the missing parent
   aborts the parse. This PK3 provides a minimal HCDE-authored `FloatyIcon`
   stub (see `DECORATE`).

DECORATE resolves parent classes by load order at parse time, so both the
Skulltag content and this stub must load before DoomCenter. The HCDE mod-compat
loader inserts them ahead of the matched DoomCenter archive (preload).

The hub map is `MAP55`; the compat entry also registers a startup-map override
so launchers that pass `MAP01` boot into the DoomCenter hub.

## Known non-fatal noise (not errors)

`NODELAY may only be used immediately after Spawn:` warnings from DoomCenter's
`mining.dec` are printed by the stricter GZDoom-derived parser but are not
fatal (they do not increment the parse error counter). DoomCenter loads and
runs with them present.
