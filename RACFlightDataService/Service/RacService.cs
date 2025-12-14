using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MAMQSqlExtension;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ORMContext;
using ORMContext.Domain;
using RACFlightDataService.Databases.Gaca;
using RACFlightDataService.Extensions;
using RACFlightDataService.HttpClients.Rac;
using RACFlightDataService.HttpClients.Rac.Requests;
using RACFlightDataService.Jobs;
using RACFlightDataService.Logging;
using RACFlightDataService.Options;

namespace RACFlightDataService.Service;

public interface IRacService
{
    [RetryInQueue("rac-flight")]
    Task PullFlightsFromGacaAsync(CancellationToken cancellationToken);
    [RetryInQueue("rac-flight")]
    Task TavAutoServiceProcess(CancellationToken cancellationToken);
    [RetryInQueue("rac-flight")]
    Task SentForRuh(CancellationToken cancellationToken);
}

public class RacService : IRacService
{
    private readonly ILoggerAdapter<RacService> _logger;
    private readonly IGacaRepository _gacaRepository;
    private readonly IAppDbContext _context;
    private readonly RacRestClient _httpClient;
    private readonly TavRestClient _tavRestClient;
    private readonly RACOptions _options;

    public RacService(
        ILoggerAdapter<RacService> logger,
        IGacaRepository gacaRepository,
        IAppDbContext context,
        RacRestClient httpClient,
        TavRestClient tavRestClient,
        IOptions<RACOptions> options
    )
    {
        _logger = logger;
        _gacaRepository = gacaRepository;
        _context = context;
        _httpClient = httpClient;
        _tavRestClient = tavRestClient;
        _options = options.Value;
    }
    [RetryInQueue("rac-flight")]
    public async Task PullFlightsFromGacaAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start pulling flight from Gaca");
            var dateTime = DateTime.Now.Date;
            var gacaFlights = await _gacaRepository.GetFlightsDataAsync(dateTime, cancellationToken);
            var flightAdded = 0;
            var flightEdited = 0;
            _logger.LogInformation("Found flights count:{flightCount} ", gacaFlights.Count());
            if (_options.EnableTest)
            {
                var flightsDateNo = _options.TestFlights.Split(',');
                var flightsNo = _options.TestFlightsNo.Split(',');
                if (_options.TestFlightWithDates && flightsDateNo.Length < 1)
                    return;
                if (!_options.TestFlightWithDates && flightsNo.Length < 1)
                    return;

                gacaFlights = _options.TestFlightWithDates
                    ? gacaFlights.Where(f => flightsDateNo.Contains($"{f.FlightNumber}:{f.OriginDate}")).ToList()
                    : gacaFlights
                        .Where(f => flightsNo.Contains($"{f.FlightNumber}")).ToList();
            }
            
