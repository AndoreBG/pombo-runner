#!/usr/bin/env python3
"""Gera Assets/Scenes/MainMenu.unity e Assets/Scenes/Game.unity.

Estrategia (ver plano, E1): os blocos de Main Camera / Directional Light /
Global Volume sao copiados do template validado (tools/scene_template.unity,
extraido de Assets/Scenes/gameplay.unity) com remapeamento de fileIDs; o
resto da cena (jogador, pools, HUD...) e montado em runtime pelo Bootstrap.

Uso: python3 tools/gen_scenes.py
Requer os .meta dos bootstraps já gerados (tools/gen_meta.py).
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
TEMPLATE = ROOT / "tools" / "scene_template.unity"
SCENES_DIR = ROOT / "Assets" / "Scenes"

BLOCK_RE = re.compile(r"^--- !u!(\d+) &(\d+)\s*$", re.M)

SCENE_CONFIGS = {
    "MainMenu": {
        "bootstrap": "MainMenuBootstrap",
        "cam_pos": (0.0, 0.0, -10.0),
        "cam_euler": (0.0, 0.0, 0.0),
        "cam_fov": 60,
        "cam_far": 1000,
        "cam_bg": (0.45, 0.62, 0.78, 1.0),
    },
    "Game": {
        "bootstrap": "GameBootstrap",
        "cam_pos": (0.0, 6.0, -10.0),
        "cam_euler": (10.0, 0.0, 0.0),
        "cam_fov": 65,
        "cam_far": 130,
        "cam_bg": (0.45, 0.62, 0.78, 1.0),
    },
}

# ids do template por grupo
GROUPS = {
    "camera": ["330585543", "330585544", "330585545", "330585546", "330585547"],
    "light": ["410087039", "410087040", "410087041", "410087042"],
    "volume": ["832575517", "832575518", "832575519"],
}
SETTINGS_IDS = {"1", "2", "3", "4"}
SCENEROOTS_ID = "9223372036854775807"


def parse_template(text: str) -> dict[str, tuple[str, str]]:
    """id -> (class_id, corpo_do_bloco_sem_o_cabecalho)"""
    matches = list(BLOCK_RE.finditer(text))
    blocks: dict[str, tuple[str, str]] = {}
    for i, m in enumerate(matches):
        start = m.end()
        end = matches[i + 1].start() if i + 1 < len(matches) else len(text)
        blocks[m.group(2)] = (m.group(1), text[start:end].lstrip("\n"))
    return blocks


def euler_to_quat(ex: float, ey: float, ez: float) -> tuple[float, float, float, float]:
    import math
    rx, ry, rz = map(math.radians, (ex, ey, ez))
    cx, sx = math.cos(rx / 2), math.sin(rx / 2)
    cy, sy = math.cos(ry / 2), math.sin(ry / 2)
    cz, sz = math.cos(rz / 2), math.sin(rz / 2)
    x = sx * cy * cz + cx * sy * sz
    y = cx * sy * cz - sx * cy * sz
    z = cx * cy * sz + sx * sy * cz
    w = cx * cy * cz - sx * sy * sz
    return x, y, z, w


def fmt(v: float) -> str:
    s = f"{v:.7g}"
    return s


def remap(body: str, mapping: dict[str, str]) -> str:
    def sub(m: re.Match) -> str:
        fid = m.group(1)
        return f"{{fileID: {mapping.get(fid, fid)}}}"
    return re.sub(r"\{fileID: (\d+)\}", sub, body)


def apply_camera(body: str, cfg: dict) -> str:
    px, py, pz = cfg["cam_pos"]
    ex, ey, ez = cfg["cam_euler"]
    qx, qy, qz, qw = euler_to_quat(ex, ey, ez)
    body = re.sub(
        r"m_LocalPosition: \{[^}]*\}",
        f"m_LocalPosition: {{x: {fmt(px)}, y: {fmt(py)}, z: {fmt(pz)}}}",
        body, count=1)
    body = re.sub(
        r"m_LocalRotation: \{[^}]*\}",
        f"m_LocalRotation: {{x: {fmt(qx)}, y: {fmt(qy)}, z: {fmt(qz)}, w: {fmt(qw)}}}",
        body, count=1)
    body = re.sub(
        r"m_LocalEulerAnglesHint: \{[^}]*\}",
        f"m_LocalEulerAnglesHint: {{x: {fmt(ex)}, y: {fmt(ey)}, z: {fmt(ez)}}}",
        body, count=1)
    return body


def apply_camera_component(body: str, cfg: dict) -> str:
    bg = cfg["cam_bg"]
    # clear color solido = cor da nevoa (WorldBuilder.SkyColor)
    body = re.sub(r"m_ClearFlags: \d+", "m_ClearFlags: 2", body, count=1)
    body = re.sub(r"far clip plane: \d+", f"far clip plane: {cfg['cam_far']}", body, count=1)
    body = re.sub(r"field of view: \d+", f"field of view: {cfg['cam_fov']}", body, count=1)
    body = re.sub(
        r"m_BackGroundColor: \{[^}]*\}",
        f"m_BackGroundColor: {{r: {fmt(bg[0])}, g: {fmt(bg[1])}, b: {fmt(bg[2])}, a: {fmt(bg[3])}}}",
        body, count=1)
    return body


def bootstrap_blocks(cfg: dict, next_id: int) -> tuple[dict[str, tuple[str, str]], dict[str, str], int]:
    """Cria GO + Transform + MonoBehaviour do bootstrap. Retorna (novos blocos, mapping, next_id)."""
    script_meta = ROOT / "Assets" / "Scripts" / "Boot" / f"{cfg['bootstrap']}.cs.meta"
    if not script_meta.exists():
        raise SystemExit(
            f"meta do bootstrap ausente: {script_meta.relative_to(ROOT)} — rode tools/gen_meta.py antes")
    guid = re.search(r"^guid:\s*([0-9a-f]{32})", script_meta.read_text(), re.M).group(1)

    go_id, tr_id, mb_id = str(next_id), str(next_id + 1), str(next_id + 2)
    next_id += 3
    go = f"""GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {mb_id}}}
  m_Layer: 0
  m_Name: {cfg['bootstrap']}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
