// See https://aka.ms/new-console-template for more information


using System.Text.Json;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

var server = WireMockServer.Start(54174);
var responsePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Request","OAuthResponse.json");
var flightPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Request","UpdateFlightInofResponse.json");
var response = File.ReadAllText(responsePath);
var flightResponse = File.ReadAllText(flightPath);


server.Given(Request.Create()
        .WithPath($"/oauth2")
        .UsingPost()
        .WithHeader("content-type","application/x-www-form-urlencoded")
        .WithParam("grant_type", new ExactMatcher("client_credentials"))
        // .WithParam("grant_type","client_credentials")
        // .WithParam("client_id","zjykep8xv8qsv4apr3zyk8cupw92mvf65bs4n8wyx4f4ttxgjd")
        // .WithParam("client_secret","haCtZtkmGt2H3YhEA8D4UqtKVKXubxpXnkcbD9YE77ccksgKH4")
      
    )
    .RespondWith(Response.Create()
        .WithBody(response)
        .WithHeader("content-type", "application/x-www-form-urlencoded")
        .WithStatusCode(200));

server.Given(Request.Create()
        .WithPath($"/tst")
        // .WithHeader("X-version", "42")
        // .WithHeader("content-type","application/json")
        // .WithHeader("accept","*/*")
        // .WithHeader("accept-encoding","gzip, deflate, br")
         .WithBody(@"{""tst"":1}")
        .UsingPost())
    .RespondWith(Response.Create()
        .WithBody("Done")
        .WithStatusCode(200));

server.Given(Request.Create()
        .WithPath($"/updateactiveflightinformation")
        // .WithHeader("X-version", "42")
        // .WithHeader("content-type","application/json")
        // .WithHeader("accept","*/*")
        // .WithHeader("accept-encoding","gzip, deflate, br")
        .WithBody(new JsonMatcher(flightResponse))
        //  .WithBody(data =>
        // {
        //     var matecher=new JsonMatcher(MatchBehaviour.AcceptOnMatch).Value(data.BodyAsString)
        //     return data.BodyAsJson.Equals(JsonMatche);
        //
        // } )
        .UsingPost())
    .RespondWith(Response.Create()
        .WithBody("Done")
        .WithStatusCode(200));


server.Given(Request.Create()
        .WithPath($"/tstget")
        // .WithHeader("X-version", "42")
        // .WithHeader("content-type","application/json")
        // .WithHeader("accept","*/*")
        // .WithHeader("accept-encoding","gzip, deflate, br")
        // .WithBody(@"{""tst"":1}")
        .UsingGet())
    .RespondWith(Response.Create()
        .WithBody("Done")
        .WithStatusCode(200));



Console.ReadLine();
server.Dispose();

public class Req
{
    public string Type { get; set; }
}