using System.Runtime.Serialization;

namespace RoutingWithBikes
{
    [DataContract]
    public class Itinerary
    {
        [DataMember]
        public Waypoint[] waypoints { get; set; }

        [DataMember]
        public Route[] routes { get; set; }

        public string code { get; set; }
    }

    [DataContract]
    public class Waypoint
    {
        [DataMember]
        public float[] location { get; set; }

        public string hint { get; set; }
        public float distance { get; set; }
        public string name { get; set; }
    }

    [DataContract]
    public class Route
    {
        [DataMember]
        public Leg[] legs { get; set; }

        public string weight_name { get; set; }
        public float weight { get; set; }
        public float distance { get; set; }
        public float duration { get; set; }
    }

    [DataContract]
    public class Leg
    {
        [DataMember]
        public Step[] steps { get; set; }

        public float weight { get; set; }
        public float distance { get; set; }
        public string summary { get; set; }
        public float duration { get; set; }
    }

    [DataContract]
    public class Step
    {
        [DataMember]
        public string driving_side { get; set; }

        [DataMember]
        public float distance { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public Maneuver maneuver { get; set; }

        public Intersection[] intersections { get; set; }
        public string geometry { get; set; }
        public float duration { get; set; }
        public float weight { get; set; }
        public string mode { get; set; }
        public string rotary_name { get; set; }
        public string _ref { get; set; }
    }

    [DataContract]
    public class Maneuver
    {
        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string modifier { get; set; }

        [DataMember]
        public float[] location { get; set; }

        public int bearing_after { get; set; }
        public int bearing_before { get; set; }
        public int exit { get; set; }
    }

    [DataContract]
    public class Intersection
    {
        public int _out { get; set; }
        public bool[] entry { get; set; }
        public float[] location { get; set; }
        public int[] bearings { get; set; }
        public int _in { get; set; }
        public string[] classes { get; set; }
    }
}
