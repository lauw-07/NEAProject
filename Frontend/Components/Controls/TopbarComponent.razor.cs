using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class TopbarComponent {
        private List<string> _instruments = new List<string>() {
            "Indices", "FX", "Cryptocurrency", "Stocks", "Commodities", "Bonds and Rates"
        };
        //Using strings here just temporarily to make it easy to test

        //private string? InstrumentParameter;
        [Parameter]
        public EventCallback<string> SelectInstrumentCallback { get; set; }

        private void SelectInstrument(string instrument) {
            Console.WriteLine($"Selected Instrument: {instrument}");
            SelectInstrumentCallback.InvokeAsync(instrument);
        }
    }
}
