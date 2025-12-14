
using System.Dynamic;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using RACFlightDataService.HttpClients.Rac;
using RACFlightDataService.HttpClients.Rac.Requests;
using RACFlightDataService.Logging;
using RACFlightDataService.Options;
using RestSharp;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RACFlightDataService.Tests;

public class Base
{
    private readonly ILogger _logger = Substitute.For<ILogger>(); 
    public Base()
    {
        HttpClientExt.Logger = _logger;
    }
}
public class RacOAuthApiTests:IDisposable
{
    private readonly RacRestClient _sut;
    private readonly TavRestClient _tavClient;
    private readonly RacAuth _auth;
    private readonly ILoggerAdapter<RacRestClient> _logger = Substitute.For<ILoggerAdapter<RacRestClient>>();
    private readonly ILoggerAdapter<TavRestClient> _tavLogger = Substitute.For<ILoggerAdapter<TavRestClient>>();
    private readonly ILoggerAdapter<RacAuth> _authLogger = Substitute.For<ILoggerAdapter<RacAuth>>();
    private readonly IOptions<RACOptions> _options = Substitute.For<IOptions<RACOptions>>();
    private RacFakeApi fakeServer;
    public RacOAuthApiTests()
    {
        fakeServer = new RacFakeApi();
        fakeServer.Start();
        _options.Value.Returns(new RACOptions()
        {
            BaseUrl =fakeServer.Url ,
            FlightInfoApiEndPoint = "/updateactiveflightinformation",
            AuthApiEndPoint = "/updateactiveflightinformation/auth/gettoken",
            ApiKey="ex7jh3t32388p8m24unpjvt7axyz6b9xg7qynerg8hcbdrbmh4",
            AuthClientId="zjykep8xv8qsv4apr3zyk8cupw92mvf65bs4n8wyx4f4ttxgjd",
            AuthClientSecret="t6gbQE6cTHwQZGW4dKWeBWSsa5zdUg8wV5QD35G5cgDNpt5bVv",
            TavAutoProcessUrl = "https://www.google.com/"
        } );
        _auth = new RacAuth(_options, _authLogger);
        _sut = new RacRestClient(_options, _logger,_auth);
        _tavClient = new TavRestClient(_options, _tavLogger);
    }
    [Fact]
    public async Task Test1()
    {
        fakeServer.SetupOAuthApi();
        var authResult= await _auth.GetAuthTokenAsync(_sut, default);
        authResult.IsSuccess.Should().Be(true);
    }    
    [Fact]
    public async Task Test2()
    {        var flightResponsePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Request","UpdateFlightInofReqest.json");
        var flightResponse = File.ReadAllText(flightResponsePath);
        var request = JsonSerializer.Deserialize<RacFlightRequest>(flightResponse,new JsonSerializerOptions(JsonSerializerDefaults.Web));
        fakeServer.SetupOAuthApi();
        var authResult= await _sut.UpdateFlightAsync(request, default);
        authResult.IsSuccess.Should().Be(true);
    }    
    [Fact]
    public async Task Serialize()
    {
        var request = new Faker<RacFlightRequest>().Generate();
        var result = JsonConvert.SerializeObject(request,new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        var foo = JsonConvert.DeserializeObject<ExpandoObject>(result);
        var jsonResult = JsonSerializer.Serialize(foo);
        var obj =  JToken.Parse(result);;
        var racRequest = CreateFlightRequest(request, new RacToken("adfasdf", DateTime.Now.AddDays(1)));

        
    }   
    
    [Fact]
    public async Task check_get_method()
    {
        int c=Convert.ToInt32( TimeSpan.FromMinutes(10).TotalMilliseconds);
       var response= await _tavClient.UpdateTavAutoService();
    }
    
    
    
    private RestRequest CreateFlightRequest(RacFlightRequest body, RacToken token)
    {
        var transactionId = Guid.NewGuid().ToString();
        string jsonBody = JsonConvert.SerializeObject(body,new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        var request = new RestRequest(fakeServer.Url, Method.POST);
        request.AddHeader("Authorization", "Bearer " + token.AuthToken);
        request.AddHeader("transaction-id", transactionId);
        request.AddHeader("Originator", "FLYNAS_API");
        request.AddHeader("Content-Type", "application/json");
        // request.RequestFormat = DataFormat.Json;
        // request.AddJsonBody(body);
        request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
        return request;
    }

    public void Dispose()
    {
        fakeServer.Dispose();
    }
}