namespace Jibble.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Jibble.Interfaces;
    using Jibble.Models;
    using Microsoft.Extensions.Options;

    public class PeopleService : IPeopleService
    {
        private readonly HttpClient _httpClient;
        private readonly ODataSettings _settings;

        public PeopleService(HttpClient httpClient, IOptions<ODataSettings> options)
        {
            this._httpClient = httpClient;
            this._settings = options.Value;
        }

        public async Task<IReadOnlyList<Person>> ListPeopleAsync(string filter = null)
        {
            var query = "People";
            if (!string.IsNullOrWhiteSpace(filter))
            {
                // Apply $filter to the query, ensuring proper URI encoding
                query += $"?$filter={Uri.EscapeDataString(filter)}";
            }

            try
            {
                var response = await this._httpClient.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                {
                    // Handle 404/400/500 errors gracefully
                    throw new ODataServiceException($"API call failed ({response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
                }

                var odataResponse = await response.Content.ReadFromJsonAsync<ODataResponse<List<Person>>>();

                return odataResponse?.Value ?? new List<Person>();
            }
            catch (HttpRequestException ex)
            {
                throw new ODataServiceException("A network error occurred while communicating with the OData service.", ex);
            }
            catch (JsonException ex)
            {
                throw new ODataServiceException("Failed to parse the response from the OData service.", ex);
            }
        }

        public async Task<Person> GetPersonDetailsAsync(string key)
        {

            var query = $"People('{Uri.EscapeDataString(key)}')";

            try
            {
                var response = await this._httpClient.GetAsync(query);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ODataServiceException($"Person with key '{key}' not found.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new ODataServiceException($"API call failed ({response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
                }

                var person = await response.Content.ReadFromJsonAsync<Person>();

                return person ?? throw new ODataServiceException("Details response was empty or malformed.");
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                throw new ODataServiceException($"Failed to get details for '{key}'.", ex);
            }
        }
    }
}
