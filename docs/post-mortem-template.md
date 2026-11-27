# Post-Mortem — Pigeon Rooftop Rush (template, 1–2 páginas)

> Preencha ao final do projeto. Seções sugeridas — adapte ao que aconteceu de verdade.

## 1. Resumo

- **O que era para ser:** endless flyer 3D onde o aparelho é o controle de voo
  (MVP: voo, tilt+calibragem, 5 obstáculos, 2 ventos, energia, score, pooling, APK).
- **O que foi entregue:** _ex.: MVP completo em X dias; Y itens do GDD adiados._
- **Veredito em uma frase:** _ex.: “O núcleo funcionou; o maior custo foi validar
  sensores sem poder testar no aparelho todo dia.”_

## 2. Números

| Métrica | Valor |
|---|---|
| Tempo total estimado/real | |
| Scripts criados | 25 |
| Cenas | 2 (montadas em runtime) |
| Pools com prewarm | 9 |
| Bugs corrigidos (compile/console) | |
| Itens do MVP concluídos | 9/9 |
| Itens pós-MVP adiados | |

## 3. O que deu certo

- _ex.: montagem de cena em runtime eliminou erros de GUID/prefab;_
- _ex.: validação estática (tree-sitter + checagem de referências) pegou erros antes de
  abrir o Unity;_
- _ex.: padrão IFlightInput facilitou testar com teclado no editor;_
- _ex.: calibragem com correção de eixos em dispositivo evitou hotfix recompilar._

## 4. O que não deu (e por quê)

| Problema | Causa raiz | Solução adotada |
|---|---|---|
| | | |
| | | |

## 5. Lições

1. _ex.: “Mapear eixos de acelerômetro analiticamente sem aparelho é arriscado —
   sempre expor leitura ao vivo + correção em runtime.”_
2. _ex.: “Pool + factory em runtime valeu mais que prefabs authorados à mão sem editor.”_
3. _ex.: _

## 6. Impacto no design (o que mudou do GDD)

- _ex.: energia sem morte instantânea (só cair no chão) aumentou a tensão sem
  frustrar;_
- _ex.:_

## 7. Próximos passos (ordem de prioridade)

1. _ex.: haptics + áudio;_
2. _ex.: swipe mergulho/freio;_
3. _ex.: power-ups e zonas;_
4. _ex.: otimização de draw calls / culling por distância._

## 8. Agradecimentos / referências

- _ex.: Unity Docs (Input System, URP), referências de endless runner com pooling._
