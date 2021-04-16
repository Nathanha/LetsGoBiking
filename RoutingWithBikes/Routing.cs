using System;
using System.Runtime.Serialization;

namespace RoutingWithBikes
{
    [DataContract]
    public class Routing
    {
        [DataMember]
        public string type { get; set; }

        [DataMember]
        public Point[] waypoints { get; set; }

        [DataMember]
        public RouteStep[] steps { get; set; }

        [DataMember]
        public float duration { get; set; }
    }

    [DataContract]
    public class RouteStep
    {
        [DataMember]
        public string driving_side { get; set; }

        [DataMember]
        public float distance { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string modifier { get; set; }

        [DataMember]
        public Point location { get; set; }

        public RouteStep(string driving_side, float distance, string name, string type, string modifier, Point location)
        {
            this.driving_side = driving_side;
            this.distance = distance;
            this.name = name;
            this.type = type;
            this.modifier = modifier;
            this.location = location;
        }
    }

}
