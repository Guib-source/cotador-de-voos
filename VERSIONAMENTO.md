# Fluxo de desenvolvimento e entrega

- `main`: código da última entrega estável, agora 4.0.0.
- `codex/preparacao-v4.0`: branch usado para desenvolver e avaliar a versão 4.0.
- `4.0.0-beta.1` e `4.0.0-beta.2`: compilações históricas de avaliação.

## Identidade da compilação

`VersionInfo.cs` é a fonte da versão usada pela janela, pelos metadados do
executável e pelos scripts. `Assembly` contém a versão numérica do Windows;
`Display` contém a versão completa, incluindo o sufixo de avaliação.
Para novas versões de avaliação, use um sufixo `-beta.N`; a versão estável
usa somente o número da versão, como `4.0.0`.

`Build.ps1` grava em `bin/<versão>/`, junto com `Ocr.ps1`. Assim é possível
testar a beta mantendo o executável estável da raiz. Os testes usam a mesma
pasta por padrão. `-OutputDirectory` no build e `-AppDirectory` nos testes
permitem avaliar outra pasta explicitamente.

## Preparar uma entrega de avaliação

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Package.ps1
```

O script compila, executa as suítes e prepara o ZIP completo em `releases/`.
Confere a versão do executável e recusa sobrescrever um pacote existente.
Não publica no GitHub, não cria tags e não mescla alterações em `main`.
Os pacotes e executáveis continuam fora do Git.

## Publicação de uma versão estável

Depois de avaliar as mudanças no branch e revisar o diff:

1. Atualizar `VersionInfo.cs` e finalizar `RELEASE-NOTES.md`.
2. Compilar, executar os testes e conferir visualmente os fluxos alterados.
3. Integrar o branch em `main`, criar a tag `v4.0.0` e enviar ao GitHub público.
4. Preparar o pacote estável e publicar a release vinculada à tag.

As descrições de releases e os novos registros em `VERSOES.md` devem listar
**somente as mudanças**, sem quantidades de verificações, resultados de
testes ou logs técnicos. A evidência de validação fica nos testes e na revisão
do desenvolvimento. As entregas históricas publicadas são preservadas.
