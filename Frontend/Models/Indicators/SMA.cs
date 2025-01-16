using Frontend.Models.Timeseries;

namespace Frontend.Models.Indicators {
    public class SMA {
        public TS OriginalTS { get; set; }
        public int N { get; set; }

        private TS smaTS = new();

        public SMA(TS originalTS, int n) {
            OriginalTS = originalTS;
            N = n;
        }

        private double CalculateSMAAverage(List<double> prices) {
            double sum = prices.Sum();
            return sum / N;
        }

        private void ApplySMA() {
            if (OriginalTS == null || OriginalTS.Size() == 0) {
                throw new InvalidOperationException("Empty TS");
            }
            if (N <= 0) {
                throw new ArgumentException("Window size must be > 0");
            }
            if (N > OriginalTS.Size()) {
                throw new ArgumentException("Window size must be < TS size");
            }

            List<double> values = OriginalTS.GetValues();
            List<DateTime> timestamps = OriginalTS.GetTimestamps();
            for (int i = 0; i <= OriginalTS.Size() - N; i++) {
                List<double> window = values.GetRange(i, N);

                double sma = CalculateSMAAverage(window);
                smaTS.Add(timestamps[i + N - 1], sma);
            }
        }

        public TS GetSmaTS() {
            ApplySMA();
            return smaTS;
        }
    }
}
