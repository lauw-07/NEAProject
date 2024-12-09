using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Frontend.Models.PolygonData {
    public class Result {
        [JsonPropertyName("v")]
        public double Volume { get; set; }

        [JsonPropertyName("vw")]
        public double VolumeWeightedPrice { get; set; }

        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp { get; set; }

        [JsonPropertyName("n")]
        public int Transactions { get; set; }

        public Result() {
            Volume = default(int);
            VolumeWeightedPrice = default;
            Open = default;
            Close = default;
            High = default;
            Low = default;
            Timestamp = default;
            Transactions = default;
        }
    }
}
