﻿using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class PnlGraphContainer {
        [Parameter]
        public string? Strategy { get; set; }

        private List<string> _strategyParameters = new();

        private Dictionary<string, string> _selectedParams = new();
        private bool _hasParamsBeenSelected = false;

        protected override void OnParametersSet() {
            if (Strategy != null) {
                _strategyParameters = GetBacktestParams(Strategy);

                foreach (string param in _strategyParameters) {
                    _selectedParams[param] = "";
                }
            }
        }

        private void SelectParameter(string param, string value) {
            if (value != null) {
                _selectedParams[param] = value;
            }
        }

        private List<string> GetStrategyParameters(string param) {
            // This is just for testing purposes, realistically i will not be just using the bollinger breakout strategy so these params will be different
            // These _values are just for testing purposes
            switch (param) {
                case "Ticker":
                    return new List<string>() { "AAPL", "MSFT", "NVDA", "AMZN", "TSLA" };
                case "Window Size":
                    return new List<string>() { "10", "20", "30", "40", "50" };
                case "Slow Half Life":
                    return new List<string>() { "10", "20", "30", "40", "50" };
                case "Fast Half Life":
                    return new List<string>() { "5", "10", "15", "20", "25" };
                case "Width":
                    return new List<string>() { "1.5", "2", "2.5" };
                case "Exposure Type":
                    return new List<string>() { "Fixed Value", "Fixed Share" };
                case "Exposure":
                    return new List<string>() { "20", "50", "80", "200", "1000", "2000" }; 
                case "Exit Type":
                    return new List<string>() { "At Opposite", "At Reference", "With Trailing Stop" };
                default:
                    return new List<string>();
            }
        }

        private List<string> GetBacktestParams(string strategy) {
            switch (Strategy) {
                case "Bollinger Bands Breakout":
                    return new List<string>() { "Ticker", "Window Size", "Width", "Exposure Type", "Exposure", "Exit Type" };
                case "EWMA Crossover":
                    return new List<string>() { "Ticker", "Slow Half Life", "Fast Half Life", "Exposure Type", "Exposure" };
                case "Mean Reversion":
                    return new List<string>() { "Ticker", "Window Size", "Width", "Exposure Type", "Exposure", "Exit Type" };
                default:
                    return new List<string>();
            }
        }

        private void GeneratePnlGraph() {
            foreach (KeyValuePair<string, string> pair in _selectedParams) {
                if (pair.Value == "") {
                    return;
                }
            }

            _hasParamsBeenSelected = true;
        }
    }
}
