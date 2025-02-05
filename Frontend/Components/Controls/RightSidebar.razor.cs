using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class RightSidebar {
        private List<string> _mostUsedStrats = new List<string>() {
            "EWMA Crossover", "Bollinger Bands Breakout"
        };

        private List<string> _extraStrats = new List<string>() {
            "Bollinger Bands Reversion"
        };

        private void SelectStrategy(string strategy) {
            navigationManager.NavigateTo($"/PnlGraph/{strategy}");
        }
    }
}
