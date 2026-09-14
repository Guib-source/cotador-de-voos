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

    private static void RejectItinerary(string text, string message)
    {
        bool rejected = false;
        try { Quote.Group(Quote.Parse(text, 2026)); }
        catch (QuoteReadException) { rejected = true; }
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
            string outbound = "AZUL 6407 17 Set 06:20h 17 Set 07:30h SDU CGH\n";
            var sameDay = Quote.Group(Quote.Parse(outbound + "AZUL 6052 17 Set 20:50h 17 Set 21:50h GRU SDU", 2026));
            Check(sameDay.Count == 2 && sameDay[0].Date == sameDay[1].Date, "Ida e volta no mesmo dia com troca de aeroporto.");
            Check(sameDay[0].To == "CGH" && sameDay[1].From == "GRU", "Preservar aeroportos distintos em São Paulo.");
            Check(sameDay.All(f => f.Connection == "Voo direto"), "Não inventar conexão nem traslado entre CGH e GRU.");
            Check(Quote.Group(Quote.Parse(outbound + "AZUL 6052 17 Set 08:00h 17 Set 09:00h CGH SDU", 2026)).Count == 2, "Mesmo aeroporto: ida e volta não exigem estadia mínima.");
            Check(Quote.Group(Quote.Parse(outbound + "AZUL 6052 17 Set 20:50h 17 Set 21:50h GRU GIG", 2026))[1].To == "GIG", "Permitir aeroporto alternativo também na cidade de origem.");
            Check(Quote.Group(Quote.Parse(outbound + "AZUL 6052 18 Set 20:50h 18 Set 21:50h GRU SDU", 2026)).Count == 2, "Troca de aeroporto também em datas diferentes.");
            var oneWay = Quote.Group(Quote.Parse(outbound + "AZUL 6052 17 Set 08:50h 17 Set 10:50h CGH BSB", 2026));
            Check(oneWay.Count == 1 && oneWay[0].To == "BSB" && oneWay[0].Connection.Contains("1 conexão"), "Preservar somente ida com conexão no mesmo dia.");
            RejectItinerary(outbound + "AZUL 6052 17 Set 07:00h 17 Set 08:00h GRU SDU", "Rejeitar volta anterior à chegada da ida.");
            RejectItinerary(outbound + "AZUL 6052 17 Set 20:50h 17 Set 21:50h BSB SDU", "Não unir destinos de cidades diferentes.");
            RejectItinerary(outbound + "AZUL 6052 17 Set 20:50h 17 Set 21:50h GRU BSB", "Não interpretar troca de aeroporto como conexão de somente ida.");
            RejectItinerary("AZUL 1 17 Set 06:20h 17 Set 07:30h SDU XYZ\nAZUL 2 17 Set 20:50h 17 Set 21:50h QQQ SDU", "Não supor cidade de aeroporto desconhecido.");
            RejectItinerary(outbound + "AZUL 2 17 Set 10:00h 17 Set 11:00h GRU BSB\nAZUL 3 17 Set 20:00h 17 Set 21:00h BSB SDU", "Não relaxar continuidade em itinerários com mais de dois segmentos.");
            Check(AirportLine("SDU CCH").To == "CGH", "Corrigir CCH no destino sem cidade.");
            Check(AirportLine("cch SDU 01:20").From == "CGH", "Corrigir CCH na origem com duração e caixa baixa.");
            Check(AirportLine("CCH - São Paulo SDU - Rio de Janeiro").From == "CGH", "Cidade confirma CCH para CGH.");
            Check(AirportLine("SDU - Rio de Janeiro CCH - Congonhas").To == "CGH", "Nome Congonhas confirma CGH.");
            Check(AirportLine("CCH - Outra cidade SDU - Rio de Janeiro").From == "CCH", "Não corrigir CCH com cidade incompatível.");
            Check(AirportLine("CGB CGH").From == "CGB", "Não trocar C/G em outros códigos.");
            var cchRoundTrip = Quote.Group(Quote.Parse("6407 17 Set 06:20h 17 Set 07:30h SDU CCH\n6052 17 Set 20:50h 17 Set 21:50h GRU SDU", 2026));
            Check(cchRoundTrip.Count == 2 && cchRoundTrip[0].To == "CGH" && cchRoundTrip[1].From == "GRU", "Integrar CCH à ida e volta no mesmo dia.");
            Check(Quote.Format(cchRoundTrip, 1, "R$ 2.450,00", Quote.Baggage(false)).Contains("São Paulo (CGH)"), "Exibir CGH corrigido na cotação.");
            bool multi;
            string openJaw = "LATAM 3313 12 Nov 04:35h 12 Nov 07:05h SLZ BSB\nLATAM 3024 16 Nov 07:00h 16 Nov 09:35h BSB FOR";
            var many = Quote.Identify(Quote.Parse(openJaw, 2026), false, out multi);
            Check(multi && many.Count == 2 && many[0].From == "SLZ" && many[1].To == "FOR", "Detectar automaticamente o exemplo SLZ–BSB–FOR.");
            Check(many[0].Date == "12/11/2026" && many[1].Departure == "07:00" && many[1].Arrival == "09:35", "Preservar datas e horários do exemplo.");
            string multiText = Quote.Format(many, 1, "R$ 2.450,00", Quote.Baggage(false), true);
            Check(multiText.Contains("TRECHO 1") && multiText.Contains("TRECHO 2") && !multiText.Contains("VOLTA") && multiText.Contains("(2 trechos)"), "Mensagem identifica múltiplos trechos e valor total.");
            string three = openJaw + "\nLATAM 3025 20 Nov 07:00h 20 Nov 09:35h FOR REC";
            many = Quote.Identify(Quote.Parse(three, 2026), false, out multi);
            Check(multi && many.Count == 3 && many[2].To == "REC", "Não truncar terceiro trecho.");
            var normal = Quote.Identify(Quote.Parse(outbound + "AZUL 6052 17 Set 20:50h 17 Set 21:50h GRU SDU", 2026), false, out multi);
            Check(!multi && normal.Count == 2, "Preservar ida e volta automática no mesmo dia.");
            normal = Quote.Identify(Quote.Parse(outbound + "AZUL 6052 17 Set 20:50h 17 Set 21:50h GRU SDU", 2026), true, out multi);
            Check(multi && normal.Count == 2, "Respeitar escolha explícita de múltiplos trechos.");
            bool badMulti = false;
            try { Quote.Identify(Quote.Parse(outbound + "AZUL 6052 17 Set 07:00h 17 Set 08:00h GRU SDU", 2026), false, out multi); }
            catch (QuoteReadException) { badMulti = true; }
            Check(badMulti, "Alternativa múltipla não esconde horários inconsistentes.");
            var shortDates = new[] { "SLZ", "BSB", "12/11/26", "12/11/26", "04:35", "07:05", "LATAM", "Voo direto" };
            Check(QuoteValidation.ReadFlight(shortDates).Date == "12/11/2026", "Normalizar ano curto para a cotação.");
            shortDates[2] = shortDates[3] = "29/02/24";
            Check(QuoteValidation.ReadFlight(shortDates).Date == "29/02/2024", "Aceitar ano bissexto curto.");
            shortDates[2] = "29/02/26";
            Reject(() => QuoteValidation.ReadFlight(shortDates), "Rejeitar data impossível após máscara.");
            Check(EntryMask.Format("121126", true) == "12/11/26", "Máscara de data.");
            Check(EntryMask.Format("0435", false) == "04:35", "Máscara de hora com zero inicial.");
            Check(EntryMask.Format("12/11/2026", true) == "12/11/2026", "Preservar colagem de ano completo.");
            Console.WriteLine("PASS: " + assertions + " verificações.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error.Message); return 1; }
    }
}
