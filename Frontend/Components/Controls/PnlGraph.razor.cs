using Frontend.Models.Backtest;
using Frontend.Models.Backtest.Breakout;
using Frontend.Models.Database;
using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.Metrics;
using Instrument = Frontend.Models.Database.Instrument;

namespace Frontend.Components.Controls {
    public partial class PnlGraph {
        [Parameter]
        public string? Strategy { get; set; }

        [Parameter]
        public Dictionary<string, string> Parameters { get; set; } = new();

        private TS _pnlTs = new();

        protected override async void OnParametersSet() {
            _pnlTs.Clear();
            if (Parameters != null && Parameters.Count > 0) {
                await Backtest(databaseHandler);
                await Js.InvokeVoidAsync("DrawGraph", _pnlTs);
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
                    _pnlTs = GenerateBollingerBreakoutPnl(instrument, timeseries, backtestManager);
                    break;
                default:
                    break;
            }
        }

        private Dictionary<string, object> GenerateBollingerBreakoutParams(Dictionary<string, string> rawParams) {
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
                            exposureParams.AddInput("ExposureFixedValue", pair.Value);
                            strategyParams.Add("ExposureFixedValue", pair.Value);
                        } else {
                            exposureParams.AddInput("ExposureFixedShare", pair.Value);
                            strategyParams.Add("ExposureFixedShare", pair.Value);
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

        private TS GenerateBollingerBreakoutPnl(Instrument instrument, List<TS> timeseries, BacktestManager backtestManager) {
            TS closePxTs = timeseries[1];

            Type strategyType = typeof(BollingerBreakoutStrategy);

            StrategyParams strategyParams = new StrategyParams();
            Dictionary<string, object> inputDict = GenerateBollingerBreakoutParams(Parameters);
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
            return pnlTs;
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
