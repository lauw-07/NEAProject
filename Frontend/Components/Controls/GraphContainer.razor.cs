using Frontend.Models;
using Frontend.Models.Database;
using Frontend.Models.Indicators;
using Frontend.Models.PolygonData;
using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public struct IndicatorKey {
        // Not all the attributes will be used in each indicator so some
        // indicators will pass in default values
        public string IndicatorName { get; set; }
        public int WindowSize { get; set; }
        public double HalfLife { get; set; }
        public double Width { get; set; }

        public IndicatorKey() {
            IndicatorName = "";
            WindowSize = 0;
            HalfLife = 0;
            Width = 0;
        }

        public IndicatorKey(string indicatorName, int windowSize, double halfLife, double width) {
            IndicatorName = indicatorName;
            WindowSize = windowSize;
            HalfLife = halfLife;
            Width = width;
        }

        public override bool Equals(object? obj) {
            if (obj is IndicatorKey key)
                return IndicatorName == key.IndicatorName &&
                       WindowSize == key.WindowSize &&
                       HalfLife == key.HalfLife &&
                       Width == key.Width;
            return false;
        }

        public override int GetHashCode() {
            return HashCode.Combine(IndicatorName, WindowSize, HalfLife, Width);
        }
    }

        public partial class GraphContainer {
        [Parameter]
        public string? SelectedInstrument { get; set; }
        [Parameter]
        public string? SelectedSecurity { get; set; }

        private string? Ticker { get; set; }
        private int Multiplier { get; set; }
        private string? Timespan { get; set; }
        private DateTime DateFrom { get; set; }
        private DateTime DateTo { get; set; }

        private string? _currentSecurity;
        private PolygonStockPriceData? polygonStockPriceData;
        private string _symbol = string.Empty;
        private List<Result>? _results;
        private TS _localTimeseries = new();
        //private Dictionary<string, List<TS>> _indicatorCache = new();
        private HashTable<IndicatorKey, List<TS>> _indicatorCache = new();
        private List<string> _selectedIndicatorList = new List<string>();

        private TS _timeseriesParam = new();
        private List<TS> _indicatorTSParameter = new();

        protected override async Task OnParametersSetAsync() {
            // Only triggered if a parameter has been set
            if (SelectedSecurity != _currentSecurity) {
                _currentSecurity = SelectedSecurity;
                _symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                if (!string.IsNullOrEmpty(_symbol)) {
                    _timeseriesParam = await ReadFromDatabase(_symbol);
                }
                // Clear indicator state when a new stock is selected.
                _selectedIndicatorList.Clear();
                _indicatorCache = new();
            }
            await RebuildIndicatorTsParameter();
        }

        // method to toggle an indicator which gets called from a parent component
        public async Task ToggleIndicator(string indicator) {
            // Remove indicator if exists and add if not exists
            if (_selectedIndicatorList.Contains(indicator)) {
                _selectedIndicatorList.Remove(indicator);

                var (key, ts) = await GenerateIndicatorTS(_timeseriesParam, indicator);
                _indicatorCache.Remove(key);
            } else {
                _selectedIndicatorList.Add(indicator);
            }
            await RebuildIndicatorTsParameter();
            StateHasChanged();
        }

        private async Task<(IndicatorKey, List<TS>)> GenerateIndicatorTS(TS closePxTs, string indicator) {
            IndicatorKey key = new IndicatorKey();
            switch (indicator) {
                case "Simple Moving Average":
                    TS smaTs = closePxTs.Sma(20);
                    smaTs.SetIndicator("Sma");

                    key = new IndicatorKey("Simple Moving Average", 20, 0, 0);
                    return (key, new List<TS> { smaTs });
                case "Exponentially Weighted Moving Average":
                    TS ewmaTs = closePxTs.Ewma(20);
                    ewmaTs.SetIndicator("Ewma");

                    key = new IndicatorKey("Exponentially Weighted Moving Average", 0, 20, 0);
                    return (key, new List<TS> { ewmaTs });
                case "Bollinger Bands":
                    (TS, TS) bollingerBands = closePxTs.BollingerBands(20, 2);
                    TS upperBound = bollingerBands.Item1;
                    upperBound.SetIndicator("Bollinger Bands Upper Band");
                    TS lowerBound = bollingerBands.Item2;
                    lowerBound.SetIndicator("Bollinger Bands Lower Band");

                    key = new IndicatorKey("Bollinger Bands", 20, 0, 2);

                    return (key, new List<TS> { upperBound, lowerBound });
                case "Exponential Weighted Volatility":
                    TS ewvolTs = closePxTs.Ewvol(20);
                    ewvolTs.SetIndicator("Ewvol");

                    key = new IndicatorKey("Exponential Weighted Volatility", 0, 20, 0);
                    return (key, new List<TS> { ewvolTs });
                case "Linear Regression":
                    List<string> instrumentTickers = new List<string>();

                    List<Instrument> instruments = await databaseHandler.GetInstrumentDataByTypeAsync(SelectedInstrument);
                    foreach (Instrument instrument in instruments) {
                        if (instrument.InstrumentSymbol != _symbol) {
                            instrumentTickers.Add(instrument.InstrumentSymbol);
                        }
                    }

                    List<TS> predictorsTS = new List<TS>();
                    foreach (string ticker in instrumentTickers) {
                        predictorsTS.Add(await ReadFromDatabase(ticker));
                    }

                    TS linregTs = closePxTs.LinearRegression(predictorsTS);
                    linregTs.SetIndicator("Linear Regression");
                    key = new IndicatorKey("Linear Regression", 0, 0, 0);
                    return (key, new List<TS> { linregTs });
                default:
                    return (key, new List<TS>());
            }
        }

        private bool ContainsKey(IndicatorKey key) {
            try {
                List<TS> _ = _indicatorCache[key];
                return true;
            } catch (KeyNotFoundException) {
                return false;
            }
        }

        // Clear the timeseries parameters and rebuild all the parameters
        private async Task RebuildIndicatorTsParameter() {
            // Remove all indicators and just rebuild them again
            _indicatorTSParameter.Clear();

            foreach (string indicator in _selectedIndicatorList) {
                // Create indicator
                var (key, ts) = await GenerateIndicatorTS(_timeseriesParam, indicator);

                // Add indicator to cache if it doesn't exist
                if (!ContainsKey(key)) {
                    _indicatorCache.Add(key, ts);
                }
                _indicatorTSParameter.AddRange(_indicatorCache[key]);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Console.WriteLine("Fetching initial data from database");
                _symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                _timeseriesParam = await GetLocalTS();
                
                if (_timeseriesParam.Size() <= 0) {
                    Console.WriteLine("Timeseries is empty");
                }
                StateHasChanged();
            }
        }
        private async Task FetchData() {
            await LoadData();
            await SaveToDatabase();
            _timeseriesParam = await GetLocalTS();
        }

        private async Task<TS> GetLocalTS() {
            _localTimeseries = await ReadFromDatabase(_symbol);
            return _localTimeseries;
        }

        private async Task LoadData() {
            if (string.IsNullOrEmpty(Ticker) || string.IsNullOrEmpty(Timespan)) {
                return;
            }

            polygonDataLoader.SetParameters(Ticker, Multiplier, Timespan, DateFrom.ToString("yyyy-MM-dd"), DateTo.ToString("yyyy-MM-dd"));
            polygonStockPriceData = await polygonDataLoader.LoadPolygonStockDataAsync();

            if (polygonStockPriceData != null) {
                _symbol = polygonStockPriceData.Ticker;
                _results = polygonStockPriceData.Results;
            }
        }

        private async Task SaveToDatabase() {
            if (_results == null || _results.Count == 0) {
                return;
            }
            foreach (Result result in _results) {
                DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(result.Timestamp);
                string pxDate = offset.Date.ToShortDateString();
                double openPx = result.Open;
                double closePx = result.Close;
                double highPx = result.High;
                double lowPx = result.Low;
                double volume = result.Volume;

                try {
                    await databaseHandler.AddPriceDataAsync(_symbol, pxDate, openPx, closePx, highPx, lowPx, volume);

                    await databaseHandler.AddInstrumentDataAsync(_symbol);
                } catch (Exception ex) {
                    Console.WriteLine($"An error occurred while adding price data: {ex.Message}\n");
                }
            }
        }

        private async Task<TS> ReadFromDatabase(string ticker) {
            List<PriceData> priceData = await databaseHandler.GetPriceDataAsync(ticker);
            int initialCapacity = priceData.Count;
            TS closePxTimeseries = new TS(initialCapacity);

            // Each price data object contains single prices related to a single timestamp

            foreach (PriceData priceDataObj in priceData) {
                double openPx = priceDataObj.OpenPx;
                double closePx = priceDataObj.ClosePx;
                DateTime pxDate = priceDataObj.PxDate;
                closePxTimeseries.Add(pxDate, closePx);
            }
            return closePxTimeseries;
        }
    }
}