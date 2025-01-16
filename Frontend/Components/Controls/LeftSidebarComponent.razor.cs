using Frontend.Models.Database;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace Frontend.Components.Controls {
    public partial class LeftSidebarComponent {
        /*private List<TradingIndicator> tradingIndicators = new() {  }; 
         * I will just use strings for now 
         * I can also probably just hard code in the trading indicators since I am going to be making my own versions of them
         */

        private List<string> _popularTradingIndicators = new List<string>() { 
            "Exponentially Weighted Moving Average", "Bollinger Bands", "Simple Moving Average", "RSI" 
        };

        private List<string> _extraTradingIndicators = new List<string>() {
            "Linear Regression", "Standard Deviation", "Momentum", "Aroon"
        };

        //private string? IndicatorParameter;

        [Parameter]
        public EventCallback<string> SelectIndicatorCallback { get; set; }

        private void SelectIndicator(string indicator) {
            SelectIndicatorCallback.InvokeAsync(indicator);
        }
    }
}
