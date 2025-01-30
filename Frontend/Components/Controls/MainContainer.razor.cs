using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class MainContainer {

        [Parameter]
        public string? SelectedInstrument { get; set; }

        [Parameter]
        public string? SelectedSecurity { get; set; }

        private string? selectedIndicator;

        private void HandleIndicatorSelection(string indicator) {
            selectedIndicator = indicator;
        }

    }
}
