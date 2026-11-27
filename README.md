# Pombo Runner

Pombo Runner como um endless flyer arcade 3D para Android. Minha ideia é transformar o celular em um controle de voo: o jogador inclina o aparelho para conduzir o pombo, toca para bater asas e segura para planar. Durante a partida, ele desvia de obstáculos, administra energia, aproveita correntes de ar e acumula pontos.

[ACESSE AQUI PARA BAIXAR E JOGAR!](https://github.com/AndoreBG/pombo-runner/releases/tag/1.0.0)

## Integrantes do grupo
- 010 - André Bolandim Gorzoni
- 049 - Leonardo Trevisan
- 035 - Miguel Arcanjo Pellizzari Baldo
- 005 - Vitor Mathias Rodrigues dos Santos

## Controles

| Ação | Celular | Editor |
|---|---|---|
| Mover lateralmente | Inclinar para a esquerda ou direita; no neutro, o movimento lateral para | `A`/`D` ou setas |
| Subir ou mergulhar | Inclinar no eixo vertical | `W`/`S` ou setas |
| Bater asas | Toque curto | `Espaço` |
| Planar | Segurar por mais de 0,22 s | `Shift` |

Na calibragem, seguro o aparelho na posição desejada e uso **ZERAR X/Y** para definir o neutro. Posso ajustar a direção com **INVERTER X**, **INVERTER Y** e **TROCAR EIXOS**, sem perder essas escolhas ao zerar. Depois, seleciono **COMEÇAR**. Os ajustes de eixos valem para a sessão.

Na implementação, uso `GravitySensor` quando há leitura válida e `Accelerometer` como alternativa. Quando disponível, o giroscópio ajuda a detectar rotação durante o refinamento da calibragem. Os padrões dos eixos, a sensibilidade `1.2` e a zona morta `0.14` ficam em `Assets/Scripts/Input/SensorCalibration.cs`; a velocidade lateral padrão de `10 m/s` fica em `PigeonFlightController.cs`.

## Documentação

Reuni aqui os principais arquivos da entrega:

- [Diagrama da arquitetura — diagrama-arquitetura.png](docs/diagrama-arquitetura.png)
- [GDD final — gdd-final.pdf](docs/gdd-final.pdf)
- [Post-mortem — post-mortem.pdf](docs/post-mortem.pdf)
- [Relatório de testes — relatorio-de-testes.pdf](docs/relatorio-de-testes.pdf)

### Diagrama da arquitetura

![Diagrama da arquitetura do Pombo Runner](docs/diagrama-arquitetura.png)

## O que implementei

- Voo com sensores e calibragem, além de controles de teclado para o Editor.
- Cinco obstáculos: prédio, antena, fio elétrico, outdoor e árvore.
- Corrente azul, que restaura a energia a 100%, e corrente vermelha, que desconta 25 pontos por entrada.
- Energia, pontuação, recorde, dificuldade progressiva e reutilização de objetos com pooling.
- Menu, tela de calibragem, HUD e tela de fim de jogo.

Deixei áudio, haptics, swipes, power-ups, corrente verde, novas áreas e obstáculos adicionais para depois do MVP. Detalhei o escopo em [GDD](docs/gdd.md).

## Estrutura

- `Assets/Scenes`: cenas `MainMenu` e `Game`.
- `Assets/Scripts`: sistemas de voo, entrada, jogo, interface, mundo e montagem.
- `Assets/Shaders` e `Assets/Settings`: shaders e configurações visuais.
- `Packages` e `ProjectSettings`: pacotes e configurações do projeto Unity.
- `docs`: GDD, arquitetura, testes, post-mortem, apresentação e roteiro de vídeo.
- `tools`: validadores, testes simulados e geração dos documentos.
