using Frontend.Models.Database;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace Frontend.Components.Controls {
    public partial class LeftSidebar {
        /*
         * I will just use strings for now 
         * I can also probably just hard code in the trading indicators since I am going to be making my own versions of them
         */

        private List<string> _popularTradingIndicators = new List<string>() {
            "Exponentially Weighted Moving Average", "Bollinger Bands", "Simple Moving Average", "Exponential Weighted Volatility"
        };

        private List<string> _extraTradingIndicators = new List<string>() {
            "Linear Regression", "Standard Deviation", "RSI", "Aroon"
        };

        [Parameter]
        public EventCallback<string> SelectIndicatorCallback { get; set; }

        private void SelectIndicator(string indicator) {
            SelectIndicatorCallback.InvokeAsync(indicator);
        }
    }
}
