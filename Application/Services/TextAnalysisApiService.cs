using System.Text;
using System.Text.Json;
using ValuatorLib.Requests;
using ValuatorLib.Responses;

namespace Application.Services;

public class TextAnalysisApiService(HttpClient httpClient, ILogger<TextAnalysisApiService> logger)
    : ITextAnalysisApiService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<ProcessTextResponse?> ProcessTextAsync(ProcessTextRequest request)
    {
        try
        {
            string json = JsonSerializer.Serialize(request, _jsonOptions);
            StringContent content = new(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("/api/textanalysis", content);

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProcessTextResponse>(responseJson, _jsonOptions);
            }

            logger.LogWarning("API call failed with status {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling ProcessText API");
            return null;
        }
    }

    public async Task<TextAnalysisResponse?> GetAnalysisAsync(Guid id)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"/api/textanalysis/{id}");

            if (response.IsSuccessStatusCode)
            {
                string responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TextAnalysisResponse>(responseJson, _jsonOptions);
            }

            logger.LogWarning("API call failed with status {StatusCode} for ID {Id}", response.StatusCode, id);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling GetAnalysis API for ID {Id}", id);
            return null;
        }
    }
}