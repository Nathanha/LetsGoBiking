using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

namespace WebProxyService
{
    public class WebProxyService : IWebProxyService
    {
        private HttpClient client;
        private Cache<List<Station>> stationsCache;
        private Cache<Station> stationCache;

        WebProxyService()
        {
            client = new HttpClient();
            stationsCache = new Cache<List<Station>>(60000);
            stationCache = new Cache<Station>();
        }

        /* Method to get a specific station from its ID and its contract */
        public Station GetStation(int station_id, string contract)
        {
            string url = "https://api.jcdecaux.com/vls/v3/stations/"+ 
                         station_id  + "?apiKey=67b2054c6e21e957152012426c86ed264fb1745c&contract=" + contract;

            if (stationCache.Get(url) == null)
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    stationCache.Set(url, JsonSerializer.Deserialize<Station>(responseBody));
                }
                else
                {
                    return null;
                }
            }

            return stationCache.Get(url);
        }

        /* Method to get the list of station for a specific contract */
        public List<Station> GetStationsForContract(string contract)
        {

            string url = "https://api.jcdecaux.com/vls/v3/stations?apiKey=67b2054c6e21e957152012426c86ed264fb1745c&contract=" + contract;

            if (stationsCache.Get(url) == null)
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    stationsCache.Set(url, JsonSerializer.Deserialize<List<Station>>(responseBody));
                } else
                {
                    return null;
                }
            }

            return stationsCache.Get(url);
        }

    }
}
