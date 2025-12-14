namespace RACFlightDataService.Options {
  public class RACOptions {
    public int SendingIntervalInMinutes { get; set; }
    public int MaxFlightsPerMinute { get; set; }
    public string BaseUrl { get; set; }
    public string FlightInfoApiEndPoint { get; set; }
    public string AuthApiEndPoint { get; set; }
    public string AuthClientId { get; set; }
    public string AuthClientSecret { get; set; }
    public string ApiKey { get; set; }
    public string TestFlights { get; set; }
    public bool EnableTest { get; set; } = false;
    public string TavAutoProcessUrl { get; set; }

    public bool EnableTAVUpdate { get; set; } = false;

    public bool TestFlightWithDates { get; set; }
    public string TestFlightsNo { get; set; }
    public bool EnablePushToRac { get; set; }
    public int[] ExcludedFlight { get; set; }
  }
}