using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ecos.Services
{
    public class OpenAIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _apiKey = configuration["OpenAI:ApiKey"];
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
                        
                        messages = new[]
                        {
                    new { role = "user", content = prompt }
                },
                        max_tokens = 100
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

                    // Set Authorization header for every request
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

                    // Send request
                    var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        try
                        {
                            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                            // Check if jsonResponse and choices are valid
                            if (jsonResponse != null && jsonResponse.choices != null && jsonResponse.choices.Count > 0)
                            {
                                return jsonResponse.choices[0].message.content.ToString();
                            }
                            else
                            {
                                return "No insights available at this time.";
                            }
                        }
                        catch (Exception ex)
                        {
                            // Catch errors in deserialization or if the response structure is unexpected
                            return $"Error parsing OpenAI response: {ex.Message}";
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error: {response.StatusCode} - {errorContent}");

                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return "The OpenAI API endpoint could not be found. Please check the URL and try again." + $"Error: {response.StatusCode} - {errorContent}";
                        }
                    }
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (i == maxRetries - 1)
                        return "Request rate limit exceeded. Please try again later.";

                    await Task.Delay(delay);
                    delay *= 2;
                }
                catch (Exception ex)
                {
                    // General exception handling for other errors
                    return $"An error occurred while retrieving insights: {ex.Message}";
                }
            }

            return "Unable to retrieve insights at this time. Please try again later.";
        }

    }
}
