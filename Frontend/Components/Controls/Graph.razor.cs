using Frontend.Models.Timeseries;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Controls {
    public partial class Graph {
        [Parameter]
        public TS? Timeseries { get; set; }
        [Parameter]
        public List<TS>? IndicatorTS { get; set; }
        private readonly Dictionary<string, Dictionary<string, object>> _dataset = new();

        //Need to loop through the indicator timeseries aswell and pass it on inside the _dataset object
        protected override void OnParametersSet() {
            _dataset.Clear();
            if (Timeseries != null && Timeseries.Size() > 0) {
                AddToDataset(Timeseries);
            } else {
                Console.WriteLine("Timeseries data is null ");
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

            _dataset.Add((ts.GetIndicator() == "") ? "Close Prices" : ts.GetIndicator(), dataPoints);
        }

        private void AddToDataset(List<TS> timeseries) {
            foreach (TS ts in timeseries) {
                AddToDataset(ts);
            }
        }

        
        protected override async Task OnAfterRenderAsync(bool firstRender) {
            Console.WriteLine("OnAfterRenderAsync function in Graph Component has been triggered");

            if (Timeseries != null && Timeseries.Size() != 0) {
                Console.WriteLine("Attempting to draw graph");
                // Pass timeseries as _dataset into the graphing.js file
                await Js.InvokeVoidAsync("DrawGraph", _dataset, "graph");
                Console.WriteLine("Graph drawn");
            }
        }
    }
}
