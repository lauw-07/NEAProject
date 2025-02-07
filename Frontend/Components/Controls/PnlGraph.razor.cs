using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Controls {
    public partial class PnlGraph {
        [Parameter]
        public string? Strategy { get; set; }

        [Parameter]
        public List<string>? Parameters { get; set; }

        private readonly List<Dictionary<string, object>> Dataset = new();


    }
}
