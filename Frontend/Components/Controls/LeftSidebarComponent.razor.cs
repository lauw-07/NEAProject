using Frontend.Models.Database;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace Frontend.Components.Controls {
    public partial class LeftSidebarComponent {
        /*private List<TradingStrategy> tradingStrategies = new() {  }; 
         * I will just use strings for now 
         * I can also probably just hard code in the trading strategies since I am going to be making my own versions of them
         */

        private List<string> _popularTradingStrategies = new List<string>() { 
            "Exponentially Weighted Moving Average", "Bollinger Bands", "RSI" 
        };

        private List<string> _extraTradingStrategies = new List<string>() {
            "Linear Regression", "Standard Deviation", "Momentum", "Aroon"
        };

        //private string? StrategyParameter;

        [Parameter]
        public EventCallback<string> SelectStrategyCallback { get; set; }

        private void SelectStrategy(string strategy) {
            SelectStrategyCallback.InvokeAsync(strategy);
        }
    }
}
