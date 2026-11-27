from pathlib import Path
from docx import Document
from docx.shared import Inches as DIn, Pt as DPt, RGBColor as DRGB
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from pptx import Presentation
from pptx.util import Inches, Pt
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
import fitz
import re
D=Path(__file__).resolve().parent.parent / 'docs'
svg='''<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="900" viewBox="0 0 1600 900">
<defs><marker id="arrow" markerWidth="11" markerHeight="11" refX="9" refY="5.5" orient="auto"><path d="M1 1 L10 5.5 L1 10" fill="none" stroke="#79a6bd" stroke-width="2"/></marker></defs>
<rect width="1600" height="900" rx="30" fill="#0c1a28"/><text x="72" y="78" fill="#72daca" font-family="DejaVu Sans,sans-serif" font-size="21" letter-spacing="4">POMBO RUNNER  /  PROJETO 2</text>
<text x="72" y="138" fill="#ffffff" font-family="DejaVu Sans,sans-serif" font-size="41" font-weight="700">Arquitetura do jogo</text><text x="72" y="178" fill="#a8b9c7" font-family="DejaVu Sans,sans-serif" font-size="22">Fluxo dos sistemas e responsabilidade dos componentes</text>
<g fill="none" stroke="#79a6bd" stroke-width="3" marker-end="url(#arrow)">
<path d="M800 299 L800 345 L275 345 L275 375"/><path d="M800 299 L800 375"/><path d="M800 299 L800 345 L1325 345 L1325 375"/>
<path d="M275 493 L275 570"/><path d="M800 493 L800 570"/><path d="M1325 493 L1325 570"/>
</g>
<g font-family="DejaVu Sans,sans-serif"><rect x="555" y="210" width="490" height="90" rx="16" fill="#173a49" stroke="#56cdb6" stroke-width="2"/><text x="800" y="249" text-anchor="middle" fill="#ffffff" font-size="29" font-weight="bold">GameManager</text><text x="800" y="279" text-anchor="middle" fill="#a5d6d0" font-size="19">estados · atualização · fluxo da partida</text>
<g fill="#162d40" stroke="#365872" stroke-width="2"><rect x="75" y="375" width="400" height="118" rx="14"/><rect x="600" y="375" width="400" height="118" rx="14"/><rect x="1125" y="375" width="400" height="118" rx="14"/><rect x="75" y="570" width="400" height="124" rx="14"/><rect x="600" y="570" width="400" height="124" rx="14"/><rect x="1125" y="570" width="400" height="124" rx="14"/></g>
<g text-anchor="middle" fill="#f4f7fa"><text x="275" y="420" font-size="26" font-weight="bold">IFlightInput</text><text x="275" y="455" font-size="19" fill="#b5c6d2">MobileTiltInput / EditorInput</text><text x="800" y="420" font-size="26" font-weight="bold">SpawnManager</text><text x="800" y="455" font-size="19" fill="#b5c6d2">obstáculos · ventos · cenário</text><text x="1325" y="420" font-size="26" font-weight="bold">Jogador</text><text x="1325" y="455" font-size="19" fill="#b5c6d2">voo · energia · colisão</text>
<text x="275" y="615" font-size="24" font-weight="bold">Sensor / teclado</text><text x="275" y="649" font-size="18" fill="#b5c6d2">calibragem · toque · inclinação</text><text x="800" y="615" font-size="24" font-weight="bold">ObjectPool + Factory</text><text x="800" y="649" font-size="18" fill="#b5c6d2">reutilização de objetos gerados</text><text x="1325" y="615" font-size="24" font-weight="bold">EventBus + HUD</text><text x="1325" y="649" font-size="18" fill="#b5c6d2">eventos de score · energia · crash</text></g>
<text x="800" y="782" text-anchor="middle" font-size="21" fill="#8ba7b6">DifficultyManager controla a progressão   •   ScoreManager calcula e salva o recorde</text><text x="800" y="820" text-anchor="middle" font-size="18" fill="#708b9c">As setas indicam relação funcional simplificada, não propriedade exclusiva.</text></g></svg>'''
(D/'diagrama-arquitetura.svg').write_text(svg,encoding='utf8')
doc=fitz.open(stream=svg.encode(),filetype='svg'); pix=doc[0].get_pixmap(matrix=fitz.Matrix(1,1),alpha=False); pix.save(str(D/'diagrama-arquitetura.png'))

