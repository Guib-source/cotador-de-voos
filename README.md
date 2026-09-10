# Cotador de Voos

Aplicativo Windows em português que lê um print de voos com OCR local,
permite revisar o itinerário e gera uma cotação para copiar ao atendimento.

![Ícone do Cotador](assets/Cotador.png)

**Versão atual: 3.7.** [Histórico de versões](VERSOES.md).

## Usar o programa

Baixe o ZIP da versão em **Releases**, extraia todo o conteúdo e abra
`Cotador.exe`. Mantenha `Ocr.ps1` na mesma pasta do executável.

Requer Windows 10/11, .NET Framework 4.x e um idioma de OCR instalado.
O reconhecimento acontece localmente, sem chave de API.

1. Selecione o ano e importe ou cole o print.
2. Confira aeroportos, datas, horários, companhia e conexões.
3. Informe valor, passageiros e bagagem.
4. Confirme a revisão e gere a cotação. Cole com Ctrl+V.

O OCR pode errar; a revisão manual é necessária. O programa não consulta preços
nem envia mensagens automaticamente. [Instruções completas](LEIA-ME.txt).

## Compilar e testar

Abra PowerShell na raiz do projeto, com o Cotador fechado:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Test.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Test-Visual.ps1
```

Não há pacotes externos. O build usa o compilador do .NET Framework instalado
no Windows. A versão 3.7 passou em 1.220 verificações de cotação e 35 de
renderização. As imagens de QA ficam em `tests/visual-output` e não são versionadas.

## Organização

- `Quote.*.cs`: catálogo, interpretação e formatação dos voos.
- `MainForm*.cs` e `ModernUI.cs`: interface Windows Forms.
- `OcrClient.cs` e `Ocr.ps1`: reconhecimento local.
- `assets`: ícone e fontes gráficas para recompilação.
- `tests`: regressões e amostras de OCR.

Consulte [MANUTENCAO.md](MANUTENCAO.md) antes de alterar regras de reconhecimento.
O catálogo usa dados de domínio público do [OurAirports](https://ourairports.com/data/).

## Entregas

O código-fonte é mantido no Git. Executáveis e ZIPs ficam em **Releases**, com
tags como `v3.6`. A pasta local `releases` é ignorada pelo Git. Pacotes históricos
citados na documentação podem existir apenas na pasta de trabalho original.
