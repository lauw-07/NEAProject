using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class GraphComponent {
        [Parameter]
        public List<TS>? TSLists { get; set; }

        // Pass timeseries into the graphing.js file

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender) {
                /*Format Timeseries data
                 * var formattedTS = ...;
                 *
                 */
                Console.WriteLine("Attempting to initialise");
                if (TSLists != null && TSLists.Count != 0) {
                    var temp = TSLists;

                    /*
                     * Maybe on initialisation, i could load display some data from the local database
                     */
                    Console.WriteLine("Timeseries data received, invoking draw graph function");
                    await Js.InvokeVoidAsync("DrawGraph", temp);
                    Console.WriteLine("Graph drawn");
                }
            } else {
                if (TSLists != null && TSLists.Count != 0) {
                    var temp = TSLists;

                    await Js.InvokeVoidAsync("DrawGraph", temp);
                }
            }
        }
    }
}
