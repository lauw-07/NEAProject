using Frontend.Models.PolygonData;
using Frontend.Models.Timeseries;

namespace Frontend.Components.MiniComponents {
    public partial class GraphContainer {
        private string? ticker { get; set; }
        private int multiplier { get; set; }
        private string? timespan { get; set; }
        private DateTime dateFrom { get; set; }
        private DateTime dateTo { get; set; }

        private PolygonStockPriceData? polygonStockPriceData;
        private List<TS>? TSLists;

        private async Task LoadData() {
            if (string.IsNullOrEmpty(ticker) || string.IsNullOrEmpty(timespan)) {
                return;
            }

            polygonDataLoader.SetParameters(ticker, multiplier, timespan, dateFrom.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd"));

            polygonStockPriceData = await polygonDataLoader.LoadPolygonStockDataAsync();
        }

        /* Store this new data into the database (however i haven't validated whether the database already contains this new data loaded)
         * Read all the data relating to the specific ticker passed in from the input from the database
         * Convert data into a timeseries
         * Create two timeseries for opening and closing price
         * Store this in the data variable and pass this through as a parameter to the graph component
         */


        private void ConvertToTS() {
            //TS openingPxTS = new TS() 
        }
    }
}