def base_doc(title,subtitle):
 doc=Document(); sec=doc.sections[0]; sec.top_margin=DIn(.67); sec.bottom_margin=DIn(.67); sec.left_margin=DIn(.8); sec.right_margin=DIn(.8)
 styles=doc.styles
 styles['Normal'].font.name='Calibri'; styles['Normal'].font.size=DPt(10); styles['Normal'].paragraph_format.space_after=DPt(5)
 for h in ['Title','Heading 1','Heading 2']:
  styles[h].font.name='Calibri'; styles[h].font.color.rgb=DRGB.from_string('153A4A')
 styles['Title'].font.size=DPt(26); styles['Heading 1'].font.size=DPt(15); styles['Heading 2'].font.size=DPt(12)
 doc.add_heading(title,0); p=doc.add_paragraph(subtitle); p.runs[0].italic=True; p.runs[0].font.color.rgb=DRGB.from_string('597481')
 return doc

def table(doc,headers,rows,width=None):
 t=doc.add_table(rows=1, cols=len(headers)); t.style='Light Shading Accent 1'; t.alignment=WD_TABLE_ALIGNMENT.CENTER
 for i,h in enumerate(headers): t.rows[0].cells[i].text=h
 for row in rows:
  c=t.add_row().cells
  for i,v in enumerate(row): c[i].text=v
 for row in t.rows:
  row._tr.get_or_add_trPr().append(OxmlElement('w:cantSplit'))
  for c in row.cells:
   for p in c.paragraphs:
    p.paragraph_format.space_after=DPt(2)
    for r in p.runs:r.font.size=DPt(9)
 return t

post=base_doc('Post-mortem','Pombo Runner  /  Projeto 2  •  nome e data: preencher')
post.styles['Normal'].font.size=DPt(10)
post.styles['Normal'].paragraph_format.space_after=DPt(4)
paragraph=[]
for line in (D/'post-mortem.md').read_text().splitlines()[1:]:
 line=line.strip()
 if not line:
  if paragraph:
   post.add_paragraph(' '.join(paragraph).replace('**',''))
   paragraph=[]
 elif line.startswith('## '):
  if paragraph:
   post.add_paragraph(' '.join(paragraph).replace('**',''))
   paragraph=[]
  if line == '## Problemas e ajustes': post.add_page_break()
  post.add_heading(line[3:],1)
 else:
  paragraph.append(line)
if paragraph: post.add_paragraph(' '.join(paragraph).replace('**',''))
post.save(D/'post-mortem.docx')

r=base_doc('Relatório de testes','Pombo Runner  /  Projeto 2')
r.add_paragraph('Responsável: ____________________________    Data: ____________________________')
r.add_paragraph('Aparelho / Android: _______________________    Versão do jogo: __________________')
r.add_heading('Objetivo',1)
r.add_paragraph('Testei o Pombo Runner para verificar se os controles, as mecânicas e o fluxo da partida funcionam corretamente.')
r.add_heading('O que testei',1)
table(r,['Área','Verificação'],[
 ('Acesso e interface','Abertura do jogo, menu, calibragem e navegação entre telas.'),
 ('Controles de voo','Inclinação do aparelho, ajustes dos eixos, botão ZERAR X/Y e resposta do pombo.'),
 ('Toque e física','Bater asas, planar, limites de voo e encerramento ao tocar o chão.'),
 ('Energia e correntes','Consumo de energia e efeitos das correntes azul e vermelha.'),
 ('Partida','Obstáculos, colisões, pontuação, progressão, fim de jogo e reinício.')])
r.add_heading('Resultado',1)
r.add_paragraph('Concluí os testes e, na versão testada, o jogo funcionou corretamente.')
r.add_heading('Conclusão',1)
r.add_paragraph('Com base nos testes que realizei, considero que o jogo está funcionando conforme o esperado. Se eu fizer novas alterações, repetirei os testes nas partes afetadas.')
r.save(D/'relatorio-de-testes.docx')

