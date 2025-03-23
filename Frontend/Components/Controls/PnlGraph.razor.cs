using Frontend.Models.Backtest;
using Frontend.Models.Backtest.Breakout;
using Frontend.Models.Backtest.Crossover;
using Frontend.Models.Backtest.Reversion;
using Frontend.Models.Database;
using Instrument = Frontend.Models.Database.Instrument;
using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Data;

namespace Frontend.Components.Controls
{
    public partial class PnlGraph {
        [Parameter]
        public string? Strategy { get; set; }

        [Parameter]
        public Dictionary<string, string> Parameters { get; set; } = new();

        private List<TS> _timeseriesToGraph = new();
        private readonly Dictionary<string, Dictionary<string, object>> _dataset = new();

        // for example { "IndicatorTs" : [{ "Timestamps" : [ 1230912, 1230912, 1230912, 1230912 ] }, { "Values" : [ 123, 123 ] } ]}

        protected override async void OnParametersSet() {
            _dataset.Clear();
            _timeseriesToGraph.Clear();
            if (Parameters != null && Parameters.Count > 0) {
                await Backtest(databaseHandler);
                await Js.InvokeVoidAsync("DrawGraph", _dataset, "pnlGraph");
            } else {
                Console.WriteLine("Parameters data is null");
            }
        }

        private async Task Backtest(DatabaseHandler dbHandler) {
            BacktestManager backtestManager = new BacktestManager();


            string ticker = Parameters["Ticker"];

            Instrument instrument = await ReadInstrumentFromDatabase(dbHandler, ticker);
            List<TS> timeseries = await ReadPriceDataFromDatabase(dbHandler, ticker);

            switch (Strategy) {
                case "Bollinger Bands Breakout":
                    GenerateBollingerBreakoutPnl(instrument, timeseries, backtestManager);
                    break;
                case "EWMA Crossover":
                    GenerateEwmaCrossoverPnl(instrument, timeseries, backtestManager);
                    break;
                case "Mean Reversion":
                    GenerateMeanReversionPnl(instrument, timeseries, backtestManager);
                    break;
                default:
                    break;
            }
        }

        private void GenerateEwmaCrossoverPnl(Instrument instrument, List<TS> timeseries, BacktestManager backtestManager) {
            TS closePxTs = timeseries[1];

            Type strategyType = typeof(EwmaCrossoverStrategy);

            StrategyParams strategyParams = new StrategyParams();
            Dictionary<string, object> inputDict = GenerateEwmaCrossoverParams(Parameters);
            strategyParams.AddInputs(inputDict);

            backtestManager.SetStrategy(strategyType, strategyParams, instrument);

            List<StrategyInput> strategyInputs = new List<StrategyInput>();
            for (int i = 0; i < closePxTs.Size(); i++) {
                StrategyInput strategyInput = new StrategyInput();
                strategyInput.AddInput("ClosePrice", closePxTs.GetValue(i));
                strategyInput.AddInput("Timestamp", closePxTs.GetTimestamp(i));
                strategyInputs.Add(strategyInput);
            }

            TS pnlTs = backtestManager.RunBacktest(strategyInputs);
            
            TS ewmaFast = closePxTs.Ewma((double)inputDict["FastHL"]);
            TS ewmaSlow = closePxTs.Ewma((double)inputDict["SlowHL"]);

            List<TS> indicatorTs = new List<TS>() { ewmaFast, ewmaSlow };

            AddToDataset(closePxTs, "Close Prices");
            AddToDataset(pnlTs, "P&L");
            AddToDataset(indicatorTs, "Moving Averages");
        }

