using Frontend.Models.Timeseries;

namespace Frontend.Models.Indicators {
    public class Ewvol : Ewma {
        private bool useMean;
        protected double vv = double.NaN; // exponential weighted variance


        public Ewvol(double halfLife, double seedVol, double seedMa) : base(halfLife, seedMa) {
            useMean = !double.IsNaN(seedMa);
            vv = Math.Pow(seedVol, 2);
            _name = "Exponential Weighted Volatility";
        }

        public override void Update(double dt, double value) {
            decay = Math.Pow(unitDecay, dt);
            if (useMean) {
                currentMa = decay * currentMa + (1 - decay) * value;
                vv = decay * vv + (1 - decay) * Math.Pow(value - currentMa, 2);
            } else {
                vv = decay * vv + (1 - decay) * Math.Pow(value, 2);
            }
        }

        public override void Update(TS values) {
            if (values == null || values.Size() == 0) return;

            DateTime prevTime;
            DateTime currentTime = values.GetTimestamp(0);
            for (int t = 1; t < values.Size(); t++) {
                prevTime = currentTime;
                currentTime = values.GetTimestamp(t);
                Update((currentTime - prevTime).Days, values.GetValue(t));
            }
        }

        public override double GetUpdate(double dt, double value) {
            Update(dt, value);
            return GetVol();
        }

        public override double GetUpdate(TS values) {
            Update(values);
            return GetVol();
        }

        public double GetVol() {
            return Math.Sqrt(vv);
        }
    }
}
