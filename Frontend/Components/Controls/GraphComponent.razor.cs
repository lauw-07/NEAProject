using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class GraphComponent {
        [Parameter]
        public List<TS>? Timeseries { get; set; }
        private TS? OpenPxTS;
        private TS? ClosePxTS;

        protected override void OnParametersSet() {
            if (Timeseries != null && Timeseries.Count > 0) {
                OpenPxTS = Timeseries[0];
                ClosePxTS = Timeseries[1];
            } else {
                Console.WriteLine("Timeseries data is null");
            }
        }

        // Pass timeseries into the graphing.js file

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            Console.WriteLine("OnAfterRenderAsync function in Graph Component has been triggered");
            if (firstRender) {
                /*Format Timeseries data
                 * var formattedTS = ...;
                 *
                 */
                Console.WriteLine("Attempting First Render");
                if (Timeseries != null && Timeseries.Count != 0) {
                    var temp = Timeseries;
                    Console.WriteLine(temp);
                    
                    /*
                     * Maybe on initialisation, i could load display some data from the local database
                     */
                    Console.WriteLine("Timeseries data received, invoking draw graph function");
                    await Js.InvokeVoidAsync("DrawGraph", temp);
                    Console.WriteLine("Graph drawn");
                }
            } else {
                Console.WriteLine("Not first render");
                if (Timeseries != null && Timeseries.Count != 0) {
                    var temp = Timeseries;

                    Console.WriteLine("Attempting to draw graph");
                    await Js.InvokeVoidAsync("DrawGraph", temp);
                }
            }
        }
    }
}
