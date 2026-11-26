# GDD — 🏙 Pigeon Rooftop Rush (Projeto 2)

**Gênero:** endless flyer / arcade 3D · **Orientação:** paisagem · **Dificuldade:** média
**Mobile:** acelerômetro + giroscópio + touch (+ haptics pós-MVP)
**Plataforma alvo:** Android (ARM64, IL2CPP)

---

## 1. Elevator Pitch

Um pombo precisa voar o máximo possível pela cidade, desviando de fios, prédios,
placas, drones e árvores enquanto procura comida e aproveita correntes de ar.

> **Diferencial:** o smartphone **se transforma no controle de voo** — inclinar o
> aparelho é virar, subir e mergulhar; tocar é bater asas; pressionar é planar.

## 2. Fantasia do jogador

“Sou um pombo ágil que lê as correntes de ar da cidade e nunca bate.”

## 3. Core Loop

```
Voar → desviar → coletar (correntes) → gerenciar energia → aumentar score
     → aumentar velocidade → sobreviver
```

## 4. Controles

### 4.1 Inclinação (principal — sensor real)

| Ação no aparelho | Efeito |
|---|---|
| Inclinar ← / → | virar (X lateral) |
| Inclinar ↑ / ↓ | subir / mergulhar (Y vertical) |

- **Acionamento:** `Input.acceleration` com `Input.compensatingSensors = true`
  (o Unity compensa a orientação da tela — o giroscópio do aparelho entra nessa
  compensação) e **calibragem de neutro** antes de cada partida.
- Dead-zone 0,09 · sensibilidade 1,7 · leitura ao vivo no CalibrationUI.
- Correção em dispositivo: botões INVERTER X / INVERTER Y / TROCAR EIXOS.

### 4.2 Touch

| Toque | Ação |
|---|---|
| Tap | bater asas (impulso vertical, −6 energia) |
| Segurar > 0,22 s | planar (gravidade reduzida, consumo mínimo) |

### 4.3 Editor (teclado)

`WASD`/setas = inclinar · `Espaço` = tap · `Shift` = hold.

## 5. Energia

Energia 0–100, começa cheia. **Sem energia não há morte direta**: o flap é bloqueado e
o jogador precisa alcançar uma corrente azul antes de bater no chão.

| Ação | Efeito |
|---|---|
| Voo normal | −3,0 /s |
| Tap (bater asas) | −6 por toque |
| Planar (hold) | −1,4 /s |
| Corrente **azul** (ascendente) | **+9,0 /s** e gravidade ×0,3 |
| Corrente **vermelha** (turbulência) | **−8,0 /s**, gravidade ×1,6, tremor de câmera e sacudida no controle |
| Energia = 0 | sem flap; voo normal continua (mergulho controlado) |

Barra de energia no rodapé; “SEM ENERGIA!” pisca ao tentar tap sem energia.

## 6. Obstáculos (5 no MVP)

| # | Obstáculo | Geometria | Esquiva |
|---|---|---|---|
| 1 | **Prédio** | caixa 11–16 m de altura desde o chão | por cima |
| 2 | **Antena** | mastro fino 9–13 m + base + ponteira vermelha | lateral |
| 3 | **Fio elétrico** | barra atravessando a pista inteira (altura 4–16 m) | por cima ou por baixo |
| 4 | **Outdoor** | placa flutuante 6,5 m (altura 5,5–13 m) | por cima/baixo/lado |
| 5 | **Árvore** | tronco + copa (~6 m) | por cima/lado |

- Lanes de spawn: x ∈ {−6, −3, 0, 3, 6}; o fio sempre ocupa o centro.
- Fileiras simples ou **duplas com corredor garantido** (distância ≥ 2 lanes) conforme
  a dificuldade sobe.
- Pós-MVP: drones, janelas, outros pássaros.

## 7. Correntes de ar (2 no MVP)

