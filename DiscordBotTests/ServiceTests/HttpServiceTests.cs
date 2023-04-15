using System.Net;
using DiscordBot.Interfaces;
using DiscordBot.Services;
using Moq;
using Newtonsoft.Json;
using RestSharp;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class HttpServiceTests
{
    private Mock<IRestClient> _mockRestClient;
    private IHttpService _httpService;

    [SetUp]
    public void SetUp()
    {
        _mockRestClient = new Mock<IRestClient>();
        _httpService = new HttpService(_mockRestClient.Object);
    }

    [Test]
    public async Task GetResponseFromURL_WithValidResponse_ReturnsSuccess()
    {
        // Arrange
        const string resource = "https://api.example.com/test";
        const string content = "response content";
        var response = new RestResponse
            { StatusCode = HttpStatusCode.OK, Content = content, IsSuccessStatusCode = true };
        _mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), default)).ReturnsAsync(response);

        // Act
        var result = await _httpService.GetResponseFromUrl(resource);

        // Assert
        Assert.IsTrue(result.IsSuccessStatusCode);
        Assert.AreEqual(content, result.Content);
    }

    [Test]
    public async Task GetResponseFromURL_WithErrorResponse_ReturnsError()
    {
        // Arrange
        const string resource = "https://api.example.com/test";
        const string errorMessage = "Error message";
        var response = new RestResponse { StatusCode = HttpStatusCode.BadRequest, ErrorMessage = errorMessage };
        _mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), default)).ReturnsAsync(response);

        // Act
        var result = await _httpService.GetResponseFromUrl(resource, errorMessage: errorMessage);

        // Assert
        Assert.IsFalse(result.IsSuccessStatusCode);
        Assert.AreEqual($"StatusCode: {response.StatusCode} | {errorMessage}", result.Content);
    }

    [Test]
    public async Task GetResponseFromURL_WithHeaders_SendsHeaders()
    {
        // Arrange
        const string resource = "https://api.example.com/test";
        var headers = new List<KeyValuePair<string, string>>
        {
            new("Header1", "Value1"),
            new("Header2", "Value2")
        };
        var response = new RestResponse { StatusCode = HttpStatusCode.OK, Content = "OK", IsSuccessStatusCode = true };

        _mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), default))
            .Callback<RestRequest, CancellationToken>((req, token) =>
            {
                // Assert headers are set correctly within the Callback method.
                foreach (var header in headers)
                    Assert.AreEqual(header.Value, req.Parameters.FirstOrDefault(p => p.Name == header.Key)?.Value);
            })
            .ReturnsAsync(response);

        // Act
        var result = await _httpService.GetResponseFromUrl(resource, headers: headers);

        // Assert
        Assert.IsTrue(result.IsSuccessStatusCode);
        Assert.AreEqual(response.Content, result.Content);
    }


    [Test]
    public async Task GetResponseFromURL_WithJsonBody_SendsJsonBody()
    {
        // Arrange
        const string resource = "https://api.example.com/test";
        var jsonBodyString = JsonConvert.SerializeObject(new { key = "value" });
        var response = new RestResponse { StatusCode = HttpStatusCode.OK, IsSuccessStatusCode = true };
        RestRequest? capturedRequest = null;
        _mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
            .Callback<RestRequest, CancellationToken>((r, _) => capturedRequest = r).ReturnsAsync(response);

        // Act
        await _httpService.GetResponseFromUrl(resource, jsonBodyString: jsonBodyString);

        // Assert
        Assert.IsNotNull(capturedRequest);
        var requestBodyParameter = capturedRequest?.Parameters.FirstOrDefault(p =>
            p.Type.Equals(ParameterType.RequestBody) && p.ContentType.Equals("application/json"));
        Assert.IsNotNull(requestBodyParameter);
        Assert.AreEqual(jsonBodyString, JsonConvert.SerializeObject(requestBodyParameter?.Value));
    }
}