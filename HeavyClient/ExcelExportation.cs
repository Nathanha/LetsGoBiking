using System.Data;

namespace HeavyClient
{
    /* Class to export the stations frequentation to an excel sheet */
    class ExcelExportation
    {
        RoutingWithBikes.IRoutingWithBikes routingWithBikes;

        public ExcelExportation() 
        {
            routingWithBikes = new RoutingWithBikes.RoutingWithBikesClient();
        }

        /* Method to get a dataTable with the stations frequentation */
        private System.Data.DataTable CreateDataTable()
        {
            System.Data.DataTable table = new System.Data.DataTable();

            string[] stationsName = routingWithBikes.GetStationLogList();

            table.Columns.Add("Station", typeof(string));
            table.Columns.Add("Number of use", typeof(int));

            foreach (var stationName in stationsName)
            {
                table.Rows.Add(stationName, routingWithBikes.GetStationLog(stationName));
            }

            return table;
        }

        /* Method to export the dataTable to an excel sheet, and store it in the path given in parameter */
        public void ExportToExcel(string path)
        {
            DataSet ds = new DataSet("New_DataSet");
            ds.Tables.Add(CreateDataTable());
            ExcelLibrary.DataSetHelper.CreateWorkbook(path, ds);
        }
    }
}
