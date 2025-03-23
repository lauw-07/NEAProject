namespace Frontend.Components.Controls {
    public partial class AppContainer {
        private string? selectedSecurity;

        private void HandleSecuritySelection(string security) {
            selectedSecurity = security;
        }
    }
}