using System.Text.Json;
using RacingTelemetry.Models;

namespace RacingTelemetry.Services;

public class OpenF1Service
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    // HttpClient is injected — never create it with new HttpClient() directly
    // it causes socket exhaustion under load
    public OpenF1Service(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["OpenF1:BaseUrl"]!;
    }

    public async Task<JsonElement> GetAsync(string endpoint)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(content);
    }

    public async Task<JsonElement> GetDriverAsync(int driverNumber, int sessionKey)
    {
        var endpoint = $"/drivers?driver_number={driverNumber}&session_key={sessionKey}";
        return await GetAsync(endpoint);
    }

    public async Task<JsonElement> GetSessionAsync(int sessionKey)
    {
        var endpoint = $"/sessions?session_key={sessionKey}";
        return await GetAsync(endpoint);
    }

    public async Task<JsonElement> GetLapsAsync(int sessionKey, int driver_number)
    {
        var endpoint = $"/laps?session_key={sessionKey}&driver_number={driver_number}";
        return await GetAsync(endpoint);
    }

    public async Task<JsonElement> GetLocationAsync(int sessionKey, int driver_number, DateTime timestamp)
    {
        var before = timestamp.AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var after = timestamp.AddMinutes(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        var endpoint = $"/location?session_key={sessionKey}&driver_number={driver_number}&date>{before}&date<{after}";
        return await GetAsync(endpoint);
    }

    public async Task<JsonElement> GetCarDataAsync(int sessionKey, int driver_number, DateTime? dateStart)
    {

        var dateFilter = dateStart.HasValue ? $"&date>{dateStart.Value:yyyy-MM-ddTHH:mm:ss.fffZ}&date<{dateStart.Value.AddHours(1):yyyy-MM-ddTHH:mm:ss.fffZ}" : string.Empty;
        var endpoint = $"/car_data?session_key={sessionKey}&driver_number={driver_number}{dateFilter}";
        return await GetAsync(endpoint);
    }
}