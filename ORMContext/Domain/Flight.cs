using System.ComponentModel.DataAnnotations.Schema;

namespace ORMContext.Domain;

public class Flight
{
    
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    public string OriginDate { get; set; }
    public string FlightNumber { get; set; }
    public string DepartureAirport { get; set; }
    public string ArrivalAirport { get; set; }
    public HashSet<FlightHistory> FlightHistories { get; set; } = new HashSet<FlightHistory>();
}