using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingWithBikes
{
    /* Singleton class to store stations frequentation */
    class StationLog
    {
        private IDictionary<string, int> dictionary;

        private static StationLog instance = null;

        private StationLog()
        {
            dictionary = new Dictionary<string, int>();
        }

        /* Method to get the unique instance of the class */
        public static StationLog GetInstance()
        {
            if (instance == null) instance = new StationLog();
            return instance;
        }

        /* Method to add a frequentation log for a station */
        public void AddLogForStation(string stationName)
        {
            if (dictionary.ContainsKey(stationName))
            {
                dictionary[stationName] += 1;
            } else
            {
                dictionary.Add(stationName, 1);
            }
        }

        /* Method to get the frequentation of a station */
        public int getStationLog(string stationName)
        {
            return dictionary[stationName];
        }

        /* Method to get the list of the stations stored in the class */
        public List<string> getStationsName()
        {
            List<string> stations = new List<string>();
            foreach (var item in dictionary)
                stations.Add(item.Key);
            return stations;
        }
    }
}
