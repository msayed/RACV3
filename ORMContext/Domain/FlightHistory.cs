namespace ORMContext.Domain;

public class FlightHistory
{
    public string Id { get; set; }

    public string InternationalStatus { get; set; }
    public string FlightNumber { get; set; }
    public string FlightSuffix { get; set; }
    public string DepartureAirport { get; set; }
    public string ArrivalAirport { get; set; }
    public DateTime FlightDate { get; set; }
    public string OriginDate { get; set; }
    public DateTime ScheduledDeparture { get; set; }
    public DateTime EstimatedDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public DateTime EstimatedArrival { get; set; }
    public string AirlineCode { get; set; }
    public string AirlineICAOCode { get; set; }
    public DateTime OffBlock { get; set; }
    public DateTime OnBlock { get; set; }
    public string AircraftType { get; set; }
    public DateTime TakeOff { get; set; }
    public DateTime TouchDown { get; set; }
    public string CodeShareInfo { get; set; }
    public string FirstBag { get; set; }
    public string LastBag { get; set; }
    public string NumberOfPassengers { get; set; }
    public string AircraftSubType { get; set; }
    public string Registration { get; set; }
    public string CallSign { get; set; }
    public string FleetIdentifier { get; set; }
    public string DayOfWeek { get; set; }
    public string ServiceType { get; set; }
    public string FlightType { get; set; }
    public string FlighStatus { get; set; }
    public string AdultCount { get; set; }
    public string ChildCount { get; set; }
    public string CrewCount { get; set; }
    public string TotalPlannedPaxCount { get; set; }
    public string TransitPaxCount { get; set; }
    public string LocalPaxCount { get; set; }
    public string TransferPaxCount { get; set; }
    public string SeatCapacity { get; set; }
    public string TakeOffFromOutStation { get; set; }
    public string ActualStartBoaringTime { get; set; }
    public string CalculatedTakeOffTime { get; set; }
    public DateTime LastActionTime { get; set; }
    public string LastActionCode { get; set; }
    public string ClientUniqueKey { get; set; }
    public string CargoDetails { get; set; }
    public string AgentInfo { get; set; }
    public int BookedSeats { get; set; }
    public long FlightId { get; set; }
    public Flight Flight { get; set; }
    
    public bool Sent { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime CreatedAt { get; set; }=DateTime.Now;
    public DateTime UpdateAt { get; set; }
}