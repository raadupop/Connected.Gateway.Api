using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Connected.Gateway.Api.Helpers;
using Microsoft.AspNetCore.Http;

namespace Connected.Gateway.Api
{
    public class Router
    {
        public List<Route> Routes { get; set; }

        public Destination AuthenticationService { get; set; }


        public Router(string routeConfigurationPath)
        {
            dynamic router = JsonLoader.LoadFromFile<dynamic>(routeConfigurationPath);

            Routes = JsonLoader.Deserializer<List<Route>>(Convert.ToString(router.routes));
            AuthenticationService = JsonLoader.Deserializer<Destination>(Convert.ToString(router.authenticationService));
        }

        public async Task<HttpResponseMessage> RouteRequest(HttpRequest request)
        {
            var path = request.Path.ToString();
            var basePath = '/' + path.Split('/')[1];

            Destination destination;
            try
            {
                destination = Routes.First(r => r.Endpoint.Equals(basePath)).Destination;
            }
            catch
            {
                return ConstructErrorMessage("The path could not be found.");
            }


            if (destination.RequestAuthentication)
            {
                HttpResponseMessage authRespone = await AuthenticationService.SendRequest(request);

                if (!authRespone.IsSuccessStatusCode)
                {
                    return ConstructErrorMessage("Authentication failed.");
                }
            }

            return await destination.SendRequest(request);
        }

        private static HttpResponseMessage ConstructErrorMessage(string error)
        {
            HttpResponseMessage errorMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(error)
            };

            return errorMessage;
        }
    }
}
