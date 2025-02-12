using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class Graph {
        [Parameter]
        public TS? Timeseries { get; set; }

        [Parameter]
        public List<TS>? IndicatorTS { get; set; }

        //private readonly List<Dictionary<string, object>> Dataset = new();
        private readonly Dictionary<string, Dictionary<string, object>> Dataset = new();

        //Need to loop through the indicator timeseries aswell and pass it on inside the Dataset object
        protected override void OnParametersSet() {
            Dataset.Clear();
            if (Timeseries != null && Timeseries.Size() > 0) {
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

        private void AddToDataset(TS ts) {
            List<string> timestamps = ts.GetTimestamps().Select(ts => ts.ToShortDateString()).ToList();
            List<double> values = ts.GetValues();

            Dictionary<string, object> dataPoints = new() {
                { "timestamps", timestamps },
                { "values", values }
            };

            Dataset.Add((ts.GetIndicator() == "") ? "Close Prices" : ts.GetIndicator(), dataPoints);
        }

        private void AddToDataset(List<TS> timeseries) {
            foreach (TS ts in timeseries) {
                AddToDataset(ts);
            }
        }

        // Pass timeseries into the graphing.js file

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            Console.WriteLine("OnAfterRenderAsync function in Graph Component has been triggered");

            if (Timeseries != null && Timeseries.Size() != 0) {
                Console.WriteLine("Attempting to draw graph");
                //await Js.InvokeVoidAsync("CreateGraph", Dataset, "graph");
                await Js.InvokeVoidAsync("DrawGraph", Dataset, "graph");
                

                //IJSObjectReference module = await Js.InvokeAsync<IJSObjectReference>("import", "/js/drawGraph.js");
                //await module.InvokeVoidAsync("DrawGraph", Dataset, "graph");

                Console.WriteLine("Graph drawn");
            }
        }
    }
}
