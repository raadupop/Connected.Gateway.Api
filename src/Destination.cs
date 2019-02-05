using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Connected.Gateway.Api
{
    public class Destination
    {
        public string Uri { get; set; }

        public bool RequestAuthentication { get; set; }

        static HttpClient client = new HttpClient();

        public Destination(string uri, bool requestAuthentication)
        {
            Uri = uri;
            RequestAuthentication = requestAuthentication;
        }

        public Destination(string path) : this(path, false)
        {
        }

        private Destination()
        {
            Uri = "/";
            RequestAuthentication = false;
        }

        public async Task<HttpResponseMessage> SendRequest(HttpRequest request)
        {
            string requstContent;
            using (var reviceStream = request.Body)
            {
                using (var readStream = new StreamReader(reviceStream, Encoding.UTF8))
                {
                    requstContent = readStream.ReadToEnd();
                }
            }

            using (var newRequest =
                new HttpRequestMessage(new HttpMethod(request.Method), CreateDestinationUri(request)))
            {
                newRequest.Content = new StringContent(requstContent, Encoding.UTF8, request.ContentType);
                using (var response = await client.SendAsync(newRequest))
                {
                    return response;
                }
            }
        }

        private string CreateDestinationUri(HttpRequest request)
        {
            var requestPath = request.Path.ToString();
            var queryString = request.QueryString.ToString();

            var endpoint = string.Empty;
            var endpointSplit = requestPath.Substring(1).Split("/");

            if (endpointSplit.Length > 1)
            {
                endpoint = endpointSplit[1];
            }

            return Uri = endpoint + queryString;
        }
    }
}
