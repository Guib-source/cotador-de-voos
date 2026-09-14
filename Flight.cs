

// Dados locais impressos: nenhuma conversão de fuso horário é aplicada.
public class Flight
{
    public System.Collections.Generic.List<FlightNotice> Notices = new System.Collections.Generic.List<FlightNotice>();
    public string From = "";
    public string To = "";
    public string Date = "";
    public string ArrivalDate = "";
    public string Departure = "";
    public string Arrival = "";
    public string Airline = "";
    public string Connection = "";
}

public class FlightNotice
{
    public int Column;
    public string Message;
    public string Value;
}
