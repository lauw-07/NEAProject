﻿using Frontend.Models.Database;
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
            // This method is only triggered when a parameter is set
            if (!string.IsNullOrEmpty(SelectedIndicator)) {
                if (_selectedIndicatorList.Contains(SelectedIndicator)) {
                    _selectedIndicatorList.Remove(SelectedIndicator);
                } else {
                    _selectedIndicatorList.Add(SelectedIndicator);
                }
            }

            if (SelectedSecurity != _currentSecurity) {
                _currentSecurity = SelectedSecurity;
                symbol = await databaseHandler.GetInstrumentByNameAsync(SelectedSecurity);
                if (!string.IsNullOrEmpty(symbol)) {
                    TimeseriesParam = await ReadFromDatabase();
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

            // This should be called here since a parameter has to have been set and there are only two parameters so this 
            // means that i do not have to repeat this code in each if condition
            RebuildIndicatorTsParameter();
            StateHasChanged();
        }
        private List<TS> GenerateIndicatorTS(TS closePxTs, string indicator) {
            switch (indicator) {
                case "Simple Moving Average":
                    TS smaTs = closePxTs.Sma(20);
                    smaTs.SetIndicator("Sma");
                    return new List<TS> { smaTs };
                case "Exponentially Weighted Moving Average":
                    TS ewmaTs = closePxTs.Ewma(20);
                    ewmaTs.SetIndicator("Ewma");
                    return new List<TS> { ewmaTs };
                case "Bollinger Bands":
                    (TS, TS) bollingerBands = closePxTs.BollingerBands(20, 2);
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

        private void RebuildIndicatorTsParameter() {
            // Remove all indicators and just rebuild them again
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
