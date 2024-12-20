namespace Frontend.Components.Controls {
    public partial class EntireContainerComponent {
        private string? selectedInstrument;
        private void HandleInstrumentSelection(string instrument) {
            Console.WriteLine($"Selected Instrument: {instrument}");
            selectedInstrument = instrument;
        }
    }
}
