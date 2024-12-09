using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Frontend.Models.PolygonData {
    public class PolygonStockPriceData {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }

        [JsonPropertyName("queryCount")]
        public int QueryCount { get; set; }

        [JsonPropertyName("resultsCount")]
        public int ResultsCount { get; set; }

        [JsonPropertyName("adjusted")]
        public bool Adjusted { get; set; }

        [JsonPropertyName("results")]
        public List<Result> Results { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        public PolygonStockPriceData() {
            Ticker = string.Empty;
            QueryCount = default;
            ResultsCount = default;
            Adjusted = default;
            Results = new List<Result>();
            Status = string.Empty;
            RequestId = string.Empty;
            Count = default;
        }
    }
}