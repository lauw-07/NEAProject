using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class MainContainer {
        [Parameter]
        public string? SelectedSecurity { get; set; }

        private GraphContainer? graphContainerRef;

        private void HandleIndicatorSelection(string indicator) {
            graphContainerRef?.ToggleIndicator(indicator);
        }
    }
}