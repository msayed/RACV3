using RestSharp;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace RACFlightDataService.Tests;

public class RacFakeApi:IDisposable
{
    private WireMockServer _server;

    public string Url => _server.Url!;

    public void Start()
    {
        _server = WireMockServer.Start();
    }
    
    public void SetupOAuthApi()
    {
        var responsePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Responses","OAuthResponse.json");
        var flightResponsePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Request","UpdateFlightInofReqest.json");
        var flightResponse = File.ReadAllText(flightResponsePath);
        var response = File.ReadAllText(responsePath);
        _server.Given(Request.Create()
                .WithPath($"/oauth2")
                .WithHeader("content-type","application/x-www-form-urlencoded")
                // .WithParam("grant_type","client_credentials")
                // .WithParam("client_id","zjykep8xv8qsv4apr3zyk8cupw92mvf65bs4n8wyx4f4ttxgjd")
                // .WithParam("client_secret","haCtZtkmGt2H3YhEA8D4UqtKVKXubxpXnkcbD9YE77ccksgKH4")
                // .WithParam("application/x-www-form-urlencoded","grant_type=client_credentials&client_id=zjykep8xv8qsv4apr3zyk8cupw92mvf65bs4n8wyx4f4ttxgjd&client_secret=haCtZtkmGt2H3YhEA8D4UqtKVKXubxpXnkcbD9YE77ccksgKH4")
                // .WithBody()
                .UsingPost())
            .RespondWith(Response.Create()
                .WithBody(response)
                .WithHeader("content-type", "application/x-www-form-urlencoded")
                .WithStatusCode(200));       
        

        _server.Given(Request.Create()
                .WithPath($"/updateactiveflightinformation")
                .WithBody(new JsonPartialMatcher(flightResponse))
                .UsingPost())
            .RespondWith(Response.Create()
                .WithBody("Done")
                .WithStatusCode(200));
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}