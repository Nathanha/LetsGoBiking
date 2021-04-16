using System;

namespace HeavyClient
{
    class Program
    {
        static RoutingWithBikes.IRoutingWithBikes routingWithBikes = new RoutingWithBikes.RoutingWithBikesClient();

        static void Main(string[] args)
        {
            ExcelExportation excel = new ExcelExportation();

            DisplayItinerary();

            int choice = 0;

            while(choice != 3)
            {
                Console.WriteLine("\nChoose an action : ");
                Console.WriteLine("0 - Go to another destination");
                Console.WriteLine("1 - Display stations statistics");
                Console.WriteLine("2 - Export stations statistics to excel");
                Console.WriteLine("3 - Quit");
                choice = Int16.Parse(Console.ReadLine());

                Console.WriteLine("");

                if (choice == 0)
                {
                    DisplayItinerary();
                } else if (choice == 1)
                {
                    DisplayStationsLogs();
                } else if (choice == 2)
                {
                    string path = "";
                    foreach (var el in args)
                        path += el;
                    excel.ExportToExcel(path);
                    Console.WriteLine("Data exported");
                }
            }
        }

        /* Method to let the user enter a starting point and a destination, and display the route to connect the two */
        private static void DisplayItinerary()
        {
            RoutingWithBikes.IRoutingWithBikes routingWithBikes = new RoutingWithBikes.RoutingWithBikesClient();

            Console.WriteLine("Enter your actual position address :");
            string startingAddress = Console.ReadLine();
            //string startingAddress = "37 Rue de la Bastille, 44000 Nantes";

            Console.WriteLine("Enter your destination address");
            string destinationAddress = Console.ReadLine();
            //string destinationAddress = "Place des Enfants Nantais, 44000 Nantes";


            RoutingWithBikes.Routing[] routings = routingWithBikes.GetItineraryBetweenTwoAddress(startingAddress, destinationAddress);

            if (routings.Length > 0)
            {
                Console.WriteLine("\nRouting from " + startingAddress + " to " + destinationAddress + "\n");

                foreach (var routing in routings)
                {
                    if (routing.type == "Foot" && routing.waypoints[1].station != null)
                    {
                        Console.WriteLine("------ " + Math.Floor(routing.duration / 60) + " min : Routing to station " + routing.waypoints[1].station.name + " ------");
                    }
                    else if (routing.type == "Foot")
                    {
                        Console.WriteLine("------ " + Math.Floor(routing.duration / 60) + " min : Routing to destination ------");
                    }
                    else if (routing.type == "Bike")
                    {
                        Console.WriteLine("------ " + Math.Floor(routing.duration / 60) + " min : Routing to station " + routing.waypoints[1].station.name + " ------");
                    }


                    foreach (RoutingWithBikes.RouteStep step in routing.steps)
                    {
                        string text = step.type;
                        if (step.modifier != null) text += " " + step.modifier;
                        if (step.name != null && step.name != "") text += " on " + step.name;
                        if (step.distance > 0) text += " in " + step.distance + "m";
                        Console.WriteLine(text);
                    }
                }
            } else
            {
                Console.WriteLine("\nOne of the two addresses has an incorrect syntax !");
            }
        }

        /* Method to display the stations frequentation */
        private static void DisplayStationsLogs()
        {
            string[] stationsNames = routingWithBikes.GetStationLogList();

            foreach (var stationName in stationsNames)
            {
                Console.WriteLine("Station " + stationName + " : " + routingWithBikes.GetStationLog(stationName) + " uses");
            }
        }
    }
}
