using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ecos.Services
{
    public class CohereService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public CohereService(IConfiguration configuration, HttpClient httpClient)
        {
            _apiKey = configuration["Cohere:ApiKey"];
            _httpClient = httpClient;
        }

        public async Task<string> GetElectricityInsights(string prompt)
        {
            int maxRetries = 3;
            int delay = 1000;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var requestContent = new
                    {
                        model = "command",
                        prompt = prompt,
                        max_tokens = 100,
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                    // Cohere's endpoint for generating text completions
                    var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/generate", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        try
                        {
                            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                            if (jsonResponse != null && jsonResponse.generations != null && jsonResponse.generations.Count > 0)
                            {
                                return jsonResponse.generations[0].text.ToString();
                            }
                            else
                            {
                                return "No insights available at this time.";
                            }
                        }
                        catch (Exception ex)
                        {
                            return $"Error parsing Cohere response: {ex.Message}";
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error: {response.StatusCode} - {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    return $"An error occurred: {ex.Message}";
                }
            }

            return "Unable to retrieve insights at this time. Please try again later.";
        }
    }
}
