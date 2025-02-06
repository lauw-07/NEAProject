using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class PnlGraphContainer {
        [Parameter]
        public string? Strategy { get; set; }

        private List<string> _strategyParameters = new();

        private Dictionary<string, string> _selectedParams = new();

        protected override void OnInitialized() {
            foreach (string param in _strategyParameters) {
                _selectedParams[param] = "";
            }
        }

        protected override void OnParametersSet() {
            if (Strategy != null) {
                _strategyParameters = GetBacktestParams(Strategy);
            }
        }

        private void SelectParameter(string param, string value) {
            if (value != null) {
                _selectedParams[param] = value;
            }
        }

        private List<string> GetStrategyParameters(string param) {
            switch (param) {
                case "Window Size":
                    return new List<string>() { "10", "20", "30", "40", "50" };
                case "Width":
                    return new List<string>() { "1.5", "2", "2.5" };
                case "Exposure Type":
                    return new List<string>() { "Fixed Value", "Fixed Share" };
                case "Exposure":
                    return new List<string>() { "20", "50", "80", "200" }; // these values are just for testing (i think i will enable the user to type their input for this parameter
                case "Exit Type":
                    return new List<string>() { "At Opposite", "At Reference" };
                default:
                    return new List<string>();
            }
        }

        private List<string> GetBacktestParams(string strategy) {
            switch (Strategy) {
                case "Bollinger Bands Breakout":
                    return new List<string>() { "Window Size", "Width", "Exposure Type", "Exposure", "Exit Type"};
                default:
                    return new List<string>();
            }
        }
    }
}
