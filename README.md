# Pombo Runner

Desenvolvi o Pombo Runner como um endless flyer arcade 3D para Android. Minha ideia é transformar o celular em um controle de voo: o jogador inclina o aparelho para conduzir o pombo, toca para bater asas e segura para planar. Durante a partida, ele desvia de obstáculos, administra energia, aproveita correntes de ar e acumula pontos.

O projeto foi configurado para **Unity 6000.3.18f1**, URP e Android em paisagem. O [enunciado](docs/enunciado-projeto.pdf) apresenta o Projeto 2, que serviu de referência para meu trabalho.

## Como abrir

1. Instale o Unity Hub e o Unity **6000.3.18f1**, com Android Build Support, SDK, NDK e OpenJDK.
2. No Unity Hub, escolha **Add → Add project from disk** e selecione a pasta do projeto.
3. Aguarde a importação dos pacotes e abra `Assets/Scenes/MainMenu.unity`.
4. Pressione **Play**. No Editor, use teclado para controlar o voo.

## Controles

| Ação | Celular | Editor |
|---|---|---|
| Mover lateralmente | Inclinar para a esquerda ou direita; no neutro, o movimento lateral para | `A`/`D` ou setas |
| Subir ou mergulhar | Inclinar no eixo vertical | `W`/`S` ou setas |
| Bater asas | Toque curto | `Espaço` |
| Planar | Segurar por mais de 0,22 s | `Shift` |

Na calibragem, seguro o aparelho na posição desejada e uso **ZERAR X/Y** para definir o neutro. Posso ajustar a direção com **INVERTER X**, **INVERTER Y** e **TROCAR EIXOS**, sem perder essas escolhas ao zerar. Depois, seleciono **COMEÇAR**. Os ajustes de eixos valem para a sessão.

Na implementação, uso `GravitySensor` quando há leitura válida e `Accelerometer` como alternativa. Quando disponível, o giroscópio ajuda a detectar rotação durante o refinamento da calibragem. Os padrões dos eixos, a sensibilidade `1.2` e a zona morta `0.14` ficam em `Assets/Scripts/Input/SensorCalibration.cs`; a velocidade lateral padrão de `10 m/s` fica em `PigeonFlightController.cs`.

## Como gerar a build Android

1. Abra **File → Build Profiles** (ou **Build Settings**) e selecione Android em **Switch Platform**.
2. Confira as cenas `MainMenu` e `Game`, nessa ordem, na lista do perfil.
3. Em Player Settings, confira **Input System Package (New)** em Active Input Handling, orientação paisagem, backend **IL2CPP** e arquitetura **ARM64**. Não use `Both` para Android.
4. Confira o identificador `com.pomborunner.pigeonrooftoprush` e escolha **Build**. Para APK, mantenha Build App Bundle desativado; para AAB, ative-o. O AAB não é instalado diretamente como um APK.
5. Para executar no aparelho, instale o APK ou use **Build And Run** com o Android autorizado. Associe a versão entregue aos testes e ao vídeo.

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

## Verificações adicionais

Além dos testes que realizei no jogo, posso repetir as verificações estáticas e a simulação de lógica abaixo. Elas não substituem a execução real no Unity ou no aparelho.

```bash
python3 -m pip install --user tree-sitter tree-sitter-c-sharp pyyaml
python3 tools/validate.py
python3 tools/deep_check.py
mcs -langversion:latest -out:/tmp/pombo-testes.exe tools/testes/LogicaSimulada.cs Assets/Scripts/Player/EnergySystem.cs Assets/Scripts/Input/IFlightInput.cs Assets/Scripts/Input/SensorCalibration.cs Assets/Scripts/Input/MobileTiltInput.cs Assets/Scripts/Input/EditorInput.cs Assets/Scripts/Player/PigeonFlightController.cs
mono /tmp/pombo-testes.exe
```

Para gerar novamente os documentos editáveis e o diagrama, instalo `python-docx`, `python-pptx` e `PyMuPDF` e executo `python3 tools/gerar_entrega.py`. **O gerador sobrescreve** os documentos e diagramas gerados; faço uma cópia antes de preencher informações ou editar a apresentação. Posso exportar os PDFs separadamente no LibreOffice.

## Documentação

Reuni aqui os principais arquivos da entrega:

- [Diagrama da arquitetura — diagrama-arquitetura.png](docs/diagrama-arquitetura.png)
- [GDD final — gdd-final.pdf](docs/gdd-final.pdf)
- [Post-mortem — post-mortem.pdf](docs/post-mortem.pdf)
- [Relatório de testes — relatorio-de-testes.pdf](docs/relatorio-de-testes.pdf)

### Diagrama da arquitetura

![Diagrama da arquitetura do Pombo Runner](docs/diagrama-arquitetura.png)

### Outros formatos e materiais

| Material | Arquivo |
|---|---|
| GDD | [Markdown](docs/gdd.md) · [Word](docs/gdd-final.docx) · [PDF](docs/gdd-final.pdf) |
| Arquitetura e diagrama | [Descrição](docs/arquitetura.md) · [SVG](docs/diagrama-arquitetura.svg) · [PNG](docs/diagrama-arquitetura.png) |
| Testes | [Relatório editável](docs/relatorio-de-testes.docx) · [PDF](docs/relatorio-de-testes.pdf) · [Plano](docs/plano-de-testes.md) |
| Post-mortem | [Markdown](docs/post-mortem.md) · [Word](docs/post-mortem.docx) · [PDF](docs/post-mortem.pdf) |
| Apresentação | [Slides](docs/apresentacao-final.pptx) · [PDF](docs/apresentacao-final.pdf) · [Roteiro](docs/apresentacao.md) |
| Entrega | [Checklist](docs/entrega-final.md) · [Roteiro do vídeo](docs/roteiro-video.md) · [Verificações](docs/verificacoes.md) |
| Desenvolvimento | [Plano de implementação](docs/plano-implementacao.md) |