"""
    tr = """Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: %s}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
""" % go_id
    mb = f"""MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
"""
    return {go_id: ("1", go), tr_id: ("4", tr), mb_id: ("114", mb)}, {"bootstrap_tr": tr_id}, next_id


def build_scene(name: str, cfg: dict, blocks: dict[str, tuple[str, str]]) -> str:
    # aloca novos ids para grupos de objetos
    mapping: dict[str, str] = {}
    next_id = 100001
    for group in ("camera", "light", "volume"):
        for old in GROUPS[group]:
            mapping[old] = str(next_id)
            next_id += 1

    new_blocks, bs_map, next_id = bootstrap_blocks(cfg, next_id)
    mapping.update({k: v for k, v in bs_map.items() if k != "bootstrap_tr"})

    parts: list[str] = ["%YAML 1.1\n%TAG !u! tag:unity3d.com,2011\n"]

    # settings (copiados verbatim)
    for sid in ("1", "2", "3", "4"):
        cid, body = blocks[sid]
        parts.append(f"--- !u!{cid} &{sid}\n{body}")

    # objetos do template remapeados
    cam_tr = mapping["330585546"]
    light_tr = mapping["410087041"]
    vol_tr = mapping["832575519"]
    for old_id in GROUPS["camera"] + GROUPS["light"] + GROUPS["volume"]:
        cid, body = blocks[old_id]
        body = remap(body, mapping)
        if old_id == "330585546":      # camera transform
            body = apply_camera(body, cfg)
        if old_id == "330585545":      # camera component
            body = apply_camera_component(body, cfg)
        parts.append(f"--- !u!{cid} &{mapping[old_id]}\n{body}")

    # bootstrap
    for bid, (cid, body) in new_blocks.items():
        parts.append(f"--- !u!{cid} &{bid}\n{body}")

    # SceneRoots
    roots = [cam_tr, light_tr, vol_tr, bs_map["bootstrap_tr"]]
    roots_yaml = "\n".join(f"  - {{fileID: {r}}}" for r in roots)
    parts.append(
        "--- !u!1660057539 &9223372036854775807\n"
        "SceneRoots:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_Roots:\n" + roots_yaml + "\n"
    )
    return "".join(parts)


def main() -> int:
    if not TEMPLATE.exists():
        print(f"template ausente: {TEMPLATE}", file=sys.stderr)
        return 1
    blocks = parse_template(TEMPLATE.read_text(encoding="utf-8"))
    needed = SETTINGS_IDS | set(GROUPS["camera"]) | set(GROUPS["light"]) | set(GROUPS["volume"])
    missing = needed - set(blocks)
    if missing:
        print(f"template sem blocos: {missing}", file=sys.stderr)
        return 1

    SCENES_DIR.mkdir(parents=True, exist_ok=True)
    for name, cfg in SCENE_CONFIGS.items():
        out = SCENES_DIR / f"{name}.unity"
        out.write_text(build_scene(name, cfg, blocks), encoding="utf-8")
        print(f"+ {out.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