md=(D/'gdd.md').read_text(); g=base_doc('GDD final','Pombo Runner  /  Projeto 2  •  Unity / Android')
g.styles['Normal'].font.size=DPt(9.5); g.styles['Normal'].paragraph_format.space_after=DPt(3)
lines=md.splitlines(); i=1
while i<len(lines):
 line=lines[i].strip()
 if not line or line=='---': i+=1; continue
 if line.startswith('|') and i+1<len(lines) and lines[i+1].strip().startswith('|---'):
  rows=[]
  while i<len(lines) and lines[i].strip().startswith('|'):
   cells=[re.sub(r'[`*]','',v.strip()) for v in lines[i].strip().strip('|').split('|')]; rows.append(cells); i+=1
  if len(rows)>1:
   width=len(rows[0]); body=[(row+['']*width)[:width] for row in rows[2:]]; table(g,rows[0],body)
  continue
 if line.startswith('```'):
  i+=1
  while i<len(lines) and not lines[i].strip().startswith('```'):
   if lines[i].strip(): g.add_paragraph(lines[i],style='Normal')
   i+=1
  i+=1; continue
 line=re.sub(r'\[([^]]+)\]\([^)]+\)',r'\1',line); line=line.replace('**','').replace('`','')
 if line.startswith('### '): g.add_heading(line[4:],2)
 elif line.startswith('## '): g.add_heading(line[3:],1)
 elif line.startswith('- '): g.add_paragraph(line[2:],style='List Bullet')
 elif line.startswith('> '): g.add_paragraph(line[2:])
 else: g.add_paragraph(line)
 i+=1
g.save(D/'gdd-final.docx')

prs=Presentation();prs.slide_width=Inches(13.333);prs.slide_height=Inches(7.5)
navy='0D1B2B'; white='F5F8F9'; muted='AFC2CC'; teal='70DEC4'; amber='FFC978'; red='F4867C'; dark='162C3C'; light='CEDCE2'
def col(hex):return RGBColor.from_string(hex)
def box(s,x,y,w,h,color,round=True):
 sh=s.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE if round else MSO_SHAPE.RECTANGLE, Inches(x), Inches(y), Inches(w), Inches(h));sh.fill.solid();sh.fill.fore_color.rgb=col(color);sh.line.fill.background();return sh
def text(s,x,y,w,h,txt,size=18,color=white,bold=False,align=None):
 sh=s.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h));tf=sh.text_frame;tf.clear();tf.word_wrap=True;tf.margin_left=0;tf.margin_top=0;tf.margin_right=0;tf.margin_bottom=0
 p=tf.paragraphs[0];p.text=txt;p.font.name='Aptos';p.font.size=Pt(size);p.font.bold=bold;p.font.color.rgb=col(color)
 if align is not None:p.alignment=align
 return sh
def slide(k,title,tag,time):
 s=prs.slides.add_slide(prs.slide_layouts[6]);s.background.fill.solid();s.background.fill.fore_color.rgb=col(navy)
 text(s,.62,.34,10,.25,'POMBO RUNNER  /  PROJETO 2',10,teal,True)
 text(s,.62,.85,12,.52,title,29,white,True)
 text(s,.64,1.48,12,.25,tag,12,muted)
 box(s,.63,7.10,12.05,.015,'38505B',False);text(s,.64,7.18,7,.18,'POMBO RUNNER  •  ENTREGA FINAL',9,muted);text(s,11.38,7.18,1.3,.18,f'{k:02d}  /  {time}',9,muted,align=PP_ALIGN.RIGHT)
 return s
def notes(s,content):
 s.notes_slide.notes_text_frame.text=content
s=slide(1,'Pombo Runner','O celular deixa de ser apenas uma tela e passa a controlar o voo.','0:40')
text(s,.75,2.15,7,1.20,'Um pombo.\nUma cidade. Seu voo.',38,white,True)
text(s,.75,3.85,6.7,.7,'Endless flyer arcade 3D para Android, orientado pela inclinação do aparelho.',20,light)
box(s,.75,5.35,6.0,.75,dark);text(s,1.0,5.55,5.5,.3,'AUTORIA  •  DISCIPLINA  •  DATA: PREENCHER',11,amber,True)
box(s,8.4,2.1,3.9,3.8,'17384A');text(s,8.8,2.55,3.1,.35,'INCLINAR',14,teal,True);text(s,8.8,3.1,3.1,1.2,'↖    ↑    ↗\n←    •    →',27,white,True,PP_ALIGN.CENTER);text(s,8.8,5.05,3.1,.4,'VOAR  /  DESVIAR',14,light,True)
notes(s,'0:40 — Apresento o Pombo Runner, meu projeto de jogo de voo para Android. Minha proposta é usar a inclinação do smartphone como parte da mecânica, não apenas como uma tela. Apresento meu nome e a disciplina.')
s=slide(2,'O ciclo de jogo','Sobreviver é equilibrar direção, energia e leitura do cenário.','0:50')
for j,(a,b) in enumerate([('01','VOAR'),('02','DESVIAR'),('03','GERENCIAR'),('04','PONTUAR')]):
 x=.75+j*3.05;box(s,x,2.45,2.75,2.15,dark);text(s,x+.24,2.7,2.2,.35,a,17,teal,True);text(s,x+.24,3.32,2.3,.42,b,20,white,True);text(s,x+.24,3.93,2.3,.4,['Inclinação e toque','Cinco obstáculos','Energia e ventos','Distância + bônus'][j],12,muted)
