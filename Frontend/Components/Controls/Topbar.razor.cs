using Microsoft.AspNetCore.Components;
using Frontend.Models.Database;

namespace Frontend.Components.Controls {
    public partial class Topbar {
        private List<string> _instruments = new List<string>() {
            "Indices", "FX", "Cryptocurrency", "Stock", "Commodities", "Bonds and Rates"
        };

        private List<string> _availableSecurities = new List<string>();
        //Using strings here just temporarily to make it easy to test

        [Parameter]
        public EventCallback<string> SelectSecurityCallback { get; set; }
        private List<List<string>> _summaryData = new();

        private void SelectSecurity(string security) {
            Console.WriteLine($"Selected Security: {security}");
            SelectSecurityCallback.InvokeAsync(security);
        }

        private async Task GetAvailableSecurities(string instrumentType) {
            _availableSecurities = await databaseHandler.GetInstrumentNamesFromTypeAsync(instrumentType);
        }

        private async Task GetSummaryData() {
            _summaryData = await databaseHandler.GetPriceDataSummary();
        }
    }
}
