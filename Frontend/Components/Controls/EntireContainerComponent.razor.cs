namespace Frontend.Components.Controls {
    public partial class EntireContainerComponent {
        private string? selectedInstrument;
        private string? selectedSecurity;
        private void HandleInstrumentSelection(string instrument) {
            selectedInstrument = instrument;
        }

        private void HandleSecuritySelection(string security) {
            selectedSecurity = security;
        }
    }
}
