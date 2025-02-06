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

        List<string> _selectedIndicatorList = new List<string>();

        /*protected override void OnParametersSet() {
            if (SelectedIndicator != null && TimeseriesParameter != null && TimeseriesParameter.Count > 0) {
                if (_selectedIndicatorList.Contains(SelectedIndicator)) {
                    _selectedIndicatorList.Remove(SelectedIndicator);
                    UpdateIndicatorTs(TimeseriesParameter, SelectedIndicator, false);
                } else {
                    _selectedIndicatorList.Add(SelectedIndicator);
                    UpdateIndicatorTs(TimeseriesParameter, SelectedIndicator, true);
                }

                StateHasChanged();
            }
        }*/

        private string? Ticker { get; set; }
        private int Multiplier { get; set; }
        private string? Timespan { get; set; }
        private DateTime DateFrom { get; set; }
        private DateTime DateTo { get; set; }

        private PolygonStockPriceData? polygonStockPriceData;
        private string symbol = string.Empty;
        private List<Result>? results;
        private List<TS> _localTimeseries = new();
        private Dictionary<string, List<TS>> _indicatorCache = new();

        private List<TS> TimeseriesParameter = new(); //Parameter to pass on to GraphComponent
        private List<TS> IndicatorTSParameter = new();

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Console.WriteLine("Fetching initial data from database");
                symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                TimeseriesParameter = await GetLocalTS();
                StateHasChanged(); // Only needed if data actually changes
            }
        }

        private async Task FetchData() {
            await LoadData();
            await SaveToDatabase();
            TimeseriesParameter = await GetLocalTS();
        }

        private async Task<List<TS>> GetLocalTS() {
            if (_localTimeseries == null) {
                _localTimeseries = await ReadFromDatabase();
            }
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

        

        private void UpdateIndicatorTs(List<TS> originalTs, string indicator, bool addIndicator) {
            if (originalTs == null || originalTs.Count == 0) return;

            if (addIndicator) {
                if (!_indicatorCache.ContainsKey(indicator)) {
                    _indicatorCache[indicator] = GenerateIndicatorTS(originalTs, indicator);
                }
                IndicatorTSParameter.AddRange(_indicatorCache[indicator]);
            } else {
                if (_indicatorCache.ContainsKey(indicator)) {
                    IndicatorTSParameter.RemoveAll(ts => _indicatorCache[indicator].Contains(ts));
                }
            }
        }

        private List<TS> GenerateIndicatorTS(List<TS> originalTs, string indicator) {
            TS openPxTS = originalTs[0];
            TS closePxTS = originalTs[1];

            switch (indicator) {
                case "Simple Moving Average":
                    return new List<TS> { openPxTS.Sma(20), closePxTS.Sma(20) };
                case "Exponentially Weighted Moving Average":
                    return new List<TS> { openPxTS.Ewma(20), closePxTS.Ewma(20) };
                case "Bollinger Bands":
                    (TS,TS) bollingerBands = closePxTS.BollingerBands(20, 2);
                    return new List<TS> { bollingerBands.Item1, bollingerBands.Item2 };
                case "Exponential Weighted Volatility":
                    return new List<TS> { closePxTS.Ewvol(20) };
                default:
                    return new List<TS>();
            }
        }

        private void ToggleIndicator(string indicator) {
            if (_selectedIndicatorList.Contains(indicator)) {
                _selectedIndicatorList.Remove(indicator);
                UpdateIndicatorTs(TimeseriesParameter, indicator, false);
            } else {
                _selectedIndicatorList.Add(indicator);
                UpdateIndicatorTs(TimeseriesParameter, indicator, true);
            }

            StateHasChanged(); // Ensure UI updates
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