        private Dictionary<string, object> GenerateEwmaCrossoverParams(Dictionary<string, string> rawParams) {
            Dictionary<string, object> strategyParams = new();

            bool IsFixedValueExposure = false;
            foreach (KeyValuePair<string, string> pair in rawParams) {
                switch (pair.Key) {
                    case "Ticker":
                        break;
                    case "Slow Half Life":
                        strategyParams.Add("SlowHL", double.Parse(pair.Value));
                        break;
                    case "Fast Half Life":
                        strategyParams.Add("FastHL", double.Parse(pair.Value));
                        break;
                    case "Width":
                        strategyParams.Add("Width", double.Parse(pair.Value));
                        break;
                    case "Exposure Type":
                        switch (pair.Value) {
                            case "Fixed Value":
                                strategyParams.Add("ExposureClass", typeof(FixedValueExposureManager));
                                IsFixedValueExposure = true;
                                break;
                            case "Fixed Share":
                                strategyParams.Add("ExposureClass", typeof(FixedShareExposureManager));
                                break;
                            default:
                                break;
                        }
                        break;
                    case "Exposure":
                        StrategyParams exposureParams = new StrategyParams();
                        if (IsFixedValueExposure) {
                            exposureParams.AddInput("ExposureFixedValue", double.Parse(pair.Value));
                            strategyParams.Add("ExposureFixedValue", double.Parse(pair.Value));
                        } else {
                            exposureParams.AddInput("ExposureFixedShare", double.Parse(pair.Value));
                            strategyParams.Add("ExposureFixedShare", double.Parse(pair.Value));
                        }
                        strategyParams.Add("ExposureParams", exposureParams);
                        break;
                    default:
                        break;
                }
            }
            return strategyParams;
        }

