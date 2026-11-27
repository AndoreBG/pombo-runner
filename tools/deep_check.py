#!/usr/bin/env python3


from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "Assets"

errors: list[str] = []
warnings: list[str] = []


def err(msg: str) -> None:
    errors.append(msg)


def warn(msg: str) -> None:
    warnings.append(msg)


def read(p: Path) -> str:
    return p.read_text(encoding="utf-8", errors="replace")


def check_scene_fileids() -> None:
    for scene in sorted(ASSETS.rglob("*.unity")):
        text = read(scene)
        anchors = set(re.findall(r"^--- !u!\d+ &(\d+)", text, re.M))
        rel = scene.relative_to(ROOT)

        for i, line in enumerate(text.splitlines(), 1):
            for m in re.finditer(r"\{fileID: (\d+)\}", line):
                fid = m.group(1)
                if fid == "0":
                    continue
                if "guid:" in line:
                    continue
                if fid not in anchors:
                    err(f"{rel}:{i}: fileID local {fid} sem âncora no arquivo")

        for m in re.finditer(r"component: \{fileID: (\d+)\}", text):
            if m.group(1) not in anchors:
                err(f"{rel}: component fileID {m.group(1)} sem âncora")


def strip_comments(src: str) -> str:
    src = re.sub(r"/\*.*?\*/", "", src, flags=re.S)
    src = re.sub(r"//[^\n]*", "", src)
    return src


def check_interfaces() -> None:
    iface = read(ASSETS / "Scripts/Input/IFlightInput.cs")
    iface = strip_comments(iface)

    members = set(
        re.findall(
            r"^\s{8}(?:[\w<>\[\],\s]+?)\s+(\w+)\s*(?:\(|\{)", iface, re.M)
    )
    members -= {"get", "set"}
    if not members:
        err("IFlightInput: nenhum membro extraido (regex)")
        return

    for cls_file in ("MobileTiltInput.cs", "EditorInput.cs"):
        src = strip_comments(read(ASSETS / "Scripts/Input" / cls_file))
        missing = []
        for mem in sorted(members):

            if not re.search(rf"\b{mem}\s*\(", src) and \
               not re.search(rf"\b{mem}\s*(\{{|\=)", src):
                missing.append(mem)
        if missing:
            err(f"{cls_file}: nao implementa {missing} de IFlightInput")


    cal = strip_comments(read(ASSETS / "Scripts/Input/SensorCalibration.cs"))
    used = set(re.findall(r"Calibration\.(\w+)\s*\(", strip_comments(
        read(ASSETS / "Scripts/Input/MobileTiltInput.cs"))))
    used |= set(re.findall(r"Calibration\.(\w+)", strip_comments(
        read(ASSETS / "Scripts/Input/MobileTiltInput.cs"))))
    for u in sorted(used):
        if not re.search(rf"\b{u}\b\s*[\(:=]", cal):
            err(f"MobileTiltInput usa Calibration.{u} inexistente")


def check_eventbus_pairing() -> None:
    for cs in sorted(ASSETS.rglob("*.cs")):
        src = strip_comments(read(cs))
        rel = cs.relative_to(ROOT)
        subs = re.findall(r"EventBus\.Subscribe<(\w+)>\(([\w.]+)\)", src)
        unsubs = re.findall(r"EventBus\.Unsubscribe<(\w+)>\(([\w.]+)\)", src)
        if not subs and not unsubs:
            continue
        from collections import Counter
        cs_sub, cs_un = Counter(subs), Counter(unsubs)
        for key, n in cs_sub.items():
            if cs_un.get(key, 0) < n:
                err(f"{rel}: Subscribe<{key[0]}> x{n} sem Unsubscribe pareado "
                    f"({key[1]})")
        for key, n in cs_un.items():
            if cs_sub.get(key, 0) < n:
                err(f"{rel}: Unsubscribe<{key[0]}> x{n} sem Subscribe "
                    f"({key[1]})")


