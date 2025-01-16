using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class MainContainerComponent {

        [Parameter]
        public string? SelectedInstrument {  get; set; }

        [Parameter]
        public string? SelectedItem { get; set; }

        private string? selectedIndicator;

        private void HandleIndicatorSelection(string indicator) {
            selectedIndicator = indicator;
        }

    }
}
