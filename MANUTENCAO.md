# Cotador de voos — manutenção da versão 3.9

Esta versão reorganiza o código da 3.2, preservando a interface e o formato da cotação. O projeto usa C# compatível com o compilador do .NET Framework 4.x do Windows e não requer pacotes externos.

## Organização

| Arquivo | Responsabilidade |
|---|---|
| `Program.cs` | Inicialização da interface em thread STA, necessária para o clipboard. |
| `Flight.cs` | Dados de um segmento ou de um sentido já agrupado. |
| `Quote.Airlines.cs` | Normalização contextual de GOL, AZUL e LATAM, restrita ao prefixo da linha. |
| `Quote.Airports.cs` | Nomes dos aeroportos e correções conhecidas do OCR. |
| `Quote.Parsing.cs` | Conversão de cada linha reconhecida em um segmento. |
| `Quote.Itinerary.cs` | Continuidade dos aeroportos, conexões e divisão de ida/volta. |
| `Quote.Formatting.cs` | Mensagem em português e descrição da bagagem. |
| `QuoteValidation.cs` | Validação independente dos campos editados e do valor. |
| `QuoteReadException.cs` | Erros de reconhecimento que permitem tentar outra escala. |
| `OcrClient.cs` | Processo PowerShell assíncrono, timeout e arquivo de resultado. |
| `Ocr.ps1` | Ampliação, OCR do Windows e reconstrução das linhas por coordenadas. |
| `MainForm.cs` | Importação, revisão, geração e cópia da cotação. |
| `MainForm.Layout.cs` | Construção de cada cartão e associação dos eventos. |
| `ModernUI.cs` | Paleta e controles visuais personalizados. |

As partes de `Quote` e `MainForm` usam classes `partial`: são arquivos separados, mas formam a mesma classe na compilação. Os nomes públicos existentes foram preservados.

## Compilar e testar

