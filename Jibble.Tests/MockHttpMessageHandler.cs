namespace Jibble.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
        {
            this._handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = this._handlerFunc(request);
            return Task.FromResult(response);
        }
    }
}
