# Plano de Testes — Pigeon Rooftop Rush (MVP)

**Como usar:** execute em ordem e marque cada item. Qualquer falha → registre
(comportamento esperado × obtido, aparelho, prints/log) e corrija antes de avançar.
O **item 1 (eixos do tilt)** é o mais crítico — foi derivado sem teste em dispositivo.

---

## Fase 0 — Importação (Unity Editor)

| # | Teste | Esperado | ✓ |
|---|---|---|---|
| 0.1 | Abrir o projeto na 6000.3.18f1 | importa sem erros no Console | ☐ |
| 0.2 | Console limpo após importação | 0 erros vermelhos (warnings do template são aceitáveis) | ☐ |
| 0.3 | `Assets/Scenes/MainMenu.unity` e `Game.unity` abrem | sem “Missing Script” | ☐ |
| 0.4 | File → Build Settings | cenas MainMenu (0) e Game (1) | ☐ |

## Fase 1 — Editor (teclado)

| # | Teste | Esperado | ✓ |
|---|---|---|---|
| 1.1 | Play no MainMenu | título, recorde, 3 botões; sem erros | ☐ |
| 1.2 | COMO JOGAR → abre/fecha | painel alterna | ☐ |
| 1.3 | SAIR (no editor) | para o Play | ☐ |
| 1.4 | JOGAR | carrega Game; overlay de calibragem; pombo parado | ☐ |
| 1.5 | COMEÇAR | HUD aparece; mundo começa a mover; spawn inicial ~1,3 s | ☐ |
| 1.6 | `A/D` e `W/S` | pombo move lateral/verticalmente na direção esperada | ☐ |
| 1.7 | `Espaço` | impulso de subida + animação de asas; −6 energia (barra cai) | ☐ |
| 1.7a | `Espaço` com energia 0 | “SEM ENERGIA!” pisca; sem impulso | ☐ |
| 1.8 | Segurar `Shift` | plana (queda lenta); consumo menor que voo normal | ☐ |
| 1.9 | Bater em obstáculo | game over “Você bateu!”; pontuação salva | ☐ |
| 1.10 | Mergulhar até o chão | game over “Você caiu!” | ☐ |
| 1.11 | JOGAR NOVAMENTE | reinicia zerado (pontos 0, energia 100) | ☐ |
| 1.12 | MENU PRINCIPAL | volta ao menu; recorde mantido | ☐ |
| 1.13 | Rodar 2 min sem bater | velocidade sobe (9→~19 m/s); spawns mais frequentes; fileiras duplas aparecem | ☐ |
| 1.14 | Passar por todos os 5 obstáculos | colisão funciona em todos (wire, building, antenna, billboard, tree) | ☐ |
| 1.15 | Entrar na corrente azul | barra sobe; voo fica leve (flap forte) | ☐ |
| 1.16 | Entrar na vermelha | barra cai; câmera treme; controle sacode | ☐ |
| 1.17 | Esgotar energia e pegar azul | recupera e continua a run | ☐ |
| 1.18 | Recorde novo | “NOVO RECORDE!” no game over; menu atualiza | ☐ |
| 1.19 | Profiler (opcional) | sem allocations gigantes por frame nos pools (após prewarm) | ☐ |

## Fase 2 — Dispositivo Android (crítico)

Pré-requisito: build APK instalado; aparelho com giroscópio (qualquer smartphone moderno).

### 2.1 Eixos do tilt ⚠️

| # | Teste | Esperado | ✓ |
|---|---|---|---|
| 2.1.1 | Abrir o jogo → calibragem | “ENTRADA: Acelerômetro + touch”; eixos ~0,00 parado | ☐ |
| 2.1.2 | Inclinar para a **direita** (lado direito mais baixo) | **X fica positivo**; no jogo o pombo vai à direita | ☐ |
| 2.1.3 | Inclinar o **topo para cima** (erguer a borda distante) | **Y fica positivo**; no jogo o pombo sobe | ☐ |
| 2.1.4 | Se qualquer um estiver invertido | usar **INVERTER X / INVERTER Y** na calibragem e repetir 2.1.2/2.1.3 | ☐ |
| 2.1.5 | Se X/Y parecerem trocados (inclinar p/ direita muda Y) | usar **TROCAR EIXOS** + INVERTER conforme 2.1.2/2.1.3 | ☐ |
| 2.1.6 | Ajuste funcionou? | anotar: precisou de quais botões? (→ ajustar padrão em `SensorCalibration.cs`) | ☐ |

### 2.2 Jogo no aparelho

| # | Teste | Esperado | ✓ |
|---|---|---|---|
| 2.2.1 | COMEÇAR com o aparelho parado | neutro capturado; sem drift perceptível no início | ☐ |
| 2.2.2 | Tap na tela | bater asas; toque em botão (menu/game over) **não** gera flap | ☐ |
| 2.2.3 | Segurar ~0,5 s | plana | ☐ |
| 2.2.4 | Tap + segurar | primeiro flap, depois plana | ☐ |
| 2.2.5 | Paisagem | jogo nasce em paisagem; não gira para retrato | ☐ |
| 2.2.6 | Colisões em alta velocidade (>20 m/s) | fios/outdoors sempre detectados (sem atravessar) | ☐ |
| 2.2.7 | Correntes | azul/vermelha com efeito e listras animadas; sair/restaurar normal | ☐ |
| 2.2.8 | Performance | ~60 fps estáveis (menos em builds de teste longos); sem travamentos | ☐ |
| 2.2.9 | Recorde | sobrevive a troca de partida e reinício do app | ☐ |
| 2.2.10 | Game over → menu → jogar | fluxo completo sem botões travados | ☐ |
| 2.2.11 | Deixar o app em background e voltar | estado preservado ou menu (aceitável); sem crash | ☐ |
| 2.2.12 | Aparelho sem giroscópio (antigo, opcional) | acelerômetro puro ainda calibra | ☐ |

## Fase 3 — Build/entrega

| # | Teste | Esperado | ✓ |
|---|---|---|---|
| 3.1 | Build Android (IL2CPP/ARM64) | compila sem erros | ☐ |
| 3.2 | APK instala e abre | splash Unity → MainMenu | ☐ |
| 3.3 | Versão/package | `com.pomborunner.pigeonrooftoprush`, paisagem | ☐ |
| 3.4 | `python3 tools/validate.py` | exit 0 | ☐ |

## Registro de resultados

| Data | Aparelho/Android | Build | Tester | Issues abertos |
|---|---|---|---|---|
| | | | | |

**Critério de aceite do MVP:** Fase 1 100% + Fase 2 (2.1 resolvido com eixos corretos,
demais itens sem bloqueadores) + Fase 3 concluída.
