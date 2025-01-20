using Frontend.Models.Database;
using Frontend.Models.Indicators;
using Frontend.Models.PolygonData;
using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class GraphContainerComponent {

        [Parameter]
        public string? SelectedInstrument { get; set; }

        [Parameter]
        public string? SelectedSecurity { get; set; }

        [Parameter]
        public string? SelectedIndicator { get; set; }

        List<string> _selectedIndicatorList = new List<string>();

        protected override void OnParametersSet() {
            if (SelectedIndicator != null) {
                if (!_selectedIndicatorList.Contains(SelectedIndicator)) {
                    _selectedIndicatorList.Add(SelectedIndicator);
                }

                if (TimeseriesParameter != null && TimeseriesParameter.Count > 0) {
                    CreateIndicatorTS(TimeseriesParameter, SelectedIndicator);
                }
            }
        }

        private string? Ticker { get; set; }
        private int Multiplier { get; set; }
        private string? Timespan { get; set; }
        private DateTime DateFrom { get; set; }
        private DateTime DateTo { get; set; }

        private PolygonStockPriceData? polygonStockPriceData;
        private string symbol = string.Empty;
        private List<Result>? results;

        private List<TS> TimeseriesParameter = new(); //Parameter to pass on to GraphComponent
        private List<TS> IndicatorTSParameter = new();

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            Console.WriteLine("OnAfterRenderAsync triggered in GraphContainer component");

            if (firstRender) {
                Console.WriteLine("Fetching intial data from database");
                symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                Console.WriteLine($"Symbol: {symbol}");
                TimeseriesParameter = await ReadFromDatabase();
                Console.WriteLine(TimeseriesParameter);

                StateHasChanged();
            } else {
                Console.WriteLine("Fetching new data from database");
                symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                Console.WriteLine($"Ticker: {symbol}");
                TimeseriesParameter = await ReadFromDatabase();
                Console.WriteLine(TimeseriesParameter);
            }
        }

        private async Task FetchData() {
            await LoadData();
            await SaveToDatabase();
            TimeseriesParameter = await ReadFromDatabase();
        }

        private async Task LoadData() {
            if (string.IsNullOrEmpty(Ticker) || string.IsNullOrEmpty(Timespan)) {
                return;
            }

            polygonDataLoader.SetParameters(Ticker, Multiplier, Timespan, DateFrom.ToString("yyyy-MM-dd"), DateTo.ToString("yyyy-MM-dd"));

            polygonStockPriceData = await polygonDataLoader.LoadPolygonStockDataAsync();

            if (polygonStockPriceData != null) {
                symbol = polygonStockPriceData.Ticker;
                results = polygonStockPriceData.Results;
            }
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

                    await databaseHandler.AddInstrumentDataAsync(symbol);
                } catch (Exception ex) { 
                    Console.WriteLine($"An error occurred while adding price data: {ex.Message}\n");
                }
            }
        }


        private async Task<List<TS>> ReadFromDatabase() {
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

                openPxTimeseries.Add(pxDate, openPx);
                closePxTimeseries.Add(pxDate, closePx);
            }

            return new List<TS> { openPxTimeseries, closePxTimeseries };
        }

        private void CreateIndicatorTS(List<TS> originalTimeseries, string indicator) {
            //My strategy will take in the original timeseries as a parameter and return a List<TS> object
            //I can then set the IndicatorTSParameter as this timeseries object
            if (originalTimeseries == null || originalTimeseries.Count <= 0) {
                return;
            }

            TS openPxTS = originalTimeseries[0];
            TS closePxTS = originalTimeseries[1];

            switch (indicator) {
                case "Simple Moving Average":
                    //for now i will choose my own values for the n-day period
                    //This is not very optimised right now, will need to optimise

                    if (!openPxTS.RemoveIndicator("Sma")) {
                        TS sma20openPxTs = openPxTS.Sma(20);
                        IndicatorTSParameter.Add(sma20openPxTs);
                    }

                    if (!closePxTS.RemoveIndicator("Sma")) {
                        TS sma20closePxTs = closePxTS.Sma(20);
                        IndicatorTSParameter.Add(sma20closePxTs);
                    }
                    break;
                case "Exponentially Weighted Moving Average":
                    if (!openPxTS.RemoveIndicator("Ewma")) {
                        TS ewma20openPxTs = openPxTS.Ewma(20);
                        IndicatorTSParameter.Add(ewma20openPxTs);
                    }
                    
                    if (!closePxTS.RemoveIndicator("Ewma")) {
                        TS ewma20closePxTs = closePxTS.Ewma(20);
                        IndicatorTSParameter.Add(ewma20closePxTs);
                    }
                    
                    break;
                default:
                    break;
            }
        }

        /* Store this new data into the database (however i haven't validated whether the database already contains this new data loaded)
         * Read all the data relating to the specific Ticker passed in from the input from the database
         * Convert data into a timeseries
         * Create two timeseries for opening and closing price
         * Store this in the data variable and pass this through as a parameter to the graph component
         * 
         * I will also need to update the database with new instruments and new price data
         */
    }
}
