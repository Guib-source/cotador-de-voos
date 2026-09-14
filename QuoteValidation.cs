using System;
using System.Globalization;
using System.Linq;

/// <summary>Validação dos campos editados, independente dos controles da janela.</summary>
public static class QuoteValidation
{
    public const string DateFormat = "dd/MM/yyyy";
    public static readonly CultureInfo BrazilianCulture = new CultureInfo("pt-BR");
    public static bool TryReadDate(string text, out DateTime date)
    {
        // Dois dígitos representam explicitamente 2000–2099, independente do Windows.
        if (text.Length == 8 && text[2] == '/' && text[5] == '/')
            text = text.Substring(0, 6) + "20" + text.Substring(6);
        return DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }
    // A ordem corresponde às colunas da tabela, incluindo a coluna oculta de conexões.
    public static Flight ReadFlight(string[] values)
    {
        if (values.Length != 8)
            throw new ArgumentException("A linha deve conter oito campos.", "values");
        if (values.All(string.IsNullOrWhiteSpace))
            return null; // A linha VOLTA vazia representa uma viagem somente de ida.
        if (values.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Preencha todos os campos do trecho. Para somente ida, deixe toda a linha VOLTA vazia.");
        DateTime departureDate, arrivalDate;
        TimeSpan departureTime, arrivalTime;
        if (!TryReadDate(values[2], out departureDate) || !TryReadDate(values[3], out arrivalDate))
            throw new ArgumentException("Use datas válidas no formato dd/MM/aa.");
        if (!TimeSpan.TryParseExact(values[4], @"hh\:mm", CultureInfo.InvariantCulture, out departureTime) || !TimeSpan.TryParseExact(values[5], @"hh\:mm", CultureInfo.InvariantCulture, out arrivalTime))
            throw new ArgumentException("Use horários válidos no formato HH:mm.");
        if (arrivalDate < departureDate)
            throw new ArgumentException("A data de chegada não pode ser anterior à saída.");
        // Horários locais podem retroceder em voos internacionais. Não os compare
        // como se origem e destino estivessem no mesmo fuso horário.
        return new Flight
        {
            From = values[0],
            To = values[1],
            Date = departureDate.ToString(DateFormat),
            ArrivalDate = arrivalDate.ToString(DateFormat),
            Departure = values[4],
            Arrival = values[5],
            Airline = values[6],
            Connection = values[7]
        };
    }

    public static decimal ReadAmount(string text)
    {
        decimal amount;
        if (!decimal.TryParse(text, NumberStyles.Number, BrazilianCulture, out amount) || amount <= 0)
            throw new ArgumentException("Informe um valor positivo, por exemplo 2.450,00.");
        return amount;
    }
}
