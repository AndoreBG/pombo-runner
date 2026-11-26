#!/usr/bin/env python3
"""Validação estática do projeto Unity (substitui o compilador neste ambiente).

Checagens:
  1. .meta: todo asset sob Assets/ tem .meta; todo .meta tem asset; GUIDs únicos.
  2. Sintaxe C#: tree-sitter-c-sharp em todos os .cs — zero nós ERROR/MISSING;
     o nome do arquivo deve declarar class/struct/enum/interface correspondente.
  3. Cenar e .asset: todo guid citado existe em algum .meta ou é built-in Unity.
  4. MonoBehaviours em cena: guid -> .cs contém a classe declarada.
  5. EditorBuildSettings: caminhos de cena existem.
  6. Chaves/parênteses balanceados por arquivo .cs (redundante, barato).

Uso: python3 tools/validate.py   -> exit code 0 = verde.
"""
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
except Exception as exc:  # pragma: no cover
    print(f"FALHA ao carregar tree-sitter-c-sharp: {exc}", file=sys.stderr)
    sys.exit(2)

GUID_RE = re.compile(r"guid:\s*([0-9a-f]{32})")
BUILTIN_GUIDS = {
    "0000000000000000e000000000000000",  # built-in assets (meshes, etc.)
    "0000000000000000f000000000000000",  # built-in extra
    "00000000000000000000000000000000",  # none
}

# GUIDs já presentes no commit original do template (referências legítimas a
# scripts/shaders/texturas de pacotes URP/InputSystem que vivem no
# Library/PackageCache, fora do repositório).
_TEMPLATE_REV = "418b2eb1f1f2f335f3ecf745c9289c74b72fc34b"


def load_template_guids() -> set[str]:
    try:
        import subprocess
        out = subprocess.run(
            ["git", "grep", "-h", "-o", "-E", r"\b[0-9a-f]{32}\b", _TEMPLATE_REV],
            capture_output=True, text=True, cwd=ROOT, timeout=30)
        if out.returncode == 0:
            return set(out.stdout.split())
    except Exception:
        pass
    return set()


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


# ---------------------------------------------------------------- 1. metas
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


# ---------------------------------------------------------------- 2. .cs
def check_csharp() -> None:
    for cs in sorted(ASSETS.rglob("*.cs")):
        rel = cs.relative_to(ROOT)
        src = cs.read_bytes()
        tree = PARSER.parse(src)

        # nós de erro
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

        # nome do arquivo declarado?
        text = src.decode("utf-8", "replace")
        stem = cs.stem
        decls = DECL_RE.findall(text)
        if stem not in decls:
            err(f"{rel}: nenhuma declaracao com o nome '{stem}' (Unity exige)")

        # balanceamento barato
        if text.count("{") != text.count("}"):
            err(f"{rel}: chaves desbalanceadas {{={text.count('{')} }}={text.count('}')}")
        if text.count("(") != text.count(")"):
            err(f"{rel}: parenteses desbalanceados")


# ------------------------------------------------- 3/4. refs em cenas/assets
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
                    continue  # referencia legitima a pacote (URP/InputSystem)
                err(f"{rel}: guid desconhecido {g}")
                continue
            target = guid_to_path[g]
            # se aponta para .cs, conferir classe declarada
            if target.suffix == ".cs":
                body = target.read_text(encoding="utf-8", errors="replace")
                cls = target.stem
                if not re.search(rf"\bclass\s+{re.escape(cls)}\b", body):
                    err(f"{rel}: {g} -> {target.relative_to(ROOT)} sem class {cls}")


# ------------------------------------------------------------ 5. build settings
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


# ---------------------------------------------------------------- 6. settings
def check_project_settings() -> None:
    ps = (ROOT / "ProjectSettings" / "ProjectSettings.asset").read_text(
        encoding="utf-8", errors="replace")
    if "activeInputHandler: 1" in ps:
        warn("activeInputHandler ainda = 1 (so Input System); plano exige 2 (Both)")
    if "allowedAutorotateToPortrait: 1" in ps:
        warn("autorotacao para retrato ainda habilitada (plano exige paisagem)")


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
