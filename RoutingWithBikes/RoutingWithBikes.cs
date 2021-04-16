using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.ServiceModel.Web;
using System.Text.Json;

namespace RoutingWithBikes
{
    public class RoutingWithBikes : IRoutingWithBikes
    {

        private HttpClient client;
        private ObjectCache cache;
        private CacheItemPolicy cacheItemPolicy;
        private WebProxyService.IWebProxyService webProxyService;
        private StationLog stationLog;

        RoutingWithBikes()
        {
            webProxyService = new WebProxyService.WebProxyServiceClient();
            client = new HttpClient();
            cache = MemoryCache.Default;
            stationLog = StationLog.GetInstance();
            cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60),
            };
        }
        
        /* Method to a list of routing between two addresses */
        public List<Routing> GetItineraryBetweenTwoAddress(string startingAddress, string destination)
        {
            Point startingAddressPoint = getCoordinatesFromAddress(startingAddress);
            Point destinationPoint = getCoordinatesFromAddress(destination);

            List<Routing> routings = new List<Routing>();

            if (startingAddressPoint != null && destinationPoint != null)
            {
                WebProxyService.Station[] stations = webProxyService.GetStationsForContract(startingAddressPoint.city);

                Itinerary footItinerary = getFootItinerary(startingAddressPoint, destinationPoint);

                if (stations != null && stations.Length > 0)
                {
                    GeoCoordinate startPointCoord = new GeoCoordinate(Convert.ToDouble(startingAddressPoint.latitude), Convert.ToDouble(startingAddressPoint.longitude));
                    GeoCoordinate endPointCoord = new GeoCoordinate(Convert.ToDouble(destinationPoint.latitude), Convert.ToDouble(destinationPoint.longitude));

                    WebProxyService.Station startPointNearestStation = getNearestAvailableStation(stations, startPointCoord, startingAddressPoint.city);
                    WebProxyService.Station endPointNearestStation = getNearestAvailableStation(stations, endPointCoord, destinationPoint.city);

                    List<Itinerary> itineraries = new List<Itinerary>();

                    itineraries.Add(getFootItinerary(startingAddressPoint, new Point(startPointNearestStation.position.latitude, startPointNearestStation.position.longitude)));
                    itineraries.Add(getBikeItinerary(new Point(startPointNearestStation.position.latitude, startPointNearestStation.position.longitude), new Point(endPointNearestStation.position.latitude, endPointNearestStation.position.longitude)));
                    itineraries.Add(getFootItinerary(new Point(endPointNearestStation.position.latitude, endPointNearestStation.position.longitude), destinationPoint));


                    float total_duration = 0;

                    foreach (var itineray in itineraries)
                    {
                        total_duration += itineray.routes[0].duration;
                    }

                    if (footItinerary.routes[0].duration < total_duration)
                    {
                        routings.Add(createRouting("Foot", footItinerary.routes[0].duration, startingAddressPoint,
                                                destinationPoint, footItinerary.routes[0].legs[0].steps));
                    }
                    else
                    {
                        routings.Add(createRouting("Foot", itineraries[0].routes[0].duration, startingAddressPoint,
                                    new Point(startPointNearestStation), itineraries[0].routes[0].legs[0].steps));

                        routings.Add(createRouting("Bike", itineraries[1].routes[0].duration, new Point(startPointNearestStation),
                                                    new Point(endPointNearestStation), itineraries[1].routes[0].legs[0].steps));

                        routings.Add(createRouting("Foot", itineraries[2].routes[0].duration, new Point(endPointNearestStation),
                                                    destinationPoint, itineraries[2].routes[0].legs[0].steps));

                        stationLog.AddLogForStation(startPointNearestStation.name);
                        stationLog.AddLogForStation(endPointNearestStation.name);
                    }
                }
                else
                {
                    routings.Add(createRouting("Foot", footItinerary.routes[0].duration, startingAddressPoint,
                                                destinationPoint, footItinerary.routes[0].legs[0].steps));
                }
            } 

            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return routings;
        }

        /* Method to retreive the coordinates of an address */
        private Point getCoordinatesFromAddress(string addr)
        {
            var url = "https://geocoder.ls.hereapi.com/6.2/geocode.json?searchtext=" +
                    addr +
                    "&gen=" + 9 +
                    "&apiKey=xC9v5CLgnlJVFg6yahihcsH9a2Hv1Mz9R3KfF5N0zgs";


            HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            HereAddress address = JsonSerializer.Deserialize<HereAddress>(responseBody);

            if (address.Response.View.Length > 0)
            {
                Point point = new Point(address.Response.View[0].Result[0].Location.DisplayPosition.Latitude,
                                        address.Response.View[0].Result[0].Location.DisplayPosition.Longitude);

                point.city = address.Response.View[0].Result[0].Location.Address.City;

                return point;
            } else
            {
                return null;
            }
        }

        /* Method to get a routing object according to the parameters */
        private Routing createRouting(string type, float duration, Point a, Point b, Step[] steps)
        {
            int step_index = 0;
            Routing routing = new Routing();
            routing.type = type;
            routing.duration = duration;
            routing.waypoints = new Point[2];
            routing.waypoints[0] = a;
            routing.waypoints[1] = b;
            routing.steps = new RouteStep[steps.Length];
            foreach (var step in steps)
                routing.steps[step_index++] = new RouteStep(step.driving_side, step.distance, step.name,
                    step.maneuver.type, step.maneuver.modifier, new Point(step.maneuver.location[1],
                    step.maneuver.location[0]));
            return routing;
        }

        /* Method to get a foot itinerary between two points */
        private Itinerary getFootItinerary(Point start, Point end)
        {

            string url = "https://routing.openstreetmap.de/routed-foot/route/v1/driving/" +
                start.longitude.ToString().Replace(",", ".") + "," +
                start.latitude.ToString().Replace(",", ".") + ";" +
                end.longitude.ToString().Replace(",", ".") + "," +
                end.latitude.ToString().Replace(",", ".") +
                "?overview=false&geometries=polyline&steps=true";
            string responseBody;

            if (cache.Get(url) == null)
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                cache.Add(url, responseBody, cacheItemPolicy);
            }
            else
            {
                responseBody = (string)cache.Get(url);
            }

            Itinerary itinerary = JsonSerializer.Deserialize<Itinerary>(responseBody);

            return itinerary;
        }

        /* Method to get a bike itinerary between two points */
        private Itinerary getBikeItinerary(Point start, Point end)
        {

            string url = "https://routing.openstreetmap.de/routed-bike/route/v1/driving/" +
                start.longitude.ToString().Replace(",", ".") + "," +
                start.latitude.ToString().Replace(",", ".") + ";" +
                end.longitude.ToString().Replace(",", ".") + "," +
                end.latitude.ToString().Replace(",", ".") +
                "?overview=false&geometries=polyline&steps=true";
            string responseBody;

            if (cache.Get(url) == null)
            {
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                cache.Add(url, responseBody, cacheItemPolicy);
            }
            else
            {
                responseBody = (string)cache.Get(url);
            }

            Itinerary itinerary = JsonSerializer.Deserialize<Itinerary>(responseBody);

            return itinerary;
        }

        /* Method to get the nearest station, with available bikes, to a given point */
        private WebProxyService.Station getNearestAvailableStation(WebProxyService.Station[] stations, GeoCoordinate pointCoord, string contract)
        {
            WebProxyService.Station nearestStation = getNearestStation(stations, pointCoord);
            WebProxyService.Station station = webProxyService.GetStation(nearestStation.number, contract);

            while (station.totalStands.availabilities.bikes == 0 && station.totalStands.availabilities.electricalBikes == 0)
            {
                stations = stations.Where((source, index) => index != Array.IndexOf(stations, nearestStation)).ToArray();
                nearestStation = getNearestStation(stations, pointCoord);
                station = webProxyService.GetStation(nearestStation.number, contract);
            }

            return nearestStation;
        }

        /* Method to get the nearest station to a given point */
        private WebProxyService.Station getNearestStation(WebProxyService.Station[] stations, GeoCoordinate pointCoord)
        {
            WebProxyService.Station nearestStation = null;
            double bestDistance = 0;

            foreach (var station in stations)
            {
                if (nearestStation == null)
                {
                    nearestStation = station;
                    bestDistance = pointCoord.GetDistanceTo(new GeoCoordinate(station.position.latitude, station.position.longitude));
                }
                else if (pointCoord.GetDistanceTo(new GeoCoordinate(station.position.latitude, station.position.longitude)) < bestDistance)
                {
                    nearestStation = station;
                    bestDistance = pointCoord.GetDistanceTo(new GeoCoordinate(station.position.latitude, station.position.longitude));
                }
            }

            return nearestStation;
        }

        /* Method to get the frequentation stations list
         This method is only used by the HeavyClient*/
        public List<string> GetStationLogList()
        {
            return stationLog.getStationsName();
        }

        /* Method to get the frequentation of a station
         This method is only used by the HeavyClient*/
        public int GetStationLog(string stationName)
        {
            return stationLog.getStationLog(stationName);
        }
    }
}
