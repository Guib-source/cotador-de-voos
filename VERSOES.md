# Histórico de versões — Cotador de Voos

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
