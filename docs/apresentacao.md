# Roteiro da Apresentação Final — Pigeon Rooftop Rush (~10 minutos)

> Guia de fala slide a slide. Tempo médio: 45–50 s por bloco. Trechos em *itálico* são
> sugestão literal de fala; o resto é o que deve aparecer no slide.

---

## Bloco 1 — Abertura (0:00–1:00)

**S1. Capa (0:00)** — título, nome, disciplina, data.
*“Pigeon Rooftop Rush: um endless flyer onde o seu smartphone vira o controle de voo.”*

**S2. Pitch (0:20)**
- Um pombo voa pela cidade desviando de fios, prédios, antenas, outdoors e árvores.
- Inclinar o aparelho é virar/subir/mergulhar; tocar é bater asas; segurar é planar.
- Correntes de ar dão energia ou causam turbulência — sobreviva o máximo possível.

**S3. O diferencial mobile (0:40)**
- *“A tese do projeto é simples: o celular não é só uma tela, é o controle. A mesma
  inclinação que você usa para jogar é a inclinação que o pombo interpreta como
  comando de voo.”*
- Mapa: inclinar ↑↓←→ + toque + haptics (roadmap).

## Bloco 2 — GDD resumido (1:00–2:30)

**S4. Core loop** — diagrama: Voar → desviar → coletar → energia → score → velocidade.

**S5. Controles** — tabela celular × editor (WASD/Space/Shift para testar no PC).

**S6. Energia** — tabela: voo −3/s, flap −6, planar −1,4/s, azul +9/s, vermelha −8/s;
*“Sem energia você não morre na hora: você perde o flap e precisa alcançar uma
corrente azul — isso cria a tensão central do jogo.”*

**S7. Obstáculos e ventos** — as 5 figuras (prédio, antena, fio, outdoor, árvore) +
2 ventos com cores; esquivas (por cima/lado/baixo).

**S8. Progressão** — curvas: 9→24 m/s, spawn 1,55→0,62 s, fileiras duplas 0→45%;
score = metros + 25/obstáculo; recorde salvo.

## Bloco 3 — Arquitetura (2:30–5:00)

**S9. Visão geral** — diagrama Mermaid do sistema (GameManager → Flight/Input/Spawn/
Difficulty/Score/Energy; EventBus p/ UI).

**S10. Padrões do enunciado** — tabela:
- Singleton → GameManager
- Strategy → IFlightInput (MobileTiltInput × EditorInput)
- State Machine → Boot/Calibrating/Playing/GameOver
- Observer → EventBus tipado
- Pooling → ObjectPool ×9 com prewarm

**S11. Cenas em runtime (decisão técnica 1)** — *“As cenas guardam só câmera, luz e
bootstrap; o resto é construído em Awake. Isso eliminou classes inteiras de erro de
referência e deixou o jogo todo versionável em YAML mínimo.”*

**S12. Shaders próprios (decisão técnica 2)** — *“Dois shaders autocontidos — luz e
névoa chegam por uniforms globais — registrados em Always Included Shaders. Zero
dependência de GUIDs de pacote; sombra blob no lugar de shadow maps para performance
mobile.”*

**S13. Sensores e calibragem (decisão técnica 3)**
- `Input.compensatingSensors` (gyro compensa a orientação da tela);
- neutro capturado por partida com refinamento só enquanto o aparelho está parado;
- leitura de eixos ao vivo + botões INVERTER/TROCAR em dispositivo;
- fallback de teclado no editor com a mesma interface.

**S14. Estrutura de código** — árvore Scripts/Core…Boot (25 scripts) + ferramentas
`tools/validate.py` (sintaxe tree-sitter, GUIDs, cenas, build settings).

## Bloco 4 — Gameplay (5:00–7:00)

**S15–S17. Vídeo de gameplay (1–2 min)** — gravir: menu → calibragem → run com flap,
glide, corrente azul, turbulência, fileira dupla, crash, game over com recorde.
*Comentar por cima:* “aqui o tilt está fazendo X… aqui a energia cai com a
vermelha…”.

**S18. Números da run de exemplo** — score, distância, velocidade máxima, tempo de
sobrevivência.

## Bloco 5 — Testes e entrega (7:00–8:30)

**S19. Plano de testes** — resumo das 3 fases (editor / dispositivo / build); destaque
para o item 1: eixos do tilt validados no aparelho com os botões de correção.

**S20. Problemas encontrados e correções** — 2–3 exemplos reais (ex.: pitch do mergulho
invertido; botão de menu transbordando o painel; chamada de UI dentro de callback de
física → crash deferido).

**S21. Entregáveis** — APK, projeto Unity, GDD, arquitetura, README, este roteiro,
plano de testes, post-mortem.

## Bloco 6 — Fechamento (8:30–10:00)

**S22. MVP × pós-MVP** — 9/9 itens do MVP; adiados: power-ups, 5 zonas, haptics,
áudio, drones/janelas/outros pássaros, corrente verde, pausa.

**S23. Lições** — ex.: validar sensores exige dispositivo desde o dia 1; montagem em
runtime valeu mais que prefabs sem editor; sempre expor leitura crua do sensor na UI.

**S24. Próximos passos** — haptics+áudio → gestos de swipe → power-ups/zonas →
otimização.

**S25. Perguntas** — *“Obrigado — fica a disposição para dúvidas.”*

---

### Dicas de apresentação

1. Rode o vídeo **antes** de apresentar (arquivo local, sem depender de rede).
2. Moste o `validate.py` rodando (10 s) — prova de rigor sem editor.
3. Se der tempo, abra o Unity e dê Play ao vivo na fase de perguntas.
4. Cronômetro: bloco 3 é onde mais se gasta tempo — praticar com o relógio.