text(s,.82,5.47,11.5,.5,'A velocidade aumenta; uma colisão ou queda encerra a partida.',21,light)
notes(s,'0:50 — Explico o ciclo: o jogador voa, desvia dos obstáculos, administra a energia com as correntes e acumula pontos. Com o tempo, a velocidade aumenta. Uma colisão ou queda encerra a partida.')
s=slide(3,'O smartphone é o controle','Sensor real na mecânica, não apenas botões na tela.','1:05')
for x,title,body,c in [(.75,'INCLINAÇÃO','Inclinação move continuamente; ao voltar ao neutro, a pomba para.',teal),(4.85,'TOQUE CURTO','Um toque curto bate asas e consome energia.',amber),(8.95,'SEGURAR','Após 0,22 s, o pombo plana com consumo menor.',red)]:
 box(s,x,2.24,3.7,2.75,dark);text(s,x+.28,2.58,3.1,.35,title,16,c,True);text(s,x+.28,3.2,3.1,1.35,body,18,white)
text(s,.77,5.63,11.7,.6,'Calibragem  •  ZERAR X/Y  •  INVERTER / TROCAR EIXOS',19,light)
notes(s,'1:05 — Uso o sensor de gravidade quando há leitura válida e o acelerômetro como alternativa. Quando disponível, o giroscópio ajuda durante a calibragem. A inclinação lateral move continuamente a pomba; ao voltar ao neutro ela para. Na vertical, mantenho gravidade, flap e planar. Também incluí ZERAR X/Y e ajustes de eixos.')
s=slide(4,'Energia: escolhas em tempo real','A barra começa em 100 e não gera morte imediata ao zerar.','1:05')
for x,title,big,body,c in [(.75,'VOO NORMAL','−1,5/s','Planar: −0,7/s',light),(4.85,'CORRENTE AZUL','100%','Recuperação imediata',teal),(8.95,'CORRENTE VERMELHA','−25','Pontos por entrada',red)]:
 box(s,x,2.25,3.7,2.85,dark);text(s,x+.27,2.56,3.1,.3,title,13,c,True);text(s,x+.27,3.03,3.1,.75,big,34,c,True);text(s,x+.27,4.22,3.1,.4,body,16,white)
