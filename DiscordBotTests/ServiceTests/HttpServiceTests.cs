using DiscordBot.Services;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace DiscordBotTests.ServiceTests;

[TestFixture]
public class HttpServiceTests
{
	[SetUp]
	public void SetUp()
	{
		_mockRestClient = new Mock<IRestClient>();
		_httpService = new HttpService(_mockRestClient.Object);
	}

	private Mock<IRestClient> _mockRestClient = null!;
	private HttpService _httpService = null!;

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
		Assert.That(result.IsSuccessStatusCode, Is.True);
		Assert.That(content, Is.EqualTo(result.Content));
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
		Assert.That(result.IsSuccessStatusCode, Is.False);
		Assert.That($"StatusCode: {response.StatusCode} | {errorMessage}", Is.EqualTo(result.Content));
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
			.Callback<RestRequest, CancellationToken>((req, _) =>
			{
				// Assert headers are set correctly within the Callback method.
				foreach (var header in headers)
					Assert.That(header.Value, Is.EqualTo(req.Parameters.FirstOrDefault(p => p.Name == header.Key)?.Value));
			})
			.ReturnsAsync(response);

		// Act
		var result = await _httpService.GetResponseFromUrl(resource, headers: headers);

		// Assert
		Assert.That(result.IsSuccessStatusCode, Is.True);
		Assert.That(response.Content, Is.EqualTo(result.Content));
	}

	[Test]
	public async Task GetResponseFromURL_WithJsonBody_SendsJsonBody()
	{
		// Arrange
		const string resource = "https://api.example.com/test";
		var jsonBody = JsonConvert.SerializeObject(new { key = "value" });
		var response = new RestResponse { StatusCode = HttpStatusCode.OK, IsSuccessStatusCode = true };
		RestRequest? capturedRequest = null;
		_mockRestClient.Setup(client => client.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.Callback<RestRequest, CancellationToken>((r, _) => capturedRequest = r).ReturnsAsync(response);

		// Act
		await _httpService.GetResponseFromUrl(resource, jsonBody: jsonBody);

		// Assert
		Assert.That(capturedRequest, Is.Not.Null);
		var requestBodyParameter = capturedRequest?.Parameters.FirstOrDefault(p =>
			p.Type.Equals(ParameterType.RequestBody) && p.ContentType.Equals("application/json"));
		Assert.That(requestBodyParameter, Is.Not.Null);
		Assert.That(jsonBody, Is.EqualTo(requestBodyParameter?.Value));
	}
}