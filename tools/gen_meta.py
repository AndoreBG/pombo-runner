#!/usr/bin/env python3


from __future__ import annotations

import re
import sys
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "Assets"

META_FOLDER = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

META_CS = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

META_SHADER = """fileFormatVersion: 2
guid: {guid}
ShaderImporter:
  externalObjects: {{}}
  defaultTextures: []
  nonModifiableTextures: []
  userData:
  assetBundleName:
  assetBundleVariant:
"""

META_NATIVE = """fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: {main_object}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

META_DEFAULT = """fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

GUID_RE = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.M)


def collect_used_guids() -> set[str]:
    used: set[str] = set()
    for meta in ROOT.rglob("*.meta"):
        m = GUID_RE.search(meta.read_text(encoding="utf-8", errors="replace"))
        if m:
            used.add(m.group(1))
    return used


def new_guid(used: set[str]) -> str:
    while True:
        g = uuid.uuid4().hex
        if g not in used:
            used.add(g)
            return g


def meta_for(path: Path, guid: str) -> str:
    if path.is_dir():
        return META_FOLDER.format(guid=guid)
    ext = path.suffix.lower()
    if ext == ".cs":
        return META_CS.format(guid=guid)
    if ext == ".shader":
        return META_SHADER.format(guid=guid)
    if ext in (".unity", ".asset", ".mat", ".prefab", ".inputactions",
               ".volumeprofile", ".playable", ".controller"):

        if ext == ".unity":
            return META_DEFAULT.format(guid=guid)


        main = 11400000 if ext == ".asset" else 0
        if ext in (".asset", ".mat"):
            return META_NATIVE.format(guid=guid, main_object=main)
        return META_DEFAULT.format(guid=guid)
    return META_DEFAULT.format(guid=guid)


def main() -> int:
    if not ASSETS.is_dir():
        print("Assets/ nao encontrada", file=sys.stderr)
        return 1
    used = collect_used_guids()
    created = 0
    entries = sorted(ASSETS.rglob("*"), key=lambda p: str(p))
    for path in entries:
        if path.name.endswith(".meta"):
            continue
        meta = Path(str(path) + ".meta")
        if meta.exists():
            continue
        guid = new_guid(used)
        meta.write_text(meta_for(path, guid), encoding="utf-8")
        print(f"+ {meta.relative_to(ROOT)}  guid={guid}")
        created += 1
    print(f"{created} .meta(s) criado(s).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
