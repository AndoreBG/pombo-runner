# 🏙 Pigeon Rooftop Rush — Projeto 2

**Endless flyer arcade 3D para Android** em que o smartphone vira o controle de voo:
incline o aparelho para virar, subir e mergulhar; toque para bater asas; segure para
planar. Desvie de prédios, antenas, fios, outdoors e árvores, aproveite correntes de ar
e leve o pombo o mais longe possível.

> Projeto 2 do desafio de jogos móveis (veja o enunciado em anexo e o
> [GDD completo](docs/gdd.md)). Engine: **Unity 6000.3.18f1 (URP)**.

## 📁 Como abrir

1. Instale o **Unity Hub** e a edição **6000.3.18f1** (a versão exata do projeto —
   outras versões 6000.x podem abrir, mas não são testadas).
2. `Add → Add project from disk` nesta pasta e abra o projeto.
3. Aguarde a importação inicial (URP + Input System já estão no `Packages/manifest.json`).
4. Abra a cena **`Assets/Scenes/MainMenu.unity`** e aperte **Play**.

> **Dica:** o projeto usa *Enter Play Mode Options* com domain reload desligado
> (configuração do template). Tudo continua funcionando — os handlers do EventBus são
> removidos em `OnDisable` — mas se notar comportamento estranho após muitos Plays,
> desligue e ligue `Edit → Preferences → Enter Play Mode Options`.

## 🎮 Controles

| Ação | Celular | Editor (teclado) |
|---|---|---|
| Virar ← / → | inclinar o aparelho | `A`/`D` ou setas |
| Subir / mergulhar | inclinar ↑ / ↓ | `W`/`S` ou setas |
| Bater asas | **toque** na tela | `Espaço` |
| Planar | **segure** o toque (> 0,22 s) | `Shift` |

**Calibragem:** cada partida começa com um overlay — segure o aparelho na posição
natural e toque em **COMEÇAR**. Se os eixos saírem trocados no seu aparelho, use os
botões **INVERTER X / INVERTER Y / TROCAR EIXOS** ali mesmo (afeta apenas a sessão).
Para mudar o padrão permanentemente, ajuste `invertX/invertY/swapXY` em
`Assets/Scripts/Input/SensorCalibration.cs`.

## 🏗️ Build Android (APK)

1. `File → Build Settings`.
2. Selecione **Android** → **Switch Platform**.
3. Verifique em `Player Settings`:
   - Orientation: **Landscape Left/Right** (o projeto já vem configurado só-paisagem);
   - Scripting Backend: **IL2CPP** + Target Architectures: **ARM64** (já configurado);
   - Package Name: `com.pomborunner.pigeonrooftoprush` (já configurado).
4. Instale pelo Hub os módulos **Android Build Support + SDK + NDK + OpenJDK** se ainda
   não tiver.
5. **Build** (ou Build And Run com o aparelho em depuração USB).
   - Para testar rápido: **Build Settings → Build** gera o `.apk`; instale tocando nele
     no aparelho.

## 🕹️ O que está no MVP

- Voo 3D arcade com **inclinação do aparelho** (acelerômetro compensado + giroscópio) e
  **calibragem**;
- **5 obstáculos**: prédio, antena, fio elétrico, outdoor, árvore;
- **2 correntes de ar**: azul (sobe + recupera energia) e vermelha (tremor + drena);
- **Energia** com decisões (voar × planar × correntes);
- **Score** (distância + bônus) e recorde em `PlayerPrefs`;
- **Object pooling** em todos os spawns;
- Cenas **MainMenu + Game** (game over como overlay);
- **Build Android** paisagem com todas as configurações prontas.

Itens do GDD deixados para pós-MVP (power-ups, 5 zonas, haptics, áudio, drones etc.)
estão listados em [`docs/gdd.md`](docs/gdd.md).

## 🗂️ Estrutura do projeto

```
Assets/
├── Scenes/
│   ├── MainMenu.unity        # YAML mínimo + MainMenuBootstrap
│   └── Game.unity            # YAML mínimo + GameBootstrap
├── Shaders/
│   ├── PomboFlat.shader      # lambert próprio (luz/nevoa via globals)
│   └── PomboTransparent.shader
├── Settings/                 # assets URP do template (mobile/PC)
└── Scripts/
    ├── Core/     GameManager, EventBus, MaterialFactory
    ├── Input/    IFlightInput, MobileTiltInput, EditorInput, SensorCalibration
    ├── Player/   PigeonFlightController, EnergySystem, CollisionHandler, PigeonBuilder
    ├── World/    SpawnManager, ObjectPool, WindZone, ObstacleFactory, WorldBuilder,
    │             ObstacleMarker
    ├── Gameplay/ ScoreManager, DifficultyManager
    ├── UI/       HUD, CalibrationUI, MainMenuUI, UiFactory
    └── Boot/     GameBootstrap, MainMenuBootstrap
```

**Arquitetura em uma frase:** as cenas YAML contêm apenas câmera/luz/volume/bootstrap;
todo o resto (jogador, pools, HUD, cenário) é montado **em runtime** pelos bootstraps —
isso elimina erros de referência/GUID autorados à mão e deixa o jogo 100% versionável
sem depender de biblioteca gerada. Detalhes e diagramas em
[`docs/arquitetura.md`](docs/arquitetura.md).

## 🛠️ Ferramentas (validação sem Unity)

Como o código pode ser escrito fora do editor, o repositório traz validação estática:

```bash
python3 -m pip install --user tree-sitter tree-sitter-c-sharp pyyaml
python3 tools/validate.py     # 0 = verde: sintaxe C#, .metas, GUIDs, cenas, build settings
python3 tools/deep_check.py   # 0 = verde: verificação profunda (fileIDs, API de input, eventos, shaders, GDD)
python3 tools/gen_meta.py     # cria .meta faltantes (idempotente)
python3 tools/gen_scenes.py   # regenera as 2 cenas a partir do template validado
```

## 📖 Documentos

| Documento | Conteúdo |
|---|---|
| [`docs/gdd.md`](docs/gdd.md) | GDD final do Projeto 2 |
| [`docs/arquitetura.md`](docs/arquitetura.md) | Diagramas (Mermaid) e padrões |
| [`docs/plano-implementacao.md`](docs/plano-implementacao.md) | Plano de implementação seguido |
| [`docs/plano-de-testes.md`](docs/plano-de-testes.md) | Checklist de teste em dispositivo |
| [`docs/post-mortem-template.md`](docs/post-mortem-template.md) | Template de post-mortem |
| [`docs/apresentacao.md`](docs/apresentacao.md) | Roteiro da apresentação (~10 min) |

## ⚠️ Notas de teste

- **Eixos do tilt:** o mapeamento usa `Input.compensatingSensors` + calibragem; os
  padrões `invertX/invertY` foram derivados analiticamente e **precisam de confirmação
  em dispositivo** (item 1 do plano de testes). O CalibrationUI mostra os eixos ao vivo
  e permite inverter em dispositivo.
- **Colisões finas:** `fixedDeltaTime` foi fixado em 60 Hz e os fios têm 0,5 m de
  espessura para evitar tunelamento a 24 m/s.
- Este ambiente de desenvolvimento não tem Unity: o projeto foi validado estaticamente
  (sintaxe via tree-sitter, GUIDs e cenas via `tools/validate.py`). Qualquer erro de
  compilação/console reportado é corrigido na sequência.