def extract_class_members() -> dict[str, set[str]]:

    out: dict[str, set[str]] = {}
    for cs in sorted(ASSETS.rglob("*.cs")):
        src = strip_comments(read(cs))
        for m in re.finditer(
                r"\b(?:class|interface|struct|enum)\s+(\w+)[^{]*\{", src):
            name = m.group(1)

            body = src[m.end():]
            members = set(re.findall(
                r"\b(?:public|private|protected|internal)?\s*[\w<>\[\],\s\.]+?"
                r"\s+(\w+)\s*(?:\(|\{|\=)", body))

            members -= {"get", "set", "if", "for", "while", "switch", "return",
                        "new", "using", "else", "foreach", "catch", "this"}
            out.setdefault(name, set()).update(members)
    return out


def check_calls() -> None:
    members_by_type = extract_class_members()


    type_map: dict[str, str] = {}
    all_src = "\n".join(strip_comments(read(c)) for c in sorted(ASSETS.rglob("*.cs")))
    for t, v in re.findall(r"\b([A-Z]\w+)\s+(\w+)\s*=\s*new\s+\w+", all_src):
        type_map[v] = t
    for t, v in re.findall(r"\b([A-Z]\w+)\s+(\w+)\s*[;,)]", all_src):
        type_map.setdefault(v, t)


    universal = {"ToString", "Equals", "GetHashCode", "GetType", "Clone"}

    known_calls = {
        "_gm": "GameManager", "gm": "GameManager", "GameManager": "GameManager",
        "_gm.Calibration": "CalibrationUI",
        "_flight": "PigeonFlightController",
    }


    receiver_types = {
        "gm": "GameManager",
        "_gm": "GameManager",
        "spawner": "SpawnManager",
        "score": "ScoreManager",
        "difficulty": "DifficultyManager",
        "energy": "EnergySystem",
        "flight": "PigeonFlightController",
        "collisions": "CollisionHandler",
        "hud": "HUD",
        "calibration": "CalibrationUI",
        "input": "IFlightInput",
        "Calibration": "CalibrationUI",
        "Hud": "GameManager",
    }


    gm_members = members_by_type.get("GameManager", set())


    receiver_types.pop("Calibration", None)
    receiver_types.pop("Hud", None)

    for cs in sorted(ASSETS.rglob("*.cs")):
        src = strip_comments(read(cs))
        rel = cs.relative_to(ROOT)


        local_types: dict[str, str] = {}
        for typ, name in re.findall(
                r"\b([A-Z]\w*)\s+(\w+)\s*(?:=|\{|;)", src):
            local_types.setdefault(name, typ)

        for recv, member in re.findall(r"\b(\w+)\??\.(\w+)\s*\(", src):
            if recv in ("EventBus", "Input", "PlayerPrefs", "Time", "Random",
                        "Mathf", "Vector2", "Vector3", "Quaternion", "Shader",
                        "SceneManager", "Application", "Debug", "Object",
                        "Resources", "Camera", "System", "Player", "Screen",
                        "RenderSettings", "Transform", "GameObject", "Color",
                        "Rigidbody", "Collider", "BoxCollider", "Material",
                        "Stack", "Dictionary", "Delegate", "Action", "Text",
                        "Image", "Button", "Canvas", "EventSystem", "Font",
                        "WaitForSeconds", "Coroutine", "IEnumerator"):
                continue
            if recv in ("this", "base"):
                continue
            if member in universal:
                continue
            t = local_types.get(recv) or receiver_types.get(recv) \
                or type_map.get(recv)
            if t is None:
                continue
            members = members_by_type.get(t)
            if members is None:
                continue
            if member not in members:
                if member in ("GetComponent", "AddComponent", "SetActive",
                              "CompareTag", "SendMessage", "Invoke"):
                    continue
                err(f"{rel}: {recv}.{member}() — tipo {t} nao declara '{member}'")


    for cs in sorted(ASSETS.rglob("*.cs")):
        src = strip_comments(read(cs))
        rel = cs.relative_to(ROOT)
        for recv, member in re.findall(
                r"\b(UiFactory|MaterialFactory|ObstacleFactory|WorldBuilder|"
                r"PigeonBuilder)\??\.(\w+)\b", src):
            members = members_by_type.get(recv, set())
            if member not in members:
                err(f"{rel}: {recv}.{member} — {recv} nao declara '{member}'")


    for cs in sorted(ASSETS.rglob("*.cs")):
        src = strip_comments(read(cs))
        rel = cs.relative_to(ROOT)
        for prop, member in re.findall(r"\b_gm\??\.(\w+)\??\.(\w+)", src):
            if prop not in gm_members:
                err(f"{rel}: _gm.{prop} inexistente em GameManager")
                continue

            prop_type = {
                "Score": "ScoreManager", "Difficulty": "DifficultyManager",
                "Flight": "PigeonFlightController", "Energy": "EnergySystem",
                "Spawner": "SpawnManager", "Hud": "HUD",
                "Calibration": "CalibrationUI", "Collisions": "CollisionHandler",
                "FlightInput": "IFlightInput",
            }.get(prop)
            if prop_type:
                tm = members_by_type.get(prop_type, set())
                if member not in tm:
                    err(f"{rel}: _gm.{prop}.{member} — {prop_type} nao declara "
                        f"'{member}'")


