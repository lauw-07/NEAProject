using Frontend.Models;
using Frontend.Models.Database;
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
        // Fields for displaying data on graph

        // Maybe don't need this
        //[Parameter]
        //public string SelectedInstrument { get; set; } = "";

        [Parameter]
        public string SelectedSecurity { get; set; } = "";
        private string _currentSecurity = ""; 
        private string _ticker = "AAPL"; // By default set to AAPL
        private string _selectedInstrument = "Stocks";

        // Fields for indicators
        private HashTable<IndicatorKey, List<TS>> _indicatorCache = new();
        private List<string> _selectedIndicators = new();

        // Variables to be passed as parameters to the graph component
        private TS _timeseriesParam = new();
        private List<TS> _indicatorsParam = new();

        // Methods for handling changes in parameters
        protected override async Task OnParametersSetAsync() {
            // Only triggered if a parameter has been set
            if (SelectedSecurity != _currentSecurity) {
                _currentSecurity = SelectedSecurity;
                _ticker = await databaseHandler.GetTickerByNameAsync(SelectedSecurity);
                _selectedInstrument = await databaseHandler.GetInstrumentTypeFromTickerAsync(_ticker);
                if (!string.IsNullOrEmpty(_ticker)) {
                    _timeseriesParam = await GetTSDataFromDatabase(_ticker);
                }
                // Clear indicator state when a new stock is selected.
                _selectedIndicators.Clear();
                _indicatorCache = new();
            }
            await RebuildIndicatorTsParameter();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Console.WriteLine("Fetching initial data from database");
                _timeseriesParam = await GetTSDataFromDatabase(_ticker);

                if (_timeseriesParam.Size() <= 0) {
                    Console.WriteLine("Timeseries is empty");
                }
                StateHasChanged();
            }
        }

        public async Task ToggleIndicator(string indicator) {
            // method to toggle an indicator which gets called from a parent component
            // Remove indicator if exists and add if not exists
            if (_selectedIndicators.Contains(indicator)) {
                _selectedIndicators.Remove(indicator);

                var (key, ts) = await GenerateIndicatorTS(_timeseriesParam, indicator);
                _indicatorCache.Remove(key);
            } else {
                _selectedIndicators.Add(indicator);
            }
            await RebuildIndicatorTsParameter();
            StateHasChanged();
        }


        // Methods for handling indicator selection
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
                    List<string> predictorsTickers = await databaseHandler.GetTickersBasedOfATickerAsync(_ticker);

                    List<TS> predictorsTS = new List<TS>();
                    foreach (string ticker in predictorsTickers) {
                        predictorsTS.Add(await GetTSDataFromDatabase(ticker));
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

        
        private async Task RebuildIndicatorTsParameter() {
            // Clear the timeseries parameters and rebuild all the parameters
            // Remove all indicators and just rebuild them again
            _indicatorsParam.Clear();

            foreach (string indicator in _selectedIndicators) {
                // Create indicator
                var (key, ts) = await GenerateIndicatorTS(_timeseriesParam, indicator);

                // Add indicator to cache if it doesn't exist
                if (!ContainsKey(key)) {
                    _indicatorCache.Add(key, ts);
                }
                _indicatorsParam.AddRange(_indicatorCache[key]);
            }
        }

        // Methods for getting data from database
        private async Task<TS> GetTSDataFromDatabase(string ticker) {
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
