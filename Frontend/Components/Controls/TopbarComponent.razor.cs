using Microsoft.AspNetCore.Components;
using Frontend.Models.Database;

namespace Frontend.Components.Controls {
    public partial class TopbarComponent {
        private List<string> _instruments = new List<string>() {
            "Indices", "FX", "Cryptocurrency", "Stocks", "Commodities", "Bonds and Rates"
        };

        private List<string> _availableMarkets = new List<string>();
        //Using strings here just temporarily to make it easy to test

        //private string? InstrumentParameter;
        [Parameter]
        public EventCallback<string> SelectInstrumentCallback { get; set; }

        [Parameter]
        public EventCallback<string> SelectMarketCallback { get; set; }

        private string? instrumentSelected;

        private void SelectInstrument(string instrument) {
            Console.WriteLine($"Selected Instrument: {instrument}");
            instrumentSelected = instrument;
            SelectInstrumentCallback.InvokeAsync(instrument);
        }

        private void SelectMarket(string market) {
            Console.WriteLine($"Selected Market: {market}");
            SelectMarketCallback.InvokeAsync(market);
        }

        private async Task GetAvailableMarkets(string instrumentType) {
            List<string> instrumentNames = new List<string>();
            
            List<Instrument> instruments = await databaseHandler.GetInstrumentDataAsync();
            foreach (Instrument instrument in instruments) {
                if (instrument.InstrumentType == instrumentSelected) {
                    instrumentNames.Add(instrument.InstrumentName);
                }
            }
        }
    }
}
