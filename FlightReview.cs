using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class FlightReview
{
    public static Dictionary<int, string> Inspect(string[] values, IEnumerable<FlightNotice> notices)
    {
        var warnings = new Dictionary<int, string>();
        if (values.All(string.IsNullOrWhiteSpace)) return warnings;
        string[] labels = { "Origem", "Destino", "Data de saída", "Data de chegada", "Horário de saída", "Horário de chegada", "Companhia", "Conexões ou voo direto" };
        for (int i = 0; i < values.Length; i++)
            if (string.IsNullOrWhiteSpace(values[i])) warnings[i] = labels[i] + " não preenchido(a). Confira o print e complete o campo.";
        for (int i = 0; i <= 1; i++)
            if (!string.IsNullOrWhiteSpace(values[i]) && !Quote.Cities.ContainsKey(values[i].Trim().ToUpperInvariant()))
                warnings[i] = "Código fora do catálogo. Confira o IATA ou use Buscar aeroporto (F3).";
        if (!string.IsNullOrWhiteSpace(values[0]) && values[0].Equals(values[1], StringComparison.OrdinalIgnoreCase))
            warnings[1] = "Origem e destino são iguais. Confira os aeroportos.";
        DateTime departure, arrival;
        bool validDeparture = QuoteValidation.TryReadDate(values[2], out departure);
        bool validArrival = QuoteValidation.TryReadDate(values[3], out arrival);
        if (values[2] != "" && !validDeparture) warnings[2] = "Data inválida. Use dd/mm/aa, por exemplo 12/11/26.";
        if (values[3] != "" && !validArrival) warnings[3] = "Data inválida. Use dd/mm/aa, por exemplo 12/11/26.";
        if (validDeparture && validArrival && arrival < departure) warnings[3] = "A chegada está em uma data anterior à saída.";
        for (int i = 4; i <= 5; i++)
        {
            TimeSpan time;
            if (values[i] != "" && !TimeSpan.TryParseExact(values[i], @"hh\:mm", CultureInfo.InvariantCulture, out time))
                warnings[i] = "Horário inválido. Use hh:mm, entre 00:00 e 23:59.";
        }
        foreach (var notice in notices)
            if (values[notice.Column].Equals(notice.Value, StringComparison.OrdinalIgnoreCase))
                warnings[notice.Column] = warnings.ContainsKey(notice.Column) ? warnings[notice.Column] + " " + notice.Message : notice.Message;
        return warnings;
    }
}