| Cor | Tipo | Efeito | Visual |
|---|---|---|---|
| **Azul** `#3EC6FF` | ascendente | +9/s energia, gravidade ×0,3 | coluna translúcida com listras subindo (aceleram dentro) |
| **Vermelha** `#FF5252` | turbulência | −8/s, gravidade ×1,6, tremor | coluna translúcida com listras tremendo |

- Volume trigger 10 × 18 × 6; spawn em x aleatório a z = 75.
- Pós-MVP: corrente **verde** favorável (velocidade/eco).

## 8. Score e progressão

- `score = floor(distância em m) + 25 × obstáculos ultrapassados`
- Recorde em `PlayerPrefs` (`PRR_BestScore`), exibido no menu, no HUD e no game over.
- **Dificuldade (curvas da run):**

| Parâmetro | Início | Máximo (~150 s) |
|---|---|---|
| Velocidade do mundo | 9 m/s | 24 m/s |
| Intervalo de spawn | 1,55 s | 0,62 s |
| Chance de fileira dupla | 0 % | 45 % |
| Intervalo de correntes | 8 s | 5 s |

- Câmera: segue o X do pombo, ganha FOV com a velocidade (62→74) e treme na
  turbulência.
- Pós-MVP (zonas do GDD): Centro → Bairro residencial → Parque → Zona industrial →
  Centro noturno, cada um com paleta/obstáculos próprios.

## 9. Power-ups (pós-MVP, não implementados)

Super Planador · Pena Dourada · Escudo · Migalhas Magnéticas.
O script `PowerupManager.cs` aparece na lista do PDF, mas **fora do MVP escolhido** —
fica no roadmap.

## 10. Fluxo de cenas

```
MainMenu ──[JOGAR]──▶ Game
   │                   │
   │            Calibragem (overlay)
   │                   │ [COMEÇAR]
   │                 Playing
   │                   │ crash (obstáculo/chão)
   │                 GameOver (overlay)
   │                   ├─[JOGAR NOVAMENTE]─▶ recarrega Game
   └───────────────────┴─[MENU PRINCIPAL]──▶ MainMenu
```

## 11. MVP — checklist de cobertura

| Item do PDF | Onde está |
|---|---|
| voo 3D | `PigeonFlightController` |
| sensor de inclinação | `MobileTiltInput` + `SensorCalibration` |
| calibragem | `CalibrationUI` + `MobileTiltInput.Calibrate()` |
| cinco obstáculos | `ObstacleFactory` (building/antenna/wire/billboard/tree) |
| duas correntes de ar | `ObstacleFactory.CreateWind` + `WindZone` (Ascending/Turbulence) |
| energia | `EnergySystem` + barra no `HUD` |
| score | `ScoreManager` + recorde |
| pooling | `ObjectPool` + `SpawnManager` (9 pools com prewarm) |
| build Android | `ProjectSettings` (paisagem, IL2CPP, ARM64, package id) |

## 12. Roadmap pós-MVP

1. Haptics (`Handheld.Vibrate` em crash/quase-tiro) e sons.
2. Gestos de swipe: mergulho rápido (swipe down), frear (swipe up).
3. Corrente verde + power-ups (4) + `PowerupManager`.
4. Zonas visuais (5) com paleta e mix de obstáculos próprios.
5. Drones, janelas e outros pássaros (obstáculos móveis).
6. Pausa, tutorial de primeira partida, dificuldade por perfil.
7. Otimizações: occlusion/distance culling por tipo, SpriteAtlas de UI, batch custom.

## 13. Arte e áudio (estado atual)

- **Primitivas estilizadas** (cubo/cápsula/esfera/plano) com materiais de cor dos
  shaders próprios `Pombo/Flat` e `Pombo/Transparent` — sem assets externos.
- Sombra blob (quad) sob o pombo e sob obstáculos; névoa linear até a distância de
  spawn (z = 70) para esconder aparição.
- Áudio: nenhum no MVP (pós-MVP).
