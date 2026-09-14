using System.Collections.Generic;
using System.Text;

public static partial class Quote
{
    public static string Baggage(bool includesCheckedBaggage)
    {
        return includesCheckedBaggage ? "Inclui bagagem de mão e bagagem despachada" : "Inclui somente bagagem de mão";
    }

    /// <summary>Monta o texto comercial sem consultar preços ou enviar mensagens.</summary>
    /// <remarks>As quebras de linha e os símbolos fazem parte do formato solicitado.</remarks>
    public static string Format(List<Flight> flights, int passengers, string price, string baggage)
    {
        return Format(flights, passengers, price, baggage, false);
    }

    public static string Format(List<Flight> flights, int passengers, string price, string baggage, bool multiple)
    {
        var messageBuilder = new StringBuilder("✈️ Segue sua cotação especial para a sua próxima viagem:\r\n\r\n");
        for (int i = 0; i < flights.Count; i++)
        {
            var flight = flights[i];
            messageBuilder.AppendLine(Place(flight.From) + " ➡️ " + Place(flight.To));
            messageBuilder.AppendLine("📅 " + (multiple ? "TRECHO " + (i + 1) : (i == 0 ? "IDA" : "VOLTA")) + ": " + flight.Date);
            messageBuilder.AppendLine("✈️ CIA: " + flight.Airline + " | Passageiros: " + passengers.ToString("00"));
            messageBuilder.AppendLine("➡ Saída: " + flight.Departure + "h | " + flight.Connection);
            messageBuilder.AppendLine("➡ Chegada: " + flight.Arrival + "h em " + Place(flight.To) + (flight.ArrivalDate != flight.Date ? " em " + flight.ArrivalDate : ""));
            messageBuilder.AppendLine();
        }

        messageBuilder.AppendLine("💰 Valor: " + price + (multiple ? " (" + flights.Count + " trechos)" : (flights.Count == 2 ? " (ida e volta)" : " (somente ida)")));
        messageBuilder.AppendLine("🧳 Bagagem: " + baggage);
        messageBuilder.AppendLine();
        messageBuilder.Append("Valores sujeitos à disponibilidade e alteração sem aviso prévio.");
        return messageBuilder.ToString();
    }
}
