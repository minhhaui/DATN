using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Web;

namespace API_Ver1
{
    //Để kiềm tra key
    public class ApiKeyHandler : DelegatingHandler
    {
        private const string API_KEY_NAME = "HAUI";
        private const string VALID_API_KEY = "MINHMAX";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Kiểm tra header chứa API key
            if (!request.Headers.TryGetValues(API_KEY_NAME, out var apiKeyValues))
            {
                return request.CreateResponse(HttpStatusCode.Unauthorized, "API Key is missing.");
            }

            var apiKey = apiKeyValues.First();

            if (apiKey != VALID_API_KEY)
            {
                return request.CreateResponse(HttpStatusCode.Forbidden, "Invalid API Key.");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}