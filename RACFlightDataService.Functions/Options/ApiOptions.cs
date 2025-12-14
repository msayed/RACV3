using System.ComponentModel.DataAnnotations;

namespace RACFlightDataService.Functions.Options;

public class ApiOptions
{
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    public string? ApiKey { get; set; }

    public Dictionary<string, string>? DefaultHeaders { get; set; }
}
