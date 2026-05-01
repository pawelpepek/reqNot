using System.Text.Json;

namespace ReqNot.WebApi.Services;

public class DeviceChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DeviceChecker> _logger;

    public DeviceChecker(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<DeviceChecker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task<bool> CheckAsync()
    {
        var deviceAddress = _configuration["DeviceAddress"]
            ?? throw new InvalidOperationException("DeviceAddress is not configured.");

        using var client = GetClient();

        var url = $"{deviceAddress}/API/RunFunction?name=CurrentWorkParameters";
        _logger.LogInformation("Polling device at {Url}", url);

        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var values = root.TryGetProperty("Values", out var v) ? v : root.GetProperty("values");

        if (values.GetArrayLength() < 5)
            throw new InvalidOperationException($"Unexpected Values array length: {values.GetArrayLength()}");

        var statusValue = values[0].GetInt32();
        var value3 = values[3].GetDouble();
        var value4 = values[4].GetDouble();

        _logger.LogInformation("Device status: {Status}, value3: {V3}, value4: {V4}", statusValue, value3, value4);

        return statusValue == 1 && value3 >= 20 && value4 >= 20;
    }

    private HttpClient GetClient()
    {
        if (_environment.IsDevelopment())
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        }
        else
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            return client;
        }
    }
}
