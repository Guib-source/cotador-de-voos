using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static partial class Quote
{
    private const double MinimumStayHours = 18;
    private const double MinimumGapSeparationHours = 6;
    private const double LongStopHours = 24;
    // As comparações são entre voos no MESMO aeroporto de conexão.
    // Não compare duração de voos internacionais sem conhecer os fusos.
    static DateTime GetLocalDateTime(Flight flight, bool arrival)
    {
        return DateTime.ParseExact((arrival ? flight.ArrivalDate : flight.Date) + " " + (arrival ? flight.Arrival : flight.Departure), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    static Flight CombineSegments(List<Flight> segments)
    {
        var first = segments.First();
        var last = segments.Last();
        var stops = new List<string>();
        for (int i = 1; i < segments.Count; i++)
        {
            var previous = segments[i - 1];
            var next = segments[i];
            stops.Add(Place(next.From) + " (chegada " + previous.Arrival + "h" + (previous.ArrivalDate != first.Date ? " em " + previous.ArrivalDate : "") + "; saída " + next.Departure + "h" + (next.Date != first.Date ? " em " + next.Date : "") + ")");
        }

        return new Flight
        {
            From = first.From,
            To = last.To,
            Date = first.Date,
            ArrivalDate = last.ArrivalDate,
            Departure = first.Departure,
            Arrival = last.Arrival,
            Airline = string.Join(" / ", segments.Select(flight => flight.Airline).Where(s => s != "").Distinct()),
            Connection = segments.Count == 1 ? "Voo direto" : (segments.Count - 1) + (segments.Count == 2 ? " conexão em " : " conexões em ") + string.Join(" e ", stops)
        };
    }

    static bool SameJourneyCity(string first, string second)
    {
        if (first == second) return true;
        string firstCity, secondCity;
        return Cities.TryGetValue(first, out firstCity) &&
            Cities.TryGetValue(second, out secondCity) &&
            !string.IsNullOrWhiteSpace(firstCity) && firstCity == secondCity;
    }

    /// <summary>Agrupa segmentos contínuos em somente ida ou em ida e volta.</summary>
    /// <remarks>A quantidade de conexões pode ser diferente em cada sentido.</remarks>
    public static List<Flight> Group(List<Flight> segments)
    {
        if (segments.Count == 0)
            throw new QuoteReadException("Nenhum trecho completo foi reconhecido. Preencha os campos manualmente.");
        if (segments.Any(flight => flight.From == "" || flight.To == "" || flight.From == flight.To))
            throw new QuoteReadException("Não foi possível confirmar os aeroportos de todos os trechos. Revise o print e preencha manualmente.");
        // Duas pernas inversas entre as mesmas cidades são ida e volta, mesmo
        // no mesmo dia e com aeroportos distintos (ex.: SDU-CGH / GRU-SDU).
        // Preserve os IATA reais; essa equivalência NÃO autoriza conexões com
        // troca de aeroporto dentro de um sentido nem calcula traslado terrestre.
        if (segments.Count == 2 &&
            SameJourneyCity(segments[0].From, segments[1].To) &&
            SameJourneyCity(segments[0].To, segments[1].From) &&
            !SameJourneyCity(segments[0].From, segments[0].To))
        {
            // Horários locais da mesma cidade de destino, não duração de voo.
            if (GetLocalDateTime(segments[1], false) < GetLocalDateTime(segments[0], true))
                throw new QuoteReadException("A volta sai antes da chegada da ida. Revise datas e horários.");
            return new List<Flight>
            {
                CombineSegments(segments.Take(1).ToList()),
                CombineSegments(segments.Skip(1).ToList())
            };
        }
        for (int i = 1; i < segments.Count; i++)
            if (segments[i - 1].To != segments[i].From)
                throw new QuoteReadException("Os aeroportos dos trechos não formam uma sequência contínua. Revise e preencha a ida e a volta manualmente.");
        if (segments.Count == 1)
            return new List<Flight>
            {
                CombineSegments(segments)
            };
        // A maior pausa em uma rota fechada é a candidata à estadia no destino.
        // Os limites abaixo são heurísticas conservadoras, não regras das companhias.
        var gaps = Enumerable.Range(1, segments.Count - 1).Select(i => new { Index = i, Hours = (GetLocalDateTime(segments[i], false) - GetLocalDateTime(segments[i - 1], true)).TotalHours }).OrderByDescending(g => g.Hours).ToList();
        if (gaps.Any(g => g.Hours < 0))
            throw new QuoteReadException("Horários de conexão inconsistentes. Revise os dados reconhecidos.");
        if (segments.Last().To == segments.First().From)
        {
            if (segments.Count > 2 && (gaps[0].Hours < MinimumStayHours || (gaps.Count > 1 && gaps[0].Hours - gaps[1].Hours < MinimumGapSeparationHours)))
                throw new QuoteReadException("A divisão entre ida e volta está ambígua. Preencha os dois trajetos manualmente.");
            int split = gaps[0].Index;
            return new List<Flight>
            {
                CombineSegments(segments.Take(split).ToList()),
                CombineSegments(segments.Skip(split).ToList())
            };
        }

        if (gaps[0].Hours >= LongStopHours)
            throw new QuoteReadException("Há uma parada longa ou múltiplos destinos. Revise a divisão dos trajetos manualmente.");
        return new List<Flight>
        {
            CombineSegments(segments)
        };
    }
}