Abra PowerShell na pasta extraída, feche o Cotador e execute:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Test.ps1
```

`Build.ps1` reúne os arquivos C# da raiz. Os testes ficam em uma subpasta para não introduzir outro ponto de entrada no aplicativo.

Os testes não exigem acesso aos prints originais. Há 11 amostras de texto reconhecido com respostas geradas pela versão 3.2, antes da refatoração. A comparação exata verifica os horários, as conexões, as cidades, a bagagem e as quebras de linha. Outros testes cobrem valores inválidos, datas, horários impossíveis e normalização do OCR. São 44 verificações no total, incluindo normalização das companhias e proteção contra correspondências em cidades ou nomes ambíguos.

Também foram executados testes de importação real dos prints da sessão, conferência dos campos preenchidos, clipboard, checkbox de bagagem e abertura da interface em dois tamanhos. Esses testes dependem dos arquivos locais originais e não fazem parte da suíte portátil.

## Decisões e limites relevantes

### Catálogo de aeroportos — atualização de 09/09/2026

`Quote.Airports.cs` contém 552 códigos: 345 brasileiros e 207 internacionais,
em ordem alfabética de IATA dentro de cada grupo. Os valores continuam sendo
as cidades atendidas, preservando a apresentação `Cidade (IATA)` e os nomes
que já existiam na versão 3.4.

Fonte: [OurAirports](https://ourairports.com/data/), base de domínio público
[airports.csv](https://davidmegginson.github.io/ourairports-data/airports.csv),
consultada em 09/09/2026. Para o Brasil, foram incluídos todos os registros da
base com `iso_country = BR`, IATA de três letras e tipo `small_airport`,
`medium_airport` ou `large_airport`. Isso inclui aeroportos regionais e sem
voos regulares; exclui helipontos, aeroportos marcados como fechados e locais
sem IATA. A cobertura é completa segundo esse recorte da fonte, não uma
garantia de completude do cadastro oficial da IATA ou de operação comercial.

A seleção internacional cobre 207 aeroportos nas Américas, Europa, África,
Oriente Médio, Ásia e Oceania, com nomes usuais de cidades em português.
Não é um ranking por movimento. Códigos metropolitanos como NYC e SAO não
são aeroportos individuais e não foram incluídos. Códigos desconhecidos
continuam aparecendo sem nome para permitir revisão manual.

O catálogo fica compilado no executável: não requer internet nem arquivos de
dados adicionais durante o uso. Ao atualizar, confira códigos e cidades na
fonte, mantenha os nomes existentes quando corretos e execute build e testes.
A suíte atual tem 1.228 verificações, incluindo os 552 códigos isolados e
acompanhados da cidade, destinos regionais, internacionais, código desconhecido
e integração com a mensagem final.

### Correção contextual dos códigos reconhecidos

Além dos reparos históricos, a identificação aceita uma única confusão visual
nos grupos `O/D/Q/0`, `I/L/1`, `B/8`, `S/5`, `Z/2`, `G/6` e `T/7`, desde que
a cidade imediatamente após o código confirme um único aeroporto do catálogo.
Exemplos: `LOB - Londrina → LDB`, `LD8 - Londrina → LDB`,
`D0H - Doha → DOH` e `SY0 - Sydney → SYD`. Aceita hífen, travessão ou apenas
espaço, sem diferenciar caixa, acentos e espaços repetidos no nome da cidade.

Um código exato confirmado pela cidade tem prioridade. Sem cidade, com duas
trocas, com cidade incompatível ou com múltiplos candidatos, o texto é mantido
para revisão; não se presume que um código ausente do catálogo seja inválido.
Exceção adicional conferida com o print `Screenshot 2026-09-09 082922.png`:
em linhas com apenas dois códigos e duração numérica opcional, `LOB` é
interpretado como `LDB`, tanto na origem quanto no destino. Essa regra específica
atende à tabela enviada, sem exigir o nome Londrina. Não é uma regra universal
de equivalência dos códigos; revise rotas em outros contextos. Rótulos com cidade
incompatível continuam sem essa substituição.

As regras históricas de GRU, FLN, Lisboa e Lima continuam aplicadas antes dessa
etapa. Códigos com dígitos são mantidos quando não há confirmação, mas números
isolados de equipamento, como 321, não são interpretados como aeroportos.

Os testes gerais usam texto OCR simulado e não medem a precisão visual do OCR
do Windows. A amostra `tests/fixtures/londrina.ocr.txt` contém o texto real
enviado pelo usuário; seu resultado esperado foi conferido com o print acima:
CNF–VCP–LDB em 02/10/2026, saída 06:15 e chegada 09:55; LDB–VCP–CNF em
04/10/2026, saída 19:10 e chegada 22:55. O ano 2026 é o parâmetro do teste.
O OCR não trouxe a companhia, portanto o resultado esperado a deixa vazia;
o print mostra Azul, que deve ser preenchida na revisão. As 11 cotações
anteriores permanecem idênticas; são agora 12 amostras completas.

- O parser lê segmentos; o agrupador decide a composição de cada sentido. Não suponha que ida e volta tenham o mesmo número de voos.
- A divisão de uma rota fechada usa a maior pausa. Os limites de 18, 6 e 24 horas estão nomeados e comentados, mas continuam sendo heurísticas. Itinerários ambíguos exigem revisão manual.
- Os horários são locais, como no print. Não calcule a duração de um voo internacional subtraindo horários de aeroportos diferentes sem informação de fuso.
- O ano vem do seletor. A sequência de meses pode sugerir uma virada de ano; essa sugestão precisa ser conferida.
- `US` só vira `LIS` quando acompanhado de Lisboa; os reparos de Lima também exigem contexto. As substituições antigas `CRU/GRIJ → GRU` e `FIN → FLN` foram mantidas por compatibilidade, e não são uma solução universal para aeroportos.
- Só `QuoteReadException` dispara a segunda leitura por causa de interpretação incompleta. Falhas técnicas não são mais ocultadas por um `catch` vazio.
- A coluna oculta de conexões mantém os dados sincronizados com os dois editores visíveis. A comparação antes de atribuir o texto evita recursão de eventos.
- Toda alteração nos campos invalida a confirmação e a mensagem anterior. Copiar exige uma nova revisão.
- O OCR e a área de transferência dependem do Windows. O logotipo Azul pode continuar exigindo preenchimento manual.

## Correções encontradas na revisão

- Recursos de imagem e stream do OCR agora são liberados também em falhas; a imagem da interface é descartada ao fechar a janela.
- Horários impossíveis reconhecidos pelo OCR são classificados como erro de leitura, permitindo tentar a outra ampliação.
- Dias reconhecidos com `i` minúsculo recebem a mesma normalização dos dias com `I` maiúsculo.
- Validação e criação da mensagem foram separadas para facilitar novos testes sem abrir a janela.

Ao adicionar um novo layout, inclua o texto reconhecido e um resultado esperado conferido com a imagem em `tests/fixtures`. Evite alterar resultados esperados apenas para acomodar uma falha: compare primeiro com o print.

## Versionamento das entregas

Manter o padrão de entregas numeradas: a versão 3.5 consolida as alterações
posteriores à entrega 3.4. A cada nova entrega, incrementar a versão (próxima:
3.10), atualizar MainForm.Layout.cs, AssemblyInfo.cs, LEIA-ME.txt, este documento
e VERSOES.md. Executar os testes antes das alterações e novamente após compilar.

Distribuir um ZIP completo Cotador-de-Voos-Windows-vX.Y.zip na pasta releases,
com a pasta interna Cotador contendo executável, Ocr.ps1, fontes, documentação
e testes. Não incluir releases dentro do pacote. Preservar pacotes anteriores;
não sobrescrever entregas publicadas. Os ZIPs até 3.4 continuam na pasta outputs,
acima deste projeto. Não criar retrospectivamente pacotes intermediários.
## Interface e ícone — 3.6

Os cantos pretos vinham do modo Flat/UserPaint do ButtonBase: o preenchimento
arredondado deixava pixels sem pintar, pois o fundo nem sempre era desenhado.
ModernButton agora repinta explicitamente o fundo do pai em OnPaint, respeitando
painéis transparentes, antes de desenhar contorno, preenchimento, texto e foco.
Não voltar a depender apenas de OnPaintBackground. O foco de teclado mantém
um indicador arredondado; clique, hover e estado desabilitado têm cores próprias.

assets/Cotador.svg é o desenho vetorial original; assets/Build-Icon.ps1 gera
Cotador.ico (16, 24, 32, 48, 64, 128 e 256 px) e Cotador.png. Build.ps1 incorpora
ICO e PNG como recursos e define o ícone nativo do executável. O usuário não
precisa dos arquivos assets para executar; eles são necessários para recompilar.

Além de tests/Test.ps1 (1.228 verificações), executar:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Test-Visual.ps1
```

