using Frontend.Models.PolygonData;
using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.MiniComponents {
    public partial class Graph {
        [Parameter]
        public List<TS>? TSLists { get; set; }

        // Pass timeseries into the graphing.js file
    }
}
