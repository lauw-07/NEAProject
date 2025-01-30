using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class Graph {
        [Parameter]
        public List<TS>? Timeseries { get; set; }

        [Parameter]
        public List<TS>? IndicatorTS { get; set; }

        private readonly List<Dictionary<string, object>> Dataset = new();

        //Need to loop through the indicator timeseries aswell and pass it on inside the Dataset object
        protected override void OnParametersSet() {
            Dataset.Clear();
            if (Timeseries != null && Timeseries.Count > 0) {
                AddToDataset(Timeseries);
            } else {
                Console.WriteLine("Timeseries data is null");
            }

            if (IndicatorTS != null && IndicatorTS.Count > 0) {
                AddToDataset(IndicatorTS);
            } else {
                Console.WriteLine("Indicator timeseries data is null");
            }
        }

        private void AddToDataset(List<TS> timeseries) {
            foreach (TS ts in timeseries) {
                List<string> timestamps = ts.GetTimestamps().Select(ts => ts.ToShortDateString()).ToList();
                List<double> values = ts.GetValues();

                Dataset.Add(new Dictionary<string, object> {
                        { "timestamps", timestamps },
                        { "values", values }
                });
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
