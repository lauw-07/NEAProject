namespace Frontend.Components.Controls {
    public partial class EntireContainerComponent {
        private string? selectedInstrument;
        private string? selectedMarket;
        private void HandleInstrumentSelection(string instrument) {
            selectedInstrument = instrument;
        }

        private void HandleMarketSelection(string market) {
            selectedMarket = market;
        }
    }
}
