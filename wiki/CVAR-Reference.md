# HCDE CVAR Reference

Generated: 2026-06-05 20:15:03 UTC

This reference is generated from source-defined CVAR macros and the category taxonomy in `tools/cvar-categories.json`.

## Coverage

- Source CVAR definitions discovered: **1073** unique / **1080** total macro definitions

## Category Index

CVARs are grouped using prefix rules and explicit overrides in [`tools/cvar-categories.json`](../tools/cvar-categories.json).

| Category | CVARs | Description |
| --- | ---: | --- |
| [HCDE Invasion & Server](#category-hcde-invasion) | 25 | Wave timing, budgets, spawn rules, and Skulltag compatibility overrides for invasion game modes. |
| [HCDE Netcode & Diagnostics](#category-hcde-netcode) | 20 | Prediction, replication, lag HUD, checksums, blackbox, and HCDE-native networking controls. |
| [HCDE Rendering](#category-hcde-rendering) | 11 | Shadow auto-budget, NanoBSP loader, and other HCDE-specific rendering extensions. |
| [Server & Multiplayer](#category-server) | 111 | Server rules, dmflags, corpse cleanup, and general multiplayer session settings. |
| [Client](#category-client) | 49 | Client-side behavior, prediction, and local session preferences. |
| [Audio](#category-audio) | 26 | Sound backend selection, volume, spatial audio, and environmental reverb. |
| [Music](#category-music) | 3 | MIDI, module, and streaming music playback configuration. |
| [Renderer](#category-rendering) | 169 | Hardware and software renderer options, lighting, sprites, and draw quality. |
| [Post-Processing](#category-postprocess) | 0 | Bloom, tonemap, SSAO, and other screen-space effects. |
| [Video & Display](#category-video) | 38 | Resolution, scaling, fullscreen, vsync, and framebuffer settings. |
| [Gameplay](#category-gameplay) | 62 | Movement, weapons, monsters, compatibility, and general play simulation. |
| [HUD & Status Bar](#category-hud) | 50 | Heads-up display, crosshair, messages, and status bar layout. |
| [Automap](#category-automap) | 83 | Automap colors, rotation, overlay, and navigation aids. |
| [Input](#category-input) | 12 | Keyboard, mouse, gamepad, and gyro controls. |
| [Menu & UI](#category-menu) | 26 | Menu appearance, save-game UI, and interface preferences. |
| [Debug & Development](#category-debug) | 15 | Developer diagnostics, tracing, profiling, and cheat toggles. |
| [Other](#category-misc) | 373 | CVARs that do not match a more specific category rule. |

See also the compact category catalog: [`wiki/CVAR-Categories.md`](CVAR-Categories.md).

## Flag Legend

- Position 1: `A` = archived, space = not archived
- Position 2: `U` = userinfo, `S` = serverinfo, `C` = auto/custom, space = local/general
- Position 3: `-` = write-protected, `L` = latched, `*` = unsettable auto cvar, space = writable
- Position 4: `M` = modified/session-marked
- Position 5: `X` = ignored/hidden from normal flow

## Source Catalog by Category

Compact index of source-defined CVARs grouped by category.

### Category: HCDE Invasion & Server {#category-hcde-invasion}

Wave timing, budgets, spawn rules, and Skulltag compatibility overrides for invasion game modes.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `duellimit` | Int | 0 | Legacy Skulltag compatibility value for duel limit metadata. | `/workspace/src/d_net_invasion.inl:113` |
| `sv_invasionbasebudget` | Int | 24 | Base monster budget each wave starts with. | `/workspace/src/d_net_invasion.inl:123` |
| `sv_invasionbossbonus` | Int | 20 | Extra budget added during boss waves. | `/workspace/src/d_net_invasion.inl:158` |
| `sv_invasionbosswaveevery` | Int | 5 | Boss wave cadence (e.g. 5 = every 5th wave, 0 = never). | `/workspace/src/d_net_invasion.inl:153` |
| `sv_invasionbudgetstep` | Int | 8 | Budget increase applied per wave number. | `/workspace/src/d_net_invasion.inl:128` |
| `sv_invasioncleanuptime` | Float | 4.0f | Seconds allowed for cleanup phase after spawning ends. | `/workspace/src/d_net_invasion.inl:81` |
| `sv_invasioncountdowntime` | Float | 30.0f | Seconds before wave 1 starts ("Prepare for invasion" countdown). | `/workspace/src/d_net_invasion.inl:68` |
| `sv_invasiondebug` | Int | 0 | Server setting: Invasion Debug | `/workspace/src/d_net.cpp:189` |
| `sv_invasionexitonvictory` | Bool | true | Server setting: Invasion Exit Victory | `/workspace/src/d_net_invasion.inl:96` |
| `sv_invasionintermissiontime` | Float | 6.0f | Seconds between completed waves before the next wave starts. | `/workspace/src/d_net_invasion.inl:86` |
| `sv_invasionmaxactive` | Int | 0 | Optional cap for active invasion monsters. 0 disables the cap; positive values are clamped by the engine. | `/workspace/src/d_net_invasion.inl:148` |
| `sv_invasionperplayer` | Int | 6 | Additional budget per extra active player. | `/workspace/src/d_net_invasion.inl:133` |
| `sv_invasionresulttime` | Float | 8.0f | Seconds to keep the final victory/failure state visible. | `/workspace/src/d_net_invasion.inl:91` |
| `sv_invasionsimlod` | Bool | true | Enables server-side simulation LOD for invasion monsters so distant actors think less often under heavy load. | `/workspace/src/d_net_invasion.inl:169` |
| `sv_invasionsimloddormantinterval` | Int | TICRATE * 3 | Think interval in tics for dormant distant invasion simulation. | `/workspace/src/d_net_invasion.inl:190` |
| `sv_invasionsimlodfullrange` | Float | 2048.0f | Distance within which invasion monsters keep full-rate simulation. | `/workspace/src/d_net_invasion.inl:172` |
| `sv_invasionsimlodreducedinterval` | Int | 5 | Think interval in tics for reduced-rate invasion simulation. | `/workspace/src/d_net_invasion.inl:184` |
| `sv_invasionsimlodreducedrange` | Float | 4096.0f | Distance within which invasion monsters use reduced-rate simulation before becoming dormant. | `/workspace/src/d_net_invasion.inl:178` |
| `sv_invasionspawnburst` | Int | 3 | Maximum monsters spawned per spawn tick burst. | `/workspace/src/d_net_invasion.inl:143` |
| `sv_invasionspawninterval` | Float | 0.35f | Seconds between spawn ticks while wave spawning is active. | `/workspace/src/d_net_invasion.inl:138` |
| `sv_invasionspawntime` | Float | 8.0f | Wave spawn window length in seconds before cleanup phase. | `/workspace/src/d_net_invasion.inl:76` |
| `sv_invasionspotfallback` | Bool | true | Fallback to generic spawning when tagged invasion spots cannot be used. | `/workspace/src/d_net_invasion.inl:166` |
| `sv_invasionspotusemaptags` | Bool | false | Restrict native invasion spots by map thing TID/tag. Keep disabled for Skulltag/Zandronum map compatibility; the spot arguments already control wave timing. | `/workspace/src/d_net_invasion.inl:163` |
| `sv_invasionwaves` | Int | 8 | Maximum number of invasion waves in a run. | `/workspace/src/d_net_invasion.inl:99` |
| `wavelimit` | Int | 0 | Legacy Skulltag compatibility override for invasion waves. 0 disables the override; 1..255 forces that wave count. | `/workspace/src/d_net_invasion.inl:106` |

### Category: HCDE Netcode & Diagnostics {#category-hcde-netcode}

Prediction, replication, lag HUD, checksums, blackbox, and HCDE-native networking controls.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `cl_hcde_predict_dedicated` | Bool | true | Enable client-side movement prediction when connected to a dedicated HCDE server. | `/workspace/src/playsim/p_user.cpp:92` |
| `hcde_hud_debug` | Bool | true | Mirror net diagnostics to the HUD console for live operator visibility. | `/workspace/src/d_net.cpp:185` |
| `hcde_lag_hud` | Bool | false | Persistent on-screen lag/invasion overlay (top-left). Also enable with `stat hcde_lag`. | `/workspace/src/d_net.cpp:197` |
| `hcde_startup_profile` | Bool | false | Emit startup timing profile data for engine initialization diagnostics. | `/workspace/src/scripting/thingdef.cpp:54` |
| `net_blackbox_record` | Int | 1 | Likely controls blackbox record behavior for network. | `/workspace/src/d_net_blackbox.cpp:35` |
| `net_blackbox_size_mb` | Int | 32 | Likely controls blackbox size mb behavior for network. | `/workspace/src/d_net_blackbox.cpp:45` |
| `net_checksum` | Int | 1 | Likely controls checksum behavior for network. | `/workspace/src/d_net_checksum.cpp:34` |
| `net_checksum_categories` | Int | 0x3F | Likely controls checksum categories behavior for network. | `/workspace/src/d_net_checksum.cpp:53` |
| `net_checksum_interval` | Int | 4 | Likely controls checksum interval behavior for network. | `/workspace/src/d_net_checksum.cpp:45` |
| `net_hcde_native_only` | Bool | true | Requires HCDE-native networking/capability paths for multiplayer sessions. | `/workspace/src/d_net.cpp:312` |
| `net_movement_debug` | Int | 0 | Likely controls movement debug behavior for network. | `/workspace/src/d_net_movement_diag.cpp:33` |
| `net_predict_debug` | Int | 0 | Controls HCDE prediction diagnostics: off, CSV sampling, and/or on-screen/debug trace output depending on level. | `/workspace/src/d_net.cpp:211` |
| `net_predict_debug_interval` | Int | 15 | Tic interval used by prediction CSV/debug sampling. | `/workspace/src/d_net.cpp:219` |
| `net_predict_softwarn_ack_lag` | Int | 3 | Soft warning threshold for client ack lag during prediction diagnostics. | `/workspace/src/d_net.cpp:268` |
| `net_predict_softwarn_mirror_delta` | Int | 2 | Soft warning threshold for invasion mirror drift during prediction diagnostics. | `/workspace/src/d_net.cpp:295` |
| `net_predict_softwarn_passive_storm` | Int | 5 | Soft warning threshold for passive update storms during prediction diagnostics. | `/workspace/src/d_net.cpp:303` |
| `net_rewind_depth` | Int | 10 | Likely controls rewind depth behavior for network. | `/workspace/src/d_net_rewind.cpp:48` |
| `net_rewind_enable` | Bool | false | Likely controls rewind enable behavior for network. | `/workspace/src/d_net_rewind.cpp:61` |
| `net_rewind_interval` | Float | 1.0f | Likely controls rewind interval behavior for network. | `/workspace/src/d_net_rewind.cpp:40` |
| `net_rewind_max_mb` | Int | 32 | Likely controls rewind max mb behavior for network. | `/workspace/src/d_net_rewind.cpp:69` |

### Category: HCDE Rendering {#category-hcde-rendering}

Shadow auto-budget, NanoBSP loader, and other HCDE-specific rendering extensions.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `hcde_k8vavoom_lighting_profile` | Int | 0 | Selects a composed K8vavoom lighting preset (0=off, 1+=profile id) and applies bundled renderer toggles. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:214` |
| `hcde_k8vavoom_raylight_probe` | Bool | false | Enable ray-light probing hooks used by K8vavoom-style lighting profile diagnostics. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:212` |
| `hcde_k8vavoom_shadow_boost` | Bool | false | Apply stronger shadow-map defaults when a K8vavoom lighting profile is active. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:211` |
| `hcde_nanobsp_loader` | Int | 0 | Selects NanoBSP loader mode for map geometry ingestion (0=off, 1=on, 2=force). | `/workspace/src/d_nanobsp_loader.cpp:51` |
| `hcde_shadow_autobudget` | Bool | false | Adaptively reduce shadow-casting light count to stay near the target shadow-map frame budget. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:70` |
| `hcde_shadow_autobudget_minlights` | Int | 64 | Minimum number of shadow-casting lights retained while auto-budget throttles the light count. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:280` |
| `hcde_shadow_autobudget_step` | Int | 32 | Number of shadow-casting lights removed or restored per auto-budget adjustment step. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:288` |
| `hcde_shadow_autobudget_targetms` | Float | 1.20f | Target milliseconds per frame allocated to shadow-map rendering when auto-budget is enabled. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:272` |
| `hcde_shadow_autofallback` | Bool | true | Automatically disable shadow maps when the renderer reports unsupported or failing shadow-map paths. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:69` |
| `hcde_shadow_forcealllights` | Bool | true | Force eligible dynamic lights onto the shadow-map path even when not explicitly marked shadowmapped. | `/workspace/src/rendering/hwrenderer/hw_entrypoint.cpp:59` |
| `hcde_shadowprofile` | Int | HCDE_SHADOWPROFILE_DOOM3 | applies HCDE grouped shadow settings. 0 = manual, 1 = off, 2 = performance, 3 = balanced, 4 = enhanced, 5 = cinematic, 6 = quake-style, 7 = doom3-style | `/workspace/src/menu/doommenu.cpp:802` |

### Category: Server & Multiplayer {#category-server}

Server rules, dmflags, corpse cleanup, and general multiplayer session settings.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `sv_aidirector_enable` | Bool | false | Likely controls aidirector enable behavior for server. | `/workspace/src/d_net_aidirector.cpp:66` |
| `sv_aidirector_regroup_hint` | Bool | false | Likely controls aidirector regroup hint behavior for server. | `/workspace/src/d_net_aidirector.cpp:77` |
| `sv_aidirector_sweep_tics` | Int | 7 | Likely controls aidirector sweep tics behavior for server. | `/workspace/src/d_net_aidirector.cpp:71` |
| `sv_aircontrol` | Float | 0.00390625f | Server setting: Air Control | `/workspace/src/playsim/p_user.cpp:1444` |
| `sv_allowallscripts` | Bool | false | Likely controls allowallscripts behavior for server. | `/workspace/src/playsim/p_acs.cpp:10958` |
| `sv_allowcrouch` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:708` |
| `sv_allowfreelook` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:704` |
| `sv_allowjump` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:702` |
| `sv_alwaysspawnmulti` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:793` |
| `sv_alwaystally` | Int | 0 | Server setting: Tally Policy | `/workspace/src/g_level.cpp:173` |
| `sv_ammofactor` | Float | 1.0 | Likely controls ammofactor behavior for server. | `/workspace/src/playsim/p_interaction.cpp:76` |
| `sv_autocompat` | Bool | true | Likely controls autocompat behavior for server. | `/workspace/src/playsim/p_map.cpp:71` |
| `sv_barrelrespawn` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:774` |
| `sv_chasecam` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:785` |
| `sv_cheats` | Bool | false | Likely controls cheats behavior for server. | `/workspace/src/console/c_cmds.cpp:58` |
| `sv_coophalveammo` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:715` |
| `sv_cooploseammo` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:714` |
| `sv_cooplosearmor` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:712` |
| `sv_cooploseinventory` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:709` |
| `sv_cooplosekeys` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:710` |
| `sv_cooplosepowerups` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:713` |
| `sv_cooploseweapons` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:711` |
| `sv_coopsharekeys` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:808` |
| `sv_corpsefilter` | Int | 1 | Selects which corpse queues sv_corpsequeuesize trims: 0 off, 1 monsters, 2 players, 3 both. | `/workspace/src/g_cvars.cpp:176` |
| `sv_corpsequeuesize` | Int | 64 | Maximum queued corpses retained by corpse cleanup; used with sv_corpsefilter. | `/workspace/src/g_cvars.cpp:184` |
| `sv_crouch` | Mask | dmflags | Likely controls crouch behavior for server. | `/workspace/src/d_main.cpp:719` |
| `sv_damagefactorfriendly` | Float | 1.0 | Likely controls damagefactorfriendly behavior for server. | `/workspace/src/playsim/p_interaction.cpp:74` |
| `sv_damagefactormobj` | Float | 1.0 | Likely controls damagefactormobj behavior for server. | `/workspace/src/playsim/p_interaction.cpp:73` |
| `sv_damagefactorplayer` | Float | 1.0 | Likely controls damagefactorplayer behavior for server. | `/workspace/src/playsim/p_interaction.cpp:75` |
| `sv_dedicated_autostart` | Bool | true | Likely controls dedicated autostart behavior for server. | `/workspace/src/common/engine/i_net.cpp:88` |
| `sv_degeneration` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:772` |
| `sv_disableautohealth` | Bool | false | Likely controls disableautohealth behavior for server. | `/workspace/src/playsim/p_interaction.cpp:840` |
| `sv_disallowspying` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:784` |
| `sv_disallowsuicide` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:786` |
| `sv_dontcheckammo` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:788` |
| `sv_doubleammo` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:771` |
| `sv_dropstyle` | Int | 0 | Server setting: Drop Style | `/workspace/src/playsim/p_enemy.cpp:77` |
| `sv_falldamage` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:689` |
| `sv_fallingdamage` | Mask | dmflags | Likely controls fallingdamage behavior for server. | `/workspace/src/d_main.cpp:721` |
| `sv_fastmonsters` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:700` |
| `sv_fastweapons` | Int | 0 | Server setting: Fast Weapons | `/workspace/src/playsim/p_pspr.cpp:90` |
| `sv_forcerespawn` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:693` |
| `sv_freelook` | Mask | dmflags | Likely controls freelook behavior for server. | `/workspace/src/d_main.cpp:722` |
| `sv_gametype` | Int | 0 | Server setting: Game Type | `/workspace/src/g_game.cpp:337` |
| `sv_gravity` | Float | 800.f | Server setting: Gravity | `/workspace/src/playsim/p_mobj.cpp:122` |
| `sv_hostname` | String | GAMENAME " server" | Server setting: Hostname | `/workspace/src/common/engine/i_net.cpp:84` |
| `sv_infiniteammo` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:696` |
| `sv_infiniteinventory` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:780` |
| `sv_instantreaction` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:716` |
| `sv_itemrespawn` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:699` |
| `sv_jump` | Mask | dmflags | Likely controls jump behavior for server. | `/workspace/src/d_main.cpp:720` |
| `sv_keepfrags` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:775` |
| `sv_killallmonsters` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:781` |
| `sv_killbossmonst` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:789` |
| `sv_lagcomp` | Bool | false | Likely controls lagcomp behavior for server. | `/workspace/src/d_net_rewind.cpp:813` |
| `sv_lagcomp_max_age_tics` | Int | 12 | Likely controls lagcomp max age tics behavior for server. | `/workspace/src/d_net_rewind.cpp:817` |
| `sv_lateJoin` | Bool | true | Likely controls lateJoin behavior for server. | `/workspace/src/common/engine/i_net.cpp:109` |
| `sv_localitems` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:809` |
| `sv_losefrag` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:777` |
| `sv_maxplayers` | Int | 0 | Server setting: Max Players | `/workspace/src/common/engine/i_net.cpp:89` |
| `sv_monsterrespawn` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:698` |
| `sv_motd` | String | "Welcome to " GAMENAME | Server setting: MOTD | `/workspace/src/common/engine/i_net.cpp:85` |
| `sv_natport` | Int | 0 | Server setting: NAT Port | `/workspace/src/common/engine/sv_master.cpp:54` |
| `sv_noarmor` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:694` |
| `sv_noautoaim` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:787` |
| `sv_noautomap` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:782` |
| `sv_noautomapallies` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:783` |
| `sv_nobfgaim` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:773` |
| `sv_nocoopitems` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:811` |
| `sv_nocoopthings` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:812` |
| `sv_nocountendmonst` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:790` |
| `sv_nocrouch` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:707` |
| `sv_noexit` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:695` |
| `sv_noextraammo` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:795` |
| `sv_nofov` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:705` |
| `sv_nofreelook` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:703` |
| `sv_nohealth` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:686` |
| `sv_noitems` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:687` |
| `sv_nojump` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:701` |
| `sv_nolocaldrops` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:810` |
| `sv_nomonsters` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:697` |
| `sv_noplayerclip` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:807` |
| `sv_norespawn` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:776` |
| `sv_noteamswitch` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:770` |
| `sv_nothingspawn` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:792` |
| `sv_novertspread` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:794` |
| `sv_noweaponspawn` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:706` |
| `sv_oldfalldamage` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:690` |
| `sv_pistolstart` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:814` |
| `sv_predator_allow_cheats` | Bool | false | Likely controls predator allow cheats behavior for server. | `/workspace/src/d_net_predator.cpp:126` |
| `sv_predator_buy_seconds` | Int | 20 | Likely controls predator buy seconds behavior for server. | `/workspace/src/d_net_predator.cpp:132` |
| `sv_predator_enable` | Bool | false | Likely controls predator enable behavior for server. | `/workspace/src/d_net_predator.cpp:125` |
| `sv_predator_round_seconds` | Int | 180 | Likely controls predator round seconds behavior for server. | `/workspace/src/d_net_predator.cpp:127` |
| `sv_predator_starting_currency` | Int | 800 | Likely controls predator starting currency behavior for server. | `/workspace/src/d_net_predator.cpp:137` |
| `sv_rcon_enable` | Bool | false | Likely controls rcon enable behavior for server. | `/workspace/src/d_net_rcon.cpp:302` |
| `sv_rcon_password` | String | "" | Likely controls rcon password behavior for server. | `/workspace/src/d_net_rcon.cpp:308` |
| `sv_rcon_port` | Int | 0 | Likely controls rcon port behavior for server. | `/workspace/src/d_net_rcon.cpp:314` |
| `sv_rememberlastweapon` | Flag | dmflags3 | Flag alias backed by dmflags3. | `/workspace/src/d_main.cpp:813` |
| `sv_respawnprotect` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:778` |
| `sv_respawnsuper` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:791` |
| `sv_samelevel` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:691` |
| `sv_samespawnspot` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:779` |
| `sv_singleplayerrespawn` | Bool | false | Likely controls singleplayerrespawn behavior for server. | `/workspace/src/playsim/p_user.cpp:75` |
| `sv_smartaim` | Int | 0 | Server setting: Smart Aim | `/workspace/src/playsim/p_map.cpp:69` |
| `sv_spawnfarthest` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:692` |
| `sv_unlimited_pickup` | Bool | false | Likely controls unlimited pickup behavior for server. | `/workspace/src/console/c_cmds.cpp:59` |
| `sv_upnp` | Bool | false | Likely controls upnp behavior for server. | `/workspace/src/common/engine/sv_master.cpp:55` |
| `sv_usemapsettingswavelimit` | Bool | true | If enabled, map-defined invasion wavelimit metadata overrides sv_invasionwaves when present. | `/workspace/src/d_net_invasion.inl:120` |
| `sv_usemasters` | Bool | false | Likely controls usemasters behavior for server. | `/workspace/src/common/engine/sv_master.cpp:774` |
| `sv_weapondrop` | Flag | dmflags2 | Flag alias backed by dmflags2. | `/workspace/src/d_main.cpp:769` |
| `sv_weaponstay` | Flag | dmflags | Flag alias backed by dmflags. | `/workspace/src/d_main.cpp:688` |

### Category: Client {#category-client}

Client-side behavior, prediction, and local session preferences.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `cl_analog_run` | Bool | true | Likely controls analog run behavior for client. | `/workspace/src/g_game.cpp:333` |
| `cl_analog_sensitivity_pitch` | Float | 0.6f | Likely controls analog sensitivity pitch behavior for client. | `/workspace/src/g_game.cpp:332` |
| `cl_analog_sensitivity_yaw` | Float | 1.f | Likely controls analog sensitivity yaw behavior for client. | `/workspace/src/g_game.cpp:331` |
| `cl_analog_straferun` | Bool | false | Likely controls analog straferun behavior for client. | `/workspace/src/g_game.cpp:334` |
| `cl_bbannounce` | Bool | false | Likely controls bbannounce behavior for client. | `/workspace/src/bbannouncer.cpp:63` |
| `cl_blockcheats` | Int | 0 | Likely controls blockcheats behavior for client. | `/workspace/src/console/c_cmds.cpp:60` |
| `cl_bloodsplats` | Bool | true | Likely controls bloodsplats behavior for client. | `/workspace/src/playsim/p_map.cpp:68` |
| `cl_bloodtype` | Int | 0 | Likely controls bloodtype behavior for client. | `/workspace/src/playsim/p_mobj.cpp:137` |
| `cl_capfps` | Bool | false | Likely controls capfps behavior for client. | `/workspace/src/common/rendering/v_framebuffer.cpp:51` |
| `cl_custominvulmapcolor1` | Color | 0x00001a | Likely controls custominvulmapcolor1 behavior for client. | `/workspace/src/r_data/colormaps.cpp:41` |
| `cl_custominvulmapcolor2` | Color | 0xa6a67a | Likely controls custominvulmapcolor2 behavior for client. | `/workspace/src/r_data/colormaps.cpp:46` |
| `cl_customizeinvulmap` | Bool | false | Likely controls customizeinvulmap behavior for client. | `/workspace/src/r_data/colormaps.cpp:37` |
| `cl_debug_monster_proximity` | Int | 768 | Likely controls debug monster proximity behavior for client. | `/workspace/src/d_net.cpp:287` |
| `cl_debugprediction` | Int | 0 | Likely controls debugprediction behavior for client. | `/workspace/src/d_net_invasion.inl:207` |
| `cl_defaultconfiguration` | Int | 0 | Likely controls defaultconfiguration behavior for client. | `/workspace/src/common/console/c_bind.cpp:879` |
| `cl_doautoaim` | Bool | false | Likely controls doautoaim behavior for client. | `/workspace/src/playsim/p_map.cpp:70` |
| `cl_doubleclickthreshold` | Int | 250 | Likely controls doubleclickthreshold behavior for client. | `/workspace/src/common/console/c_bind.cpp:139` |
| `cl_gfxlocalization` | Int | 3 | Likely controls gfxlocalization behavior for client. | `/workspace/src/menu/doommenu.cpp:1614` |
| `cl_maxdecals` | Int | 1024 | Likely controls maxdecals behavior for client. | `/workspace/src/g_cvars.cpp:195` |
| `cl_missiledecals` | Bool | true | Likely controls missiledecals behavior for client. | `/workspace/src/playsim/p_mobj.cpp:134` |
| `cl_net_prediction_lead` | Int | 1 | Likely controls net prediction lead behavior for client. | `/workspace/src/d_net.cpp:260` |
| `cl_noboldchat` | Bool | false | Likely controls noboldchat behavior for client. | `/workspace/src/d_net_invasion.inl:197` |
| `cl_nochatsound` | Bool | false | Likely controls nochatsound behavior for client. | `/workspace/src/d_net_invasion.inl:198` |
| `cl_nointros` | Bool | false | Likely controls nointros behavior for client. | `/workspace/src/d_main.cpp:539` |
| `cl_noprediction` | Bool | false | Likely controls noprediction behavior for client. | `/workspace/src/playsim/p_user.cpp:89` |
| `cl_oldfreelooklimit` | Bool | false | Likely controls oldfreelooklimit behavior for client. | `/workspace/src/rendering/swrenderer/r_swrenderer.cpp:49` |
| `cl_predict_lerpscale` | Float | 0.05f | Likely controls predict lerpscale behavior for client. | `/workspace/src/playsim/p_user.cpp:93` |
| `cl_predict_lerpthreshold` | Float | 2.00f | Likely controls predict lerpthreshold behavior for client. | `/workspace/src/playsim/p_user.cpp:94` |
| `cl_predict_max` | Int | 24 | Likely controls predict max behavior for client. | `/workspace/src/playsim/p_user.cpp:128` |
| `cl_predict_specials` | Bool | true | Likely controls predict specials behavior for client. | `/workspace/src/playsim/p_user.cpp:87` |
| `cl_pufftype` | Int | 0 | Likely controls pufftype behavior for client. | `/workspace/src/playsim/p_mobj.cpp:136` |
| `cl_restartondeath` | Bool | false | Likely controls restartondeath behavior for client. | `/workspace/src/g_game.cpp:296` |
| `cl_rockettrails` | Int | 0 | Likely controls rockettrails behavior for client. | `/workspace/src/playsim/p_effect.cpp:51` |
| `cl_rubberband_limit` | Float | 756.0f | Likely controls rubberband limit behavior for client. | `/workspace/src/playsim/p_user.cpp:117` |
| `cl_rubberband_minmove` | Float | 20.0f | Likely controls rubberband minmove behavior for client. | `/workspace/src/playsim/p_user.cpp:112` |
| `cl_rubberband_scale` | Float | 0.3f | Likely controls rubberband scale behavior for client. | `/workspace/src/playsim/p_user.cpp:96` |
| `cl_rubberband_threshold` | Float | 32.0f | Likely controls rubberband threshold behavior for client. | `/workspace/src/playsim/p_user.cpp:107` |
| `cl_run` | Bool | false | Likely controls run behavior for client. | `/workspace/src/g_game.cpp:323` |
| `cl_scaleweaponfov` | Float | 1.0f | Likely controls scaleweaponfov behavior for client. | `/workspace/src/g_cvars.cpp:244` |
| `cl_showchat` | Int | CHAT_GLOBAL | Likely controls showchat behavior for client. | `/workspace/src/d_net_invasion.inl:199` |
| `cl_showmultikills` | Bool | true | Likely controls showmultikills behavior for client. | `/workspace/src/playsim/p_interaction.cpp:67` |
| `cl_showsecretmessage` | Bool | true | Likely controls showsecretmessage behavior for client. | `/workspace/src/playsim/p_spec.cpp:594` |
| `cl_showsprees` | Bool | true | Likely controls showsprees behavior for client. | `/workspace/src/playsim/p_interaction.cpp:66` |
| `cl_smooth_decay` | Float | 0.85f | Likely controls smooth decay behavior for client. | `/workspace/src/d_net.cpp:320` |
| `cl_smooth_maxdist` | Float | 32.0f | Likely controls smooth maxdist behavior for client. | `/workspace/src/d_net.cpp:329` |
| `cl_smooth_reconcile` | Bool | true | Likely controls smooth reconcile behavior for client. | `/workspace/src/d_net.cpp:317` |
| `cl_spreaddecals` | Bool | true | Likely controls spreaddecals behavior for client. | `/workspace/src/g_cvars.cpp:136` |
| `cl_stannounce` | Bool | false | Likely controls stannounce behavior for client. | `/workspace/src/bbannouncer.cpp:64` |
| `cl_waitforsave` | Bool | true | Likely controls waitforsave behavior for client. | `/workspace/src/g_game.cpp:294` |

### Category: Audio {#category-audio}

Sound backend selection, volume, spatial audio, and environmental reverb.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `snd_aldevice` | String | "Default" | Likely controls snd aldevice. | `/workspace/src/common/audio/sound/oalsound.cpp:59` |
| `snd_aldriver` | String | DEFAULT_DRIVER | See alsoftrc.sample for details | `/workspace/src/common/audio/sound/oalsound.cpp:57` |
| `snd_alresampler` | String | "Default" | Likely controls snd alresampler. | `/workspace/src/common/audio/sound/oalsound.cpp:69` |
| `snd_backend` | String | DEF_BACKEND | Audio backend selector: `openal` (default), `null` (silent), or `eternity` (spatial facade). | `/workspace/src/common/audio/sound/i_sound.cpp:64` |
| `snd_buffersize` | Int | 0 | Likely controls snd buffersize. | `/workspace/src/common/audio/sound/i_sound.cpp:48` |
| `snd_channels` | Int | 128 | Likely controls snd channels. | `/workspace/src/common/audio/sound/oalsound.cpp:53` |
| `snd_drawoutput` | Int | 0 | Likely controls snd drawoutput. | `/workspace/src/d_main.cpp:489` |
| `snd_efx` | Bool | true | Likely controls snd efx. | `/workspace/src/common/audio/sound/oalsound.cpp:60` |
| `snd_enabled` | Bool | true | enables/disables sound effects | `/workspace/src/common/audio/sound/s_sound.cpp:39` |
| `snd_env_reverb` | Bool | true | Likely controls snd env reverb. | `/workspace/src/common/audio/sound/oalsound.cpp:62` |
| `snd_environmentprofile` | Int | 1 | Global reverb profile. 0=classic, 1=doomsday room, 2=doomsday cave, 3=doomsday cinematic. | `/workspace/src/common/audio/sound/oalsound.cpp:63` |
| `snd_footsteps_surface` | Bool | false | Likely controls snd footsteps surface. | `/workspace/src/i_input_feel.cpp:35` |
| `snd_footstepvolume` | Float | 1.f | Likely controls snd footstepvolume. | `/workspace/src/playsim/p_user.cpp:76` |
| `snd_hrtf` | Int | -1 | Likely controls snd hrtf. | `/workspace/src/common/audio/sound/i_sound.cpp:49` |
| `snd_mastervolume` | Float | 0.5f | Likely controls snd mastervolume. | `/workspace/src/common/audio/sound/i_sound.cpp:85` |
| `snd_menuvolume` | Float | 0.6f | Likely controls snd menuvolume. | `/workspace/src/common/menu/menu.cpp:57` |
| `snd_mididevice` | Int | DEF_MIDIDEV | Likely controls snd mididevice. | `/workspace/src/common/audio/music/music_midi_base.cpp:92` |
| `snd_midiprecache` | Bool | false | Likely controls snd midiprecache. | `/workspace/src/common/audio/music/music_config.cpp:491` |
| `snd_musicmode` | Int | 0 | Likely controls snd musicmode. | `/workspace/src/common/audio/sound/oalsound.cpp:70` |
| `snd_musicvolume` | Float | 1.0f | controls music volume | `/workspace/src/common/audio/music/i_music.cpp:59` |
| `snd_pitched` | Bool | false | Likely controls snd pitched. | `/workspace/src/common/audio/sound/s_sound.cpp:43` |
| `snd_samplerate` | Int | 0 | Likely controls snd samplerate. | `/workspace/src/common/audio/sound/i_sound.cpp:40` |
| `snd_sfxvolume` | Float | 1.f | Likely controls snd sfxvolume. | `/workspace/src/common/audio/sound/i_sound.cpp:104` |
| `snd_streambuffersize` | Int | 64 | Likely controls snd streambuffersize. | `/workspace/src/common/audio/music/music_config.cpp:513` |
| `snd_superstereowidth` | Float | 0.45f | Likely controls snd superstereowidth. | `/workspace/src/common/audio/sound/oalsound.cpp:71` |
| `snd_waterreverb` | Bool | true | Likely controls snd waterreverb. | `/workspace/src/common/audio/sound/oalsound.cpp:58` |

### Category: Music {#category-music}

MIDI, module, and streaming music playback configuration.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `mus_calcgain` | Bool | true | Likely controls mus calcgain. | `/workspace/src/common/audio/music/music.cpp:85` |
| `mus_enabled` | Bool | true | enables/disables music | `/workspace/src/common/audio/music/i_music.cpp:86` |
| `mus_usereplaygain` | Bool | false | Likely controls mus usereplaygain. | `/workspace/src/common/audio/music/music.cpp:86` |

### Category: Renderer {#category-rendering}

Hardware and software renderer options, lighting, sprites, and draw quality.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `gl_aalines` | Bool | false | Likely controls gl aalines. | `/workspace/src/common/rendering/hwrenderer/hw_draw2d.cpp:41` |
| `gl_bandedswlight` | Bool | false | Likely controls gl bandedswlight. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:45` |
| `gl_billboard_faces_camera` | Bool | false | Likely controls gl billboard faces camera. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:82` |
| `gl_billboard_mode` | Int | 0 | Likely controls gl billboard mode. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:81` |
| `gl_billboard_particles` | Bool | true | Likely controls gl billboard particles. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:84` |
| `gl_bloom` | Bool | false | Likely controls gl bloom. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:31` |
| `gl_bloom_amount` | Float | 1.4f | Likely controls gl bloom amount. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:32` |
| `gl_breaksec` | Int | -1 | Likely controls gl breaksec. | `/workspace/src/rendering/hwrenderer/scene/hw_flats.cpp:44` |
| `gl_brightfog` | Bool | false | Likely controls gl brightfog. | `/workspace/src/g_level.cpp:124` |
| `gl_cachenodes` | Bool | true | Likely controls gl cachenodes. | `/workspace/src/g_cvars.cpp:138` |
| `gl_cachetime` | Float | 0.6f | Likely controls gl cachetime. | `/workspace/src/g_cvars.cpp:139` |
| `gl_control_tear` | Bool | false | Likely controls gl control tear. | `/workspace/src/common/platform/win32/gl_sysfb.cpp:103` |
| `gl_coronas` | Bool | true | Likely controls gl coronas. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:54` |
| `gl_custompost` | Bool | true | Likely controls gl custompost. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess.cpp:901` |
| `gl_customshader` | Bool | true | Likely controls gl customshader. | `/workspace/src/common/textures/hw_material.cpp:28` |
| `gl_debug` | Bool | false | Likely controls gl debug. | `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:78` |
| `gl_debug_breakpoint` | Bool | false | Likely controls gl debug breakpoint. | `/workspace/src/common/rendering/gl/gl_debug.cpp:39` |
| `gl_debug_level` | Int | 0 | Likely controls gl debug level. | `/workspace/src/common/rendering/gl/gl_debug.cpp:31` |
| `gl_distfog` | Int | 70 | Likely controls gl distfog. | `/workspace/src/rendering/hwrenderer/scene/hw_lighting.cpp:36` |
| `gl_dither_bpc` | Int | 0 | Likely controls gl dither bpc. | `/workspace/src/common/rendering/gl/gl_postprocess.cpp:46` |
| `gl_enhanced_nightvision` | Bool | false | Likely controls gl enhanced nightvision. | `/workspace/src/rendering/hwrenderer/scene/hw_lighting.cpp:28` |
| `gl_enhanced_nv_stealth` | Int | 3 | Likely controls gl enhanced nv stealth. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:48` |
| `gl_es` | Bool | false | Likely controls gl es. | `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:82` |
| `gl_exposure_base` | Float | 0.35f | Likely controls gl exposure base. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:39` |
| `gl_exposure_min` | Float | 0.35f | Likely controls gl exposure min. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:38` |
| `gl_exposure_scale` | Float | 1.3f | Likely controls gl exposure scale. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:37` |
| `gl_exposure_speed` | Float | 0.05f | Likely controls gl exposure speed. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:40` |
| `gl_finishbeforeswap` | Bool | false | Likely controls gl finishbeforeswap. | `/workspace/src/common/rendering/gl/gl_framebuffer.cpp:251` |
| `gl_fogmode` | Int | 2 | Likely controls gl fogmode. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:32` |
| `gl_fuzztype` | Int | 8 | Likely controls gl fuzztype. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:85` |
| `gl_fxaa` | Int | 0 | Likely controls gl fxaa. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:54` |
| `gl_interpolate_model_frames` | Bool | true | Likely controls gl interpolate model frames. | `/workspace/src/r_data/models.cpp:45` |
| `gl_lens` | Bool | false | Likely controls gl lens. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:48` |
| `gl_lens_chromatic` | Float | 1.12f | Likely controls gl lens chromatic. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:52` |
| `gl_lens_k` | Float | -0.12f | Likely controls gl lens k. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:50` |
| `gl_lens_kcube` | Float | 0.1f | Likely controls gl lens kcube. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:51` |
| `gl_light_models` | Bool | true | Likely controls gl light models. | `/workspace/src/rendering/hwrenderer/hw_models.cpp:36` |
| `gl_light_particles` | Bool | true | Likely controls gl light particles. | `/workspace/src/rendering/hwrenderer/hw_dynlightdata.cpp:36` |
| `gl_light_shadowmap` | Bool | false | Likely controls gl light shadowmap. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:67` |
| `gl_light_sprites` | Bool | true | Likely controls gl light sprites. | `/workspace/src/rendering/hwrenderer/hw_dynlightdata.cpp:35` |
| `gl_lightadditivesurfaces` | Bool | false | Likely controls gl lightadditivesurfaces. | `/workspace/src/g_level.cpp:132` |
| `gl_lightmode` | Int | 1 | Select lighting mode. 2 is vanilla accurate, 1 is accurate to the ZDoom software renderer and 0 is a less demanding non-shader implementation | `/workspace/src/g_level.cpp:153` |
| `gl_lights` | Bool | true | Likely controls gl lights. | `/workspace/src/g_cvars.cpp:164` |
| `gl_maplightmode` | Int | -1 | Likely controls gl maplightmode. | `/workspace/src/g_level.cpp:148` |
| `gl_mask_sprite_threshold` | Float | 0.5f | Likely controls gl mask sprite threshold. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:52` |
| `gl_mask_threshold` | Float | 0.5f | Likely controls gl mask threshold. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:51` |
| `gl_menu_blur` | Float | -1.0f | Likely controls gl menu blur. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:98` |
| `gl_mirror_envmap` | Bool | true | Likely controls gl mirror envmap. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:42` |
| `gl_mirrors` | Bool | true | Likely controls gl mirrors. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:41` |
| `gl_multisample` | Int | 1 | Likely controls gl multisample. | `/workspace/src/common/rendering/gl/gl_renderbuffers.cpp:38` |
| `gl_multithread` | Bool | true | Likely controls gl multithread. | `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:44` |
| `gl_no_skyclear` | Bool | false | Likely controls gl no skyclear. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:47` |
| `gl_noskyboxes` | Bool | false | Likely controls gl noskyboxes. | `/workspace/src/rendering/hwrenderer/scene/hw_sky.cpp:33` |
| `gl_notexturefill` | Bool | false | Likely controls gl notexturefill. | `/workspace/src/g_level.cpp:140` |
| `gl_paltonemap_powtable` | Float | 2.0f | Likely controls gl paltonemap powtable. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:88` |
| `gl_paltonemap_reverselookup` | Bool | true | Likely controls gl paltonemap reverselookup. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:93` |
| `gl_particles_style` | Int | 0 | Likely controls gl particles style. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:80` |
| `gl_pipeline_depth` | Int | 0 | Likely controls gl pipeline depth. | `/workspace/src/common/rendering/v_video.cpp:59` |
| `gl_plane_reflection` | Bool | true | Likely controls gl plane reflection. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:52` |
| `gl_portals` | Bool | true | Likely controls gl portals. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:40` |
| `gl_precache` | Bool | false | Likely controls gl precache. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:197` |
| `gl_render_flats` | Bool | true | Likely controls gl render flats. | `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:248` |
| `gl_render_things` | Bool | true | Likely controls gl render things. | `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:246` |
| `gl_render_walls` | Bool | true | Likely controls gl render walls. | `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:247` |
| `gl_satformula` | Int | 2 | Likely controls gl satformula. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:179` |
| `gl_scale_viewport` | Bool | true | Likely controls gl scale viewport. | `/workspace/src/common/rendering/v_framebuffer.cpp:48` |
| `gl_sclipfactor` | Float | 1.8f | Likely controls gl sclipfactor. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:79` |
| `gl_sclipthreshold` | Float | 10.0 | Likely controls gl sclipthreshold. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:78` |
| `gl_seamless` | Bool | true | Likely controls gl seamless. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:43` |
| `gl_shadowmap_filter` | Int | 0 | Likely controls gl shadowmap filter. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:200` |
| `gl_shadowmap_maxlights` | Int | 0 | Likely controls gl shadowmap maxlights. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:296` |
| `gl_shadowmap_prioritize` | Bool | true | Likely controls gl shadowmap prioritize. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:68` |
| `gl_shadowmap_quality` | Int | 512 | Likely controls gl shadowmap quality. | `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:315` |
| `gl_sort_textures` | Bool | false | Likely controls gl sort textures. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:46` |
| `gl_sprite_blend` | Bool | false | Likely controls gl sprite blend. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:74` |
| `gl_spriteclip` | Int | -1 | Likely controls gl spriteclip. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:75` |
| `gl_ssao` | Int | 0 | Likely controls gl ssao. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:62` |
| `gl_ssao_bias` | Float | 0.2f | Likely controls gl ssao bias. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:76` |
| `gl_ssao_blur` | Float | 16.0f | Likely controls gl ssao blur. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:78` |
| `gl_ssao_debug` | Int | 0 | Likely controls gl ssao debug. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:75` |
| `gl_ssao_exponent` | Float | 1.8f | Likely controls gl ssao exponent. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:83` |
| `gl_ssao_portals` | Int | 1 | Likely controls gl ssao portals. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:68` |
| `gl_ssao_radius` | Float | 80.0f | Likely controls gl ssao radius. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:77` |
| `gl_ssao_strength` | Float | 0.7f | Likely controls gl ssao strength. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:74` |
| `gl_texture` | Bool | true | Likely controls gl texture. | `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:50` |
| `gl_texture_filter` | Int | 6 | changes the texture filtering settings | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:191` |
| `gl_texture_filter_anisotropic` | Float | 16.f | changes the OpenGL texture anisotropy setting | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:186` |
| `gl_texture_hqresize_fonts` | Flag | gl_texture_hqresize_targets | Flag alias backed by gl_texture_hqresize_targets. | `/workspace/src/common/textures/hires/hqresize.cpp:76` |
| `gl_texture_hqresize_maxinputsize` | Int | 512 | Likely controls gl texture hqresize maxinputsize. | `/workspace/src/common/textures/hires/hqresize.cpp:62` |
| `gl_texture_hqresize_mt_height` | Int | 4 | Likely controls gl texture hqresize mt height. | `/workspace/src/common/textures/hires/hqresize.cpp:87` |
| `gl_texture_hqresize_mt_width` | Int | 16 | Likely controls gl texture hqresize mt width. | `/workspace/src/common/textures/hires/hqresize.cpp:81` |
| `gl_texture_hqresize_multithread` | Bool | true | Likely controls gl texture hqresize multithread. | `/workspace/src/common/textures/hires/hqresize.cpp:79` |
| `gl_texture_hqresize_skins` | Flag | gl_texture_hqresize_targets | Flag alias backed by gl_texture_hqresize_targets. | `/workspace/src/common/textures/hires/hqresize.cpp:77` |
| `gl_texture_hqresize_sprites` | Flag | gl_texture_hqresize_targets | Flag alias backed by gl_texture_hqresize_targets. | `/workspace/src/common/textures/hires/hqresize.cpp:75` |
| `gl_texture_hqresize_targets` | Int | 15 | Likely controls gl texture hqresize targets. | `/workspace/src/common/textures/hires/hqresize.cpp:68` |
| `gl_texture_hqresize_textures` | Flag | gl_texture_hqresize_targets | Flag alias backed by gl_texture_hqresize_targets. | `/workspace/src/common/textures/hires/hqresize.cpp:74` |
| `gl_texture_hqresizemode` | Int | 0 | Likely controls gl texture hqresizemode. | `/workspace/src/common/textures/hires/hqresize.cpp:42` |
| `gl_texture_hqresizemult` | Int | 1 | Likely controls gl texture hqresizemult. | `/workspace/src/common/textures/hires/hqresize.cpp:52` |
| `gl_tonemap` | Int | 0 | Likely controls gl tonemap. | `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:42` |
| `gl_usecolorblending` | Bool | true | Likely controls gl usecolorblending. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:73` |
| `gl_weapon_purelightlevel` | Bool | false | Makes the lighting on weapon sprites (or models) purely match the sector's light level you're standing in | `/workspace/src/rendering/hwrenderer/scene/hw_weapon.cpp:50` |
| `gl_weaponlight` | Int | 8 | Likely controls gl weaponlight. | `/workspace/src/rendering/hwrenderer/scene/hw_lighting.cpp:27` |
| `hw_2dmip` | Bool | true | Likely controls hw 2dmip. | `/workspace/src/common/rendering/hwrenderer/hw_draw2d.cpp:42` |
| `hw_force_cambbpref` | Bool | false | Likely controls hw force cambbpref. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:83` |
| `hw_npottest` | Bool | false | Likely controls hw npottest. | `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:202` |
| `r_3dfloors` | Int | 1 | Likely controls r 3dfloors. | `/workspace/src/rendering/swrenderer/scene/r_3dfloors.cpp:35` |
| `r_actorspriteshadow` | Int | 2 | render actor sprite shadows. 0 = off, 1 = default, 2 = always on | `/workspace/src/rendering/r_utility.cpp:108` |
| `r_actorspriteshadowalpha` | Float | 0.7 | maximum sprite shadow opacity, only effective with hardware renderers (0.0 = fully transparent, 1.0 = opaque) | `/workspace/src/rendering/r_utility.cpp:122` |
| `r_actorspriteshadowdist` | Float | 2200.0 | how far sprite shadows should be rendered | `/workspace/src/rendering/r_utility.cpp:115` |
| `r_actorspriteshadowfadeheight` | Float | 0.0 | distance over which sprite shadows should fade, only effective with hardware renderers (0 = infinite) | `/workspace/src/rendering/r_utility.cpp:129` |
| `r_actorspriteshadowstyle` | Int | 1 | actor sprite shadow style. 0 = classic, 1 = quake-style, 2 = doom3-style | `/workspace/src/rendering/r_utility.cpp:136` |
| `r_blendmethod` | Bool | false | Likely controls r blendmethod. | `/workspace/src/rendering/swrenderer/drawers/r_draw_pal.cpp:40` |
| `r_clearbuffer` | Int | 0 | Likely controls r clearbuffer. | `/workspace/src/rendering/r_utility.cpp:97` |
| `r_crosshair_recoil` | Bool | false | Likely controls r crosshair recoil. | `/workspace/src/i_input_feel.cpp:29` |
| `r_deathcamera` | Bool | false | Likely controls r deathcamera. | `/workspace/src/rendering/r_utility.cpp:96` |
| `r_debug_disable_vis_filter` | Bool | false | Likely controls r debug disable vis filter. | `/workspace/src/d_main.cpp:525` |
| `r_debug_draw` | Int | 0 | Likely controls r debug draw. | `/workspace/src/common/rendering/r_thread.cpp:34` |
| `r_debug_nolimitanamorphoses` | Bool | false | Likely controls r debug nolimitanamorphoses. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:76` |
| `r_dithertransparency` | Bool | false | Use dithered-transparency shading for actor-occluding level geometry | `/workspace/src/rendering/r_utility.cpp:101` |
| `r_drawfuzz` | Int | 1 | Likely controls r drawfuzz. | `/workspace/src/common/engine/renderstyle.cpp:31` |
| `r_drawmirrors` | Bool | true | Likely controls r drawmirrors. | `/workspace/src/rendering/swrenderer/line/r_line.cpp:62` |
| `r_drawplayersprites` | Bool | true | Likely controls r drawplayersprites. | `/workspace/src/rendering/r_utility.cpp:99` |
| `r_drawtrans` | Bool | true | Likely controls r drawtrans. | `/workspace/src/common/engine/renderstyle.cpp:30` |
| `r_drawvoxels` | Bool | true | Likely controls r drawvoxels. | `/workspace/src/rendering/r_utility.cpp:98` |
| `r_dynlights` | Bool | true | Likely controls r dynlights. | `/workspace/src/rendering/swrenderer/drawers/r_draw.cpp:49` |
| `r_extralight` | Int | 0 | Likely controls r extralight. | `/workspace/src/rendering/r_utility.cpp:354` |
| `r_fakecontrast` | Int | 1 | Likely controls r fakecontrast. | `/workspace/src/playsim/p_sectors.cpp:53` |
| `r_fakeradio` | Bool | false | Likely controls r fakeradio. | `/workspace/src/r_doomsday_features.cpp:19` |
| `r_fakeradio_strength` | Float | 0.5f | Likely controls r fakeradio strength. | `/workspace/src/r_doomsday_features.cpp:21` |
| `r_fogboundary` | Bool | true | Likely controls r fogboundary. | `/workspace/src/rendering/swrenderer/line/r_line.cpp:61` |
| `r_fullbright_overrides` | Bool | false | Likely controls r fullbright overrides. | `/workspace/src/d_main.cpp:519` |
| `r_fullbrightignoresectorcolor` | Bool | true | Likely controls r fullbrightignoresectorcolor. | `/workspace/src/rendering/swrenderer/scene/r_translucent_pass.cpp:52` |
| `r_fuzzscale` | Bool | true | Likely controls r fuzzscale. | `/workspace/src/rendering/swrenderer/drawers/r_draw.cpp:54` |
| `r_geom_ao` | Bool | false | Likely controls r geom ao. | `/workspace/src/r_doomsday_features.cpp:28` |
| `r_geom_ao_strength` | Float | 0.4f | Likely controls r geom ao strength. | `/workspace/src/r_doomsday_features.cpp:30` |
| `r_highlight_portals` | Bool | false | Likely controls r highlight portals. | `/workspace/src/rendering/swrenderer/scene/r_portal.cpp:70` |
| `r_killfeed` | Bool | false | Likely controls r killfeed. | `/workspace/src/i_input_feel.cpp:30` |
| `r_line_distance_cull` | Float | 0.f | Likely controls r line distance cull. | `/workspace/src/rendering/swrenderer/scene/r_opaque_pass.cpp:95` |
| `r_linearsky` | Bool | false | Likely controls r linearsky. | `/workspace/src/rendering/swrenderer/plane/r_skyplane.cpp:55` |
| `r_lod_bias` | Float | -1.5 | Likely controls r lod bias. | `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:70` |
| `r_magfilter` | Bool | false | Likely controls r magfilter. | `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:61` |
| `r_maxparticles` | Int | 4000 | Likely controls r maxparticles. | `/workspace/src/g_cvars.cpp:218` |
| `r_minfilter` | Bool | true | Likely controls r minfilter. | `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:64` |
| `r_mipmap` | Bool | true | Likely controls r mipmap. | `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:67` |
| `r_model_distance_cull` | Float | 1024.f | Likely controls r model distance cull. | `/workspace/src/rendering/swrenderer/scene/r_opaque_pass.cpp:107` |
| `r_models` | Bool | true | Likely controls r models. | `/workspace/src/rendering/swrenderer/scene/r_scene.cpp:42` |
| `r_multithreaded` | Int | 1 | Likely controls r multithreaded. | `/workspace/src/common/rendering/r_thread.cpp:33` |
| `r_noaccel` | Bool | false | Likely controls r noaccel. | `/workspace/src/rendering/swrenderer/things/r_playersprite.cpp:75` |
| `r_particles` | Bool | true | Likely controls r particles. | `/workspace/src/playsim/p_effect.cpp:55` |
| `r_portal_recursions` | Int | 4 | Likely controls r portal recursions. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:45` |
| `r_quakeintensity` | Float | 1.0f | Likely controls r quakeintensity. | `/workspace/src/rendering/r_utility.cpp:102` |
| `r_radarclipper` | Bool | false | Use the horizontal clipper from camera->tracer's perspective | `/workspace/src/rendering/r_utility.cpp:100` |
| `r_rail_smartspiral` | Bool | false | Likely controls r rail smartspiral. | `/workspace/src/playsim/p_effect.cpp:52` |
| `r_rail_spiralsparsity` | Int | 1 | Likely controls r rail spiralsparsity. | `/workspace/src/playsim/p_effect.cpp:53` |
| `r_rail_trailsparsity` | Int | 1 | Likely controls r rail trailsparsity. | `/workspace/src/playsim/p_effect.cpp:54` |
| `r_scene_multithreaded` | Int | 1 | Likely controls r scene multithreaded. | `/workspace/src/rendering/swrenderer/scene/r_scene.cpp:41` |
| `r_skipmats` | Bool | false | Likely controls r skipmats. | `/workspace/src/common/rendering/v_video.cpp:56` |
| `r_skyboxes` | Bool | true | Likely controls r skyboxes. | `/workspace/src/rendering/swrenderer/scene/r_portal.cpp:72` |
| `r_skymode` | Int | 2 | Likely controls r skymode. | `/workspace/src/rendering/r_sky.cpp:44` |
| `r_sprite_distance_cull` | Float | 0.f | Likely controls r sprite distance cull. | `/workspace/src/rendering/swrenderer/scene/r_opaque_pass.cpp:83` |
| `r_spriteadjust` | Int | 2 | Likely controls r spriteadjust. | `/workspace/src/common/textures/gametexture.cpp:425` |
| `r_spriteclipanamorphicminbias` | Float | 0.6 | Likely controls r spriteclipanamorphicminbias. | `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:77` |
| `r_ticstability` | Bool | true | Likely controls r ticstability. | `/workspace/src/d_net_diag_commands.cpp:1549` |
| `r_vanillatrans` | Int | 0 | Likely controls r vanillatrans. | `/workspace/src/r_data/r_vanillatrans.cpp:32` |
| `r_view_pain_smooth` | Bool | false | Likely controls r view pain smooth. | `/workspace/src/r_view_pain_smooth.cpp:30` |
| `r_view_pain_smooth_strength` | Float | 0.6f | Likely controls r view pain smooth strength. | `/workspace/src/r_view_pain_smooth.cpp:35` |
| `r_viewsize` | String | "" | Likely controls r viewsize. | `/workspace/src/rendering/swrenderer/viewport/r_viewport.cpp:46` |
| `r_visibility` | Float | 8.0f | Likely controls r visibility. | `/workspace/src/rendering/r_utility.cpp:336` |
| `r_weapon_bob_smooth` | Bool | false | Likely controls r weapon bob smooth. | `/workspace/src/playsim/p_pspr.cpp:679` |

### Category: Video & Display {#category-video}

Resolution, scaling, fullscreen, vsync, and framebuffer settings.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `vid_activeinbackground` | Bool | false | Likely controls vid activeinbackground. | `/workspace/src/d_main.cpp:970` |
| `vid_adapter` | Int | 0 | Likely controls vid adapter. | `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:224` |
| `vid_allowtrueultrawide` | Int | 1 | Likely controls vid allowtrueultrawide. | `/workspace/src/common/2d/v_draw.cpp:43` |
| `vid_aspect` | Int | 0 | Likely controls vid aspect. | `/workspace/src/common/rendering/v_video.cpp:437` |
| `vid_blackpoint` | Float | 0.f | adjusts what the engine outputs as black | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:152` |
| `vid_contrast` | Float | 1.f | adjusts contrast component of gamma ramp | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:133` |
| `vid_cropaspect` | Bool | false | Likely controls vid cropaspect. | `/workspace/src/common/rendering/r_videoscale.cpp:187` |
| `vid_cursor` | String | "None" | Likely controls vid cursor. | `/workspace/src/d_main.cpp:490` |
| `vid_defheight` | Int | 480 | Likely controls vid defheight. | `/workspace/src/common/rendering/v_video.cpp:203` |
| `vid_defwidth` | Int | 640 | Likely controls vid defwidth. | `/workspace/src/common/rendering/v_video.cpp:202` |
| `vid_dontdowait` | Bool | false | Likely controls vid dontdowait. | `/workspace/src/d_net_invasion.inl:38` |
| `vid_fixgamma` | Float | 0.0f | adjusts gamma component of gamma ramp | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:118` |
| `vid_fps` | Bool | false | Likely controls vid fps. | `/workspace/src/common/engine/i_interface.cpp:52` |
| `vid_fsdwmhack` | Bool | false | Likely controls vid fsdwmhack. | `/workspace/src/common/platform/win32/base_sysfb.cpp:56` |
| `vid_fsdwmhackalpha` | Int | 255 | Likely controls vid fsdwmhackalpha. | `/workspace/src/common/platform/win32/base_sysfb.cpp:60` |
| `vid_fullscreen` | Bool | true | Likely controls vid fullscreen. | `/workspace/src/common/rendering/v_video.cpp:468` |
| `vid_gamma` | Float | GAMMA_DEFAULT | (internal) target output gamma | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:83` |
| `vid_hdr` | Bool | false | Likely controls vid hdr. | `/workspace/src/common/rendering/v_video.cpp:473` |
| `vid_i_blackpoint` | Float | 1.f | Likely controls vid i blackpoint. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:149` |
| `vid_i_whitepoint` | Float | 1.f | Likely controls vid i whitepoint. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:150` |
| `vid_lowerinbackground` | Bool | true | Likely controls vid lowerinbackground. | `/workspace/src/d_net_invasion.inl:39` |
| `vid_maxfps` | Int | 500 | Likely controls vid maxfps. | `/workspace/src/common/rendering/v_video.cpp:73` |
| `vid_nopalsubstitutions` | Bool | false | Likely controls vid nopalsubstitutions. | `/workspace/src/rendering/swrenderer/textures/r_swtexture.cpp:540` |
| `vid_preferbackend` | Int | BACKEND_DEFAULT | Likely controls vid preferbackend. | `/workspace/src/common/rendering/v_video.cpp:87` |
| `vid_renderer` | Int | 1 | Likely controls vid renderer. | `/workspace/src/d_main.cpp:147` |
| `vid_rendermode` | Int | 4 | Likely controls vid rendermode. | `/workspace/src/d_main.cpp:444` |
| `vid_saturation` | Float | 1.f | adjusts saturation component of gamma ramp | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:139` |
| `vid_scale_customheight` | Int | VID_MIN_HEIGHT | Likely controls vid scale customheight. | `/workspace/src/common/rendering/r_videoscale.cpp:48` |
| `vid_scale_custompixelaspect` | Float | 1.0 | Likely controls vid scale custompixelaspect. | `/workspace/src/common/rendering/r_videoscale.cpp:55` |
| `vid_scale_customwidth` | Int | VID_MIN_WIDTH | Likely controls vid scale customwidth. | `/workspace/src/common/rendering/r_videoscale.cpp:42` |
| `vid_scale_linear` | Bool | false | Likely controls vid scale linear. | `/workspace/src/common/rendering/r_videoscale.cpp:54` |
| `vid_scalefactor` | Float | 1.0 | Likely controls vid scalefactor. | `/workspace/src/common/rendering/r_videoscale.cpp:171` |
| `vid_scalemode` | Int | 0 | Likely controls vid scalemode. | `/workspace/src/common/rendering/r_videoscale.cpp:180` |
| `vid_sdl_render_driver` | String | "" | Likely controls vid sdl render driver. | `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:87` |
| `vid_shadersupport` | Bool | true | Likely controls vid shadersupport. | `/workspace/src/common/rendering/v_video.cpp:85` |
| `vid_showpalette` | Int | 0 | Likely controls vid showpalette. | `/workspace/src/d_main.cpp:526` |
| `vid_vsync` | Bool | false | Likely controls vid vsync. | `/workspace/src/common/rendering/v_video.cpp:206` |
| `vid_whitepoint` | Float | 0.f | adjusts what the engine outputs as white | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:164` |

### Category: Gameplay {#category-gameplay}

Movement, weapons, monsters, compatibility, and general play simulation.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `compat_anybossdeath` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:935` |
| `compat_avoidhazard` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:957` |
| `compat_badangles` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:946` |
| `compat_boomscroll` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:929` |
| `compat_checkswitchrange` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:953` |
| `compat_crossdropoff` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:934` |
| `compat_dehhealth` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:926` |
| `compat_dr_crusher` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:966` |
| `compat_dr_liquidfriction` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:967` |
| `compat_dropoff` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:928` |
| `compat_emulatemikoportals` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:963` |
| `compat_explode1` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:954` |
| `compat_explode2` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:955` |
| `compat_fdteleport` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:961` |
| `compat_floormove` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:947` |
| `compat_hitscan` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:942` |
| `compat_invisibility` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:930` |
| `compat_light` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:943` |
| `compat_limitpain` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:916` |
| `compat_maskedmidtex` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:945` |
| `compat_mbfmonstermove` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:938` |
| `compat_minotaur` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:936` |
| `compat_missileclip` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:933` |
| `compat_multiexit` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:950` |
| `compat_mushroom` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:937` |
| `compat_noblockfriends` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:940` |
| `compat_nodoorlight` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:923` |
| `compat_nofriendlyspawn` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:965` |
| `compat_noid24` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:968` |
| `compat_nombf21` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:959` |
| `compat_nopassover` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:918` |
| `compat_notossdrops` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:921` |
| `compat_novdolllockmsg` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:962` |
| `compat_pointonline` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:949` |
| `compat_polyobj` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:944` |
| `compat_pushwindow` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:952` |
| `compat_railing` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:956` |
| `compat_ravenscroll` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:924` |
| `compat_reservedlineflag` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:964` |
| `compat_sectorsounds` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:932` |
| `compat_shortTex` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:914` |
| `compat_silentinstantfloors` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:931` |
| `compat_silentpickup` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:917` |
| `compat_soundcutoff` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:948` |
| `compat_soundslots` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:919` |
| `compat_soundtarget` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:925` |
| `compat_spritesort` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:941` |
| `compat_stairs` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:915` |
| `compat_stayonlift` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:958` |
| `compat_teleport` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:951` |
| `compat_trace` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:927` |
| `compat_useblocking` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:922` |
| `compat_vileghosts` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:939` |
| `compat_voodoozombies` | Flag | compatflags2 | Flag alias backed by compatflags2. | `/workspace/src/d_main.cpp:960` |
| `compat_wallrun` | Flag | compatflags | Flag alias backed by compatflags. | `/workspace/src/d_main.cpp:920` |
| `compatflags` | Int | 0 | Server setting: Raw Compat Flags | `/workspace/src/d_main.cpp:824` |
| `compatflags2` | Int | 0 | Server setting: Raw Compat Flags 2 | `/workspace/src/d_main.cpp:832` |
| `compatmode` | Int | 0 | Server setting: Compat Mode | `/workspace/src/d_main.cpp:841` |
| `deathmatch` | Int | 0 | Likely controls deathmatch. | `/workspace/src/g_game.cpp:290` |
| `playerclass` | String | "Fighter" | Likely controls playerclass. | `/workspace/src/d_netinfo.cpp:60` |
| `skill` | Int | 2 | sets the skill for the next newly started game | `/workspace/src/g_game.cpp:288` |
| `teamplay` | Bool | false | Likely controls teamplay. | `/workspace/src/g_game.cpp:336` |

### Category: HUD & Status Bar {#category-hud}

Heads-up display, crosshair, messages, and status bar layout.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `crosshair` | Int | 1 | Likely controls crosshair. | `/workspace/src/g_statusbar/shared_sbar.cpp:119` |
| `crosshair_offset_x` | Float | 0. | Likely controls crosshair offset x. | `/workspace/src/g_statusbar/shared_sbar.cpp:86` |
| `crosshair_offset_y` | Float | 0. | Likely controls crosshair offset y. | `/workspace/src/g_statusbar/shared_sbar.cpp:87` |
| `crosshaircolor` | Color | 0xff0000 | Likely controls crosshaircolor. | `/workspace/src/common/statusbar/base_sbar.cpp:47` |
| `crosshaircolorFull` | Color | 0x00ff00 | Likely controls crosshaircolorFull. | `/workspace/src/common/statusbar/base_sbar.cpp:48` |
| `crosshaircolorMax` | Color | 0x7f7fff | Likely controls crosshaircolorMax. | `/workspace/src/common/statusbar/base_sbar.cpp:49` |
| `crosshaircolors` | Int | 2 | 0: basic, 1: show health, 2: show health bonus, 3: inverted | `/workspace/src/common/statusbar/base_sbar.cpp:52` |
| `crosshairforce` | Bool | false | Likely controls crosshairforce. | `/workspace/src/g_statusbar/shared_sbar.cpp:120` |
| `crosshairgrow` | Bool | false | grow crosshair upon pickup | `/workspace/src/common/statusbar/base_sbar.cpp:64` |
| `crosshairhascolor` | Bool | false | Likely controls crosshairhascolor. | `/workspace/src/common/statusbar/base_sbar.cpp:51` |
| `crosshairon` | Bool | true | Likely controls crosshairon. | `/workspace/src/g_statusbar/shared_sbar.cpp:118` |
| `crosshairscale` | Float | 1.0 | changes the size of the crosshair | `/workspace/src/common/statusbar/base_sbar.cpp:63` |
| `crosshairshowshealth` | Bool | false | Likely controls crosshairshowshealth. | `/workspace/src/common/statusbar/base_sbar.cpp:50` |
| `hud_althud` | Bool | false | Likely controls hud althud. | `/workspace/src/g_statusbar/shared_hud.cpp:47` |
| `hud_althud_forceinternal` | Bool | false | Likely controls hud althud forceinternal. | `/workspace/src/g_statusbar/shared_hud.cpp:97` |
| `hud_althudscale` | Int | 0 | Likely controls hud althudscale. | `/workspace/src/g_statusbar/shared_hud.cpp:46` |
| `hud_ammo_order` | Int | 0 | Likely controls hud ammo order. | `/workspace/src/g_statusbar/shared_hud.cpp:64` |
| `hud_ammo_red` | Int | 25 | Likely controls hud ammo red. | `/workspace/src/g_statusbar/shared_hud.cpp:65` |
| `hud_ammo_yellow` | Int | 50 | Likely controls hud ammo yellow. | `/workspace/src/g_statusbar/shared_hud.cpp:66` |
| `hud_armor_green` | Int | 100 | Likely controls hud armor green. | `/workspace/src/g_statusbar/shared_hud.cpp:73` |
| `hud_armor_red` | Int | 25 | Likely controls hud armor red. | `/workspace/src/g_statusbar/shared_hud.cpp:71` |
| `hud_armor_yellow` | Int | 50 | Likely controls hud armor yellow. | `/workspace/src/g_statusbar/shared_hud.cpp:72` |
| `hud_aspectscale` | Bool | true | enables aspect ratio correction for the status bar | `/workspace/src/common/statusbar/base_sbar.cpp:73` |
| `hud_berserk_health` | Bool | true | Likely controls hud berserk health. | `/workspace/src/g_statusbar/shared_hud.cpp:75` |
| `hud_health_green` | Int | 100 | Likely controls hud health green. | `/workspace/src/g_statusbar/shared_hud.cpp:70` |
| `hud_health_red` | Int | 25 | Likely controls hud health red. | `/workspace/src/g_statusbar/shared_hud.cpp:68` |
| `hud_health_yellow` | Int | 50 | Likely controls hud health yellow. | `/workspace/src/g_statusbar/shared_hud.cpp:69` |
| `hud_oldscale` | Bool | true | Likely controls hud oldscale. | `/workspace/src/g_statusbar/shared_sbar.cpp:85` |
| `hud_scale` | Int | -1 | Likely controls hud scale. | `/workspace/src/g_statusbar/shared_sbar.cpp:83` |
| `hud_scalefactor` | Float | 1.f | changes the hud scale | `/workspace/src/common/statusbar/base_sbar.cpp:66` |
| `hud_showammo` | Int | 2 | Likely controls hud showammo. | `/workspace/src/g_statusbar/shared_hud.cpp:58` |
| `hud_showangles` | Bool | false | Likely controls hud showangles. | `/workspace/src/g_statusbar/shared_hud.cpp:76` |
| `hud_showitems` | Bool | false | Likely controls hud showitems. | `/workspace/src/g_statusbar/shared_hud.cpp:52` |
| `hud_showlag` | Int | 0 | Likely controls hud showlag. | `/workspace/src/g_statusbar/shared_hud.cpp:62` |
| `hud_showmonsters` | Bool | true | Likely controls hud showmonsters. | `/workspace/src/g_statusbar/shared_hud.cpp:51` |
| `hud_showscore` | Bool | false | Likely controls hud showscore. | `/workspace/src/g_statusbar/shared_hud.cpp:54` |
| `hud_showsecrets` | Bool | true | Likely controls hud showsecrets. | `/workspace/src/g_statusbar/shared_hud.cpp:50` |
| `hud_showstats` | Bool | false | Likely controls hud showstats. | `/workspace/src/g_statusbar/shared_hud.cpp:53` |
| `hud_showtime` | Int | 0 | Likely controls hud showtime. | `/workspace/src/g_statusbar/shared_hud.cpp:59` |
| `hud_showtimestat` | Int | 0 | Likely controls hud showtimestat. | `/workspace/src/g_statusbar/shared_hud.cpp:60` |
| `hud_showweapons` | Bool | true | Likely controls hud showweapons. | `/workspace/src/g_statusbar/shared_hud.cpp:55` |
| `hud_swaphealtharmor` | Bool | false | Likely controls hud swaphealtharmor. | `/workspace/src/g_statusbar/shared_hud.cpp:67` |
| `hud_timecolor` | Int | CR_GOLD | Likely controls hud timecolor. | `/workspace/src/g_statusbar/shared_hud.cpp:61` |
| `hud_toggled` | Bool | false | Likely controls hud toggled. | `/workspace/src/d_main.cpp:592` |
| `save_dir` | String | "" | Likely controls save dir. | `/workspace/src/common/menu/savegamemanager.cpp:41` |
| `save_formatted` | Bool | false | Likely controls save formatted. | `/workspace/src/g_game.cpp:289` |
| `save_sort_order` | Int | 1 | Likely controls save sort order. | `/workspace/src/common/menu/savegamemanager.cpp:43` |
| `screenblocks` | Int | 12 | Likely controls screenblocks. | `/workspace/src/rendering/r_utility.cpp:429` |
| `st_oldouch` | Bool | false | Likely controls st oldouch. | `/workspace/src/g_statusbar/sbar_mugshot.cpp:316` |
| `st_scale` | Int | -1 | Likely controls st scale. | `/workspace/src/g_statusbar/shared_sbar.cpp:101` |

### Category: Automap {#category-automap}

Automap colors, rotation, overlay, and navigation aids.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `am_backcolor` | Color | 0x6c5440 | Likely controls am backcolor. | `/workspace/src/am_map.cpp:269` |
| `am_cdwallcolor` | Color | 0x4c3820 | Likely controls am cdwallcolor. | `/workspace/src/am_map.cpp:276` |
| `am_cheat` | Int | 0 | Likely controls am cheat. | `/workspace/src/am_map.cpp:133` |
| `am_colorset` | Int | -1 | Likely controls am colorset. | `/workspace/src/am_map.cpp:159` |
| `am_customcolors` | Bool | true | Likely controls am customcolors. | `/workspace/src/am_map.cpp:160` |
| `am_drawmapback` | Int | 1 | Likely controls am drawmapback. | `/workspace/src/am_map.cpp:162` |
| `am_efwallcolor` | Color | 0x665555 | Likely controls am efwallcolor. | `/workspace/src/am_map.cpp:277` |
| `am_emptyspacemargin` | Int | 0 | Likely controls am emptyspacemargin. | `/workspace/src/am_map.cpp:168` |
| `am_fdwallcolor` | Color | 0x887058 | Likely controls am fdwallcolor. | `/workspace/src/am_map.cpp:275` |
| `am_followplayer` | Bool | true | Likely controls am followplayer. | `/workspace/src/am_map.cpp:191` |
| `am_gridcolor` | Color | 0x8b5a2b | Likely controls am gridcolor. | `/workspace/src/am_map.cpp:279` |
| `am_interlevelcolor` | Color | 0xff0000 | Likely controls am interlevelcolor. | `/workspace/src/am_map.cpp:284` |
| `am_intralevelcolor` | Color | 0x0000ff | Likely controls am intralevelcolor. | `/workspace/src/am_map.cpp:283` |
| `am_linealpha` | Float | 1.0f | Likely controls am linealpha. | `/workspace/src/am_map.cpp:119` |
| `am_lineantialiasing` | Int | 0 | Likely controls am lineantialiasing. | `/workspace/src/am_map.cpp:121` |
| `am_linethickness` | Int | 1 | Likely controls am linethickness. | `/workspace/src/am_map.cpp:120` |
| `am_lockedcolor` | Color | 0x007800 | Likely controls am lockedcolor. | `/workspace/src/am_map.cpp:282` |
| `am_map_secrets` | Int | 1 | Likely controls am map secrets. | `/workspace/src/am_map.cpp:161` |
| `am_markcolor` | Int | CR_GREY | Likely controls am markcolor. | `/workspace/src/am_map.cpp:198` |
| `am_markfont` | String | DEFAULT_FONT_NAME | Likely controls am markfont. | `/workspace/src/am_map.cpp:197` |
| `am_notseencolor` | Color | 0x6c6c6c | Likely controls am notseencolor. | `/workspace/src/am_map.cpp:281` |
| `am_ovcdwallcolor` | Color | 0x008844 | Likely controls am ovcdwallcolor. | `/workspace/src/am_map.cpp:304` |
| `am_ovefwallcolor` | Color | 0x008844 | Likely controls am ovefwallcolor. | `/workspace/src/am_map.cpp:302` |
| `am_overlay` | Int | 0 | Likely controls am overlay. | `/workspace/src/am_map.cpp:144` |
| `am_ovfdwallcolor` | Color | 0x008844 | Likely controls am ovfdwallcolor. | `/workspace/src/am_map.cpp:303` |
| `am_ovinterlevelcolor` | Color | 0xffff00 | Likely controls am ovinterlevelcolor. | `/workspace/src/am_map.cpp:307` |
| `am_ovlockedcolor` | Color | 0x008844 | Likely controls am ovlockedcolor. | `/workspace/src/am_map.cpp:301` |
| `am_ovotherwallscolor` | Color | 0x008844 | Likely controls am ovotherwallscolor. | `/workspace/src/am_map.cpp:300` |
| `am_ovportalcolor` | Color | 0x004022 | Likely controls am ovportalcolor. | `/workspace/src/am_map.cpp:318` |
| `am_ovsecretsectorcolor` | Color | 0x00ffff | Likely controls am ovsecretsectorcolor. | `/workspace/src/am_map.cpp:308` |
| `am_ovsecretwallcolor` | Color | 0x008844 | Likely controls am ovsecretwallcolor. | `/workspace/src/am_map.cpp:298` |
| `am_ovsectorfillalpha` | Float | 0.2f | Likely controls am ovsectorfillalpha. | `/workspace/src/am_map.cpp:317` |
| `am_ovsectorfillcolor` | Color | 0x000000 | Likely controls am ovsectorfillcolor. | `/workspace/src/am_map.cpp:316` |
| `am_ovspecialwallcolor` | Color | 0xffffff | Likely controls am ovspecialwallcolor. | `/workspace/src/am_map.cpp:299` |
| `am_ovtelecolor` | Color | 0xffff00 | Likely controls am ovtelecolor. | `/workspace/src/am_map.cpp:306` |
| `am_ovthingcolor` | Color | 0xe88800 | Likely controls am ovthingcolor. | `/workspace/src/am_map.cpp:310` |
| `am_ovthingcolor_citem` | Color | 0xe88800 | Likely controls am ovthingcolor citem. | `/workspace/src/am_map.cpp:315` |
| `am_ovthingcolor_friend` | Color | 0xe88800 | Likely controls am ovthingcolor friend. | `/workspace/src/am_map.cpp:311` |
| `am_ovthingcolor_item` | Color | 0xe88800 | Likely controls am ovthingcolor item. | `/workspace/src/am_map.cpp:314` |
| `am_ovthingcolor_monster` | Color | 0xe88800 | Likely controls am ovthingcolor monster. | `/workspace/src/am_map.cpp:312` |
| `am_ovthingcolor_ncmonster` | Color | 0xe88800 | Likely controls am ovthingcolor ncmonster. | `/workspace/src/am_map.cpp:313` |
| `am_ovunexploredsecretcolor` | Color | 0x00ffff | Likely controls am ovunexploredsecretcolor. | `/workspace/src/am_map.cpp:309` |
| `am_ovunseencolor` | Color | 0x00226e | Likely controls am ovunseencolor. | `/workspace/src/am_map.cpp:305` |
| `am_ovwallcolor` | Color | 0x00ff00 | Likely controls am ovwallcolor. | `/workspace/src/am_map.cpp:297` |
| `am_ovyourcolor` | Color | 0xfce8d8 | Likely controls am ovyourcolor. | `/workspace/src/am_map.cpp:296` |
| `am_portalcolor` | Color | 0x404040 | Likely controls am portalcolor. | `/workspace/src/am_map.cpp:294` |
| `am_portaloverlay` | Bool | true | Likely controls am portaloverlay. | `/workspace/src/am_map.cpp:192` |
| `am_rotate` | Int | 0 | Likely controls am rotate. | `/workspace/src/am_map.cpp:143` |
| `am_secretsectorcolor` | Color | 0xff00ff | Likely controls am secretsectorcolor. | `/workspace/src/am_map.cpp:285` |
| `am_secretwallcolor` | Color | 0x000000 | Likely controls am secretwallcolor. | `/workspace/src/am_map.cpp:272` |
| `am_sectorfillalpha` | Float | 0.4f | Likely controls am sectorfillalpha. | `/workspace/src/am_map.cpp:293` |
| `am_sectorfillcolor` | Color | 0x4e3621 | Likely controls am sectorfillcolor. | `/workspace/src/am_map.cpp:292` |
| `am_showalllines` | Int | -1 | Likely controls am showalllines. | `/workspace/src/am_map.cpp:126` |
| `am_showcluster` | Bool | false | Likely controls am showcluster. | `/workspace/src/g_statusbar/shared_hud.cpp:57` |
| `am_showepisode` | Bool | false | Likely controls am showepisode. | `/workspace/src/g_statusbar/shared_hud.cpp:56` |
| `am_showgrid` | Bool | false | Likely controls am showgrid. | `/workspace/src/am_map.cpp:193` |
| `am_showitems` | Bool | false | Likely controls am showitems. | `/workspace/src/am_map.cpp:155` |
| `am_showkeys` | Bool | true | Likely controls am showkeys. | `/workspace/src/am_map.cpp:163` |
| `am_showkeys_always` | Bool | false | Likely controls am showkeys always. | `/workspace/src/am_map.cpp:166` |
| `am_showlevelname` | Bool | true | Likely controls am showlevelname. | `/workspace/src/am_map.cpp:158` |
| `am_showmaplabel` | Int | 2 | Likely controls am showmaplabel. | `/workspace/src/g_statusbar/shared_sbar.cpp:121` |
| `am_showmonsters` | Bool | true | Likely controls am showmonsters. | `/workspace/src/am_map.cpp:154` |
| `am_showsecrets` | Bool | true | Likely controls am showsecrets. | `/workspace/src/am_map.cpp:153` |
| `am_showsubsector` | Int | -1 | Likely controls am showsubsector. | `/workspace/src/am_map.cpp:123` |
| `am_showthingsprites` | Int | 0 | Likely controls am showthingsprites. | `/workspace/src/am_map.cpp:165` |
| `am_showtime` | Bool | true | Likely controls am showtime. | `/workspace/src/am_map.cpp:156` |
| `am_showtotaltime` | Bool | false | Likely controls am showtotaltime. | `/workspace/src/am_map.cpp:157` |
| `am_showtriggerlines` | Int | 0 | Likely controls am showtriggerlines. | `/workspace/src/am_map.cpp:164` |
| `am_specialwallcolor` | Color | 0xffffff | Likely controls am specialwallcolor. | `/workspace/src/am_map.cpp:273` |
| `am_textured` | Bool | false | Likely controls am textured. | `/workspace/src/am_map.cpp:118` |
| `am_thingcolor` | Color | 0xfcfcfc | Likely controls am thingcolor. | `/workspace/src/am_map.cpp:278` |
| `am_thingcolor_citem` | Color | 0xfcfcfc | Likely controls am thingcolor citem. | `/workspace/src/am_map.cpp:291` |
| `am_thingcolor_friend` | Color | 0xfcfcfc | Likely controls am thingcolor friend. | `/workspace/src/am_map.cpp:287` |
| `am_thingcolor_item` | Color | 0xfcfcfc | Likely controls am thingcolor item. | `/workspace/src/am_map.cpp:290` |
| `am_thingcolor_monster` | Color | 0xfcfcfc | Likely controls am thingcolor monster. | `/workspace/src/am_map.cpp:288` |
| `am_thingcolor_ncmonster` | Color | 0xfcfcfc | Likely controls am thingcolor ncmonster. | `/workspace/src/am_map.cpp:289` |
| `am_thingrenderstyles` | Bool | true | Likely controls am thingrenderstyles. | `/workspace/src/am_map.cpp:122` |
| `am_tswallcolor` | Color | 0x888888 | Likely controls am tswallcolor. | `/workspace/src/am_map.cpp:274` |
| `am_unexploredsecretcolor` | Color | 0xff00ff | Likely controls am unexploredsecretcolor. | `/workspace/src/am_map.cpp:286` |
| `am_wallcolor` | Color | 0x2c1808 | Likely controls am wallcolor. | `/workspace/src/am_map.cpp:271` |
| `am_xhaircolor` | Color | 0x808080 | Likely controls am xhaircolor. | `/workspace/src/am_map.cpp:280` |
| `am_yourcolor` | Color | 0xfce8d8 | Likely controls am yourcolor. | `/workspace/src/am_map.cpp:270` |
| `am_zoomdir` | Float | 0.f | Likely controls am zoomdir. | `/workspace/src/am_map.cpp:194` |

### Category: Input {#category-input}

Keyboard, mouse, gamepad, and gyro controls.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `in_mouse` | Int | 0 | Likely controls in mouse. | `/workspace/src/common/platform/win32/i_mouse.cpp:157` |
| `joy_axespolling` | Bool | true | Likely controls joy axespolling. | `/workspace/src/common/platform/posix/cocoa/i_joystick.cpp:1356` |
| `joy_dinput` | Bool | true | Likely controls joy dinput. | `/workspace/src/common/platform/win32/i_dijoy.cpp:285` |
| `joy_gyro_deadzone` | Float | 0.05f | Likely controls joy gyro deadzone. | `/workspace/src/i_input_gyro.cpp:162` |
| `joy_gyro_enable` | Bool | false | Likely controls joy gyro enable. | `/workspace/src/i_input_gyro.cpp:151` |
| `joy_gyro_invertpitch` | Bool | false | Likely controls joy gyro invertpitch. | `/workspace/src/i_input_gyro.cpp:171` |
| `joy_gyro_invertyaw` | Bool | false | Likely controls joy gyro invertyaw. | `/workspace/src/i_input_gyro.cpp:170` |
| `joy_gyro_mode` | Int | 0 | Likely controls joy gyro mode. | `/workspace/src/i_input_gyro.cpp:167` |
| `joy_gyro_pitchscale` | Float | 2.0f | Likely controls joy gyro pitchscale. | `/workspace/src/i_input_gyro.cpp:158` |
| `joy_gyro_yawscale` | Float | 2.5f | Likely controls joy gyro yawscale. | `/workspace/src/i_input_gyro.cpp:157` |
| `joy_ps2raw` | Bool | true | Likely controls joy ps2raw. | `/workspace/src/common/platform/win32/i_rawps2.cpp:234` |
| `joy_xinput` | Bool | true | Likely controls joy xinput. | `/workspace/src/common/platform/win32/i_xinput.cpp:203` |

### Category: Menu & UI {#category-menu}

Menu appearance, save-game UI, and interface preferences.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `m_blockcontrollers` | Bool | false | Likely controls m blockcontrollers. | `/workspace/src/common/menu/menu.cpp:55` |
| `m_cleanscale` | Bool | true | Likely controls m cleanscale. | `/workspace/src/common/menu/menu.cpp:60` |
| `m_forward` | Float | 1.f | Likely controls m forward. | `/workspace/src/g_game.cpp:326` |
| `m_hidepointer` | Bool | true | Likely controls m hidepointer. | `/workspace/src/common/platform/win32/i_mouse.cpp:155` |
| `m_pitch` | Float | 1.f | Likely controls m pitch. | `/workspace/src/d_main.cpp:619` |
| `m_quickexit` | Bool | false | Likely controls m quickexit. | `/workspace/src/common/menu/messagebox.cpp:34` |
| `m_sensitivity_x` | Float | 2.f | Likely controls m sensitivity x. | `/workspace/src/common/engine/d_event.cpp:45` |
| `m_sensitivity_y` | Float | 2.f | Likely controls m sensitivity y. | `/workspace/src/common/engine/d_event.cpp:46` |
| `m_show_backbutton` | Int | 0 | Likely controls m show backbutton. | `/workspace/src/common/menu/menu.cpp:59` |
| `m_showinputgrid` | Int | 0 | Likely controls m showinputgrid. | `/workspace/src/common/menu/menu.cpp:54` |
| `m_side` | Float | 2.f | Likely controls m side. | `/workspace/src/g_game.cpp:327` |
| `m_simpleoptions` | Bool | false | Likely controls m simpleoptions. | `/workspace/src/menu/doommenu.cpp:96` |
| `m_simpleoptions_view` | Bool | true | Likely controls m simpleoptions view. | `/workspace/src/menu/doommenu.cpp:97` |
| `m_smooth_curve` | Int | 0 | Likely controls m smooth curve. | `/workspace/src/i_input_feel.cpp:20` |
| `m_swapbuttons` | Bool | false | Likely controls m swapbuttons. | `/workspace/src/common/platform/win32/i_mouse.cpp:391` |
| `m_tooltip_alpha` | Float | 0.6f | Likely controls m tooltip alpha. | `/workspace/src/common/menu/menu.cpp:85` |
| `m_tooltip_capratio` | Float | 4.0/3.0 | Likely controls m tooltip capratio. | `/workspace/src/common/menu/menu.cpp:64` |
| `m_tooltip_delay` | Float | 9.0f | Likely controls m tooltip delay. | `/workspace/src/common/menu/menu.cpp:75` |
| `m_tooltip_lines` | Int | 3 | Likely controls m tooltip lines. | `/workspace/src/common/menu/menu.cpp:70` |
| `m_tooltip_small` | Bool | true | Likely controls m tooltip small. | `/workspace/src/common/menu/menu.cpp:69` |
| `m_tooltip_speed` | Float | 3.0f | Likely controls m tooltip speed. | `/workspace/src/common/menu/menu.cpp:80` |
| `m_use_mouse` | Int | 1 | Likely controls m use mouse. | `/workspace/src/common/menu/menu.cpp:58` |
| `m_yaw` | Float | 1.f | Likely controls m yaw. | `/workspace/src/d_main.cpp:620` |
| `menu_overscroll` | Int | 8 | Number of lines you can scroll past the bottom of a menu | `/workspace/src/common/menu/optionmenu.cpp:32` |
| `menu_resolution_custom_height` | Int | 480 | Likely controls menu resolution custom height. | `/workspace/src/common/menu/resolutionmenu.cpp:31` |
| `menu_resolution_custom_width` | Int | 640 | Likely controls menu resolution custom width. | `/workspace/src/common/menu/resolutionmenu.cpp:30` |

### Category: Debug & Development {#category-debug}

Developer diagnostics, tracing, profiling, and cheat toggles.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `debug_languages` | Bool | false | Likely controls debug languages. | `/workspace/src/common/engine/stringtable.cpp:39` |
| `debuganimated` | Bool | false | Likely controls debuganimated. | `/workspace/src/gamedata/textures/animations.cpp:189` |
| `debugtrace_capacity` | Int | 16384 | Likely controls debugtrace capacity. | `/workspace/src/common/engine/debugtrace.cpp:47` |
| `debugtrace_enable` | Bool | true | Likely controls debugtrace enable. | `/workspace/src/common/engine/debugtrace.cpp:43` |
| `debugtrace_filter` | String | "" | Likely controls debugtrace filter. | `/workspace/src/common/engine/debugtrace.cpp:44` |
| `debugtrace_minseverity` | Int | 0 | Likely controls debugtrace minseverity. | `/workspace/src/common/engine/debugtrace.cpp:45` |
| `debugtrace_stats` | Bool | true | Likely controls debugtrace stats. | `/workspace/src/common/engine/debugtrace.cpp:46` |
| `debugtrace_stream` | Bool | true | Likely controls debugtrace stream. | `/workspace/src/common/engine/debugtrace.cpp:48` |
| `debugtrace_stream_rotate_count` | Int | 4 | Likely controls debugtrace stream rotate count. | `/workspace/src/common/engine/debugtrace.cpp:50` |
| `debugtrace_stream_rotate_mb` | Int | 10 | Likely controls debugtrace stream rotate mb. | `/workspace/src/common/engine/debugtrace.cpp:49` |
| `vm_debug` | Bool | false | Likely controls vm debug. | `/workspace/src/d_main.cpp:3333` |
| `vm_debug_port` | Int | 19021 | Likely controls vm debug port. | `/workspace/src/d_main.cpp:3349` |
| `vm_jit` | Bool | false | Likely controls vm jit. | `/workspace/src/common/scripting/vm/vmframe.cpp:41` |
| `vm_jit_aot` | Bool | true | Likely controls vm jit aot. | `/workspace/src/common/scripting/vm/vmframe.cpp:49` |
| `vm_warnthinkercreation` | Bool | false | Likely controls vm warnthinkercreation. | `/workspace/src/scripting/backend/codegen_doom.cpp:991` |

### Category: Other {#category-misc}

CVARs that do not match a more specific category rule.

| CVAR | Type | Default | Description | Source |
| --- | --- | --- | --- | --- |
| `addrocketexplosion` | Bool | true | Likely controls addrocketexplosion. | `/workspace/src/playsim/p_mobj.cpp:135` |
| `adl_auto_arpeggio` | Bool | false | Likely controls adl auto arpeggio. | `/workspace/src/common/audio/music/music_config.cpp:107` |
| `adl_bank` | Int | 14 | Likely controls adl bank. | `/workspace/src/common/audio/music/music_config.cpp:77` |
| `adl_chan_alloc` | Int | 0 /*ADLMIDI_ChanAlloc_AUTO*/ | Likely controls adl chan alloc. | `/workspace/src/common/audio/music/music_config.cpp:102` |
| `adl_chips_count` | Int | 6 | Likely controls adl chips count. | `/workspace/src/common/audio/music/music_config.cpp:57` |
| `adl_custom_bank` | String | "" | Likely controls adl custom bank. | `/workspace/src/common/audio/music/music_config.cpp:92` |
| `adl_emulator_id` | Int | 0 | Likely controls adl emulator id. | `/workspace/src/common/audio/music/music_config.cpp:62` |
| `adl_fullpan` | Bool | true | Likely controls adl fullpan. | `/workspace/src/common/audio/music/music_config.cpp:72` |
| `adl_gain` | Float | 1.0 | Likely controls adl gain. | `/workspace/src/common/audio/music/music_config.cpp:112` |
| `adl_run_at_pcm_rate` | Bool | false | Likely controls adl run at pcm rate. | `/workspace/src/common/audio/music/music_config.cpp:67` |
| `adl_use_custom_bank` | Bool | false | Likely controls adl use custom bank. | `/workspace/src/common/audio/music/music_config.cpp:82` |
| `adl_use_genmidi` | Bool | false | Likely controls adl use genmidi. | `/workspace/src/common/audio/music/music_config.cpp:87` |
| `adl_volume_model` | Int | 0 /*ADLMIDI_VolumeModel_AUTO*/ | Likely controls adl volume model. | `/workspace/src/common/audio/music/music_config.cpp:97` |
| `aimdebug` | Bool | false | Likely controls aimdebug. | `/workspace/src/playsim/p_map.cpp:3895` |
| `allcheats` | Bool | false | Likely controls allcheats. | `/workspace/src/st_stuff.cpp:296` |
| `allowsingleplayerscripts` | Bool | true | Likely controls allowsingleplayerscripts. | `/workspace/src/playsim/p_acs.cpp:10957` |
| `alwaysapplydmflags` | Bool | false | Server setting: Apply DM Flags Always | `/workspace/src/g_cvars.cpp:140` |
| `anonstats_enabled411` | Int | -1 | Likely controls anonstats enabled411. | `/workspace/src/d_anonstats.cpp:58` |
| `anonstats_host` | String | "gzstats.drdteam.org" | Likely controls anonstats host. | `/workspace/src/d_anonstats.cpp:59` |
| `anonstats_port` | Int | 80 | Likely controls anonstats port. | `/workspace/src/d_anonstats.cpp:60` |
| `autoaim` | Float | 35.f | Likely controls autoaim. | `/workspace/src/d_netinfo.cpp:47` |
| `autoloadbrightmaps` | Bool | true | Likely controls autoloadbrightmaps. | `/workspace/src/d_main.cpp:511` |
| `autoloadlights` | Bool | true | Likely controls autoloadlights. | `/workspace/src/d_main.cpp:523` |
| `autoloadwidescreen` | Bool | true | Likely controls autoloadwidescreen. | `/workspace/src/d_main.cpp:524` |
| `autosavecount` | Int | 4 | Likely controls autosavecount. | `/workspace/src/g_game.cpp:417` |
| `autosavenum` | Int | 0 | Likely controls autosavenum. | `/workspace/src/g_game.cpp:413` |
| `blood_fade_scalar` | Float | 1.0f | Likely controls blood fade scalar. | `/workspace/src/rendering/2d/v_blend.cpp:56` |
| `bot_allowspy` | Bool | false | Likely controls bot allowspy. | `/workspace/src/g_game.cpp:411` |
| `bot_next_color` | Int | 11 | Likely controls bot next color. | `/workspace/src/playsim/bots/b_bot.cpp:144` |
| `bottomskew` | Int | 0 | Likely controls bottomskew. | `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:2131` |
| `chase_dist` | Float | 90.f | Likely controls chase dist. | `/workspace/src/playsim/p_map.cpp:5684` |
| `chase_height` | Float | -8.f | Likely controls chase height. | `/workspace/src/playsim/p_map.cpp:5683` |
| `chasedemo` | Bool | false | Likely controls chasedemo. | `/workspace/src/g_game.cpp:291` |
| `chat_substitution` | Bool | false | Likely controls chat substitution. | `/workspace/src/ct_chat.cpp:111` |
| `chatmacro0` | String | "No" | Likely controls chatmacro0. | `/workspace/src/ct_chat.cpp:95` |
| `chatmacro1` | String | "I'm ready to kick butt!" | Likely controls chatmacro1. | `/workspace/src/ct_chat.cpp:86` |
| `chatmacro2` | String | "I'm OK." | Likely controls chatmacro2. | `/workspace/src/ct_chat.cpp:87` |
| `chatmacro3` | String | "I'm not looking too good!" | Likely controls chatmacro3. | `/workspace/src/ct_chat.cpp:88` |
| `chatmacro4` | String | "Help!" | Likely controls chatmacro4. | `/workspace/src/ct_chat.cpp:89` |
| `chatmacro5` | String | "You suck!" | Likely controls chatmacro5. | `/workspace/src/ct_chat.cpp:90` |
| `chatmacro6` | String | "Next time, scumbag..." | Likely controls chatmacro6. | `/workspace/src/ct_chat.cpp:91` |
| `chatmacro7` | String | "Come here!" | Likely controls chatmacro7. | `/workspace/src/ct_chat.cpp:92` |
| `chatmacro8` | String | "I'll take care of it." | Likely controls chatmacro8. | `/workspace/src/ct_chat.cpp:93` |
| `chatmacro9` | String | "Yes" | Likely controls chatmacro9. | `/workspace/src/ct_chat.cpp:94` |
| `classic_scaling_factor` | Float | 1.0 | Likely controls classic scaling factor. | `/workspace/src/common/2d/v_2ddrawer.cpp:39` |
| `classic_scaling_pixelaspect` | Float | 1.2f | Likely controls classic scaling pixelaspect. | `/workspace/src/common/2d/v_2ddrawer.cpp:40` |
| `classicflight` | Bool | false | Likely controls classicflight. | `/workspace/src/d_netinfo.cpp:61` |
| `color` | Color | 0x40cf00 | Likely controls color. | `/workspace/src/d_netinfo.cpp:49` |
| `colorset` | Int | 0 | Likely controls colorset. | `/workspace/src/d_netinfo.cpp:50` |
| `con_4bitansi` | Bool | false | Likely controls con 4bitansi. | `/workspace/src/common/platform/posix/sdl/i_system.cpp:67` |
| `con_alpha` | Float | 0.75f | Likely controls con alpha. | `/workspace/src/common/console/c_console.cpp:133` |
| `con_buffersize` | Int | -1 | Likely controls con buffersize. | `/workspace/src/common/console/c_console.cpp:76` |
| `con_centernotify` | Bool | false | Likely controls con centernotify. | `/workspace/src/console/c_notifybuffer.cpp:51` |
| `con_ctrl_d` | String | "" | Likely controls con ctrl d. | `/workspace/src/common/console/c_console.cpp:150` |
| `con_debugoutput` | Bool | false | Likely controls con debugoutput. | `/workspace/src/common/platform/win32/i_system.cpp:114` |
| `con_midtime` | Float | 3.f | Likely controls con midtime. | `/workspace/src/g_statusbar/hudmessages.cpp:869` |
| `con_notablist` | Bool | false | Likely controls con notablist. | `/workspace/src/common/console/c_tabcomplete.cpp:63` |
| `con_notifylines` | Int | 4 | Likely controls con notifylines. | `/workspace/src/console/c_notifybuffer.cpp:62` |
| `con_notifytime` | Float | 3.f | Likely controls con notifytime. | `/workspace/src/console/c_notifybuffer.cpp:50` |
| `con_printansi` | Bool | true | Likely controls con printansi. | `/workspace/src/common/platform/posix/sdl/i_system.cpp:66` |
| `con_pulsetext` | Bool | false | Likely controls con pulsetext. | `/workspace/src/console/c_notifybuffer.cpp:52` |
| `con_quick_home_end` | Bool | true | Use HOME/END keys to scroll when cursor is at start/end of line already | `/workspace/src/common/console/c_console.cpp:139` |
| `con_scale` | Int | 0 | Likely controls con scale. | `/workspace/src/common/console/c_console.cpp:128` |
| `con_scaletext` | Int | 0 | Likely controls con scaletext. | `/workspace/src/console/c_notifybuffer.cpp:54` |
| `con_stackident` | Bool | true | Likely controls con stackident. | `/workspace/src/console/c_cmds.cpp:63` |
| `consoleendoom` | Bool | true | Likely controls consoleendoom. | `/workspace/src/common/startscreen/endoom.cpp:64` |
| `defaultaddonfiles` | String | "" | Likely controls defaultaddonfiles. | `/workspace/src/common/engine/i_interface.cpp:58` |
| `defaultargs` | String | "" | Likely controls defaultargs. | `/workspace/src/common/engine/i_interface.cpp:59` |
| `defaultiwad` | String | "" | Likely controls defaultiwad. | `/workspace/src/common/engine/i_interface.cpp:57` |
| `defaultnetaddress` | String | "" | Likely controls defaultnetaddress. | `/workspace/src/common/engine/i_interface.cpp:67` |
| `defaultnetaltdm` | Bool | false | Likely controls defaultnetaltdm. | `/workspace/src/common/engine/i_interface.cpp:66` |
| `defaultnetargs` | String | "" | Likely controls defaultnetargs. | `/workspace/src/common/engine/i_interface.cpp:61` |
| `defaultnetextratic` | Bool | false | Likely controls defaultnetextratic. | `/workspace/src/common/engine/i_interface.cpp:72` |
| `defaultnetgamemode` | Int | 0 | Likely controls defaultnetgamemode. | `/workspace/src/common/engine/i_interface.cpp:65` |
| `defaultnethostport` | Int | 0 | Likely controls defaultnethostport. | `/workspace/src/common/engine/i_interface.cpp:63` |
| `defaultnethostteam` | Int | 255 | Likely controls defaultnethostteam. | `/workspace/src/common/engine/i_interface.cpp:70` |
| `defaultnetiwad` | String | "" | Likely controls defaultnetiwad. | `/workspace/src/common/engine/i_interface.cpp:60` |
| `defaultnetjoinport` | Int | 0 | Likely controls defaultnetjoinport. | `/workspace/src/common/engine/i_interface.cpp:68` |
| `defaultnetjointeam` | Int | 255 | Likely controls defaultnetjointeam. | `/workspace/src/common/engine/i_interface.cpp:71` |
| `defaultnetpage` | Int | 0 | Likely controls defaultnetpage. | `/workspace/src/common/engine/i_interface.cpp:69` |
| `defaultnetplayers` | Int | 8 | Likely controls defaultnetplayers. | `/workspace/src/common/engine/i_interface.cpp:62` |
| `defaultnetsavefile` | String | "" | Likely controls defaultnetsavefile. | `/workspace/src/common/engine/i_interface.cpp:73` |
| `defaultnetticdup` | Int | 0 | Likely controls defaultnetticdup. | `/workspace/src/common/engine/i_interface.cpp:64` |
| `dehload` | Int | 0 | Likely controls dehload. | `/workspace/src/gamedata/d_dehacked.cpp:3210` |
| `demo_compress` | Bool | true | Likely controls demo compress. | `/workspace/src/g_game.cpp:321` |
| `developer` | Int | 0 | Likely controls developer. | `/workspace/src/common/console/c_console.cpp:143` |
| `dimamount` | Float | 0.8f | Likely controls dimamount. | `/workspace/src/menu/doommenu.cpp:376` |
| `dimcolor` | Color | 0x000000 | Likely controls dimcolor. | `/workspace/src/menu/doommenu.cpp:387` |
| `disableautoload` | Bool | false | Likely controls disableautoload. | `/workspace/src/d_main.cpp:510` |
| `disableautosave` | Int | 0 | Likely controls disableautosave. | `/workspace/src/g_game.cpp:414` |
| `disablecrashlog` | Bool | false | Likely controls disablecrashlog. | `/workspace/src/common/platform/win32/i_main.cpp:626` |
| `displaynametags` | Int | 0 | Likely controls displaynametags. | `/workspace/src/g_game.cpp:312` |
| `dlg_musicvolume` | Float | 1.0f | Likely controls dlg musicvolume. | `/workspace/src/p_conversation.cpp:218` |
| `dlg_vgafont` | Bool | false | Likely controls dlg vgafont. | `/workspace/src/p_conversation.cpp:67` |
| `dmflags` | Int | 0 | Server setting: Raw DM Flags | `/workspace/src/d_main.cpp:657` |
| `dmflags2` | Int | 0 | Server setting: Raw DM Flags 2 | `/workspace/src/d_main.cpp:733` |
| `dmflags3` | Int | 0 | Server setting: Raw DM Flags 3 | `/workspace/src/d_main.cpp:803` |
| `dumpspawnedthings` | Bool | false | Likely controls dumpspawnedthings. | `/workspace/src/playsim/p_mobj.cpp:7074` |
| `eaxedit_test` | Bool | false | Likely controls eaxedit test. | `/workspace/src/common/audio/sound/s_reverbedit.cpp:57` |
| `enablescriptscreenshot` | Bool | false | Likely controls enablescriptscreenshot. | `/workspace/src/g_game.cpp:295` |
| `fluid_chorus` | Bool | false | Likely controls fluid chorus. | `/workspace/src/common/audio/music/music_config.cpp:161` |
| `fluid_chorus_depth` | Float | 8.f | Likely controls fluid chorus depth. | `/workspace/src/common/audio/music/music_config.cpp:222` |
| `fluid_chorus_level` | Float | 1.2f | Likely controls fluid chorus level. | `/workspace/src/common/audio/music/music_config.cpp:211` |
| `fluid_chorus_speed` | Float | 0.3f | Likely controls fluid chorus speed. | `/workspace/src/common/audio/music/music_config.cpp:216` |
| `fluid_chorus_type` | Int | 0/*FLUID_CHORUS_DEFAULT_TYPE*/ | Likely controls fluid chorus type. | `/workspace/src/common/audio/music/music_config.cpp:227` |
| `fluid_chorus_voices` | Int | 3 | Likely controls fluid chorus voices. | `/workspace/src/common/audio/music/music_config.cpp:206` |
| `fluid_gain` | Float | 0.5 | Likely controls fluid gain. | `/workspace/src/common/audio/music/music_config.cpp:142` |
| `fluid_interp` | Int | 1 | Likely controls fluid interp. | `/workspace/src/common/audio/music/music_config.cpp:171` |
| `fluid_lib` | String | "" | Likely controls fluid lib. | `/workspace/src/common/audio/music/music_config.cpp:132` |
| `fluid_patchset` | String | GAMENAMELOWERCASE | Likely controls fluid patchset. | `/workspace/src/common/audio/music/music_config.cpp:137` |
| `fluid_reverb` | Bool | false | Likely controls fluid reverb. | `/workspace/src/common/audio/music/music_config.cpp:156` |
| `fluid_reverb_damping` | Float | 0.23f | Likely controls fluid reverb damping. | `/workspace/src/common/audio/music/music_config.cpp:191` |
| `fluid_reverb_level` | Float | 0.57f | Likely controls fluid reverb level. | `/workspace/src/common/audio/music/music_config.cpp:201` |
| `fluid_reverb_roomsize` | Float | 0.61f | Likely controls fluid reverb roomsize. | `/workspace/src/common/audio/music/music_config.cpp:186` |
| `fluid_reverb_width` | Float | 0.76f | Likely controls fluid reverb width. | `/workspace/src/common/audio/music/music_config.cpp:196` |
| `fluid_samplerate` | Int | 0 | Likely controls fluid samplerate. | `/workspace/src/common/audio/music/music_config.cpp:176` |
| `fluid_threads` | Int | 1 | Likely controls fluid threads. | `/workspace/src/common/audio/music/music_config.cpp:181` |
| `fluid_voices` | Int | 128 | Likely controls fluid voices. | `/workspace/src/common/audio/music/music_config.cpp:166` |
| `forcewater` | Bool | false | Likely controls forcewater. | `/workspace/src/p_setup.cpp:765` |
| `fov` | Float | 90.f | Likely controls fov. | `/workspace/src/playsim/p_user.cpp:143` |
| `fraglimit` | Int | 0 | Server setting: Frag Limit | `/workspace/src/d_main.cpp:466` |
| `freelook` | Bool | true | Likely controls freelook. | `/workspace/src/g_game.cpp:324` |
| `fviewbob` | Bool | true | Likely controls fviewbob. | `/workspace/src/d_netinfo.cpp:56` |
| `gamma` | Float | GAMMA_DEFAULT | Likely controls gamma. | `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:102` |
| `genblockmap` | Bool | false | Likely controls genblockmap. | `/workspace/src/maploader/maploader.cpp:65` |
| `gender` | String | "neutral" | Likely controls gender. | `/workspace/src/d_netinfo.cpp:53` |
| `gennodes` | Bool | false | Likely controls gennodes. | `/workspace/src/maploader/maploader.cpp:66` |
| `gles_force_glsl_v100` | Bool | false | Likely controls gles force glsl v100. | `/workspace/src/common/rendering/gles/gles_system.cpp:24` |
| `gles_glsl_precision` | Int | 2 | Likely controls gles glsl precision. | `/workspace/src/common/rendering/gles/gles_shader.cpp:42` |
| `gles_max_lights_per_surface` | Int | 32 | Likely controls gles max lights per surface. | `/workspace/src/common/rendering/gles/gles_system.cpp:25` |
| `gles_use_mapped_buffer` | Bool | false | Likely controls gles use mapped buffer. | `/workspace/src/common/rendering/gles/gles_system.cpp:23` |
| `gme_stereodepth` | Float | 0.f | Likely controls gme stereodepth. | `/workspace/src/common/audio/music/music_config.cpp:502` |
| `gus_memsize` | Int | 0 | Likely controls gus memsize. | `/workspace/src/common/audio/music/music_config.cpp:361` |
| `gus_patchdir` | String | "" | Likely controls gus patchdir. | `/workspace/src/common/audio/music/music_config.cpp:351` |
| `haptics_compat` | Int | HAPTCOMPAT_MATCH | haptic feedback compatibility level | `/workspace/src/common/engine/m_haptics.cpp:140` |
| `haptics_debug` | Bool | false | print diagnostics for haptic feedback | `/workspace/src/common/engine/m_haptics.cpp:138` |
| `haptics_do_action` | Bool | true | allow haptic feedback for player doing things | `/workspace/src/common/engine/m_haptics.cpp:149` |
| `haptics_do_damage` | Bool | true | allow haptic feedback for things hurting player | `/workspace/src/common/engine/m_haptics.cpp:148` |
| `haptics_do_menus` | Bool | true | allow haptic feedback for menus | `/workspace/src/common/engine/m_haptics.cpp:146` |
| `haptics_do_world` | Bool | true | allow haptic feedback for things acting on player | `/workspace/src/common/engine/m_haptics.cpp:147` |
| `haptics_strength` | Int | 10 | Translate linear haptics to audio taper | `/workspace/src/common/engine/m_haptics.cpp:119` |
| `haptics_strength_hf` | Float | 1.0 | high frequency motor fine-control | `/workspace/src/common/engine/m_haptics.cpp:103` |
| `haptics_strength_lf` | Float | 1.0 | low frequency motor fine-control | `/workspace/src/common/engine/m_haptics.cpp:98` |
| `haptics_strength_lt` | Float | 1.0 | left trigger motor fine-control | `/workspace/src/common/engine/m_haptics.cpp:108` |
| `haptics_strength_rt` | Float | 1.0 | right trigger motor fine-control | `/workspace/src/common/engine/m_haptics.cpp:113` |
| `hudcolor_ltim` | Int | CR_ORANGE | Likely controls hudcolor ltim. | `/workspace/src/g_statusbar/shared_hud.cpp:80` |
| `hudcolor_statnames` | Int | CR_RED | Likely controls hudcolor statnames. | `/workspace/src/g_statusbar/shared_hud.cpp:84` |
| `hudcolor_stats` | Int | CR_GREEN | Likely controls hudcolor stats. | `/workspace/src/g_statusbar/shared_hud.cpp:85` |
| `hudcolor_time` | Int | CR_RED | Likely controls hudcolor time. | `/workspace/src/g_statusbar/shared_hud.cpp:79` |
| `hudcolor_titl` | Int | CR_YELLOW | Likely controls hudcolor titl. | `/workspace/src/g_statusbar/shared_hud.cpp:78` |
| `hudcolor_ttim` | Int | CR_GOLD | Likely controls hudcolor ttim. | `/workspace/src/g_statusbar/shared_hud.cpp:81` |
| `hudcolor_xyco` | Int | CR_GREEN | Likely controls hudcolor xyco. | `/workspace/src/g_statusbar/shared_hud.cpp:82` |
| `i_discordrpc` | Bool | false | Likely controls i discordrpc. | `/workspace/src/d_main.cpp:529` |
| `i_display_new_release` | Int | 1 | Show changelog upon update | `/workspace/src/d_iwad.cpp:60` |
| `i_exit_on_not_found` | Int | REQUIRE_DEFAULT | Exits game if a specified file is not found | `/workspace/src/common/utility/findfile.cpp:38` |
| `I_FriendlyWindowTitle` | Int | 1 | Likely controls I FriendlyWindowTitle. | `/workspace/src/d_main.cpp:535` |
| `i_is_new_release` | Bool | true | Likely controls i is new release. | `/workspace/src/d_iwad.cpp:56` |
| `i_loadsupportwad` | Bool | true | Load id24.wad | `/workspace/src/d_iwad.cpp:54` |
| `i_pauseinbackground` | Bool | true | Likely controls i pauseinbackground. | `/workspace/src/common/audio/sound/s_sound.cpp:41` |
| `i_searchdistributors` | Bool | true | Search storefront intallations for IWADS | `/workspace/src/d_iwad.cpp:58` |
| `i_soundinbackground` | Bool | false | Likely controls i soundinbackground. | `/workspace/src/common/audio/sound/s_sound.cpp:40` |
| `i_timescale` | Float | 1.0f | Likely controls i timescale. | `/workspace/src/d_main.cpp:422` |
| `idmypos` | Bool | false | Likely controls idmypos. | `/workspace/src/g_statusbar/shared_sbar.cpp:126` |
| `infighting` | Int | 0 | Likely controls infighting. | `/workspace/src/gamedata/d_dehacked.cpp:63` |
| `inter_classic_scaling` | Bool | true | Likely controls inter classic scaling. | `/workspace/src/intermission/intermission.cpp:74` |
| `inter_subtitles` | Bool | true | Likely controls inter subtitles. | `/workspace/src/common/cutscenes/screenjob.cpp:43` |
| `invertmouse` | Bool | false | Likely controls invertmouse. | `/workspace/src/common/engine/d_event.cpp:47` |
| `invertmousex` | Bool | false | Likely controls invertmousex. | `/workspace/src/common/engine/d_event.cpp:48` |
| `joykey_stop_conflict` | Int | -1 | Detect joypad/keyboard conflicts, dropping events as needed. Useful for handheld PCs such as the SteamDeck. -1: auto-detect, 0: disabled, 1: detected, 2: forced | `/workspace/src/common/platform/posix/sdl/i_input.cpp:51` |
| `k_allowfullscreentoggle` | Bool | true | Likely controls k allowfullscreentoggle. | `/workspace/src/common/platform/win32/i_input.cpp:120` |
| `k_mergekeys` | Bool | true | Likely controls k mergekeys. | `/workspace/src/common/platform/win32/i_keyboard.cpp:113` |
| `language` | String | "auto" | Likely controls language. | `/workspace/src/common/engine/i_interface.cpp:81` |
| `language_debug_maxlen` | Int | 64 | Likely controls language debug maxlen. | `/workspace/src/common/engine/stringtable.cpp:40` |
| `log_vgafont` | Bool | false | Likely controls log vgafont. | `/workspace/src/g_statusbar/shared_sbar.cpp:84` |
| `longsavemessages` | Bool | false | Likely controls longsavemessages. | `/workspace/src/g_game.cpp:293` |
| `lookspring` | Bool | true | Likely controls lookspring. | `/workspace/src/d_main.cpp:3318` |
| `lookstrafe` | Bool | false | Likely controls lookstrafe. | `/workspace/src/g_game.cpp:325` |
| `map_point_coordinates` | Bool | true | Likely controls map point coordinates. | `/workspace/src/g_statusbar/shared_hud.cpp:88` |
| `maxviewpitch` | Float | 90.f | Likely controls maxviewpitch. | `/workspace/src/rendering/r_utility.cpp:1279` |
| `midi_config` | String | "" | Likely controls midi config. | `/workspace/src/common/audio/music/music_config.cpp:341` |
| `midi_dmxgus` | Bool | false | Likely controls midi dmxgus. | `/workspace/src/common/audio/music/music_config.cpp:346` |
| `midi_voices` | Int | 32 | Likely controls midi voices. | `/workspace/src/common/audio/music/music_config.cpp:356` |
| `midskew` | Int | 0 | Likely controls midskew. | `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:2130` |
| `mod_autochip` | Bool | false | Likely controls mod autochip. | `/workspace/src/common/audio/music/music_config.cpp:544` |
| `mod_autochip_scan_threshold` | Int | 12 | Likely controls mod autochip scan threshold. | `/workspace/src/common/audio/music/music_config.cpp:559` |
| `mod_autochip_size_force` | Int | 100 | Likely controls mod autochip size force. | `/workspace/src/common/audio/music/music_config.cpp:549` |
| `mod_autochip_size_scan` | Int | 500 | Likely controls mod autochip size scan. | `/workspace/src/common/audio/music/music_config.cpp:554` |
| `mod_dumb_mastervolume` | Float | 1.f | Likely controls mod dumb mastervolume. | `/workspace/src/common/audio/music/music_config.cpp:564` |
| `mod_interp` | Int | 2/*DUMB_LQ_CUBIC*/ | Likely controls mod interp. | `/workspace/src/common/audio/music/music_config.cpp:539` |
| `mod_preferred_player` | Int | 0 | Likely controls mod preferred player. | `/workspace/src/common/audio/music/music.cpp:87` |
| `mod_samplerate` | Int | 0 | Likely controls mod samplerate. | `/workspace/src/common/audio/music/music_config.cpp:524` |
| `mod_volramp` | Int | 2 | Likely controls mod volramp. | `/workspace/src/common/audio/music/music_config.cpp:534` |
| `mouse_capturemode` | Int | 1 | Likely controls mouse capturemode. | `/workspace/src/d_main.cpp:3321` |
| `movebob` | Float | 0.25f | Likely controls movebob. | `/workspace/src/d_netinfo.cpp:55` |
| `msg` | Int | 0 | Filters HUD message by importance | `/workspace/src/common/console/c_console.cpp:182` |
| `msg0color` | Int | CR_UNTRANSLATED | Likely controls msg0color. | `/workspace/src/common/console/c_console.cpp:184` |
| `msg1color` | Int | CR_GOLD | Likely controls msg1color. | `/workspace/src/common/console/c_console.cpp:189` |
| `msg2color` | Int | CR_GRAY | Likely controls msg2color. | `/workspace/src/common/console/c_console.cpp:194` |
| `msg3color` | Int | CR_GREEN | Likely controls msg3color. | `/workspace/src/common/console/c_console.cpp:199` |
| `msg4color` | Int | CR_GREEN | Likely controls msg4color. | `/workspace/src/common/console/c_console.cpp:204` |
| `msgmidcolor` | Int | CR_UNTRANSLATED | Likely controls msgmidcolor. | `/workspace/src/common/console/c_console.cpp:209` |
| `msgmidcolor2` | Int | CR_BROWN | Likely controls msgmidcolor2. | `/workspace/src/common/console/c_console.cpp:214` |
| `name` | String | "Player" | descr | `/workspace/src/d_netinfo.cpp:48` |
| `nametagcolor` | Int | CR_GOLD | Likely controls nametagcolor. | `/workspace/src/g_game.cpp:320` |
| `net_adaptive_lead` | Bool | true | Likely controls adaptive lead behavior for network. | `/workspace/src/d_net_movement_diag.cpp:44` |
| `net_adaptive_lead_guard` | Int | 3 | Likely controls adaptive lead guard behavior for network. | `/workspace/src/d_net_movement_diag.cpp:64` |
| `net_adaptive_lead_max` | Int | 6 | Likely controls adaptive lead max behavior for network. | `/workspace/src/d_net_movement_diag.cpp:56` |
| `net_adaptive_lead_min` | Int | 1 | Likely controls adaptive lead min behavior for network. | `/workspace/src/d_net_movement_diag.cpp:48` |
| `net_chatslowmode` | Int | 0 | Server setting: Chat Slowmode | `/workspace/src/ct_chat.cpp:85` |
| `net_cutscenecountdown` | Float | 30.0f | Server setting: Ready Time | `/workspace/src/d_net_invasion.inl:67` |
| `net_cutscenereadypercent` | Float | 0.5f | Server setting: Ready Percent | `/workspace/src/d_net_invasion.inl:60` |
| `net_cutscenereadytype` | Int | RT_VOTE | Server setting: Ready Mode | `/workspace/src/d_net_invasion.inl:53` |
| `net_desyncdebug` | Bool | true | Likely controls desyncdebug behavior for network. | `/workspace/src/d_net_invasion.inl:214` |
| `net_disablepause` | Int | 0 | Server setting: Pause Policy | `/workspace/src/d_net_invasion.inl:46` |
| `net_echo_debug` | Int | 1 | Likely controls echo debug behavior for network. | `/workspace/src/d_net.cpp:226` |
| `net_event_debug` | Int | 1 | Likely controls event debug behavior for network. | `/workspace/src/d_net_diagnostics.cpp:51` |
| `net_extratic` | Bool | false | Likely controls extratic behavior for network. | `/workspace/src/d_net_invasion.inl:42` |
| `net_limitconversations` | Bool | false | Likely controls limitconversations behavior for network. | `/workspace/src/d_net_invasion.inl:45` |
| `net_limitsaves` | Bool | true | Likely controls limitsaves behavior for network. | `/workspace/src/d_net_invasion.inl:43` |
| `net_password` | String | "" | Likely controls password behavior for network. | `/workspace/src/common/engine/i_net.cpp:1029` |
| `net_reconcile_debug` | Int | 1 | Likely controls reconcile debug behavior for network. | `/workspace/src/d_net.cpp:235` |
| `net_repeatableactioncooldown` | Bool | true | Likely controls repeatableactioncooldown behavior for network. | `/workspace/src/d_net_invasion.inl:44` |
| `net_self_test_run_client` | Int | 0 | Likely controls self test run client behavior for network. | `/workspace/src/d_net.cpp:242` |
| `net_ticbalance` | Bool | true | Likely controls ticbalance behavior for network. | `/workspace/src/d_net_invasion.inl:41` |
| `neverswitchonpickup` | Bool | false | Likely controls neverswitchonpickup. | `/workspace/src/d_netinfo.cpp:54` |
| `nocheats` | Bool | false | Likely controls nocheats. | `/workspace/src/st_stuff.cpp:297` |
| `nointerscrollabort` | Bool | false | Likely controls nointerscrollabort. | `/workspace/src/intermission/intermission.cpp:72` |
| `nomonsterinterpolation` | Bool | false | Likely controls nomonsterinterpolation. | `/workspace/src/playsim/p_enemy.cpp:76` |
| `opl_core` | Int | 0 | Likely controls opl core. | `/workspace/src/common/audio/music/music_config.cpp:244` |
| `opl_fullpan` | Bool | true | Likely controls opl fullpan. | `/workspace/src/common/audio/music/music_config.cpp:249` |
| `opl_gain` | Float | 1.0 | Likely controls opl gain. | `/workspace/src/common/audio/music/music_config.cpp:254` |
| `opl_numchips` | Int | 2 | Likely controls opl numchips. | `/workspace/src/common/audio/music/music_config.cpp:239` |
| `opn_auto_arpeggio` | Bool | false | Likely controls opn auto arpeggio. | `/workspace/src/common/audio/music/music_config.cpp:316` |
| `opn_chan_alloc` | Int | -1 /*OPNMIDI_ChanAlloc_AUTO*/ | Likely controls opn chan alloc. | `/workspace/src/common/audio/music/music_config.cpp:311` |
| `opn_chips_count` | Int | 8 | Likely controls opn chips count. | `/workspace/src/common/audio/music/music_config.cpp:276` |
| `opn_custom_bank` | String | "" | Likely controls opn custom bank. | `/workspace/src/common/audio/music/music_config.cpp:301` |
| `opn_emulator_id` | Int | 0 | Likely controls opn emulator id. | `/workspace/src/common/audio/music/music_config.cpp:281` |
| `opn_fullpan` | Bool | true | Likely controls opn fullpan. | `/workspace/src/common/audio/music/music_config.cpp:291` |
| `opn_gain` | Float | 1.0 | Likely controls opn gain. | `/workspace/src/common/audio/music/music_config.cpp:321` |
| `opn_run_at_pcm_rate` | Bool | false | Likely controls opn run at pcm rate. | `/workspace/src/common/audio/music/music_config.cpp:286` |
| `opn_use_custom_bank` | Bool | false | Likely controls opn use custom bank. | `/workspace/src/common/audio/music/music_config.cpp:296` |
| `opn_volume_model` | Int | 0 /*OPNMIDI_VolumeModel_AUTO*/ | Likely controls opn volume model. | `/workspace/src/common/audio/music/music_config.cpp:306` |
| `os_isanyof` | Bool | true | Likely controls os isanyof. | `/workspace/src/common/menu/menu.cpp:62` |
| `paletteflash` | Int | 0 | Likely controls paletteflash. | `/workspace/src/g_statusbar/shared_sbar.cpp:93` |
| `pf_hazard` | Flag | paletteflash | Flag alias backed by paletteflash. | `/workspace/src/g_statusbar/shared_sbar.cpp:97` |
| `pf_hexenweaps` | Flag | paletteflash | Flag alias backed by paletteflash. | `/workspace/src/g_statusbar/shared_sbar.cpp:94` |
| `pf_ice` | Flag | paletteflash | Flag alias backed by paletteflash. | `/workspace/src/g_statusbar/shared_sbar.cpp:96` |
| `pf_poison` | Flag | paletteflash | Flag alias backed by paletteflash. | `/workspace/src/g_statusbar/shared_sbar.cpp:95` |
| `pickup_fade_scalar` | Float | 1.0f | Likely controls pickup fade scalar. | `/workspace/src/rendering/2d/v_blend.cpp:57` |
| `png_gamma` | Float | 0.f | Likely controls png gamma. | `/workspace/src/common/textures/m_png.cpp:107` |
| `png_level` | Int | 5 | Likely controls png level. | `/workspace/src/common/textures/m_png.cpp:100` |
| `powerup_fade_scalar` | Float | 1.0f | Likely controls powerup fade scalar. | `/workspace/src/rendering/2d/v_blend.cpp:58` |
| `queryiwad` | Bool | QUERYIWADDEFAULT | Likely controls queryiwad. | `/workspace/src/common/engine/i_interface.cpp:53` |
| `queryiwad_key` | String | "shift" | Likely controls queryiwad key. | `/workspace/src/common/platform/posix/sdl/i_system.cpp:65` |
| `quicksavenum` | Int | -1 | Likely controls quicksavenum. | `/workspace/src/g_game.cpp:423` |
| `quicksaverotation` | Bool | false | Likely controls quicksaverotation. | `/workspace/src/g_game.cpp:424` |
| `quicksaverotationcount` | Int | 4 | Likely controls quicksaverotationcount. | `/workspace/src/g_game.cpp:426` |
| `reverbedit_id1` | Int | 0 | Likely controls reverbedit id1. | `/workspace/src/common/audio/sound/s_reverbedit.cpp:52` |
| `reverbedit_id2` | Int | 0 | Likely controls reverbedit id2. | `/workspace/src/common/audio/sound/s_reverbedit.cpp:53` |
| `reverbedit_name` | String | "" | Likely controls reverbedit name. | `/workspace/src/common/audio/sound/s_reverbedit.cpp:51` |
| `reverbsavename` | String | "" | Likely controls reverbsavename. | `/workspace/src/common/audio/sound/s_reverbedit.cpp:54` |
| `saveargs` | Bool | true | Likely controls saveargs. | `/workspace/src/common/engine/i_interface.cpp:54` |
| `saved_drawplayersprite` | Bool | true | Likely controls saved drawplayersprite. | `/workspace/src/d_main.cpp:590` |
| `saved_screenblocks` | Int | 10 | Likely controls saved screenblocks. | `/workspace/src/d_main.cpp:589` |
| `saved_showmessages` | Bool | true | Likely controls saved showmessages. | `/workspace/src/d_main.cpp:591` |
| `saveloadconfirmation` | Bool | true | Likely controls saveloadconfirmation. | `/workspace/src/g_game.cpp:415` |
| `savenetargs` | Bool | true | Likely controls savenetargs. | `/workspace/src/common/engine/i_interface.cpp:56` |
| `savenetfile` | Bool | false | Likely controls savenetfile. | `/workspace/src/common/engine/i_interface.cpp:55` |
| `savestatistics` | Int | 0 | Likely controls savestatistics. | `/workspace/src/gamedata/statistics.cpp:50` |
| `sb_cooperative_enable` | Bool | true | Likely controls sb cooperative enable. | `/workspace/src/hu_scores.cpp:68` |
| `sb_cooperative_headingcolor` | Int | CR_RED | Likely controls sb cooperative headingcolor. | `/workspace/src/hu_scores.cpp:69` |
| `sb_cooperative_otherplayercolor` | Int | CR_GREY | Likely controls sb cooperative otherplayercolor. | `/workspace/src/hu_scores.cpp:71` |
| `sb_cooperative_yourplayercolor` | Int | CR_GREEN | Likely controls sb cooperative yourplayercolor. | `/workspace/src/hu_scores.cpp:70` |
| `sb_deathmatch_enable` | Bool | true | Likely controls sb deathmatch enable. | `/workspace/src/hu_scores.cpp:73` |
| `sb_deathmatch_headingcolor` | Int | CR_RED | Likely controls sb deathmatch headingcolor. | `/workspace/src/hu_scores.cpp:74` |
| `sb_deathmatch_otherplayercolor` | Int | CR_GREY | Likely controls sb deathmatch otherplayercolor. | `/workspace/src/hu_scores.cpp:76` |
| `sb_deathmatch_yourplayercolor` | Int | CR_GREEN | Likely controls sb deathmatch yourplayercolor. | `/workspace/src/hu_scores.cpp:75` |
| `sb_teamdeathmatch_enable` | Bool | true | Likely controls sb teamdeathmatch enable. | `/workspace/src/hu_scores.cpp:78` |
| `sb_teamdeathmatch_headingcolor` | Int | CR_RED | Likely controls sb teamdeathmatch headingcolor. | `/workspace/src/hu_scores.cpp:79` |
| `screenshot_dir` | String | "" | Likely controls screenshot dir. | `/workspace/src/m_misc.cpp:70` |
| `screenshot_quiet` | Bool | false | Likely controls screenshot quiet. | `/workspace/src/m_misc.cpp:68` |
| `screenshot_type` | String | "png" | Likely controls screenshot type. | `/workspace/src/m_misc.cpp:69` |
| `script_debug` | Bool | false | Likely controls script debug. | `/workspace/src/playsim/fragglescript/t_parse.cpp:29` |
| `sentstats_hwr_done` | Int | 0 | Likely controls sentstats hwr done. | `/workspace/src/d_anonstats.cpp:64` |
| `setslotstrict` | Bool | true | Likely controls setslotstrict. | `/workspace/src/gamedata/a_weapons.cpp:524` |
| `show_messages` | Bool | true | enable/disable showing messages | `/workspace/src/console/c_cmds.cpp:62` |
| `show_obituaries` | Bool | true | Likely controls show obituaries. | `/workspace/src/console/c_cmds.cpp:64` |
| `showendoom` | Int | 0 | Likely controls showendoom. | `/workspace/src/common/startscreen/endoom.cpp:58` |
| `showsecretsector` | Bool | false | Likely controls showsecretsector. | `/workspace/src/playsim/p_spec.cpp:593` |
| `silence_menu_hover` | Bool | true | Silences cursor movement when implicitly selecting with mouse | `/workspace/src/common/menu/optionmenu.cpp:30` |
| `silence_menu_scroll` | Bool | true | Silences cursor movement when using mouse wheel | `/workspace/src/common/menu/optionmenu.cpp:29` |
| `skin` | String | "base" | Likely controls skin. | `/workspace/src/d_netinfo.cpp:51` |
| `skyoffset` | Float | 0.f | Likely controls skyoffset. | `/workspace/src/common/rendering/hwrenderer/data/hw_skydome.cpp:42` |
| `splashfactor` | Float | 1.f | Likely controls splashfactor. | `/workspace/src/playsim/p_map.cpp:6096` |
| `statfile` | String | "zdoomstat.txt" | Likely controls statfile. | `/workspace/src/gamedata/statistics.cpp:51` |
| `stillbob` | Float | 0.f | Likely controls stillbob. | `/workspace/src/d_netinfo.cpp:57` |
| `storesavepic` | Bool | true | Likely controls storesavepic. | `/workspace/src/g_game.cpp:292` |
| `strictdecorate` | Bool | false | Likely controls strictdecorate. | `/workspace/src/common/scripting/backend/vmbuilder.cpp:34` |
| `team` | Int | TEAM_NONE | Likely controls team. | `/workspace/src/d_netinfo.cpp:52` |
| `teamdamage` | Float | 0.f | Server setting: Team Damage | `/workspace/src/g_cvars.cpp:236` |
| `telezoom` | Bool | true | Likely controls telezoom. | `/workspace/src/playsim/p_teleport.cpp:34` |
| `tf` | Int | 0 | Likely controls tf. | `/workspace/src/rendering/r_utility.cpp:485` |
| `ticker` | Bool | false | Likely controls ticker. | `/workspace/src/common/rendering/v_video.cpp:204` |
| `tilt` | Bool | false | Likely controls tilt. | `/workspace/src/rendering/swrenderer/plane/r_visibleplane.cpp:50` |
| `timelimit` | Float | 0.f | Server setting: Time Limit | `/workspace/src/d_main.cpp:484` |
| `timidity_channel_pressure` | Bool | false | Likely controls timidity channel pressure. | `/workspace/src/common/audio/music/music_config.cpp:402` |
| `timidity_chorus` | Int | 0 | Likely controls timidity chorus. | `/workspace/src/common/audio/music/music_config.cpp:392` |
| `timidity_config` | String | GAMENAMELOWERCASE | Likely controls timidity config. | `/workspace/src/common/audio/music/music_config.cpp:458` |
| `timidity_drum_effect` | Bool | false | Likely controls timidity drum effect. | `/workspace/src/common/audio/music/music_config.cpp:427` |
| `timidity_drum_power` | Float | 1.0 | Likely controls timidity drum power. | `/workspace/src/common/audio/music/music_config.cpp:437` |
| `timidity_key_adjust` | Int | 0 | Likely controls timidity key adjust. | `/workspace/src/common/audio/music/music_config.cpp:442` |
| `timidity_lpf_def` | Int | 1 | Likely controls timidity lpf def. | `/workspace/src/common/audio/music/music_config.cpp:407` |
| `timidity_min_sustain_time` | Float | 5000.f | Likely controls timidity min sustain time. | `/workspace/src/common/audio/music/music_config.cpp:452` |
| `timidity_modulation_envelope` | Bool | true | Likely controls timidity modulation envelope. | `/workspace/src/common/audio/music/music_config.cpp:417` |
| `timidity_modulation_wheel` | Bool | true | Likely controls timidity modulation wheel. | `/workspace/src/common/audio/music/music_config.cpp:372` |
| `timidity_overlap_voice_allow` | Bool | true | Likely controls timidity overlap voice allow. | `/workspace/src/common/audio/music/music_config.cpp:422` |
| `timidity_pan_delay` | Bool | false | Likely controls timidity pan delay. | `/workspace/src/common/audio/music/music_config.cpp:432` |
| `timidity_portamento` | Bool | true | Likely controls timidity portamento. | `/workspace/src/common/audio/music/music_config.cpp:377` |
| `timidity_reverb` | Int | 0 | Likely controls timidity reverb. | `/workspace/src/common/audio/music/music_config.cpp:382` |
| `timidity_reverb_level` | Int | 0 | Likely controls timidity reverb level. | `/workspace/src/common/audio/music/music_config.cpp:387` |
| `timidity_surround_chorus` | Bool | false | Likely controls timidity surround chorus. | `/workspace/src/common/audio/music/music_config.cpp:397` |
| `timidity_temper_control` | Bool | true | Likely controls timidity temper control. | `/workspace/src/common/audio/music/music_config.cpp:412` |
| `timidity_tempo_adjust` | Float | 1.f | Likely controls timidity tempo adjust. | `/workspace/src/common/audio/music/music_config.cpp:447` |
| `topskew` | Int | 0 | Likely controls topskew. | `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:2129` |
| `transsouls` | Float | 0.75f | Likely controls transsouls. | `/workspace/src/common/rendering/v_video.cpp:518` |
| `turbo` | Float | 100.f | Likely controls turbo. | `/workspace/src/g_game.cpp:386` |
| `turnspeedsprintfast` | Int | 1280 | Likely controls turnspeedsprintfast. | `/workspace/src/g_cvars.cpp:149` |
| `turnspeedsprintslow` | Int | 320 | Likely controls turnspeedsprintslow. | `/workspace/src/g_cvars.cpp:157` |
| `turnspeedwalkfast` | Int | 640 | Likely controls turnspeedwalkfast. | `/workspace/src/g_cvars.cpp:145` |
| `turnspeedwalkslow` | Int | 320 | Likely controls turnspeedwalkslow. | `/workspace/src/g_cvars.cpp:153` |
| `ui_color_mix` | Float | .35 | Likely controls ui color mix. | `/workspace/src/common/engine/i_interface.cpp:75` |
| `ui_colors` | String | "" | Likely controls ui colors. | `/workspace/src/common/engine/i_interface.cpp:74` |
| `ui_generic` | Bool | false | Likely controls ui generic. | `/workspace/src/common/fonts/v_text.cpp:313` |
| `ui_screenborder_classic_scaling` | Bool | true | Likely controls ui screenborder classic scaling. | `/workspace/src/common/2d/v_draw.cpp:36` |
| `ui_theme` | Int | 0 | launcher theme. 0: auto, 1: dark, 2: light | `/workspace/src/widgets/widgetresourcedata.cpp:30` |
| `uiscale` | Int | 1 | Likely controls uiscale. | `/workspace/src/common/rendering/v_video.cpp:144` |
| `underwater_fade_scalar` | Float | 1.0f | Likely controls underwater fade scalar. | `/workspace/src/rendering/2d/v_blend.cpp:55` |
| `use_joystick` | Bool | true | enables input from the joystick if it is present | `/workspace/src/common/engine/m_joy.cpp:80` |
| `use_mouse` | Bool | true | Likely controls use mouse. | `/workspace/src/common/platform/posix/sdl/i_input.cpp:50` |
| `var_friction` | Bool | true | Likely controls var friction. | `/workspace/src/g_cvars.cpp:143` |
| `var_pushers` | Bool | true | Likely controls var pushers. | `/workspace/src/g_cvars.cpp:137` |
| `vertspread` | Bool | false | Likely controls vertspread. | `/workspace/src/d_netinfo.cpp:62` |
| `vk_debug` | Bool | false | Likely controls vk debug. | `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:74` |
| `vk_debug_callstack` | Bool | true | Likely controls vk debug callstack. | `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:79` |
| `vk_device` | Int | 0 | Likely controls vk device. | `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:81` |
| `vk_exclusivefullscreen` | Bool | false | Likely controls vk exclusivefullscreen. | `/workspace/src/common/rendering/vulkan/textures/vk_framebuffer.cpp:32` |
| `vk_hdr` | Bool | false | Likely controls vk hdr. | `/workspace/src/common/rendering/vulkan/textures/vk_framebuffer.cpp:31` |
| `vk_raytrace` | Bool | false | Likely controls vk raytrace. | `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:68` |
| `vk_submit_size` | Int | 1000 | Likely controls vk submit size. | `/workspace/src/common/rendering/vulkan/renderer/vk_renderstate.cpp:42` |
| `vr_enable_quadbuffered` | Bool | false | Likely controls vr enable quadbuffered. | `/workspace/src/common/platform/win32/win32glvideo.cpp:68` |
| `vr_hunits_per_meter` | Float | 41.0f | Likely controls vr hunits per meter. | `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:45` |
| `vr_ipd` | Float | 0.062f | Likely controls vr ipd. | `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:39` |
| `vr_mode` | Int | 0 | Likely controls vr mode. | `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:33` |
| `vr_screendist` | Float | 0.80f | Likely controls vr screendist. | `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:42` |
| `vr_swap_eyes` | Bool | false | Likely controls vr swap eyes. | `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:36` |
| `warningstoerrors` | Bool | false | Likely controls warningstoerrors. | `/workspace/src/common/scripting/backend/vmbuilder.cpp:35` |
| `wbobfire` | Float | 0.f | Likely controls wbobfire. | `/workspace/src/d_netinfo.cpp:59` |
| `wbobspeed` | Float | 1.f | Likely controls wbobspeed. | `/workspace/src/d_netinfo.cpp:58` |
| `wi_autoadvance` | Int | 0 | Likely controls wi autoadvance. | `/workspace/src/wi_stuff.cpp:57` |
| `wi_cleantextscale` | Bool | false | Likely controls wi cleantextscale. | `/workspace/src/wi_stuff.cpp:58` |
| `wi_noautostartmap` | Bool | false | Likely controls wi noautostartmap. | `/workspace/src/wi_stuff.cpp:56` |
| `wi_percents` | Bool | true | Likely controls wi percents. | `/workspace/src/wi_stuff.cpp:54` |
| `wi_showtotaltime` | Bool | true | Likely controls wi showtotaltime. | `/workspace/src/wi_stuff.cpp:55` |
| `wildmidi_config` | String | "" | Likely controls wildmidi config. | `/workspace/src/common/audio/music/music_config.cpp:469` |
| `wildmidi_enhanced_resampling` | Bool | true | Likely controls wildmidi enhanced resampling. | `/workspace/src/common/audio/music/music_config.cpp:479` |
| `wildmidi_reverb` | Bool | false | Likely controls wildmidi reverb. | `/workspace/src/common/audio/music/music_config.cpp:474` |
| `win_h` | Int | -1 | Likely controls win h. | `/workspace/src/common/rendering/v_video.cpp:53` |
| `win_maximized` | Bool | false | Likely controls win maximized. | `/workspace/src/common/rendering/v_video.cpp:54` |
| `win_w` | Int | -1 | Likely controls win w. | `/workspace/src/common/rendering/v_video.cpp:52` |
| `win_x` | Int | -1 | Likely controls win x. | `/workspace/src/common/rendering/v_video.cpp:50` |
| `win_y` | Int | -1 | Likely controls win y. | `/workspace/src/common/rendering/v_video.cpp:51` |
| `wipetype` | Int | 1 | Likely controls wipetype. | `/workspace/src/d_main.cpp:485` |
| `xbrz_colorformat` | Int | 0 | Likely controls xbrz colorformat. | `/workspace/src/common/textures/hires/hqresize.cpp:93` |

## HCDE Server, Invasion, and Netcode CVARs

These are the high-value controls for invasion, net diagnostics, compatibility, and heavy-load cleanup.

### `cl_hcde_predict_dedicated`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Enable client-side movement prediction when connected to a dedicated HCDE server.
- Source default: `true`
- Valid range/shape: `n/a`
- Source: `/workspace/src/playsim/p_user.cpp:92`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `duellimit`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Legacy Skulltag compatibility value for duel limit metadata.
- Source default: `0`
- Valid range/shape: `0..255`
- Source: `/workspace/src/d_net_invasion.inl:113`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_hud_debug`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Mirror net diagnostics to the HUD console for live operator visibility.
- Source default: `true`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:185`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_k8vavoom_lighting_profile`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Selects a composed K8vavoom lighting preset (0=off, 1+=profile id) and applies bundled renderer toggles.
- Source default: `0`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:214`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_k8vavoom_raylight_probe`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Enable ray-light probing hooks used by K8vavoom-style lighting profile diagnostics.
- Source default: `false`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:212`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_k8vavoom_shadow_boost`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Apply stronger shadow-map defaults when a K8vavoom lighting profile is active.
- Source default: `false`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:211`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_lag_hud`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Persistent on-screen lag/invasion overlay (top-left). Also enable with `stat hcde_lag`.
- Source default: `false`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:197`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_nanobsp_loader`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Selects NanoBSP loader mode for map geometry ingestion (0=off, 1=on, 2=force).
- Source default: `0`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_nanobsp_loader.cpp:51`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Adaptively reduce shadow-casting light count to stay near the target shadow-map frame budget.
- Source default: `false`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:70`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget_minlights`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Minimum number of shadow-casting lights retained while auto-budget throttles the light count.
- Source default: `64`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:280`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget_step`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Number of shadow-casting lights removed or restored per auto-budget adjustment step.
- Source default: `32`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:288`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget_targetms`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Target milliseconds per frame allocated to shadow-map rendering when auto-budget is enabled.
- Source default: `1.20f`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:272`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_shadow_autofallback`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Automatically disable shadow maps when the renderer reports unsupported or failing shadow-map paths.
- Source default: `true`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:69`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_shadow_forcealllights`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Force eligible dynamic lights onto the shadow-map path even when not explicitly marked shadowmapped.
- Source default: `true`
- Valid range/shape: `n/a`
- Source: `/workspace/src/rendering/hwrenderer/hw_entrypoint.cpp:59`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `hcde_startup_profile`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Emit startup timing profile data for engine initialization diagnostics.
- Source default: `false`
- Valid range/shape: `n/a`
- Source: `/workspace/src/scripting/thingdef.cpp:54`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `net_hcde_native_only`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Requires HCDE-native networking/capability paths for multiplayer sessions.
- Source default: `true`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:312`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `net_predict_debug`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Controls HCDE prediction diagnostics: off, CSV sampling, and/or on-screen/debug trace output depending on level.
- Source default: `0`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:211`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `net_predict_debug_interval`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Tic interval used by prediction CSV/debug sampling.
- Source default: `15`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:219`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `net_predict_softwarn_ack_lag`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Soft warning threshold for client ack lag during prediction diagnostics.
- Source default: `3`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:268`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `net_predict_softwarn_mirror_delta`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Soft warning threshold for invasion mirror drift during prediction diagnostics.
- Source default: `2`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:295`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `net_predict_softwarn_passive_storm`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Soft warning threshold for passive update storms during prediction diagnostics.
- Source default: `5`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:303`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `snd_backend`

- Category: [Audio](#category-audio)
- Description: Audio backend selector: `openal` (default), `null` (silent), or `eternity` (spatial facade).
- Source default: `DEF_BACKEND`
- Valid range/shape: `n/a`
- Source: `/workspace/src/common/audio/sound/i_sound.cpp:64`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_corpsefilter`

- Category: [Server & Multiplayer](#category-server)
- Description: Selects which corpse queues sv_corpsequeuesize trims: 0 off, 1 monsters, 2 players, 3 both.
- Source default: `1`
- Valid range/shape: `0..3`
- Source: `/workspace/src/g_cvars.cpp:176`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_corpsequeuesize`

- Category: [Server & Multiplayer](#category-server)
- Description: Maximum queued corpses retained by corpse cleanup; used with sv_corpsefilter.
- Source default: `64`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/g_cvars.cpp:184`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionbasebudget`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Base monster budget each wave starts with.
- Source default: `24`
- Valid range/shape: `>= 1`
- Source: `/workspace/src/d_net_invasion.inl:123`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionbossbonus`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Extra budget added during boss waves.
- Source default: `20`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:158`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionbosswaveevery`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Boss wave cadence (e.g. 5 = every 5th wave, 0 = never).
- Source default: `5`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:153`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionbudgetstep`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Budget increase applied per wave number.
- Source default: `8`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:128`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasioncleanuptime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds allowed for cleanup phase after spawning ends.
- Source default: `4.0f`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:81`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasioncountdowntime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds before wave 1 starts ("Prepare for invasion" countdown).
- Source default: `30.0f`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:68`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasiondebug`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Server setting: Invasion Debug
- Source default: `0`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net.cpp:189`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionexitonvictory`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Server setting: Invasion Exit Victory
- Source default: `true`
- Valid range/shape: `n/a`
- Source: `/workspace/src/d_net_invasion.inl:96`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionintermissiontime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds between completed waves before the next wave starts.
- Source default: `6.0f`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:86`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionmaxactive`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Optional cap for active invasion monsters. 0 disables the cap; positive values are clamped by the engine.
- Source default: `0`
- Valid range/shape: `0 or 1..1024`
- Source: `/workspace/src/d_net_invasion.inl:148`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionperplayer`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Additional budget per extra active player.
- Source default: `6`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:133`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionresulttime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds to keep the final victory/failure state visible.
- Source default: `8.0f`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:91`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionsimlod`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Enables server-side simulation LOD for invasion monsters so distant actors think less often under heavy load.
- Source default: `true`
- Valid range/shape: `bool`
- Source: `/workspace/src/d_net_invasion.inl:169`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionsimloddormantinterval`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Think interval in tics for dormant distant invasion simulation.
- Source default: `TICRATE * 3`
- Valid range/shape: `>= 1 tic`
- Source: `/workspace/src/d_net_invasion.inl:190`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionsimlodfullrange`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Distance within which invasion monsters keep full-rate simulation.
- Source default: `2048.0f`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:172`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionsimlodreducedinterval`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Think interval in tics for reduced-rate invasion simulation.
- Source default: `5`
- Valid range/shape: `>= 1 tic`
- Source: `/workspace/src/d_net_invasion.inl:184`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionsimlodreducedrange`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Distance within which invasion monsters use reduced-rate simulation before becoming dormant.
- Source default: `4096.0f`
- Valid range/shape: `>= sv_invasionsimlodfullrange`
- Source: `/workspace/src/d_net_invasion.inl:178`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionspawnburst`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Maximum monsters spawned per spawn tick burst.
- Source default: `3`
- Valid range/shape: `>= 1`
- Source: `/workspace/src/d_net_invasion.inl:143`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionspawninterval`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds between spawn ticks while wave spawning is active.
- Source default: `0.35f`
- Valid range/shape: `>= 0.05`
- Source: `/workspace/src/d_net_invasion.inl:138`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionspawntime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Wave spawn window length in seconds before cleanup phase.
- Source default: `8.0f`
- Valid range/shape: `>= 0`
- Source: `/workspace/src/d_net_invasion.inl:76`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionspotfallback`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Fallback to generic spawning when tagged invasion spots cannot be used.
- Source default: `true`
- Valid range/shape: `bool`
- Source: `/workspace/src/d_net_invasion.inl:166`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionspotusemaptags`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Restrict native invasion spots by map thing TID/tag. Keep disabled for Skulltag/Zandronum map compatibility; the spot arguments already control wave timing.
- Source default: `false`
- Valid range/shape: `bool`
- Source: `/workspace/src/d_net_invasion.inl:163`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_invasionwaves`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Maximum number of invasion waves in a run.
- Source default: `8`
- Valid range/shape: `1..255`
- Source: `/workspace/src/d_net_invasion.inl:99`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `sv_usemapsettingswavelimit`

- Category: [Server & Multiplayer](#category-server)
- Description: If enabled, map-defined invasion wavelimit metadata overrides sv_invasionwaves when present.
- Source default: `true`
- Valid range/shape: `bool`
- Source: `/workspace/src/d_net_invasion.inl:120`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

### `wavelimit`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Legacy Skulltag compatibility override for invasion waves. 0 disables the override; 1..255 forces that wave count.
- Source default: `0`
- Valid range/shape: `0..255`
- Source: `/workspace/src/d_net_invasion.inl:106`
- Present in runtime snapshot: n/a (source-only generation)
- Runtime snapshot value: `n/a`

## Source-Defined CVAR Catalog

This section is generated from CVAR, CUSTOM_CVAR, CVARD, CUSTOM_CVARD, and named CVAR macros in src/.

### `addrocketexplosion`

- Category: [Other](#category-misc)
- Description: Likely controls addrocketexplosion.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_mobj.cpp:135`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_auto_arpeggio`

- Category: [Other](#category-misc)
- Description: Likely controls adl auto arpeggio.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:107`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_bank`

- Category: [Other](#category-misc)
- Description: Likely controls adl bank.
- Type: `Int`
- Source default: `14`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:77`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_chan_alloc`

- Category: [Other](#category-misc)
- Description: Likely controls adl chan alloc.
- Type: `Int`
- Source default: `0 /*ADLMIDI_ChanAlloc_AUTO*/`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:102`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_chips_count`

- Category: [Other](#category-misc)
- Description: Likely controls adl chips count.
- Type: `Int`
- Source default: `6`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_custom_bank`

- Category: [Other](#category-misc)
- Description: Likely controls adl custom bank.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL | CVAR_SYSTEM_ONLY`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:92`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_emulator_id`

- Category: [Other](#category-misc)
- Description: Likely controls adl emulator id.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_fullpan`

- Category: [Other](#category-misc)
- Description: Likely controls adl fullpan.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:72`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_gain`

- Category: [Other](#category-misc)
- Description: Likely controls adl gain.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:112`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_run_at_pcm_rate`

- Category: [Other](#category-misc)
- Description: Likely controls adl run at pcm rate.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_use_custom_bank`

- Category: [Other](#category-misc)
- Description: Likely controls adl use custom bank.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:82`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_use_genmidi`

- Category: [Other](#category-misc)
- Description: Likely controls adl use genmidi.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `adl_volume_model`

- Category: [Other](#category-misc)
- Description: Likely controls adl volume model.
- Type: `Int`
- Source default: `0 /*ADLMIDI_VolumeModel_AUTO*/`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:97`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `aimdebug`

- Category: [Other](#category-misc)
- Description: Likely controls aimdebug.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:3895`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `allcheats`

- Category: [Other](#category-misc)
- Description: Likely controls allcheats.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/st_stuff.cpp:296`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `allowsingleplayerscripts`

- Category: [Other](#category-misc)
- Description: Likely controls allowsingleplayerscripts.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_acs.cpp:10957`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `alwaysapplydmflags`

- Category: [Other](#category-misc)
- Description: Server setting: Apply DM Flags Always
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:140`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_backcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am backcolor.
- Type: `Color`
- Source default: `0x6c5440`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:269`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_cdwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am cdwallcolor.
- Type: `Color`
- Source default: `0x4c3820`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:276`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_cheat`

- Category: [Automap](#category-automap)
- Description: Likely controls am cheat.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:133`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_colorset`

- Category: [Automap](#category-automap)
- Description: Likely controls am colorset.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:159`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_customcolors`

- Category: [Automap](#category-automap)
- Description: Likely controls am customcolors.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:160`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_drawmapback`

- Category: [Automap](#category-automap)
- Description: Likely controls am drawmapback.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:162`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_efwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am efwallcolor.
- Type: `Color`
- Source default: `0x665555`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:277`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_emptyspacemargin`

- Category: [Automap](#category-automap)
- Description: Likely controls am emptyspacemargin.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:168`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_fdwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am fdwallcolor.
- Type: `Color`
- Source default: `0x887058`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:275`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_followplayer`

- Category: [Automap](#category-automap)
- Description: Likely controls am followplayer.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:191`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_gridcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am gridcolor.
- Type: `Color`
- Source default: `0x8b5a2b`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:279`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_interlevelcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am interlevelcolor.
- Type: `Color`
- Source default: `0xff0000`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:284`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_intralevelcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am intralevelcolor.
- Type: `Color`
- Source default: `0x0000ff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:283`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_linealpha`

- Category: [Automap](#category-automap)
- Description: Likely controls am linealpha.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:119`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_lineantialiasing`

- Category: [Automap](#category-automap)
- Description: Likely controls am lineantialiasing.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:121`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_linethickness`

- Category: [Automap](#category-automap)
- Description: Likely controls am linethickness.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:120`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_lockedcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am lockedcolor.
- Type: `Color`
- Source default: `0x007800`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:282`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_map_secrets`

- Category: [Automap](#category-automap)
- Description: Likely controls am map secrets.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:161`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_markcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am markcolor.
- Type: `Int`
- Source default: `CR_GREY`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:198`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_markfont`

- Category: [Automap](#category-automap)
- Description: Likely controls am markfont.
- Type: `String`
- Source default: `DEFAULT_FONT_NAME`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:197`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_notseencolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am notseencolor.
- Type: `Color`
- Source default: `0x6c6c6c`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:281`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovcdwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovcdwallcolor.
- Type: `Color`
- Source default: `0x008844`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:304`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovefwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovefwallcolor.
- Type: `Color`
- Source default: `0x008844`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:302`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_overlay`

- Category: [Automap](#category-automap)
- Description: Likely controls am overlay.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:144`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovfdwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovfdwallcolor.
- Type: `Color`
- Source default: `0x008844`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:303`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovinterlevelcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovinterlevelcolor.
- Type: `Color`
- Source default: `0xffff00`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:307`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovlockedcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovlockedcolor.
- Type: `Color`
- Source default: `0x008844`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:301`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovotherwallscolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovotherwallscolor.
- Type: `Color`
- Source default: `0x008844`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:300`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovportalcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovportalcolor.
- Type: `Color`
- Source default: `0x004022`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:318`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovsecretsectorcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovsecretsectorcolor.
- Type: `Color`
- Source default: `0x00ffff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:308`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovsecretwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovsecretwallcolor.
- Type: `Color`
- Source default: `0x008844`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:298`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovsectorfillalpha`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovsectorfillalpha.
- Type: `Float`
- Source default: `0.2f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:317`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovsectorfillcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovsectorfillcolor.
- Type: `Color`
- Source default: `0x000000`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:316`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovspecialwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovspecialwallcolor.
- Type: `Color`
- Source default: `0xffffff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:299`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovtelecolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovtelecolor.
- Type: `Color`
- Source default: `0xffff00`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:306`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovthingcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovthingcolor.
- Type: `Color`
- Source default: `0xe88800`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:310`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovthingcolor_citem`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovthingcolor citem.
- Type: `Color`
- Source default: `0xe88800`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:315`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovthingcolor_friend`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovthingcolor friend.
- Type: `Color`
- Source default: `0xe88800`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:311`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovthingcolor_item`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovthingcolor item.
- Type: `Color`
- Source default: `0xe88800`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:314`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovthingcolor_monster`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovthingcolor monster.
- Type: `Color`
- Source default: `0xe88800`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:312`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovthingcolor_ncmonster`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovthingcolor ncmonster.
- Type: `Color`
- Source default: `0xe88800`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:313`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovunexploredsecretcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovunexploredsecretcolor.
- Type: `Color`
- Source default: `0x00ffff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:309`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovunseencolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovunseencolor.
- Type: `Color`
- Source default: `0x00226e`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:305`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovwallcolor.
- Type: `Color`
- Source default: `0x00ff00`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:297`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_ovyourcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am ovyourcolor.
- Type: `Color`
- Source default: `0xfce8d8`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:296`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_portalcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am portalcolor.
- Type: `Color`
- Source default: `0x404040`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:294`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_portaloverlay`

- Category: [Automap](#category-automap)
- Description: Likely controls am portaloverlay.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:192`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_rotate`

- Category: [Automap](#category-automap)
- Description: Likely controls am rotate.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:143`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_secretsectorcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am secretsectorcolor.
- Type: `Color`
- Source default: `0xff00ff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:285`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_secretwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am secretwallcolor.
- Type: `Color`
- Source default: `0x000000`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:272`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_sectorfillalpha`

- Category: [Automap](#category-automap)
- Description: Likely controls am sectorfillalpha.
- Type: `Float`
- Source default: `0.4f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:293`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_sectorfillcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am sectorfillcolor.
- Type: `Color`
- Source default: `0x4e3621`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:292`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showalllines`

- Category: [Automap](#category-automap)
- Description: Likely controls am showalllines.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:126`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showcluster`

- Category: [Automap](#category-automap)
- Description: Likely controls am showcluster.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showepisode`

- Category: [Automap](#category-automap)
- Description: Likely controls am showepisode.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showgrid`

- Category: [Automap](#category-automap)
- Description: Likely controls am showgrid.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:193`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showitems`

- Category: [Automap](#category-automap)
- Description: Likely controls am showitems.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:155`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showkeys`

- Category: [Automap](#category-automap)
- Description: Likely controls am showkeys.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:163`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showkeys_always`

- Category: [Automap](#category-automap)
- Description: Likely controls am showkeys always.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:166`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showlevelname`

- Category: [Automap](#category-automap)
- Description: Likely controls am showlevelname.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:158`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showmaplabel`

- Category: [Automap](#category-automap)
- Description: Likely controls am showmaplabel.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:121`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showmonsters`

- Category: [Automap](#category-automap)
- Description: Likely controls am showmonsters.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:154`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showsecrets`

- Category: [Automap](#category-automap)
- Description: Likely controls am showsecrets.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:153`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showsubsector`

- Category: [Automap](#category-automap)
- Description: Likely controls am showsubsector.
- Type: `Int`
- Source default: `-1`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:123`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showthingsprites`

- Category: [Automap](#category-automap)
- Description: Likely controls am showthingsprites.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:165`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showtime`

- Category: [Automap](#category-automap)
- Description: Likely controls am showtime.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:156`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showtotaltime`

- Category: [Automap](#category-automap)
- Description: Likely controls am showtotaltime.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:157`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_showtriggerlines`

- Category: [Automap](#category-automap)
- Description: Likely controls am showtriggerlines.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:164`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_specialwallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am specialwallcolor.
- Type: `Color`
- Source default: `0xffffff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:273`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_textured`

- Category: [Automap](#category-automap)
- Description: Likely controls am textured.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:118`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_thingcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am thingcolor.
- Type: `Color`
- Source default: `0xfcfcfc`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:278`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_thingcolor_citem`

- Category: [Automap](#category-automap)
- Description: Likely controls am thingcolor citem.
- Type: `Color`
- Source default: `0xfcfcfc`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:291`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_thingcolor_friend`

- Category: [Automap](#category-automap)
- Description: Likely controls am thingcolor friend.
- Type: `Color`
- Source default: `0xfcfcfc`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:287`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_thingcolor_item`

- Category: [Automap](#category-automap)
- Description: Likely controls am thingcolor item.
- Type: `Color`
- Source default: `0xfcfcfc`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:290`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_thingcolor_monster`

- Category: [Automap](#category-automap)
- Description: Likely controls am thingcolor monster.
- Type: `Color`
- Source default: `0xfcfcfc`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:288`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_thingcolor_ncmonster`

- Category: [Automap](#category-automap)
- Description: Likely controls am thingcolor ncmonster.
- Type: `Color`
- Source default: `0xfcfcfc`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:289`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_thingrenderstyles`

- Category: [Automap](#category-automap)
- Description: Likely controls am thingrenderstyles.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:122`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_tswallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am tswallcolor.
- Type: `Color`
- Source default: `0x888888`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:274`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_unexploredsecretcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am unexploredsecretcolor.
- Type: `Color`
- Source default: `0xff00ff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:286`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_wallcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am wallcolor.
- Type: `Color`
- Source default: `0x2c1808`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:271`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_xhaircolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am xhaircolor.
- Type: `Color`
- Source default: `0x808080`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:280`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_yourcolor`

- Category: [Automap](#category-automap)
- Description: Likely controls am yourcolor.
- Type: `Color`
- Source default: `0xfce8d8`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:270`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `am_zoomdir`

- Category: [Automap](#category-automap)
- Description: Likely controls am zoomdir.
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/am_map.cpp:194`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `anonstats_enabled411`

- Category: [Other](#category-misc)
- Description: Likely controls anonstats enabled411.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_anonstats.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `anonstats_host`

- Category: [Other](#category-misc)
- Description: Likely controls anonstats host.
- Type: `String`
- Source default: `"gzstats.drdteam.org"`
- Source flags: `CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_anonstats.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `anonstats_port`

- Category: [Other](#category-misc)
- Description: Likely controls anonstats port.
- Type: `Int`
- Source default: `80`
- Source flags: `CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_anonstats.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `autoaim`

- Category: [Other](#category-misc)
- Description: Likely controls autoaim.
- Type: `Float`
- Source default: `35.f`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:47`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `autoloadbrightmaps`

- Category: [Other](#category-misc)
- Description: Likely controls autoloadbrightmaps.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:511`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `autoloadlights`

- Category: [Other](#category-misc)
- Description: Likely controls autoloadlights.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:523`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `autoloadwidescreen`

- Category: [Other](#category-misc)
- Description: Likely controls autoloadwidescreen.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:524`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `autosavecount`

- Category: [Other](#category-misc)
- Description: Likely controls autosavecount.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:417`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `autosavenum`

- Category: [Other](#category-misc)
- Description: Likely controls autosavenum.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_NOSET|CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:413`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `blood_fade_scalar`

- Category: [Other](#category-misc)
- Description: Likely controls blood fade scalar.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/2d/v_blend.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `bot_allowspy`

- Category: [Other](#category-misc)
- Description: Likely controls bot allowspy.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:411`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `bot_next_color`

- Category: [Other](#category-misc)
- Description: Likely controls bot next color.
- Type: `Int`
- Source default: `11`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/bots/b_bot.cpp:144`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `bottomskew`

- Category: [Other](#category-misc)
- Description: Likely controls bottomskew.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:2131`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chase_dist`

- Category: [Other](#category-misc)
- Description: Likely controls chase dist.
- Type: `Float`
- Source default: `90.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:5684`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chase_height`

- Category: [Other](#category-misc)
- Description: Likely controls chase height.
- Type: `Float`
- Source default: `-8.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:5683`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chasedemo`

- Category: [Other](#category-misc)
- Description: Likely controls chasedemo.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:291`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chat_substitution`

- Category: [Other](#category-misc)
- Description: Likely controls chat substitution.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:111`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro0`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro0.
- Type: `String`
- Source default: `"No"`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:95`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro1`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro1.
- Type: `String`
- Source default: `"I'm ready to kick butt!"`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:86`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro2`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro2.
- Type: `String`
- Source default: `"I'm OK."`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro3`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro3.
- Type: `String`
- Source default: `"I'm not looking too good!"`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:88`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro4`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro4.
- Type: `String`
- Source default: `"Help!"`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:89`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro5`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro5.
- Type: `String`
- Source default: `"You suck!"`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:90`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro6`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro6.
- Type: `String`
- Source default: `"Next time, scumbag..."`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:91`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro7`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro7.
- Type: `String`
- Source default: `"Come here!"`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:92`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro8`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro8.
- Type: `String`
- Source default: `"I'll take care of it."`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:93`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `chatmacro9`

- Category: [Other](#category-misc)
- Description: Likely controls chatmacro9.
- Type: `String`
- Source default: `"Yes"`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:94`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_analog_run`

- Category: [Client](#category-client)
- Description: Likely controls analog run behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:333`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_analog_sensitivity_pitch`

- Category: [Client](#category-client)
- Description: Likely controls analog sensitivity pitch behavior for client.
- Type: `Float`
- Source default: `0.6f`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:332`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_analog_sensitivity_yaw`

- Category: [Client](#category-client)
- Description: Likely controls analog sensitivity yaw behavior for client.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:331`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_analog_straferun`

- Category: [Client](#category-client)
- Description: Likely controls analog straferun behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:334`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_bbannounce`

- Category: [Client](#category-client)
- Description: Likely controls bbannounce behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/bbannouncer.cpp:63`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_blockcheats`

- Category: [Client](#category-client)
- Description: Likely controls blockcheats behavior for client.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_cmds.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_bloodsplats`

- Category: [Client](#category-client)
- Description: Likely controls bloodsplats behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_bloodtype`

- Category: [Client](#category-client)
- Description: Likely controls bloodtype behavior for client.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_mobj.cpp:137`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_capfps`

- Category: [Client](#category-client)
- Description: Likely controls capfps behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_framebuffer.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_custominvulmapcolor1`

- Category: [Client](#category-client)
- Description: Likely controls custominvulmapcolor1 behavior for client.
- Type: `Color`
- Source default: `0x00001a`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_data/colormaps.cpp:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_custominvulmapcolor2`

- Category: [Client](#category-client)
- Description: Likely controls custominvulmapcolor2 behavior for client.
- Type: `Color`
- Source default: `0xa6a67a`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_data/colormaps.cpp:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_customizeinvulmap`

- Category: [Client](#category-client)
- Description: Likely controls customizeinvulmap behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_data/colormaps.cpp:37`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_debug_monster_proximity`

- Category: [Client](#category-client)
- Description: Likely controls debug monster proximity behavior for client.
- Type: `Int`
- Source default: `768`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:287`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_debugprediction`

- Category: [Client](#category-client)
- Description: Likely controls debugprediction behavior for client.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_CHEAT`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:207`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_defaultconfiguration`

- Category: [Client](#category-client)
- Description: Likely controls defaultconfiguration behavior for client.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_bind.cpp:879`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_doautoaim`

- Category: [Client](#category-client)
- Description: Likely controls doautoaim behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_doubleclickthreshold`

- Category: [Client](#category-client)
- Description: Likely controls doubleclickthreshold behavior for client.
- Type: `Int`
- Source default: `250`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_bind.cpp:139`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_gfxlocalization`

- Category: [Client](#category-client)
- Description: Likely controls gfxlocalization behavior for client.
- Type: `Int`
- Source default: `3`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/menu/doommenu.cpp:1614`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_hcde_predict_dedicated`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Enable client-side movement prediction when connected to a dedicated HCDE server.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:92`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_maxdecals`

- Category: [Client](#category-client)
- Description: Likely controls maxdecals behavior for client.
- Type: `Int`
- Source default: `1024`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:195`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_missiledecals`

- Category: [Client](#category-client)
- Description: Likely controls missiledecals behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_mobj.cpp:134`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_net_prediction_lead`

- Category: [Client](#category-client)
- Description: Likely controls net prediction lead behavior for client.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:260`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_noboldchat`

- Category: [Client](#category-client)
- Description: Likely controls noboldchat behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:197`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_nochatsound`

- Category: [Client](#category-client)
- Description: Likely controls nochatsound behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:198`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_nointros`

- Category: [Client](#category-client)
- Description: Likely controls nointros behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:539`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_noprediction`

- Category: [Client](#category-client)
- Description: Likely controls noprediction behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:89`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_oldfreelooklimit`

- Category: [Client](#category-client)
- Description: Likely controls oldfreelooklimit behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/r_swrenderer.cpp:49`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_predict_lerpscale`

- Category: [Client](#category-client)
- Description: Likely controls predict lerpscale behavior for client.
- Type: `Float`
- Source default: `0.05f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:93`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_predict_lerpthreshold`

- Category: [Client](#category-client)
- Description: Likely controls predict lerpthreshold behavior for client.
- Type: `Float`
- Source default: `2.00f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:94`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_predict_max`

- Category: [Client](#category-client)
- Description: Likely controls predict max behavior for client.
- Type: `Int`
- Source default: `24`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:128`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_predict_specials`

- Category: [Client](#category-client)
- Description: Likely controls predict specials behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_pufftype`

- Category: [Client](#category-client)
- Description: Likely controls pufftype behavior for client.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_mobj.cpp:136`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_restartondeath`

- Category: [Client](#category-client)
- Description: Likely controls restartondeath behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:296`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_rockettrails`

- Category: [Client](#category-client)
- Description: Likely controls rockettrails behavior for client.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_effect.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_rubberband_limit`

- Category: [Client](#category-client)
- Description: Likely controls rubberband limit behavior for client.
- Type: `Float`
- Source default: `756.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:117`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_rubberband_minmove`

- Category: [Client](#category-client)
- Description: Likely controls rubberband minmove behavior for client.
- Type: `Float`
- Source default: `20.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:112`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_rubberband_scale`

- Category: [Client](#category-client)
- Description: Likely controls rubberband scale behavior for client.
- Type: `Float`
- Source default: `0.3f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:96`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_rubberband_threshold`

- Category: [Client](#category-client)
- Description: Likely controls rubberband threshold behavior for client.
- Type: `Float`
- Source default: `32.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:107`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_run`

- Category: [Client](#category-client)
- Description: Likely controls run behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:323`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_scaleweaponfov`

- Category: [Client](#category-client)
- Description: Likely controls scaleweaponfov behavior for client.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:244`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_showchat`

- Category: [Client](#category-client)
- Description: Likely controls showchat behavior for client.
- Type: `Int`
- Source default: `CHAT_GLOBAL`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:199`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_showmultikills`

- Category: [Client](#category-client)
- Description: Likely controls showmultikills behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_interaction.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_showsecretmessage`

- Category: [Client](#category-client)
- Description: Likely controls showsecretmessage behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_spec.cpp:594`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_showsprees`

- Category: [Client](#category-client)
- Description: Likely controls showsprees behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_interaction.cpp:66`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_smooth_decay`

- Category: [Client](#category-client)
- Description: Likely controls smooth decay behavior for client.
- Type: `Float`
- Source default: `0.85f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:320`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_smooth_maxdist`

- Category: [Client](#category-client)
- Description: Likely controls smooth maxdist behavior for client.
- Type: `Float`
- Source default: `32.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:329`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_smooth_reconcile`

- Category: [Client](#category-client)
- Description: Likely controls smooth reconcile behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:317`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_spreaddecals`

- Category: [Client](#category-client)
- Description: Likely controls spreaddecals behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:136`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_stannounce`

- Category: [Client](#category-client)
- Description: Likely controls stannounce behavior for client.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/bbannouncer.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `cl_waitforsave`

- Category: [Client](#category-client)
- Description: Likely controls waitforsave behavior for client.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:294`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `classic_scaling_factor`

- Category: [Other](#category-misc)
- Description: Likely controls classic scaling factor.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/2d/v_2ddrawer.cpp:39`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `classic_scaling_pixelaspect`

- Category: [Other](#category-misc)
- Description: Likely controls classic scaling pixelaspect.
- Type: `Float`
- Source default: `1.2f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/2d/v_2ddrawer.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `classicflight`

- Category: [Other](#category-misc)
- Description: Likely controls classicflight.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:61`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `color`

- Category: [Other](#category-misc)
- Description: Likely controls color.
- Type: `Color`
- Source default: `0x40cf00`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:49`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `colorset`

- Category: [Other](#category-misc)
- Description: Likely controls colorset.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_anybossdeath`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_ANYBOSSDEATH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:935`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_avoidhazard`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_AVOID_HAZARDS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:957`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_badangles`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_BADANGLES`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:946`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_boomscroll`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_BOOMSCROLL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:929`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_checkswitchrange`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_CHECKSWITCHRANGE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:953`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_crossdropoff`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_CROSSDROPOFF`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:934`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_dehhealth`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_DEHHEALTH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:926`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_dr_crusher`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_DR_CRUSHER`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:966`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_dr_liquidfriction`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_DR_LIQUIDFRICTION`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:967`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_dropoff`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_DROPOFF`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:928`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_emulatemikoportals`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_EMULATEMIKOPORTALS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:963`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_explode1`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_EXPLODE1`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:954`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_explode2`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_EXPLODE2`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:955`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_fdteleport`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_FDTELEPORT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:961`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_floormove`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_FLOORMOVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:947`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_hitscan`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_HITSCAN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:942`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_invisibility`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_INVISIBILITY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:930`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_light`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_LIGHT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:943`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_limitpain`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_LIMITPAIN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:916`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_maskedmidtex`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_MASKEDMIDTEX`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:945`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_mbfmonstermove`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_MBFMONSTERMOVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:938`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_minotaur`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_MINOTAUR`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:936`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_missileclip`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_MISSILECLIP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:933`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_multiexit`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_MULTIEXIT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:950`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_mushroom`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_MUSHROOM`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:937`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_noblockfriends`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_NOBLOCKFRIENDS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:940`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_nodoorlight`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_NODOORLIGHT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:923`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_nofriendlyspawn`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_NOFRIENDLYSPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:965`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_noid24`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_NOID24`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:968`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_nombf21`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_NOMBF21`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:959`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_nopassover`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_NO_PASSMOBJ`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:918`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_notossdrops`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_NOTOSSDROPS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:921`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_novdolllockmsg`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_NOVDOLLLOCKMSG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:962`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_pointonline`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_POINTONLINE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:949`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_polyobj`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_POLYOBJ`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:944`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_pushwindow`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_PUSHWINDOW`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:952`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_railing`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_RAILING`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:956`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_ravenscroll`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_RAVENSCROLL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:924`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_reservedlineflag`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_RESERVEDLINEFLAG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:964`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_sectorsounds`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_SECTORSOUNDS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:932`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_shortTex`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_SHORTTEX`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:914`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_silentinstantfloors`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_SILENT_INSTANT_FLOORS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:931`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_silentpickup`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_SILENTPICKUP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:917`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_soundcutoff`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_SOUNDCUTOFF`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:948`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_soundslots`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_MAGICSILENCE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:919`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_soundtarget`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_SOUNDTARGET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:925`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_spritesort`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_SPRITESORT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:941`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_stairs`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_STAIRINDEX`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:915`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_stayonlift`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_STAYONLIFT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:958`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_teleport`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_TELEPORT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:951`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_trace`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_TRACE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:927`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_useblocking`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_USEBLOCKING`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:922`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_vileghosts`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_VILEGHOSTS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:939`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_voodoozombies`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags2.
- Type: `Flag`
- Source default: `compatflags2`
- Source flags: `COMPATF2_VOODOO_ZOMBIES`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:960`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compat_wallrun`

- Category: [Gameplay](#category-gameplay)
- Description: Flag alias backed by compatflags.
- Type: `Flag`
- Source default: `compatflags`
- Source flags: `COMPATF_WALLRUN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:920`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compatflags`

- Category: [Gameplay](#category-gameplay)
- Description: Server setting: Raw Compat Flags
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_SERVERINFO | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:824`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compatflags2`

- Category: [Gameplay](#category-gameplay)
- Description: Server setting: Raw Compat Flags 2
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_SERVERINFO | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:832`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `compatmode`

- Category: [Gameplay](#category-gameplay)
- Description: Server setting: Compat Mode
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:841`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_4bitansi`

- Category: [Other](#category-misc)
- Description: Likely controls con 4bitansi.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/i_system.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_alpha`

- Category: [Other](#category-misc)
- Description: Likely controls con alpha.
- Type: `Float`
- Source default: `0.75f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:133`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_buffersize`

- Category: [Other](#category-misc)
- Description: Likely controls con buffersize.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_centernotify`

- Category: [Other](#category-misc)
- Description: Likely controls con centernotify.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_notifybuffer.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_ctrl_d`

- Category: [Other](#category-misc)
- Description: Likely controls con ctrl d.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:150`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_debugoutput`

- Category: [Other](#category-misc)
- Description: Likely controls con debugoutput.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_system.cpp:114`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_midtime`

- Category: [Other](#category-misc)
- Description: Likely controls con midtime.
- Type: `Float`
- Source default: `3.f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/hudmessages.cpp:869`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_notablist`

- Category: [Other](#category-misc)
- Description: Likely controls con notablist.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_tabcomplete.cpp:63`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_notifylines`

- Category: [Other](#category-misc)
- Description: Likely controls con notifylines.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_notifybuffer.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_notifytime`

- Category: [Other](#category-misc)
- Description: Likely controls con notifytime.
- Type: `Float`
- Source default: `3.f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_notifybuffer.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_printansi`

- Category: [Other](#category-misc)
- Description: Likely controls con printansi.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/i_system.cpp:66`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_pulsetext`

- Category: [Other](#category-misc)
- Description: Likely controls con pulsetext.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_notifybuffer.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_quick_home_end`

- Category: [Other](#category-misc)
- Description: Use HOME/END keys to scroll when cursor is at start/end of line already
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:139`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_scale`

- Category: [Other](#category-misc)
- Description: Likely controls con scale.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:128`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_scaletext`

- Category: [Other](#category-misc)
- Description: Likely controls con scaletext.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_notifybuffer.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `con_stackident`

- Category: [Other](#category-misc)
- Description: Likely controls con stackident.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_cmds.cpp:63`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `consoleendoom`

- Category: [Other](#category-misc)
- Description: Likely controls consoleendoom.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/startscreen/endoom.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshair`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshair.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:119`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshair_offset_x`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshair offset x.
- Type: `Float`
- Source default: `0.`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:86`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshair_offset_y`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshair offset y.
- Type: `Float`
- Source default: `0.`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshaircolor`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshaircolor.
- Type: `Color`
- Source default: `0xff0000`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:47`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshaircolorFull`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshaircolorFull.
- Type: `Color`
- Source default: `0x00ff00`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshaircolorMax`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshaircolorMax.
- Type: `Color`
- Source default: `0x7f7fff`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:49`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshaircolors`

- Category: [HUD & Status Bar](#category-hud)
- Description: 0: basic, 1: show health, 2: show health bonus, 3: inverted
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshairforce`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshairforce.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:120`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshairgrow`

- Category: [HUD & Status Bar](#category-hud)
- Description: grow crosshair upon pickup
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshairhascolor`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshairhascolor.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_HIDDEN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshairon`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshairon.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:118`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshairscale`

- Category: [HUD & Status Bar](#category-hud)
- Description: changes the size of the crosshair
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:63`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `crosshairshowshealth`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls crosshairshowshealth.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_HIDDEN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `deathmatch`

- Category: [Gameplay](#category-gameplay)
- Description: Likely controls deathmatch.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO|CVAR_LATCH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:290`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debug_languages`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debug languages.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/stringtable.cpp:39`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debuganimated`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debuganimated.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/gamedata/textures/animations.cpp:189`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_capacity`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace capacity.
- Type: `Int`
- Source default: `16384`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:47`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_enable`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace enable.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:43`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_filter`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace filter.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:44`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_minseverity`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace minseverity.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_stats`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace stats.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_stream`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace stream.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_stream_rotate_count`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace stream rotate count.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `debugtrace_stream_rotate_mb`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls debugtrace stream rotate mb.
- Type: `Int`
- Source default: `10`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/debugtrace.cpp:49`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultaddonfiles`

- Category: [Other](#category-misc)
- Description: Likely controls defaultaddonfiles.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultargs`

- Category: [Other](#category-misc)
- Description: Likely controls defaultargs.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultiwad`

- Category: [Other](#category-misc)
- Description: Likely controls defaultiwad.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetaddress`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetaddress.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetaltdm`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetaltdm.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:66`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetargs`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetargs.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:61`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetextratic`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetextratic.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:72`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetgamemode`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetgamemode.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:65`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnethostport`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnethostport.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:63`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnethostteam`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnethostteam.
- Type: `Int`
- Source default: `255`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetiwad`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetiwad.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetjoinport`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetjoinport.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetjointeam`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetjointeam.
- Type: `Int`
- Source default: `255`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:71`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetpage`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetpage.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetplayers`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetplayers.
- Type: `Int`
- Source default: `8`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetsavefile`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetsavefile.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:73`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `defaultnetticdup`

- Category: [Other](#category-misc)
- Description: Likely controls defaultnetticdup.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dehload`

- Category: [Other](#category-misc)
- Description: Likely controls dehload.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/gamedata/d_dehacked.cpp:3210`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `demo_compress`

- Category: [Other](#category-misc)
- Description: Likely controls demo compress.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:321`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `developer`

- Category: [Other](#category-misc)
- Description: Likely controls developer.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:143`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dimamount`

- Category: [Other](#category-misc)
- Description: Likely controls dimamount.
- Type: `Float`
- Source default: `0.8f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/menu/doommenu.cpp:376`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dimcolor`

- Category: [Other](#category-misc)
- Description: Likely controls dimcolor.
- Type: `Color`
- Source default: `0x000000`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/menu/doommenu.cpp:387`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `disableautoload`

- Category: [Other](#category-misc)
- Description: Likely controls disableautoload.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:510`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `disableautosave`

- Category: [Other](#category-misc)
- Description: Likely controls disableautosave.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:414`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `disablecrashlog`

- Category: [Other](#category-misc)
- Description: Likely controls disablecrashlog.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_main.cpp:626`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `displaynametags`

- Category: [Other](#category-misc)
- Description: Likely controls displaynametags.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:312`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dlg_musicvolume`

- Category: [Other](#category-misc)
- Description: Likely controls dlg musicvolume.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/p_conversation.cpp:218`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dlg_vgafont`

- Category: [Other](#category-misc)
- Description: Likely controls dlg vgafont.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/p_conversation.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dmflags`

- Category: [Other](#category-misc)
- Description: Server setting: Raw DM Flags
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:657`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dmflags2`

- Category: [Other](#category-misc)
- Description: Server setting: Raw DM Flags 2
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:733`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dmflags3`

- Category: [Other](#category-misc)
- Description: Server setting: Raw DM Flags 3
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:803`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `duellimit`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Legacy Skulltag compatibility value for duel limit metadata.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR_NAMED`
- Ref symbol: `duellimit_compat`
- Source: `/workspace/src/d_net_invasion.inl:113`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `dumpspawnedthings`

- Category: [Other](#category-misc)
- Description: Likely controls dumpspawnedthings.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_mobj.cpp:7074`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `eaxedit_test`

- Category: [Other](#category-misc)
- Description: Likely controls eaxedit test.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_reverbedit.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `enablescriptscreenshot`

- Category: [Other](#category-misc)
- Description: Likely controls enablescriptscreenshot.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:295`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_chorus`

- Category: [Other](#category-misc)
- Description: Likely controls fluid chorus.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:161`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_chorus_depth`

- Category: [Other](#category-misc)
- Description: Likely controls fluid chorus depth.
- Type: `Float`
- Source default: `8.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:222`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_chorus_level`

- Category: [Other](#category-misc)
- Description: Likely controls fluid chorus level.
- Type: `Float`
- Source default: `1.2f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:211`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_chorus_speed`

- Category: [Other](#category-misc)
- Description: Likely controls fluid chorus speed.
- Type: `Float`
- Source default: `0.3f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:216`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_chorus_type`

- Category: [Other](#category-misc)
- Description: Likely controls fluid chorus type.
- Type: `Int`
- Source default: `0/*FLUID_CHORUS_DEFAULT_TYPE*/`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:227`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_chorus_voices`

- Category: [Other](#category-misc)
- Description: Likely controls fluid chorus voices.
- Type: `Int`
- Source default: `3`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:206`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_gain`

- Category: [Other](#category-misc)
- Description: Likely controls fluid gain.
- Type: `Float`
- Source default: `0.5`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:142`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_interp`

- Category: [Other](#category-misc)
- Description: Likely controls fluid interp.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:171`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_lib`

- Category: [Other](#category-misc)
- Description: Likely controls fluid lib.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL | CVAR_SYSTEM_ONLY`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:132`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_patchset`

- Category: [Other](#category-misc)
- Description: Likely controls fluid patchset.
- Type: `String`
- Source default: `GAMENAMELOWERCASE`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL | CVAR_SYSTEM_ONLY`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:137`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_reverb`

- Category: [Other](#category-misc)
- Description: Likely controls fluid reverb.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:156`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_reverb_damping`

- Category: [Other](#category-misc)
- Description: Likely controls fluid reverb damping.
- Type: `Float`
- Source default: `0.23f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:191`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_reverb_level`

- Category: [Other](#category-misc)
- Description: Likely controls fluid reverb level.
- Type: `Float`
- Source default: `0.57f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:201`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_reverb_roomsize`

- Category: [Other](#category-misc)
- Description: Likely controls fluid reverb roomsize.
- Type: `Float`
- Source default: `0.61f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:186`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_reverb_width`

- Category: [Other](#category-misc)
- Description: Likely controls fluid reverb width.
- Type: `Float`
- Source default: `0.76f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:196`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_samplerate`

- Category: [Other](#category-misc)
- Description: Likely controls fluid samplerate.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:176`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_threads`

- Category: [Other](#category-misc)
- Description: Likely controls fluid threads.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:181`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fluid_voices`

- Category: [Other](#category-misc)
- Description: Likely controls fluid voices.
- Type: `Int`
- Source default: `128`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:166`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `forcewater`

- Category: [Other](#category-misc)
- Description: Likely controls forcewater.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/p_setup.cpp:765`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fov`

- Category: [Other](#category-misc)
- Description: Likely controls fov.
- Type: `Float`
- Source default: `90.f`
- Source flags: `CVAR_ARCHIVE | CVAR_USERINFO | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:143`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fraglimit`

- Category: [Other](#category-misc)
- Description: Server setting: Frag Limit
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:466`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `freelook`

- Category: [Other](#category-misc)
- Description: Likely controls freelook.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:324`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `fviewbob`

- Category: [Other](#category-misc)
- Description: Likely controls fviewbob.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gamma`

- Category: [Other](#category-misc)
- Description: Likely controls gamma.
- Type: `Float`
- Source default: `GAMMA_DEFAULT`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR_NAMED`
- Ref symbol: `vid_gamma_compat`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:102`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `genblockmap`

- Category: [Other](#category-misc)
- Description: Likely controls genblockmap.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/maploader/maploader.cpp:65`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gender`

- Category: [Other](#category-misc)
- Description: Likely controls gender.
- Type: `String`
- Source default: `"neutral"`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gennodes`

- Category: [Other](#category-misc)
- Description: Likely controls gennodes.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/maploader/maploader.cpp:66`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_aalines`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl aalines.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/hw_draw2d.cpp:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_bandedswlight`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl bandedswlight.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_billboard_faces_camera`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl billboard faces camera.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:82`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_billboard_mode`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl billboard mode.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:81`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_billboard_particles`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl billboard particles.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:84`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_bloom`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl bloom.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:31`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_bloom_amount`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl bloom amount.
- Type: `Float`
- Source default: `1.4f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:32`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_breaksec`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl breaksec.
- Type: `Int`
- Source default: `-1`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_flats.cpp:44`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_brightfog`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl brightfog.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_level.cpp:124`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_cachenodes`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl cachenodes.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:138`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_cachetime`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl cachetime.
- Type: `Float`
- Source default: `0.6f`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:139`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_control_tear`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl control tear.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/gl_sysfb.cpp:103`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_coronas`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl coronas.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_custompost`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl custompost.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess.cpp:901`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_customshader`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl customshader.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hw_material.cpp:28`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_debug`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl debug.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:78`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_debug_breakpoint`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl debug breakpoint.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gl/gl_debug.cpp:39`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_debug_level`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl debug level.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gl/gl_debug.cpp:31`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_distfog`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl distfog.
- Type: `Int`
- Source default: `70`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_lighting.cpp:36`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_dither_bpc`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl dither bpc.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gl/gl_postprocess.cpp:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_enhanced_nightvision`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl enhanced nightvision.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_lighting.cpp:28`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_enhanced_nv_stealth`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl enhanced nv stealth.
- Type: `Int`
- Source default: `3`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_es`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl es.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:82`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_exposure_base`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl exposure base.
- Type: `Float`
- Source default: `0.35f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:39`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_exposure_min`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl exposure min.
- Type: `Float`
- Source default: `0.35f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:38`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_exposure_scale`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl exposure scale.
- Type: `Float`
- Source default: `1.3f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:37`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_exposure_speed`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl exposure speed.
- Type: `Float`
- Source default: `0.05f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_finishbeforeswap`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl finishbeforeswap.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gl/gl_framebuffer.cpp:251`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_fogmode`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl fogmode.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:32`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_fuzztype`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl fuzztype.
- Type: `Int`
- Source default: `8`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_fxaa`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl fxaa.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_interpolate_model_frames`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl interpolate model frames.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_data/models.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_lens`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl lens.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_lens_chromatic`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl lens chromatic.
- Type: `Float`
- Source default: `1.12f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_lens_k`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl lens k.
- Type: `Float`
- Source default: `-0.12f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_lens_kcube`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl lens kcube.
- Type: `Float`
- Source default: `0.1f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_light_models`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl light models.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/hw_models.cpp:36`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_light_particles`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl light particles.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/hw_dynlightdata.cpp:36`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_light_shadowmap`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl light shadowmap.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_light_sprites`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl light sprites.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/hw_dynlightdata.cpp:35`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_lightadditivesurfaces`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl lightadditivesurfaces.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_level.cpp:132`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_lightmode`

- Category: [Renderer](#category-rendering)
- Description: Select lighting mode. 2 is vanilla accurate, 1 is accurate to the ZDoom software renderer and 0 is a less demanding non-shader implementation
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_level.cpp:153`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_lights`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl lights.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:164`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_maplightmode`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl maplightmode.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_NOINITCALL | CVAR_CHEAT`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_level.cpp:148`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_mask_sprite_threshold`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl mask sprite threshold.
- Type: `Float`
- Source default: `0.5f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_mask_threshold`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl mask threshold.
- Type: `Float`
- Source default: `0.5f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_menu_blur`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl menu blur.
- Type: `Float`
- Source default: `-1.0f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:98`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_mirror_envmap`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl mirror envmap.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_mirrors`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl mirrors.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_multisample`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl multisample.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gl/gl_renderbuffers.cpp:38`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_multithread`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl multithread.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:44`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_no_skyclear`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl no skyclear.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:47`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_noskyboxes`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl noskyboxes.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sky.cpp:33`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_notexturefill`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl notexturefill.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_level.cpp:140`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_paltonemap_powtable`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl paltonemap powtable.
- Type: `Float`
- Source default: `2.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:88`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_paltonemap_reverselookup`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl paltonemap reverselookup.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:93`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_particles_style`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl particles style.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:80`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_pipeline_depth`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl pipeline depth.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_plane_reflection`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl plane reflection.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_portals`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl portals.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_precache`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl precache.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:197`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_render_flats`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl render flats.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:248`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_render_things`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl render things.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:246`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_render_walls`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl render walls.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_bsp.cpp:247`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_satformula`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl satformula.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:179`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_scale_viewport`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl scale viewport.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_framebuffer.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_sclipfactor`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl sclipfactor.
- Type: `Float`
- Source default: `1.8f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:79`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_sclipthreshold`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl sclipthreshold.
- Type: `Float`
- Source default: `10.0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:78`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_seamless`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl seamless.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:43`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_shadowmap_filter`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl shadowmap filter.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:200`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_shadowmap_maxlights`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl shadowmap maxlights.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:296`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_shadowmap_prioritize`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl shadowmap prioritize.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_shadowmap_quality`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl shadowmap quality.
- Type: `Int`
- Source default: `512`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:315`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_sort_textures`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl sort textures.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_sprite_blend`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl sprite blend.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_spriteclip`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl spriteclip.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao_bias`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao bias.
- Type: `Float`
- Source default: `0.2f`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao_blur`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao blur.
- Type: `Float`
- Source default: `16.0f`
- Source flags: `0`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:78`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao_debug`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao debug.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao_exponent`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao exponent.
- Type: `Float`
- Source default: `1.8f`
- Source flags: `0`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:83`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao_portals`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao portals.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao_radius`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao radius.
- Type: `Float`
- Source default: `80.0f`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:77`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_ssao_strength`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl ssao strength.
- Type: `Float`
- Source default: `0.7f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_drawinfo.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_filter`

- Category: [Renderer](#category-rendering)
- Description: changes the texture filtering settings
- Type: `Int`
- Source default: `6`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:191`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_filter_anisotropic`

- Category: [Renderer](#category-rendering)
- Description: changes the OpenGL texture anisotropy setting
- Type: `Float`
- Source default: `16.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:186`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_fonts`

- Category: [Renderer](#category-rendering)
- Description: Flag alias backed by gl_texture_hqresize_targets.
- Type: `Flag`
- Source default: `gl_texture_hqresize_targets`
- Source flags: `4`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_maxinputsize`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture hqresize maxinputsize.
- Type: `Int`
- Source default: `512`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_mt_height`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture hqresize mt height.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_mt_width`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture hqresize mt width.
- Type: `Int`
- Source default: `16`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:81`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_multithread`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture hqresize multithread.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:79`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_skins`

- Category: [Renderer](#category-rendering)
- Description: Flag alias backed by gl_texture_hqresize_targets.
- Type: `Flag`
- Source default: `gl_texture_hqresize_targets`
- Source flags: `8`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:77`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_sprites`

- Category: [Renderer](#category-rendering)
- Description: Flag alias backed by gl_texture_hqresize_targets.
- Type: `Flag`
- Source default: `gl_texture_hqresize_targets`
- Source flags: `2`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_targets`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture hqresize targets.
- Type: `Int`
- Source default: `15`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresize_textures`

- Category: [Renderer](#category-rendering)
- Description: Flag alias backed by gl_texture_hqresize_targets.
- Type: `Flag`
- Source default: `gl_texture_hqresize_targets`
- Source flags: `1`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresizemode`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture hqresizemode.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_texture_hqresizemult`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl texture hqresizemult.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_tonemap`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl tonemap.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/postprocessing/hw_postprocess_cvars.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_usecolorblending`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl usecolorblending.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:73`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_weapon_purelightlevel`

- Category: [Renderer](#category-rendering)
- Description: Makes the lighting on weapon sprites (or models) purely match the sector's light level you're standing in
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_weapon.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gl_weaponlight`

- Category: [Renderer](#category-rendering)
- Description: Likely controls gl weaponlight.
- Type: `Int`
- Source default: `8`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_lighting.cpp:27`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gles_force_glsl_v100`

- Category: [Other](#category-misc)
- Description: Likely controls gles force glsl v100.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gles/gles_system.cpp:24`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gles_glsl_precision`

- Category: [Other](#category-misc)
- Description: Likely controls gles glsl precision.
- Type: `Int`
- Source default: `2`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gles/gles_shader.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gles_max_lights_per_surface`

- Category: [Other](#category-misc)
- Description: Likely controls gles max lights per surface.
- Type: `Int`
- Source default: `32`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gles/gles_system.cpp:25`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gles_use_mapped_buffer`

- Category: [Other](#category-misc)
- Description: Likely controls gles use mapped buffer.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/gles/gles_system.cpp:23`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gme_stereodepth`

- Category: [Other](#category-misc)
- Description: Likely controls gme stereodepth.
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:502`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gus_memsize`

- Category: [Other](#category-misc)
- Description: Likely controls gus memsize.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:361`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `gus_patchdir`

- Category: [Other](#category-misc)
- Description: Likely controls gus patchdir.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL | CVAR_SYSTEM_ONLY`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:351`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_compat`

- Category: [Other](#category-misc)
- Description: haptic feedback compatibility level
- Type: `Int`
- Source default: `HAPTCOMPAT_MATCH`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:140`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_debug`

- Category: [Other](#category-misc)
- Description: print diagnostics for haptic feedback
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:138`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_do_action`

- Category: [Other](#category-misc)
- Description: allow haptic feedback for player doing things
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:149`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_do_damage`

- Category: [Other](#category-misc)
- Description: allow haptic feedback for things hurting player
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:148`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_do_menus`

- Category: [Other](#category-misc)
- Description: allow haptic feedback for menus
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:146`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_do_world`

- Category: [Other](#category-misc)
- Description: allow haptic feedback for things acting on player
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:147`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_strength`

- Category: [Other](#category-misc)
- Description: Translate linear haptics to audio taper
- Type: `Int`
- Source default: `10`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:119`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_strength_hf`

- Category: [Other](#category-misc)
- Description: high frequency motor fine-control
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:103`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_strength_lf`

- Category: [Other](#category-misc)
- Description: low frequency motor fine-control
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:98`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_strength_lt`

- Category: [Other](#category-misc)
- Description: left trigger motor fine-control
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:108`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `haptics_strength_rt`

- Category: [Other](#category-misc)
- Description: right trigger motor fine-control
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_haptics.cpp:113`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_hud_debug`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Mirror net diagnostics to the HUD console for live operator visibility.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:185`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_k8vavoom_lighting_profile`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Selects a composed K8vavoom lighting preset (0=off, 1+=profile id) and applies bundled renderer toggles.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:214`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_k8vavoom_raylight_probe`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Enable ray-light probing hooks used by K8vavoom-style lighting profile diagnostics.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:212`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_k8vavoom_shadow_boost`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Apply stronger shadow-map defaults when a K8vavoom lighting profile is active.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:211`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_lag_hud`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Persistent on-screen lag/invasion overlay (top-left). Also enable with `stat hcde_lag`.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:197`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_nanobsp_loader`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Selects NanoBSP loader mode for map geometry ingestion (0=off, 1=on, 2=force).
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_nanobsp_loader.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Adaptively reduce shadow-casting light count to stay near the target shadow-map frame budget.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget_minlights`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Minimum number of shadow-casting lights retained while auto-budget throttles the light count.
- Type: `Int`
- Source default: `64`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:280`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget_step`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Number of shadow-casting lights removed or restored per auto-budget adjustment step.
- Type: `Int`
- Source default: `32`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:288`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_shadow_autobudget_targetms`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Target milliseconds per frame allocated to shadow-map rendering when auto-budget is enabled.
- Type: `Float`
- Source default: `1.20f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:272`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_shadow_autofallback`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Automatically disable shadow maps when the renderer reports unsupported or failing shadow-map paths.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_shadowmap.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_shadow_forcealllights`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: Force eligible dynamic lights onto the shadow-map path even when not explicitly marked shadowmapped.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/hw_entrypoint.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_shadowprofile`

- Category: [HCDE Rendering](#category-hcde-rendering)
- Description: applies HCDE grouped shadow settings. 0 = manual, 1 = off, 2 = performance, 3 = balanced, 4 = enhanced, 5 = cinematic, 6 = quake-style, 7 = doom3-style
- Type: `Int`
- Source default: `HCDE_SHADOWPROFILE_DOOM3`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/menu/doommenu.cpp:802`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hcde_startup_profile`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Emit startup timing profile data for engine initialization diagnostics.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/scripting/thingdef.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_althud`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud althud.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:47`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_althud_forceinternal`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud althud forceinternal.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:97`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_althudscale`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud althudscale.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_ammo_order`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud ammo order.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_ammo_red`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud ammo red.
- Type: `Int`
- Source default: `25`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:65`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_ammo_yellow`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud ammo yellow.
- Type: `Int`
- Source default: `50`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:66`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_armor_green`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud armor green.
- Type: `Int`
- Source default: `100`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:73`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_armor_red`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud armor red.
- Type: `Int`
- Source default: `25`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:71`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_armor_yellow`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud armor yellow.
- Type: `Int`
- Source default: `50`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:72`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_aspectscale`

- Category: [HUD & Status Bar](#category-hud)
- Description: enables aspect ratio correction for the status bar
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:73`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_berserk_health`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud berserk health.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_health_green`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud health green.
- Type: `Int`
- Source default: `100`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_health_red`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud health red.
- Type: `Int`
- Source default: `25`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_health_yellow`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud health yellow.
- Type: `Int`
- Source default: `50`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_oldscale`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud oldscale.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_scale`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud scale.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:83`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_scalefactor`

- Category: [HUD & Status Bar](#category-hud)
- Description: changes the hud scale
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/statusbar/base_sbar.cpp:66`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showammo`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showammo.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showangles`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showangles.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showitems`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showitems.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showlag`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showlag.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showmonsters`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showmonsters.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showscore`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showscore.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showsecrets`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showsecrets.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showstats`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showstats.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showtime`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showtime.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showtimestat`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showtimestat.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_showweapons`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud showweapons.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_swaphealtharmor`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud swaphealtharmor.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_timecolor`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud timecolor.
- Type: `Int`
- Source default: `CR_GOLD`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:61`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hud_toggled`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls hud toggled.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:592`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hudcolor_ltim`

- Category: [Other](#category-misc)
- Description: Likely controls hudcolor ltim.
- Type: `Int`
- Source default: `CR_ORANGE`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:80`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hudcolor_statnames`

- Category: [Other](#category-misc)
- Description: Likely controls hudcolor statnames.
- Type: `Int`
- Source default: `CR_RED`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:84`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hudcolor_stats`

- Category: [Other](#category-misc)
- Description: Likely controls hudcolor stats.
- Type: `Int`
- Source default: `CR_GREEN`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hudcolor_time`

- Category: [Other](#category-misc)
- Description: Likely controls hudcolor time.
- Type: `Int`
- Source default: `CR_RED`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:79`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hudcolor_titl`

- Category: [Other](#category-misc)
- Description: Likely controls hudcolor titl.
- Type: `Int`
- Source default: `CR_YELLOW`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:78`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hudcolor_ttim`

- Category: [Other](#category-misc)
- Description: Likely controls hudcolor ttim.
- Type: `Int`
- Source default: `CR_GOLD`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:81`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hudcolor_xyco`

- Category: [Other](#category-misc)
- Description: Likely controls hudcolor xyco.
- Type: `Int`
- Source default: `CR_GREEN`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:82`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hw_2dmip`

- Category: [Renderer](#category-rendering)
- Description: Likely controls hw 2dmip.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/hw_draw2d.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hw_force_cambbpref`

- Category: [Renderer](#category-rendering)
- Description: Likely controls hw force cambbpref.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:83`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `hw_npottest`

- Category: [Renderer](#category-rendering)
- Description: Likely controls hw npottest.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:202`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_discordrpc`

- Category: [Other](#category-misc)
- Description: Likely controls i discordrpc.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:529`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_display_new_release`

- Category: [Other](#category-misc)
- Description: Show changelog upon update
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_iwad.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_exit_on_not_found`

- Category: [Other](#category-misc)
- Description: Exits game if a specified file is not found
- Type: `Int`
- Source default: `REQUIRE_DEFAULT`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/utility/findfile.cpp:38`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `I_FriendlyWindowTitle`

- Category: [Other](#category-misc)
- Description: Likely controls I FriendlyWindowTitle.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:535`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_is_new_release`

- Category: [Other](#category-misc)
- Description: Likely controls i is new release.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_HIDDEN`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_iwad.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_loadsupportwad`

- Category: [Other](#category-misc)
- Description: Load id24.wad
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_iwad.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_pauseinbackground`

- Category: [Other](#category-misc)
- Description: Likely controls i pauseinbackground.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_sound.cpp:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_searchdistributors`

- Category: [Other](#category-misc)
- Description: Search storefront intallations for IWADS
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_iwad.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_soundinbackground`

- Category: [Other](#category-misc)
- Description: Likely controls i soundinbackground.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_sound.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `i_timescale`

- Category: [Other](#category-misc)
- Description: Likely controls i timescale.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_NOINITCALL | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:422`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `idmypos`

- Category: [Other](#category-misc)
- Description: Likely controls idmypos.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:126`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `in_mouse`

- Category: [Input](#category-input)
- Description: Likely controls in mouse.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_mouse.cpp:157`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `infighting`

- Category: [Other](#category-misc)
- Description: Likely controls infighting.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/gamedata/d_dehacked.cpp:63`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `inter_classic_scaling`

- Category: [Other](#category-misc)
- Description: Likely controls inter classic scaling.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/intermission/intermission.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `inter_subtitles`

- Category: [Other](#category-misc)
- Description: Likely controls inter subtitles.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/cutscenes/screenjob.cpp:43`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `invertmouse`

- Category: [Other](#category-misc)
- Description: Likely controls invertmouse.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/d_event.cpp:47`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `invertmousex`

- Category: [Other](#category-misc)
- Description: Likely controls invertmousex.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/d_event.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_axespolling`

- Category: [Input](#category-input)
- Description: Likely controls joy axespolling.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/cocoa/i_joystick.cpp:1356`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_dinput`

- Category: [Input](#category-input)
- Description: Likely controls joy dinput.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_dijoy.cpp:285`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_gyro_deadzone`

- Category: [Input](#category-input)
- Description: Likely controls joy gyro deadzone.
- Type: `Float`
- Source default: `0.05f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_gyro.cpp:162`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_gyro_enable`

- Category: [Input](#category-input)
- Description: Likely controls joy gyro enable.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_gyro.cpp:151`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_gyro_invertpitch`

- Category: [Input](#category-input)
- Description: Likely controls joy gyro invertpitch.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_gyro.cpp:171`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_gyro_invertyaw`

- Category: [Input](#category-input)
- Description: Likely controls joy gyro invertyaw.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_gyro.cpp:170`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_gyro_mode`

- Category: [Input](#category-input)
- Description: Likely controls joy gyro mode.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_gyro.cpp:167`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_gyro_pitchscale`

- Category: [Input](#category-input)
- Description: Likely controls joy gyro pitchscale.
- Type: `Float`
- Source default: `2.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_gyro.cpp:158`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_gyro_yawscale`

- Category: [Input](#category-input)
- Description: Likely controls joy gyro yawscale.
- Type: `Float`
- Source default: `2.5f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_gyro.cpp:157`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_ps2raw`

- Category: [Input](#category-input)
- Description: Likely controls joy ps2raw.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_rawps2.cpp:234`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joy_xinput`

- Category: [Input](#category-input)
- Description: Likely controls joy xinput.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_xinput.cpp:203`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `joykey_stop_conflict`

- Category: [Other](#category-misc)
- Description: Detect joypad/keyboard conflicts, dropping events as needed. Useful for handheld PCs such as the SteamDeck. -1: auto-detect, 0: disabled, 1: detected, 2: forced
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/i_input.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `k_allowfullscreentoggle`

- Category: [Other](#category-misc)
- Description: Likely controls k allowfullscreentoggle.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_input.cpp:120`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `k_mergekeys`

- Category: [Other](#category-misc)
- Description: Likely controls k mergekeys.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_keyboard.cpp:113`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `language`

- Category: [Other](#category-misc)
- Description: Likely controls language.
- Type: `String`
- Source default: `"auto"`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:81`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `language_debug_maxlen`

- Category: [Other](#category-misc)
- Description: Likely controls language debug maxlen.
- Type: `Int`
- Source default: `64`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/stringtable.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `log_vgafont`

- Category: [Other](#category-misc)
- Description: Likely controls log vgafont.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:84`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `longsavemessages`

- Category: [Other](#category-misc)
- Description: Likely controls longsavemessages.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:293`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `lookspring`

- Category: [Other](#category-misc)
- Description: Likely controls lookspring.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:3318`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `lookstrafe`

- Category: [Other](#category-misc)
- Description: Likely controls lookstrafe.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:325`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_blockcontrollers`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m blockcontrollers.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_cleanscale`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m cleanscale.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_forward`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m forward.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:326`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_hidepointer`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m hidepointer.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_mouse.cpp:155`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_pitch`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m pitch.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:619`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_quickexit`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m quickexit.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/messagebox.cpp:34`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_sensitivity_x`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m sensitivity x.
- Type: `Float`
- Source default: `2.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/d_event.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_sensitivity_y`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m sensitivity y.
- Type: `Float`
- Source default: `2.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/d_event.cpp:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_show_backbutton`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m show backbutton.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_showinputgrid`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m showinputgrid.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_side`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m side.
- Type: `Float`
- Source default: `2.f`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:327`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_simpleoptions`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m simpleoptions.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/menu/doommenu.cpp:96`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_simpleoptions_view`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m simpleoptions view.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/menu/doommenu.cpp:97`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_smooth_curve`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m smooth curve.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_feel.cpp:20`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_swapbuttons`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m swapbuttons.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/i_mouse.cpp:391`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_tooltip_alpha`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m tooltip alpha.
- Type: `Float`
- Source default: `0.6f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_tooltip_capratio`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m tooltip capratio.
- Type: `Float`
- Source default: `4.0/3.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_tooltip_delay`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m tooltip delay.
- Type: `Float`
- Source default: `9.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_tooltip_lines`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m tooltip lines.
- Type: `Int`
- Source default: `3`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_tooltip_small`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m tooltip small.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_tooltip_speed`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m tooltip speed.
- Type: `Float`
- Source default: `3.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:80`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_use_mouse`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m use mouse.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `m_yaw`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls m yaw.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:620`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `map_point_coordinates`

- Category: [Other](#category-misc)
- Description: Likely controls map point coordinates.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_hud.cpp:88`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `maxviewpitch`

- Category: [Other](#category-misc)
- Description: Likely controls maxviewpitch.
- Type: `Float`
- Source default: `90.f`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:1279`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `menu_overscroll`

- Category: [Menu & UI](#category-menu)
- Description: Number of lines you can scroll past the bottom of a menu
- Type: `Int`
- Source default: `8`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/optionmenu.cpp:32`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `menu_resolution_custom_height`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls menu resolution custom height.
- Type: `Int`
- Source default: `480`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/resolutionmenu.cpp:31`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `menu_resolution_custom_width`

- Category: [Menu & UI](#category-menu)
- Description: Likely controls menu resolution custom width.
- Type: `Int`
- Source default: `640`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/resolutionmenu.cpp:30`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `midi_config`

- Category: [Other](#category-misc)
- Description: Likely controls midi config.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL | CVAR_SYSTEM_ONLY`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:341`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `midi_dmxgus`

- Category: [Other](#category-misc)
- Description: Likely controls midi dmxgus.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:346`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `midi_voices`

- Category: [Other](#category-misc)
- Description: Likely controls midi voices.
- Type: `Int`
- Source default: `32`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:356`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `midskew`

- Category: [Other](#category-misc)
- Description: Likely controls midskew.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:2130`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_autochip`

- Category: [Other](#category-misc)
- Description: Likely controls mod autochip.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:544`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_autochip_scan_threshold`

- Category: [Other](#category-misc)
- Description: Likely controls mod autochip scan threshold.
- Type: `Int`
- Source default: `12`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:559`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_autochip_size_force`

- Category: [Other](#category-misc)
- Description: Likely controls mod autochip size force.
- Type: `Int`
- Source default: `100`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:549`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_autochip_size_scan`

- Category: [Other](#category-misc)
- Description: Likely controls mod autochip size scan.
- Type: `Int`
- Source default: `500`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:554`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_dumb_mastervolume`

- Category: [Other](#category-misc)
- Description: Likely controls mod dumb mastervolume.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:564`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_interp`

- Category: [Other](#category-misc)
- Description: Likely controls mod interp.
- Type: `Int`
- Source default: `2/*DUMB_LQ_CUBIC*/`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:539`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_preferred_player`

- Category: [Other](#category-misc)
- Description: Likely controls mod preferred player.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_samplerate`

- Category: [Other](#category-misc)
- Description: Likely controls mod samplerate.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:524`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mod_volramp`

- Category: [Other](#category-misc)
- Description: Likely controls mod volramp.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:534`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mouse_capturemode`

- Category: [Other](#category-misc)
- Description: Likely controls mouse capturemode.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:3321`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `movebob`

- Category: [Other](#category-misc)
- Description: Likely controls movebob.
- Type: `Float`
- Source default: `0.25f`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msg`

- Category: [Other](#category-misc)
- Description: Filters HUD message by importance
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVARD_NAMED`
- Ref symbol: `msglevel`
- Source: `/workspace/src/common/console/c_console.cpp:182`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msg0color`

- Category: [Other](#category-misc)
- Description: Likely controls msg0color.
- Type: `Int`
- Source default: `CR_UNTRANSLATED`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:184`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msg1color`

- Category: [Other](#category-misc)
- Description: Likely controls msg1color.
- Type: `Int`
- Source default: `CR_GOLD`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:189`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msg2color`

- Category: [Other](#category-misc)
- Description: Likely controls msg2color.
- Type: `Int`
- Source default: `CR_GRAY`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:194`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msg3color`

- Category: [Other](#category-misc)
- Description: Likely controls msg3color.
- Type: `Int`
- Source default: `CR_GREEN`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:199`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msg4color`

- Category: [Other](#category-misc)
- Description: Likely controls msg4color.
- Type: `Int`
- Source default: `CR_GREEN`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:204`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msgmidcolor`

- Category: [Other](#category-misc)
- Description: Likely controls msgmidcolor.
- Type: `Int`
- Source default: `CR_UNTRANSLATED`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:209`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `msgmidcolor2`

- Category: [Other](#category-misc)
- Description: Likely controls msgmidcolor2.
- Type: `Int`
- Source default: `CR_BROWN`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/console/c_console.cpp:214`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mus_calcgain`

- Category: [Music](#category-music)
- Description: Likely controls mus calcgain.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mus_enabled`

- Category: [Music](#category-music)
- Description: enables/disables music
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/i_music.cpp:86`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `mus_usereplaygain`

- Category: [Music](#category-music)
- Description: Likely controls mus usereplaygain.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music.cpp:86`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `name`

- Category: [Other](#category-misc)
- Description: descr
- Type: `String`
- Source default: `"Player"`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `nametagcolor`

- Category: [Other](#category-misc)
- Description: Likely controls nametagcolor.
- Type: `Int`
- Source default: `CR_GOLD`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:320`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_adaptive_lead`

- Category: [Other](#category-misc)
- Description: Likely controls adaptive lead behavior for network.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_movement_diag.cpp:44`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_adaptive_lead_guard`

- Category: [Other](#category-misc)
- Description: Likely controls adaptive lead guard behavior for network.
- Type: `Int`
- Source default: `3`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_movement_diag.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_adaptive_lead_max`

- Category: [Other](#category-misc)
- Description: Likely controls adaptive lead max behavior for network.
- Type: `Int`
- Source default: `6`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_movement_diag.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_adaptive_lead_min`

- Category: [Other](#category-misc)
- Description: Likely controls adaptive lead min behavior for network.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_movement_diag.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_blackbox_record`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls blackbox record behavior for network.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_blackbox.cpp:35`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_blackbox_size_mb`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls blackbox size mb behavior for network.
- Type: `Int`
- Source default: `32`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_blackbox.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_chatslowmode`

- Category: [Other](#category-misc)
- Description: Server setting: Chat Slowmode
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/ct_chat.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_checksum`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls checksum behavior for network.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_checksum.cpp:34`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_checksum_categories`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls checksum categories behavior for network.
- Type: `Int`
- Source default: `0x3F`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_checksum.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_checksum_interval`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls checksum interval behavior for network.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_checksum.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_cutscenecountdown`

- Category: [Other](#category-misc)
- Description: Server setting: Ready Time
- Type: `Float`
- Source default: `30.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_cutscenereadypercent`

- Category: [Other](#category-misc)
- Description: Server setting: Ready Percent
- Type: `Float`
- Source default: `0.5f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_cutscenereadytype`

- Category: [Other](#category-misc)
- Description: Server setting: Ready Mode
- Type: `Int`
- Source default: `RT_VOTE`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_desyncdebug`

- Category: [Other](#category-misc)
- Description: Likely controls desyncdebug behavior for network.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:214`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_disablepause`

- Category: [Other](#category-misc)
- Description: Server setting: Pause Policy
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_echo_debug`

- Category: [Other](#category-misc)
- Description: Likely controls echo debug behavior for network.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:226`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_event_debug`

- Category: [Other](#category-misc)
- Description: Likely controls event debug behavior for network.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_diagnostics.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_extratic`

- Category: [Other](#category-misc)
- Description: Likely controls extratic behavior for network.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_hcde_native_only`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Requires HCDE-native networking/capability paths for multiplayer sessions.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:312`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_limitconversations`

- Category: [Other](#category-misc)
- Description: Likely controls limitconversations behavior for network.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_limitsaves`

- Category: [Other](#category-misc)
- Description: Likely controls limitsaves behavior for network.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:43`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_movement_debug`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls movement debug behavior for network.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_movement_diag.cpp:33`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_password`

- Category: [Other](#category-misc)
- Description: Likely controls password behavior for network.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_IGNORE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_net.cpp:1029`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_predict_debug`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Controls HCDE prediction diagnostics: off, CSV sampling, and/or on-screen/debug trace output depending on level.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:211`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_predict_debug_interval`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Tic interval used by prediction CSV/debug sampling.
- Type: `Int`
- Source default: `15`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:219`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_predict_softwarn_ack_lag`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Soft warning threshold for client ack lag during prediction diagnostics.
- Type: `Int`
- Source default: `3`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:268`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_predict_softwarn_mirror_delta`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Soft warning threshold for invasion mirror drift during prediction diagnostics.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:295`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_predict_softwarn_passive_storm`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Soft warning threshold for passive update storms during prediction diagnostics.
- Type: `Int`
- Source default: `5`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:303`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_reconcile_debug`

- Category: [Other](#category-misc)
- Description: Likely controls reconcile debug behavior for network.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:235`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_repeatableactioncooldown`

- Category: [Other](#category-misc)
- Description: Likely controls repeatableactioncooldown behavior for network.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:44`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_rewind_depth`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls rewind depth behavior for network.
- Type: `Int`
- Source default: `10`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rewind.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_rewind_enable`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls rewind enable behavior for network.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rewind.cpp:61`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_rewind_interval`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls rewind interval behavior for network.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rewind.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_rewind_max_mb`

- Category: [HCDE Netcode & Diagnostics](#category-hcde-netcode)
- Description: Likely controls rewind max mb behavior for network.
- Type: `Int`
- Source default: `32`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rewind.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_self_test_run_client`

- Category: [Other](#category-misc)
- Description: Likely controls self test run client behavior for network.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:242`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `net_ticbalance`

- Category: [Other](#category-misc)
- Description: Likely controls ticbalance behavior for network.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `neverswitchonpickup`

- Category: [Other](#category-misc)
- Description: Likely controls neverswitchonpickup.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `nocheats`

- Category: [Other](#category-misc)
- Description: Likely controls nocheats.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/st_stuff.cpp:297`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `nointerscrollabort`

- Category: [Other](#category-misc)
- Description: Likely controls nointerscrollabort.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/intermission/intermission.cpp:72`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `nomonsterinterpolation`

- Category: [Other](#category-misc)
- Description: Likely controls nomonsterinterpolation.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_enemy.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opl_core`

- Category: [Other](#category-misc)
- Description: Likely controls opl core.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:244`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opl_fullpan`

- Category: [Other](#category-misc)
- Description: Likely controls opl fullpan.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:249`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opl_gain`

- Category: [Other](#category-misc)
- Description: Likely controls opl gain.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:254`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opl_numchips`

- Category: [Other](#category-misc)
- Description: Likely controls opl numchips.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:239`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_auto_arpeggio`

- Category: [Other](#category-misc)
- Description: Likely controls opn auto arpeggio.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:316`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_chan_alloc`

- Category: [Other](#category-misc)
- Description: Likely controls opn chan alloc.
- Type: `Int`
- Source default: `-1 /*OPNMIDI_ChanAlloc_AUTO*/`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:311`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_chips_count`

- Category: [Other](#category-misc)
- Description: Likely controls opn chips count.
- Type: `Int`
- Source default: `8`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:276`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_custom_bank`

- Category: [Other](#category-misc)
- Description: Likely controls opn custom bank.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:301`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_emulator_id`

- Category: [Other](#category-misc)
- Description: Likely controls opn emulator id.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:281`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_fullpan`

- Category: [Other](#category-misc)
- Description: Likely controls opn fullpan.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:291`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_gain`

- Category: [Other](#category-misc)
- Description: Likely controls opn gain.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:321`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_run_at_pcm_rate`

- Category: [Other](#category-misc)
- Description: Likely controls opn run at pcm rate.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:286`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_use_custom_bank`

- Category: [Other](#category-misc)
- Description: Likely controls opn use custom bank.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:296`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `opn_volume_model`

- Category: [Other](#category-misc)
- Description: Likely controls opn volume model.
- Type: `Int`
- Source default: `0 /*OPNMIDI_VolumeModel_AUTO*/`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:306`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `os_isanyof`

- Category: [Other](#category-misc)
- Description: Likely controls os isanyof.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `paletteflash`

- Category: [Other](#category-misc)
- Description: Likely controls paletteflash.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:93`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `pf_hazard`

- Category: [Other](#category-misc)
- Description: Flag alias backed by paletteflash.
- Type: `Flag`
- Source default: `paletteflash`
- Source flags: `PF_HAZARD`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:97`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `pf_hexenweaps`

- Category: [Other](#category-misc)
- Description: Flag alias backed by paletteflash.
- Type: `Flag`
- Source default: `paletteflash`
- Source flags: `PF_HEXENWEAPONS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:94`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `pf_ice`

- Category: [Other](#category-misc)
- Description: Flag alias backed by paletteflash.
- Type: `Flag`
- Source default: `paletteflash`
- Source flags: `PF_ICE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:96`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `pf_poison`

- Category: [Other](#category-misc)
- Description: Flag alias backed by paletteflash.
- Type: `Flag`
- Source default: `paletteflash`
- Source flags: `PF_POISON`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:95`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `pickup_fade_scalar`

- Category: [Other](#category-misc)
- Description: Likely controls pickup fade scalar.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/2d/v_blend.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `playerclass`

- Category: [Gameplay](#category-gameplay)
- Description: Likely controls playerclass.
- Type: `String`
- Source default: `"Fighter"`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `png_gamma`

- Category: [Other](#category-misc)
- Description: Likely controls png gamma.
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/m_png.cpp:107`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `png_level`

- Category: [Other](#category-misc)
- Description: Likely controls png level.
- Type: `Int`
- Source default: `5`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/m_png.cpp:100`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `powerup_fade_scalar`

- Category: [Other](#category-misc)
- Description: Likely controls powerup fade scalar.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/2d/v_blend.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `queryiwad`

- Category: [Other](#category-misc)
- Description: Likely controls queryiwad.
- Type: `Bool`
- Source default: `QUERYIWADDEFAULT`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `queryiwad_key`

- Category: [Other](#category-misc)
- Description: Likely controls queryiwad key.
- Type: `String`
- Source default: `"shift"`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/i_system.cpp:65`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `quicksavenum`

- Category: [Other](#category-misc)
- Description: Likely controls quicksavenum.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_NOSET|CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:423`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `quicksaverotation`

- Category: [Other](#category-misc)
- Description: Likely controls quicksaverotation.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:424`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `quicksaverotationcount`

- Category: [Other](#category-misc)
- Description: Likely controls quicksaverotationcount.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:426`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_3dfloors`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r 3dfloors.
- Type: `Int`
- Source default: `1`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_3dfloors.cpp:35`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_actorspriteshadow`

- Category: [Renderer](#category-rendering)
- Description: render actor sprite shadows. 0 = off, 1 = default, 2 = always on
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:108`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_actorspriteshadowalpha`

- Category: [Renderer](#category-rendering)
- Description: maximum sprite shadow opacity, only effective with hardware renderers (0.0 = fully transparent, 1.0 = opaque)
- Type: `Float`
- Source default: `0.7`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:122`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_actorspriteshadowdist`

- Category: [Renderer](#category-rendering)
- Description: how far sprite shadows should be rendered
- Type: `Float`
- Source default: `2200.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:115`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_actorspriteshadowfadeheight`

- Category: [Renderer](#category-rendering)
- Description: distance over which sprite shadows should fade, only effective with hardware renderers (0 = infinite)
- Type: `Float`
- Source default: `0.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:129`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_actorspriteshadowstyle`

- Category: [Renderer](#category-rendering)
- Description: actor sprite shadow style. 0 = classic, 1 = quake-style, 2 = doom3-style
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:136`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_blendmethod`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r blendmethod.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/drawers/r_draw_pal.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_clearbuffer`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r clearbuffer.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:97`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_crosshair_recoil`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r crosshair recoil.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_feel.cpp:29`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_deathcamera`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r deathcamera.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:96`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_debug_disable_vis_filter`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r debug disable vis filter.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:525`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_debug_draw`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r debug draw.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_thread.cpp:34`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_debug_nolimitanamorphoses`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r debug nolimitanamorphoses.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_dithertransparency`

- Category: [Renderer](#category-rendering)
- Description: Use dithered-transparency shading for actor-occluding level geometry
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO | CVAR_CHEAT`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:101`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_drawfuzz`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r drawfuzz.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/renderstyle.cpp:31`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_drawmirrors`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r drawmirrors.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/line/r_line.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_drawplayersprites`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r drawplayersprites.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:99`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_drawtrans`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r drawtrans.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/renderstyle.cpp:30`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_drawvoxels`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r drawvoxels.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:98`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_dynlights`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r dynlights.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/drawers/r_draw.cpp:49`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_extralight`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r extralight.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:354`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_fakecontrast`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r fakecontrast.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_sectors.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_fakeradio`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r fakeradio.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_doomsday_features.cpp:19`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_fakeradio_strength`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r fakeradio strength.
- Type: `Float`
- Source default: `0.5f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_doomsday_features.cpp:21`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_fogboundary`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r fogboundary.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/line/r_line.cpp:61`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_fullbright_overrides`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r fullbright overrides.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:519`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_fullbrightignoresectorcolor`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r fullbrightignoresectorcolor.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_translucent_pass.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_fuzzscale`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r fuzzscale.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/drawers/r_draw.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_geom_ao`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r geom ao.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_doomsday_features.cpp:28`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_geom_ao_strength`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r geom ao strength.
- Type: `Float`
- Source default: `0.4f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_doomsday_features.cpp:30`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_highlight_portals`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r highlight portals.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_portal.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_killfeed`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r killfeed.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_feel.cpp:30`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_line_distance_cull`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r line distance cull.
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_opaque_pass.cpp:95`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_linearsky`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r linearsky.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/plane/r_skyplane.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_lod_bias`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r lod bias.
- Type: `Float`
- Source default: `-1.5`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_magfilter`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r magfilter.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:61`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_maxparticles`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r maxparticles.
- Type: `Int`
- Source default: `4000`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:218`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_minfilter`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r minfilter.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_mipmap`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r mipmap.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/drawers/r_draw_rgba.cpp:67`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_model_distance_cull`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r model distance cull.
- Type: `Float`
- Source default: `1024.f`
- Source flags: `0/*CVAR_ARCHIVE | CVAR_GLOBALCONFIG*/`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_opaque_pass.cpp:107`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_models`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r models.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_scene.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_multithreaded`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r multithreaded.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_thread.cpp:33`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_noaccel`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r noaccel.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/things/r_playersprite.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_particles`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r particles.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_effect.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_portal_recursions`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r portal recursions.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_quakeintensity`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r quakeintensity.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:102`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_radarclipper`

- Category: [Renderer](#category-rendering)
- Description: Use the horizontal clipper from camera->tracer's perspective
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO | CVAR_CHEAT`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:100`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_rail_smartspiral`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r rail smartspiral.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_effect.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_rail_spiralsparsity`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r rail spiralsparsity.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_effect.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_rail_trailsparsity`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r rail trailsparsity.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_effect.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_scene_multithreaded`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r scene multithreaded.
- Type: `Int`
- Source default: `1`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_scene.cpp:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_skipmats`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r skipmats.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_skyboxes`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r skyboxes.
- Type: `Bool`
- Source default: `true`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_portal.cpp:72`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_skymode`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r skymode.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_sky.cpp:44`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_sprite_distance_cull`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r sprite distance cull.
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/scene/r_opaque_pass.cpp:83`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_spriteadjust`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r spriteadjust.
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/gametexture.cpp:425`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_spriteclipanamorphicminbias`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r spriteclipanamorphicminbias.
- Type: `Float`
- Source default: `0.6`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_sprites.cpp:77`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_ticstability`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r ticstability.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_diag_commands.cpp:1549`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_vanillatrans`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r vanillatrans.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_data/r_vanillatrans.cpp:32`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_view_pain_smooth`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r view pain smooth.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_view_pain_smooth.cpp:30`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_view_pain_smooth_strength`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r view pain smooth strength.
- Type: `Float`
- Source default: `0.6f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/r_view_pain_smooth.cpp:35`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_viewsize`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r viewsize.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/viewport/r_viewport.cpp:46`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_visibility`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r visibility.
- Type: `Float`
- Source default: `8.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:336`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `r_weapon_bob_smooth`

- Category: [Renderer](#category-rendering)
- Description: Likely controls r weapon bob smooth.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_pspr.cpp:679`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `reverbedit_id1`

- Category: [Other](#category-misc)
- Description: Likely controls reverbedit id1.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_reverbedit.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `reverbedit_id2`

- Category: [Other](#category-misc)
- Description: Likely controls reverbedit id2.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_reverbedit.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `reverbedit_name`

- Category: [Other](#category-misc)
- Description: Likely controls reverbedit name.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_reverbedit.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `reverbsavename`

- Category: [Other](#category-misc)
- Description: Likely controls reverbsavename.
- Type: `String`
- Source default: `""`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_reverbedit.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `save_dir`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls save dir.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_SYSTEM_ONLY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/savegamemanager.cpp:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `save_formatted`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls save formatted.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:289`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `save_sort_order`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls save sort order.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/savegamemanager.cpp:43`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `saveargs`

- Category: [Other](#category-misc)
- Description: Likely controls saveargs.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `saved_drawplayersprite`

- Category: [Other](#category-misc)
- Description: Likely controls saved drawplayersprite.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:590`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `saved_screenblocks`

- Category: [Other](#category-misc)
- Description: Likely controls saved screenblocks.
- Type: `Int`
- Source default: `10`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:589`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `saved_showmessages`

- Category: [Other](#category-misc)
- Description: Likely controls saved showmessages.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:591`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `saveloadconfirmation`

- Category: [Other](#category-misc)
- Description: Likely controls saveloadconfirmation.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:415`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `savenetargs`

- Category: [Other](#category-misc)
- Description: Likely controls savenetargs.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `savenetfile`

- Category: [Other](#category-misc)
- Description: Likely controls savenetfile.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `savestatistics`

- Category: [Other](#category-misc)
- Description: Likely controls savestatistics.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/gamedata/statistics.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_cooperative_enable`

- Category: [Other](#category-misc)
- Description: Likely controls sb cooperative enable.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_cooperative_headingcolor`

- Category: [Other](#category-misc)
- Description: Likely controls sb cooperative headingcolor.
- Type: `Int`
- Source default: `CR_RED`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_cooperative_otherplayercolor`

- Category: [Other](#category-misc)
- Description: Likely controls sb cooperative otherplayercolor.
- Type: `Int`
- Source default: `CR_GREY`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:71`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_cooperative_yourplayercolor`

- Category: [Other](#category-misc)
- Description: Likely controls sb cooperative yourplayercolor.
- Type: `Int`
- Source default: `CR_GREEN`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_deathmatch_enable`

- Category: [Other](#category-misc)
- Description: Likely controls sb deathmatch enable.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:73`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_deathmatch_headingcolor`

- Category: [Other](#category-misc)
- Description: Likely controls sb deathmatch headingcolor.
- Type: `Int`
- Source default: `CR_RED`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_deathmatch_otherplayercolor`

- Category: [Other](#category-misc)
- Description: Likely controls sb deathmatch otherplayercolor.
- Type: `Int`
- Source default: `CR_GREY`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_deathmatch_yourplayercolor`

- Category: [Other](#category-misc)
- Description: Likely controls sb deathmatch yourplayercolor.
- Type: `Int`
- Source default: `CR_GREEN`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_teamdeathmatch_enable`

- Category: [Other](#category-misc)
- Description: Likely controls sb teamdeathmatch enable.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:78`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sb_teamdeathmatch_headingcolor`

- Category: [Other](#category-misc)
- Description: Likely controls sb teamdeathmatch headingcolor.
- Type: `Int`
- Source default: `CR_RED`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/hu_scores.cpp:79`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `screenblocks`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls screenblocks.
- Type: `Int`
- Source default: `12`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:429`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `screenshot_dir`

- Category: [Other](#category-misc)
- Description: Likely controls screenshot dir.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/m_misc.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `screenshot_quiet`

- Category: [Other](#category-misc)
- Description: Likely controls screenshot quiet.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/m_misc.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `screenshot_type`

- Category: [Other](#category-misc)
- Description: Likely controls screenshot type.
- Type: `String`
- Source default: `"png"`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/m_misc.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `script_debug`

- Category: [Other](#category-misc)
- Description: Likely controls script debug.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/fragglescript/t_parse.cpp:29`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sentstats_hwr_done`

- Category: [Other](#category-misc)
- Description: Likely controls sentstats hwr done.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOSET`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_anonstats.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `setslotstrict`

- Category: [Other](#category-misc)
- Description: Likely controls setslotstrict.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/gamedata/a_weapons.cpp:524`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `show_messages`

- Category: [Other](#category-misc)
- Description: enable/disable showing messages
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_cmds.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `show_obituaries`

- Category: [Other](#category-misc)
- Description: Likely controls show obituaries.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_cmds.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `showendoom`

- Category: [Other](#category-misc)
- Description: Likely controls showendoom.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/startscreen/endoom.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `showsecretsector`

- Category: [Other](#category-misc)
- Description: Likely controls showsecretsector.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_spec.cpp:593`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `silence_menu_hover`

- Category: [Other](#category-misc)
- Description: Silences cursor movement when implicitly selecting with mouse
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/optionmenu.cpp:30`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `silence_menu_scroll`

- Category: [Other](#category-misc)
- Description: Silences cursor movement when using mouse wheel
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/optionmenu.cpp:29`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `skill`

- Category: [Gameplay](#category-gameplay)
- Description: sets the skill for the next newly started game
- Type: `Int`
- Source default: `2`
- Source flags: `CVAR_SERVERINFO|CVAR_LATCH`
- Macro: `CVARD_NAMED`
- Ref symbol: `gameskill`
- Source: `/workspace/src/g_game.cpp:288`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `skin`

- Category: [Other](#category-misc)
- Description: Likely controls skin.
- Type: `String`
- Source default: `"base"`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `skyoffset`

- Category: [Other](#category-misc)
- Description: Likely controls skyoffset.
- Type: `Float`
- Source default: `0.f`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_skydome.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_aldevice`

- Category: [Audio](#category-audio)
- Description: Likely controls snd aldevice.
- Type: `String`
- Source default: `"Default"`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_aldriver`

- Category: [Audio](#category-audio)
- Description: See alsoftrc.sample for details
- Type: `String`
- Source default: `DEFAULT_DRIVER`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_alresampler`

- Category: [Audio](#category-audio)
- Description: Likely controls snd alresampler.
- Type: `String`
- Source default: `"Default"`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_backend`

- Category: [Audio](#category-audio)
- Description: Audio backend selector: `openal` (default), `null` (silent), or `eternity` (spatial facade).
- Type: `String`
- Source default: `DEF_BACKEND`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/i_sound.cpp:64`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_buffersize`

- Category: [Audio](#category-audio)
- Description: Likely controls snd buffersize.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/i_sound.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_channels`

- Category: [Audio](#category-audio)
- Description: Likely controls snd channels.
- Type: `Int`
- Source default: `128`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_drawoutput`

- Category: [Audio](#category-audio)
- Description: Likely controls snd drawoutput.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:489`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_efx`

- Category: [Audio](#category-audio)
- Description: Likely controls snd efx.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_enabled`

- Category: [Audio](#category-audio)
- Description: enables/disables sound effects
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_sound.cpp:39`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_env_reverb`

- Category: [Audio](#category-audio)
- Description: Likely controls snd env reverb.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_environmentprofile`

- Category: [Audio](#category-audio)
- Description: Global reverb profile. 0=classic, 1=doomsday room, 2=doomsday cave, 3=doomsday cinematic.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:63`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_footsteps_surface`

- Category: [Audio](#category-audio)
- Description: Likely controls snd footsteps surface.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/i_input_feel.cpp:35`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_footstepvolume`

- Category: [Audio](#category-audio)
- Description: Likely controls snd footstepvolume.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_hrtf`

- Category: [Audio](#category-audio)
- Description: Likely controls snd hrtf.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/i_sound.cpp:49`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_mastervolume`

- Category: [Audio](#category-audio)
- Description: Likely controls snd mastervolume.
- Type: `Float`
- Source default: `0.5f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/i_sound.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_menuvolume`

- Category: [Audio](#category-audio)
- Description: Likely controls snd menuvolume.
- Type: `Float`
- Source default: `0.6f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/menu/menu.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_mididevice`

- Category: [Audio](#category-audio)
- Description: Likely controls snd mididevice.
- Type: `Int`
- Source default: `DEF_MIDIDEV`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_midi_base.cpp:92`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_midiprecache`

- Category: [Audio](#category-audio)
- Description: Likely controls snd midiprecache.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:491`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_musicmode`

- Category: [Audio](#category-audio)
- Description: Likely controls snd musicmode.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:70`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_musicvolume`

- Category: [Audio](#category-audio)
- Description: controls music volume
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/i_music.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_pitched`

- Category: [Audio](#category-audio)
- Description: Likely controls snd pitched.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/s_sound.cpp:43`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_samplerate`

- Category: [Audio](#category-audio)
- Description: Likely controls snd samplerate.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/i_sound.cpp:40`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_sfxvolume`

- Category: [Audio](#category-audio)
- Description: Likely controls snd sfxvolume.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/i_sound.cpp:104`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_streambuffersize`

- Category: [Audio](#category-audio)
- Description: Likely controls snd streambuffersize.
- Type: `Int`
- Source default: `64`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:513`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_superstereowidth`

- Category: [Audio](#category-audio)
- Description: Likely controls snd superstereowidth.
- Type: `Float`
- Source default: `0.45f`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:71`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `snd_waterreverb`

- Category: [Audio](#category-audio)
- Description: Likely controls snd waterreverb.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/sound/oalsound.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `splashfactor`

- Category: [Other](#category-misc)
- Description: Likely controls splashfactor.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:6096`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `st_oldouch`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls st oldouch.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/sbar_mugshot.cpp:316`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `st_scale`

- Category: [HUD & Status Bar](#category-hud)
- Description: Likely controls st scale.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_statusbar/shared_sbar.cpp:101`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `statfile`

- Category: [Other](#category-misc)
- Description: Likely controls statfile.
- Type: `String`
- Source default: `"zdoomstat.txt"`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/gamedata/statistics.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `stillbob`

- Category: [Other](#category-misc)
- Description: Likely controls stillbob.
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `storesavepic`

- Category: [Other](#category-misc)
- Description: Likely controls storesavepic.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:292`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `strictdecorate`

- Category: [Other](#category-misc)
- Description: Likely controls strictdecorate.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/scripting/backend/vmbuilder.cpp:34`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_aidirector_enable`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls aidirector enable behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_aidirector.cpp:66`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_aidirector_regroup_hint`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls aidirector regroup hint behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_aidirector.cpp:77`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_aidirector_sweep_tics`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls aidirector sweep tics behavior for server.
- Type: `Int`
- Source default: `7`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_aidirector.cpp:71`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_aircontrol`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Air Control
- Type: `Float`
- Source default: `0.00390625f`
- Source flags: `CVAR_SERVERINFO|CVAR_NOSAVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:1444`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_allowallscripts`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls allowallscripts behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_acs.cpp:10958`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_allowcrouch`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_YES_CROUCH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:708`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_allowfreelook`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_YES_FREELOOK`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:704`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_allowjump`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_YES_JUMP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:702`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_alwaysspawnmulti`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_ALWAYS_SPAWN_MULTI`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:793`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_alwaystally`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Tally Policy
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_level.cpp:173`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_ammofactor`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls ammofactor behavior for server.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_SERVERINFO|CVAR_CHEAT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_interaction.cpp:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_autocompat`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls autocompat behavior for server.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:71`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_barrelrespawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_BARRELS_RESPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:774`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_chasecam`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_CHASECAM`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:785`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_cheats`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls cheats behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_cmds.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_coophalveammo`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_COOP_HALVE_AMMO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:715`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_cooploseammo`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_COOP_LOSE_AMMO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:714`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_cooplosearmor`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_COOP_LOSE_ARMOR`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:712`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_cooploseinventory`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_COOP_LOSE_INVENTORY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:709`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_cooplosekeys`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_COOP_LOSE_KEYS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:710`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_cooplosepowerups`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_COOP_LOSE_POWERUPS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:713`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_cooploseweapons`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_COOP_LOSE_WEAPONS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:711`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_coopsharekeys`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_COOP_SHARE_KEYS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:808`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_corpsefilter`

- Category: [Server & Multiplayer](#category-server)
- Description: Selects which corpse queues sv_corpsequeuesize trims: 0 off, 1 monsters, 2 players, 3 both.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE|CVAR_SERVERINFO|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:176`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_corpsequeuesize`

- Category: [Server & Multiplayer](#category-server)
- Description: Maximum queued corpses retained by corpse cleanup; used with sv_corpsefilter.
- Type: `Int`
- Source default: `64`
- Source flags: `CVAR_ARCHIVE|CVAR_SERVERINFO|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:184`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_crouch`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls crouch behavior for server.
- Type: `Mask`
- Source default: `dmflags`
- Source flags: `DF_NO_CROUCH|DF_YES_CROUCH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:719`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_damagefactorfriendly`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls damagefactorfriendly behavior for server.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_SERVERINFO|CVAR_CHEAT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_interaction.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_damagefactormobj`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls damagefactormobj behavior for server.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_SERVERINFO|CVAR_CHEAT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_interaction.cpp:73`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_damagefactorplayer`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls damagefactorplayer behavior for server.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_SERVERINFO|CVAR_CHEAT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_interaction.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_dedicated_autostart`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls dedicated autostart behavior for server.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_net.cpp:88`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_degeneration`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_YES_DEGENERATION`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:772`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_disableautohealth`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls disableautohealth behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_interaction.cpp:840`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_disallowspying`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_DISALLOW_SPYING`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:784`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_disallowsuicide`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NOSUICIDE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:786`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_dontcheckammo`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_DONTCHECKAMMO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:788`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_doubleammo`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_YES_DOUBLEAMMO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:771`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_dropstyle`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Drop Style
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_enemy.cpp:77`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_falldamage`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_FORCE_FALLINGHX`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:689`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_fallingdamage`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls fallingdamage behavior for server.
- Type: `Mask`
- Source default: `dmflags`
- Source flags: `DF_FORCE_FALLINGHX|DF_FORCE_FALLINGZD`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:721`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_fastmonsters`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_FAST_MONSTERS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:700`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_fastweapons`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Fast Weapons
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_pspr.cpp:90`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_forcerespawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_FORCE_RESPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:693`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_freelook`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls freelook behavior for server.
- Type: `Mask`
- Source default: `dmflags`
- Source flags: `DF_NO_FREELOOK|DF_YES_FREELOOK`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:722`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_gametype`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Game Type
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:337`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_gravity`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Gravity
- Type: `Float`
- Source default: `800.f`
- Source flags: `CVAR_SERVERINFO|CVAR_NOSAVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_mobj.cpp:122`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_hostname`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Hostname
- Type: `String`
- Source default: `GAMENAME " server"`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_net.cpp:84`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_infiniteammo`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_INFINITE_AMMO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:696`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_infiniteinventory`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_INFINITE_INVENTORY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:780`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_instantreaction`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_INSTANT_REACTION`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:716`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionbasebudget`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Base monster budget each wave starts with.
- Type: `Int`
- Source default: `24`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:123`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionbossbonus`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Extra budget added during boss waves.
- Type: `Int`
- Source default: `20`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:158`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionbosswaveevery`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Boss wave cadence (e.g. 5 = every 5th wave, 0 = never).
- Type: `Int`
- Source default: `5`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:153`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionbudgetstep`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Budget increase applied per wave number.
- Type: `Int`
- Source default: `8`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:128`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasioncleanuptime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds allowed for cleanup phase after spawning ends.
- Type: `Float`
- Source default: `4.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:81`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasioncountdowntime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds before wave 1 starts ("Prepare for invasion" countdown).
- Type: `Float`
- Source default: `30.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasiondebug`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Server setting: Invasion Debug
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net.cpp:189`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionexitonvictory`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Server setting: Invasion Exit Victory
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:96`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionintermissiontime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds between completed waves before the next wave starts.
- Type: `Float`
- Source default: `6.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:86`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionmaxactive`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Optional cap for active invasion monsters. 0 disables the cap; positive values are clamped by the engine.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:148`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionperplayer`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Additional budget per extra active player.
- Type: `Int`
- Source default: `6`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:133`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionresulttime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds to keep the final victory/failure state visible.
- Type: `Float`
- Source default: `8.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:91`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionsimlod`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Enables server-side simulation LOD for invasion monsters so distant actors think less often under heavy load.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:169`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionsimloddormantinterval`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Think interval in tics for dormant distant invasion simulation.
- Type: `Int`
- Source default: `TICRATE * 3`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:190`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionsimlodfullrange`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Distance within which invasion monsters keep full-rate simulation.
- Type: `Float`
- Source default: `2048.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:172`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionsimlodreducedinterval`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Think interval in tics for reduced-rate invasion simulation.
- Type: `Int`
- Source default: `5`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:184`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionsimlodreducedrange`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Distance within which invasion monsters use reduced-rate simulation before becoming dormant.
- Type: `Float`
- Source default: `4096.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:178`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionspawnburst`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Maximum monsters spawned per spawn tick burst.
- Type: `Int`
- Source default: `3`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:143`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionspawninterval`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Seconds between spawn ticks while wave spawning is active.
- Type: `Float`
- Source default: `0.35f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:138`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionspawntime`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Wave spawn window length in seconds before cleanup phase.
- Type: `Float`
- Source default: `8.0f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:76`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionspotfallback`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Fallback to generic spawning when tagged invasion spots cannot be used.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:166`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionspotusemaptags`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Restrict native invasion spots by map thing TID/tag. Keep disabled for Skulltag/Zandronum map compatibility; the spot arguments already control wave timing.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:163`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_invasionwaves`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Maximum number of invasion waves in a run.
- Type: `Int`
- Source default: `8`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:99`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_itemrespawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_ITEMS_RESPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:699`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_jump`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls jump behavior for server.
- Type: `Mask`
- Source default: `dmflags`
- Source flags: `DF_NO_JUMP|DF_YES_JUMP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:720`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_keepfrags`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_YES_KEEPFRAGS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:775`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_killallmonsters`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_KILL_MONSTERS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:781`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_killbossmonst`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_KILLBOSSMONST`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:789`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_lagcomp`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls lagcomp behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rewind.cpp:813`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_lagcomp_max_age_tics`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls lagcomp max age tics behavior for server.
- Type: `Int`
- Source default: `12`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rewind.cpp:817`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_lateJoin`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls lateJoin behavior for server.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_net.cpp:109`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_localitems`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_LOCAL_ITEMS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:809`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_losefrag`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_YES_LOSEFRAG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:777`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_maxplayers`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Max Players
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_net.cpp:89`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_monsterrespawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_MONSTERS_RESPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:698`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_motd`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: MOTD
- Type: `String`
- Source default: `"Welcome to " GAMENAME`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_net.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_natport`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: NAT Port
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/sv_master.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noarmor`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_ARMOR`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:694`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noautoaim`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NOAUTOAIM`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:787`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noautomap`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NO_AUTOMAP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:782`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noautomapallies`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NO_AUTOMAP_ALLIES`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:783`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nobfgaim`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NO_FREEAIMBFG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:773`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nocoopitems`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_NO_COOP_ONLY_ITEMS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:811`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nocoopthings`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_NO_COOP_ONLY_THINGS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:812`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nocountendmonst`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NOCOUNTENDMONST`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:790`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nocrouch`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_CROUCH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:707`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noexit`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_EXIT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:695`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noextraammo`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NO_EXTRA_AMMO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:795`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nofov`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_FOV`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:705`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nofreelook`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_FREELOOK`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:703`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nohealth`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_HEALTH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:686`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noitems`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_ITEMS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:687`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nojump`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_JUMP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:701`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nolocaldrops`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_NO_LOCAL_DROPS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:810`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nomonsters`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_MONSTERS`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:697`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noplayerclip`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_NO_PLAYER_CLIP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:807`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_norespawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NO_RESPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:776`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noteamswitch`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NO_TEAM_SWITCH`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:770`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_nothingspawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NO_COOP_THING_SPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:792`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_novertspread`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_NOVERTSPREAD`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:794`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_noweaponspawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_NO_COOP_WEAPON_SPAWN`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:706`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_oldfalldamage`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_FORCE_FALLINGZD`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:690`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_pistolstart`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_PISTOL_START`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:814`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_predator_allow_cheats`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls predator allow cheats behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_predator.cpp:126`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_predator_buy_seconds`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls predator buy seconds behavior for server.
- Type: `Int`
- Source default: `20`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_predator.cpp:132`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_predator_enable`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls predator enable behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_predator.cpp:125`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_predator_round_seconds`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls predator round seconds behavior for server.
- Type: `Int`
- Source default: `180`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_predator.cpp:127`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_predator_starting_currency`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls predator starting currency behavior for server.
- Type: `Int`
- Source default: `800`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_predator.cpp:137`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_rcon_enable`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls rcon enable behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rcon.cpp:302`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_rcon_password`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls rcon password behavior for server.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rcon.cpp:308`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_rcon_port`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls rcon port behavior for server.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_rcon.cpp:314`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_rememberlastweapon`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags3.
- Type: `Flag`
- Source default: `dmflags3`
- Source flags: `DF3_REMEMBER_LAST_WEAP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:813`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_respawnprotect`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_YES_RESPAWN_INVUL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:778`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_respawnsuper`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_RESPAWN_SUPER`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:791`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_samelevel`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_SAME_LEVEL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:691`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_samespawnspot`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_SAME_SPAWN_SPOT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:779`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_singleplayerrespawn`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls singleplayerrespawn behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO | CVAR_CHEAT`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_user.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_smartaim`

- Category: [Server & Multiplayer](#category-server)
- Description: Server setting: Smart Aim
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_map.cpp:69`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_spawnfarthest`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_SPAWN_FARTHEST`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:692`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_unlimited_pickup`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls unlimited pickup behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/console/c_cmds.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_upnp`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls upnp behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/sv_master.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_usemapsettingswavelimit`

- Category: [Server & Multiplayer](#category-server)
- Description: If enabled, map-defined invasion wavelimit metadata overrides sv_invasionwaves when present.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:120`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_usemasters`

- Category: [Server & Multiplayer](#category-server)
- Description: Likely controls usemasters behavior for server.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/sv_master.cpp:774`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_weapondrop`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags2.
- Type: `Flag`
- Source default: `dmflags2`
- Source flags: `DF2_YES_WEAPONDROP`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:769`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `sv_weaponstay`

- Category: [Server & Multiplayer](#category-server)
- Description: Flag alias backed by dmflags.
- Type: `Flag`
- Source default: `dmflags`
- Source flags: `DF_WEAPONS_STAY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:688`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `team`

- Category: [Other](#category-misc)
- Description: Likely controls team.
- Type: `Int`
- Source default: `TEAM_NONE`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `teamdamage`

- Category: [Other](#category-misc)
- Description: Server setting: Team Damage
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_SERVERINFO | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:236`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `teamplay`

- Category: [Gameplay](#category-gameplay)
- Description: Likely controls teamplay.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:336`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `telezoom`

- Category: [Other](#category-misc)
- Description: Likely controls telezoom.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/playsim/p_teleport.cpp:34`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `tf`

- Category: [Other](#category-misc)
- Description: Likely controls tf.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/r_utility.cpp:485`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `ticker`

- Category: [Other](#category-misc)
- Description: Likely controls ticker.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:204`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `tilt`

- Category: [Other](#category-misc)
- Description: Likely controls tilt.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/plane/r_visibleplane.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timelimit`

- Category: [Other](#category-misc)
- Description: Server setting: Time Limit
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:484`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_channel_pressure`

- Category: [Other](#category-misc)
- Description: Likely controls timidity channel pressure.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:402`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_chorus`

- Category: [Other](#category-misc)
- Description: Likely controls timidity chorus.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:392`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_config`

- Category: [Other](#category-misc)
- Description: Likely controls timidity config.
- Type: `String`
- Source default: `GAMENAMELOWERCASE`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL | CVAR_SYSTEM_ONLY`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:458`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_drum_effect`

- Category: [Other](#category-misc)
- Description: Likely controls timidity drum effect.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:427`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_drum_power`

- Category: [Other](#category-misc)
- Description: Likely controls timidity drum power.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:437`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_key_adjust`

- Category: [Other](#category-misc)
- Description: Likely controls timidity key adjust.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:442`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_lpf_def`

- Category: [Other](#category-misc)
- Description: Likely controls timidity lpf def.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:407`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_min_sustain_time`

- Category: [Other](#category-misc)
- Description: Likely controls timidity min sustain time.
- Type: `Float`
- Source default: `5000.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:452`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_modulation_envelope`

- Category: [Other](#category-misc)
- Description: Likely controls timidity modulation envelope.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:417`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_modulation_wheel`

- Category: [Other](#category-misc)
- Description: Likely controls timidity modulation wheel.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:372`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_overlap_voice_allow`

- Category: [Other](#category-misc)
- Description: Likely controls timidity overlap voice allow.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:422`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_pan_delay`

- Category: [Other](#category-misc)
- Description: Likely controls timidity pan delay.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:432`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_portamento`

- Category: [Other](#category-misc)
- Description: Likely controls timidity portamento.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:377`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_reverb`

- Category: [Other](#category-misc)
- Description: Likely controls timidity reverb.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:382`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_reverb_level`

- Category: [Other](#category-misc)
- Description: Likely controls timidity reverb level.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:387`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_surround_chorus`

- Category: [Other](#category-misc)
- Description: Likely controls timidity surround chorus.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:397`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_temper_control`

- Category: [Other](#category-misc)
- Description: Likely controls timidity temper control.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:412`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `timidity_tempo_adjust`

- Category: [Other](#category-misc)
- Description: Likely controls timidity tempo adjust.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:447`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `topskew`

- Category: [Other](#category-misc)
- Description: Likely controls topskew.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/hwrenderer/scene/hw_walls.cpp:2129`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `transsouls`

- Category: [Other](#category-misc)
- Description: Likely controls transsouls.
- Type: `Float`
- Source default: `0.75f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:518`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `turbo`

- Category: [Other](#category-misc)
- Description: Likely controls turbo.
- Type: `Float`
- Source default: `100.f`
- Source flags: `CVAR_NOINITCALL | CVAR_CHEAT`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_game.cpp:386`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `turnspeedsprintfast`

- Category: [Other](#category-misc)
- Description: Likely controls turnspeedsprintfast.
- Type: `Int`
- Source default: `1280`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:149`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `turnspeedsprintslow`

- Category: [Other](#category-misc)
- Description: Likely controls turnspeedsprintslow.
- Type: `Int`
- Source default: `320`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:157`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `turnspeedwalkfast`

- Category: [Other](#category-misc)
- Description: Likely controls turnspeedwalkfast.
- Type: `Int`
- Source default: `640`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:145`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `turnspeedwalkslow`

- Category: [Other](#category-misc)
- Description: Likely controls turnspeedwalkslow.
- Type: `Int`
- Source default: `320`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:153`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `ui_color_mix`

- Category: [Other](#category-misc)
- Description: Likely controls ui color mix.
- Type: `Float`
- Source default: `.35`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:75`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `ui_colors`

- Category: [Other](#category-misc)
- Description: Likely controls ui colors.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `ui_generic`

- Category: [Other](#category-misc)
- Description: Likely controls ui generic.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/fonts/v_text.cpp:313`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `ui_screenborder_classic_scaling`

- Category: [Other](#category-misc)
- Description: Likely controls ui screenborder classic scaling.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/2d/v_draw.cpp:36`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `ui_theme`

- Category: [Other](#category-misc)
- Description: launcher theme. 0: auto, 1: dark, 2: light
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/widgets/widgetresourcedata.cpp:30`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `uiscale`

- Category: [Other](#category-misc)
- Description: Likely controls uiscale.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:144`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `underwater_fade_scalar`

- Category: [Other](#category-misc)
- Description: Likely controls underwater fade scalar.
- Type: `Float`
- Source default: `1.0f`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/2d/v_blend.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `use_joystick`

- Category: [Other](#category-misc)
- Description: enables input from the joystick if it is present
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/m_joy.cpp:80`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `use_mouse`

- Category: [Other](#category-misc)
- Description: Likely controls use mouse.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/i_input.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `var_friction`

- Category: [Other](#category-misc)
- Description: Likely controls var friction.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:143`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `var_pushers`

- Category: [Other](#category-misc)
- Description: Likely controls var pushers.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/g_cvars.cpp:137`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vertspread`

- Category: [Other](#category-misc)
- Description: Likely controls vertspread.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:62`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_activeinbackground`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid activeinbackground.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:970`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_adapter`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid adapter.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:224`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_allowtrueultrawide`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid allowtrueultrawide.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE|CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/2d/v_draw.cpp:43`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_aspect`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid aspect.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:437`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_blackpoint`

- Category: [Video & Display](#category-video)
- Description: adjusts what the engine outputs as black
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:152`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_contrast`

- Category: [Video & Display](#category-video)
- Description: adjusts contrast component of gamma ramp
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:133`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_cropaspect`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid cropaspect.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_videoscale.cpp:187`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_cursor`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid cursor.
- Type: `String`
- Source default: `"None"`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:490`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_defheight`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid defheight.
- Type: `Int`
- Source default: `480`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:203`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_defwidth`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid defwidth.
- Type: `Int`
- Source default: `640`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:202`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_dontdowait`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid dontdowait.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:38`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_fixgamma`

- Category: [Video & Display](#category-video)
- Description: adjusts gamma component of gamma ramp
- Type: `Float`
- Source default: `0.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:118`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_fps`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid fps.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/engine/i_interface.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_fsdwmhack`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid fsdwmhack.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/base_sysfb.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_fsdwmhackalpha`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid fsdwmhackalpha.
- Type: `Int`
- Source default: `255`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/base_sysfb.cpp:60`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_fullscreen`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid fullscreen.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:468`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_gamma`

- Category: [Video & Display](#category-video)
- Description: (internal) target output gamma
- Type: `Float`
- Source default: `GAMMA_DEFAULT`
- Source flags: `CVAR_NOINITCALL`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:83`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_hdr`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid hdr.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:473`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_i_blackpoint`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid i blackpoint.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_VIRTUAL | CVAR_NOINITCALL | CVAR_SYSTEM_ONLY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:149`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_i_whitepoint`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid i whitepoint.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_VIRTUAL | CVAR_NOINITCALL | CVAR_SYSTEM_ONLY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:150`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_lowerinbackground`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid lowerinbackground.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_net_invasion.inl:39`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_maxfps`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid maxfps.
- Type: `Int`
- Source default: `500`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:73`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_nopalsubstitutions`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid nopalsubstitutions.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/rendering/swrenderer/textures/r_swtexture.cpp:540`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_preferbackend`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid preferbackend.
- Type: `Int`
- Source default: `BACKEND_DEFAULT`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_renderer`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid renderer.
- Type: `Int`
- Source default: `1`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:147`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_rendermode`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid rendermode.
- Type: `Int`
- Source default: `4`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:444`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_saturation`

- Category: [Video & Display](#category-video)
- Description: adjusts saturation component of gamma ramp
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:139`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_scale_customheight`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid scale customheight.
- Type: `Int`
- Source default: `VID_MIN_HEIGHT`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_videoscale.cpp:48`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_scale_custompixelaspect`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid scale custompixelaspect.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_videoscale.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_scale_customwidth`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid scale customwidth.
- Type: `Int`
- Source default: `VID_MIN_WIDTH`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_videoscale.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_scale_linear`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid scale linear.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_videoscale.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_scalefactor`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid scalefactor.
- Type: `Float`
- Source default: `1.0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_videoscale.cpp:171`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_scalemode`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid scalemode.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/r_videoscale.cpp:180`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_sdl_render_driver`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid sdl render driver.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/posix/sdl/sdlglvideo.cpp:87`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_shadersupport`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid shadersupport.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_SYSTEM_ONLY`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:85`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_showpalette`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid showpalette.
- Type: `Int`
- Source default: `0`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:526`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_vsync`

- Category: [Video & Display](#category-video)
- Description: Likely controls vid vsync.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:206`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vid_whitepoint`

- Category: [Video & Display](#category-video)
- Description: adjusts what the engine outputs as white
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVARD`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_cvars.cpp:164`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vk_debug`

- Category: [Other](#category-misc)
- Description: Likely controls vk debug.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:74`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vk_debug_callstack`

- Category: [Other](#category-misc)
- Description: Likely controls vk debug callstack.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:79`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vk_device`

- Category: [Other](#category-misc)
- Description: Likely controls vk device.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:81`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vk_exclusivefullscreen`

- Category: [Other](#category-misc)
- Description: Likely controls vk exclusivefullscreen.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/vulkan/textures/vk_framebuffer.cpp:32`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vk_hdr`

- Category: [Other](#category-misc)
- Description: Likely controls vk hdr.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/vulkan/textures/vk_framebuffer.cpp:31`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vk_raytrace`

- Category: [Other](#category-misc)
- Description: Likely controls vk raytrace.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/vulkan/system/vk_renderdevice.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vk_submit_size`

- Category: [Other](#category-misc)
- Description: Likely controls vk submit size.
- Type: `Int`
- Source default: `1000`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/vulkan/renderer/vk_renderstate.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vm_debug`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls vm debug.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:3333`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vm_debug_port`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls vm debug port.
- Type: `Int`
- Source default: `19021`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:3349`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vm_jit`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls vm jit.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/scripting/vm/vmframe.cpp:41`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vm_jit_aot`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls vm jit aot.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/scripting/vm/vmframe.cpp:49`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vm_warnthinkercreation`

- Category: [Debug & Development](#category-debug)
- Description: Likely controls vm warnthinkercreation.
- Type: `Bool`
- Source default: `false`
- Source flags: `0`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/scripting/backend/codegen_doom.cpp:991`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vr_enable_quadbuffered`

- Category: [Other](#category-misc)
- Description: Likely controls vr enable quadbuffered.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/platform/win32/win32glvideo.cpp:68`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vr_hunits_per_meter`

- Category: [Other](#category-misc)
- Description: Likely controls vr hunits per meter.
- Type: `Float`
- Source default: `41.0f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:45`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vr_ipd`

- Category: [Other](#category-misc)
- Description: Likely controls vr ipd.
- Type: `Float`
- Source default: `0.062f`
- Source flags: `CVAR_ARCHIVE|CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:39`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vr_mode`

- Category: [Other](#category-misc)
- Description: Likely controls vr mode.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_GLOBALCONFIG|CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:33`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vr_screendist`

- Category: [Other](#category-misc)
- Description: Likely controls vr screendist.
- Type: `Float`
- Source default: `0.80f`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:42`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `vr_swap_eyes`

- Category: [Other](#category-misc)
- Description: Likely controls vr swap eyes.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/hwrenderer/data/hw_vrmodes.cpp:36`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `warningstoerrors`

- Category: [Other](#category-misc)
- Description: Likely controls warningstoerrors.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_GLOBALCONFIG | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/scripting/backend/vmbuilder.cpp:35`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wavelimit`

- Category: [HCDE Invasion & Server](#category-hcde-invasion)
- Description: Legacy Skulltag compatibility override for invasion waves. 0 disables the override; 1..255 forces that wave count.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO | CVAR_NOSAVE`
- Macro: `CUSTOM_CVAR_NAMED`
- Ref symbol: `wavelimit_compat`
- Source: `/workspace/src/d_net_invasion.inl:106`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wbobfire`

- Category: [Other](#category-misc)
- Description: Likely controls wbobfire.
- Type: `Float`
- Source default: `0.f`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:59`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wbobspeed`

- Category: [Other](#category-misc)
- Description: Likely controls wbobspeed.
- Type: `Float`
- Source default: `1.f`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_netinfo.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wi_autoadvance`

- Category: [Other](#category-misc)
- Description: Likely controls wi autoadvance.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_SERVERINFO`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/wi_stuff.cpp:57`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wi_cleantextscale`

- Category: [Other](#category-misc)
- Description: Likely controls wi cleantextscale.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/wi_stuff.cpp:58`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wi_noautostartmap`

- Category: [Other](#category-misc)
- Description: Likely controls wi noautostartmap.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_USERINFO | CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/wi_stuff.cpp:56`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wi_percents`

- Category: [Other](#category-misc)
- Description: Likely controls wi percents.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/wi_stuff.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wi_showtotaltime`

- Category: [Other](#category-misc)
- Description: Likely controls wi showtotaltime.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/wi_stuff.cpp:55`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wildmidi_config`

- Category: [Other](#category-misc)
- Description: Likely controls wildmidi config.
- Type: `String`
- Source default: `""`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL | CVAR_SYSTEM_ONLY`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:469`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wildmidi_enhanced_resampling`

- Category: [Other](#category-misc)
- Description: Likely controls wildmidi enhanced resampling.
- Type: `Bool`
- Source default: `true`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:479`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wildmidi_reverb`

- Category: [Other](#category-misc)
- Description: Likely controls wildmidi reverb.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_VIRTUAL`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/audio/music/music_config.cpp:474`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `win_h`

- Category: [Other](#category-misc)
- Description: Likely controls win h.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:53`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `win_maximized`

- Category: [Other](#category-misc)
- Description: Likely controls win maximized.
- Type: `Bool`
- Source default: `false`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG | CVAR_NOINITCALL`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:54`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `win_w`

- Category: [Other](#category-misc)
- Description: Likely controls win w.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:52`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `win_x`

- Category: [Other](#category-misc)
- Description: Likely controls win x.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:50`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `win_y`

- Category: [Other](#category-misc)
- Description: Likely controls win y.
- Type: `Int`
- Source default: `-1`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/rendering/v_video.cpp:51`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `wipetype`

- Category: [Other](#category-misc)
- Description: Likely controls wipetype.
- Type: `Int`
- Source default: `1`
- Source flags: `CVAR_ARCHIVE`
- Macro: `CUSTOM_CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/d_main.cpp:485`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

### `xbrz_colorformat`

- Category: [Other](#category-misc)
- Description: Likely controls xbrz colorformat.
- Type: `Int`
- Source default: `0`
- Source flags: `CVAR_ARCHIVE | CVAR_GLOBALCONFIG`
- Macro: `CVAR`
- Ref symbol: `same as cvar name`
- Source: `/workspace/src/common/textures/hires/hqresize.cpp:93`
- Present in runtime snapshot: n/a
- Runtime snapshot value: `n/a`

