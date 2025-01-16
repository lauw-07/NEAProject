using Microsoft.AspNetCore.Components;
using Frontend.Models.Database;

namespace Frontend.Components.Controls {
    public partial class TopbarComponent {
        private List<string> _instruments = new List<string>() {
            "Indices", "FX", "Cryptocurrency", "Stocks", "Commodities", "Bonds and Rates"
        };

        private List<string> _availableItems = new List<string>();
        //Using strings here just temporarily to make it easy to test

        //private string? InstrumentParameter;
        [Parameter]
        public EventCallback<string> SelectInstrumentCallback { get; set; }

        [Parameter]
        public EventCallback<string> SelectItemCallback { get; set; }

        private string? instrumentSelected;

        private async Task SelectInstrument(string instrument) {
            Console.WriteLine($"Selected Instrument: {instrument}");
            instrumentSelected = instrument;
            await GetAvailableItems(instrument);
            await SelectInstrumentCallback.InvokeAsync(instrument);
        }

        private void SelectItem(string items) {
            Console.WriteLine($"Selected Item: {items}");
            SelectItemCallback.InvokeAsync(items);
        }

        private async Task GetAvailableItems(string instrumentType) {
            List<string> instrumentNames = new List<string>();
            
            List<Instrument> instruments = await databaseHandler.GetInstrumentDataAsync();
            foreach (Instrument instrument in instruments) {
                if (instrument.InstrumentType == instrumentSelected) {
                    instrumentNames.Add(instrument.InstrumentName);
                }
            }

            _availableItems = instrumentNames;
        }
    }
}
