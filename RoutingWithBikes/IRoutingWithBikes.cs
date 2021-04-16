using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace RoutingWithBikes
{

    [ServiceContract]
    public interface IRoutingWithBikes
    {

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "GetItineraryBetweenTwoAddress?start={startingAddress}&end={destination}", ResponseFormat = WebMessageFormat.Json)]
        List<Routing> GetItineraryBetweenTwoAddress(string startingAddress, string destination);

        [OperationContract]
        List<string> GetStationLogList();

        [OperationContract]
        int GetStationLog(string stationName);
    }

    [DataContract]
    public class Point
    {
        [DataMember]
        public WebProxyService.Station station { get; set; }

        [DataMember]
        public double latitude { get; set; }

        [DataMember]
        public double longitude { get; set; }

        public string city { get; set; }

        public Point(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public Point(WebProxyService.Station station)
        {
            this.latitude = station.position.latitude;
            this.longitude = station.position.longitude;
            this.station = station;
        }
    }
}