            foreach (var gacaFlight in gacaFlights)
            {
                if (_options.ExcludedFlight.Select(n => n.ToString().Trim()).Contains(gacaFlight.FlightNumber))
                {
                    _logger.LogInformation("Can't pull changes on flight:{FlightNumber} because excluded ",gacaFlight.FlightNumber);
                    continue;
                }
                var flight = await _context.Flights
                    .Include(f => f.FlightHistories)
                    .FirstOrDefaultAsync(f => f.Id == gacaFlight.Id, cancellationToken);
                if (flight is null)
                {
                    var flightHistory = gacaFlight.Adapt<FlightHistory>();
                    flight = new Flight()
                    {
                        Id = gacaFlight.Id,
                        ArrivalAirport = gacaFlight.ArrivalAirport,
                        DepartureAirport = gacaFlight.DepartureAirport,
                        FlightNumber = gacaFlight.FlightNumber,
                        OriginDate = gacaFlight.OriginDate,
                    };
                    flight.FlightHistories.Add(flightHistory);
                    await _context.Flights.AddAsync(flight, cancellationToken);
                    flightAdded++;
                    continue;
                }

                var lastFlightHistory = flight.FlightHistories.OrderByDescending(h => h.CreatedAt);
                if (gacaFlight.CompareValues(lastFlightHistory.First()))
                {
                    _logger.LogInformation("No change happen on flight No :{flightNumber} ", flight.FlightNumber);

                    continue;
                }

                _logger.LogInformation("flight to modify :{flightNumber} ,", flight.FlightNumber);
                var updatedFlight = gacaFlight.Adapt<FlightHistory>();
                flight.FlightHistories.Add(updatedFlight);
                _context.Flights.Update(flight);
                flightEdited++;
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Flights added count:{flightCount} ", flightAdded);
            _logger.LogInformation("Flights edited count:{flightCount} ", flightEdited);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while trying to sync with GACA database");
        }
        // finally
        // {
        //     await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
        //     BackgroundJob.Enqueue(() => PullFlightsFromGacaAsync(cancellationToken));
        // }
    }
    [RetryInQueue("rac-flight")]
    public async Task TavAutoServiceProcess(CancellationToken cancellationToken)
    {
        try
        {
            //Add EnableTAVUpdate
            if(_options.EnableTAVUpdate)
            await _tavRestClient.UpdateTavAutoService(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "failed to update tab auto service ...");
        }
        // finally
        // {
        //     await Task.Delay(TimeSpan.FromMinutes(20), cancellationToken);
        //     BackgroundJob.Enqueue(() => TavAutoServiceProcess(cancellationToken));
        // }
    }
    [RetryInQueue("rac-flight")]
    public async Task SentForRuh(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start post updates to RUH Airport");
            // var response =
            //     await _httpClient.UpdateFlightAsync(new FlightHistory(){}.Adapt<RacFlightRequest>(), cancellationToken);
            //
            if (!_options.EnablePushToRac)
            {
                _logger.LogInformation("push to rac api is disabled");
                return;
            }
            var query = _context.FlightHistories.Where(fh =>fh.FlightDate >= DateTime.Now.Date);
            var flightsNotBeenSent = await query.ToListAsync(cancellationToken);
            if (_options.EnableTest)
            {
                var flightsDateNo = _options.TestFlights.Split(',');
                var flightsNo = _options.TestFlightsNo.Split(',');
                if (_options.TestFlightWithDates && flightsDateNo.Length < 1)
                    return;
                if (!_options.TestFlightWithDates && flightsNo.Length < 1)
                    return;

                flightsNotBeenSent = _options.TestFlightWithDates
                    ? flightsNotBeenSent.Where(f => flightsDateNo.Contains($"{f.FlightNumber}:{f.OriginDate}")).ToList()
                    : flightsNotBeenSent
                        .Where(f => flightsNo.Contains($"{f.FlightNumber}")).ToList();
            }

            var flights = flightsNotBeenSent.GroupBy(f => f.FlightId).Select(s=>s.MaxBy(f=>f.CreatedAt)).ToArray();

            _logger.LogInformation("Flight to post to RUH is {flights}",string.Join(',' ,flights.Select(s=>s.FlightNumber)));
            foreach (var flightHistory in flights)
            {
                if(flightHistory is null)
                    continue;
                if(flightHistory.Sent)
                    continue;
                if (_options.ExcludedFlight.Select(n => n.ToString().Trim()).Contains(flightHistory.FlightNumber))
                {
                    _logger.LogInformation("Can't post changes on flight:{FlightNumber} because excluded ",flightHistory.FlightNumber);
                    continue;
                }
                _logger.LogInformation("try to send the new updates for flight no:{flightNumber}",
                    flightHistory.FlightNumber);
                var response =
                    await _httpClient.UpdateFlightAsync(flightHistory.Adapt<RacFlightRequest>(), cancellationToken);
                if (response.IsFaulted)
                {
                    _logger.LogError(response.GetException(),
                        "Failed to post flight no: {flightNumber} for date {date}",
                        flightHistory.FlightNumber, flightHistory.OriginDate);
                    continue;
                }

                flightHistory.Sent = true;
                flightHistory.SentAt = DateTime.Now;
                _context.FlightHistories.Update(flightHistory);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Flight no {flightNumber} for date {date} ,Registration: {Registration} , from:{DepartureAirport} , to:{ArrivalAirport} , std:{ScheduledDeparture} , sta:{ScheduledArrival} posted successfully",
                    flightHistory.FlightNumber, flightHistory.OriginDate, flightHistory.Registration,
                    flightHistory.DepartureAirport, flightHistory.ArrivalAirport, flightHistory.ScheduledDeparture,
                    flightHistory.ScheduledArrival);
            }

            _logger.LogInformation("End post updates to RUH Airport");
            flightsNotBeenSent = null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "failed to post flights to rac ...");
        }
        // finally
        // {
        //     await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        //     BackgroundJob.Enqueue(() => SentForRuh(cancellationToken));
        // }
    }
}