using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace RACFlightDataService.Functions.Options;

public class JobOptions
{
    [Required]
    [ConfigurationKeyName("JobSchedule")]
    public string JobSchedule { get; set; } = string.Empty;

    [ConfigurationKeyName("Job:Enabled")]
    public bool Enabled { get; set; }
}
