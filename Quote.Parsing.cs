using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

public static partial class Quote
{
    static List<string> ReadAirportPair(string text, bool normalize)
    {
        Func<string, string> code = value => normalize ? NormalizeAirportCode(value.ToUpperInvariant()) : value.ToUpperInvariant();
        var labeled = Regex.Matches(text, @"\b(?<code>GRIJ|[A-Z0-9]{2,3})\s*[-–—]\s*(?=[A-Za-zÀ-ÿ])", RegexOptions.IgnoreCase).Cast<Match>().Select(m => code(m.Groups["code"].Value)).ToList();
        if (labeled.Count >= 2) return labeled.Take(2).ToList();
        string pattern = normalize ? @"\b(?:GRIJ|(?=[A-Z0-9]*[A-Z])[A-Z0-9]{3})\b" : @"\b(?:US|UM|GRIJ|(?=[A-Z0-9]*[A-Z])[A-Z0-9]{3})\b";
        var bare = Regex.Matches(text, pattern, RegexOptions.IgnoreCase).Cast<Match>().Select(m => code(m.Value)).Where(c => c != "SAO").ToList();
        return bare.Count < 2 ? new List<string>() : new List<string> { bare.First(), bare.Last() };
    }
    static string NormalizeTime(string value)
    {
        string normalized = value.ToUpperInvariant()
            .Replace('O', '0').Replace('I', '1').Replace('L', '1').Replace('T', '1');
        TimeSpan parsedTime;
        // Uma sequência reconhecida pode ter o formato correto, mas ser impossível.
        // Mantenha a falha como erro de leitura para permitir a segunda escala de OCR.
        if (!TimeSpan.TryParseExact(normalized, @"hh\:mm", CultureInfo.InvariantCulture, out parsedTime))
            throw new QuoteReadException("Um horário do print é inválido. Revise e preencha manualmente.");
        return normalized;
    }

    /// <summary>Lê uma linha por voo, sem agrupar conexões e sem inventar campos ausentes.</summary>
    public static List<Flight> Parse(string text, int year)
    {
        var flights = new List<Flight>();
        int previousMonth = 0;
        var pattern = @"(?<day>\d{1,2}|II|I1|1I)\s+(?<month>Jan|Fev|Mar|Abr|Mai|Jun|Jul|Ago|Set|Out|Nov|Dez)\s+(?<hour>[\dOILT]{2})[:.•\s]*(?<minute>[\dOIL]{2})h?";
        foreach (var line in text.Split('\n'))
        {
            var dateMatches = Regex.Matches(line, pattern, RegexOptions.IgnoreCase);
            if (dateMatches.Count < 2)
            {
                if (Regex.IsMatch(line, @"\b(Jan|Fev|Mar|Abr|Mai|Jun|Jul|Ago|Set|Out|Nov|Dez)\b", RegexOptions.IgnoreCase))
                    throw new QuoteReadException("Um trecho teve data ou horário ilegível. Revise o print e preencha manualmente.");
                continue;
            }

            var flight = new Flight();
            Func<Match, DateTime> parsePrintedDate = match =>
            {
                int month = Array.FindIndex(new[] { "jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez" }, s => s == match.Groups["month"].Value.ToLowerInvariant()) + 1;
                // O print não informa ano. A sequência de meses usa o ano escolhido
                // e avança quando o mês diminui; o usuário deve conferir a sugestão.
                if (previousMonth > month)
                    year++;
                previousMonth = month;
                return new DateTime(year, month, int.Parse(match.Groups["day"].Value.ToUpperInvariant().Replace('I', '1'), CultureInfo.InvariantCulture));
            };
            try
            {
                flight.Date = parsePrintedDate(dateMatches[0]).ToString("dd/MM/yyyy");
                flight.ArrivalDate = parsePrintedDate(dateMatches[1]).ToString("dd/MM/yyyy");
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new QuoteReadException("Uma data do print não pôde ser reconhecida. Revise e preencha manualmente.");
            }

            flight.Departure = NormalizeTime(dateMatches[0].Groups["hour"].Value + ":" + dateMatches[0].Groups["minute"].Value);
            flight.Arrival = NormalizeTime(dateMatches[1].Groups["hour"].Value + ":" + dateMatches[1].Groups["minute"].Value);
            var printedAirports = line.Substring(dateMatches[1].Index + dateMatches[1].Length);
            var airportCodes = ReadAirportPair(NormalizeAirportLabels(printedAirports), true);
            if (airportCodes.Count >= 2)
            {
                flight.From = airportCodes[0];
                flight.To = airportCodes[1];
                var original = ReadAirportPair(printedAirports, false);
                for (int i = 0; i < original.Count; i++)
                    if (original[i] != airportCodes[i])
                        flight.Notices.Add(new FlightNotice { Column = i, Value = airportCodes[i], Message = "OCR: " + original[i] + " corrigido para " + airportCodes[i] + ". Confira com o print." });
            }

            // Restrinja a correção ao início da linha (CIA e número do voo).
            flight.Airline = ReadAirline(line.Substring(0, dateMatches[0].Index));
            flights.Add(flight);
        }

        return flights;
    }
}
