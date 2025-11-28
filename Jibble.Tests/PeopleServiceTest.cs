namespace Jibble.Tests
{
    using System.Net;
    using Jibble.Services;
    using Microsoft.Extensions.Options;

    public class PeopleServiceTest
    {
        private const string ListSuccessJson = @"{
        ""@odata.context"": ""http://test.local/$metadata#People"",
        ""value"": [
            {""UserName"": ""scott"", ""FirstName"": ""Scott"", ""LastName"": ""Hanselman"", ""Emails"": [""test@example.com""]},
            {""UserName"": ""tim"", ""FirstName"": ""Tim"", ""LastName"": ""Corey"", ""Emails"": []}
        ]
    }";

        // Simplified JSON response for testing a successful detail fetch
        private const string DetailSuccessJson = @"{
        ""UserName"": ""scott"", ""FirstName"": ""Scott"", ""LastName"": ""Hanselman"", ""Emails"": [""test@example.com""]
    }";

        private readonly IOptions<ODataSettings> _mockOptions;

        public PeopleServiceTest()
        {
            var settings = new ODataSettings { BaseUrl = "http://test.local/" };
            this._mockOptions = Options.Create(settings);
        }

        [Fact]
        public async Task ListPeopleAsync_ReturnsCorrectData_OnSuccess()
        {
            var mockHandler = new MockHttpMessageHandler((request) =>
            {
                Assert.Equal("http://test.local/People", request.RequestUri.ToString());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ListSuccessJson),
                };
            });
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local/") };
            var service = new PeopleService(httpClient, this._mockOptions);

            // ACT
            var people = await service.ListPeopleAsync();

            Assert.Equal(2, people.Count);
            Assert.Equal("scott", people[0].UserName);
        }

        [Fact]
        public async Task ListPeopleAsync_ThrowsODataServiceException_OnApiError()
        {
            var mockHandler = new MockHttpMessageHandler((request) =>
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Server died."),
                };
            });
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local/") };
            var service = new PeopleService(httpClient, this._mockOptions);

            await Assert.ThrowsAsync<ODataServiceException>(() => service.ListPeopleAsync());
        }

        [Fact]
        public async Task GetPersonDetailsAsync_ConstructsCorrectUrl_AndReturnsPerson()
        {
            var key = "Scott";
            var expectedQuery = $"People('{key}')";

            var mockHandler = new MockHttpMessageHandler((request) =>
            {
                Assert.Equal($"http://test.local/{expectedQuery}", request.RequestUri.ToString());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(DetailSuccessJson),
                };
            });
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local/") };
            var service = new PeopleService(httpClient, this._mockOptions);

            var person = await service.GetPersonDetailsAsync(key);

            Assert.Equal("Scott", person.FirstName);
        }

        // This test ensures that the service uses the filter parameter correctly
        [Fact]
        public async Task ListPeopleAsync_AppliesFilterCorrectly_ToRequestUrl()
        {
            var testFilter = "FirstName eq 'Scott'";
            var expectedQuery = $"People?$filter={Uri.EscapeDataString(testFilter)}";

            var mockHandler = new MockHttpMessageHandler((request) =>
            {
                // ASSERTION POINT: Verify the full request URI
                Assert.EndsWith(expectedQuery, request.RequestUri.ToString());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ListSuccessJson),
                };
            });

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://test.local/") };
            var service = new PeopleService(httpClient, this._mockOptions);

            await service.ListPeopleAsync(testFilter);
        }
    }
}
