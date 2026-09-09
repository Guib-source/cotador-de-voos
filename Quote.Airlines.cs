using System;
using System.Text.RegularExpressions;

public static partial class Quote
{
    /// <summary>Normaliza erros conhecidos na coluna da companhia.</summary>
    /// <remarks>
    /// Recebe somente o texto antes da primeira data do voo. Não procura nomes
    /// em aeroportos ou cidades e não deduz uma companhia a partir da rota.
    /// Um logotipo sem texto reconhecido continua exigindo revisão manual.
    /// </remarks>
    public static string ReadAirline(string airlineColumn)
    {
        if (string.IsNullOrWhiteSpace(airlineColumn)) return "";

        // Permita espaços entre letras, mas não uma correspondência dentro de
        // palavras maiores. G/C, O/0, L/I/1 e Z/2 são confusões comuns do OCR.
        const string start = @"(?<![A-Z0-9])";
        const string end = @"(?![A-Z0-9])";
        string[] patterns = {
            @"[GC]\s*[O0]\s*[LI1]",
            @"A\s*[Z2]\s*U\s*[LI1]",
            @"[LI1]\s*A\s*T\s*A\s*M",
            @"TAP",
            @"AVIANCA"
        };
        string[] names = { "GOL", "AZUL", "LATAM", "TAP", "AVIANCA" };
        string recognized = "";
        for (int index = 0; index < patterns.Length; index++)
        {
            if (!Regex.IsMatch(airlineColumn, start + patterns[index] + end,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) continue;

            // Dois nomes diferentes na mesma coluna são ambíguos: não escolha um.
            if (recognized.Length > 0) return "";
            recognized = names[index];
        }
        return recognized;
    }
}
