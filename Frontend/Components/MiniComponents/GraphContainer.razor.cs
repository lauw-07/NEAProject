using Frontend.Models.PolygonData;
using Frontend.Models.Timeseries;
using Frontend.Models.Database;

namespace Frontend.Components.MiniComponents {
    public partial class GraphContainer {
        private string? ticker { get; set; }
        private int multiplier { get; set; }
        private string? timespan { get; set; }
        private DateTime dateFrom { get; set; }
        private DateTime dateTo { get; set; }

        private PolygonStockPriceData? polygonStockPriceData;
        private string symbol = string.Empty;
        private List<Result>? results;
        private List<TS>? TSLists;

        private async Task FetchData() {
            await LoadData();
            await SaveToDatabase();
            await ReadFromDatabase();
        }

        private async Task LoadData() {
            if (string.IsNullOrEmpty(ticker) || string.IsNullOrEmpty(timespan)) {
                return;
            }

            polygonDataLoader.SetParameters(ticker, multiplier, timespan, dateFrom.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd"));
            
            polygonStockPriceData = await polygonDataLoader.LoadPolygonStockDataAsync();

            symbol = polygonStockPriceData.Ticker;
            results = polygonStockPriceData.Results;
        }

        private async Task SaveToDatabase() {
            if (results == null || results.Count == 0) {
                return;
            }
            foreach (Result result in results) {
                DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(result.Timestamp);
                string pxDate = offset.Date.ToShortDateString();
                double openPx = result.Open;
                double closePx = result.Close;
                double highPx = result.High;
                double lowPx = result.Low;
                double volume = result.Volume;

                try {
                    await databaseHandler.AddPriceDataAsync(symbol, pxDate, openPx, closePx, highPx, lowPx, volume);
                } catch (Exception ex) {
                    Console.WriteLine($"An error occurred while adding price data: {ex.Message}\n");
                }
            }
        }


        private async Task<TS[]> ReadFromDatabase() {
            List<PriceData> priceData = await databaseHandler.GetPriceDataAsync(symbol);
            int initialCapacity = priceData.Count;

            TS openPxTimeseries = new TS(initialCapacity);
            TS closePxTimeseries = new TS(initialCapacity);

            /*
             * Each price data object contains single prices related to a single timestamp
             */
            

            foreach (PriceData priceDataObj in priceData) {
                double openPx = priceDataObj.OpenPx;
                double closePx = priceDataObj.ClosePx;
                DateTime pxDate = priceDataObj.PxDate;

                
            }
        }

        /* Store this new data into the database (however i haven't validated whether the database already contains this new data loaded)
         * Read all the data relating to the specific ticker passed in from the input from the database
         * Convert data into a timeseries
         * Create two timeseries for opening and closing price
         * Store this in the data variable and pass this through as a parameter to the graph component
         */


        private TS ConvertToTS() {
            //TS openingPxTS = new TS() 
        }
    }
}
