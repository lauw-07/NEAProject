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
        public EventCallback<string> SelectInstrumentCallback { get; set; }

        [Parameter]
        public EventCallback<string> SelectSecurityCallback { get; set; }

        private string? instrumentSelected;

        private async Task SelectInstrument(string instrument) {
            Console.WriteLine($"Selected Instrument: {instrument}");
            instrumentSelected = instrument;
            await GetAvailableSecurities(instrument);
            await SelectInstrumentCallback.InvokeAsync(instrument);
        }

        private void SelectSecurity(string security) {
            Console.WriteLine($"Selected Security: {security}");
            SelectSecurityCallback.InvokeAsync(security);
        }

        private async Task GetAvailableSecurities(string instrumentType) {
            List<string> instrumentNames = new List<string>();

            List<Instrument> instruments = await databaseHandler.GetInstrumentDataByTypeAsync(instrumentType);
            foreach (Instrument instrument in instruments) {
                instrumentNames.Add(instrument.InstrumentName);
            }

            _availableSecurities = instrumentNames;
        }
    }
}
