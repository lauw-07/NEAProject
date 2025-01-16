namespace Frontend.Components.Controls {
    public partial class EntireContainerComponent {
        private string? selectedInstrument;
        private string? selectedItem;
        private void HandleInstrumentSelection(string instrument) {
            selectedInstrument = instrument;
        }

        private void HandleItemSelection(string item) {
            selectedItem = item;
        }
    }
}
