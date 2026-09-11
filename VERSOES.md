# Histórico de versões — Cotador de Voos

## 3.9 — 11/09/2026

- Campo de valor com máscara monetária brasileira durante a digitação.
  Digite os centavos: 1 → 0,01; 100 → 1,00; 245000 → 2.450,00.
- Aceita colagem de valores brasileiros como R$ 2.450,00; mantém seleção,
  exclusão de dígitos e limpeza na nova cotação. Letras e valores negativos
  são rejeitados; entradas acima de 15 dígitos significativos não são truncadas.
- Alterar o valor continua invalidando a revisão e a cotação anterior.
- 1.228 verificações de cotação e 50 verificações de interface (15 novas para valor).
- Pacote: `releases/Cotador-de-Voos-Windows-v3.9.zip`.

## 3.8 — 10/09/2026

- Corrige CCH → CGH na origem e no destino de tabelas sem cidades, incluindo
  duração após os códigos, ou com rótulos São Paulo/Congonhas.
- Mantém códigos acompanhados por cidade incompatível e não troca C/G globalmente.
- Oito novos testes sintéticos, incluindo ida e volta no mesmo dia e cotação.
- 1.228 verificações de cotação e 35 de renderização aprovadas.
- Pacote: `releases/Cotador-de-Voos-Windows-v3.8.zip`.

## 3.7 — 10/09/2026

- Identifica dois voos de ida e volta entre as mesmas cidades, no mesmo dia
  ou em dias diferentes, mesmo com aeroportos distintos em cada sentido.
- Corrige o caso real SDU–CGH / GRU–SDU de 17/09: a troca CGH/GRU antes
  fazia a validação de continuidade rejeitar a importação.
- Preserva os IATA reais e informa a troca de aeroporto no status da revisão.
  Não inventa conexão ou traslado terrestre nem presume tempo suficiente para ele.
- Mantém a continuidade estrita para conexões e itinerários com mais de dois
  segmentos; cidades desconhecidas ou incompatíveis continuam exigindo revisão.
- 1.220 verificações de cotação e 35 de renderização; 13 amostras completas.
- OCR real do print enviado em 10/09/2026 executado na escala 3 e conferido.
- Pacote: `releases/Cotador-de-Voos-Windows-v3.7.zip`.

## 3.6 — 09/09/2026

- Correção dos cantos pretos em botões: pintura explícita do fundo do pai
  antes do preenchimento arredondado, inclusive sobre painéis transparentes.
- Contornos suaves e estados de hover, pressionado, foco de teclado e desabilitado.
- Ícone original de avião em verde, incorporado ao executável e à janela,
  com resoluções de 16 a 256 pixels; marca no cabeçalho.
- Campos de valor e passageiros com contorno, linhas alternadas no itinerário
  e rótulos compactos de datas para janelas menores.
- 1.207 verificações de cotação e 35 verificações de renderização aprovadas.
- Interface renderizada e inspecionada em 1400×900 e 1160×800; botões normais,
  com hover, pressionados, desabilitados e após redimensionamento.
- Pacote: `releases/Cotador-de-Voos-Windows-v3.6.zip`.

## 3.5 — 09/09/2026

- Catálogo com 552 aeroportos: 345 brasileiros e 207 internacionais.
- Correção de confusões de letras e números no IATA, confirmada pela cidade.
- Exceção LOB → LDB em tabelas sem cidades, conferida com o print de Londrina.
- 1.207 verificações; 12 amostras completas de cotação.
- Versão na janela, documentação e propriedades do executável.
- Pacote: `releases/Cotador-de-Voos-Windows-v3.5.zip`.

Consolida as alterações posteriores à 3.4 que ainda não tinham um pacote
numerado. Os testes gerais de OCR usam texto simulado; a amostra de Londrina
usa o texto real enviado, com resultado conferido com o print.

## 3.4 — entrega anterior à transferência para o projeto

- Reconhecimento de GOL, Azul e LATAM; 44 verificações na entrega original.
- Pacote original preservado: `../Cotador-de-Voos-Windows-v3.4.zip`.

## 3.3 — revisão estrutural

- Separação dos módulos, build reproduzível, documentação e testes portáteis.
- Pacote original preservado: `../Cotador-de-Voos-Windows-v3.3-revisado.zip`.

## Entregas anteriores

Os pacotes 3.2, 3.1, 3.0, 2.1 e 2 permanecem na pasta outputs, acima do projeto.
Não foram recriados ou modificados nesta retomada do versionamento.
