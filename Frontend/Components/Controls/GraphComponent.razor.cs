using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class GraphComponent {
        [Parameter]
        public List<TS>? Timeseries { get; set; }
        private readonly List<Dictionary<string, object>> Dataset = new();

        protected override void OnParametersSet() {
            if (Timeseries != null && Timeseries.Count > 0) {
                foreach (TS timeseries in Timeseries) {
                    List<string> timestamps = timeseries.GetTimestamps().Select(ts => ts.ToShortDateString()).ToList();
                    List<double> values = timeseries.GetValues();

                    Dataset.Add(new Dictionary<string, object> {
                        { "timestamps", timestamps },
                        { "values", values }
                    });
                }
            } else {
                Console.WriteLine("Timeseries data is null");
            }
        }

        // Pass timeseries into the graphing.js file

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            Console.WriteLine("OnAfterRenderAsync function in Graph Component has been triggered");
            if (firstRender) {
                Console.WriteLine("Attempting First Render");
                if (Timeseries != null && Timeseries.Count != 0) {
                    /*
                     * Maybe on initialisation, i could load display some data from the local database
                     */
                    Console.WriteLine("Timeseries data received, invoking draw graph function");
                    await Js.InvokeVoidAsync("DrawGraph", Dataset);
                    Console.WriteLine("Graph drawn");
                }
            } else {
                Console.WriteLine("Not first render");
                if (Timeseries != null && Timeseries.Count != 0) {

                    Console.WriteLine("Attempting to draw graph");
                    await Js.InvokeVoidAsync("DrawGraph", Dataset);
                }
            }
        }
    }
}
