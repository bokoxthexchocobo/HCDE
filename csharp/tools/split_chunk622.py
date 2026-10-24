#!/usr/bin/env python3
"""Split chunk622 (442 native) into session lifecycle (<=400) and launcher/pregame UI chunk648."""
from __future__ import annotations

import json
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
TALLY = TOOLS.parent / "docs/data/loc_tally.json"


def load(num: int) -> dict:
    data = json.loads((TOOLS / f"chunk{num}.json").read_text(encoding="utf-8"))
    return data[0] if isinstance(data, list) else data


def save(num: int, ch: dict) -> None:
    (TOOLS / f"chunk{num}.json").write_text(json.dumps([ch], indent=2) + "\n", encoding="utf-8")


def main() -> int:
    ch622 = load(622)
    ranges = ch622.get("nativeRanges") or []
    lifecycle = [r for r in ranges if int(r["start"]) >= 1468 and int(r["end"]) <= 1638]
    launcher_ui = [r for r in ranges if int(r["end"]) < 1468 or int(r["start"]) >= 1641]

    ch622["nativeRanges"] = lifecycle
    ch622["managedSourceFiles"] = [
        "csharp/src/HCDE.Net.Core/VmINetQueryClient.cs",
        "csharp/src/HCDE.Net.Core/VmINetSessionLifecycle.cs",
        "csharp/src/HCDE.Net.Core/VmINetSocketAddress.cs",
    ]
    audit = ch622.get("auditPaddingReview") or ""
    ch622["auditPaddingReview"] = audit.split("| Managed | Native (`i_net.cpp`) |")[0] + (
        "| Managed | Native (`i_net.cpp`) |\n|---------|-------------------|\n"
        "| `QueryServerInfoNative` | `I_QueryServerInfo` 1468–1548 |\n"
        "| `StartNetworkNative` | `StartNetwork` 1550–1572 |\n"
        "| `CloseNetworkNative` | `CloseNetwork` 1574–1601 |\n"
        "| `GenerateGameIdNative` | `GenerateGameID` 1603–1607 |\n"
        "| `INetLogNative` | `I_NetLog` 1611–1629 |\n"
        "| `SetConnectFlowNative` | `SetConnectFlow` 1631–1638 |\n"
    )
    ch622["note"] = (
        ch622.get("note", "")
        + " Split: launcher/pregame UI bands moved to chunk648."
    ).strip()
    save(622, ch622)

    ch648 = {
        "name": "i-net-launcher-pregame-ui-chunk648",
        "bucket": "conversion",
        "note": "Split from chunk622: server query respond + pregame UI (`i_net.cpp` 904–1092, 1641–1810).",
        "auditSeams": "`DebugTrace`, socket I/O, and UI pumps are injected.",
        "auditRationale": "Whole-function bands for launcher respond and `I_Net*` pregame UI entry points.",
        "auditPaddingReview": (
            "| Managed | Native (`i_net.cpp`) |\n|---------|-------------------|\n"
            "| `GetLocalServerSnapshotNative` | `I_GetLocalServerSnapshot` 904–907 |\n"
            "| `GetCachedServerQuerySnapshotNative` | `GetCachedServerQuerySnapshot` 955–970 |\n"
            "| `EncodeLauncherInfoNative` | `SendLauncherInfo` encode 972–1042 |\n"
            "| `SendLauncherInfoNative` | `SendLauncherInfo` send 1044–1050 |\n"
            "| `WriteSessionTokenNative` | `WriteSessionToken` 1053–1066 |\n"
            "| `ReadSessionTokenNative` | `ReadSessionToken` 1068–1071 |\n"
            "| `TryHandleServerQueryNative` | `TryHandleServerQuery` 1073–1092 |\n"
            "| `INetErrorNative` | `I_NetError` 1641–1649 |\n"
            "| `INetInitNative` | `I_NetInit` 1651–1665 |\n"
            "| `INetMessageNative` | `I_NetMessage` 1672–1686 |\n"
            "| `INetLoopNative` | `I_NetLoop` 1697–1728 |\n"
            "| `INetClientConnectedNative` | `I_NetClientConnected` 1732–1750 |\n"
            "| `INetClientUpdatedNative` | `I_NetClientUpdated` 1753–1756 |\n"
            "| `INetClientDisconnectedNative` | `I_NetClientDisconnected` 1758–1770 |\n"
            "| `INetUpdatePlayersNative` | `I_NetUpdatePlayers` 1772–1780 |\n"
            "| `IShouldStartNetGameNative` | `I_ShouldStartNetGame` 1782–1787 |\n"
            "| `IGetKickClientsNative` | `I_GetKickClients` 1789–1796 |\n"
            "| `IGetBanClientsNative` | `I_GetBanClients` 1798–1805 |\n"
            "| `INetDoneNative` | `I_NetDone` 1807–1810 |\n"
        ),
        "managedSourceFiles": [
            "csharp/src/HCDE.Net.Core/VmINetLauncherRespond.cs",
            "csharp/src/HCDE.Net.Core/VmINetPregameUi.cs",
        ],
        "nativeRanges": launcher_ui,
    }
    save(648, ch648)

    tally = json.loads(TALLY.read_text(encoding="utf-8"))
    # clone 622 row for 648 if missing
    by_name = {c["name"]: c for c in tally["chunks"]}
    if ch648["name"] not in by_name:
        template = by_name[ch622["name"]]
        row = {k: v for k, v in template.items() if k not in ("name", "nativeRanges", "nativeLoc", "managedLoc")}
        row["name"] = ch648["name"]
        row["nativeRanges"] = ch648["nativeRanges"]
        row["nativeLoc"] = sum(int(r["end"]) - int(r["start"]) + 1 for r in ch648["nativeRanges"])
        row["managedSourceFiles"] = ch648["managedSourceFiles"]
        row["managedLoc"] = 0
        row["note"] = ch648["note"]
        tally["chunks"].append(row)
    TALLY.write_text(json.dumps(tally, indent=2) + "\n", encoding="utf-8")
    print("split 622 -> 622 lifecycle + chunk648")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