def check_shaders() -> None:
    for shader in sorted(ASSETS.rglob("*.shader")):
        src = read(shader)
        rel = shader.relative_to(ROOT)
        if src.count("{") != src.count("}"):
            err(f"{rel}: chaves desbalanceadas")
        for token in ("SubShader", "Pass", "HLSLPROGRAM", "ENDHLSL", "Properties"):
            if token not in src:
                err(f"{rel}: token ausente {token}")
        if src.count("HLSLPROGRAM") != src.count("ENDHLSL"):
            err(f"{rel}: HLSLPROGRAM/ENDHLSL desbalanceados")
        if 'RenderPipeline" = "UniversalPipeline"' not in src and \
           'RenderPipeline"="UniversalPipeline"' not in src:
            err(f"{rel}: tag RenderPipeline ausente")

        for prop in set(re.findall(r'SetColor\("(_\w+)"', "\n".join(
                read(c) for c in ASSETS.rglob("*.cs")))):
            declared = re.findall(r"^\s+(_\w+)\s*\(", src, re.M)
            if prop in ("_BaseColor",) and prop not in declared and \
               prop not in src:
                err(f"{rel}: propriedade {prop} nao declarada")

        globals_used = set(re.findall(r"\b(_Pombo\w+)", src))
        code = "\n".join(read(c) for c in ASSETS.rglob("*.cs"))
        for g in globals_used:
            if g not in code:
                warn(f"{rel}: global {g} nunca setada no codigo C#")


def check_constants_vs_docs() -> None:
    gdd = read(ROOT / "docs/gdd.md")
    energy = strip_comments(read(ASSETS / "Scripts/Player/EnergySystem.cs"))
    diff = strip_comments(read(ASSETS / "Scripts/Gameplay/DifficultyManager.cs"))
    flight = strip_comments(read(ASSETS / "Scripts/Player/PigeonFlightController.cs"))

    checks = [
        ("CruiseDrain", energy, r"CruiseDrain\s*=\s*([\d.]+)", "−1,5 /s", r"−1,5"),
        ("GlideDrain", energy, r"GlideDrain\s*=\s*([\d.]+)", "−0,7 /s", r"0,7"),
        ("FlapCost", energy, r"FlapCost\s*=\s*([\d.]+)", "−3 por toque", r"−3"),
        ("TurbulenceCost", energy, r"TurbulenceCost\s*=\s*([\d.]+)", "−25 pontos ao entrar", r"−25 pontos ao entrar"),
        ("SpeedMin", diff, r"SpeedMin\s*=\s*([\d.]+)", "9 m/s", r"\b9 m/s"),
        ("SpeedMax", diff, r"SpeedMax\s*=\s*([\d.]+)", "24 m/s", r"24 m/s"),
        ("SpawnMin", diff, r"SpawnMin\s*=\s*([\d.]+)", "1,55 s", r"1,55"),
        ("SpawnMax", diff, r"SpawnMax\s*=\s*([\d.]+)", "0,62 s", r"0,62"),
        ("DoubleMax", diff, r"DoubleMax\s*=\s*([\d.]+)", "45 %", r"45"),
        ("gravityNormal", flight, r"gravityNormal\s*=\s*([\d.]+)", "gravidade 9.5", r"9.5|9,5"),
        ("gravityGlide", flight, r"gravityGlide\s*=\s*([\d.]+)", "glide 2.4", r"2.4|2,4"),
        ("flapBoost", flight, r"flapBoost\s*=\s*([\d.]+)", "+6,5", r"6[.,]5"),
    ]
    for name, src, rx, claim, doc_rx in checks:
        m = re.search(rx, src)
        if not m:
            err(f"C6: constante {name} nao encontrada no codigo")
            continue
        if not re.search(doc_rx, gdd):
            err(f"C6: GDD nao menciona {claim} (constante {name}={m.group(1)})")


    if "EndRun" in energy:
        err("C6: EnergySystem nao deveria chamar EndRun (design: sem morte por 0)")


