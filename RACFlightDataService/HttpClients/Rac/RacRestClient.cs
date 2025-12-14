using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RACFlightDataService.HttpClients.Base;
using RACFlightDataService.HttpClients.Rac.Requests;
using RACFlightDataService.Logging;
using RACFlightDataService.Options;
using RestSharp;

namespace RACFlightDataService.HttpClients.Rac;

public class RacRestClient : BaseRestClient<RacRestClient>
{
    private readonly IRacAuth _auth;
    private readonly RACOptions _options;

    public RacRestClient(IOptions<RACOptions> options, ILoggerAdapter<RacRestClient> logger, IRacAuth auth) : base(
        options.Value.BaseUrl, logger)
    {
        _auth = auth;
        _options = options.Value;

        //ByPass SSL Validation RAcRest Client 
        RemoteCertificateValidationCallback= (sender, certificate, chain, sslPolicyErrors) => true;
    }



    public async Task<Result<IRestResponse>> UpdateFlightAsync(RacFlightRequest body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenResult = await _auth.GetAuthTokenAsync(this, cancellationToken);
            if (tokenResult.IsFaulted)
            {
                var exp = new Result<IRestResponse>(tokenResult.Match(f => null, e => e));
                return exp;
            }

            //ByPass SSL Validation
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var token = tokenResult.Match(t => t,e=>null);
            var request = CreateFlightRequest(body, token);
            var response = await this.ExecuteWithLog(request, cancellationToken);
            if(!response.IsSuccessful)
            {
                var exp = new Exception(response.ErrorMessage);
                return new Result<IRestResponse>(exp);
            }
            return  (RestResponse)response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed get response from rac");
            return new Result<IRestResponse>(e);
        }
    }


    private RestRequest CreateFlightRequest(RacFlightRequest body, RacToken token)
    {
        var transactionId = Guid.NewGuid().ToString();
        string jsonBody = JsonConvert.SerializeObject(body,new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        jsonBody = jsonBody.Replace("cargoDetails", "CargoDetails");
        var request = new RestRequest(_options.FlightInfoApiEndPoint, Method.POST,DataFormat.Json);
        request.AddHeader("Authorization", "Bearer " + token.AuthToken);
        request.AddHeader("transaction-id", transactionId);
        request.AddHeader("Originator", "FLYNAS_API");
        request.AddHeader("Content-Type", "application/json");
         request.RequestFormat = DataFormat.Json;
         // var bdy = @"{""internationalStatus"":""International"",""flightNumber"":""333"",""flightSuffix"":""F"",""departureAirport"":""RUH"",""arrivalAirport"":""LKO"",""originDate"":""2022-09-19"",""scheduledDeparture"":""2022-09-19T21:30:00"",""estimatedDeparture"":""2022-09-19T21:30:00"",""scheduledArrival"":""2022-09-20T02:30:00"",""estimatedArrival"":""2022-09-20T02:30:00"",""airlineCode"":""XY"",""airlineICAOCode"":""XY"",""offBlock"":""2022-09-19T02:30:00"",""onBlock"":""2022-09-19T02:30:00"",""aircraftType"":""Airbus 320-251"",""takeOff"":""2022-09-19T02:30:00"",""touchDown"":""2022-09-19T02:30:00"",""codeShareInfo"":null,""firstBag"":null,""lastBag"":null,""numberOfPassengers"":""0"",""aircraftSubType"":""320"",""registration"":""HZNS35"",""callSign"":""flynas"",""fleetIdentifier"":""Airbus 320-251"",""dayOfWeek"":""Monday"",""serviceType"":""J"",""flightType"":""Passenger"",""flighStatus"":null,""adultCount"":""0"",""childCount"":""0"",""crewCount"":""7"",""totalPlannedPaxCount"":""0"",""transitPaxCount"":""0"",""localPaxCount"":""0"",""transferPaxCount"":""0"",""seatCapacity"":""147"",""takeOffFromOutStation"":null,""actualStartBoaringTime"":null,""calculatedTakeOffTime"":null,""lastActionTime"":""2022-09-18T13:51:08"",""lastActionCode"":""Add"",""clientUniqueKey"":""493094"",""cargoDetails"":null,""agentInfo"":""SGS""}";
         // request.AddJsonBody(JsonConvert.SerializeObject(foo));
         request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        return request;
    }
}