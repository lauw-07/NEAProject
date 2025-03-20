using Frontend.Models.Database;
using Frontend.Models.Indicators;
using Frontend.Models.PolygonData;
using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class GraphContainer {

        [Parameter]
        public string? SelectedInstrument { get; set; }

        [Parameter]
        public string? SelectedSecurity { get; set; }

        [Parameter]
        public string? SelectedIndicator { get; set; }

        private string? Ticker { get; set; }
        private int Multiplier { get; set; }
        private string? Timespan { get; set; }
        private DateTime DateFrom { get; set; }
        private DateTime DateTo { get; set; }

        private string? _currentSecurity;
        private PolygonStockPriceData? polygonStockPriceData;
        private string symbol = string.Empty;
        private List<Result>? results;
        private TS _localTimeseries = new();
        private Dictionary<string, List<TS>> _indicatorCache = new();
        private List<string> _selectedIndicatorList = new List<string>();

        private TS TimeseriesParam = new();
        private List<TS> IndicatorTSParameter = new();

        protected override async Task OnParametersSetAsync() {
            // When the stock changes, reset the selected indicator list and indicator cache.
            if (SelectedSecurity != _currentSecurity) {
                _currentSecurity = SelectedSecurity;
                symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                if (!string.IsNullOrEmpty(symbol)) {
                    TimeseriesParam = await ReadFromDatabase();
                }
                // Clear the list and cache when a new stock is selected.
                _selectedIndicatorList.Clear();
                _indicatorCache.Clear();
            }

            // Process the indicator parameter only if it exists
            // Remove indicator if exists and add if not exists
            if (!string.IsNullOrEmpty(SelectedIndicator)) {
                if (_selectedIndicatorList.Contains(SelectedIndicator)) {
                    _selectedIndicatorList.Remove(SelectedIndicator);
                } else {
                    _selectedIndicatorList.Add(SelectedIndicator);
                }
                // Reset the SelectedIndicator parameter so the toggle only occurs once
                SelectedIndicator = null;
            }

            RebuildIndicatorTsParameter();
            StateHasChanged();
        }

        private void RebuildIndicatorTsParameter() {
            IndicatorTSParameter.Clear();

            foreach (string indicator in _selectedIndicatorList) {
                if (!_indicatorCache.ContainsKey(indicator)) {
                    _indicatorCache[indicator] = GenerateIndicatorTS(TimeseriesParam, indicator);
                }

                IndicatorTSParameter.AddRange(_indicatorCache[indicator]);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Console.WriteLine("Fetching initial data from database");
                symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                TimeseriesParam = await GetLocalTS();
                
                if (TimeseriesParam.Size() <= 0) {
                    Console.WriteLine("Timeseries is empty");
                }

                StateHasChanged();
            }
        }

        private async Task FetchData() {
            await LoadData();
            await SaveToDatabase();
            TimeseriesParam = await GetLocalTS();
        }

        private async Task<TS> GetLocalTS() {
            _localTimeseries = await ReadFromDatabase();
            return _localTimeseries;
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

        private async Task<TS> ReadFromDatabase() {
            List<PriceData> priceData = await databaseHandler.GetPriceDataAsync(symbol);
            int initialCapacity = priceData.Count;

            //TS openPxTimeseries = new TS(initialCapacity);
            TS closePxTimeseries = new TS(initialCapacity);

            /*
             * Each price data object contains single prices related to a single timestamp
             */

            foreach (PriceData priceDataObj in priceData) {
                double openPx = priceDataObj.OpenPx;
                double closePx = priceDataObj.ClosePx;
                DateTime pxDate = priceDataObj.PxDate;

                //openPxTimeseries.Add(pxDate, openPx);
                closePxTimeseries.Add(pxDate, closePx);
            }

            //return new List<TS> { openPxTimeseries, closePxTimeseries };
            return closePxTimeseries;
        }

        private List<TS> GenerateIndicatorTS(TS closePxTs, string indicator) {
            //TS openPxTS = originalTs[0];
            //TS closePxTS = originalTs[1];

            switch (indicator) {
                case "Simple Moving Average":
                    TS smaTs = closePxTs.Sma(20);
                    smaTs.SetIndicator("Sma");
                    return new List<TS> { /*openPxTS.Sma(20),*/ smaTs };
                case "Exponentially Weighted Moving Average":
                    TS ewmaTs = closePxTs.Ewma(20);
                    ewmaTs.SetIndicator("Ewma");
                    return new List<TS> { /*openPxTS.Ewma(20),*/ ewmaTs };
                case "Bollinger Bands":
                    (TS,TS) bollingerBands = closePxTs.BollingerBands(20, 2);
                    TS upperBound = bollingerBands.Item1;
                    upperBound.SetIndicator("Bollinger Bands Upper Band");
                    TS lowerBound = bollingerBands.Item2;
                    lowerBound.SetIndicator("Bollinger Bands Lower Band");
                    return new List<TS> { upperBound, lowerBound };
                case "Exponential Weighted Volatility":
                    TS ewvolTs = closePxTs.Ewvol(20);
                    ewvolTs.SetIndicator("Ewvol");
                    return new List<TS> { ewvolTs };
                default:
                    return new List<TS>();
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