        private Dictionary<string, object> GenerateBollingerParams(Dictionary<string, string> rawParams) {
            Dictionary<string, object> strategyParams = new();

            bool IsFixedValueExposure = false;
            foreach (KeyValuePair<string, string> pair in rawParams) {
                switch (pair.Key) {
                    case "Ticker":
                        break;
                    case "Window Size":
                        strategyParams.Add("WindowSize", int.Parse(pair.Value));
                        break;
                    case "Width":
                        strategyParams.Add("Width", double.Parse(pair.Value));
                        break;
                    case "Exposure Type":
                        switch (pair.Value) {
                            case "Fixed Value":
                                strategyParams.Add("ExposureClass", typeof(FixedValueExposureManager));
                                IsFixedValueExposure = true;
                                break;
                            case "Fixed Share":
                                strategyParams.Add("ExposureClass", typeof(FixedShareExposureManager));
                                break;
                            default:
                                break;
                        }
                        break;
                    case "Exposure":
                        StrategyParams exposureParams = new StrategyParams();
                        if (IsFixedValueExposure) {
                            exposureParams.AddInput("ExposureFixedValue", double.Parse(pair.Value));
                            strategyParams.Add("ExposureFixedValue", double.Parse(pair.Value));
                        } else {
                            exposureParams.AddInput("ExposureFixedShare", double.Parse(pair.Value));
                            strategyParams.Add("ExposureFixedShare", double.Parse(pair.Value));
                        }
                        strategyParams.Add("ExposureParams", exposureParams);
                        break;
                    case "Exit Type":
                        switch (pair.Value) {
                            case "At Opposite":
                                strategyParams.Add("ExitLevelClass", typeof(BollingerExitAtOpposite));
                                break;
                            case "At Reference":
                                strategyParams.Add("ExitLevelClass", typeof(BollingerExitAtReference));
                                break;
                            case "With Trailing Stop":
                                strategyParams.Add("ExitLevelClass", typeof(BollingerExitWithTrailingStop));
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            return strategyParams;
        }

        private void GenerateMeanReversionPnl(Instrument instrument, List<TS> timeseries, BacktestManager backtestManager) {
            TS closePxTs = timeseries[1];

            Type strategyType = typeof(BollingerReversionStrategy);

            StrategyParams strategyParams = new StrategyParams();
            Dictionary<string, object> inputDict = GenerateBollingerParams(Parameters);
            strategyParams.AddInputs(inputDict);

            backtestManager.SetStrategy(strategyType, strategyParams, instrument);

            List<StrategyInput> strategyInputs = new List<StrategyInput>();
            for (int i = 0; i < closePxTs.Size(); i++) {
                StrategyInput strategyInput = new StrategyInput();
                strategyInput.AddInput("ClosePrice", closePxTs.GetValue(i));
                strategyInput.AddInput("Timestamp", closePxTs.GetTimestamp(i));
                strategyInputs.Add(strategyInput);
            }

            TS pnlTs = backtestManager.RunBacktest(strategyInputs);

            (TS, TS) bollingerBands = closePxTs.BollingerBands((int)inputDict["WindowSize"], (double)inputDict["Width"]);
            List<TS> indicatorTs = new List<TS>() { bollingerBands.Item1, bollingerBands.Item2 };

            AddToDataset(closePxTs, "Close Prices");
            AddToDataset(pnlTs, "P&L");
            AddToDataset(indicatorTs, "Bollinger Bands");
        }


        private void GenerateBollingerBreakoutPnl(Instrument instrument, List<TS> timeseries, BacktestManager backtestManager) {
            TS closePxTs = timeseries[1];

            Type strategyType = typeof(BollingerBreakoutStrategy);

            StrategyParams strategyParams = new StrategyParams();
            Dictionary<string, object> inputDict = GenerateBollingerParams(Parameters);
            strategyParams.AddInputs(inputDict);

            backtestManager.SetStrategy(strategyType, strategyParams, instrument);

            List<StrategyInput> strategyInputs = new List<StrategyInput>();
            for (int i = 0; i < closePxTs.Size(); i++) {
                StrategyInput strategyInput = new StrategyInput();
                strategyInput.AddInput("ClosePrice", closePxTs.GetValue(i));
                strategyInput.AddInput("Timestamp", closePxTs.GetTimestamp(i));
                strategyInputs.Add(strategyInput);
            }

            TS pnlTs = backtestManager.RunBacktest(strategyInputs);

            (TS, TS) bollingerBands = closePxTs.BollingerBands((int)inputDict["WindowSize"], (double)inputDict["Width"]);
            List<TS> indicatorTs = new List<TS>() { bollingerBands.Item1, bollingerBands.Item2 };

            AddToDataset(closePxTs, "Close Prices");
            AddToDataset(pnlTs, "P&L");
            AddToDataset(indicatorTs, "Bollinger Bands");
        }

        private void AddToDataset(TS timeseries, string label) {
            List<string> timestamps = timeseries.GetTimestamps().Select(ts => ts.ToShortDateString()).ToList();
            List<double> values = timeseries.GetValues();

            Dictionary<string, object> dataPoints = new() {
                { "timestamps", timestamps },
                { "values", values }
            };

            _dataset.Add(label, dataPoints);
        }

        private void AddToDataset(List<TS> timeseries, string label) {
            if (label == "Bollinger Bands") {
                AddToDataset(timeseries[0], "Bollinger Bands Upper Band");
                AddToDataset(timeseries[1], "Bollinger Bands Lower Band");
            } else if (label == "Moving Averages") {
                AddToDataset(timeseries[0], "EWMA Fast");
                AddToDataset(timeseries[1], "EWMA Slow");
            }
        }


        private async Task<Instrument> ReadInstrumentFromDatabase(DatabaseHandler databaseHandler, string ticker) {
            Instrument instrument = await databaseHandler.GetInstrumentDataAsync(ticker);
            return instrument;
        }
        private async Task<List<TS>> ReadPriceDataFromDatabase(DatabaseHandler databaseHandler, string ticker) {
            List<PriceData> priceData = await databaseHandler.GetPriceDataAsync(ticker);
            int initialCapacity = priceData.Count;

            TS openPxTs = new TS(initialCapacity);
            TS closePxTs = new TS(initialCapacity);
            TS highPxTs = new TS(initialCapacity);
            TS lowPxTs = new TS(initialCapacity);

            /*
             * Each price data object contains single prices related to a single timestamp
             */

            foreach (PriceData priceDataObj in priceData) {
                double openPx = priceDataObj.OpenPx;
                double closePx = priceDataObj.ClosePx;
                double highPx = priceDataObj.HighPx;
                double lowPx = priceDataObj.LowPx;
                DateTime pxDate = priceDataObj.PxDate;

                openPxTs.Add(pxDate, openPx);
                closePxTs.Add(pxDate, closePx);
                highPxTs.Add(pxDate, highPx);
                lowPxTs.Add(pxDate, lowPx);
            }

            return new List<TS> { openPxTs, closePxTs, highPxTs, lowPxTs };
        }
    }
}