text(s,.77,5.68,11.7,.5,'Flap: −3 pontos  •  Energia zerada: flap bloqueado',20,light)
notes(s,'1:05 — Explico os consumos e o custo do flap. Na corrente azul, a energia vai a cem por cento. Na vermelha, ela diminui vinte e cinco pontos por entrada. Com energia zerada, o flap é bloqueado, mas a partida só termina com colisão ou queda.')
s=slide(5,'Um cenário que acelera','Cinco obstáculos e duas correntes com geração procedural.','0:55')
text(s,.78,2.28,6,.3,'OBSTÁCULOS',13,teal,True)
for idx,label in enumerate(['Prédio','Antena','Fio','Outdoor','Árvore']):
 x=.8+(idx%2)*2.9;y=2.82+(idx//2)*.72;box(s,x,y,2.55,.52,dark);text(s,x+.2,y+.12,2.15,.25,label,15,white,True)
box(s,7.65,2.2,4.75,3.45,'17384A');text(s,8.0,2.56,4,.3,'VELOCIDADE',13,teal,True);text(s,8.0,3.08,4,.72,'9 → 24 m/s',29,white,True);text(s,8.0,4.05,4,.5,'máximo em 75 segundos',16,light);text(s,8.0,4.7,4,.55,'Duplas com vão livre; fio sozinho.',14,muted)
notes(s,'0:55 — Apresento os cinco obstáculos. Uso pools para reutilizar objetos, separo os pares e deixo o fio sozinho. A velocidade aumenta de nove para vinte e quatro metros por segundo em setenta e cinco segundos. A primeira corrente é azul e, após uma vermelha, a próxima é azul.')
s=slide(6,'Arquitetura','Sistemas independentes, comunicação por eventos e objetos reutilizados.','1:10')
s.shapes.add_picture(str(D/'diagrama-arquitetura.png'), Inches(.95), Inches(1.95), width=Inches(8.7), height=Inches(4.89))
text(s,10.05,2.35,2.45,.4,'PADRÕES',13,teal,True);text(s,10.05,2.85,2.45,2.9,'Strategy\nState Machine\nEventBus\nObject Pool\nComposition Root',17,white)
notes(s,'1:10 — Mostro como organizei o GameManager e os estados da partida. A interface IFlightInput separa entrada móvel e teclado. O SpawnManager usa factories e pools, enquanto o EventBus comunica eventos para a interface. Explico que o diagrama resume essas relações.')
s=slide(7,'Gameplay real','Vou apresentar um vídeo real de 1 a 2 minutos.','1:30')
box(s,1.0,2.05,11.3,3.82,'17384A');text(s,2,2.65,9.3,.65,'VÍDEO PENDENTE',31,amber,True,PP_ALIGN.CENTER);text(s,2,3.64,9.3,.85,'Vou inserir uma gravação mostrando o aparelho e a resposta do pombo à inclinação.',19,white,False,PP_ALIGN.CENTER);text(s,2,5.04,9.3,.4,'Menu → calibragem → voo → correntes → fim de jogo',14,muted,False,PP_ALIGN.CENTER)
notes(s,'1:30 — Apresento meu vídeo real de gameplay. Quando possível, mostro também o aparelho para evidenciar a inclinação. Antes de apresentar, insiro a gravação e confiro sua duração e reprodução.')
s=slide(8,'Testes e entrega','Testei o jogo e organizei os materiais da entrega.','1:15')
box(s,.78,2.18,5.75,3.62,dark);text(s,1.1,2.52,4.9,.35,'MEUS TESTES',14,teal,True);text(s,1.1,3.12,4.9,1.75,'• Testei os controles e a partida\n• O jogo funcionou corretamente na versão testada\n• Registrei o resultado no relatório',18,white)
box(s,6.8,2.18,5.75,3.62,dark);text(s,7.12,2.52,4.9,.35,'PARA A ENTREGA',14,amber,True);text(s,7.12,3.12,4.9,1.85,'• Conferir o arquivo da build\n• Incluir vídeo de gameplay\n• Completar identificação\n• Ensaiar a apresentação',18,white)
notes(s,'1:15 — Relato que testei o jogo e que ele funcionou corretamente na versão testada. Também explico que as verificações estáticas e os testes simulados complementam, mas não substituem, a execução real. Confiro a build e os arquivos da entrega.')
s=slide(9,'Lições e próximos passos','O que aprendi e o que pretendo fazer a seguir.','1:30')
for idx,(heading,body) in enumerate([('01  CONTROLES','Ajustei o voo após experimentar a movimentação.'),('02  VALIDAR','Repetirei os testes se alterar a versão.'),('03  APRESENTAR','Vou conferir a build, incluir o vídeo e ensaiar.')]):
 y=2.18+idx*1.22;box(s,.8,y,11.65,.95,dark);text(s,1.08,y+.16,3.2,.3,heading,15,teal,True);text(s,4.0,y+.16,7.9,.55,body,17,white)
text(s,.8,6.25,11.5,.3,'Obrigado.  /  Perguntas',18,light,True)
notes(s,'1:30 — Compartilho o que aprendi sobre controles por sensores, calibragem e sincronização do toque com a física. Se eu modificar a versão, repetirei os testes afetados. Apresento minhas possibilidades futuras e encerro com perguntas.')
prs.save(D/'apresentacao-final.pptx')
print('Criados SVG, PNG, DOCX (GDD, post-mortem, relatório) e PPTX:',len(prs.slides),'slides.')
