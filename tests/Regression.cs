using System;
using System.IO;
using System.Linq;

internal static class Regression
{
    private static int assertions;
    private static Flight AirportLine(string airports)
    {
        return Quote.Parse("AZUL 1234 20 Out 02:15h 20 Out 05:45h " + airports, 2026)[0];
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        assertions++;
    }

    private static void Reject(Action action, string message)
    {
        bool rejected = false;
        try { action(); } catch (ArgumentException) { rejected = true; }
        Check(rejected, message);
    }

    public static int Main(string[] args)
    {
        try
        {
            // As 11 referências antigas vieram da versão 3.2; Londrina foi
            // conferida com o print fornecido em 09/09/2026.
            // A comparação é byte a byte para preservar o texto enviado aos clientes.
            foreach (string fixture in Directory.GetFiles(args[0], "*.ocr.txt"))
            {
                var journeys = Quote.Group(Quote.Parse(File.ReadAllText(fixture), 2026));
                string actual = Quote.Format(journeys, 1, "R$ 2.450,00", Quote.Baggage(false));
                string expected = File.ReadAllText(fixture.Replace(".ocr.txt", ".expected.txt"));
                Check(actual == expected, "Cotação mudou: " + Path.GetFileName(fixture));
            }
            Check(QuoteValidation.ReadFlight(new string[8]) == null, "Linha vazia deve ser opcional.");
            Reject(() => QuoteValidation.ReadAmount("0"), "Valor zero deve ser rejeitado.");
            Reject(() => QuoteValidation.ReadAmount("abc"), "Valor inválido deve ser rejeitado.");
            Check(QuoteValidation.ReadAmount("2.450,00") == 2450m, "Formato brasileiro.");
            var values = new[] { "MIA", "GRU", "24/11/2026", "24/11/2026", "09:20", "19:45", "LATAM", "Voo direto" };
            Check(QuoteValidation.ReadFlight(values).From == "MIA", "Campos válidos.");
            values[4] = "25:00";
            Reject(() => QuoteValidation.ReadFlight(values), "Hora inválida deve ser rejeitada.");
            values[4] = "09:20"; values[3] = "23/11/2026";
            Reject(() => QuoteValidation.ReadFlight(values), "Chegada anterior deve ser rejeitada.");
            Check(Quote.Baggage(true).Contains("despachada"), "Bagagem despachada.");
            Check(Quote.Baggage(false).Contains("somente"), "Bagagem de mão.");
            bool invalidOcrRejected = false;
            try { Quote.Parse("LATAM 1 20 Out 99:00h 20 Out 10:00h GRU SLZ", 2026); }
            catch (QuoteReadException) { invalidOcrRejected = true; }
            Check(invalidOcrRejected, "Horário impossível deve permitir a segunda leitura OCR.");
            var lowercaseDay = Quote.Parse("LATAM 1 ii Jan 03:50h 11 Jan 07:30h SLZ GRU", 2026);
            Check(lowercaseDay[0].Date == "11/01/2026", "Normalização do dia deve ignorar maiúsculas/minúsculas.");
            foreach (string name in new[] { "GOL", "COL", "C0L", "G0L", "GOI", "GO1", "C O L" })
                Check(Quote.ReadAirline(name + " 1234") == "GOL", "Reconhecimento GOL: " + name);
            foreach (string name in new[] { "Azul", "AZUI", "AZU1", "A2UL", "A Z U L" })
                Check(Quote.ReadAirline(name + " 1234") == "AZUL", "Reconhecimento AZUL: " + name);
            foreach (string name in new[] { "LATAM", "LATA M", "IATAM", "1ATAM", "L A T A M" })
                Check(Quote.ReadAirline(name + " 1234") == "LATAM", "Reconhecimento LATAM: " + name);
            Check(Quote.ReadAirline("1234") == "", "Não inventar companhia ausente.");
            Check(Quote.ReadAirline("ESCOLA 1234") == "", "Não corrigir dentro de palavras.");
            Check(Quote.ReadAirline("GOL AZUL 1234") == "", "Não escolher companhia ambígua.");
            var golFlight = Quote.Parse("COL 1234 20 Out 02:15h 20 Out 05:45h SLZ GRU", 2026);
            Check(golFlight[0].Airline == "GOL", "Integração COL para GOL no parser.");
            var noAirline = Quote.Parse("1234 20 Out 02:15h 20 Out 05:45h SLZ - GOL GRU - SAO PAULO", 2026);
            Check(noAirline[0].Airline == "", "Não buscar companhia nas cidades.");
            // Cobertura regional, destinos internacionais e contrato público de exibição.
            foreach (string entry in new[] {
                "RBR|Rio Branco", "MCP|Macapá", "PVH|Porto Velho", "BVB|Boa Vista",
                "CGR|Campo Grande", "FEN|Fernando de Noronha", "JJD|Cruz",
                "STM|Santarém", "OPS|Sinop", "RAO|Ribeirão Preto", "XAP|Chapecó",
                "JFK|Nova York", "EZE|Buenos Aires", "CDG|Paris", "LHR|Londres",
                "DXB|Dubai", "JNB|Joanesburgo", "HND|Tóquio", "SYD|Sydney" })
            {
                string[] pair = entry.Split('|');
                Check(Quote.Place(pair[0]) == pair[1] + " (" + pair[0] + ")", "Cidade do aeroporto: " + pair[0]);
            }
            Check(Quote.Place("  jfk  ") == "Nova York (JFK)", "Normalizar espaços e caixa na consulta.");
            Check(Quote.Place("ZZZ") == "ZZZ", "Preservar código desconhecido para revisão manual.");
            Check(!Quote.Cities.ContainsKey("NYC") && !Quote.Cities.ContainsKey("SAO"), "Não cadastrar códigos metropolitanos como aeroportos.");
            Check(Quote.Cities.All(p => System.Text.RegularExpressions.Regex.IsMatch(p.Key, "^[A-Z]{3}$") && !string.IsNullOrWhiteSpace(p.Value)), "Catálogo deve ter IATA válido e cidade preenchida.");
            var regionalFlight = Quote.Parse("AZUL 1234 20 Out 02:15h 20 Out 05:45h RBR STM", 2026);
            string regionalQuote = Quote.Format(Quote.Group(regionalFlight), 1, "R$ 2.450,00", Quote.Baggage(false));
            Check(regionalQuote.Contains("Rio Branco (RBR)") && regionalQuote.Contains("Santarém (STM)"), "Novos aeroportos devem aparecer na cotação completa.");
            foreach (var airport in Quote.Cities)
            {
                var plain = AirportLine(airport.Key + " GRU");
                Check(plain.From == airport.Key && plain.To == "GRU", "Preservar código isolado: " + airport.Key);
                var labeled = AirportLine("GRU - São Paulo " + airport.Key.ToLowerInvariant() + " - " + airport.Value);
                Check(labeled.From == "GRU" && labeled.To == airport.Key, "Reconhecer catálogo com cidade e caixa baixa: " + airport.Key);
            }
            foreach (string sample in new[] {
                "LOB|Londrina|LDB", "L0B|Londrina|LDB", "LQB|Londrina|LDB",
                "L08|Londrina|L08", "LD8|Londrina|LDB", "1DB|Londrina|LDB",
                "C6H|Sao Paulo|CGH", "6RU|São Paulo|GRU", "8SB|Brasilia|BSB",
                "F1N|Florianopolis|FLN", "S0U|Rio de Janeiro|SDU",
                "5LZ|São Luís|SLZ", "CN5|Belo Horizonte|CN5",
                "DOU|Doha|DOU", "D0H|Doha|DOH", "CD6|Paris|CDG",
                "L6W|Londres|LGW", "JNB|Joanesburgo|JNB",
                "SY0|Sydney|SYD", "NRT|Tóquio|NRT", "HNO|Toquio|HND",
                "LOB|Outra cidade|LOB", "LOB|LondrinaExtra|LOB",
                "ZZZ|Londrina|ZZZ", "CGB|Cuiabá|CGB", "CGH|São Paulo|CGH" })
            {
                var parts = sample.Split('|');
                var result = AirportLine(parts[0] + " - " + parts[1] + " GRU - São Paulo");
                Check(result.From == parts[2] && result.To == "GRU", "Correção contextual: " + sample);
            }
            Check(AirportLine("LOB GRU").From == "LDB", "Exceção LOB confirmada pelo print sem cidades.");
            Check(AirportLine("VCP lob 01:20").To == "LDB", "Corrigir LOB antes da duração da conexão.");
            Check(AirportLine("LDB VCP").From == "LDB", "Preservar Londrina reconhecida corretamente.");
            Check(AirportLine("D0H GRU").From == "D0H", "Sem cidade preservar código com dígito para revisão.");
            Check(AirportLine("LOB LONDRINA GRU SAO PAULO").From == "LDB", "Cidade sem hífen confirma Londrina.");
            Check(AirportLine("GRU - São Paulo LOB — londrina").To == "LDB", "Corrigir destino com travessão.");
            Check(AirportLine("LGA - Nova York JFK - Nova York").To == "JFK", "Cidade compartilhada preserva aeroporto exato.");
            // Códigos de uma mesma cidade podem ser igualmente próximos: não escolher.
            Quote.Cities.Add("QDQ", "Cidade de Teste");
            Quote.Cities.Add("QOQ", "Cidade de Teste");
            try { Check(AirportLine("Q0Q - Cidade de Teste GRU - São Paulo").From == "Q0Q", "Ambiguidade deve exigir revisão."); }
            finally { Quote.Cities.Remove("QDQ"); Quote.Cities.Remove("QOQ"); }
            Console.WriteLine("PASS: " + assertions + " verificações.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }
}
