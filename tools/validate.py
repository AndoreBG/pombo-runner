#!/usr/bin/env python3


from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "Assets"

try:
    from tree_sitter import Language, Parser
    import tree_sitter_c_sharp
    LANGUAGE = Language(tree_sitter_c_sharp.language())
    PARSER = Parser(LANGUAGE)
except Exception as exc:
    print(f"FALHA ao carregar tree-sitter-c-sharp: {exc}", file=sys.stderr)
    sys.exit(2)

GUID_RE = re.compile(r"guid:\s*([0-9a-f]{32})")
BUILTIN_GUIDS = {
    "0000000000000000e000000000000000",
    "0000000000000000f000000000000000",
    "00000000000000000000000000000000",
}


def load_template_guids() -> set[str]:
    path = ROOT / "tools" / "template-guids.txt"
    if not path.exists():
        raise FileNotFoundError("Lista de GUIDs do template ausente: " + str(path))
    return set(re.findall(r"\b[0-9a-f]{32}\b", path.read_text(encoding="utf-8")))


TEMPLATE_GUIDS = load_template_guids()
DECL_RE = re.compile(
    r"\b(?:class|struct|interface|enum)\s+(?:sealed\s+|static\s+|abstract\s+|partial\s+)*([A-Za-z_][A-Za-z0-9_]*)"
)

errors: list[str] = []
warnings: list[str] = []


def err(msg: str) -> None:
    errors.append(msg)


def warn(msg: str) -> None:
    warnings.append(msg)


def check_metas() -> dict[str, Path]:
    guid_to_path: dict[str, Path] = {}
    if not ASSETS.is_dir():
        err("Assets/ nao existe")
        return guid_to_path

    for path in sorted(ASSETS.rglob("*")):
        if path.name.endswith(".meta"):
            continue
        meta = Path(str(path) + ".meta")
        if not meta.exists():
            err(f"meta ausente: {path.relative_to(ROOT)}")
            continue
        text = meta.read_text(encoding="utf-8", errors="replace")
        m = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", text, re.M)
        if not m:
            err(f"guid invalido/ausente em {meta.relative_to(ROOT)}")
            continue
        g = m.group(1)
        if g in guid_to_path:
            err(f"GUID duplicado {g}: {guid_to_path[g].relative_to(ROOT)} e {path.relative_to(ROOT)}")
        guid_to_path[g] = path

    for meta in sorted(ASSETS.rglob("*.meta")):
        asset = meta.parent / meta.name[: -len(".meta")]
        if not asset.exists():
            err(f"meta orfao: {meta.relative_to(ROOT)}")
    return guid_to_path


def check_csharp() -> None:
    for cs in sorted(ASSETS.rglob("*.cs")):
        rel = cs.relative_to(ROOT)
        src = cs.read_bytes()
        tree = PARSER.parse(src)


        bad: list[tuple[int, str]] = []

        def walk(node) -> None:
            if node.type == "ERROR" or node.is_missing:
                line = node.start_point[0] + 1
                snippet = src[node.start_byte: min(node.end_byte, node.start_byte + 80)]
                bad.append((line, snippet.decode("utf-8", "replace").replace("\n", "\\n")))
                return
            for child in node.children:
                walk(child)

        walk(tree.root_node)
        for line, snippet in bad[:8]:
            err(f"sintaxe {rel}:{line}: {snippet}")


        text = src.decode("utf-8", "replace")
        stem = cs.stem
        decls = DECL_RE.findall(text)
        if stem not in decls:
            err(f"{rel}: nenhuma declaracao com o nome '{stem}' (Unity exige)")


        if text.count("{") != text.count("}"):
            err(f"{rel}: chaves desbalanceadas {{={text.count('{')} }}={text.count('}')}")
        if text.count("(") != text.count(")"):
            err(f"{rel}: parenteses desbalanceados")


def check_references(guid_to_path: dict[str, Path]) -> None:
    ref_files = list(ASSETS.rglob("*.unity")) + list(ASSETS.rglob("*.asset")) + \
        list(ASSETS.rglob("*.prefab")) + list(ASSETS.rglob("*.mat"))
    for f in sorted(ref_files):
        if f.name.endswith(".meta"):
            continue
        rel = f.relative_to(ROOT)
        text = f.read_text(encoding="utf-8", errors="replace")
        for g in set(GUID_RE.findall(text)):
            if g in BUILTIN_GUIDS:
                continue
            if g not in guid_to_path:
                if g in TEMPLATE_GUIDS:
                    continue
                err(f"{rel}: guid desconhecido {g}")
                continue
            target = guid_to_path[g]

            if target.suffix == ".cs":
                body = target.read_text(encoding="utf-8", errors="replace")
                cls = target.stem
                if not re.search(rf"\bclass\s+{re.escape(cls)}\b", body):
                    err(f"{rel}: {g} -> {target.relative_to(ROOT)} sem class {cls}")


def check_build_settings() -> None:
    ebs = ROOT / "ProjectSettings" / "EditorBuildSettings.asset"
    if not ebs.exists():
        err("EditorBuildSettings.asset ausente")
        return
    text = ebs.read_text(encoding="utf-8", errors="replace")
    paths = re.findall(r"^\s*path:\s*(\S+)\s*$", text, re.M)
    if not paths:
        err("EditorBuildSettings sem cenas")
    for p in paths:
        if not (ROOT / p).exists():
            err(f"EditorBuildSettings: cena inexistente {p}")


def check_project_settings() -> None:
    ps = (ROOT / "ProjectSettings" / "ProjectSettings.asset").read_text(
        encoding="utf-8", errors="replace")
    if "activeInputHandler: 1" not in ps:
        err("activeInputHandler deve ser 1 (apenas New Input System) para Android")
    if "allowedAutorotateToPortrait: 1" in ps:
        warn("autorotacao para retrato ainda habilitada (plano exige paisagem)")
    for cs in ASSETS.rglob("*.cs"):
        if re.search(r"\b(?:UnityEngine\.)?Input\s*\.", cs.read_text(encoding="utf-8")):
            err(f"{cs.relative_to(ROOT)}: uso da API legada UnityEngine.Input")


def main() -> int:
    print("== validacao estatica do projeto ==")
    guid_to_path = check_metas()
    print(f"[1] metas: {len(guid_to_path)} GUIDs unicos")
    check_csharp()
    n_cs = len(list(ASSETS.rglob('*.cs'))) if ASSETS.is_dir() else 0
    print(f"[2] .cs analisados: {n_cs}")
    check_references(guid_to_path)
    print("[3/4] referencias de cena/asset verificadas")
    check_build_settings()
    print("[5] EditorBuildSettings verificado")
    check_project_settings()
    print("[6] ProjectSettings checado")

    for w in warnings:
        print(f"  AVISO: {w}")
    if errors:
        print(f"\nFALHAS ({len(errors)}):")
        for e in errors:
            print(f"  - {e}")
        return 1
    print("\nOK: nenhuma falha.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
