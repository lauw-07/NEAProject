using Frontend.Models.Timeseries;

namespace Frontend.Models.Indicators {
    public class Ewma {
        public double Alpha { get; set; }

        private TS ewmaTS = new();

        public Ewma(double alpha) {
            Alpha = alpha;
        }

       
        public TS GetEwmaTS() {
            return ewmaTS;
        }
    }
}