def check_hygiene() -> None:
    seen_classes: dict[str, Path] = {}
    for cs in sorted(ASSETS.rglob("*.cs")):
        rel = cs.relative_to(ROOT)
        src = read(cs)
        stripped = strip_comments(src)


        for i, line in enumerate(src.splitlines(), 1):
            if "using UnityEditor" in line and "#if UNITY_EDITOR" not in line:

                if "#if UNITY_EDITOR" not in src.split(line)[0]:
                    err(f"{rel}:{i}: using UnityEditor sem #if UNITY_EDITOR")


        if not re.search(
                rf"\b(?:class|interface|struct|enum)\s+{re.escape(cs.stem)}\b",
                stripped):
            err(f"{rel}: filename sem declaracao correspondente")


        for m in re.finditer(r"\bclass\s+(\w+)", stripped):
            n = m.group(1)
            if n in seen_classes and seen_classes[n] != cs:
                err(f"class duplicada {n}: {seen_classes[n]} e {cs}")
            seen_classes[n] = cs


        is_mb = "MonoBehaviour" in stripped
        if not is_mb:
            for magic in ("Awake", "Start", "Update", "FixedUpdate",
                          "OnEnable", "OnDisable", "OnDestroy",
                          "OnTriggerEnter", "OnTriggerExit"):
                if re.search(rf"void\s+{magic}\s*\(", stripped):
                    err(f"{rel}: metodo {magic} em classe que nao herda "
                        f"MonoBehaviour")


        if re.search(r"void\s+OnTrigger", stripped) and not is_mb:
            err(f"{rel}: OnTriggerEnter fora de MonoBehaviour")


def check_docs() -> None:
    readme = read(ROOT / "README.md")
    for doc in ("docs/gdd.md", "docs/arquitetura.md",
                "docs/plano-implementacao.md", "docs/plano-de-testes.md",
                "docs/post-mortem-template.md", "docs/apresentacao.md"):
        if not (ROOT / doc).exists():
            err(f"doc ausente: {doc}")
        elif not read(ROOT / doc).strip():
            err(f"doc vazio: {doc}")

    for link in re.findall(r"\]\((docs/[\w./-]+|tools/[\w./-]+)\)", readme):
        if not (ROOT / link).exists():
            err(f"README link quebrado: {link}")

    for scene in re.findall(r"Assets/Scenes/(\w+)\.unity", readme):
        if not (ASSETS / "Scenes" / f"{scene}.unity").exists():
            err(f"README cita cena inexistente: {scene}")


def main() -> int:
    print("== verificacao profunda ==")
    check_scene_fileids();   print("[C1] fileIDs das cenas")
    check_interfaces();      print("[C2] interfaces (IFlightInput)")
    check_eventbus_pairing(); print("[C3] pareamento EventBus")
    check_calls();           print("[C4] chamadas x definicoes")
    check_shaders();         print("[C5] shaders")
    check_constants_vs_docs(); print("[C6] constantes codigo x GDD")
    check_hygiene();         print("[C7] higiene C#")
    check_docs();            print("[C8] documentacao/links")

    for w in warnings:
        print(f"  AVISO: {w}")
    if errors:
        print(f"\nFALHAS ({len(errors)}):")
        for e in errors:
            print(f"  - {e}")
        return 1
    print("\nOK: nenhuma falha na verificacao profunda.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