Esse teste renderiza controles fora da tela, sem abrir janelas nem usar o
clipboard. Verifica os cantos dos dois tipos de botão em quatro estados e após
redimensionamento (35 verificações). Gera imagens em tests/visual-output para
inspeção de 1400×900 e 1160×800. Essa pasta de saída não entra nos ZIPs de entrega.
A inspeção não substitui testes em outros monitores e escalas de DPI do Windows.
Incluir assets e os novos testes no pacote de cada versão.
## Ida e volta no mesmo dia — 3.7

Dois voos em sentidos inversos entre as mesmas cidades são agrupados em duas
linhas antes da checagem de continuidade entre aeroportos. A cidade é comparada
pelo catálogo Cities; o IATA exato também é aceito. A volta não pode sair antes
da chegada da ida, mas não há estadia mínima para esses dois voos. Os IATA
originais são mantidos e a interface informa quando há troca de aeroporto.
A regra vale também para datas diferentes e aeroporto alternativo na origem.

Essa equivalência não se aplica a conexões nem a sequências com mais de dois
segmentos, que mantêm as heurísticas anteriores. Não calcula deslocamento
terrestre, não garante tempo de traslado e não resolve toda rota de múltiplos
 destinos. A revisão manual continua necessária.

A amostra same-day.ocr.txt foi obtida com Ocr.ps1 na escala 3 a partir do print
codex-clipboard-2a8df125-435f-4412-9536-3840a313ab9f.png enviado em 10/09/2026.
Resultado conferido: SDU–CGH, 17/09, 06:20–07:30; GRU–SDU, 17/09, 20:50–21:50.
O ano 2026 é o parâmetro do teste; a companhia ficou vazia no OCR embora o print
mostre Azul. São agora 13 amostras completas e 1.220 verificações de cotação.

## Correção CCH para CGH — 3.8

Exceção pontual relatada pelo usuário: CCH vira CGH em tabelas com dois códigos
e duração numérica opcional, ou acompanhado por São Paulo/Congonhas.
Rótulos com outra cidade são preservados. Não há substituição global C/G.
Os oito novos testes são sintéticos; nenhum novo print foi fornecido nesta correção.
Total atual: 1.228 testes de cotação e 35 de renderização.


## Entrada monetária — 3.9
MoneyTextBox.cs formata a entrada em centavos: 245000 vira 2.450,00.
Mantém o cursor por quantidade de dígitos à direita e trata Backspace/Delete
para não ficar preso nos separadores. O TextChanged externo só é emitido
quando o valor formatado muda, preservando a invalidação da revisão.
Colagens negativas, letras e mais de 15 dígitos significativos são rejeitadas
sem truncar o valor. Limpar todos os caracteres mantém o campo vazio.
Test-Visual.ps1 inclui 15 verificações de entrada monetária: 50 no total.
A suíte de cotações mantém 1.228 verificações.
